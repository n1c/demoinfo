using System.IO;

namespace DemoInfo.DP.FastNetmessages
{
    /// <summary>
    /// FastNetMessage adaptation of CNETMsg_SetConVar protobuf message
    /// https://github.com/SteamDatabase/Protobufs/blob/d6c75921889c65b3b885c19d94812220edfc1218/csgo/netmessages.proto#L137
    /// </summary>
    public struct SetConVar
    {
        public void Parse(IBitStream bitstream, DemoParser parser)
        {
            while (!bitstream.ChunkFinished)
            {
                int desc = bitstream.ReadProtobufVarInt();
                int wireType = desc & 7;
                int fieldnum = desc >> 3;

                if (wireType == 2 && fieldnum == 1)
                {
                    bitstream.BeginChunk(bitstream.ReadProtobufVarInt() * 8);
                    new CVars().Parse(bitstream, parser);
                    bitstream.EndChunk();
                }
                else
                {
                    throw new InvalidDataException();
                }
            }
        }
    }
}
