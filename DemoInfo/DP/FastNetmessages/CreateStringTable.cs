using System;
using System.IO;

namespace DemoInfo
{
    public struct CreateStringTable
    {
        public string Name;
        public int MaxEntries;
        public int NumEntries;
        private int _UserDataFixedSize;
        public bool UserDataFixedSize => _UserDataFixedSize != 0;
        public int UserDataSize;
        public int UserDataSizeBits;
        public int Flags;

        public void Parse(IBitStream bitstream, DemoParser parser)
        {
            while (!bitstream.ChunkFinished)
            {
                int desc = bitstream.ReadProtobufVarInt();
                int wireType = desc & 7;
                int fieldnum = desc >> 3;

                if (wireType == 2)
                {
                    if (fieldnum == 1)
                    {
                        Name = bitstream.ReadProtobufString();
                        continue;
                    }
                    else if (fieldnum == 8)
                    {
                        // String data is special.
                        // We'll simply hope that gaben is nice and sends
                        // string_data last, just like he should.
                        int len = bitstream.ReadProtobufVarInt();
                        bitstream.BeginChunk(len * 8);
                        DP.Handler.CreateStringTableUserInfoHandler.Apply(this, bitstream, parser);
                        bitstream.EndChunk();
                        if (!bitstream.ChunkFinished)
                        {
                            throw new NotImplementedException("Expectec Chunk to be finished.");
                        }
                        break;
                    }
                    else
                    {
                        throw new InvalidDataException();
                    }
                }

                if (wireType != 0)
                {
                    throw new InvalidDataException();
                }

                int val = bitstream.ReadProtobufVarInt();

                switch (fieldnum)
                {
                    case 2:
                        MaxEntries = val;
                        break;
                    case 3:
                        NumEntries = val;
                        break;
                    case 4:
                        _UserDataFixedSize = val;
                        break;
                    case 5:
                        UserDataSize = val;
                        break;
                    case 6:
                        UserDataSizeBits = val;
                        break;
                    case 7:
                        Flags = val;
                        break;
                    default:
                        // silently drop
                        break;
                }
            }
        }
    }
}

