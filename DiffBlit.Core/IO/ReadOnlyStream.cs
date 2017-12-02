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

        /// <inheritdoc />
        public ReadOnlyStream(Stream baseStream)
        {
            _base = baseStream;
        }

        /// <inheritdoc />
        public override bool CanRead => true;

        /// <inheritdoc />
        public override bool CanSeek => true;

        /// <inheritdoc />
        public override bool CanWrite => false;

        /// <inheritdoc />
        public override long Length => _base.Length;

        /// <inheritdoc />
        public override long Position
        {
            get => _base.Position;
            set => _base.Position = value;
        }

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin)
        {
            return _base.Seek(offset, origin);
        }

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
        {
            return _base.Read(buffer, offset, count);
        }

        /// <inheritdoc />
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public override void Flush()
        {
            throw new NotSupportedException();
        }
    }
}
