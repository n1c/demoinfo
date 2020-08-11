using System.IO;
using System.Collections.Generic;
using DemoInfo.DP.Handler;

namespace DemoInfo
{
    public struct GameEventList
    {
        public struct Key
        {
            public int Type;
            public string Name;

            public void Parse(IBitStream bitstream)
            {
                while (!bitstream.ChunkFinished)
                {
                    int desc = bitstream.ReadProtobufVarInt();
                    int wireType = desc & 7;
                    int fieldnum = desc >> 3;
                    if ((wireType == 0) && (fieldnum == 1))
                    {
                        Type = bitstream.ReadProtobufVarInt();
                    }
                    else if ((wireType == 2) && (fieldnum == 2))
                    {
                        Name = bitstream.ReadProtobufString();
                    }
                    else
                    {
                        throw new InvalidDataException();
                    }
                }
            }
        }

        public struct Descriptor
        {
            public int EventId;
            public string Name;
            public Key[] Keys;

            public void Parse(IBitStream bitstream)
            {
                List<Key> keys = new List<Key>();
                while (!bitstream.ChunkFinished)
                {
                    int desc = bitstream.ReadProtobufVarInt();
                    int wireType = desc & 7;
                    int fieldnum = desc >> 3;
                    if ((wireType == 0) && (fieldnum == 1))
                    {
                        EventId = bitstream.ReadProtobufVarInt();
                    }
                    else if ((wireType == 2) && (fieldnum == 2))
                    {
                        Name = bitstream.ReadProtobufString();
                    }
                    else if ((wireType == 2) && (fieldnum == 3))
                    {
                        int length = bitstream.ReadProtobufVarInt();
                        bitstream.BeginChunk(length * 8);
                        Key key = new Key();
                        key.Parse(bitstream);
                        keys.Add(key);
                        bitstream.EndChunk();
                    }
                    else
                    {
                        throw new InvalidDataException();
                    }
                }

                Keys = keys.ToArray();
            }
        }

        public void Parse(IBitStream bitstream, DemoParser parser)
        {
            GameEventHandler.HandleGameEventList(ReadDescriptors(bitstream), parser);
        }

        private IEnumerable<Descriptor> ReadDescriptors(IBitStream bitstream)
        {
            while (!bitstream.ChunkFinished)
            {
                int desc = bitstream.ReadProtobufVarInt();
                int wireType = desc & 7;
                int fieldnum = desc >> 3;
                if ((wireType != 2) || (fieldnum != 1))
                {
                    throw new InvalidDataException();
                }

                int length = bitstream.ReadProtobufVarInt();
                bitstream.BeginChunk(length * 8);
                Descriptor descriptor = new Descriptor();
                descriptor.Parse(bitstream);
                yield return descriptor;
                bitstream.EndChunk();
            }
        }
    }
}
