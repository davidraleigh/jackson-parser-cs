using Sharpen;

namespace com.fasterxml.jackson.core.json
{
	/// <summary>
	/// This is a concrete implementation of
	/// <see cref="com.fasterxml.jackson.core.JsonParser"/>
	/// , which is
	/// based on a
	/// <see cref="System.IO.StreamReader"/>
	/// to handle low-level character
	/// conversion tasks.
	/// </summary>
	public class ReaderBasedJsonParser : com.fasterxml.jackson.core.@base.ParserBase
	{
		protected internal static readonly int[] _icLatin1 = com.fasterxml.jackson.core.io.CharTypes
			.getInputCodeLatin1();

		/// <summary>
		/// Reader that can be used for reading more content, if one
		/// buffer from input source, but in some cases pre-loaded buffer
		/// is handed to the parser.
		/// </summary>
		protected internal System.IO.StreamReader _reader;

		/// <summary>
		/// Current buffer from which data is read; generally data is read into
		/// buffer from input source.
		/// </summary>
		protected internal char[] _inputBuffer;

		/// <summary>
		/// Flag that indicates whether the input buffer is recycable (and
		/// needs to be returned to recycler once we are done) or not.
		/// </summary>
		/// <remarks>
		/// Flag that indicates whether the input buffer is recycable (and
		/// needs to be returned to recycler once we are done) or not.
		/// <p>
		/// If it is not, it also means that parser can NOT modify underlying
		/// buffer.
		/// </remarks>
		protected internal bool _bufferRecyclable;

		protected internal com.fasterxml.jackson.core.ObjectCodec _objectCodec;

		protected internal readonly com.fasterxml.jackson.core.sym.CharsToNameCanonicalizer
			 _symbols;

		protected internal readonly int _hashSeed;

		/// <summary>
		/// Flag that indicates that the current token has not yet
		/// been fully processed, and needs to be finished for
		/// some access (or skipped to obtain the next token)
		/// </summary>
		protected internal bool _tokenIncomplete = false;

		/// <summary>
		/// Method called when caller wants to provide input buffer directly,
		/// and it may or may not be recyclable use standard recycle context.
		/// </summary>
		/// <since>2.4</since>
		public ReaderBasedJsonParser(com.fasterxml.jackson.core.io.IOContext ctxt, int features
			, System.IO.StreamReader r, com.fasterxml.jackson.core.ObjectCodec codec, com.fasterxml.jackson.core.sym.CharsToNameCanonicalizer
			 st, char[] inputBuffer, int start, int end, bool bufferRecyclable)
			: base(ctxt, features)
		{
			// final in 2.3, earlier
			// Latin1 encoding is not supported, but we do use 8-bit subset for
			// pre-processing task, to simplify first pass, keep it fast.
			/*
			/**********************************************************
			/* Input configuration
			/**********************************************************
			*/
			/*
			/**********************************************************
			/* Configuration
			/**********************************************************
			*/
			/*
			/**********************************************************
			/* Parsing state
			/**********************************************************
			*/
			/*
			/**********************************************************
			/* Life-cycle
			/**********************************************************
			*/
			_reader = r;
			_inputBuffer = inputBuffer;
			_inputPtr = start;
			_inputEnd = end;
			_objectCodec = codec;
			_symbols = st;
			_hashSeed = st.hashSeed();
			_bufferRecyclable = bufferRecyclable;
		}

		/// <summary>
		/// Method called when input comes as a
		/// <see cref="System.IO.StreamReader"/>
		/// , and buffer allocation
		/// can be done using default mechanism.
		/// </summary>
		public ReaderBasedJsonParser(com.fasterxml.jackson.core.io.IOContext ctxt, int features
			, System.IO.StreamReader r, com.fasterxml.jackson.core.ObjectCodec codec, com.fasterxml.jackson.core.sym.CharsToNameCanonicalizer
			 st)
			: base(ctxt, features)
		{
			_reader = r;
			_inputBuffer = ctxt.allocTokenBuffer();
			_inputPtr = 0;
			_inputEnd = 0;
			_objectCodec = codec;
			_symbols = st;
			_hashSeed = st.hashSeed();
			_bufferRecyclable = true;
		}

		/*
		/**********************************************************
		/* Base method defs, overrides
		/**********************************************************
		*/
		public override com.fasterxml.jackson.core.ObjectCodec getCodec()
		{
			return _objectCodec;
		}

		public override void setCodec(com.fasterxml.jackson.core.ObjectCodec c)
		{
			_objectCodec = c;
		}

		/// <exception cref="System.IO.IOException"/>
		public override int releaseBuffered(System.IO.TextWriter w)
		{
			int count = _inputEnd - _inputPtr;
			if (count < 1)
			{
				return 0;
			}
			// let's just advance ptr to end
			int origPtr = _inputPtr;
			w.write(_inputBuffer, origPtr, count);
			return count;
		}

		public override object getInputSource()
		{
			return _reader;
		}

		/// <exception cref="System.IO.IOException"/>
		protected internal override bool loadMore()
		{
			_currInputProcessed += _inputEnd;
			_currInputRowStart -= _inputEnd;
			if (_reader != null)
			{
				int count = _reader.read(_inputBuffer, 0, _inputBuffer.Length);
				if (count > 0)
				{
					_inputPtr = 0;
					_inputEnd = count;
					return true;
				}
				// End of input
				_closeInput();
				// Should never return 0, so let's fail
				if (count == 0)
				{
					throw new System.IO.IOException("Reader returned 0 characters when trying to read "
						 + _inputEnd);
				}
			}
			return false;
		}

		/// <exception cref="System.IO.IOException"/>
		protected internal virtual char getNextChar(string eofMsg)
		{
			if (_inputPtr >= _inputEnd)
			{
				if (!loadMore())
				{
					_reportInvalidEOF(eofMsg);
				}
			}
			return _inputBuffer[_inputPtr++];
		}

		/// <exception cref="System.IO.IOException"/>
		protected internal override void _closeInput()
		{
			/* 25-Nov-2008, tatus: As per [JACKSON-16] we are not to call close()
			*   on the underlying Reader, unless we "own" it, or auto-closing
			*   feature is enabled.
			*   One downside is that when using our optimized
			*   Reader (granted, we only do that for UTF-32...) this
			*   means that buffer recycling won't work correctly.
			*/
			if (_reader != null)
			{
				if (_ioContext.isResourceManaged() || isEnabled(com.fasterxml.jackson.core.JsonParser.Feature
					.AUTO_CLOSE_SOURCE))
				{
					_reader.close();
				}
				_reader = null;
			}
		}

		/// <summary>
		/// Method called to release internal buffers owned by the base
		/// reader.
		/// </summary>
		/// <remarks>
		/// Method called to release internal buffers owned by the base
		/// reader. This may be called along with
		/// <see cref="_closeInput()"/>
		/// (for
		/// example, when explicitly closing this reader instance), or
		/// separately (if need be).
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		protected internal override void _releaseBuffers()
		{
			base._releaseBuffers();
			// merge new symbols, if any
			_symbols.release();
			// and release buffers, if they are recyclable ones
			if (_bufferRecyclable)
			{
				char[] buf = _inputBuffer;
				if (buf != null)
				{
					_inputBuffer = null;
					_ioContext.releaseTokenBuffer(buf);
				}
			}
		}

		/*
		/**********************************************************
		/* Public API, data access
		/**********************************************************
		*/
		/// <summary>
		/// Method for accessing textual representation of the current event;
		/// if no current event (before first call to
		/// <see cref="nextToken()"/>
		/// , or
		/// after encountering end-of-input), returns null.
		/// Method can be called for any event.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		public sealed override string getText()
		{
			com.fasterxml.jackson.core.JsonToken t = _currToken;
			if (t == com.fasterxml.jackson.core.JsonToken.VALUE_STRING)
			{
				if (_tokenIncomplete)
				{
					_tokenIncomplete = false;
					_finishString();
				}
				// only strings can be incomplete
				return _textBuffer.contentsAsString();
			}
			return _getText2(t);
		}

		// // // Let's override default impls for improved performance
		// @since 2.1
		/// <exception cref="System.IO.IOException"/>
		public sealed override string getValueAsString()
		{
			if (_currToken == com.fasterxml.jackson.core.JsonToken.VALUE_STRING)
			{
				if (_tokenIncomplete)
				{
					_tokenIncomplete = false;
					_finishString();
				}
				// only strings can be incomplete
				return _textBuffer.contentsAsString();
			}
			if (_currToken == com.fasterxml.jackson.core.JsonToken.FIELD_NAME)
			{
				return getCurrentName();
			}
			return base.getValueAsString(null);
		}

		// @since 2.1
		/// <exception cref="System.IO.IOException"/>
		public sealed override string getValueAsString(string defValue)
		{
			if (_currToken == com.fasterxml.jackson.core.JsonToken.VALUE_STRING)
			{
				if (_tokenIncomplete)
				{
					_tokenIncomplete = false;
					_finishString();
				}
				// only strings can be incomplete
				return _textBuffer.contentsAsString();
			}
			if (_currToken == com.fasterxml.jackson.core.JsonToken.FIELD_NAME)
			{
				return getCurrentName();
			}
			return base.getValueAsString(defValue);
		}

		protected internal string _getText2(com.fasterxml.jackson.core.JsonToken t)
		{
			if (t == null)
			{
				return null;
			}
			switch (t.id())
			{
				case JsonTokenIdConstants.ID_FIELD_NAME:
				{
					return _parsingContext.getCurrentName();
				}

				case JsonTokenIdConstants.ID_STRING:
				case JsonTokenIdConstants.ID_NUMBER_INT:
				case JsonTokenIdConstants.ID_NUMBER_FLOAT:
				{
					// fall through
					return _textBuffer.contentsAsString();
				}

				default:
				{
					return t.asString();
				}
			}
		}

		/// <exception cref="System.IO.IOException"/>
		public sealed override char[] getTextCharacters()
		{
			if (_currToken != null)
			{
				switch (_currToken.id())
				{
					case JsonTokenIdConstants.ID_FIELD_NAME:
					{
						// null only before/after document
						if (!_nameCopied)
						{
							string name = _parsingContext.getCurrentName();
							int nameLen = name.Length;
							if (_nameCopyBuffer == null)
							{
								_nameCopyBuffer = _ioContext.allocNameCopyBuffer(nameLen);
							}
							else
							{
								if (_nameCopyBuffer.Length < nameLen)
								{
									_nameCopyBuffer = new char[nameLen];
								}
							}
							Sharpen.Runtime.getCharsForString(name, 0, nameLen, _nameCopyBuffer, 0);
							_nameCopied = true;
						}
						return _nameCopyBuffer;
					}

					case JsonTokenIdConstants.ID_STRING:
					{
						if (_tokenIncomplete)
						{
							_tokenIncomplete = false;
							_finishString();
						}
						goto case JsonTokenIdConstants.ID_NUMBER_INT;
					}

					case JsonTokenIdConstants.ID_NUMBER_INT:
					case JsonTokenIdConstants.ID_NUMBER_FLOAT:
					{
						// only strings can be incomplete
						// fall through
						return _textBuffer.getTextBuffer();
					}

					default:
					{
						return _currToken.asCharArray();
					}
				}
			}
			return null;
		}

