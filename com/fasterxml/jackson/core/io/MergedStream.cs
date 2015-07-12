using Sharpen;

namespace com.fasterxml.jackson.core.io
{
	/// <summary>
	/// Simple
	/// <see cref="Sharpen.InputStream"/>
	/// implementation that is used to "unwind" some
	/// data previously read from an input stream; so that as long as some of
	/// that data remains, it's returned; but as long as it's read, we'll
	/// just use data from the underlying original stream.
	/// This is similar to
	/// <see cref="java.io.PushbackInputStream"/>
	/// , but here there's
	/// only one implicit pushback, when instance is constructed.
	/// </summary>
	public sealed class MergedStream : Sharpen.InputStream
	{
		private readonly com.fasterxml.jackson.core.io.IOContext _ctxt;

		private readonly Sharpen.InputStream _in;

		private byte[] _b;

		private int _ptr;

		private readonly int _end;

		public MergedStream(com.fasterxml.jackson.core.io.IOContext ctxt, Sharpen.InputStream
			 @in, byte[] buf, int start, int end)
		{
			_ctxt = ctxt;
			_in = @in;
			_b = buf;
			_ptr = start;
			_end = end;
		}

		/// <exception cref="System.IO.IOException"/>
		public override int available()
		{
			if (_b != null)
			{
				return _end - _ptr;
			}
			return _in.available();
		}

		/// <exception cref="System.IO.IOException"/>
		public override void close()
		{
			_free();
			_in.close();
		}

		public override void mark(int readlimit)
		{
			if (_b == null)
			{
				_in.mark(readlimit);
			}
		}

		public override bool markSupported()
		{
			// Only supports marks past the initial rewindable section...
			return (_b == null) && _in.markSupported();
		}

		/// <exception cref="System.IO.IOException"/>
		public override int read()
		{
			if (_b != null)
			{
				int c = _b[_ptr++] & unchecked((int)(0xFF));
				if (_ptr >= _end)
				{
					_free();
				}
				return c;
			}
			return _in.read();
		}

		/// <exception cref="System.IO.IOException"/>
		public override int read(byte[] b)
		{
			return read(b, 0, b.Length);
		}

		/// <exception cref="System.IO.IOException"/>
		public override int read(byte[] b, int off, int len)
		{
			if (_b != null)
			{
				int avail = _end - _ptr;
				if (len > avail)
				{
					len = avail;
				}
				System.Array.Copy(_b, _ptr, b, off, len);
				_ptr += len;
				if (_ptr >= _end)
				{
					_free();
				}
				return len;
			}
			return _in.read(b, off, len);
		}

		/// <exception cref="System.IO.IOException"/>
		public override void reset()
		{
			if (_b == null)
			{
				_in.reset();
			}
		}

		/// <exception cref="System.IO.IOException"/>
		public override long skip(long n)
		{
			long count = 0L;
			if (_b != null)
			{
				int amount = _end - _ptr;
				if (amount > n)
				{
					// all in pushed back segment?
					_ptr += (int)n;
					return n;
				}
				_free();
				count += amount;
				n -= amount;
			}
			if (n > 0)
			{
				count += _in.skip(n);
			}
			return count;
		}

		private void _free()
		{
			byte[] buf = _b;
			if (buf != null)
			{
				_b = null;
				if (_ctxt != null)
				{
					_ctxt.releaseReadIOBuffer(buf);
				}
			}
		}
	}
}
