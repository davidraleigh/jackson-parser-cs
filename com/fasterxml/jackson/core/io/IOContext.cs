using Sharpen;

namespace com.fasterxml.jackson.core.io
{
	/// <summary>
	/// To limit number of configuration and state objects to pass, all
	/// contextual objects that need to be passed by the factory to
	/// readers and writers are combined under this object.
	/// </summary>
	/// <remarks>
	/// To limit number of configuration and state objects to pass, all
	/// contextual objects that need to be passed by the factory to
	/// readers and writers are combined under this object. One instance
	/// is created for each reader and writer.
	/// <p>
	/// NOTE: non-final since 2.4, to allow sub-classing.
	/// </remarks>
	public class IOContext
	{
		/// <summary>
		/// Reference to the source object, which can be used for displaying
		/// location information
		/// </summary>
		protected internal readonly object _sourceRef;

		/// <summary>Encoding used by the underlying stream, if known.</summary>
		protected internal com.fasterxml.jackson.core.JsonEncoding _encoding;

		/// <summary>
		/// Flag that indicates whether underlying input/output source/target
		/// object is fully managed by the owner of this context (parser or
		/// generator).
		/// </summary>
		/// <remarks>
		/// Flag that indicates whether underlying input/output source/target
		/// object is fully managed by the owner of this context (parser or
		/// generator). If true, it is, and is to be closed by parser/generator;
		/// if false, calling application has to do closing (unless auto-closing
		/// feature is enabled for the parser/generator in question; in which
		/// case it acts like the owner).
		/// </remarks>
		protected internal readonly bool _managedResource;

		/// <summary>Recycler used for actual allocation/deallocation/reuse</summary>
		protected internal readonly com.fasterxml.jackson.core.util.BufferRecycler _bufferRecycler;

		/// <summary>
		/// Reference to the allocated I/O buffer for low-level input reading,
		/// if any allocated.
		/// </summary>
		protected internal byte[] _readIOBuffer = null;

		/// <summary>
		/// Reference to the allocated I/O buffer used for low-level
		/// encoding-related buffering.
		/// </summary>
		protected internal byte[] _writeEncodingBuffer = null;

		/// <summary>
		/// Reference to the buffer allocated for temporary use with
		/// base64 encoding or decoding.
		/// </summary>
		protected internal byte[] _base64Buffer = null;

		/// <summary>
		/// Reference to the buffer allocated for tokenization purposes,
		/// in which character input is read, and from which it can be
		/// further returned.
		/// </summary>
		protected internal char[] _tokenCBuffer = null;

		/// <summary>
		/// Reference to the buffer allocated for buffering it for
		/// output, before being encoded: generally this means concatenating
		/// output, then encoding when buffer fills up.
		/// </summary>
		protected internal char[] _concatCBuffer = null;

		/// <summary>
		/// Reference temporary buffer Parser instances need if calling
		/// app decides it wants to access name via 'getTextCharacters' method.
		/// </summary>
		/// <remarks>
		/// Reference temporary buffer Parser instances need if calling
		/// app decides it wants to access name via 'getTextCharacters' method.
		/// Regular text buffer can not be used as it may contain textual
		/// representation of the value token.
		/// </remarks>
		protected internal char[] _nameCopyBuffer = null;

		public IOContext(com.fasterxml.jackson.core.util.BufferRecycler br, object sourceRef
			, bool managedResource)
		{
			/*
			/**********************************************************
			/* Configuration
			/**********************************************************
			*/
			/*
			/**********************************************************
			/* Buffer handling, recycling
			/**********************************************************
			*/
			/*
			/**********************************************************
			/* Life-cycle
			/**********************************************************
			*/
			_bufferRecycler = br;
			_sourceRef = sourceRef;
			_managedResource = managedResource;
		}

		public virtual void setEncoding(com.fasterxml.jackson.core.JsonEncoding enc)
		{
			_encoding = enc;
		}