		/// <exception cref="System.IO.IOException"/>
		public sealed override int getTextLength()
		{
			if (_currToken != null)
			{
				switch (_currToken.id())
				{
					case JsonTokenIdConstants.ID_FIELD_NAME:
					{
						// null only before/after document
						return _parsingContext.getCurrentName().Length;
					}

					case JsonTokenIdConstants.ID_STRING:
					{
						if (_tokenIncomplete)
						{
							_tokenIncomplete = false;
							_finishString();
						}
						goto case JsonTokenIdConstants.ID_NUMBER_INT;
					}

					case JsonTokenIdConstants.ID_NUMBER_INT:
					case JsonTokenIdConstants.ID_NUMBER_FLOAT:
					{
						// only strings can be incomplete
						// fall through
						return _textBuffer.size();
					}

					default:
					{
						return _currToken.asCharArray().Length;
					}
				}
			}
			return 0;
		}

		/// <exception cref="System.IO.IOException"/>
		public sealed override int getTextOffset()
		{
			// Most have offset of 0, only some may have other values:
			if (_currToken != null)
			{
				switch (_currToken.id())
				{
					case JsonTokenIdConstants.ID_FIELD_NAME:
					{
						return 0;
					}

					case JsonTokenIdConstants.ID_STRING:
					{
						if (_tokenIncomplete)
						{
							_tokenIncomplete = false;
							_finishString();
						}
						goto case JsonTokenIdConstants.ID_NUMBER_INT;
					}

					case JsonTokenIdConstants.ID_NUMBER_INT:
					case JsonTokenIdConstants.ID_NUMBER_FLOAT:
					{
						// only strings can be incomplete
						// fall through
						return _textBuffer.getTextOffset();
					}

					default:
					{
						break;
					}
				}
			}
			return 0;
		}

		/// <exception cref="System.IO.IOException"/>
		public override byte[] getBinaryValue(com.fasterxml.jackson.core.Base64Variant b64variant
			)
		{
			if (_currToken != com.fasterxml.jackson.core.JsonToken.VALUE_STRING && (_currToken
				 != com.fasterxml.jackson.core.JsonToken.VALUE_EMBEDDED_OBJECT || _binaryValue ==
				 null))
			{
				_reportError("Current token (" + _currToken + ") not VALUE_STRING or VALUE_EMBEDDED_OBJECT, can not access as binary"
					);
			}
			/* To ensure that we won't see inconsistent data, better clear up
			* state...
			*/
			if (_tokenIncomplete)
			{
				try
				{
					_binaryValue = _decodeBase64(b64variant);
				}
				catch (System.ArgumentException iae)
				{
					throw _constructError("Failed to decode VALUE_STRING as base64 (" + b64variant + 
						"): " + iae.Message);
				}
				/* let's clear incomplete only now; allows for accessing other
				* textual content in error cases
				*/
				_tokenIncomplete = false;
			}
			else
			{
				// may actually require conversion...
				if (_binaryValue == null)
				{
					com.fasterxml.jackson.core.util.ByteArrayBuilder builder = _getByteArrayBuilder();
					_decodeBase64(getText(), builder, b64variant);
					_binaryValue = builder.toByteArray();
				}
			}
			return _binaryValue;
		}

		/// <exception cref="System.IO.IOException"/>
		public override int readBinaryValue(com.fasterxml.jackson.core.Base64Variant b64variant
			, Sharpen.OutputStream @out)
		{
			// if we have already read the token, just use whatever we may have
			if (!_tokenIncomplete || _currToken != com.fasterxml.jackson.core.JsonToken.VALUE_STRING)
			{
				byte[] b = getBinaryValue(b64variant);
				@out.write(b);
				return b.Length;
			}
			// otherwise do "real" incremental parsing...
			byte[] buf = _ioContext.allocBase64Buffer();
			try
			{
				return _readBinary(b64variant, @out, buf);
			}
			finally
			{
				_ioContext.releaseBase64Buffer(buf);
			}
		}

		/// <exception cref="System.IO.IOException"/>
		protected internal virtual int _readBinary(com.fasterxml.jackson.core.Base64Variant
			 b64variant, Sharpen.OutputStream @out, byte[] buffer)
		{
			int outputPtr = 0;
			int outputEnd = buffer.Length - 3;
			int outputCount = 0;
			while (true)
			{
				// first, we'll skip preceding white space, if any
				char ch;
				do
				{
					if (_inputPtr >= _inputEnd)
					{
						loadMoreGuaranteed();
					}
					ch = _inputBuffer[_inputPtr++];
				}
				while (ch <= INT_SPACE);
				int bits = b64variant.decodeBase64Char(ch);
				if (bits < 0)
				{
					// reached the end, fair and square?
					if (ch == '"')
					{
						break;
					}
					bits = _decodeBase64Escape(b64variant, ch, 0);
					if (bits < 0)
					{
						// white space to skip
						continue;
					}
				}
				// enough room? If not, flush
				if (outputPtr > outputEnd)
				{
					outputCount += outputPtr;
					@out.write(buffer, 0, outputPtr);
					outputPtr = 0;
				}
				int decodedData = bits;
				// then second base64 char; can't get padding yet, nor ws
				if (_inputPtr >= _inputEnd)
				{
					loadMoreGuaranteed();
				}
				ch = _inputBuffer[_inputPtr++];
				bits = b64variant.decodeBase64Char(ch);
				if (bits < 0)
				{
					bits = _decodeBase64Escape(b64variant, ch, 1);
				}
				decodedData = (decodedData << 6) | bits;
				// third base64 char; can be padding, but not ws
				if (_inputPtr >= _inputEnd)
				{
					loadMoreGuaranteed();
				}
				ch = _inputBuffer[_inputPtr++];
				bits = b64variant.decodeBase64Char(ch);
				// First branch: can get padding (-> 1 byte)
				if (bits < 0)
				{
					if (bits != com.fasterxml.jackson.core.Base64Variant.BASE64_VALUE_PADDING)
					{
						// as per [JACKSON-631], could also just be 'missing'  padding
						if (ch == '"' && !b64variant.usesPadding())
						{
							decodedData >>= 4;
							buffer[outputPtr++] = unchecked((byte)decodedData);
							break;
						}
						bits = _decodeBase64Escape(b64variant, ch, 2);
					}
					if (bits == com.fasterxml.jackson.core.Base64Variant.BASE64_VALUE_PADDING)
					{
						// Ok, must get padding
						if (_inputPtr >= _inputEnd)
						{
							loadMoreGuaranteed();
						}
						ch = _inputBuffer[_inputPtr++];
						if (!b64variant.usesPaddingChar(ch))
						{
							throw reportInvalidBase64Char(b64variant, ch, 3, "expected padding character '" +
								 b64variant.getPaddingChar() + "'");
						}
						// Got 12 bits, only need 8, need to shift
						decodedData >>= 4;
						buffer[outputPtr++] = unchecked((byte)decodedData);
						continue;
					}
				}
				// Nope, 2 or 3 bytes
				decodedData = (decodedData << 6) | bits;
				// fourth and last base64 char; can be padding, but not ws
				if (_inputPtr >= _inputEnd)
				{
					loadMoreGuaranteed();
				}
				ch = _inputBuffer[_inputPtr++];
				bits = b64variant.decodeBase64Char(ch);
				if (bits < 0)
				{
					if (bits != com.fasterxml.jackson.core.Base64Variant.BASE64_VALUE_PADDING)
					{
						// as per [JACKSON-631], could also just be 'missing'  padding
						if (ch == '"' && !b64variant.usesPadding())
						{
							decodedData >>= 2;
							buffer[outputPtr++] = unchecked((byte)(decodedData >> 8));
							buffer[outputPtr++] = unchecked((byte)decodedData);
							break;
						}
						bits = _decodeBase64Escape(b64variant, ch, 3);
					}
					if (bits == com.fasterxml.jackson.core.Base64Variant.BASE64_VALUE_PADDING)
					{
						/* With padding we only get 2 bytes; but we have
						* to shift it a bit so it is identical to triplet
						* case with partial output.
						* 3 chars gives 3x6 == 18 bits, of which 2 are
						* dummies, need to discard:
						*/
						decodedData >>= 2;
						buffer[outputPtr++] = unchecked((byte)(decodedData >> 8));
						buffer[outputPtr++] = unchecked((byte)decodedData);
						continue;
					}
				}
				// otherwise, our triplet is now complete
				decodedData = (decodedData << 6) | bits;
				buffer[outputPtr++] = unchecked((byte)(decodedData >> 16));
				buffer[outputPtr++] = unchecked((byte)(decodedData >> 8));
				buffer[outputPtr++] = unchecked((byte)decodedData);
			}
			_tokenIncomplete = false;
			if (outputPtr > 0)
			{
				outputCount += outputPtr;
				@out.write(buffer, 0, outputPtr);
			}
			return outputCount;
		}

