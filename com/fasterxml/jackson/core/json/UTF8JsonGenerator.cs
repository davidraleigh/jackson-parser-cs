using Sharpen;

namespace com.fasterxml.jackson.core.json
{
	public class UTF8JsonGenerator : com.fasterxml.jackson.core.json.JsonGeneratorImpl
	{
		private const byte BYTE_u = unchecked((byte)(byte)('u'));

		private const byte BYTE_0 = unchecked((byte)(byte)('0'));

		private const byte BYTE_LBRACKET = unchecked((byte)(byte)('['));

		private const byte BYTE_RBRACKET = unchecked((byte)(byte)(']'));

		private const byte BYTE_LCURLY = unchecked((byte)(byte)('{'));

		private const byte BYTE_RCURLY = unchecked((byte)(byte)('}'));

		private const byte BYTE_BACKSLASH = unchecked((byte)(byte)('\\'));

		private const byte BYTE_COMMA = unchecked((byte)(byte)(','));

		private const byte BYTE_COLON = unchecked((byte)(byte)(':'));

		private const byte BYTE_QUOTE = unchecked((byte)(byte)('"'));

		private const int MAX_BYTES_TO_BUFFER = 512;

		internal static readonly byte[] HEX_CHARS = com.fasterxml.jackson.core.io.CharTypes
			.copyHexBytes();

		private static readonly byte[] NULL_BYTES = new byte[] { (byte)('n'), (byte)('u')
			, (byte)('l'), (byte)('l') };

		private static readonly byte[] TRUE_BYTES = new byte[] { (byte)('t'), (byte)('r')
			, (byte)('u'), (byte)('e') };

		private static readonly byte[] FALSE_BYTES = new byte[] { (byte)('f'), (byte)('a'
			), (byte)('l'), (byte)('s'), (byte)('e') };

		/// <summary>Underlying output stream used for writing JSON content.</summary>
		protected internal readonly Sharpen.OutputStream _outputStream;

		/// <summary>
		/// Intermediate buffer in which contents are buffered before
		/// being written using
		/// <see cref="_outputStream"/>
		/// .
		/// </summary>
		protected internal byte[] _outputBuffer;

		/// <summary>
		/// Pointer to the position right beyond the last character to output
		/// (end marker; may be past the buffer)
		/// </summary>
		protected internal int _outputTail = 0;

		/// <summary>
		/// End marker of the output buffer; one past the last valid position
		/// within the buffer.
		/// </summary>
		protected internal readonly int _outputEnd;

		/// <summary>
		/// Maximum number of <code>char</code>s that we know will always fit
		/// in the output buffer after escaping
		/// </summary>
		protected internal readonly int _outputMaxContiguous;

		/// <summary>
		/// Intermediate buffer in which characters of a String are copied
		/// before being encoded.
		/// </summary>
		protected internal char[] _charBuffer;

		/// <summary>Length of <code>_charBuffer</code></summary>
		protected internal readonly int _charBufferLength;

		/// <summary>
		/// 6 character temporary buffer allocated if needed, for constructing
		/// escape sequences
		/// </summary>
		protected internal byte[] _entityBuffer;

		/// <summary>
		/// Flag that indicates whether the output buffer is recycable (and
		/// needs to be returned to recycler once we are done) or not.
		/// </summary>
		protected internal bool _bufferRecyclable;

		/// <summary>
		/// Flag that is set if quoting is not to be added around
		/// JSON Object property names.
		/// </summary>
		protected internal bool _cfgUnqNames;

		public UTF8JsonGenerator(com.fasterxml.jackson.core.io.IOContext ctxt, int features
			, com.fasterxml.jackson.core.ObjectCodec codec, Sharpen.OutputStream @out)
			: base(ctxt, features, codec)
		{
			// intermediate copies only made up to certain length...
			/*
			/**********************************************************
			/* Output buffering
			/**********************************************************
			*/
			/*
			/**********************************************************
			/* Quick flags
			/**********************************************************
			*/
			/*
			/**********************************************************
			/* Life-cycle
			/**********************************************************
			*/
			_outputStream = @out;
			_bufferRecyclable = true;
			_outputBuffer = ctxt.allocWriteEncodingBuffer();
			_outputEnd = _outputBuffer.Length;
			/* To be exact, each char can take up to 6 bytes when escaped (Unicode
			* escape with backslash, 'u' and 4 hex digits); but to avoid fluctuation,
			* we will actually round down to only do up to 1/8 number of chars
			*/
			_outputMaxContiguous = _outputEnd >> 3;
			_charBuffer = ctxt.allocConcatBuffer();
			_charBufferLength = _charBuffer.Length;
			// By default we use this feature to determine additional quoting
			if (isEnabled(com.fasterxml.jackson.core.JsonGenerator.Feature.ESCAPE_NON_ASCII))
			{
				setHighestNonEscapedChar(127);
			}
			_cfgUnqNames = !com.fasterxml.jackson.core.JsonGenerator.Feature.QUOTE_FIELD_NAMES
				.enabledIn(features);
		}

		public UTF8JsonGenerator(com.fasterxml.jackson.core.io.IOContext ctxt, int features
			, com.fasterxml.jackson.core.ObjectCodec codec, Sharpen.OutputStream @out, byte[]
			 outputBuffer, int outputOffset, bool bufferRecyclable)
			: base(ctxt, features, codec)
		{
			_outputStream = @out;
			_bufferRecyclable = bufferRecyclable;
			_outputTail = outputOffset;
			_outputBuffer = outputBuffer;
			_outputEnd = _outputBuffer.Length;
			// up to 6 bytes per char (see above), rounded up to 1/8
			_outputMaxContiguous = (_outputEnd >> 3);
			_charBuffer = ctxt.allocConcatBuffer();
			_charBufferLength = _charBuffer.Length;
			_cfgUnqNames = !com.fasterxml.jackson.core.JsonGenerator.Feature.QUOTE_FIELD_NAMES
				.enabledIn(features);
		}

		/*
		/**********************************************************
		/* Overridden configuration methods
		/**********************************************************
		*/
		public override object getOutputTarget()
		{
			return _outputStream;
		}

		public override int getOutputBuffered()
		{
			// Assuming tail is always valid, set to 0 on close
			return _outputTail;
		}

		/*
		/**********************************************************
		/* Overridden methods
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		public override void writeFieldName(string name)
		{
			if (_cfgPrettyPrinter != null)
			{
				_writePPFieldName(name);
				return;
			}
			int status = _writeContext.writeFieldName(name);
			if (status == com.fasterxml.jackson.core.json.JsonWriteContext.STATUS_EXPECT_VALUE)
			{
				_reportError("Can not write a field name, expecting a value");
			}
			if (status == com.fasterxml.jackson.core.json.JsonWriteContext.STATUS_OK_AFTER_COMMA)
			{
				// need comma
				if (_outputTail >= _outputEnd)
				{
					_flushBuffer();
				}
				_outputBuffer[_outputTail++] = BYTE_COMMA;
			}
			/* To support [JACKSON-46], we'll do this:
			* (Question: should quoting of spaces (etc) still be enabled?)
			*/
			if (_cfgUnqNames)
			{
				_writeStringSegments(name, false);
				return;
			}
			int len = name.Length;
			// Does it fit in buffer?
			if (len > _charBufferLength)
			{
				// no, offline
				_writeStringSegments(name, true);
				return;
			}
			if (_outputTail >= _outputEnd)
			{
				_flushBuffer();
			}
			_outputBuffer[_outputTail++] = BYTE_QUOTE;
			// But as one segment, or multiple?
			if (len <= _outputMaxContiguous)
			{
				if ((_outputTail + len) > _outputEnd)
				{
					// caller must ensure enough space
					_flushBuffer();
				}
				_writeStringSegment(name, 0, len);
			}
			else
			{
				_writeStringSegments(name, 0, len);
			}
			// and closing quotes; need room for one more char:
			if (_outputTail >= _outputEnd)
			{
				_flushBuffer();
			}
			_outputBuffer[_outputTail++] = BYTE_QUOTE;
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeFieldName(com.fasterxml.jackson.core.SerializableString
			 name)
		{
			if (_cfgPrettyPrinter != null)
			{
				_writePPFieldName(name);
				return;
			}
			int status = _writeContext.writeFieldName(name.getValue());
			if (status == com.fasterxml.jackson.core.json.JsonWriteContext.STATUS_EXPECT_VALUE)
			{
				_reportError("Can not write a field name, expecting a value");
			}
			if (status == com.fasterxml.jackson.core.json.JsonWriteContext.STATUS_OK_AFTER_COMMA)
			{
				if (_outputTail >= _outputEnd)
				{
					_flushBuffer();
				}
				_outputBuffer[_outputTail++] = BYTE_COMMA;
			}
			if (_cfgUnqNames)
			{
				_writeUnq(name);
				return;
			}
			if (_outputTail >= _outputEnd)
			{
				_flushBuffer();
			}
			_outputBuffer[_outputTail++] = BYTE_QUOTE;
			int len = name.appendQuotedUTF8(_outputBuffer, _outputTail);
			if (len < 0)
			{
				// couldn't append, bit longer processing
				_writeBytes(name.asQuotedUTF8());
			}
			else
			{
				_outputTail += len;
			}
			if (_outputTail >= _outputEnd)
			{
				_flushBuffer();
			}
			_outputBuffer[_outputTail++] = BYTE_QUOTE;
		}