		/// <since>1.6</since>
		public virtual com.fasterxml.jackson.core.io.IOContext withEncoding(com.fasterxml.jackson.core.JsonEncoding
			 enc)
		{
			_encoding = enc;
			return this;
		}

		/*
		/**********************************************************
		/* Public API, accessors
		/**********************************************************
		*/
		public virtual object getSourceReference()
		{
			return _sourceRef;
		}

		public virtual com.fasterxml.jackson.core.JsonEncoding getEncoding()
		{
			return _encoding;
		}

		public virtual bool isResourceManaged()
		{
			return _managedResource;
		}

		/*
		/**********************************************************
		/* Public API, buffer management
		/**********************************************************
		*/
		public virtual com.fasterxml.jackson.core.util.TextBuffer constructTextBuffer()
		{
			return new com.fasterxml.jackson.core.util.TextBuffer(_bufferRecycler);
		}

		/// <summary>
		/// <p>
		/// Note: the method can only be called once during its life cycle.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Note: the method can only be called once during its life cycle.
		/// This is to protect against accidental sharing.
		/// </remarks>
		public virtual byte[] allocReadIOBuffer()
		{
			_verifyAlloc(_readIOBuffer);
			return (_readIOBuffer = _bufferRecycler.allocByteBuffer(com.fasterxml.jackson.core.util.BufferRecycler
				.BYTE_READ_IO_BUFFER));
		}

		/// <since>2.4</since>
		public virtual byte[] allocReadIOBuffer(int minSize)
		{
			_verifyAlloc(_readIOBuffer);
			return (_readIOBuffer = _bufferRecycler.allocByteBuffer(com.fasterxml.jackson.core.util.BufferRecycler
				.BYTE_READ_IO_BUFFER, minSize));
		}

		public virtual byte[] allocWriteEncodingBuffer()
		{
			_verifyAlloc(_writeEncodingBuffer);
			return (_writeEncodingBuffer = _bufferRecycler.allocByteBuffer(com.fasterxml.jackson.core.util.BufferRecycler
				.BYTE_WRITE_ENCODING_BUFFER));
		}

		/// <since>2.4</since>
		public virtual byte[] allocWriteEncodingBuffer(int minSize)
		{
			_verifyAlloc(_writeEncodingBuffer);
			return (_writeEncodingBuffer = _bufferRecycler.allocByteBuffer(com.fasterxml.jackson.core.util.BufferRecycler
				.BYTE_WRITE_ENCODING_BUFFER, minSize));
		}

		/// <since>2.1</since>
		public virtual byte[] allocBase64Buffer()
		{
			_verifyAlloc(_base64Buffer);
			return (_base64Buffer = _bufferRecycler.allocByteBuffer(com.fasterxml.jackson.core.util.BufferRecycler
				.BYTE_BASE64_CODEC_BUFFER));
		}

		public virtual char[] allocTokenBuffer()
		{
			_verifyAlloc(_tokenCBuffer);
			return (_tokenCBuffer = _bufferRecycler.allocCharBuffer(com.fasterxml.jackson.core.util.BufferRecycler
				.CHAR_TOKEN_BUFFER));
		}

		/// <since>2.4</since>
		public virtual char[] allocTokenBuffer(int minSize)
		{
			_verifyAlloc(_tokenCBuffer);
			return (_tokenCBuffer = _bufferRecycler.allocCharBuffer(com.fasterxml.jackson.core.util.BufferRecycler
				.CHAR_TOKEN_BUFFER, minSize));
		}

		public virtual char[] allocConcatBuffer()
		{
			_verifyAlloc(_concatCBuffer);
			return (_concatCBuffer = _bufferRecycler.allocCharBuffer(com.fasterxml.jackson.core.util.BufferRecycler
				.CHAR_CONCAT_BUFFER));
		}