		/*
		/**********************************************************
		/* Public API, traversal
		/**********************************************************
		*/
		/// <returns>
		/// Next token from the stream, if any found, or null
		/// to indicate end-of-input
		/// </returns>
		/// <exception cref="System.IO.IOException"/>
		public sealed override com.fasterxml.jackson.core.JsonToken nextToken()
		{
			_numTypesValid = NR_UNKNOWN;
			/* First: field names are special -- we will always tokenize
			* (part of) value along with field name to simplify
			* state handling. If so, can and need to use secondary token:
			*/
			if (_currToken == com.fasterxml.jackson.core.JsonToken.FIELD_NAME)
			{
				return _nextAfterName();
			}
			if (_tokenIncomplete)
			{
				_skipString();
			}
			// only strings can be partial
			int i = _skipWSOrEnd();
			if (i < 0)
			{
				// end-of-input
				/* 19-Feb-2009, tatu: Should actually close/release things
				*    like input source, symbol table and recyclable buffers now.
				*/
				close();
				return (_currToken = null);
			}
			/* First, need to ensure we know the starting location of token
			* after skipping leading white space
			*/
			_tokenInputTotal = _currInputProcessed + _inputPtr - 1;
			_tokenInputRow = _currInputRow;
			_tokenInputCol = _inputPtr - _currInputRowStart - 1;
			// finally: clear any data retained so far
			_binaryValue = null;
			// Closing scope?
			if (i == INT_RBRACKET)
			{
				if (!_parsingContext.inArray())
				{
					_reportMismatchedEndMarker(i, '}');
				}
				_parsingContext = ((com.fasterxml.jackson.core.json.JsonReadContext)_parsingContext
					.getParent());
				return (_currToken = com.fasterxml.jackson.core.JsonToken.END_ARRAY);
			}
			if (i == INT_RCURLY)
			{
				if (!_parsingContext.inObject())
				{
					_reportMismatchedEndMarker(i, ']');
				}
				_parsingContext = ((com.fasterxml.jackson.core.json.JsonReadContext)_parsingContext
					.getParent());
				return (_currToken = com.fasterxml.jackson.core.JsonToken.END_OBJECT);
			}
			// Nope: do we then expect a comma?
			if (_parsingContext.expectComma())
			{
				i = _skipComma(i);
			}
			/* And should we now have a name? Always true for
			* Object contexts, since the intermediate 'expect-value'
			* state is never retained.
			*/
			bool inObject = _parsingContext.inObject();
			if (inObject)
			{
				// First, field name itself:
				string name = (i == INT_QUOTE) ? _parseName() : _handleOddName(i);
				_parsingContext.setCurrentName(name);
				_currToken = com.fasterxml.jackson.core.JsonToken.FIELD_NAME;
				i = _skipColon();
			}
			// Ok: we must have a value... what is it?
			com.fasterxml.jackson.core.JsonToken t;
			switch (i)
			{
				case '"':
				{
					_tokenIncomplete = true;
					t = com.fasterxml.jackson.core.JsonToken.VALUE_STRING;
					break;
				}

				case '[':
				{
					if (!inObject)
					{
						_parsingContext = _parsingContext.createChildArrayContext(_tokenInputRow, _tokenInputCol
							);
					}
					t = com.fasterxml.jackson.core.JsonToken.START_ARRAY;
					break;
				}

				case '{':
				{
					if (!inObject)
					{
						_parsingContext = _parsingContext.createChildObjectContext(_tokenInputRow, _tokenInputCol
							);
					}
					t = com.fasterxml.jackson.core.JsonToken.START_OBJECT;
					break;
				}

				case ']':
				case '}':
				{
					// Error: neither is valid at this point; valid closers have
					// been handled earlier
					_reportUnexpectedChar(i, "expected a value");
					goto case 't';
				}

				case 't':
				{
					_matchTrue();
					t = com.fasterxml.jackson.core.JsonToken.VALUE_TRUE;
					break;
				}

				case 'f':
				{
					_matchFalse();
					t = com.fasterxml.jackson.core.JsonToken.VALUE_FALSE;
					break;
				}

				case 'n':
				{
					_matchNull();
					t = com.fasterxml.jackson.core.JsonToken.VALUE_NULL;
					break;
				}

				case '-':
				{
					/* Should we have separate handling for plus? Although
					* it is not allowed per se, it may be erroneously used,
					* and could be indicate by a more specific error message.
					*/
					t = _parseNegNumber();
					break;
				}

				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
				{
					t = _parsePosNumber(i);
					break;
				}

				default:
				{
					t = _handleOddValue(i);
					break;
				}
			}
			if (inObject)
			{
				_nextToken = t;
				return _currToken;
			}
			_currToken = t;
			return t;
		}

		private com.fasterxml.jackson.core.JsonToken _nextAfterName()
		{
			_nameCopied = false;
			// need to invalidate if it was copied
			com.fasterxml.jackson.core.JsonToken t = _nextToken;
			_nextToken = null;
			// Also: may need to start new context?
			if (t == com.fasterxml.jackson.core.JsonToken.START_ARRAY)
			{
				_parsingContext = _parsingContext.createChildArrayContext(_tokenInputRow, _tokenInputCol
					);
			}
			else
			{
				if (t == com.fasterxml.jackson.core.JsonToken.START_OBJECT)
				{
					_parsingContext = _parsingContext.createChildObjectContext(_tokenInputRow, _tokenInputCol
						);
				}
			}
			return (_currToken = t);
		}

		/*
		/**********************************************************
		/* Public API, nextXxx() overrides
		/**********************************************************
		*/
		/*
		@Override
		public boolean nextFieldName(SerializableString str)
		throws IOException
		{
		
		}
		*/
		/// <exception cref="System.IO.IOException"/>
		public override string nextFieldName()
		{
			// // // Note: this is almost a verbatim copy of nextToken() (minus comments)
			_numTypesValid = NR_UNKNOWN;
			if (_currToken == com.fasterxml.jackson.core.JsonToken.FIELD_NAME)
			{
				_nextAfterName();
				return null;
			}
			if (_tokenIncomplete)
			{
				_skipString();
			}
			int i = _skipWSOrEnd();
			if (i < 0)
			{
				close();
				_currToken = null;
				return null;
			}
			_tokenInputTotal = _currInputProcessed + _inputPtr - 1;
			_tokenInputRow = _currInputRow;
			_tokenInputCol = _inputPtr - _currInputRowStart - 1;
			_binaryValue = null;
			if (i == INT_RBRACKET)
			{
				if (!_parsingContext.inArray())
				{
					_reportMismatchedEndMarker(i, '}');
				}
				_parsingContext = ((com.fasterxml.jackson.core.json.JsonReadContext)_parsingContext
					.getParent());
				_currToken = com.fasterxml.jackson.core.JsonToken.END_ARRAY;
				return null;
			}
			if (i == INT_RCURLY)
			{
				if (!_parsingContext.inObject())
				{
					_reportMismatchedEndMarker(i, ']');
				}
				_parsingContext = ((com.fasterxml.jackson.core.json.JsonReadContext)_parsingContext
					.getParent());
				_currToken = com.fasterxml.jackson.core.JsonToken.END_OBJECT;
				return null;
			}
			if (_parsingContext.expectComma())
			{
				i = _skipComma(i);
			}
			if (!_parsingContext.inObject())
			{
				_nextTokenNotInObject(i);
				return null;
			}
			string name = (i == INT_QUOTE) ? _parseName() : _handleOddName(i);
			_parsingContext.setCurrentName(name);
			_currToken = com.fasterxml.jackson.core.JsonToken.FIELD_NAME;
			i = _skipColon();
			if (i == INT_QUOTE)
			{
				_tokenIncomplete = true;
				_nextToken = com.fasterxml.jackson.core.JsonToken.VALUE_STRING;
				return name;
			}
			// Ok: we must have a value... what is it?
			com.fasterxml.jackson.core.JsonToken t;
			switch (i)
			{
				case '-':
				{
					t = _parseNegNumber();
					break;
				}

				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
				{
					t = _parsePosNumber(i);
					break;
				}

				case 'f':
				{
					_matchFalse();
					t = com.fasterxml.jackson.core.JsonToken.VALUE_FALSE;
					break;
				}

				case 'n':
				{
					_matchNull();
					t = com.fasterxml.jackson.core.JsonToken.VALUE_NULL;
					break;
				}

				case 't':
				{
					_matchTrue();
					t = com.fasterxml.jackson.core.JsonToken.VALUE_TRUE;
					break;
				}

				case '[':
				{
					t = com.fasterxml.jackson.core.JsonToken.START_ARRAY;
					break;
				}

				case '{':
				{
					t = com.fasterxml.jackson.core.JsonToken.START_OBJECT;
					break;
				}

				default:
				{
					t = _handleOddValue(i);
					break;
				}
			}
			_nextToken = t;
			return name;
		}

		/// <exception cref="System.IO.IOException"/>
		private com.fasterxml.jackson.core.JsonToken _nextTokenNotInObject(int i)
		{
			if (i == INT_QUOTE)
			{
				_tokenIncomplete = true;
				return (_currToken = com.fasterxml.jackson.core.JsonToken.VALUE_STRING);
			}
			switch (i)
			{
				case '[':
				{
					_parsingContext = _parsingContext.createChildArrayContext(_tokenInputRow, _tokenInputCol
						);
					return (_currToken = com.fasterxml.jackson.core.JsonToken.START_ARRAY);
				}

				case '{':
				{
					_parsingContext = _parsingContext.createChildObjectContext(_tokenInputRow, _tokenInputCol
						);
					return (_currToken = com.fasterxml.jackson.core.JsonToken.START_OBJECT);
				}

				case 't':
				{
					_matchToken("true", 1);
					return (_currToken = com.fasterxml.jackson.core.JsonToken.VALUE_TRUE);
				}

				case 'f':
				{
					_matchToken("false", 1);
					return (_currToken = com.fasterxml.jackson.core.JsonToken.VALUE_FALSE);
				}

				case 'n':
				{
					_matchToken("null", 1);
					return (_currToken = com.fasterxml.jackson.core.JsonToken.VALUE_NULL);
				}

				case '-':
				{
					return (_currToken = _parseNegNumber());
				}

				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
				{
					/* Should we have separate handling for plus? Although
					* it is not allowed per se, it may be erroneously used,
					* and could be indicated by a more specific error message.
					*/
					return (_currToken = _parsePosNumber(i));
				}
			}
			return (_currToken = _handleOddValue(i));
		}

		// note: identical to one in UTF8StreamJsonParser
		/// <exception cref="System.IO.IOException"/>
		public sealed override string nextTextValue()
		{
			if (_currToken == com.fasterxml.jackson.core.JsonToken.FIELD_NAME)
			{
				// mostly copied from '_nextAfterName'
				_nameCopied = false;
				com.fasterxml.jackson.core.JsonToken t = _nextToken;
				_nextToken = null;
				_currToken = t;
				if (t == com.fasterxml.jackson.core.JsonToken.VALUE_STRING)
				{
					if (_tokenIncomplete)
					{
						_tokenIncomplete = false;
						_finishString();
					}
					return _textBuffer.contentsAsString();
				}
				if (t == com.fasterxml.jackson.core.JsonToken.START_ARRAY)
				{
					_parsingContext = _parsingContext.createChildArrayContext(_tokenInputRow, _tokenInputCol
						);
				}
				else
				{
					if (t == com.fasterxml.jackson.core.JsonToken.START_OBJECT)
					{
						_parsingContext = _parsingContext.createChildObjectContext(_tokenInputRow, _tokenInputCol
							);
					}
				}
				return null;
			}
			// !!! TODO: optimize this case as well
			return (nextToken() == com.fasterxml.jackson.core.JsonToken.VALUE_STRING) ? getText
				() : null;
		}

		// note: identical to one in Utf8StreamParser
		/// <exception cref="System.IO.IOException"/>
		public sealed override int nextIntValue(int defaultValue)
		{
			if (_currToken == com.fasterxml.jackson.core.JsonToken.FIELD_NAME)
			{
				_nameCopied = false;
				com.fasterxml.jackson.core.JsonToken t = _nextToken;
				_nextToken = null;
				_currToken = t;
				if (t == com.fasterxml.jackson.core.JsonToken.VALUE_NUMBER_INT)
				{
					return getIntValue();
				}
				if (t == com.fasterxml.jackson.core.JsonToken.START_ARRAY)
				{
					_parsingContext = _parsingContext.createChildArrayContext(_tokenInputRow, _tokenInputCol
						);
				}
				else
				{
					if (t == com.fasterxml.jackson.core.JsonToken.START_OBJECT)
					{
						_parsingContext = _parsingContext.createChildObjectContext(_tokenInputRow, _tokenInputCol
							);
					}
				}
				return defaultValue;
			}
			// !!! TODO: optimize this case as well
			return (nextToken() == com.fasterxml.jackson.core.JsonToken.VALUE_NUMBER_INT) ? getIntValue
				() : defaultValue;
		}