		/// <exception cref="System.IO.IOException"/>
		private void _writeUnq(com.fasterxml.jackson.core.SerializableString name)
		{
			int len = name.appendQuotedUTF8(_outputBuffer, _outputTail);
			if (len < 0)
			{
				_writeBytes(name.asQuotedUTF8());
			}
			else
			{
				_outputTail += len;
			}
		}

		/*
		/**********************************************************
		/* Output method implementations, structural
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		public sealed override void writeStartArray()
		{
			_verifyValueWrite("start an array");
			_writeContext = _writeContext.createChildArrayContext();
			if (_cfgPrettyPrinter != null)
			{
				_cfgPrettyPrinter.writeStartArray(this);
			}
			else
			{
				if (_outputTail >= _outputEnd)
				{
					_flushBuffer();
				}
				_outputBuffer[_outputTail++] = BYTE_LBRACKET;
			}
		}

		/// <exception cref="System.IO.IOException"/>
		public sealed override void writeEndArray()
		{
			if (!_writeContext.inArray())
			{
				_reportError("Current context not an ARRAY but " + _writeContext.getTypeDesc());
			}
			if (_cfgPrettyPrinter != null)
			{
				_cfgPrettyPrinter.writeEndArray(this, _writeContext.getEntryCount());
			}
			else
			{
				if (_outputTail >= _outputEnd)
				{
					_flushBuffer();
				}
				_outputBuffer[_outputTail++] = BYTE_RBRACKET;
			}
			_writeContext = ((com.fasterxml.jackson.core.json.JsonWriteContext)_writeContext.
				getParent());
		}

		/// <exception cref="System.IO.IOException"/>
		public sealed override void writeStartObject()
		{
			_verifyValueWrite("start an object");
			_writeContext = _writeContext.createChildObjectContext();
			if (_cfgPrettyPrinter != null)
			{
				_cfgPrettyPrinter.writeStartObject(this);
			}
			else
			{
				if (_outputTail >= _outputEnd)
				{
					_flushBuffer();
				}
				_outputBuffer[_outputTail++] = BYTE_LCURLY;
			}
		}

		/// <exception cref="System.IO.IOException"/>
		public sealed override void writeEndObject()
		{
			if (!_writeContext.inObject())
			{
				_reportError("Current context not an object but " + _writeContext.getTypeDesc());
			}
			if (_cfgPrettyPrinter != null)
			{
				_cfgPrettyPrinter.writeEndObject(this, _writeContext.getEntryCount());
			}
			else
			{
				if (_outputTail >= _outputEnd)
				{
					_flushBuffer();
				}
				_outputBuffer[_outputTail++] = BYTE_RCURLY;
			}
			_writeContext = ((com.fasterxml.jackson.core.json.JsonWriteContext)_writeContext.
				getParent());
		}

		/// <summary>
		/// Specialized version of <code>_writeFieldName</code>, off-lined
		/// to keep the "fast path" as simple (and hopefully fast) as possible.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		protected internal void _writePPFieldName(string name)
		{
			int status = _writeContext.writeFieldName(name);
			if (status == com.fasterxml.jackson.core.json.JsonWriteContext.STATUS_EXPECT_VALUE)
			{
				_reportError("Can not write a field name, expecting a value");
			}
			if ((status == com.fasterxml.jackson.core.json.JsonWriteContext.STATUS_OK_AFTER_COMMA
				))
			{
				_cfgPrettyPrinter.writeObjectEntrySeparator(this);
			}
			else
			{
				_cfgPrettyPrinter.beforeObjectEntries(this);
			}
			if (_cfgUnqNames)
			{
				_writeStringSegments(name, false);
				return;
			}
			int len = name.Length;
			if (len > _charBufferLength)
			{
				_writeStringSegments(name, true);
				return;
			}
			if (_outputTail >= _outputEnd)
			{
				_flushBuffer();
			}
			_outputBuffer[_outputTail++] = BYTE_QUOTE;
			Sharpen.Runtime.getCharsForString(name, 0, len, _charBuffer, 0);
			// But as one segment, or multiple?
			if (len <= _outputMaxContiguous)
			{
				if ((_outputTail + len) > _outputEnd)
				{
					// caller must ensure enough space
					_flushBuffer();
				}
				_writeStringSegment(_charBuffer, 0, len);
			}
			else
			{
				_writeStringSegments(_charBuffer, 0, len);
			}
			if (_outputTail >= _outputEnd)
			{
				_flushBuffer();
			}
			_outputBuffer[_outputTail++] = BYTE_QUOTE;
		}

		/// <exception cref="System.IO.IOException"/>
		protected internal void _writePPFieldName(com.fasterxml.jackson.core.SerializableString
			 name)
		{
			int status = _writeContext.writeFieldName(name.getValue());
			if (status == com.fasterxml.jackson.core.json.JsonWriteContext.STATUS_EXPECT_VALUE)
			{
				_reportError("Can not write a field name, expecting a value");
			}
			if (status == com.fasterxml.jackson.core.json.JsonWriteContext.STATUS_OK_AFTER_COMMA)
			{
				_cfgPrettyPrinter.writeObjectEntrySeparator(this);
			}
			else
			{
				_cfgPrettyPrinter.beforeObjectEntries(this);
			}
			bool addQuotes = !_cfgUnqNames;
			// standard
			if (addQuotes)
			{
				if (_outputTail >= _outputEnd)
				{
					_flushBuffer();
				}
				_outputBuffer[_outputTail++] = BYTE_QUOTE;
			}
			_writeBytes(name.asQuotedUTF8());
			if (addQuotes)
			{
				if (_outputTail >= _outputEnd)
				{
					_flushBuffer();
				}
				_outputBuffer[_outputTail++] = BYTE_QUOTE;
			}
		}

		/*
		/**********************************************************
		/* Output method implementations, textual
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		public override void writeString(string text)
		{
			_verifyValueWrite(WRITE_STRING);
			if (text == null)
			{
				_writeNull();
				return;
			}
			// First: if we can't guarantee it all fits, quoted, within output, offline
			int len = text.Length;
			if (len > _outputMaxContiguous)
			{
				// nope: off-line handling
				_writeStringSegments(text, true);
				return;
			}
			if ((_outputTail + len) >= _outputEnd)
			{
				_flushBuffer();
			}
			_outputBuffer[_outputTail++] = BYTE_QUOTE;
			_writeStringSegment(text, 0, len);
			// we checked space already above
			if (_outputTail >= _outputEnd)
			{
				_flushBuffer();
			}
			_outputBuffer[_outputTail++] = BYTE_QUOTE;
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeString(char[] text, int offset, int len)
		{
			_verifyValueWrite(WRITE_STRING);
			if (_outputTail >= _outputEnd)
			{
				_flushBuffer();
			}
			_outputBuffer[_outputTail++] = BYTE_QUOTE;
			// One or multiple segments?
			if (len <= _outputMaxContiguous)
			{
				if ((_outputTail + len) > _outputEnd)
				{
					// caller must ensure enough space
					_flushBuffer();
				}
				_writeStringSegment(text, offset, len);
			}
			else
			{
				_writeStringSegments(text, offset, len);
			}
			// And finally, closing quotes
			if (_outputTail >= _outputEnd)
			{
				_flushBuffer();
			}
			_outputBuffer[_outputTail++] = BYTE_QUOTE;
		}

		/// <exception cref="System.IO.IOException"/>
		public sealed override void writeString(com.fasterxml.jackson.core.SerializableString
			 text)
		{
			_verifyValueWrite(WRITE_STRING);
			if (_outputTail >= _outputEnd)
			{
				_flushBuffer();
			}
			_outputBuffer[_outputTail++] = BYTE_QUOTE;
			int len = text.appendQuotedUTF8(_outputBuffer, _outputTail);
			if (len < 0)
			{
				_writeBytes(text.asQuotedUTF8());
			}
			else
			{
				_outputTail += len;
			}
			if (_outputTail >= _outputEnd)
			{
				_flushBuffer();
			}
			_outputBuffer[_outputTail++] = BYTE_QUOTE;
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeRawUTF8String(byte[] text, int offset, int length)
		{
			_verifyValueWrite(WRITE_STRING);
			if (_outputTail >= _outputEnd)
			{
				_flushBuffer();
			}
			_outputBuffer[_outputTail++] = BYTE_QUOTE;
			_writeBytes(text, offset, length);
			if (_outputTail >= _outputEnd)
			{
				_flushBuffer();
			}
			_outputBuffer[_outputTail++] = BYTE_QUOTE;
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeUTF8String(byte[] text, int offset, int len)
		{
			_verifyValueWrite(WRITE_STRING);
			if (_outputTail >= _outputEnd)
			{
				_flushBuffer();
			}
			_outputBuffer[_outputTail++] = BYTE_QUOTE;
			// One or multiple segments?
			if (len <= _outputMaxContiguous)
			{
				_writeUTF8Segment(text, offset, len);
			}
			else
			{
				_writeUTF8Segments(text, offset, len);
			}
			if (_outputTail >= _outputEnd)
			{
				_flushBuffer();
			}
			_outputBuffer[_outputTail++] = BYTE_QUOTE;
		}

		/*
		/**********************************************************
		/* Output method implementations, unprocessed ("raw")
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		public override void writeRaw(string text)
		{
			int start = 0;
			int len = text.Length;
			while (len > 0)
			{
				char[] buf = _charBuffer;
				int blen = buf.Length;
				int len2 = (len < blen) ? len : blen;
				Sharpen.Runtime.getCharsForString(text, start, start + len2, buf, 0);
				writeRaw(buf, 0, len2);
				start += len2;
				len -= len2;
			}
		}

		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		public override void writeRaw(string text, int offset, int len)
		{
			while (len > 0)
			{
				char[] buf = _charBuffer;
				int blen = buf.Length;
				int len2 = (len < blen) ? len : blen;
				Sharpen.Runtime.getCharsForString(text, offset, offset + len2, buf, 0);
				writeRaw(buf, 0, len2);
				offset += len2;
				len -= len2;
			}
		}

		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		public override void writeRaw(com.fasterxml.jackson.core.SerializableString text)
		{
			byte[] raw = text.asUnquotedUTF8();
			if (raw.Length > 0)
			{
				_writeBytes(raw);
			}
		}

		// since 2.5
		/// <exception cref="System.IO.IOException"/>
		public override void writeRawValue(com.fasterxml.jackson.core.SerializableString 
			text)
		{
			_verifyValueWrite(WRITE_RAW);
			byte[] raw = text.asUnquotedUTF8();
			if (raw.Length > 0)
			{
				_writeBytes(raw);
			}
		}

		// @TODO: rewrite for speed...
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		public sealed override void writeRaw(char[] cbuf, int offset, int len)
		{
			{
				// First: if we have 3 x charCount spaces, we know it'll fit just fine
				int len3 = len + len + len;
				if ((_outputTail + len3) > _outputEnd)
				{
					// maybe we could flush?
					if (_outputEnd < len3)
					{
						// wouldn't be enough...
						_writeSegmentedRaw(cbuf, offset, len);
						return;
					}
					// yes, flushing brings enough space
					_flushBuffer();
				}
			}
			len += offset;
			// now marks the end
			// Note: here we know there is enough room, hence no output boundary checks
			while (offset < len)
			{
				while (true)
				{
					int ch = (int)cbuf[offset];
					if (ch > unchecked((int)(0x7F)))
					{
						goto inner_loop_break;
					}
					_outputBuffer[_outputTail++] = unchecked((byte)ch);
					if (++offset >= len)
					{
						goto main_loop_break;
					}
inner_loop_continue: ;
				}
inner_loop_break: ;
				char ch_1 = cbuf[offset++];
				if (ch_1 < unchecked((int)(0x800)))
				{
					// 2-byte?
					_outputBuffer[_outputTail++] = unchecked((byte)(unchecked((int)(0xc0)) | (ch_1 >>
						 6)));
					_outputBuffer[_outputTail++] = unchecked((byte)(unchecked((int)(0x80)) | (ch_1 & 
						unchecked((int)(0x3f)))));
				}
				else
				{
					offset = _outputRawMultiByteChar(ch_1, cbuf, offset, len);
				}
main_loop_continue: ;
			}
main_loop_break: ;
		}

		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		public override void writeRaw(char ch)
		{
			if ((_outputTail + 3) >= _outputEnd)
			{
				_flushBuffer();
			}
			byte[] bbuf = _outputBuffer;
			if (ch <= unchecked((int)(0x7F)))
			{
				bbuf[_outputTail++] = unchecked((byte)ch);
			}
			else
			{
				if (ch < unchecked((int)(0x800)))
				{
					// 2-byte?
					bbuf[_outputTail++] = unchecked((byte)(unchecked((int)(0xc0)) | (ch >> 6)));
					bbuf[_outputTail++] = unchecked((byte)(unchecked((int)(0x80)) | (ch & unchecked((
						int)(0x3f)))));
				}
				else
				{
					/*offset =*/
					_outputRawMultiByteChar(ch, null, 0, 0);
				}
			}
		}

		/// <summary>
		/// Helper method called when it is possible that output of raw section
		/// to output may cross buffer boundary
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		private void _writeSegmentedRaw(char[] cbuf, int offset, int len)
		{
			int end = _outputEnd;
			byte[] bbuf = _outputBuffer;
			while (offset < len)
			{
				while (true)
				{
					int ch = (int)cbuf[offset];
					if (ch >= unchecked((int)(0x80)))
					{
						goto inner_loop_break;
					}
					// !!! TODO: fast(er) writes (roll input, output checks in one)
					if (_outputTail >= end)
					{
						_flushBuffer();
					}
					bbuf[_outputTail++] = unchecked((byte)ch);
					if (++offset >= len)
					{
						goto main_loop_break;
					}
inner_loop_continue: ;
				}
inner_loop_break: ;
				if ((_outputTail + 3) >= _outputEnd)
				{
					_flushBuffer();
				}
				char ch_1 = cbuf[offset++];
				if (ch_1 < unchecked((int)(0x800)))
				{
					// 2-byte?
					bbuf[_outputTail++] = unchecked((byte)(unchecked((int)(0xc0)) | (ch_1 >> 6)));
					bbuf[_outputTail++] = unchecked((byte)(unchecked((int)(0x80)) | (ch_1 & unchecked(
						(int)(0x3f)))));
				}
				else
				{
					offset = _outputRawMultiByteChar(ch_1, cbuf, offset, len);
				}
main_loop_continue: ;
			}
main_loop_break: ;
		}

		/*
		/**********************************************************
		/* Output method implementations, base64-encoded binary
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		public override void writeBinary(com.fasterxml.jackson.core.Base64Variant b64variant
			, byte[] data, int offset, int len)
		{
			_verifyValueWrite(WRITE_BINARY);
			// Starting quotes
			if (_outputTail >= _outputEnd)
			{
				_flushBuffer();
			}
			_outputBuffer[_outputTail++] = BYTE_QUOTE;
			_writeBinary(b64variant, data, offset, offset + len);
			// and closing quotes
			if (_outputTail >= _outputEnd)
			{
				_flushBuffer();
			}
			_outputBuffer[_outputTail++] = BYTE_QUOTE;
		}

		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		public override int writeBinary(com.fasterxml.jackson.core.Base64Variant b64variant
			, Sharpen.InputStream data, int dataLength)
		{
			_verifyValueWrite(WRITE_BINARY);
			// Starting quotes
			if (_outputTail >= _outputEnd)
			{
				_flushBuffer();
			}
			_outputBuffer[_outputTail++] = BYTE_QUOTE;
			byte[] encodingBuffer = _ioContext.allocBase64Buffer();
			int bytes;
			try
			{
				if (dataLength < 0)
				{
					// length unknown
					bytes = _writeBinary(b64variant, data, encodingBuffer);
				}
				else
				{
					int missing = _writeBinary(b64variant, data, encodingBuffer, dataLength);
					if (missing > 0)
					{
						_reportError("Too few bytes available: missing " + missing + " bytes (out of " + 
							dataLength + ")");
					}
					bytes = dataLength;
				}
			}
			finally
			{
				_ioContext.releaseBase64Buffer(encodingBuffer);
			}
			// and closing quotes
			if (_outputTail >= _outputEnd)
			{
				_flushBuffer();
			}
			_outputBuffer[_outputTail++] = BYTE_QUOTE;
			return bytes;
		}

		/*
		/**********************************************************
		/* Output method implementations, primitive
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		public override void writeNumber(short s)
		{
			_verifyValueWrite(WRITE_NUMBER);
			// up to 5 digits and possible minus sign
			if ((_outputTail + 6) >= _outputEnd)
			{
				_flushBuffer();
			}
			if (_cfgNumbersAsStrings)
			{
				_writeQuotedShort(s);
				return;
			}
			_outputTail = com.fasterxml.jackson.core.io.NumberOutput.outputInt(s, _outputBuffer
				, _outputTail);
		}

		/// <exception cref="System.IO.IOException"/>
		private void _writeQuotedShort(short s)
		{
			if ((_outputTail + 8) >= _outputEnd)
			{
				_flushBuffer();
			}
			_outputBuffer[_outputTail++] = BYTE_QUOTE;
			_outputTail = com.fasterxml.jackson.core.io.NumberOutput.outputInt(s, _outputBuffer
				, _outputTail);
			_outputBuffer[_outputTail++] = BYTE_QUOTE;
		}

		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		public override void writeNumber(int i)
		{
			_verifyValueWrite(WRITE_NUMBER);
			// up to 10 digits and possible minus sign
			if ((_outputTail + 11) >= _outputEnd)
			{
				_flushBuffer();
			}
			if (_cfgNumbersAsStrings)
			{
				_writeQuotedInt(i);
				return;
			}
			_outputTail = com.fasterxml.jackson.core.io.NumberOutput.outputInt(i, _outputBuffer
				, _outputTail);
		}

		/// <exception cref="System.IO.IOException"/>
		private void _writeQuotedInt(int i)
		{
			if ((_outputTail + 13) >= _outputEnd)
			{
				_flushBuffer();
			}
			_outputBuffer[_outputTail++] = BYTE_QUOTE;
			_outputTail = com.fasterxml.jackson.core.io.NumberOutput.outputInt(i, _outputBuffer
				, _outputTail);
			_outputBuffer[_outputTail++] = BYTE_QUOTE;
		}

		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		public override void writeNumber(long l)
		{
			_verifyValueWrite(WRITE_NUMBER);
			if (_cfgNumbersAsStrings)
			{
				_writeQuotedLong(l);
				return;
			}
			if ((_outputTail + 21) >= _outputEnd)
			{
				// up to 20 digits, minus sign
				_flushBuffer();
			}
			_outputTail = com.fasterxml.jackson.core.io.NumberOutput.outputLong(l, _outputBuffer
				, _outputTail);
		}

		/// <exception cref="System.IO.IOException"/>
		private void _writeQuotedLong(long l)
		{
			if ((_outputTail + 23) >= _outputEnd)
			{
				_flushBuffer();
			}
			_outputBuffer[_outputTail++] = BYTE_QUOTE;
			_outputTail = com.fasterxml.jackson.core.io.NumberOutput.outputLong(l, _outputBuffer
				, _outputTail);
			_outputBuffer[_outputTail++] = BYTE_QUOTE;
		}

		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		public override void writeNumber(System.Numerics.BigInteger value)
		{
			_verifyValueWrite(WRITE_NUMBER);
			if (value == null)
			{
				_writeNull();
			}
			else
			{
				if (_cfgNumbersAsStrings)
				{
					_writeQuotedRaw(value.ToString());
				}
				else
				{
					writeRaw(value.ToString());
				}
			}
		}

		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		public override void writeNumber(double d)
		{
			if (_cfgNumbersAsStrings || (((double.IsNaN(d) || double.isInfinite(d)) && isEnabled
				(com.fasterxml.jackson.core.JsonGenerator.Feature.QUOTE_NON_NUMERIC_NUMBERS))))
			{
				// [JACKSON-139]
				writeString(d.ToString());
				return;
			}
			// What is the max length for doubles? 40 chars?
			_verifyValueWrite(WRITE_NUMBER);
			writeRaw(d.ToString());
		}

		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		public override void writeNumber(float f)
		{
			if (_cfgNumbersAsStrings || (((float.IsNaN(f) || float.isInfinite(f)) && isEnabled
				(com.fasterxml.jackson.core.JsonGenerator.Feature.QUOTE_NON_NUMERIC_NUMBERS))))
			{
				// [JACKSON-139]
				writeString(f.ToString());
				return;
			}
			// What is the max length for floats?
			_verifyValueWrite(WRITE_NUMBER);
			writeRaw(f.ToString());
		}

		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		public override void writeNumber(java.math.BigDecimal value)
		{
			// Don't really know max length for big decimal, no point checking
			_verifyValueWrite(WRITE_NUMBER);
			if (value == null)
			{
				_writeNull();
			}
			else
			{
				if (_cfgNumbersAsStrings)
				{
					string raw = isEnabled(com.fasterxml.jackson.core.JsonGenerator.Feature.WRITE_BIGDECIMAL_AS_PLAIN
						) ? value.toPlainString() : value.ToString();
					_writeQuotedRaw(raw);
				}
				else
				{
					if (isEnabled(com.fasterxml.jackson.core.JsonGenerator.Feature.WRITE_BIGDECIMAL_AS_PLAIN
						))
					{
						writeRaw(value.toPlainString());
					}
					else
					{
						writeRaw(value.ToString());
					}
				}
			}
		}

		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		public override void writeNumber(string encodedValue)
		{
			_verifyValueWrite(WRITE_NUMBER);
			if (_cfgNumbersAsStrings)
			{
				_writeQuotedRaw(encodedValue);
			}
			else
			{
				writeRaw(encodedValue);
			}
		}

		/// <exception cref="System.IO.IOException"/>
		private void _writeQuotedRaw(string value)
		{
			if (_outputTail >= _outputEnd)
			{
				_flushBuffer();
			}
			_outputBuffer[_outputTail++] = BYTE_QUOTE;
			writeRaw(value);
			if (_outputTail >= _outputEnd)
			{
				_flushBuffer();
			}
			_outputBuffer[_outputTail++] = BYTE_QUOTE;
		}

		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		public override void writeBoolean(bool state)
		{
			_verifyValueWrite(WRITE_BOOLEAN);
			if ((_outputTail + 5) >= _outputEnd)
			{
				_flushBuffer();
			}
			byte[] keyword = state ? TRUE_BYTES : FALSE_BYTES;
			int len = keyword.Length;
			System.Array.Copy(keyword, 0, _outputBuffer, _outputTail, len);
			_outputTail += len;
		}

		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		public override void writeNull()
		{
			_verifyValueWrite(WRITE_NULL);
			_writeNull();
		}

		/*
		/**********************************************************
		/* Implementations for other methods
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		protected internal sealed override void _verifyValueWrite(string typeMsg)
		{
			int status = _writeContext.writeValue();
			if (status == com.fasterxml.jackson.core.json.JsonWriteContext.STATUS_EXPECT_NAME)
			{
				_reportError("Can not " + typeMsg + ", expecting field name");
			}
			if (_cfgPrettyPrinter == null)
			{
				byte b;
				switch (status)
				{
					case com.fasterxml.jackson.core.json.JsonWriteContext.STATUS_OK_AFTER_COMMA:
					{
						b = BYTE_COMMA;
						break;
					}

					case com.fasterxml.jackson.core.json.JsonWriteContext.STATUS_OK_AFTER_COLON:
					{
						b = BYTE_COLON;
						break;
					}

					case com.fasterxml.jackson.core.json.JsonWriteContext.STATUS_OK_AFTER_SPACE:
					{
						// root-value separator
						if (_rootValueSeparator != null)
						{
							byte[] raw = _rootValueSeparator.asUnquotedUTF8();
							if (raw.Length > 0)
							{
								_writeBytes(raw);
							}
						}
						return;
					}

					case com.fasterxml.jackson.core.json.JsonWriteContext.STATUS_OK_AS_IS:
					default:
					{
						return;
					}
				}
				if (_outputTail >= _outputEnd)
				{
					_flushBuffer();
				}
				_outputBuffer[_outputTail] = b;
				++_outputTail;
				return;
			}
			// Otherwise, pretty printer knows what to do...
			_verifyPrettyValueWrite(typeMsg, status);
		}

		/// <exception cref="System.IO.IOException"/>
		protected internal void _verifyPrettyValueWrite(string typeMsg, int status)
		{
			switch (status)
			{
				case com.fasterxml.jackson.core.json.JsonWriteContext.STATUS_OK_AFTER_COMMA:
				{
					// If we have a pretty printer, it knows what to do:
					// array
					_cfgPrettyPrinter.writeArrayValueSeparator(this);
					break;
				}

				case com.fasterxml.jackson.core.json.JsonWriteContext.STATUS_OK_AFTER_COLON:
				{
					_cfgPrettyPrinter.writeObjectFieldValueSeparator(this);
					break;
				}

				case com.fasterxml.jackson.core.json.JsonWriteContext.STATUS_OK_AFTER_SPACE:
				{
					_cfgPrettyPrinter.writeRootValueSeparator(this);
					break;
				}

				case com.fasterxml.jackson.core.json.JsonWriteContext.STATUS_OK_AS_IS:
				{
					// First entry, but of which context?
					if (_writeContext.inArray())
					{
						_cfgPrettyPrinter.beforeArrayValues(this);
					}
					else
					{
						if (_writeContext.inObject())
						{
							_cfgPrettyPrinter.beforeObjectEntries(this);
						}
					}
					break;
				}

				default:
				{
					_throwInternal();
					break;
				}
			}
		}

		/*
		/**********************************************************
		/* Low-level output handling
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		public override void flush()
		{
			_flushBuffer();
			if (_outputStream != null)
			{
				if (isEnabled(com.fasterxml.jackson.core.JsonGenerator.Feature.FLUSH_PASSED_TO_STREAM
					))
				{
					_outputStream.flush();
				}
			}
		}

		/// <exception cref="System.IO.IOException"/>
		public override void close()
		{
			base.close();
			/* 05-Dec-2008, tatu: To add [JACKSON-27], need to close open
			*   scopes.
			*/
			// First: let's see that we still have buffers...
			if ((_outputBuffer != null) && isEnabled(com.fasterxml.jackson.core.JsonGenerator.Feature
				.AUTO_CLOSE_JSON_CONTENT))
			{
				while (true)
				{
					com.fasterxml.jackson.core.JsonStreamContext ctxt = ((com.fasterxml.jackson.core.json.JsonWriteContext
						)getOutputContext());
					if (ctxt.inArray())
					{
						writeEndArray();
					}
					else
					{
						if (ctxt.inObject())
						{
							writeEndObject();
						}
						else
						{
							break;
						}
					}
				}
			}
			_flushBuffer();
			_outputTail = 0;
			// just to ensure we don't think there's anything buffered
			/* 25-Nov-2008, tatus: As per [JACKSON-16] we are not to call close()
			*   on the underlying Reader, unless we "own" it, or auto-closing
			*   feature is enabled.
			*   One downside: when using UTF8Writer, underlying buffer(s)
			*   may not be properly recycled if we don't close the writer.
			*/
			if (_outputStream != null)
			{
				if (_ioContext.isResourceManaged() || isEnabled(com.fasterxml.jackson.core.JsonGenerator.Feature
					.AUTO_CLOSE_TARGET))
				{
					_outputStream.close();
				}
				else
				{
					if (isEnabled(com.fasterxml.jackson.core.JsonGenerator.Feature.FLUSH_PASSED_TO_STREAM
						))
					{
						// If we can't close it, we should at least flush
						_outputStream.flush();
					}
				}
			}
			// Internal buffer(s) generator has can now be released as well
			_releaseBuffers();
		}

		protected internal override void _releaseBuffers()
		{
			byte[] buf = _outputBuffer;
			if (buf != null && _bufferRecyclable)
			{
				_outputBuffer = null;
				_ioContext.releaseWriteEncodingBuffer(buf);
			}
			char[] cbuf = _charBuffer;
			if (cbuf != null)
			{
				_charBuffer = null;
				_ioContext.releaseConcatBuffer(cbuf);
			}
		}

		/*
		/**********************************************************
		/* Internal methods, low-level writing, raw bytes
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		private void _writeBytes(byte[] bytes)
		{
			int len = bytes.Length;
			if ((_outputTail + len) > _outputEnd)
			{
				_flushBuffer();
				// still not enough?
				if (len > MAX_BYTES_TO_BUFFER)
				{
					_outputStream.write(bytes, 0, len);
					return;
				}
			}
			System.Array.Copy(bytes, 0, _outputBuffer, _outputTail, len);
			_outputTail += len;
		}

		/// <exception cref="System.IO.IOException"/>
		private void _writeBytes(byte[] bytes, int offset, int len)
		{
			if ((_outputTail + len) > _outputEnd)
			{
				_flushBuffer();
				// still not enough?
				if (len > MAX_BYTES_TO_BUFFER)
				{
					_outputStream.write(bytes, offset, len);
					return;
				}
			}
			System.Array.Copy(bytes, offset, _outputBuffer, _outputTail, len);
			_outputTail += len;
		}

		/*
		/**********************************************************
		/* Internal methods, mid-level writing, String segments
		/**********************************************************
		*/
		/// <summary>
		/// Method called when String to write is long enough not to fit
		/// completely in temporary copy buffer.
		/// </summary>
		/// <remarks>
		/// Method called when String to write is long enough not to fit
		/// completely in temporary copy buffer. If so, we will actually
		/// copy it in small enough chunks so it can be directly fed
		/// to single-segment writes (instead of maximum slices that
		/// would fit in copy buffer)
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		private void _writeStringSegments(string text, bool addQuotes)
		{
			if (addQuotes)
			{
				if (_outputTail >= _outputEnd)
				{
					_flushBuffer();
				}
				_outputBuffer[_outputTail++] = BYTE_QUOTE;
			}
			int left = text.Length;
			int offset = 0;
			while (left > 0)
			{
				int len = System.Math.min(_outputMaxContiguous, left);
				if ((_outputTail + len) > _outputEnd)
				{
					// caller must ensure enough space
					_flushBuffer();
				}
				_writeStringSegment(text, offset, len);
				offset += len;
				left -= len;
			}
			if (addQuotes)
			{
				if (_outputTail >= _outputEnd)
				{
					_flushBuffer();
				}
				_outputBuffer[_outputTail++] = BYTE_QUOTE;
			}
		}

		/// <summary>
		/// Method called when character sequence to write is long enough that
		/// its maximum encoded and escaped form is not guaranteed to fit in
		/// the output buffer.
		/// </summary>
		/// <remarks>
		/// Method called when character sequence to write is long enough that
		/// its maximum encoded and escaped form is not guaranteed to fit in
		/// the output buffer. If so, we will need to choose smaller output
		/// chunks to write at a time.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		private void _writeStringSegments(char[] cbuf, int offset, int totalLen)
		{
			do
			{
				int len = System.Math.min(_outputMaxContiguous, totalLen);
				if ((_outputTail + len) > _outputEnd)
				{
					// caller must ensure enough space
					_flushBuffer();
				}
				_writeStringSegment(cbuf, offset, len);
				offset += len;
				totalLen -= len;
			}
			while (totalLen > 0);
		}

		/// <exception cref="System.IO.IOException"/>
		private void _writeStringSegments(string text, int offset, int totalLen)
		{
			do
			{
				int len = System.Math.min(_outputMaxContiguous, totalLen);
				if ((_outputTail + len) > _outputEnd)
				{
					// caller must ensure enough space
					_flushBuffer();
				}
				_writeStringSegment(text, offset, len);
				offset += len;
				totalLen -= len;
			}
			while (totalLen > 0);
		}

		/*
		/**********************************************************
		/* Internal methods, low-level writing, text segments
		/**********************************************************
		*/
		/// <summary>
		/// This method called when the string content is already in
		/// a char buffer, and its maximum total encoded and escaped length
		/// can not exceed size of the output buffer.
		/// </summary>
		/// <remarks>
		/// This method called when the string content is already in
		/// a char buffer, and its maximum total encoded and escaped length
		/// can not exceed size of the output buffer.
		/// Caller must ensure that there is enough space in output buffer,
		/// assuming case of all non-escaped ASCII characters, as well as
		/// potentially enough space for other cases (but not necessarily flushed)
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		private void _writeStringSegment(char[] cbuf, int offset, int len)
		{
			// note: caller MUST ensure (via flushing) there's room for ASCII only
			// Fast+tight loop for ASCII-only, no-escaping-needed output
			len += offset;
			// becomes end marker, then
			int outputPtr = _outputTail;
			byte[] outputBuffer = _outputBuffer;
			int[] escCodes = _outputEscapes;
			while (offset < len)
			{
				int ch = cbuf[offset];
				// note: here we know that (ch > 0x7F) will cover case of escaping non-ASCII too:
				if (ch > unchecked((int)(0x7F)) || escCodes[ch] != 0)
				{
					break;
				}
				outputBuffer[outputPtr++] = unchecked((byte)ch);
				++offset;
			}
			_outputTail = outputPtr;
			if (offset < len)
			{
				// [JACKSON-106]
				if (_characterEscapes != null)
				{
					_writeCustomStringSegment2(cbuf, offset, len);
				}
				else
				{
					// [JACKSON-102]
					if (_maximumNonEscapedChar == 0)
					{
						_writeStringSegment2(cbuf, offset, len);
					}
					else
					{
						_writeStringSegmentASCII2(cbuf, offset, len);
					}
				}
			}
		}

		/// <exception cref="System.IO.IOException"/>
		private void _writeStringSegment(string text, int offset, int len)
		{
			// note: caller MUST ensure (via flushing) there's room for ASCII only
			// Fast+tight loop for ASCII-only, no-escaping-needed output
			len += offset;
			// becomes end marker, then
			int outputPtr = _outputTail;
			byte[] outputBuffer = _outputBuffer;
			int[] escCodes = _outputEscapes;
			while (offset < len)
			{
				int ch = text[offset];
				// note: here we know that (ch > 0x7F) will cover case of escaping non-ASCII too:
				if (ch > unchecked((int)(0x7F)) || escCodes[ch] != 0)
				{
					break;
				}
				outputBuffer[outputPtr++] = unchecked((byte)ch);
				++offset;
			}
			_outputTail = outputPtr;
			if (offset < len)
			{
				if (_characterEscapes != null)
				{
					_writeCustomStringSegment2(text, offset, len);
				}
				else
				{
					if (_maximumNonEscapedChar == 0)
					{
						_writeStringSegment2(text, offset, len);
					}
					else
					{
						_writeStringSegmentASCII2(text, offset, len);
					}
				}
			}
		}

		/// <summary>
		/// Secondary method called when content contains characters to escape,
		/// and/or multi-byte UTF-8 characters.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		private void _writeStringSegment2(char[] cbuf, int offset, int end)
		{
			// Ok: caller guarantees buffer can have room; but that may require flushing:
			if ((_outputTail + 6 * (end - offset)) > _outputEnd)
			{
				_flushBuffer();
			}
			int outputPtr = _outputTail;
			byte[] outputBuffer = _outputBuffer;
			int[] escCodes = _outputEscapes;
			while (offset < end)
			{
				int ch = cbuf[offset++];
				if (ch <= unchecked((int)(0x7F)))
				{
					if (escCodes[ch] == 0)
					{
						outputBuffer[outputPtr++] = unchecked((byte)ch);
						continue;
					}
					int escape = escCodes[ch];
					if (escape > 0)
					{
						// 2-char escape, fine
						outputBuffer[outputPtr++] = BYTE_BACKSLASH;
						outputBuffer[outputPtr++] = unchecked((byte)escape);
					}
					else
					{
						// ctrl-char, 6-byte escape...
						outputPtr = _writeGenericEscape(ch, outputPtr);
					}
					continue;
				}
				if (ch <= unchecked((int)(0x7FF)))
				{
					// fine, just needs 2 byte output
					outputBuffer[outputPtr++] = unchecked((byte)(unchecked((int)(0xc0)) | (ch >> 6)));
					outputBuffer[outputPtr++] = unchecked((byte)(unchecked((int)(0x80)) | (ch & unchecked(
						(int)(0x3f)))));
				}
				else
				{
					outputPtr = _outputMultiByteChar(ch, outputPtr);
				}
			}
			_outputTail = outputPtr;
		}

		/// <exception cref="System.IO.IOException"/>
		private void _writeStringSegment2(string text, int offset, int end)
		{
			if ((_outputTail + 6 * (end - offset)) > _outputEnd)
			{
				_flushBuffer();
			}
			int outputPtr = _outputTail;
			byte[] outputBuffer = _outputBuffer;
			int[] escCodes = _outputEscapes;
			while (offset < end)
			{
				int ch = text[offset++];
				if (ch <= unchecked((int)(0x7F)))
				{
					if (escCodes[ch] == 0)
					{
						outputBuffer[outputPtr++] = unchecked((byte)ch);
						continue;
					}
					int escape = escCodes[ch];
					if (escape > 0)
					{
						// 2-char escape, fine
						outputBuffer[outputPtr++] = BYTE_BACKSLASH;
						outputBuffer[outputPtr++] = unchecked((byte)escape);
					}
					else
					{
						// ctrl-char, 6-byte escape...
						outputPtr = _writeGenericEscape(ch, outputPtr);
					}
					continue;
				}
				if (ch <= unchecked((int)(0x7FF)))
				{
					// fine, just needs 2 byte output
					outputBuffer[outputPtr++] = unchecked((byte)(unchecked((int)(0xc0)) | (ch >> 6)));
					outputBuffer[outputPtr++] = unchecked((byte)(unchecked((int)(0x80)) | (ch & unchecked(
						(int)(0x3f)))));
				}
				else
				{
					outputPtr = _outputMultiByteChar(ch, outputPtr);
				}
			}
			_outputTail = outputPtr;
		}

		/*
		/**********************************************************
		/* Internal methods, low-level writing, text segment
		/* with additional escaping (ASCII or such)
		/**********************************************************
		*/
		/// <summary>
		/// Same as <code>_writeStringSegment2(char[], ...)&lt;/code., but with
		/// additional escaping for high-range code points
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		private void _writeStringSegmentASCII2(char[] cbuf, int offset, int end)
		{
			// Ok: caller guarantees buffer can have room; but that may require flushing:
			if ((_outputTail + 6 * (end - offset)) > _outputEnd)
			{
				_flushBuffer();
			}
			int outputPtr = _outputTail;
			byte[] outputBuffer = _outputBuffer;
			int[] escCodes = _outputEscapes;
			int maxUnescaped = _maximumNonEscapedChar;
			while (offset < end)
			{
				int ch = cbuf[offset++];
				if (ch <= unchecked((int)(0x7F)))
				{
					if (escCodes[ch] == 0)
					{
						outputBuffer[outputPtr++] = unchecked((byte)ch);
						continue;
					}
					int escape = escCodes[ch];
					if (escape > 0)
					{
						// 2-char escape, fine
						outputBuffer[outputPtr++] = BYTE_BACKSLASH;
						outputBuffer[outputPtr++] = unchecked((byte)escape);
					}
					else
					{
						// ctrl-char, 6-byte escape...
						outputPtr = _writeGenericEscape(ch, outputPtr);
					}
					continue;
				}
				if (ch > maxUnescaped)
				{
					// [JACKSON-102] Allow forced escaping if non-ASCII (etc) chars:
					outputPtr = _writeGenericEscape(ch, outputPtr);
					continue;
				}
				if (ch <= unchecked((int)(0x7FF)))
				{
					// fine, just needs 2 byte output
					outputBuffer[outputPtr++] = unchecked((byte)(unchecked((int)(0xc0)) | (ch >> 6)));
					outputBuffer[outputPtr++] = unchecked((byte)(unchecked((int)(0x80)) | (ch & unchecked(
						(int)(0x3f)))));
				}
				else
				{
					outputPtr = _outputMultiByteChar(ch, outputPtr);
				}
			}
			_outputTail = outputPtr;
		}

		/// <exception cref="System.IO.IOException"/>
		private void _writeStringSegmentASCII2(string text, int offset, int end)
		{
			// Ok: caller guarantees buffer can have room; but that may require flushing:
			if ((_outputTail + 6 * (end - offset)) > _outputEnd)
			{
				_flushBuffer();
			}
			int outputPtr = _outputTail;
			byte[] outputBuffer = _outputBuffer;
			int[] escCodes = _outputEscapes;
			int maxUnescaped = _maximumNonEscapedChar;
			while (offset < end)
			{
				int ch = text[offset++];
				if (ch <= unchecked((int)(0x7F)))
				{
					if (escCodes[ch] == 0)
					{
						outputBuffer[outputPtr++] = unchecked((byte)ch);
						continue;
					}
					int escape = escCodes[ch];
					if (escape > 0)
					{
						// 2-char escape, fine
						outputBuffer[outputPtr++] = BYTE_BACKSLASH;
						outputBuffer[outputPtr++] = unchecked((byte)escape);
					}
					else
					{
						// ctrl-char, 6-byte escape...
						outputPtr = _writeGenericEscape(ch, outputPtr);
					}
					continue;
				}
				if (ch > maxUnescaped)
				{
					// [JACKSON-102] Allow forced escaping if non-ASCII (etc) chars:
					outputPtr = _writeGenericEscape(ch, outputPtr);
					continue;
				}
				if (ch <= unchecked((int)(0x7FF)))
				{
					// fine, just needs 2 byte output
					outputBuffer[outputPtr++] = unchecked((byte)(unchecked((int)(0xc0)) | (ch >> 6)));
					outputBuffer[outputPtr++] = unchecked((byte)(unchecked((int)(0x80)) | (ch & unchecked(
						(int)(0x3f)))));
				}
				else
				{
					outputPtr = _outputMultiByteChar(ch, outputPtr);
				}
			}
			_outputTail = outputPtr;
		}

		/*
		/**********************************************************
		/* Internal methods, low-level writing, text segment
		/* with fully custom escaping (and possibly escaping of non-ASCII
		/**********************************************************
		*/
		/// <summary>
		/// Same as <code>_writeStringSegmentASCII2(char[], ...)&lt;/code., but with
		/// additional checking for completely custom escapes
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		private void _writeCustomStringSegment2(char[] cbuf, int offset, int end)
		{
			// Ok: caller guarantees buffer can have room; but that may require flushing:
			if ((_outputTail + 6 * (end - offset)) > _outputEnd)
			{
				_flushBuffer();
			}
			int outputPtr = _outputTail;
			byte[] outputBuffer = _outputBuffer;
			int[] escCodes = _outputEscapes;
			// may or may not have this limit
			int maxUnescaped = (_maximumNonEscapedChar <= 0) ? unchecked((int)(0xFFFF)) : _maximumNonEscapedChar;
			com.fasterxml.jackson.core.io.CharacterEscapes customEscapes = _characterEscapes;
			// non-null
			while (offset < end)
			{
				int ch = cbuf[offset++];
				if (ch <= unchecked((int)(0x7F)))
				{
					if (escCodes[ch] == 0)
					{
						outputBuffer[outputPtr++] = unchecked((byte)ch);
						continue;
					}
					int escape = escCodes[ch];
					if (escape > 0)
					{
						// 2-char escape, fine
						outputBuffer[outputPtr++] = BYTE_BACKSLASH;
						outputBuffer[outputPtr++] = unchecked((byte)escape);
					}
					else
					{
						if (escape == com.fasterxml.jackson.core.io.CharacterEscapes.ESCAPE_CUSTOM)
						{
							com.fasterxml.jackson.core.SerializableString esc = customEscapes.getEscapeSequence
								(ch);
							if (esc == null)
							{
								_reportError("Invalid custom escape definitions; custom escape not found for character code 0x"
									 + Sharpen.Extensions.ToHexString(ch) + ", although was supposed to have one");
							}
							outputPtr = _writeCustomEscape(outputBuffer, outputPtr, esc, end - offset);
						}
						else
						{
							// ctrl-char, 6-byte escape...
							outputPtr = _writeGenericEscape(ch, outputPtr);
						}
					}
					continue;
				}
				if (ch > maxUnescaped)
				{
					// [JACKSON-102] Allow forced escaping if non-ASCII (etc) chars:
					outputPtr = _writeGenericEscape(ch, outputPtr);
					continue;
				}
				com.fasterxml.jackson.core.SerializableString esc_1 = customEscapes.getEscapeSequence
					(ch);
				if (esc_1 != null)
				{
					outputPtr = _writeCustomEscape(outputBuffer, outputPtr, esc_1, end - offset);
					continue;
				}
				if (ch <= unchecked((int)(0x7FF)))
				{
					// fine, just needs 2 byte output
					outputBuffer[outputPtr++] = unchecked((byte)(unchecked((int)(0xc0)) | (ch >> 6)));
					outputBuffer[outputPtr++] = unchecked((byte)(unchecked((int)(0x80)) | (ch & unchecked(
						(int)(0x3f)))));
				}
				else
				{
					outputPtr = _outputMultiByteChar(ch, outputPtr);
				}
			}
			_outputTail = outputPtr;
		}

		/// <exception cref="System.IO.IOException"/>
		private void _writeCustomStringSegment2(string text, int offset, int end)
		{
			// Ok: caller guarantees buffer can have room; but that may require flushing:
			if ((_outputTail + 6 * (end - offset)) > _outputEnd)
			{
				_flushBuffer();
			}
			int outputPtr = _outputTail;
			byte[] outputBuffer = _outputBuffer;
			int[] escCodes = _outputEscapes;
			// may or may not have this limit
			int maxUnescaped = (_maximumNonEscapedChar <= 0) ? unchecked((int)(0xFFFF)) : _maximumNonEscapedChar;
			com.fasterxml.jackson.core.io.CharacterEscapes customEscapes = _characterEscapes;
			// non-null
			while (offset < end)
			{
				int ch = text[offset++];
				if (ch <= unchecked((int)(0x7F)))
				{
					if (escCodes[ch] == 0)
					{
						outputBuffer[outputPtr++] = unchecked((byte)ch);
						continue;
					}
					int escape = escCodes[ch];
					if (escape > 0)
					{
						// 2-char escape, fine
						outputBuffer[outputPtr++] = BYTE_BACKSLASH;
						outputBuffer[outputPtr++] = unchecked((byte)escape);
					}
					else
					{
						if (escape == com.fasterxml.jackson.core.io.CharacterEscapes.ESCAPE_CUSTOM)
						{
							com.fasterxml.jackson.core.SerializableString esc = customEscapes.getEscapeSequence
								(ch);
							if (esc == null)
							{
								_reportError("Invalid custom escape definitions; custom escape not found for character code 0x"
									 + Sharpen.Extensions.ToHexString(ch) + ", although was supposed to have one");
							}
							outputPtr = _writeCustomEscape(outputBuffer, outputPtr, esc, end - offset);
						}
						else
						{
							// ctrl-char, 6-byte escape...
							outputPtr = _writeGenericEscape(ch, outputPtr);
						}
					}
					continue;
				}
				if (ch > maxUnescaped)
				{
					// [JACKSON-102] Allow forced escaping if non-ASCII (etc) chars:
					outputPtr = _writeGenericEscape(ch, outputPtr);
					continue;
				}
				com.fasterxml.jackson.core.SerializableString esc_1 = customEscapes.getEscapeSequence
					(ch);
				if (esc_1 != null)
				{
					outputPtr = _writeCustomEscape(outputBuffer, outputPtr, esc_1, end - offset);
					continue;
				}
				if (ch <= unchecked((int)(0x7FF)))
				{
					// fine, just needs 2 byte output
					outputBuffer[outputPtr++] = unchecked((byte)(unchecked((int)(0xc0)) | (ch >> 6)));
					outputBuffer[outputPtr++] = unchecked((byte)(unchecked((int)(0x80)) | (ch & unchecked(
						(int)(0x3f)))));
				}
				else
				{
					outputPtr = _outputMultiByteChar(ch, outputPtr);
				}
			}
			_outputTail = outputPtr;
		}

		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		private int _writeCustomEscape(byte[] outputBuffer, int outputPtr, com.fasterxml.jackson.core.SerializableString
			 esc, int remainingChars)
		{
			byte[] raw = esc.asUnquotedUTF8();
			// must be escaped at this point, shouldn't double-quote
			int len = raw.Length;
			if (len > 6)
			{
				// may violate constraints we have, do offline
				return _handleLongCustomEscape(outputBuffer, outputPtr, _outputEnd, raw, remainingChars
					);
			}
			// otherwise will fit without issues, so:
			System.Array.Copy(raw, 0, outputBuffer, outputPtr, len);
			return (outputPtr + len);
		}

		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		private int _handleLongCustomEscape(byte[] outputBuffer, int outputPtr, int outputEnd
			, byte[] raw, int remainingChars)
		{
			int len = raw.Length;
			if ((outputPtr + len) > outputEnd)
			{
				_outputTail = outputPtr;
				_flushBuffer();
				outputPtr = _outputTail;
				if (len > outputBuffer.Length)
				{
					// very unlikely, but possible...
					_outputStream.write(raw, 0, len);
					return outputPtr;
				}
				System.Array.Copy(raw, 0, outputBuffer, outputPtr, len);
				outputPtr += len;
			}
			// but is the invariant still obeyed? If not, flush once more
			if ((outputPtr + 6 * remainingChars) > outputEnd)
			{
				_flushBuffer();
				return _outputTail;
			}
			return outputPtr;
		}

		/*
		/**********************************************************
		/* Internal methods, low-level writing, "raw UTF-8" segments
		/**********************************************************
		*/
		/// <summary>
		/// Method called when UTF-8 encoded (but NOT yet escaped!) content is not guaranteed
		/// to fit in the output buffer after escaping; as such, we just need to
		/// chunk writes.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		private void _writeUTF8Segments(byte[] utf8, int offset, int totalLen)
		{
			do
			{
				int len = System.Math.min(_outputMaxContiguous, totalLen);
				_writeUTF8Segment(utf8, offset, len);
				offset += len;
				totalLen -= len;
			}
			while (totalLen > 0);
		}

		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		private void _writeUTF8Segment(byte[] utf8, int offset, int len)
		{
			// fast loop to see if escaping is needed; don't copy, just look
			int[] escCodes = _outputEscapes;
			for (int ptr = offset; ptr < end; )
			{
				// 28-Feb-2011, tatu: escape codes just cover 7-bit range, so:
				int ch = utf8[ptr++];
				if ((ch >= 0) && escCodes[ch] != 0)
				{
					_writeUTF8Segment2(utf8, offset, len);
					return;
				}
			}
			// yes, fine, just copy the sucker
			if ((_outputTail + len) > _outputEnd)
			{
				// enough room or need to flush?
				_flushBuffer();
			}
			// but yes once we flush (caller guarantees length restriction)
			System.Array.Copy(utf8, offset, _outputBuffer, _outputTail, len);
			_outputTail += len;
		}

		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		private void _writeUTF8Segment2(byte[] utf8, int offset, int len)
		{
			int outputPtr = _outputTail;
			// Ok: caller guarantees buffer can have room; but that may require flushing:
			if ((outputPtr + (len * 6)) > _outputEnd)
			{
				_flushBuffer();
				outputPtr = _outputTail;
			}
			byte[] outputBuffer = _outputBuffer;
			int[] escCodes = _outputEscapes;
			len += offset;
			// so 'len' becomes 'end'
			while (offset < len)
			{
				byte b = utf8[offset++];
				int ch = b;
				if (ch < 0 || escCodes[ch] == 0)
				{
					outputBuffer[outputPtr++] = b;
					continue;
				}
				int escape = escCodes[ch];
				if (escape > 0)
				{
					// 2-char escape, fine
					outputBuffer[outputPtr++] = BYTE_BACKSLASH;
					outputBuffer[outputPtr++] = unchecked((byte)escape);
				}
				else
				{
					// ctrl-char, 6-byte escape...
					outputPtr = _writeGenericEscape(ch, outputPtr);
				}
			}
			_outputTail = outputPtr;
		}

		/*
		/**********************************************************
		/* Internal methods, low-level writing, base64 encoded
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		protected internal void _writeBinary(com.fasterxml.jackson.core.Base64Variant b64variant
			, byte[] input, int inputPtr, int inputEnd)
		{
			// Encoding is by chunks of 3 input, 4 output chars, so:
			int safeInputEnd = inputEnd - 3;
			// Let's also reserve room for possible (and quoted) lf char each round
			int safeOutputEnd = _outputEnd - 6;
			int chunksBeforeLF = b64variant.getMaxLineLength() >> 2;
			// Ok, first we loop through all full triplets of data:
			while (inputPtr <= safeInputEnd)
			{
				if (_outputTail > safeOutputEnd)
				{
					// need to flush
					_flushBuffer();
				}
				// First, mash 3 bytes into lsb of 32-bit int
				int b24 = ((int)input[inputPtr++]) << 8;
				b24 |= ((int)input[inputPtr++]) & unchecked((int)(0xFF));
				b24 = (b24 << 8) | (((int)input[inputPtr++]) & unchecked((int)(0xFF)));
				_outputTail = b64variant.encodeBase64Chunk(b24, _outputBuffer, _outputTail);
				if (--chunksBeforeLF <= 0)
				{
					// note: must quote in JSON value
					_outputBuffer[_outputTail++] = (byte)('\\');
					_outputBuffer[_outputTail++] = (byte)('n');
					chunksBeforeLF = b64variant.getMaxLineLength() >> 2;
				}
			}
			// And then we may have 1 or 2 leftover bytes to encode
			int inputLeft = inputEnd - inputPtr;
			// 0, 1 or 2
			if (inputLeft > 0)
			{
				// yes, but do we have room for output?
				if (_outputTail > safeOutputEnd)
				{
					// don't really need 6 bytes but...
					_flushBuffer();
				}
				int b24 = ((int)input[inputPtr++]) << 16;
				if (inputLeft == 2)
				{
					b24 |= (((int)input[inputPtr++]) & unchecked((int)(0xFF))) << 8;
				}
				_outputTail = b64variant.encodeBase64Partial(b24, inputLeft, _outputBuffer, _outputTail
					);
			}
		}

		// write-method called when length is definitely known
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		protected internal int _writeBinary(com.fasterxml.jackson.core.Base64Variant b64variant
			, Sharpen.InputStream data, byte[] readBuffer, int bytesLeft)
		{
			int inputPtr = 0;
			int inputEnd = 0;
			int lastFullOffset = -3;
			// Let's also reserve room for possible (and quoted) LF char each round
			int safeOutputEnd = _outputEnd - 6;
			int chunksBeforeLF = b64variant.getMaxLineLength() >> 2;
			while (bytesLeft > 2)
			{
				// main loop for full triplets
				if (inputPtr > lastFullOffset)
				{
					inputEnd = _readMore(data, readBuffer, inputPtr, inputEnd, bytesLeft);
					inputPtr = 0;
					if (inputEnd < 3)
					{
						// required to try to read to have at least 3 bytes
						break;
					}
					lastFullOffset = inputEnd - 3;
				}
				if (_outputTail > safeOutputEnd)
				{
					// need to flush
					_flushBuffer();
				}
				int b24 = ((int)readBuffer[inputPtr++]) << 8;
				b24 |= ((int)readBuffer[inputPtr++]) & unchecked((int)(0xFF));
				b24 = (b24 << 8) | (((int)readBuffer[inputPtr++]) & unchecked((int)(0xFF)));
				bytesLeft -= 3;
				_outputTail = b64variant.encodeBase64Chunk(b24, _outputBuffer, _outputTail);
				if (--chunksBeforeLF <= 0)
				{
					_outputBuffer[_outputTail++] = (byte)('\\');
					_outputBuffer[_outputTail++] = (byte)('n');
					chunksBeforeLF = b64variant.getMaxLineLength() >> 2;
				}
			}
			// And then we may have 1 or 2 leftover bytes to encode
			if (bytesLeft > 0)
			{
				inputEnd = _readMore(data, readBuffer, inputPtr, inputEnd, bytesLeft);
				inputPtr = 0;
				if (inputEnd > 0)
				{
					// yes, but do we have room for output?
					if (_outputTail > safeOutputEnd)
					{
						// don't really need 6 bytes but...
						_flushBuffer();
					}
					int b24 = ((int)readBuffer[inputPtr++]) << 16;
					int amount;
					if (inputPtr < inputEnd)
					{
						b24 |= (((int)readBuffer[inputPtr]) & unchecked((int)(0xFF))) << 8;
						amount = 2;
					}
					else
					{
						amount = 1;
					}
					_outputTail = b64variant.encodeBase64Partial(b24, amount, _outputBuffer, _outputTail
						);
					bytesLeft -= amount;
				}
			}
			return bytesLeft;
		}

		// write method when length is unknown
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		protected internal int _writeBinary(com.fasterxml.jackson.core.Base64Variant b64variant
			, Sharpen.InputStream data, byte[] readBuffer)
		{
			int inputPtr = 0;
			int inputEnd = 0;
			int lastFullOffset = -3;
			int bytesDone = 0;
			// Let's also reserve room for possible (and quoted) LF char each round
			int safeOutputEnd = _outputEnd - 6;
			int chunksBeforeLF = b64variant.getMaxLineLength() >> 2;
			// Ok, first we loop through all full triplets of data:
			while (true)
			{
				if (inputPtr > lastFullOffset)
				{
					// need to load more
					inputEnd = _readMore(data, readBuffer, inputPtr, inputEnd, readBuffer.Length);
					inputPtr = 0;
					if (inputEnd < 3)
					{
						// required to try to read to have at least 3 bytes
						break;
					}
					lastFullOffset = inputEnd - 3;
				}
				if (_outputTail > safeOutputEnd)
				{
					// need to flush
					_flushBuffer();
				}
				// First, mash 3 bytes into lsb of 32-bit int
				int b24 = ((int)readBuffer[inputPtr++]) << 8;
				b24 |= ((int)readBuffer[inputPtr++]) & unchecked((int)(0xFF));
				b24 = (b24 << 8) | (((int)readBuffer[inputPtr++]) & unchecked((int)(0xFF)));
				bytesDone += 3;
				_outputTail = b64variant.encodeBase64Chunk(b24, _outputBuffer, _outputTail);
				if (--chunksBeforeLF <= 0)
				{
					_outputBuffer[_outputTail++] = (byte)('\\');
					_outputBuffer[_outputTail++] = (byte)('n');
					chunksBeforeLF = b64variant.getMaxLineLength() >> 2;
				}
			}
			// And then we may have 1 or 2 leftover bytes to encode
			if (inputPtr < inputEnd)
			{
				// yes, but do we have room for output?
				if (_outputTail > safeOutputEnd)
				{
					// don't really need 6 bytes but...
					_flushBuffer();
				}
				int b24 = ((int)readBuffer[inputPtr++]) << 16;
				int amount = 1;
				if (inputPtr < inputEnd)
				{
					b24 |= (((int)readBuffer[inputPtr]) & unchecked((int)(0xFF))) << 8;
					amount = 2;
				}
				bytesDone += amount;
				_outputTail = b64variant.encodeBase64Partial(b24, amount, _outputBuffer, _outputTail
					);
			}
			return bytesDone;
		}

		/// <exception cref="System.IO.IOException"/>
		private int _readMore(Sharpen.InputStream @in, byte[] readBuffer, int inputPtr, int
			 inputEnd, int maxRead)
		{
			// anything to shift to front?
			int i = 0;
			while (inputPtr < inputEnd)
			{
				readBuffer[i++] = readBuffer[inputPtr++];
			}
			inputPtr = 0;
			inputEnd = i;
			maxRead = System.Math.min(maxRead, readBuffer.Length);
			do
			{
				int length = maxRead - inputEnd;
				if (length == 0)
				{
					break;
				}
				int count = @in.read(readBuffer, inputEnd, length);
				if (count < 0)
				{
					return inputEnd;
				}
				inputEnd += count;
			}
			while (inputEnd < 3);
			return inputEnd;
		}

		/*
		/**********************************************************
		/* Internal methods, character escapes/encoding
		/**********************************************************
		*/
		/// <summary>
		/// Method called to output a character that is beyond range of
		/// 1- and 2-byte UTF-8 encodings, when outputting "raw"
		/// text (meaning it is not to be escaped or quoted)
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		private int _outputRawMultiByteChar(int ch, char[] cbuf, int inputOffset, int inputLen
			)
		{
			// Let's handle surrogates gracefully (as 4 byte output):
			if (ch >= SURR1_FIRST)
			{
				if (ch <= SURR2_LAST)
				{
					// yes, outside of BMP
					// Do we have second part?
					if (inputOffset >= inputLen || cbuf == null)
					{
						// nope... have to note down
						_reportError("Split surrogate on writeRaw() input (last character)");
					}
					_outputSurrogates(ch, cbuf[inputOffset]);
					return (inputOffset + 1);
				}
			}
			byte[] bbuf = _outputBuffer;
			bbuf[_outputTail++] = unchecked((byte)(unchecked((int)(0xe0)) | (ch >> 12)));
			bbuf[_outputTail++] = unchecked((byte)(unchecked((int)(0x80)) | ((ch >> 6) & unchecked(
				(int)(0x3f)))));
			bbuf[_outputTail++] = unchecked((byte)(unchecked((int)(0x80)) | (ch & unchecked((
				int)(0x3f)))));
			return inputOffset;
		}

		/// <exception cref="System.IO.IOException"/>
		protected internal void _outputSurrogates(int surr1, int surr2)
		{
			int c = _decodeSurrogate(surr1, surr2);
			if ((_outputTail + 4) > _outputEnd)
			{
				_flushBuffer();
			}
			byte[] bbuf = _outputBuffer;
			bbuf[_outputTail++] = unchecked((byte)(unchecked((int)(0xf0)) | (c >> 18)));
			bbuf[_outputTail++] = unchecked((byte)(unchecked((int)(0x80)) | ((c >> 12) & unchecked(
				(int)(0x3f)))));
			bbuf[_outputTail++] = unchecked((byte)(unchecked((int)(0x80)) | ((c >> 6) & unchecked(
				(int)(0x3f)))));
			bbuf[_outputTail++] = unchecked((byte)(unchecked((int)(0x80)) | (c & unchecked((int
				)(0x3f)))));
		}

		/// <param name="ch"/>
		/// <param name="outputPtr">Position within output buffer to append multi-byte in</param>
		/// <returns>New output position after appending</returns>
		/// <exception cref="System.IO.IOException"/>
		private int _outputMultiByteChar(int ch, int outputPtr)
		{
			byte[] bbuf = _outputBuffer;
			if (ch >= SURR1_FIRST && ch <= SURR2_LAST)
			{
				// yes, outside of BMP; add an escape
				bbuf[outputPtr++] = BYTE_BACKSLASH;
				bbuf[outputPtr++] = BYTE_u;
				bbuf[outputPtr++] = HEX_CHARS[(ch >> 12) & unchecked((int)(0xF))];
				bbuf[outputPtr++] = HEX_CHARS[(ch >> 8) & unchecked((int)(0xF))];
				bbuf[outputPtr++] = HEX_CHARS[(ch >> 4) & unchecked((int)(0xF))];
				bbuf[outputPtr++] = HEX_CHARS[ch & unchecked((int)(0xF))];
			}
			else
			{
				bbuf[outputPtr++] = unchecked((byte)(unchecked((int)(0xe0)) | (ch >> 12)));
				bbuf[outputPtr++] = unchecked((byte)(unchecked((int)(0x80)) | ((ch >> 6) & unchecked(
					(int)(0x3f)))));
				bbuf[outputPtr++] = unchecked((byte)(unchecked((int)(0x80)) | (ch & unchecked((int
					)(0x3f)))));
			}
			return outputPtr;
		}

		/// <exception cref="System.IO.IOException"/>
		private void _writeNull()
		{
			if ((_outputTail + 4) >= _outputEnd)
			{
				_flushBuffer();
			}
			System.Array.Copy(NULL_BYTES, 0, _outputBuffer, _outputTail, 4);
			_outputTail += 4;
		}

		/// <summary>Method called to write a generic Unicode escape for given character.</summary>
		/// <param name="charToEscape">Character to escape using escape sequence (\\uXXXX)</param>
		/// <exception cref="System.IO.IOException"/>
		private int _writeGenericEscape(int charToEscape, int outputPtr)
		{
			byte[] bbuf = _outputBuffer;
			bbuf[outputPtr++] = BYTE_BACKSLASH;
			bbuf[outputPtr++] = BYTE_u;
			if (charToEscape > unchecked((int)(0xFF)))
			{
				int hi = (charToEscape >> 8) & unchecked((int)(0xFF));
				bbuf[outputPtr++] = HEX_CHARS[hi >> 4];
				bbuf[outputPtr++] = HEX_CHARS[hi & unchecked((int)(0xF))];
				charToEscape &= unchecked((int)(0xFF));
			}
			else
			{
				bbuf[outputPtr++] = BYTE_0;
				bbuf[outputPtr++] = BYTE_0;
			}
			// We know it's a control char, so only the last 2 chars are non-0
			bbuf[outputPtr++] = HEX_CHARS[charToEscape >> 4];
			bbuf[outputPtr++] = HEX_CHARS[charToEscape & unchecked((int)(0xF))];
			return outputPtr;
		}

		/// <exception cref="System.IO.IOException"/>
		protected internal void _flushBuffer()
		{
			int len = _outputTail;
			if (len > 0)
			{
				_outputTail = 0;
				_outputStream.write(_outputBuffer, 0, len);
			}
		}
	}
}