		public virtual char[] allocNameCopyBuffer(int minSize)
		{
			_verifyAlloc(_nameCopyBuffer);
			return (_nameCopyBuffer = _bufferRecycler.allocCharBuffer(com.fasterxml.jackson.core.util.BufferRecycler
				.CHAR_NAME_COPY_BUFFER, minSize));
		}

		/// <summary>
		/// Method to call when all the processing buffers can be safely
		/// recycled.
		/// </summary>
		public virtual void releaseReadIOBuffer(byte[] buf)
		{
			if (buf != null)
			{
				/* Let's do sanity checks to ensure once-and-only-once release,
				* as well as avoiding trying to release buffers not owned
				*/
				_verifyRelease(buf, _readIOBuffer);
				_readIOBuffer = null;
				_bufferRecycler.releaseByteBuffer(com.fasterxml.jackson.core.util.BufferRecycler.
					BYTE_READ_IO_BUFFER, buf);
			}
		}

		public virtual void releaseWriteEncodingBuffer(byte[] buf)
		{
			if (buf != null)
			{
				/* Let's do sanity checks to ensure once-and-only-once release,
				* as well as avoiding trying to release buffers not owned
				*/
				_verifyRelease(buf, _writeEncodingBuffer);
				_writeEncodingBuffer = null;
				_bufferRecycler.releaseByteBuffer(com.fasterxml.jackson.core.util.BufferRecycler.
					BYTE_WRITE_ENCODING_BUFFER, buf);
			}
		}

		public virtual void releaseBase64Buffer(byte[] buf)
		{
			if (buf != null)
			{
				// sanity checks, release once-and-only-once, must be one owned
				_verifyRelease(buf, _base64Buffer);
				_base64Buffer = null;
				_bufferRecycler.releaseByteBuffer(com.fasterxml.jackson.core.util.BufferRecycler.
					BYTE_BASE64_CODEC_BUFFER, buf);
			}
		}

		public virtual void releaseTokenBuffer(char[] buf)
		{
			if (buf != null)
			{
				_verifyRelease(buf, _tokenCBuffer);
				_tokenCBuffer = null;
				_bufferRecycler.releaseCharBuffer(com.fasterxml.jackson.core.util.BufferRecycler.
					CHAR_TOKEN_BUFFER, buf);
			}
		}

		public virtual void releaseConcatBuffer(char[] buf)
		{
			if (buf != null)
			{
				// 14-Jan-2014, tatu: Let's actually allow upgrade of the original buffer.
				_verifyRelease(buf, _concatCBuffer);
				_concatCBuffer = null;
				_bufferRecycler.releaseCharBuffer(com.fasterxml.jackson.core.util.BufferRecycler.
					CHAR_CONCAT_BUFFER, buf);
			}
		}

		public virtual void releaseNameCopyBuffer(char[] buf)
		{
			if (buf != null)
			{
				// 14-Jan-2014, tatu: Let's actually allow upgrade of the original buffer.
				_verifyRelease(buf, _nameCopyBuffer);
				_nameCopyBuffer = null;
				_bufferRecycler.releaseCharBuffer(com.fasterxml.jackson.core.util.BufferRecycler.
					CHAR_NAME_COPY_BUFFER, buf);
			}
		}

		/*
		/**********************************************************
		/* Internal helpers
		/**********************************************************
		*/
		protected internal void _verifyAlloc(object buffer)
		{
			if (buffer != null)
			{
				throw new System.InvalidOperationException("Trying to call same allocXxx() method second time"
					);
			}
		}

		protected internal void _verifyRelease(byte[] toRelease, byte[] src)
		{
			if ((toRelease != src) && (toRelease.Length <= src.Length))
			{
				throw wrongBuf();
			}
		}

		protected internal void _verifyRelease(char[] toRelease, char[] src)
		{
			if ((toRelease != src) && (toRelease.Length <= src.Length))
			{
				throw wrongBuf();
			}
		}

		private System.ArgumentException wrongBuf()
		{
			return new System.ArgumentException("Trying to release buffer not owned by the context"
				);
		}
	}
}