		// note: identical to one in Utf8StreamParser
		/// <exception cref="System.IO.IOException"/>
		public sealed override long nextLongValue(long defaultValue)
		{
			if (_currToken == com.fasterxml.jackson.core.JsonToken.FIELD_NAME)
			{
				// mostly copied from '_nextAfterName'
				_nameCopied = false;
				com.fasterxml.jackson.core.JsonToken t = _nextToken;
				_nextToken = null;
				_currToken = t;
				if (t == com.fasterxml.jackson.core.JsonToken.VALUE_NUMBER_INT)
				{
					return getLongValue();
				}
				if (t == com.fasterxml.jackson.core.JsonToken.START_ARRAY)
				{
					_parsingContext = _parsingContext.createChildArrayContext(_tokenInputRow, _tokenInputCol
						);
				}
				else
				{
					if (t == com.fasterxml.jackson.core.JsonToken.START_OBJECT)
					{
						_parsingContext = _parsingContext.createChildObjectContext(_tokenInputRow, _tokenInputCol
							);
					}
				}
				return defaultValue;
			}
			// !!! TODO: optimize this case as well
			return (nextToken() == com.fasterxml.jackson.core.JsonToken.VALUE_NUMBER_INT) ? getLongValue
				() : defaultValue;
		}

		// note: identical to one in UTF8StreamJsonParser
		/// <exception cref="System.IO.IOException"/>
		public sealed override bool nextBooleanValue()
		{
			if (_currToken == com.fasterxml.jackson.core.JsonToken.FIELD_NAME)
			{
				// mostly copied from '_nextAfterName'
				_nameCopied = false;
				com.fasterxml.jackson.core.JsonToken t = _nextToken;
				_nextToken = null;
				_currToken = t;
				if (t == com.fasterxml.jackson.core.JsonToken.VALUE_TRUE)
				{
					return true;
				}
				if (t == com.fasterxml.jackson.core.JsonToken.VALUE_FALSE)
				{
					return false;
				}
				if (t == com.fasterxml.jackson.core.JsonToken.START_ARRAY)
				{
					_parsingContext = _parsingContext.createChildArrayContext(_tokenInputRow, _tokenInputCol
						);
				}
				else
				{
					if (t == com.fasterxml.jackson.core.JsonToken.START_OBJECT)
					{
						_parsingContext = _parsingContext.createChildObjectContext(_tokenInputRow, _tokenInputCol
							);
					}
				}
				return null;
			}
			com.fasterxml.jackson.core.JsonToken t_1 = nextToken();
			if (t_1 != null)
			{
				int id = t_1.id();
				if (id == JsonTokenIdConstants.ID_TRUE)
				{
					return true;
				}
				if (id == JsonTokenIdConstants.ID_FALSE)
				{
					return false;
				}
			}
			return null;
		}

		/*
		/**********************************************************
		/* Internal methods, number parsing
		/**********************************************************
		*/
		/// <summary>Initial parsing method for number values.</summary>
		/// <remarks>
		/// Initial parsing method for number values. It needs to be able
		/// to parse enough input to be able to determine whether the
		/// value is to be considered a simple integer value, or a more
		/// generic decimal value: latter of which needs to be expressed
		/// as a floating point number. The basic rule is that if the number
		/// has no fractional or exponential part, it is an integer; otherwise
		/// a floating point number.
		/// <p>
		/// Because much of input has to be processed in any case, no partial
		/// parsing is done: all input text will be stored for further
		/// processing. However, actual numeric value conversion will be
		/// deferred, since it is usually the most complicated and costliest
		/// part of processing.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		protected internal com.fasterxml.jackson.core.JsonToken _parsePosNumber(int ch)
		{
			/* Although we will always be complete with respect to textual
			* representation (that is, all characters will be parsed),
			* actual conversion to a number is deferred. Thus, need to
			* note that no representations are valid yet
			*/
			int ptr = _inputPtr;
			int startPtr = ptr - 1;
			// to include digit already read
			int inputLen = _inputEnd;
			// One special case, leading zero(es):
			if (ch == INT_0)
			{
				return _parseNumber2(false, startPtr);
			}
			/* First, let's see if the whole number is contained within
			* the input buffer unsplit. This should be the common case;
			* and to simplify processing, we will just reparse contents
			* in the alternative case (number split on buffer boundary)
			*/
			int intLen = 1;
			// already got one
			// First let's get the obligatory integer part:
			while (true)
			{
				if (ptr >= inputLen)
				{
					_inputPtr = startPtr;
					return _parseNumber2(false, startPtr);
				}
				ch = (int)_inputBuffer[ptr++];
				if (ch < INT_0 || ch > INT_9)
				{
					goto int_loop_break;
				}
				++intLen;
int_loop_continue: ;
			}
int_loop_break: ;
			if (ch == INT_PERIOD || ch == INT_e || ch == INT_E)
			{
				_inputPtr = ptr;
				return _parseFloat(ch, startPtr, ptr, false, intLen);
			}
			// Got it all: let's add to text buffer for parsing, access
			--ptr;
			// need to push back following separator
			_inputPtr = ptr;
			// As per #105, need separating space between root values; check here
			if (_parsingContext.inRoot())
			{
				_verifyRootSpace(ch);
			}
			int len = ptr - startPtr;
			_textBuffer.resetWithShared(_inputBuffer, startPtr, len);
			return resetInt(false, intLen);
		}

		/// <exception cref="System.IO.IOException"/>
		private com.fasterxml.jackson.core.JsonToken _parseFloat(int ch, int startPtr, int
			 ptr, bool neg, int intLen)
		{
			int inputLen = _inputEnd;
			int fractLen = 0;
			// And then see if we get other parts
			if (ch == '.')
			{
				// yes, fraction
				while (true)
				{
					if (ptr >= inputLen)
					{
						return _parseNumber2(neg, startPtr);
					}
					ch = (int)_inputBuffer[ptr++];
					if (ch < INT_0 || ch > INT_9)
					{
						goto fract_loop_break;
					}
					++fractLen;
fract_loop_continue: ;
				}
fract_loop_break: ;
				// must be followed by sequence of ints, one minimum
				if (fractLen == 0)
				{
					reportUnexpectedNumberChar(ch, "Decimal point not followed by a digit");
				}
			}
			int expLen = 0;
			if (ch == 'e' || ch == 'E')
			{
				// and/or exponent
				if (ptr >= inputLen)
				{
					_inputPtr = startPtr;
					return _parseNumber2(neg, startPtr);
				}
				// Sign indicator?
				ch = (int)_inputBuffer[ptr++];
				if (ch == INT_MINUS || ch == INT_PLUS)
				{
					// yup, skip for now
					if (ptr >= inputLen)
					{
						_inputPtr = startPtr;
						return _parseNumber2(neg, startPtr);
					}
					ch = (int)_inputBuffer[ptr++];
				}
				while (ch <= INT_9 && ch >= INT_0)
				{
					++expLen;
					if (ptr >= inputLen)
					{
						_inputPtr = startPtr;
						return _parseNumber2(neg, startPtr);
					}
					ch = (int)_inputBuffer[ptr++];
				}
				// must be followed by sequence of ints, one minimum
				if (expLen == 0)
				{
					reportUnexpectedNumberChar(ch, "Exponent indicator not followed by a digit");
				}
			}
			--ptr;
			// need to push back following separator
			_inputPtr = ptr;
			// As per #105, need separating space between root values; check here
			if (_parsingContext.inRoot())
			{
				_verifyRootSpace(ch);
			}
			int len = ptr - startPtr;
			_textBuffer.resetWithShared(_inputBuffer, startPtr, len);
			// And there we have it!
			return resetFloat(neg, intLen, fractLen, expLen);
		}

		/// <exception cref="System.IO.IOException"/>
		protected internal com.fasterxml.jackson.core.JsonToken _parseNegNumber()
		{
			int ptr = _inputPtr;
			int startPtr = ptr - 1;
			// to include sign/digit already read
			int inputLen = _inputEnd;
			if (ptr >= inputLen)
			{
				return _parseNumber2(true, startPtr);
			}
			int ch = _inputBuffer[ptr++];
			// First check: must have a digit to follow minus sign
			if (ch > INT_9 || ch < INT_0)
			{
				_inputPtr = ptr;
				return _handleInvalidNumberStart(ch, true);
			}
			// One special case, leading zero(es):
			if (ch == INT_0)
			{
				return _parseNumber2(true, startPtr);
			}
			int intLen = 1;
			// already got one
			// First let's get the obligatory integer part:
			while (true)
			{
				if (ptr >= inputLen)
				{
					return _parseNumber2(true, startPtr);
				}
				ch = (int)_inputBuffer[ptr++];
				if (ch < INT_0 || ch > INT_9)
				{
					goto int_loop_break;
				}
				++intLen;
int_loop_continue: ;
			}
int_loop_break: ;
			if (ch == INT_PERIOD || ch == INT_e || ch == INT_E)
			{
				_inputPtr = ptr;
				return _parseFloat(ch, startPtr, ptr, true, intLen);
			}
			--ptr;
			_inputPtr = ptr;
			if (_parsingContext.inRoot())
			{
				_verifyRootSpace(ch);
			}
			int len = ptr - startPtr;
			_textBuffer.resetWithShared(_inputBuffer, startPtr, len);
			return resetInt(true, intLen);
		}

