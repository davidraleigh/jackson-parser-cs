using Sharpen;

namespace com.fasterxml.jackson.core.json
{
	/// <summary>
	/// This class is used to determine the encoding of byte stream
	/// that is to contain JSON content.
	/// </summary>
	/// <remarks>
	/// This class is used to determine the encoding of byte stream
	/// that is to contain JSON content. Rules are fairly simple, and
	/// defined in JSON specification (RFC-4627 or newer), except
	/// for BOM handling, which is a property of underlying
	/// streams.
	/// </remarks>
	public sealed class ByteSourceJsonBootstrapper
	{
		internal const byte UTF8_BOM_1 = unchecked((byte)unchecked((int)(0xEF)));

		internal const byte UTF8_BOM_2 = unchecked((byte)unchecked((int)(0xBB)));

		internal const byte UTF8_BOM_3 = unchecked((byte)unchecked((int)(0xBF)));

		protected internal readonly com.fasterxml.jackson.core.io.IOContext _context;

		protected internal readonly Sharpen.InputStream _in;

		protected internal readonly byte[] _inputBuffer;

		private int _inputPtr;

		private int _inputEnd;

		/// <summary>
		/// Flag that indicates whether buffer above is to be recycled
		/// after being used or not.
		/// </summary>
		private readonly bool _bufferRecyclable;

		/// <summary>
		/// Current number of input units (bytes or chars) that were processed in
		/// previous blocks,
		/// before contents of current input buffer.
		/// </summary>
		/// <remarks>
		/// Current number of input units (bytes or chars) that were processed in
		/// previous blocks,
		/// before contents of current input buffer.
		/// <p>
		/// Note: includes possible BOMs, if those were part of the input.
		/// </remarks>
		protected internal int _inputProcessed;

		protected internal bool _bigEndian = true;

		protected internal int _bytesPerChar = 0;

		public ByteSourceJsonBootstrapper(com.fasterxml.jackson.core.io.IOContext ctxt, Sharpen.InputStream
			 @in)
		{
			/*
			/**********************************************************
			/* Configuration
			/**********************************************************
			*/
			/*
			/**********************************************************
			/* Input buffering
			/**********************************************************
			*/
			/*
			/**********************************************************
			/* Input location
			/**********************************************************
			*/
			/*
			/**********************************************************
			/* Data gathered
			/**********************************************************
			*/
			// 0 means "dunno yet"
			/*
			/**********************************************************
			/* Life-cycle
			/**********************************************************
			*/
			_context = ctxt;
			_in = @in;
			_inputBuffer = ctxt.allocReadIOBuffer();
			_inputEnd = _inputPtr = 0;
			_inputProcessed = 0;
			_bufferRecyclable = true;
		}

		public ByteSourceJsonBootstrapper(com.fasterxml.jackson.core.io.IOContext ctxt, byte
			[] inputBuffer, int inputStart, int inputLen)
		{
			_context = ctxt;
			_in = null;
			_inputBuffer = inputBuffer;
			_inputPtr = inputStart;
			_inputEnd = (inputStart + inputLen);
			// Need to offset this for correct location info
			_inputProcessed = -inputStart;
			_bufferRecyclable = false;
		}

