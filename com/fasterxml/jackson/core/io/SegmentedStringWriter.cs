using Sharpen;

namespace com.fasterxml.jackson.core.io
{
	/// <summary>
	/// Efficient alternative to
	/// <see cref="System.IO.StringWriter"/>
	/// , based on using segmented
	/// internal buffer. Initial input buffer is also recyclable.
	/// <p>
	/// This class is most useful when serializing JSON content as a String:
	/// if so, instance of this class can be given as the writer to
	/// <code>JsonGenerator</code>.
	/// </summary>
	public sealed class SegmentedStringWriter : System.IO.TextWriter
	{
		protected internal readonly com.fasterxml.jackson.core.util.TextBuffer _buffer;

		public SegmentedStringWriter(com.fasterxml.jackson.core.util.BufferRecycler br)
			: base()
		{
			_buffer = new com.fasterxml.jackson.core.util.TextBuffer(br);
		}

		/*
		/**********************************************************
		/* java.io.Writer implementation
		/**********************************************************
		*/
		public override System.IO.TextWriter Append(char c)
		{
			write(c);
			return this;
		}

		public override System.IO.TextWriter Append(Sharpen.CharSequence csq)
		{
			string str = csq.ToString();
			_buffer.append(str, 0, str.Length);
			return this;
		}

		public override System.IO.TextWriter AppendRange(Sharpen.CharSequence csq, int start
			, int end)
		{
			string str = csq.subSequence(start, end).ToString();
			_buffer.append(str, 0, str.Length);
			return this;
		}

		public override void close()
		{
		}

		// NOP
		public override void flush()
		{
		}

		// NOP
		public override void write(char[] cbuf)
		{
			_buffer.append(cbuf, 0, cbuf.Length);
		}

		public override void write(char[] cbuf, int off, int len)
		{
			_buffer.append(cbuf, off, len);
		}

		public override void write(int c)
		{
			_buffer.append((char)c);
		}

		public override void write(string str)
		{
			_buffer.append(str, 0, str.Length);
		}

		public override void write(string str, int off, int len)
		{
			_buffer.append(str, off, len);
		}

		/*
		/**********************************************************
		/* Extended API
		/**********************************************************
		*/
		/// <summary>
		/// Main access method that will construct a String that contains
		/// all the contents, release all internal buffers we may have,
		/// and return result String.
		/// </summary>
		/// <remarks>
		/// Main access method that will construct a String that contains
		/// all the contents, release all internal buffers we may have,
		/// and return result String.
		/// Note that the method is not idempotent -- if called second time,
		/// will just return an empty String.
		/// </remarks>
		public string getAndClear()
		{
			string result = _buffer.contentsAsString();
			_buffer.releaseBuffers();
			return result;
		}
	}
}