		/// <summary>
		/// Method called to parse a number, when the primary parse
		/// method has failed to parse it, due to it being split on
		/// buffer boundary.
		/// </summary>
		/// <remarks>
		/// Method called to parse a number, when the primary parse
		/// method has failed to parse it, due to it being split on
		/// buffer boundary. As a result code is very similar, except
		/// that it has to explicitly copy contents to the text buffer
		/// instead of just sharing the main input buffer.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		private com.fasterxml.jackson.core.JsonToken _parseNumber2(bool neg, int startPtr
			)
		{
			_inputPtr = neg ? (startPtr + 1) : startPtr;
			char[] outBuf = _textBuffer.emptyAndGetCurrentSegment();
			int outPtr = 0;
			// Need to prepend sign?
			if (neg)
			{
				outBuf[outPtr++] = '-';
			}
			// This is the place to do leading-zero check(s) too:
			int intLen = 0;
			char c = (_inputPtr < _inputEnd) ? _inputBuffer[_inputPtr++] : getNextChar("No digit following minus sign"
				);
			if (c == '0')
			{
				c = _verifyNoLeadingZeroes();
			}
			bool eof = false;
			// Ok, first the obligatory integer part:
			while (c >= '0' && c <= '9')
			{
				++intLen;
				if (outPtr >= outBuf.Length)
				{
					outBuf = _textBuffer.finishCurrentSegment();
					outPtr = 0;
				}
				outBuf[outPtr++] = c;
				if (_inputPtr >= _inputEnd && !loadMore())
				{
					// EOF is legal for main level int values
					c = CHAR_NULL;
					eof = true;
					goto int_loop_break;
				}
				c = _inputBuffer[_inputPtr++];
int_loop_continue: ;
			}
int_loop_break: ;
			// Also, integer part is not optional
			if (intLen == 0)
			{
				return _handleInvalidNumberStart(c, neg);
			}
			int fractLen = 0;
			// And then see if we get other parts
			if (c == '.')
			{
				// yes, fraction
				outBuf[outPtr++] = c;
				while (true)
				{
					if (_inputPtr >= _inputEnd && !loadMore())
					{
						eof = true;
						goto fract_loop_break;
					}
					c = _inputBuffer[_inputPtr++];
					if (c < INT_0 || c > INT_9)
					{
						goto fract_loop_break;
					}
					++fractLen;
					if (outPtr >= outBuf.Length)
					{
						outBuf = _textBuffer.finishCurrentSegment();
						outPtr = 0;
					}
					outBuf[outPtr++] = c;
fract_loop_continue: ;
				}
fract_loop_break: ;
				// must be followed by sequence of ints, one minimum
				if (fractLen == 0)
				{
					reportUnexpectedNumberChar(c, "Decimal point not followed by a digit");
				}
			}
			int expLen = 0;
			if (c == 'e' || c == 'E')
			{
				// exponent?
				if (outPtr >= outBuf.Length)
				{
					outBuf = _textBuffer.finishCurrentSegment();
					outPtr = 0;
				}
				outBuf[outPtr++] = c;
				// Not optional, can require that we get one more char
				c = (_inputPtr < _inputEnd) ? _inputBuffer[_inputPtr++] : getNextChar("expected a digit for number exponent"
					);
				// Sign indicator?
				if (c == '-' || c == '+')
				{
					if (outPtr >= outBuf.Length)
					{
						outBuf = _textBuffer.finishCurrentSegment();
						outPtr = 0;
					}
					outBuf[outPtr++] = c;
					// Likewise, non optional:
					c = (_inputPtr < _inputEnd) ? _inputBuffer[_inputPtr++] : getNextChar("expected a digit for number exponent"
						);
				}
				while (c <= INT_9 && c >= INT_0)
				{
					++expLen;
					if (outPtr >= outBuf.Length)
					{
						outBuf = _textBuffer.finishCurrentSegment();
						outPtr = 0;
					}
					outBuf[outPtr++] = c;
					if (_inputPtr >= _inputEnd && !loadMore())
					{
						eof = true;
						goto exp_loop_break;
					}
					c = _inputBuffer[_inputPtr++];
exp_loop_continue: ;
				}
exp_loop_break: ;
				// must be followed by sequence of ints, one minimum
				if (expLen == 0)
				{
					reportUnexpectedNumberChar(c, "Exponent indicator not followed by a digit");
				}
			}
			// Ok; unless we hit end-of-input, need to push last char read back
			if (!eof)
			{
				--_inputPtr;
				if (_parsingContext.inRoot())
				{
					_verifyRootSpace(c);
				}
			}
			_textBuffer.setCurrentLength(outPtr);
			// And there we have it!
			return reset(neg, intLen, fractLen, expLen);
		}

		/// <summary>
		/// Method called when we have seen one zero, and want to ensure
		/// it is not followed by another
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		private char _verifyNoLeadingZeroes()
		{
			// Fast case first:
			if (_inputPtr < _inputEnd)
			{
				char ch = _inputBuffer[_inputPtr];
				// if not followed by a number (probably '.'); return zero as is, to be included
				if (ch < '0' || ch > '9')
				{
					return '0';
				}
			}
			// and offline the less common case
			return _verifyNLZ2();
		}

		/// <exception cref="System.IO.IOException"/>
		private char _verifyNLZ2()
		{
			if (_inputPtr >= _inputEnd && !loadMore())
			{
				return '0';
			}
			char ch = _inputBuffer[_inputPtr];
			if (ch < '0' || ch > '9')
			{
				return '0';
			}
			if (!isEnabled(com.fasterxml.jackson.core.JsonParser.Feature.ALLOW_NUMERIC_LEADING_ZEROS
				))
			{
				reportInvalidNumber("Leading zeroes not allowed");
			}
			// if so, just need to skip either all zeroes (if followed by number); or all but one (if non-number)
			++_inputPtr;
			// Leading zero to be skipped
			if (ch == INT_0)
			{
				while (_inputPtr < _inputEnd || loadMore())
				{
					ch = _inputBuffer[_inputPtr];
					if (ch < '0' || ch > '9')
					{
						// followed by non-number; retain one zero
						return '0';
					}
					++_inputPtr;
					// skip previous zero
					if (ch != '0')
					{
						// followed by other number; return 
						break;
					}
				}
			}
			return ch;
		}

		/// <summary>
		/// Method called if expected numeric value (due to leading sign) does not
		/// look like a number
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		protected internal virtual com.fasterxml.jackson.core.JsonToken _handleInvalidNumberStart
			(int ch, bool negative)
		{
			if (ch == 'I')
			{
				if (_inputPtr >= _inputEnd)
				{
					if (!loadMore())
					{
						_reportInvalidEOFInValue();
					}
				}
				ch = _inputBuffer[_inputPtr++];
				if (ch == 'N')
				{
					string match = negative ? "-INF" : "+INF";
					_matchToken(match, 3);
					if (isEnabled(com.fasterxml.jackson.core.JsonParser.Feature.ALLOW_NON_NUMERIC_NUMBERS
						))
					{
						return resetAsNaN(match, negative ? double.NegativeInfinity : double.PositiveInfinity
							);
					}
					_reportError("Non-standard token '" + match + "': enable JsonParser.Feature.ALLOW_NON_NUMERIC_NUMBERS to allow"
						);
				}
				else
				{
					if (ch == 'n')
					{
						string match = negative ? "-Infinity" : "+Infinity";
						_matchToken(match, 3);
						if (isEnabled(com.fasterxml.jackson.core.JsonParser.Feature.ALLOW_NON_NUMERIC_NUMBERS
							))
						{
							return resetAsNaN(match, negative ? double.NegativeInfinity : double.PositiveInfinity
								);
						}
						_reportError("Non-standard token '" + match + "': enable JsonParser.Feature.ALLOW_NON_NUMERIC_NUMBERS to allow"
							);
					}
				}
			}
			reportUnexpectedNumberChar(ch, "expected digit (0-9) to follow minus sign, for valid numeric value"
				);
			return null;
		}

		/// <summary>
		/// Method called to ensure that a root-value is followed by a space
		/// token.
		/// </summary>
		/// <remarks>
		/// Method called to ensure that a root-value is followed by a space
		/// token.
		/// <p>
		/// NOTE: caller MUST ensure there is at least one character available;
		/// and that input pointer is AT given char (not past)
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		private void _verifyRootSpace(int ch)
		{
			// caller had pushed it back, before calling; reset
			++_inputPtr;
			switch (ch)
			{
				case ' ':
				case '\t':
				{
					return;
				}

				case '\r':
				{
					_skipCR();
					return;
				}

				case '\n':
				{
					++_currInputRow;
					_currInputRowStart = _inputPtr;
					return;
				}
			}
			_reportMissingRootWS(ch);
		}