		/*
		/**********************************************************
		/*  Encoding detection during bootstrapping
		/**********************************************************
		*/
		/// <summary>Method that should be called after constructing an instace.</summary>
		/// <remarks>
		/// Method that should be called after constructing an instace.
		/// It will figure out encoding that content uses, to allow
		/// for instantiating a proper scanner object.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public com.fasterxml.jackson.core.JsonEncoding detectEncoding()
		{
			bool foundEncoding = false;
			// First things first: BOM handling
			/* Note: we can require 4 bytes to be read, since no
			* combination of BOM + valid JSON content can have
			* shorter length (shortest valid JSON content is single
			* digit char, but BOMs are chosen such that combination
			* is always at least 4 chars long)
			*/
			if (ensureLoaded(4))
			{
				int quad = (_inputBuffer[_inputPtr] << 24) | ((_inputBuffer[_inputPtr + 1] & unchecked(
					(int)(0xFF))) << 16) | ((_inputBuffer[_inputPtr + 2] & unchecked((int)(0xFF))) <<
					 8) | (_inputBuffer[_inputPtr + 3] & unchecked((int)(0xFF)));
				if (handleBOM(quad))
				{
					foundEncoding = true;
				}
				else
				{
					/* If no BOM, need to auto-detect based on first char;
					* this works since it must be 7-bit ascii (wrt. unicode
					* compatible encodings, only ones JSON can be transferred
					* over)
					*/
					// UTF-32?
					if (checkUTF32(quad))
					{
						foundEncoding = true;
					}
					else
					{
						if (checkUTF16((int)(((uint)quad) >> 16)))
						{
							foundEncoding = true;
						}
					}
				}
			}
			else
			{
				if (ensureLoaded(2))
				{
					int i16 = ((_inputBuffer[_inputPtr] & unchecked((int)(0xFF))) << 8) | (_inputBuffer
						[_inputPtr + 1] & unchecked((int)(0xFF)));
					if (checkUTF16(i16))
					{
						foundEncoding = true;
					}
				}
			}
			com.fasterxml.jackson.core.JsonEncoding enc;
			/* Not found yet? As per specs, this means it must be UTF-8. */
			if (!foundEncoding)
			{
				enc = com.fasterxml.jackson.core.JsonEncoding.UTF8;
			}
			else
			{
				switch (_bytesPerChar)
				{
					case 1:
					{
						enc = com.fasterxml.jackson.core.JsonEncoding.UTF8;
						break;
					}

					case 2:
					{
						enc = _bigEndian ? com.fasterxml.jackson.core.JsonEncoding.UTF16_BE : com.fasterxml.jackson.core.JsonEncoding
							.UTF16_LE;
						break;
					}

					case 4:
					{
						enc = _bigEndian ? com.fasterxml.jackson.core.JsonEncoding.UTF32_BE : com.fasterxml.jackson.core.JsonEncoding
							.UTF32_LE;
						break;
					}

					default:
					{
						throw new Sharpen.RuntimeException("Internal error");
					}
				}
			}
			// should never get here
			_context.setEncoding(enc);
			return enc;
		}

