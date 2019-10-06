using System;
using System.IO;

namespace JAudio
{
	internal class VirtualStream : Stream
	{
		private BinaryReader binaryReader;

		private long _Position;

		private long _Length;

		public override bool CanRead => true;

		public override bool CanSeek => true;

		public override bool CanWrite => false;

		public override long Length => _Length;

		public override long Position
		{
			get
			{
				return _Position;
			}
			set
			{
				if (value < 0)
				{
					throw new ArgumentException("The stream position cannot be less than zero.", "Position");
				}
				if (value > Length)
				{
					throw new ArgumentException("The stream position must be less than the stream length.", "Position");
				}
				_Position = value;
			}
		}

		private long StartOffset
		{
			get;
			set;
		}

		public VirtualStream(Stream stream, long startPosition, long size)
		{
			try
			{
				if (startPosition >= stream.Length)
				{
					throw new ArgumentException("The starting offset cannot be greater than the size of the stream.", "startPosition");
				}
				if (startPosition + size > stream.Length)
				{
					throw new ArgumentException("The virtual file cannot reach beyond the end of the stream.", "size");
				}
				if (size > 0)
				{
					SetLength(size);
				}
				else
				{
					SetLength(stream.Length - startPosition);
				}
				StartOffset = startPosition;
				binaryReader = new BinaryReader(stream);
				Position = 0L;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public override void Flush()
		{
			throw new NotImplementedException();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			try
			{
				long position = binaryReader.BaseStream.Position;
				binaryReader.BaseStream.Position = Position + StartOffset;
				int num = binaryReader.Read(buffer, offset, count);
				binaryReader.BaseStream.Position = position;
				Position += num;
				return num;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			switch (origin)
			{
			case SeekOrigin.Begin:
				Position = offset;
				break;
			case SeekOrigin.Current:
				Position += offset;
				break;
			case SeekOrigin.End:
				Position = Length - 1 - offset;
				break;
			}
			return Position;
		}

		public override void SetLength(long value)
		{
			_Length = value;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException();
		}
	}
}
