using System;
using System.IO;

namespace DemoInfo.DP.FastNetmessages
{
    /// <summary>
    /// FastNetMessage adaptation of CMsg_CVars.CVar protobuf message
    /// https://github.com/SteamDatabase/Protobufs/blob/d6c75921889c65b3b885c19d94812220edfc1218/csgo/netmessages.proto#L128
    /// </summary>
    public struct CVar
    {
        public string Name;
        public string Value;
        public uint DictionaryName;

        public void Parse(IBitStream bitstream, DemoParser parser)
        {
            while (!bitstream.ChunkFinished)
            {
                int desc = bitstream.ReadProtobufVarInt();
                int wireType = desc & 7;
                int fieldnum = desc >> 3;

                if (wireType == 2 && fieldnum == 1)
                {
                    Name = bitstream.ReadProtobufString();
                }
                else if (wireType == 2 && fieldnum == 2)
                {
                    Value = bitstream.ReadProtobufString();
                }
                else if (wireType == 0 && fieldnum == 3)
                {
                    DictionaryName = (uint)bitstream.ReadProtobufVarInt();
                }
                else
                {
                    throw new InvalidDataException();
                }
            }

            Raise(parser);
        }

        private void Raise(DemoParser parser)
        {
            parser.RaiseConVarChange(new ConVarChangeEventArgs
            {
                Name = Name,
                Value = Value,
                DictionaryValue = DictionaryName,
            });
        }
    }
}
