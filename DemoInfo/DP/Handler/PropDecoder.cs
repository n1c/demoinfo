using System;
using System.Text;
using DemoInfo.DT;

namespace DemoInfo.DP.Handler
{
    internal static class PropDecoder
    {
        public static object DecodeProp(FlattenedPropEntry prop, IBitStream stream)
        {
            SendTableProperty sendProp = prop.Prop;
            switch (sendProp.Type)
            {
                case SendPropertyType.Int:
                    return DecodeInt(sendProp, stream);
                case SendPropertyType.Int64:
                    return DecodeInt64(sendProp, stream);
                case SendPropertyType.Float:
                    return DecodeFloat(sendProp, stream);
                case SendPropertyType.Vector:
                    return DecodeVector(sendProp, stream);
                case SendPropertyType.Array:
                    return DecodeArray(prop, stream);
                case SendPropertyType.String:
                    return DecodeString(sendProp, stream);
                case SendPropertyType.VectorXY:
                    return DecodeVectorXY(sendProp, stream);
                default:
                    throw new NotImplementedException("Could not read property. Abort! ABORT!");
            }
        }

        public static int DecodeInt(SendTableProperty prop, IBitStream reader)
        {
            if (prop.Flags.HasFlagFast(SendPropertyFlags.VarInt))
            {
                return prop.Flags.HasFlagFast(SendPropertyFlags.Unsigned) ? (int)reader.ReadVarInt() : (int)reader.ReadSignedVarInt();
            }
            else
            {
                return prop.Flags.HasFlagFast(SendPropertyFlags.Unsigned)
                    ? (int)reader.ReadInt(prop.NumberOfBits)
                    : reader.ReadSignedInt(prop.NumberOfBits);
            }
        }

        public static long DecodeInt64(SendTableProperty prop, IBitStream reader)
        {
            if (prop.Flags.HasFlagFast(SendPropertyFlags.VarInt))
            {
                return prop.Flags.HasFlagFast(SendPropertyFlags.Unsigned) ? reader.ReadVarInt() : reader.ReadSignedVarInt();
            }
            else
            {
                bool isNegative = false;
                uint low;
                uint high;

                if (prop.Flags.HasFlag(SendPropertyFlags.Unsigned))
                {
                    low = reader.ReadInt(32);
                    high = reader.ReadInt(prop.NumberOfBits - 32);
                }
                else
                {
                    isNegative = reader.ReadBit();
                    low = reader.ReadInt(32);
                    high = reader.ReadInt(prop.NumberOfBits - 32 - 1);
                }

                long result = ((long)high << 32) | low;

                if (isNegative)
                {
                    result = -result;
                }

                return result;
            }
        }

        public static float DecodeFloat(SendTableProperty prop, IBitStream reader)
        {
            if (DecodeSpecialFloat(prop, reader, out float fVal))
            {
                return fVal;
            }

            // Encoding: The range between lowVal and highVal is splitted into the same steps.
            // Read an int, fit it into the range.
            ulong dwInterp = reader.ReadInt(prop.NumberOfBits);
            fVal = (float)dwInterp / ((1 << prop.NumberOfBits) - 1);
            fVal = prop.LowValue + ((prop.HighValue - prop.LowValue) * fVal);

            return fVal;
        }

        public static Vector DecodeVector(SendTableProperty prop, IBitStream reader)
        {
            /* ?
            if (prop.Flags.HasFlagFast(SendPropertyFlags.Normal))
            {

            }
            */

            Vector v = new Vector
            {
                X = DecodeFloat(prop, reader),
                Y = DecodeFloat(prop, reader)
            };

            if (!prop.Flags.HasFlagFast(SendPropertyFlags.Normal))
            {
                v.Z = DecodeFloat(prop, reader);
            }
            else
            {
                bool isNegative = reader.ReadBit();

                //v0v0v1v1 in original instead of margin.
                float absolute = (v.X * v.X) + (v.Y * v.Y);
                v.Z = absolute < 1.0f ? (float)Math.Sqrt(1 - absolute) : 0f;

                if (isNegative)
                {
                    v.Z *= -1;
                }
            }

            return v;
        }

