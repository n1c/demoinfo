﻿using System;
using System.IO;

namespace DemoInfo
{
    public class LimitStream : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => _Length;

        public override long Position
        {
            get => _Position;
            set => throw new NotImplementedException();
        }

        private readonly Stream Underlying;
        private readonly long _Length;
        private long _Position;

        public LimitStream(Stream underlying, long length)
        {
            if (!underlying.CanRead)
            {
                throw new NotImplementedException();
            }

            if (length <= 0)
            {
                throw new ArgumentException("length");
            }

            Underlying = underlying;
            _Length = length;
            _Position = 0;
        }

        private const int TrashSize = 4096;
        private static readonly byte[] Dignitrash = new byte[TrashSize];

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                long remaining = Length - _Position;
                if (Underlying.CanSeek)
                {
                    _ = Underlying.Seek(remaining, SeekOrigin.Current);
                }
                else
                {
                    while (remaining > 0)
                    {
                        _ = Underlying.Read(Dignitrash, 0, checked((int)Math.Min(TrashSize, remaining)));
                        remaining -= TrashSize; // could go beyond 0, but it's signed so who cares
                    }
                }
            }
        }

        public override void Flush() { Underlying.Flush(); }

        public byte[] ReadBytes(int count)
        {
            byte[] data = new byte[count];
            int offset = 0;
            while (offset < count)
            {
                int thisTime = Read(data, offset, count - offset);
                if (thisTime == 0)
                {
                    throw new EndOfStreamException();
                }

                offset += thisTime;
            }
            return data;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            count = checked((int)Math.Min(count, Length - _Position)); // should never throw (count <= int_max)
            int ret = Underlying.Read(buffer, offset, count);
            _Position += ret;
            return ret;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}

