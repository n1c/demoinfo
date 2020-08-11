using System;
using System.IO;

namespace DemoInfo
{
    public struct UpdateStringTable
    {
        public int TableId;
        public int NumChangedEntries;

        public void Parse(IBitStream bitstream, DemoParser parser)
        {
            while (!bitstream.ChunkFinished)
            {
                int desc = bitstream.ReadProtobufVarInt();
                int wireType = desc & 7;
                int fieldnum = desc >> 3;

                if ((wireType == 2) && (fieldnum == 3))
                {
                    int len = bitstream.ReadProtobufVarInt();
                    bitstream.BeginChunk(len * 8);
                    DP.Handler.UpdateStringTableUserInfoHandler.Apply(this, bitstream, parser);
                    bitstream.EndChunk();
                    if (!bitstream.ChunkFinished)
                    {
                        throw new NotImplementedException("Expected bitstream to be finished");
                    }

                    break;
                }

                if (wireType != 0)
                {
                    throw new InvalidDataException();
                }

                int val = bitstream.ReadProtobufVarInt();

                switch (fieldnum)
                {
                    case 1:
                        TableId = val;
                        break;
                    case 2:
                        NumChangedEntries = val;
                        break;
                    default:
                        // silently drop
                        break;
                }
            }
        }
    }
}

