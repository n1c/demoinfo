using System;
using System.Linq;

namespace DemoInfo.BitStreamImpl
{
    public class DebugBitStream : IBitStream
    {
        private readonly IBitStream A, B;

        public DebugBitStream(IBitStream a, IBitStream b)
        {
            A = a;
            B = b;
        }

        public void Initialize(System.IO.Stream stream)
        {
            throw new NotImplementedException();
        }

        void IDisposable.Dispose()
        {
            A.Dispose();
            B.Dispose();
        }

        private void Verify<T>(T a, T b)
        {
            if (!a.Equals(b))
            {
                System.Diagnostics.Debug.Assert(false);
                throw new InvalidOperationException(string.Format("{0} vs {1} ({2} vs {3})",
                    a, b, A.GetType().Name, B.GetType().Name));
            }
        }

        public uint ReadInt(int bits)
        {
            uint a = A.ReadInt(bits);
            uint b = B.ReadInt(bits);
            Verify(a, b);
            return a;
        }

        public int ReadSignedInt(int bits)
        {
            int a = A.ReadSignedInt(bits);
            int b = B.ReadSignedInt(bits);
            Verify(a, b);
            return a;
        }

        public bool ReadBit()
        {
            bool a = A.ReadBit();
            bool b = B.ReadBit();
            Verify(a, b);
            return a;
        }

        public byte ReadByte()
        {
            byte a = A.ReadByte();
            byte b = B.ReadByte();
            Verify(a, b);
            return a;
        }

        public byte ReadByte(int bits)
        {
            byte a = A.ReadByte(bits);
            byte b = B.ReadByte(bits);
            Verify(a, b);
            return a;
        }

        public byte[] ReadBytes(int bytes)
        {
            byte[] a = A.ReadBytes(bytes);
            byte[] b = B.ReadBytes(bytes);
            Verify(a.SequenceEqual(b), true);
            return a;
        }

        public string ReadString()
        {
            string a = A.ReadString();
            string b = B.ReadString();
            Verify(a, b);
            return a;
        }

        public string ReadString(int size)
        {
            string a = A.ReadString(size);
            string b = B.ReadString(size);
            Verify(a, b);
            return a;
        }

        public uint ReadVarInt()
        {
            uint a = A.ReadVarInt();
            uint b = B.ReadVarInt();
            Verify(a, b);
            return a;
        }

        public uint ReadUBitInt()
        {
            uint a = A.ReadUBitInt();
            uint b = B.ReadUBitInt();
            Verify(a, b);
            return a;
        }

        public float ReadFloat()
        {
            float a = A.ReadFloat();
            float b = B.ReadFloat();
            Verify(a, b);
            return a;
        }

        public byte[] ReadBits(int bits)
        {
            byte[] a = A.ReadBits(bits);
            byte[] b = B.ReadBits(bits);
            Verify(a.SequenceEqual(b), true);
            return a;
        }

        public int ReadProtobufVarInt()
        {
            int a = A.ReadProtobufVarInt();
            int b = B.ReadProtobufVarInt();
            Verify(a, b);
            return a;
        }

        public void BeginChunk(int bits)
        {
            A.BeginChunk(bits);
            B.BeginChunk(bits);
        }

        public void EndChunk()
        {
            A.EndChunk();
            B.EndChunk();
        }

        public bool ChunkFinished
        {
            get
            {
                bool a = A.ChunkFinished;
                bool b = B.ChunkFinished;
                Verify(a, b);
                return a;
            }
        }
    }
}

