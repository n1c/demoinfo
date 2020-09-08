﻿using System.Collections.Generic;
using System.IO;

namespace DemoInfo
{
    public struct SendTable
    {
        public struct SendProp
        {
            public int Type;
            public string VarName;
            public int Flags;
            public int Priority;
            public string DtName;
            public int NumElements;
            public float LowValue;
            public float HighValue;
            public int NumBits;

            public void Parse(IBitStream bitstream)
            {
                while (!bitstream.ChunkFinished)
                {
                    int desc = bitstream.ReadProtobufVarInt();
                    int wireType = desc & 7;
                    int fieldnum = desc >> 3;

                    if (wireType == 2)
                    {
                        if (fieldnum == 2)
                        {
                            VarName = bitstream.ReadProtobufString();
                        }
                        else if (fieldnum == 5)
                        {
                            DtName = bitstream.ReadProtobufString();
                        }
                        else
                        {
                            throw new InvalidDataException();
                        }
                    }
                    else if (wireType == 0)
                    {
                        int val = bitstream.ReadProtobufVarInt();

                        switch (fieldnum)
                        {
                            case 1:
                                Type = val;
                                break;
                            case 3:
                                Flags = val;
                                break;
                            case 4:
                                Priority = val;
                                break;
                            case 6:
                                NumElements = val;
                                break;
                            case 9:
                                NumBits = val;
                                break;
                            default:
                                // silently drop
                                break;
                        }
                    }
                    else if (wireType == 5)
                    {
                        float val = bitstream.ReadFloat();

                        switch (fieldnum)
                        {
                            case 7:
                                LowValue = val;
                                break;
                            case 8:
                                HighValue = val;
                                break;
                            default:
                                // silently drop
                                break;
                        }
                    }
                    else
                    {
                        throw new InvalidDataException();
                    }
                }
            }
        }

        private int _IsEnd;
        public bool IsEnd => _IsEnd != 0;
        public string NetTableName;
        public int _NeedsDecoder;
        public bool NeedsDecoder => _NeedsDecoder != 0;

        public IEnumerable<SendProp> Parse(IBitStream bitstream)
        {
            List<SendProp> sendprops = new List<SendProp>();

            while (!bitstream.ChunkFinished)
            {
                int desc = bitstream.ReadProtobufVarInt();
                int wireType = desc & 7;
                int fieldnum = desc >> 3;

                if (wireType == 2)
                {
                    if (fieldnum == 2)
                    {
                        NetTableName = bitstream.ReadProtobufString();
                    }
                    else if (fieldnum == 4)
                    {
                        // Props are special.
                        // We'll simply hope that gaben is nice and sends
                        // props last, just like he should.
                        int len = bitstream.ReadProtobufVarInt();
                        bitstream.BeginChunk(len * 8);
                        SendProp sendprop = new SendProp();
                        sendprop.Parse(bitstream);
                        sendprops.Add(sendprop);
                        bitstream.EndChunk();
                    }
                    else
                    {
                        throw new InvalidDataException();
                    }
                }
                else if (wireType == 0)
                {
                    int val = bitstream.ReadProtobufVarInt();

                    switch (fieldnum)
                    {
                        case 1:
                            _IsEnd = val;
                            break;
                        case 3:
                            _NeedsDecoder = val;
                            break;
                        default:
                            // silently drop
                            break;
                    }
                }
                else
                {
                    throw new InvalidDataException();
                }
            }

            return sendprops;
        }
    }
}
