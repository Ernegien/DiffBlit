using System;
using System.IO;

namespace DiffBlit.Core.IO
{
    /// <summary>
    /// Provides read-only seekable access to a base stream.
    /// </summary>
    public class ReadOnlyStream : Stream
    {
        private readonly Stream _base;

        public ReadOnlyStream(Stream baseStream)
        {
            _base = baseStream;
        }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;

        public override long Length => _base.Length;

        public override long Position
        {
            get => _base.Position;
            set => _base.Position = value;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _base.Seek(offset, origin);
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            return _base.Read(buffer, offset, count);
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }
    }
}