        public static object[] DecodeArray(FlattenedPropEntry flattenedProp, IBitStream reader)
        {
            int numElements = flattenedProp.Prop.NumberOfElements;
            int maxElements = numElements;
            int numBits = 1;

            while ((maxElements >>= 1) != 0)
            {
                numBits++;
            }

            int nElements = (int)reader.ReadInt(numBits);

            object[] result = new object[nElements];

            FlattenedPropEntry temp = new FlattenedPropEntry("", flattenedProp.ArrayElementProp, null);
            for (int i = 0; i < nElements; i++)
            {
                result[i] = DecodeProp(temp, reader);
            }

            return result;
        }

        public static string DecodeString(SendTableProperty _, IBitStream reader)
        {
            return Encoding.Default.GetString(reader.ReadBytes((int)reader.ReadInt(9)));
        }

        public static Vector DecodeVectorXY(SendTableProperty prop, IBitStream reader)
        {
            return new Vector
            {
                X = DecodeFloat(prop, reader),
                Y = DecodeFloat(prop, reader)
            };
        }

        private static bool DecodeSpecialFloat(SendTableProperty prop, IBitStream reader, out float result)
        {
            if (prop.Flags.HasFlagFast(SendPropertyFlags.Coord))
            {
                result = ReadBitCoord(reader);
                return true;
            }
            else if (prop.Flags.HasFlagFast(SendPropertyFlags.CoordMp))
            {
                result = ReadBitCoordMP(reader, false, false);
                return true;
            }
            else if (prop.Flags.HasFlagFast(SendPropertyFlags.CoordMpLowPrecision))
            {
                result = ReadBitCoordMP(reader, false, true);
                return true;
            }
            else if (prop.Flags.HasFlagFast(SendPropertyFlags.CoordMpIntegral))
            {
                result = ReadBitCoordMP(reader, true, false);
                return true;
            }
            else if (prop.Flags.HasFlagFast(SendPropertyFlags.NoScale))
            {
                result = reader.ReadFloat();
                return true;
            }
            else if (prop.Flags.HasFlagFast(SendPropertyFlags.Normal))
            {
                result = ReadBitNormal(reader);
                return true;
            }
            else if (prop.Flags.HasFlagFast(SendPropertyFlags.CellCoord))
            {
                result = ReadBitCellCoord(reader, prop.NumberOfBits, false, false);
                return true;
            }
            else if (prop.Flags.HasFlagFast(SendPropertyFlags.CellCoordLowPrecision))
            {
                result = ReadBitCellCoord(reader, prop.NumberOfBits, true, false);
                return true;
            }
            else if (prop.Flags.HasFlagFast(SendPropertyFlags.CellCoordIntegral))
            {
                result = ReadBitCellCoord(reader, prop.NumberOfBits, false, true);
                return true;
            }

            result = 0;
            return false;
        }

        private static readonly int COORD_FRACTIONAL_BITS = 5;
        private static readonly int COORD_DENOMINATOR = 1 << (COORD_FRACTIONAL_BITS);
        private static readonly float COORD_RESOLUTION = 1.0f / COORD_DENOMINATOR;
        private static readonly int COORD_FRACTIONAL_BITS_MP_LOWPRECISION = 3;
        private static readonly float COORD_DENOMINATOR_LOWPRECISION = 1 << (COORD_FRACTIONAL_BITS_MP_LOWPRECISION);
        private static readonly float COORD_RESOLUTION_LOWPRECISION = 1.0f / COORD_DENOMINATOR_LOWPRECISION;