		/*
		/**********************************************************
		/* Constructing a Reader
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		public System.IO.StreamReader constructReader()
		{
			com.fasterxml.jackson.core.JsonEncoding enc = _context.getEncoding();
			switch (enc.bits())
			{
				case 8:
				case 16:
				{
					// only in non-common case where we don't want to do direct mapping
					// First: do we have a Stream? If not, need to create one:
					Sharpen.InputStream @in = _in;
					if (@in == null)
					{
						@in = new System.IO.MemoryStream(_inputBuffer, _inputPtr, _inputEnd);
					}
					else
					{
						/* Also, if we have any read but unused input (usually true),
						* need to merge that input in:
						*/
						if (_inputPtr < _inputEnd)
						{
							@in = new com.fasterxml.jackson.core.io.MergedStream(_context, @in, _inputBuffer, 
								_inputPtr, _inputEnd);
						}
					}
					return new System.IO.StreamReader(@in, enc.getJavaName());
				}

				case 32:
				{
					return new com.fasterxml.jackson.core.io.UTF32Reader(_context, _in, _inputBuffer, 
						_inputPtr, _inputEnd, _context.getEncoding().isBigEndian());
				}
			}
			throw new Sharpen.RuntimeException("Internal error");
		}

		// should never get here
		/// <exception cref="System.IO.IOException"/>
		public com.fasterxml.jackson.core.JsonParser constructParser(int parserFeatures, 
			com.fasterxml.jackson.core.ObjectCodec codec, com.fasterxml.jackson.core.sym.ByteQuadsCanonicalizer
			 rootByteSymbols, com.fasterxml.jackson.core.sym.CharsToNameCanonicalizer rootCharSymbols
			, int factoryFeatures)
		{
			com.fasterxml.jackson.core.JsonEncoding enc = detectEncoding();
			if (enc == com.fasterxml.jackson.core.JsonEncoding.UTF8)
			{
				/* and without canonicalization, byte-based approach is not performance; just use std UTF-8 reader
				* (which is ok for larger input; not so hot for smaller; but this is not a common case)
				*/
				if (com.fasterxml.jackson.core.JsonFactory.Feature.CANONICALIZE_FIELD_NAMES.enabledIn
					(factoryFeatures))
				{
					com.fasterxml.jackson.core.sym.ByteQuadsCanonicalizer can = rootByteSymbols.makeChild
						(factoryFeatures);
					return new com.fasterxml.jackson.core.json.UTF8StreamJsonParser(_context, parserFeatures
						, _in, codec, can, _inputBuffer, _inputPtr, _inputEnd, _bufferRecyclable);
				}
			}
			return new com.fasterxml.jackson.core.json.ReaderBasedJsonParser(_context, parserFeatures
				, constructReader(), codec, rootCharSymbols.makeChild(factoryFeatures));
		}

		/*
		/**********************************************************
		/*  Encoding detection for data format auto-detection
		/**********************************************************
		*/
		/// <summary>
		/// Current implementation is not as thorough as other functionality
		/// (
		/// <see cref="ByteSourceJsonBootstrapper"/>
		/// );
		/// supports UTF-8, for example. But it should work, for now, and can
		/// be improved as necessary.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		public static com.fasterxml.jackson.core.format.MatchStrength hasJSONFormat(com.fasterxml.jackson.core.format.InputAccessor
			 acc)
		{
			// Ideally we should see "[" or "{"; but if not, we'll accept double-quote (String)
			// in future could also consider accepting non-standard matches?
			if (!acc.hasMoreBytes())
			{
				return com.fasterxml.jackson.core.format.MatchStrength.INCONCLUSIVE;
			}
			byte b = acc.nextByte();
			// Very first thing, a UTF-8 BOM?
			if (b == UTF8_BOM_1)
			{
				// yes, looks like UTF-8 BOM
				if (!acc.hasMoreBytes())
				{
					return com.fasterxml.jackson.core.format.MatchStrength.INCONCLUSIVE;
				}
				if (acc.nextByte() != UTF8_BOM_2)
				{
					return com.fasterxml.jackson.core.format.MatchStrength.NO_MATCH;
				}
				if (!acc.hasMoreBytes())
				{
					return com.fasterxml.jackson.core.format.MatchStrength.INCONCLUSIVE;
				}
				if (acc.nextByte() != UTF8_BOM_3)
				{
					return com.fasterxml.jackson.core.format.MatchStrength.NO_MATCH;
				}
				if (!acc.hasMoreBytes())
				{
					return com.fasterxml.jackson.core.format.MatchStrength.INCONCLUSIVE;
				}
				b = acc.nextByte();
			}
			// Then possible leading space
			int ch = skipSpace(acc, b);
			if (ch < 0)
			{
				return com.fasterxml.jackson.core.format.MatchStrength.INCONCLUSIVE;
			}
			// First, let's see if it looks like a structured type:
			if (ch == '{')
			{
				// JSON object?
				// Ideally we need to find either double-quote or closing bracket
				ch = skipSpace(acc);
				if (ch < 0)
				{
					return com.fasterxml.jackson.core.format.MatchStrength.INCONCLUSIVE;
				}
				if (ch == '"' || ch == '}')
				{
					return com.fasterxml.jackson.core.format.MatchStrength.SOLID_MATCH;
				}
				// ... should we allow non-standard? Let's not yet... can add if need be
				return com.fasterxml.jackson.core.format.MatchStrength.NO_MATCH;
			}
			com.fasterxml.jackson.core.format.MatchStrength strength;
			if (ch == '[')
			{
				ch = skipSpace(acc);
				if (ch < 0)
				{
					return com.fasterxml.jackson.core.format.MatchStrength.INCONCLUSIVE;
				}
				// closing brackets is easy; but for now, let's also accept opening...
				if (ch == ']' || ch == '[')
				{
					return com.fasterxml.jackson.core.format.MatchStrength.SOLID_MATCH;
				}
				return com.fasterxml.jackson.core.format.MatchStrength.SOLID_MATCH;
			}
			else
			{
				// plain old value is not very convincing...
				strength = com.fasterxml.jackson.core.format.MatchStrength.WEAK_MATCH;
			}
			if (ch == '"')
			{
				// string value
				return strength;
			}
			if (ch <= '9' && ch >= '0')
			{
				// number
				return strength;
			}
			if (ch == '-')
			{
				// negative number
				ch = skipSpace(acc);
				if (ch < 0)
				{
					return com.fasterxml.jackson.core.format.MatchStrength.INCONCLUSIVE;
				}
				return (ch <= '9' && ch >= '0') ? strength : com.fasterxml.jackson.core.format.MatchStrength
					.NO_MATCH;
			}
			// or one of literals
			if (ch == 'n')
			{
				// null
				return tryMatch(acc, "ull", strength);
			}
			if (ch == 't')
			{
				// true
				return tryMatch(acc, "rue", strength);
			}
			if (ch == 'f')
			{
				// false
				return tryMatch(acc, "alse", strength);
			}
			return com.fasterxml.jackson.core.format.MatchStrength.NO_MATCH;
		}

		/// <exception cref="System.IO.IOException"/>
		private static com.fasterxml.jackson.core.format.MatchStrength tryMatch(com.fasterxml.jackson.core.format.InputAccessor
			 acc, string matchStr, com.fasterxml.jackson.core.format.MatchStrength fullMatchStrength
			)
		{
			for (int i = 0; i < len; ++i)
			{
				if (!acc.hasMoreBytes())
				{
					return com.fasterxml.jackson.core.format.MatchStrength.INCONCLUSIVE;
				}
				if (acc.nextByte() != matchStr[i])
				{
					return com.fasterxml.jackson.core.format.MatchStrength.NO_MATCH;
				}
			}
			return fullMatchStrength;
		}

		/// <exception cref="System.IO.IOException"/>
		private static int skipSpace(com.fasterxml.jackson.core.format.InputAccessor acc)
		{
			if (!acc.hasMoreBytes())
			{
				return -1;
			}
			return skipSpace(acc, acc.nextByte());
		}

		/// <exception cref="System.IO.IOException"/>
		private static int skipSpace(com.fasterxml.jackson.core.format.InputAccessor acc, 
			byte b)
		{
			while (true)
			{
				int ch = (int)b & unchecked((int)(0xFF));
				if (!(ch == ' ' || ch == '\r' || ch == '\n' || ch == '\t'))
				{
					return ch;
				}
				if (!acc.hasMoreBytes())
				{
					return -1;
				}
				b = acc.nextByte();
				ch = (int)b & unchecked((int)(0xFF));
			}
		}

		/*
		/**********************************************************
		/* Internal methods, parsing
		/**********************************************************
		*/
		/// <returns>
		/// True if a BOM was succesfully found, and encoding
		/// thereby recognized.
		/// </returns>
		/// <exception cref="System.IO.IOException"/>
		private bool handleBOM(int quad)
		{
			switch (quad)
			{
				case unchecked((int)(0x0000FEFF)):
				{
					/* Handling of (usually) optional BOM (required for
					* multi-byte formats); first 32-bit charsets:
					*/
					_bigEndian = true;
					_inputPtr += 4;
					_bytesPerChar = 4;
					return true;
				}

				case unchecked((int)(0xFFFE0000)):
				{
					// UCS-4, LE?
					_inputPtr += 4;
					_bytesPerChar = 4;
					_bigEndian = false;
					return true;
				}

				case unchecked((int)(0x0000FFFE)):
				{
					// UCS-4, in-order...
					reportWeirdUCS4("2143");
					goto case unchecked((int)(0xFEFF0000));
				}

				case unchecked((int)(0xFEFF0000)):
				{
					// throws exception
					// UCS-4, in-order...
					reportWeirdUCS4("3412");
					break;
				}
			}
			// throws exception
			// Ok, if not, how about 16-bit encoding BOMs?
			int msw = (int)(((uint)quad) >> 16);
			if (msw == unchecked((int)(0xFEFF)))
			{
				// UTF-16, BE
				_inputPtr += 2;
				_bytesPerChar = 2;
				_bigEndian = true;
				return true;
			}
			if (msw == unchecked((int)(0xFFFE)))
			{
				// UTF-16, LE
				_inputPtr += 2;
				_bytesPerChar = 2;
				_bigEndian = false;
				return true;
			}
			// And if not, then UTF-8 BOM?
			if (((int)(((uint)quad) >> 8)) == unchecked((int)(0xEFBBBF)))
			{
				// UTF-8
				_inputPtr += 3;
				_bytesPerChar = 1;
				_bigEndian = true;
				// doesn't really matter
				return true;
			}
			return false;
		}

		/// <exception cref="System.IO.IOException"/>
		private bool checkUTF32(int quad)
		{
			/* Handling of (usually) optional BOM (required for
			* multi-byte formats); first 32-bit charsets:
			*/
			if ((quad >> 8) == 0)
			{
				// 0x000000?? -> UTF32-BE
				_bigEndian = true;
			}
			else
			{
				if ((quad & unchecked((int)(0x00FFFFFF))) == 0)
				{
					// 0x??000000 -> UTF32-LE
					_bigEndian = false;
				}
				else
				{
					if ((quad & ~unchecked((int)(0x00FF0000))) == 0)
					{
						// 0x00??0000 -> UTF32-in-order
						reportWeirdUCS4("3412");
					}
					else
					{
						if ((quad & ~unchecked((int)(0x0000FF00))) == 0)
						{
							// 0x0000??00 -> UTF32-in-order
							reportWeirdUCS4("2143");
						}
						else
						{
							// Can not be valid UTF-32 encoded JSON...
							return false;
						}
					}
				}
			}
			// Not BOM (just regular content), nothing to skip past:
			//_inputPtr += 4;
			_bytesPerChar = 4;
			return true;
		}

		private bool checkUTF16(int i16)
		{
			if ((i16 & unchecked((int)(0xFF00))) == 0)
			{
				// UTF-16BE
				_bigEndian = true;
			}
			else
			{
				if ((i16 & unchecked((int)(0x00FF))) == 0)
				{
					// UTF-16LE
					_bigEndian = false;
				}
				else
				{
					// nope, not  UTF-16
					return false;
				}
			}
			// Not BOM (just regular content), nothing to skip past:
			//_inputPtr += 2;
			_bytesPerChar = 2;
			return true;
		}

		/*
		/**********************************************************
		/* Internal methods, problem reporting
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		private void reportWeirdUCS4(string type)
		{
			throw new System.IO.IOException("Unsupported UCS-4 endianness (" + type + ") detected"
				);
		}

		/*
		/**********************************************************
		/* Internal methods, raw input access
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		protected internal bool ensureLoaded(int minimum)
		{
			/* Let's assume here buffer has enough room -- this will always
			* be true for the limited used this method gets
			*/
			int gotten = (_inputEnd - _inputPtr);
			while (gotten < minimum)
			{
				int count;
				if (_in == null)
				{
					// block source
					count = -1;
				}
				else
				{
					count = _in.read(_inputBuffer, _inputEnd, _inputBuffer.Length - _inputEnd);
				}
				if (count < 1)
				{
					return false;
				}
				_inputEnd += count;
				gotten += count;
			}
			return true;
		}
	}
}