		/*
		/**********************************************************
		/* Internal methods, secondary parsing
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		protected internal string _parseName()
		{
			// First: let's try to see if we have a simple name: one that does
			// not cross input buffer boundary, and does not contain escape sequences.
			int ptr = _inputPtr;
			int hash = _hashSeed;
			int[] codes = _icLatin1;
			while (ptr < _inputEnd)
			{
				int ch = _inputBuffer[ptr];
				if (ch < codes.Length && codes[ch] != 0)
				{
					if (ch == '"')
					{
						int start = _inputPtr;
						_inputPtr = ptr + 1;
						// to skip the quote
						return _symbols.findSymbol(_inputBuffer, start, ptr - start, hash);
					}
					break;
				}
				hash = (hash * com.fasterxml.jackson.core.sym.CharsToNameCanonicalizer.HASH_MULT)
					 + ch;
				++ptr;
			}
			int start_1 = _inputPtr;
			_inputPtr = ptr;
			return _parseName2(start_1, hash, INT_QUOTE);
		}

		/// <exception cref="System.IO.IOException"/>
		private string _parseName2(int startPtr, int hash, int endChar)
		{
			_textBuffer.resetWithShared(_inputBuffer, startPtr, (_inputPtr - startPtr));
			/* Output pointers; calls will also ensure that the buffer is
			* not shared and has room for at least one more char.
			*/
			char[] outBuf = _textBuffer.getCurrentSegment();
			int outPtr = _textBuffer.getCurrentSegmentSize();
			while (true)
			{
				if (_inputPtr >= _inputEnd)
				{
					if (!loadMore())
					{
						_reportInvalidEOF(": was expecting closing '" + ((char)endChar) + "' for name");
					}
				}
				char c = _inputBuffer[_inputPtr++];
				int i = (int)c;
				if (i <= INT_BACKSLASH)
				{
					if (i == INT_BACKSLASH)
					{
						/* Although chars outside of BMP are to be escaped as
						* an UTF-16 surrogate pair, does that affect decoding?
						* For now let's assume it does not.
						*/
						c = _decodeEscaped();
					}
					else
					{
						if (i <= endChar)
						{
							if (i == endChar)
							{
								break;
							}
							if (i < INT_SPACE)
							{
								_throwUnquotedSpace(i, "name");
							}
						}
					}
				}
				hash = (hash * com.fasterxml.jackson.core.sym.CharsToNameCanonicalizer.HASH_MULT)
					 + c;
				// Ok, let's add char to output:
				outBuf[outPtr++] = c;
				// Need more room?
				if (outPtr >= outBuf.Length)
				{
					outBuf = _textBuffer.finishCurrentSegment();
					outPtr = 0;
				}
			}
			_textBuffer.setCurrentLength(outPtr);
			{
				com.fasterxml.jackson.core.util.TextBuffer tb = _textBuffer;
				char[] buf = tb.getTextBuffer();
				int start = tb.getTextOffset();
				int len = tb.size();
				return _symbols.findSymbol(buf, start, len, hash);
			}
		}

		/// <summary>
		/// Method called when we see non-white space character other
		/// than double quote, when expecting a field name.
		/// </summary>
		/// <remarks>
		/// Method called when we see non-white space character other
		/// than double quote, when expecting a field name.
		/// In standard mode will just throw an expection; but
		/// in non-standard modes may be able to parse name.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		protected internal virtual string _handleOddName(int i)
		{
			// [JACKSON-173]: allow single quotes
			if (i == '\'' && isEnabled(com.fasterxml.jackson.core.JsonParser.Feature.ALLOW_SINGLE_QUOTES
				))
			{
				return _parseAposName();
			}
			// [JACKSON-69]: allow unquoted names if feature enabled:
			if (!isEnabled(com.fasterxml.jackson.core.JsonParser.Feature.ALLOW_UNQUOTED_FIELD_NAMES
				))
			{
				_reportUnexpectedChar(i, "was expecting double-quote to start field name");
			}
			int[] codes = com.fasterxml.jackson.core.io.CharTypes.getInputCodeLatin1JsNames();
			int maxCode = codes.Length;
			// Also: first char must be a valid name char, but NOT be number
			bool firstOk;
			if (i < maxCode)
			{
				// identifier, or a number ([Issue#102])
				firstOk = (codes[i] == 0);
			}
			else
			{
				firstOk = char.isJavaIdentifierPart((char)i);
			}
			if (!firstOk)
			{
				_reportUnexpectedChar(i, "was expecting either valid name character (for unquoted name) or double-quote (for quoted) to start field name"
					);
			}
			int ptr = _inputPtr;
			int hash = _hashSeed;
			int inputLen = _inputEnd;
			if (ptr < inputLen)
			{
				do
				{
					int ch = _inputBuffer[ptr];
					if (ch < maxCode)
					{
						if (codes[ch] != 0)
						{
							int start = _inputPtr - 1;
							// -1 to bring back first char
							_inputPtr = ptr;
							return _symbols.findSymbol(_inputBuffer, start, ptr - start, hash);
						}
					}
					else
					{
						if (!char.isJavaIdentifierPart((char)ch))
						{
							int start = _inputPtr - 1;
							// -1 to bring back first char
							_inputPtr = ptr;
							return _symbols.findSymbol(_inputBuffer, start, ptr - start, hash);
						}
					}
					hash = (hash * com.fasterxml.jackson.core.sym.CharsToNameCanonicalizer.HASH_MULT)
						 + ch;
					++ptr;
				}
				while (ptr < inputLen);
			}
			int start_1 = _inputPtr - 1;
			_inputPtr = ptr;
			return _handleOddName2(start_1, hash, codes);
		}

		/// <exception cref="System.IO.IOException"/>
		protected internal virtual string _parseAposName()
		{
			// Note: mostly copy of_parseFieldName
			int ptr = _inputPtr;
			int hash = _hashSeed;
			int inputLen = _inputEnd;
			if (ptr < inputLen)
			{
				int[] codes = _icLatin1;
				int maxCode = codes.Length;
				do
				{
					int ch = _inputBuffer[ptr];
					if (ch == '\'')
					{
						int start = _inputPtr;
						_inputPtr = ptr + 1;
						// to skip the quote
						return _symbols.findSymbol(_inputBuffer, start, ptr - start, hash);
					}
					if (ch < maxCode && codes[ch] != 0)
					{
						break;
					}
					hash = (hash * com.fasterxml.jackson.core.sym.CharsToNameCanonicalizer.HASH_MULT)
						 + ch;
					++ptr;
				}
				while (ptr < inputLen);
			}
			int start_1 = _inputPtr;
			_inputPtr = ptr;
			return _parseName2(start_1, hash, '\'');
		}

		/// <summary>
		/// Method for handling cases where first non-space character
		/// of an expected value token is not legal for standard JSON content.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		protected internal virtual com.fasterxml.jackson.core.JsonToken _handleOddValue(int
			 i)
		{
			switch (i)
			{
				case '\'':
				{
					// Most likely an error, unless we are to allow single-quote-strings
					/* [JACKSON-173]: allow single quotes. Unlike with regular
					* Strings, we'll eagerly parse contents; this so that there's
					* no need to store information on quote char used.
					*
					* Also, no separation to fast/slow parsing; we'll just do
					* one regular (~= slowish) parsing, to keep code simple
					*/
					if (isEnabled(com.fasterxml.jackson.core.JsonParser.Feature.ALLOW_SINGLE_QUOTES))
					{
						return _handleApos();
					}
					break;
				}

				case 'N':
				{
					_matchToken("NaN", 1);
					if (isEnabled(com.fasterxml.jackson.core.JsonParser.Feature.ALLOW_NON_NUMERIC_NUMBERS
						))
					{
						return resetAsNaN("NaN", double.NaN);
					}
					_reportError("Non-standard token 'NaN': enable JsonParser.Feature.ALLOW_NON_NUMERIC_NUMBERS to allow"
						);
					break;
				}

				case 'I':
				{
					_matchToken("Infinity", 1);
					if (isEnabled(com.fasterxml.jackson.core.JsonParser.Feature.ALLOW_NON_NUMERIC_NUMBERS
						))
					{
						return resetAsNaN("Infinity", double.PositiveInfinity);
					}
					_reportError("Non-standard token 'Infinity': enable JsonParser.Feature.ALLOW_NON_NUMERIC_NUMBERS to allow"
						);
					break;
				}

				case '+':
				{
					// note: '-' is taken as number
					if (_inputPtr >= _inputEnd)
					{
						if (!loadMore())
						{
							_reportInvalidEOFInValue();
						}
					}
					return _handleInvalidNumberStart(_inputBuffer[_inputPtr++], false);
				}
			}
			// [Issue#77] Try to decode most likely token
			if (char.isJavaIdentifierStart(i))
			{
				_reportInvalidToken(string.Empty + ((char)i), "('true', 'false' or 'null')");
			}
			// but if it doesn't look like a token:
			_reportUnexpectedChar(i, "expected a valid value (number, String, array, object, 'true', 'false' or 'null')"
				);
			return null;
		}

		/// <exception cref="System.IO.IOException"/>
		protected internal virtual com.fasterxml.jackson.core.JsonToken _handleApos()
		{
			char[] outBuf = _textBuffer.emptyAndGetCurrentSegment();
			int outPtr = _textBuffer.getCurrentSegmentSize();
			while (true)
			{
				if (_inputPtr >= _inputEnd)
				{
					if (!loadMore())
					{
						_reportInvalidEOF(": was expecting closing quote for a string value");
					}
				}
				char c = _inputBuffer[_inputPtr++];
				int i = (int)c;
				if (i <= '\\')
				{
					if (i == '\\')
					{
						/* Although chars outside of BMP are to be escaped as
						* an UTF-16 surrogate pair, does that affect decoding?
						* For now let's assume it does not.
						*/
						c = _decodeEscaped();
					}
					else
					{
						if (i <= '\'')
						{
							if (i == '\'')
							{
								break;
							}
							if (i < INT_SPACE)
							{
								_throwUnquotedSpace(i, "string value");
							}
						}
					}
				}
				// Need more room?
				if (outPtr >= outBuf.Length)
				{
					outBuf = _textBuffer.finishCurrentSegment();
					outPtr = 0;
				}
				// Ok, let's add char to output:
				outBuf[outPtr++] = c;
			}
			_textBuffer.setCurrentLength(outPtr);
			return com.fasterxml.jackson.core.JsonToken.VALUE_STRING;
		}

		/// <exception cref="System.IO.IOException"/>
		private string _handleOddName2(int startPtr, int hash, int[] codes)
		{
			_textBuffer.resetWithShared(_inputBuffer, startPtr, (_inputPtr - startPtr));
			char[] outBuf = _textBuffer.getCurrentSegment();
			int outPtr = _textBuffer.getCurrentSegmentSize();
			int maxCode = codes.Length;
			while (true)
			{
				if (_inputPtr >= _inputEnd)
				{
					if (!loadMore())
					{
						// acceptable for now (will error out later)
						break;
					}
				}
				char c = _inputBuffer[_inputPtr];
				int i = (int)c;
				if (i <= maxCode)
				{
					if (codes[i] != 0)
					{
						break;
					}
				}
				else
				{
					if (!char.isJavaIdentifierPart(c))
					{
						break;
					}
				}
				++_inputPtr;
				hash = (hash * com.fasterxml.jackson.core.sym.CharsToNameCanonicalizer.HASH_MULT)
					 + i;
				// Ok, let's add char to output:
				outBuf[outPtr++] = c;
				// Need more room?
				if (outPtr >= outBuf.Length)
				{
					outBuf = _textBuffer.finishCurrentSegment();
					outPtr = 0;
				}
			}
			_textBuffer.setCurrentLength(outPtr);
			{
				com.fasterxml.jackson.core.util.TextBuffer tb = _textBuffer;
				char[] buf = tb.getTextBuffer();
				int start = tb.getTextOffset();
				int len = tb.size();
				return _symbols.findSymbol(buf, start, len, hash);
			}
		}

		/// <exception cref="System.IO.IOException"/>
		protected internal sealed override void _finishString()
		{
			/* First: let's try to see if we have simple String value: one
			* that does not cross input buffer boundary, and does not
			* contain escape sequences.
			*/
			int ptr = _inputPtr;
			int inputLen = _inputEnd;
			if (ptr < inputLen)
			{
				int[] codes = _icLatin1;
				int maxCode = codes.Length;
				do
				{
					int ch = _inputBuffer[ptr];
					if (ch < maxCode && codes[ch] != 0)
					{
						if (ch == '"')
						{
							_textBuffer.resetWithShared(_inputBuffer, _inputPtr, (ptr - _inputPtr));
							_inputPtr = ptr + 1;
							// Yes, we got it all
							return;
						}
						break;
					}
					++ptr;
				}
				while (ptr < inputLen);
			}
			/* Either ran out of input, or bumped into an escape
			* sequence...
			*/
			_textBuffer.resetWithCopy(_inputBuffer, _inputPtr, (ptr - _inputPtr));
			_inputPtr = ptr;
			_finishString2();
		}

		/// <exception cref="System.IO.IOException"/>
		protected internal virtual void _finishString2()
		{
			char[] outBuf = _textBuffer.getCurrentSegment();
			int outPtr = _textBuffer.getCurrentSegmentSize();
			int[] codes = _icLatin1;
			int maxCode = codes.Length;
			while (true)
			{
				if (_inputPtr >= _inputEnd)
				{
					if (!loadMore())
					{
						_reportInvalidEOF(": was expecting closing quote for a string value");
					}
				}
				char c = _inputBuffer[_inputPtr++];
				int i = (int)c;
				if (i < maxCode && codes[i] != 0)
				{
					if (i == INT_QUOTE)
					{
						break;
					}
					else
					{
						if (i == INT_BACKSLASH)
						{
							/* Although chars outside of BMP are to be escaped as
							* an UTF-16 surrogate pair, does that affect decoding?
							* For now let's assume it does not.
							*/
							c = _decodeEscaped();
						}
						else
						{
							if (i < INT_SPACE)
							{
								_throwUnquotedSpace(i, "string value");
							}
						}
					}
				}
				// anything else?
				// Need more room?
				if (outPtr >= outBuf.Length)
				{
					outBuf = _textBuffer.finishCurrentSegment();
					outPtr = 0;
				}
				// Ok, let's add char to output:
				outBuf[outPtr++] = c;
			}
			_textBuffer.setCurrentLength(outPtr);
		}

		/// <summary>
		/// Method called to skim through rest of unparsed String value,
		/// if it is not needed.
		/// </summary>
		/// <remarks>
		/// Method called to skim through rest of unparsed String value,
		/// if it is not needed. This can be done bit faster if contents
		/// need not be stored for future access.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		protected internal void _skipString()
		{
			_tokenIncomplete = false;
			int inPtr = _inputPtr;
			int inLen = _inputEnd;
			char[] inBuf = _inputBuffer;
			while (true)
			{
				if (inPtr >= inLen)
				{
					_inputPtr = inPtr;
					if (!loadMore())
					{
						_reportInvalidEOF(": was expecting closing quote for a string value");
					}
					inPtr = _inputPtr;
					inLen = _inputEnd;
				}
				char c = inBuf[inPtr++];
				int i = (int)c;
				if (i <= INT_BACKSLASH)
				{
					if (i == INT_BACKSLASH)
					{
						/* Although chars outside of BMP are to be escaped as
						* an UTF-16 surrogate pair, does that affect decoding?
						* For now let's assume it does not.
						*/
						_inputPtr = inPtr;
						c = _decodeEscaped();
						inPtr = _inputPtr;
						inLen = _inputEnd;
					}
					else
					{
						if (i <= INT_QUOTE)
						{
							if (i == INT_QUOTE)
							{
								_inputPtr = inPtr;
								break;
							}
							if (i < INT_SPACE)
							{
								_inputPtr = inPtr;
								_throwUnquotedSpace(i, "string value");
							}
						}
					}
				}
			}
		}

		/*
		/**********************************************************
		/* Internal methods, other parsing
		/**********************************************************
		*/
		/// <summary>
		/// We actually need to check the character value here
		/// (to see if we have \n following \r).
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		protected internal void _skipCR()
		{
			if (_inputPtr < _inputEnd || loadMore())
			{
				if (_inputBuffer[_inputPtr] == '\n')
				{
					++_inputPtr;
				}
			}
			++_currInputRow;
			_currInputRowStart = _inputPtr;
		}

		/// <exception cref="System.IO.IOException"/>
		private int _skipColon()
		{
			if ((_inputPtr + 4) >= _inputEnd)
			{
				return _skipColon2(false);
			}
			char c = _inputBuffer[_inputPtr];
			if (c == ':')
			{
				// common case, no leading space
				int i = _inputBuffer[++_inputPtr];
				if (i > INT_SPACE)
				{
					// nor trailing
					if (i == INT_SLASH || i == INT_HASH)
					{
						return _skipColon2(true);
					}
					++_inputPtr;
					return i;
				}
				if (i == INT_SPACE || i == INT_TAB)
				{
					i = (int)_inputBuffer[++_inputPtr];
					if (i > INT_SPACE)
					{
						if (i == INT_SLASH || i == INT_HASH)
						{
							return _skipColon2(true);
						}
						++_inputPtr;
						return i;
					}
				}
				return _skipColon2(true);
			}
			// true -> skipped colon
			if (c == ' ' || c == '\t')
			{
				c = _inputBuffer[++_inputPtr];
			}
			if (c == ':')
			{
				int i = _inputBuffer[++_inputPtr];
				if (i > INT_SPACE)
				{
					if (i == INT_SLASH || i == INT_HASH)
					{
						return _skipColon2(true);
					}
					++_inputPtr;
					return i;
				}
				if (i == INT_SPACE || i == INT_TAB)
				{
					i = (int)_inputBuffer[++_inputPtr];
					if (i > INT_SPACE)
					{
						if (i == INT_SLASH || i == INT_HASH)
						{
							return _skipColon2(true);
						}
						++_inputPtr;
						return i;
					}
				}
				return _skipColon2(true);
			}
			return _skipColon2(false);
		}

		/// <exception cref="System.IO.IOException"/>
		private int _skipColon2(bool gotColon)
		{
			while (true)
			{
				if (_inputPtr >= _inputEnd)
				{
					loadMoreGuaranteed();
				}
				int i = (int)_inputBuffer[_inputPtr++];
				if (i > INT_SPACE)
				{
					if (i == INT_SLASH)
					{
						_skipComment();
						continue;
					}
					if (i == INT_HASH)
					{
						if (_skipYAMLComment())
						{
							continue;
						}
					}
					if (gotColon)
					{
						return i;
					}
					if (i != INT_COLON)
					{
						if (i < INT_SPACE)
						{
							_throwInvalidSpace(i);
						}
						_reportUnexpectedChar(i, "was expecting a colon to separate field name and value"
							);
					}
					gotColon = true;
					continue;
				}
				if (i < INT_SPACE)
				{
					if (i == INT_LF)
					{
						++_currInputRow;
						_currInputRowStart = _inputPtr;
					}
					else
					{
						if (i == INT_CR)
						{
							_skipCR();
						}
						else
						{
							if (i != INT_TAB)
							{
								_throwInvalidSpace(i);
							}
						}
					}
				}
			}
		}

		// Primary loop: no reloading, comment handling
		/// <exception cref="System.IO.IOException"/>
		private int _skipComma(int i)
		{
			if (i != INT_COMMA)
			{
				_reportUnexpectedChar(i, "was expecting comma to separate " + _parsingContext.getTypeDesc
					() + " entries");
			}
			while (_inputPtr < _inputEnd)
			{
				i = (int)_inputBuffer[_inputPtr++];
				if (i > INT_SPACE)
				{
					if (i == INT_SLASH || i == INT_HASH)
					{
						--_inputPtr;
						return _skipAfterComma2();
					}
					return i;
				}
				if (i < INT_SPACE)
				{
					if (i == INT_LF)
					{
						++_currInputRow;
						_currInputRowStart = _inputPtr;
					}
					else
					{
						if (i == INT_CR)
						{
							_skipCR();
						}
						else
						{
							if (i != INT_TAB)
							{
								_throwInvalidSpace(i);
							}
						}
					}
				}
			}
			return _skipAfterComma2();
		}

		/// <exception cref="System.IO.IOException"/>
		private int _skipAfterComma2()
		{
			while (_inputPtr < _inputEnd || loadMore())
			{
				int i = (int)_inputBuffer[_inputPtr++];
				if (i > INT_SPACE)
				{
					if (i == INT_SLASH)
					{
						_skipComment();
						continue;
					}
					if (i == INT_HASH)
					{
						if (_skipYAMLComment())
						{
							continue;
						}
					}
					return i;
				}
				if (i < INT_SPACE)
				{
					if (i == INT_LF)
					{
						++_currInputRow;
						_currInputRowStart = _inputPtr;
					}
					else
					{
						if (i == INT_CR)
						{
							_skipCR();
						}
						else
						{
							if (i != INT_TAB)
							{
								_throwInvalidSpace(i);
							}
						}
					}
				}
			}
			throw _constructError("Unexpected end-of-input within/between " + _parsingContext
				.getTypeDesc() + " entries");
		}

		/// <exception cref="System.IO.IOException"/>
		private int _skipWSOrEnd()
		{
			// Let's handle first character separately since it is likely that
			// it is either non-whitespace; or we have longer run of white space
			if (_inputPtr >= _inputEnd)
			{
				if (!loadMore())
				{
					return _eofAsNextChar();
				}
			}
			int i = _inputBuffer[_inputPtr++];
			if (i > INT_SPACE)
			{
				if (i == INT_SLASH || i == INT_HASH)
				{
					--_inputPtr;
					return _skipWSOrEnd2();
				}
				return i;
			}
			if (i != INT_SPACE)
			{
				if (i == INT_LF)
				{
					++_currInputRow;
					_currInputRowStart = _inputPtr;
				}
				else
				{
					if (i == INT_CR)
					{
						_skipCR();
					}
					else
					{
						if (i != INT_TAB)
						{
							_throwInvalidSpace(i);
						}
					}
				}
			}
			while (_inputPtr < _inputEnd)
			{
				i = (int)_inputBuffer[_inputPtr++];
				if (i > INT_SPACE)
				{
					if (i == INT_SLASH || i == INT_HASH)
					{
						--_inputPtr;
						return _skipWSOrEnd2();
					}
					return i;
				}
				if (i != INT_SPACE)
				{
					if (i == INT_LF)
					{
						++_currInputRow;
						_currInputRowStart = _inputPtr;
					}
					else
					{
						if (i == INT_CR)
						{
							_skipCR();
						}
						else
						{
							if (i != INT_TAB)
							{
								_throwInvalidSpace(i);
							}
						}
					}
				}
			}
			return _skipWSOrEnd2();
		}

		/// <exception cref="System.IO.IOException"/>
		private int _skipWSOrEnd2()
		{
			while (true)
			{
				if (_inputPtr >= _inputEnd)
				{
					if (!loadMore())
					{
						// We ran out of input...
						return _eofAsNextChar();
					}
				}
				int i = (int)_inputBuffer[_inputPtr++];
				if (i > INT_SPACE)
				{
					if (i == INT_SLASH)
					{
						_skipComment();
						continue;
					}
					if (i == INT_HASH)
					{
						if (_skipYAMLComment())
						{
							continue;
						}
					}
					return i;
				}
				else
				{
					if (i != INT_SPACE)
					{
						if (i == INT_LF)
						{
							++_currInputRow;
							_currInputRowStart = _inputPtr;
						}
						else
						{
							if (i == INT_CR)
							{
								_skipCR();
							}
							else
							{
								if (i != INT_TAB)
								{
									_throwInvalidSpace(i);
								}
							}
						}
					}
				}
			}
		}

		/// <exception cref="System.IO.IOException"/>
		private void _skipComment()
		{
			if (!isEnabled(com.fasterxml.jackson.core.JsonParser.Feature.ALLOW_COMMENTS))
			{
				_reportUnexpectedChar('/', "maybe a (non-standard) comment? (not recognized as one since Feature 'ALLOW_COMMENTS' not enabled for parser)"
					);
			}
			// First: check which comment (if either) it is:
			if (_inputPtr >= _inputEnd && !loadMore())
			{
				_reportInvalidEOF(" in a comment");
			}
			char c = _inputBuffer[_inputPtr++];
			if (c == '/')
			{
				_skipLine();
			}
			else
			{
				if (c == '*')
				{
					_skipCComment();
				}
				else
				{
					_reportUnexpectedChar(c, "was expecting either '*' or '/' for a comment");
				}
			}
		}

		/// <exception cref="System.IO.IOException"/>
		private void _skipCComment()
		{
			// Ok: need the matching '*/'
			while ((_inputPtr < _inputEnd) || loadMore())
			{
				int i = (int)_inputBuffer[_inputPtr++];
				if (i <= '*')
				{
					if (i == '*')
					{
						// end?
						if ((_inputPtr >= _inputEnd) && !loadMore())
						{
							break;
						}
						if (_inputBuffer[_inputPtr] == INT_SLASH)
						{
							++_inputPtr;
							return;
						}
						continue;
					}
					if (i < INT_SPACE)
					{
						if (i == INT_LF)
						{
							++_currInputRow;
							_currInputRowStart = _inputPtr;
						}
						else
						{
							if (i == INT_CR)
							{
								_skipCR();
							}
							else
							{
								if (i != INT_TAB)
								{
									_throwInvalidSpace(i);
								}
							}
						}
					}
				}
			}
			_reportInvalidEOF(" in a comment");
		}

		/// <exception cref="System.IO.IOException"/>
		private bool _skipYAMLComment()
		{
			if (!isEnabled(com.fasterxml.jackson.core.JsonParser.Feature.ALLOW_YAML_COMMENTS))
			{
				return false;
			}
			_skipLine();
			return true;
		}

		/// <exception cref="System.IO.IOException"/>
		private void _skipLine()
		{
			// Ok: need to find EOF or linefeed
			while ((_inputPtr < _inputEnd) || loadMore())
			{
				int i = (int)_inputBuffer[_inputPtr++];
				if (i < INT_SPACE)
				{
					if (i == INT_LF)
					{
						++_currInputRow;
						_currInputRowStart = _inputPtr;
						break;
					}
					else
					{
						if (i == INT_CR)
						{
							_skipCR();
							break;
						}
						else
						{
							if (i != INT_TAB)
							{
								_throwInvalidSpace(i);
							}
						}
					}
				}
			}
		}

		/// <exception cref="System.IO.IOException"/>
		protected internal override char _decodeEscaped()
		{
			if (_inputPtr >= _inputEnd)
			{
				if (!loadMore())
				{
					_reportInvalidEOF(" in character escape sequence");
				}
			}
			char c = _inputBuffer[_inputPtr++];
			switch ((int)c)
			{
				case 'b':
				{
					// First, ones that are mapped
					return '\b';
				}

				case 't':
				{
					return '\t';
				}

				case 'n':
				{
					return '\n';
				}

				case 'f':
				{
					return '\f';
				}

				case 'r':
				{
					return '\r';
				}

				case '"':
				case '/':
				case '\\':
				{
					// And these are to be returned as they are
					return c;
				}

				case 'u':
				{
					// and finally hex-escaped
					break;
				}

				default:
				{
					return _handleUnrecognizedCharacterEscape(c);
				}
			}
			// Ok, a hex escape. Need 4 characters
			int value = 0;
			for (int i = 0; i < 4; ++i)
			{
				if (_inputPtr >= _inputEnd)
				{
					if (!loadMore())
					{
						_reportInvalidEOF(" in character escape sequence");
					}
				}
				int ch = (int)_inputBuffer[_inputPtr++];
				int digit = com.fasterxml.jackson.core.io.CharTypes.charToHex(ch);
				if (digit < 0)
				{
					_reportUnexpectedChar(ch, "expected a hex-digit for character escape sequence");
				}
				value = (value << 4) | digit;
			}
			return (char)value;
		}

		/// <exception cref="System.IO.IOException"/>
		private void _matchTrue()
		{
			int ptr = _inputPtr;
			if ((ptr + 3) < _inputEnd)
			{
				char[] b = _inputBuffer;
				if (b[ptr] == 'r' && b[++ptr] == 'u' && b[++ptr] == 'e')
				{
					char c = b[++ptr];
					if (c < '0' || c == ']' || c == '}')
					{
						// expected/allowed chars
						_inputPtr = ptr;
						return;
					}
				}
			}
			// buffer boundary, or problem, offline
			_matchToken("true", 1);
		}

		/// <exception cref="System.IO.IOException"/>
		private void _matchFalse()
		{
			int ptr = _inputPtr;
			if ((ptr + 4) < _inputEnd)
			{
				char[] b = _inputBuffer;
				if (b[ptr] == 'a' && b[++ptr] == 'l' && b[++ptr] == 's' && b[++ptr] == 'e')
				{
					char c = b[++ptr];
					if (c < '0' || c == ']' || c == '}')
					{
						// expected/allowed chars
						_inputPtr = ptr;
						return;
					}
				}
			}
			// buffer boundary, or problem, offline
			_matchToken("false", 1);
		}

		/// <exception cref="System.IO.IOException"/>
		private void _matchNull()
		{
			int ptr = _inputPtr;
			if ((ptr + 3) < _inputEnd)
			{
				char[] b = _inputBuffer;
				if (b[ptr] == 'u' && b[++ptr] == 'l' && b[++ptr] == 'l')
				{
					char c = b[++ptr];
					if (c < '0' || c == ']' || c == '}')
					{
						// expected/allowed chars
						_inputPtr = ptr;
						return;
					}
				}
			}
			// buffer boundary, or problem, offline
			_matchToken("null", 1);
		}

		/// <summary>Helper method for checking whether input matches expected token</summary>
		/// <exception cref="System.IO.IOException"/>
		protected internal void _matchToken(string matchStr, int i)
		{
			int len = matchStr.Length;
			do
			{
				if (_inputPtr >= _inputEnd)
				{
					if (!loadMore())
					{
						_reportInvalidToken(Sharpen.Runtime.substring(matchStr, 0, i));
					}
				}
				if (_inputBuffer[_inputPtr] != matchStr[i])
				{
					_reportInvalidToken(Sharpen.Runtime.substring(matchStr, 0, i));
				}
				++_inputPtr;
			}
			while (++i < len);
			// but let's also ensure we either get EOF, or non-alphanum char...
			if (_inputPtr >= _inputEnd)
			{
				if (!loadMore())
				{
					return;
				}
			}
			char c = _inputBuffer[_inputPtr];
			if (c < '0' || c == ']' || c == '}')
			{
				// expected/allowed chars
				return;
			}
			// if Java letter, it's a problem tho
			if (char.isJavaIdentifierPart(c))
			{
				_reportInvalidToken(Sharpen.Runtime.substring(matchStr, 0, i));
			}
			return;
		}

		/*
		/**********************************************************
		/* Binary access
		/**********************************************************
		*/
		/// <summary>
		/// Efficient handling for incremental parsing of base64-encoded
		/// textual content.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		protected internal virtual byte[] _decodeBase64(com.fasterxml.jackson.core.Base64Variant
			 b64variant)
		{
			com.fasterxml.jackson.core.util.ByteArrayBuilder builder = _getByteArrayBuilder();
			//main_loop:
			while (true)
			{
				// first, we'll skip preceding white space, if any
				char ch;
				do
				{
					if (_inputPtr >= _inputEnd)
					{
						loadMoreGuaranteed();
					}
					ch = _inputBuffer[_inputPtr++];
				}
				while (ch <= INT_SPACE);
				int bits = b64variant.decodeBase64Char(ch);
				if (bits < 0)
				{
					if (ch == '"')
					{
						// reached the end, fair and square?
						return builder.toByteArray();
					}
					bits = _decodeBase64Escape(b64variant, ch, 0);
					if (bits < 0)
					{
						// white space to skip
						continue;
					}
				}
				int decodedData = bits;
				// then second base64 char; can't get padding yet, nor ws
				if (_inputPtr >= _inputEnd)
				{
					loadMoreGuaranteed();
				}
				ch = _inputBuffer[_inputPtr++];
				bits = b64variant.decodeBase64Char(ch);
				if (bits < 0)
				{
					bits = _decodeBase64Escape(b64variant, ch, 1);
				}
				decodedData = (decodedData << 6) | bits;
				// third base64 char; can be padding, but not ws
				if (_inputPtr >= _inputEnd)
				{
					loadMoreGuaranteed();
				}
				ch = _inputBuffer[_inputPtr++];
				bits = b64variant.decodeBase64Char(ch);
				// First branch: can get padding (-> 1 byte)
				if (bits < 0)
				{
					if (bits != com.fasterxml.jackson.core.Base64Variant.BASE64_VALUE_PADDING)
					{
						// as per [JACKSON-631], could also just be 'missing'  padding
						if (ch == '"' && !b64variant.usesPadding())
						{
							decodedData >>= 4;
							builder.append(decodedData);
							return builder.toByteArray();
						}
						bits = _decodeBase64Escape(b64variant, ch, 2);
					}
					if (bits == com.fasterxml.jackson.core.Base64Variant.BASE64_VALUE_PADDING)
					{
						// Ok, must get more padding chars, then
						if (_inputPtr >= _inputEnd)
						{
							loadMoreGuaranteed();
						}
						ch = _inputBuffer[_inputPtr++];
						if (!b64variant.usesPaddingChar(ch))
						{
							throw reportInvalidBase64Char(b64variant, ch, 3, "expected padding character '" +
								 b64variant.getPaddingChar() + "'");
						}
						// Got 12 bits, only need 8, need to shift
						decodedData >>= 4;
						builder.append(decodedData);
						continue;
					}
				}
				// otherwise we got escaped other char, to be processed below
				// Nope, 2 or 3 bytes
				decodedData = (decodedData << 6) | bits;
				// fourth and last base64 char; can be padding, but not ws
				if (_inputPtr >= _inputEnd)
				{
					loadMoreGuaranteed();
				}
				ch = _inputBuffer[_inputPtr++];
				bits = b64variant.decodeBase64Char(ch);
				if (bits < 0)
				{
					if (bits != com.fasterxml.jackson.core.Base64Variant.BASE64_VALUE_PADDING)
					{
						// as per [JACKSON-631], could also just be 'missing'  padding
						if (ch == '"' && !b64variant.usesPadding())
						{
							decodedData >>= 2;
							builder.appendTwoBytes(decodedData);
							return builder.toByteArray();
						}
						bits = _decodeBase64Escape(b64variant, ch, 3);
					}
					if (bits == com.fasterxml.jackson.core.Base64Variant.BASE64_VALUE_PADDING)
					{
						// With padding we only get 2 bytes; but we have
						// to shift it a bit so it is identical to triplet
						// case with partial output.
						// 3 chars gives 3x6 == 18 bits, of which 2 are
						// dummies, need to discard:
						decodedData >>= 2;
						builder.appendTwoBytes(decodedData);
						continue;
					}
				}
				// otherwise we got escaped other char, to be processed below
				// otherwise, our triplet is now complete
				decodedData = (decodedData << 6) | bits;
				builder.appendThreeBytes(decodedData);
			}
		}

		/*
		/**********************************************************
		/* Error reporting
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		protected internal virtual void _reportInvalidToken(string matchedPart)
		{
			_reportInvalidToken(matchedPart, "'null', 'true', 'false' or NaN");
		}

		/// <exception cref="System.IO.IOException"/>
		protected internal virtual void _reportInvalidToken(string matchedPart, string msg
			)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder(matchedPart);
			/* Let's just try to find what appears to be the token, using
			* regular Java identifier character rules. It's just a heuristic,
			* nothing fancy here.
			*/
			while (true)
			{
				if (_inputPtr >= _inputEnd)
				{
					if (!loadMore())
					{
						break;
					}
				}
				char c = _inputBuffer[_inputPtr];
				if (!char.isJavaIdentifierPart(c))
				{
					break;
				}
				++_inputPtr;
				sb.Append(c);
			}
			_reportError("Unrecognized token '" + sb.ToString() + "': was expecting " + msg);
		}
	}
}
