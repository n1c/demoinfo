using System;
using System.IO;

namespace DemoInfo
{
    public struct PacketEntities
    {
        public int MaxEntries;
        public int UpdatedEntries;
        private int _IsDelta;
        public bool IsDelta => _IsDelta != 0;
        private int _UpdateBaseline;
        public bool UpdateBaseline => _UpdateBaseline != 0;
        public int Baseline;
        public int DeltaFrom;

        public void Parse(IBitStream bitstream, DemoParser parser)
        {
            while (!bitstream.ChunkFinished)
            {
                int desc = bitstream.ReadProtobufVarInt();
                int wireType = desc & 7;
                int fieldnum = desc >> 3;

                if ((fieldnum == 7) && (wireType == 2))
                {
                    // Entity data is special.
                    // We'll simply hope that gaben is nice and sends
                    // entity_data last, just like he should.

                    int len = bitstream.ReadProtobufVarInt();
                    bitstream.BeginChunk(len * 8);
                    DP.Handler.PacketEntitesHandler.Apply(this, bitstream, parser);
                    bitstream.EndChunk();
                    if (!bitstream.ChunkFinished)
                    {
                        throw new NotImplementedException("Expected Chunk to be finished.");
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
                        MaxEntries = val;
                        break;
                    case 2:
                        UpdatedEntries = val;
                        break;
                    case 3:
                        _IsDelta = val;
                        break;
                    case 4:
                        _UpdateBaseline = val;
                        break;
                    case 5:
                        Baseline = val;
                        break;
                    case 6:
                        DeltaFrom = val;
                        break;
                    default:
                        // silently drop
                        break;
                }
            }
        }
    }
}