        private static float ReadBitCoord(IBitStream reader)
        {
            float value = 0;
            bool isNegative = false;

            // Read the required integer and fraction flags
            int intVal = (int)reader.ReadInt(1);
            int fractVal = (int)reader.ReadInt(1);

            // If we got either parse them, otherwise it's a zero.
            if ((intVal | fractVal) != 0)
            {
                // Read the sign bit
                isNegative = reader.ReadBit();

                if (intVal == 1)
                {
                    // Adjust the integers from [0..MAX_COORD_VALUE-1] to [1..MAX_COORD_VALUE]
                    intVal = (int)reader.ReadInt(14) + 1; //14 --> Coord int bits
                }

                if (fractVal == 1)
                {
                    fractVal = (int)reader.ReadInt(COORD_FRACTIONAL_BITS);
                }

                value = intVal + (fractVal * COORD_RESOLUTION);
            }

            if (isNegative)
            {
                value *= -1;
            }

            return value;
        }

        private static float ReadBitCoordMP(IBitStream reader, bool isIntegral, bool isLowPrecision)
        {
            int intval;
            float value = 0.0f;
            bool isNegative = false;
            bool inBounds = reader.ReadBit();

            if (isIntegral)
            {
                // Read the required integer and fraction flags
                intval = reader.ReadBit() ? 1 : 0;

                // If we got either parse them, otherwise it's a zero.
                if (intval == 1)
                {
                    // Read the sign bit
                    isNegative = reader.ReadBit();

                    // If there's an integer, read it in
                    // Adjust the integers from [0..MAX_COORD_VALUE-1] to [1..MAX_COORD_VALUE]
                    value = reader.ReadInt(inBounds ? 11 : 14) + 1;
                }
            }
            else
            {
                // Read the required integer and fraction flags
                intval = reader.ReadBit() ? 1 : 0;

                // Read the sign bit
                isNegative = reader.ReadBit();

                // If we got either parse them, otherwise it's a zero.
                if (intval == 1)
                {
                    // If there's an integer, read it in
                    // Adjust the integers from [0..MAX_COORD_VALUE-1] to [1..MAX_COORD_VALUE]
                    _ = (float)(reader.ReadInt(inBounds ? 11 : 14) + 1);

                    /* @TODO: Remove when we confirm the above simplificaion is correct
                    if (inBounds)
                    {
                        value = (float)(reader.ReadInt(11) + 1);
                    }
                    else
                    {
                        value = (float)(reader.ReadInt(14) + 1);
                    }
                    */
                }

                // If there's a fraction, read it in
                int fractval = (int)reader.ReadInt(isLowPrecision ? 3 : 5);

                // Calculate the correct floating point value
                value = intval + (fractval * (isLowPrecision ? COORD_RESOLUTION_LOWPRECISION : COORD_RESOLUTION));
            }

            if (isNegative)
            {
                value = -value;
            }

            return value;
        }

        private static float ReadBitCellCoord(IBitStream reader, int bits, bool lowPrecision, bool integral)
        {
            float value;
            if (integral)
            {
                value = reader.ReadInt(bits);
            }
            else
            {
                int intval = (int)reader.ReadInt(bits);
                int fractval = (int)reader.ReadInt(lowPrecision ? COORD_FRACTIONAL_BITS_MP_LOWPRECISION : COORD_FRACTIONAL_BITS);
                value = intval + (fractval * (lowPrecision ? COORD_RESOLUTION_LOWPRECISION : COORD_RESOLUTION));
            }

            return value;
        }

        private static readonly int NORMAL_FRACTIONAL_BITS = 11;
        private static readonly int NORMAL_DENOMINATOR = (1 << (NORMAL_FRACTIONAL_BITS)) - 1;
        private static readonly float NORMAL_RESOLUTION = 1.0f / NORMAL_DENOMINATOR;

        private static float ReadBitNormal(IBitStream reader)
        {
            bool isNegative = reader.ReadBit();
            uint fractVal = reader.ReadInt(NORMAL_FRACTIONAL_BITS);
            float value = fractVal * NORMAL_RESOLUTION;

            if (isNegative)
            {
                value *= -1;
            }

            return value;
        }
    }
}
