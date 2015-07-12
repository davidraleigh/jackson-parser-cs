using Sharpen;

namespace com.fasterxml.jackson.core.@base
{
	/// <summary>
	/// Intermediate base class used by all Jackson
	/// <see cref="com.fasterxml.jackson.core.JsonParser"/>
	/// implementations. Contains most common things that are independent
	/// of actual underlying input source.
	/// </summary>
	public abstract class ParserBase : com.fasterxml.jackson.core.@base.ParserMinimalBase
	{
		/// <summary>I/O context for this reader.</summary>
		/// <remarks>
		/// I/O context for this reader. It handles buffer allocation
		/// for the reader.
		/// </remarks>
		protected internal readonly com.fasterxml.jackson.core.io.IOContext _ioContext;

		/// <summary>Flag that indicates whether parser is closed or not.</summary>
		/// <remarks>
		/// Flag that indicates whether parser is closed or not. Gets
		/// set when parser is either closed by explicit call
		/// (
		/// <see cref="close()"/>
		/// ) or when end-of-input is reached.
		/// </remarks>
		protected internal bool _closed;

		/// <summary>Pointer to next available character in buffer</summary>
		protected internal int _inputPtr = 0;

		/// <summary>Index of character after last available one in the buffer.</summary>
		protected internal int _inputEnd = 0;

		/// <summary>
		/// Number of characters/bytes that were contained in previous blocks
		/// (blocks that were already processed prior to the current buffer).
		/// </summary>
		protected internal long _currInputProcessed = 0L;

		/// <summary>
		/// Current row location of current point in input buffer, starting
		/// from 1, if available.
		/// </summary>
		protected internal int _currInputRow = 1;

		/// <summary>
		/// Current index of the first character of the current row in input
		/// buffer.
		/// </summary>
		/// <remarks>
		/// Current index of the first character of the current row in input
		/// buffer. Needed to calculate column position, if necessary; benefit
		/// of not having column itself is that this only has to be updated
		/// once per line.
		/// </remarks>
		protected internal int _currInputRowStart = 0;

		/// <summary>Total number of bytes/characters read before start of current token.</summary>
		/// <remarks>
		/// Total number of bytes/characters read before start of current token.
		/// For big (gigabyte-sized) sizes are possible, needs to be long,
		/// unlike pointers and sizes related to in-memory buffers.
		/// </remarks>
		protected internal long _tokenInputTotal = 0;

		/// <summary>Input row on which current token starts, 1-based</summary>
		protected internal int _tokenInputRow = 1;

		/// <summary>
		/// Column on input row that current token starts; 0-based (although
		/// in the end it'll be converted to 1-based)
		/// </summary>
		protected internal int _tokenInputCol = 0;

		/// <summary>
		/// Information about parser context, context in which
		/// the next token is to be parsed (root, array, object).
		/// </summary>
		protected internal com.fasterxml.jackson.core.json.JsonReadContext _parsingContext;

		/// <summary>
		/// Secondary token related to the next token after current one;
		/// used if its type is known.
		/// </summary>
		/// <remarks>
		/// Secondary token related to the next token after current one;
		/// used if its type is known. This may be value token that
		/// follows FIELD_NAME, for example.
		/// </remarks>
		protected internal com.fasterxml.jackson.core.JsonToken _nextToken;

		/// <summary>
		/// Buffer that contains contents of String values, including
		/// field names if necessary (name split across boundary,
		/// contains escape sequence, or access needed to char array)
		/// </summary>
		protected internal readonly com.fasterxml.jackson.core.util.TextBuffer _textBuffer;

		/// <summary>
		/// Temporary buffer that is needed if field name is accessed
		/// using
		/// <see cref="ParserMinimalBase.getTextCharacters()"/>
		/// method (instead of String
		/// returning alternatives)
		/// </summary>
		protected internal char[] _nameCopyBuffer = null;

		/// <summary>
		/// Flag set to indicate whether the field name is available
		/// from the name copy buffer or not (in addition to its String
		/// representation  being available via read context)
		/// </summary>
		protected internal bool _nameCopied = false;

		/// <summary>ByteArrayBuilder is needed if 'getBinaryValue' is called.</summary>
		/// <remarks>
		/// ByteArrayBuilder is needed if 'getBinaryValue' is called. If so,
		/// we better reuse it for remainder of content.
		/// </remarks>
		protected internal com.fasterxml.jackson.core.util.ByteArrayBuilder _byteArrayBuilder
			 = null;

		/// <summary>
		/// We will hold on to decoded binary data, for duration of
		/// current event, so that multiple calls to
		/// <see cref="com.fasterxml.jackson.core.JsonParser.getBinaryValue()"/>
		/// will not need to decode data more
		/// than once.
		/// </summary>
		protected internal byte[] _binaryValue;

		protected internal const int NR_UNKNOWN = 0;

		protected internal const int NR_INT = unchecked((int)(0x0001));

		protected internal const int NR_LONG = unchecked((int)(0x0002));

		protected internal const int NR_BIGINT = unchecked((int)(0x0004));

		protected internal const int NR_DOUBLE = unchecked((int)(0x008));

		protected internal const int NR_BIGDECIMAL = unchecked((int)(0x0010));

		internal static readonly System.Numerics.BigInteger BI_MIN_INT = System.Convert.ToInt64
			(int.MinValue);

		internal static readonly System.Numerics.BigInteger BI_MAX_INT = System.Convert.ToInt64
			(int.MaxValue);

		internal static readonly System.Numerics.BigInteger BI_MIN_LONG = System.Convert.ToInt64
			(long.MinValue);

		internal static readonly System.Numerics.BigInteger BI_MAX_LONG = System.Convert.ToInt64
			(long.MaxValue);

		internal static readonly java.math.BigDecimal BD_MIN_LONG = new java.math.BigDecimal
			(BI_MIN_LONG);

		internal static readonly java.math.BigDecimal BD_MAX_LONG = new java.math.BigDecimal
			(BI_MAX_LONG);

		internal static readonly java.math.BigDecimal BD_MIN_INT = new java.math.BigDecimal
			(BI_MIN_INT);

		internal static readonly java.math.BigDecimal BD_MAX_INT = new java.math.BigDecimal
			(BI_MAX_INT);

		internal const long MIN_INT_L = (long)int.MinValue;

		internal const long MAX_INT_L = (long)int.MaxValue;

		internal const double MIN_LONG_D = (double)long.MinValue;

		internal const double MAX_LONG_D = (double)long.MaxValue;

		internal const double MIN_INT_D = (double)int.MinValue;

		internal const double MAX_INT_D = (double)int.MaxValue;

		protected internal const int INT_0 = '0';

		protected internal const int INT_9 = '9';

		protected internal const int INT_MINUS = '-';

		protected internal const int INT_PLUS = '+';

		protected internal const char CHAR_NULL = '\0';

		/// <summary>
		/// Bitfield that indicates which numeric representations
		/// have been calculated for the current type
		/// </summary>
		protected internal int _numTypesValid = NR_UNKNOWN;

		protected internal int _numberInt;

		protected internal long _numberLong;

		protected internal double _numberDouble;

		protected internal System.Numerics.BigInteger _numberBigInt;

		protected internal java.math.BigDecimal _numberBigDecimal;

		/// <summary>
		/// Flag that indicates whether numeric value has a negative
		/// value.
		/// </summary>
		/// <remarks>
		/// Flag that indicates whether numeric value has a negative
		/// value. That is, whether its textual representation starts
		/// with minus character.
		/// </remarks>
		protected internal bool _numberNegative;

		/// <summary>Length of integer part of the number, in characters</summary>
		protected internal int _intLength;

		/// <summary>
		/// Length of the fractional part (not including decimal
		/// point or exponent), in characters.
		/// </summary>
		/// <remarks>
		/// Length of the fractional part (not including decimal
		/// point or exponent), in characters.
		/// Not used for  pure integer values.
		/// </remarks>
		protected internal int _fractLength;

		/// <summary>
		/// Length of the exponent part of the number, if any, not
		/// including 'e' marker or sign, just digits.
		/// </summary>
		/// <remarks>
		/// Length of the exponent part of the number, if any, not
		/// including 'e' marker or sign, just digits.
		/// Not used for  pure integer values.
		/// </remarks>
		protected internal int _expLength;

		protected internal ParserBase(com.fasterxml.jackson.core.io.IOContext ctxt, int features
			)
			: base(features)
		{
			/*
			/**********************************************************
			/* Generic I/O state
			/**********************************************************
			*/
			/*
			/**********************************************************
			/* Current input data
			/**********************************************************
			*/
			// Note: type of actual buffer depends on sub-class, can't include
			/*
			/**********************************************************
			/* Current input location information
			/**********************************************************
			*/
			/*
			/**********************************************************
			/* Information about starting location of event
			/* Reader is pointing to; updated on-demand
			/**********************************************************
			*/
			// // // Location info at point when current token was started
			/*
			/**********************************************************
			/* Parsing state
			/**********************************************************
			*/
			/*
			/**********************************************************
			/* Buffer(s) for local name(s) and text content
			/**********************************************************
			*/
			/*
			/**********************************************************
			/* Constants and fields of former 'JsonNumericParserBase'
			/**********************************************************
			*/
			// First, integer types
			// And then floating point types
			// Also, we need some numeric constants
			// These are not very accurate, but have to do... (for bounds checks)
			// Digits, numeric
			// Numeric value holders: multiple fields used for
			// for efficiency
			// First primitives
			// And then object types
			// And then other information about value itself
			/*
			/**********************************************************
			/* Life-cycle
			/**********************************************************
			*/
			_ioContext = ctxt;
			_textBuffer = ctxt.constructTextBuffer();
			com.fasterxml.jackson.core.json.DupDetector dups = com.fasterxml.jackson.core.JsonParser.Feature
				.STRICT_DUPLICATE_DETECTION.enabledIn(features) ? com.fasterxml.jackson.core.json.DupDetector
				.rootDetector(this) : null;
			_parsingContext = com.fasterxml.jackson.core.json.JsonReadContext.createRootContext
				(dups);
		}

		public override com.fasterxml.jackson.core.Version version()
		{
			return com.fasterxml.jackson.core.json.PackageVersion.VERSION;
		}

		public override object getCurrentValue()
		{
			return _parsingContext.getCurrentValue();
		}

		public override void setCurrentValue(object v)
		{
			_parsingContext.setCurrentValue(v);
		}

		/*
		/**********************************************************
		/* Overrides for Feature handling
		/**********************************************************
		*/
		public override com.fasterxml.jackson.core.JsonParser enable(com.fasterxml.jackson.core.JsonParser.Feature
			 f)
		{
			_features |= f.getMask();
			if (f == com.fasterxml.jackson.core.JsonParser.Feature.STRICT_DUPLICATE_DETECTION)
			{
				// enabling dup detection?
				if (_parsingContext.getDupDetector() == null)
				{
					// but only if disabled currently
					_parsingContext = _parsingContext.withDupDetector(com.fasterxml.jackson.core.json.DupDetector
						.rootDetector(this));
				}
			}
			return this;
		}

		public override com.fasterxml.jackson.core.JsonParser disable(com.fasterxml.jackson.core.JsonParser.Feature
			 f)
		{
			_features &= ~f.getMask();
			if (f == com.fasterxml.jackson.core.JsonParser.Feature.STRICT_DUPLICATE_DETECTION)
			{
				_parsingContext = _parsingContext.withDupDetector(null);
			}
			return this;
		}

		public override com.fasterxml.jackson.core.JsonParser setFeatureMask(int newMask)
		{
			int changes = (_features ^ newMask);
			if (changes != 0)
			{
				_features = newMask;
				if (com.fasterxml.jackson.core.JsonParser.Feature.STRICT_DUPLICATE_DETECTION.enabledIn
					(newMask))
				{
					// enabling
					if (_parsingContext.getDupDetector() == null)
					{
						// but only if disabled currently
						_parsingContext = _parsingContext.withDupDetector(com.fasterxml.jackson.core.json.DupDetector
							.rootDetector(this));
					}
				}
				else
				{
					// disabling
					_parsingContext = _parsingContext.withDupDetector(null);
				}
			}
			return this;
		}

		/*
		/**********************************************************
		/* JsonParser impl
		/**********************************************************
		*/
		/// <summary>
		/// Method that can be called to get the name associated with
		/// the current event.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		public override string getCurrentName()
		{
			// [JACKSON-395]: start markers require information from parent
			if (_currToken == com.fasterxml.jackson.core.JsonToken.START_OBJECT || _currToken
				 == com.fasterxml.jackson.core.JsonToken.START_ARRAY)
			{
				com.fasterxml.jackson.core.json.JsonReadContext parent = ((com.fasterxml.jackson.core.json.JsonReadContext
					)_parsingContext.getParent());
				return parent.getCurrentName();
			}
			return _parsingContext.getCurrentName();
		}

		public override void overrideCurrentName(string name)
		{
			// Simple, but need to look for START_OBJECT/ARRAY's "off-by-one" thing:
			com.fasterxml.jackson.core.json.JsonReadContext ctxt = _parsingContext;
			if (_currToken == com.fasterxml.jackson.core.JsonToken.START_OBJECT || _currToken
				 == com.fasterxml.jackson.core.JsonToken.START_ARRAY)
			{
				ctxt = ((com.fasterxml.jackson.core.json.JsonReadContext)ctxt.getParent());
			}
			/* 24-Sep-2013, tatu: Unfortunate, but since we did not expose exceptions,
			*   need to wrap this here
			*/
			try
			{
				ctxt.setCurrentName(name);
			}
			catch (System.IO.IOException e)
			{
				throw new System.InvalidOperationException(e);
			}
		}

		/// <exception cref="System.IO.IOException"/>
		public override void close()
		{
			if (!_closed)
			{
				_closed = true;
				try
				{
					_closeInput();
				}
				finally
				{
					// as per [JACKSON-324], do in finally block
					// Also, internal buffer(s) can now be released as well
					_releaseBuffers();
				}
			}
		}

		public override bool isClosed()
		{
			return _closed;
		}

		public override com.fasterxml.jackson.core.JsonStreamContext getParsingContext()
		{
			return _parsingContext;
		}

		/// <summary>
		/// Method that return the <b>starting</b> location of the current
		/// token; that is, position of the first character from input
		/// that starts the current token.
		/// </summary>
		public override com.fasterxml.jackson.core.JsonLocation getTokenLocation()
		{
			return new com.fasterxml.jackson.core.JsonLocation(_ioContext.getSourceReference(
				), -1L, getTokenCharacterOffset(), getTokenLineNr(), getTokenColumnNr());
		}

		// bytes, chars
		/// <summary>
		/// Method that returns location of the last processed character;
		/// usually for error reporting purposes
		/// </summary>
		public override com.fasterxml.jackson.core.JsonLocation getCurrentLocation()
		{
			int col = _inputPtr - _currInputRowStart + 1;
			// 1-based
			return new com.fasterxml.jackson.core.JsonLocation(_ioContext.getSourceReference(
				), -1L, _currInputProcessed + _inputPtr, _currInputRow, col);
		}

		// bytes, chars
		/*
		/**********************************************************
		/* Public API, access to token information, text and similar
		/**********************************************************
		*/
		public override bool hasTextCharacters()
		{
			if (_currToken == com.fasterxml.jackson.core.JsonToken.VALUE_STRING)
			{
				return true;
			}
			// usually true        
			if (_currToken == com.fasterxml.jackson.core.JsonToken.FIELD_NAME)
			{
				return _nameCopied;
			}
			return false;
		}

		// No embedded objects with base impl...
		/// <exception cref="System.IO.IOException"/>
		public override object getEmbeddedObject()
		{
			return null;
		}

		/*
		/**********************************************************
		/* Public low-level accessors
		/**********************************************************
		*/
		public virtual long getTokenCharacterOffset()
		{
			return _tokenInputTotal;
		}

		public virtual int getTokenLineNr()
		{
			return _tokenInputRow;
		}

		public virtual int getTokenColumnNr()
		{
			// note: value of -1 means "not available"; otherwise convert from 0-based to 1-based
			int col = _tokenInputCol;
			return (col < 0) ? col : (col + 1);
		}

		/*
		/**********************************************************
		/* Low-level reading, other
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		protected internal void loadMoreGuaranteed()
		{
			if (!loadMore())
			{
				_reportInvalidEOF();
			}
		}

		/*
		/**********************************************************
		/* Abstract methods needed from sub-classes
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		protected internal abstract bool loadMore();

		/// <exception cref="System.IO.IOException"/>
		protected internal abstract void _finishString();

		/// <exception cref="System.IO.IOException"/>
		protected internal abstract void _closeInput();

		/*
		/**********************************************************
		/* Low-level reading, other
		/**********************************************************
		*/
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
		protected internal virtual void _releaseBuffers()
		{
			_textBuffer.releaseBuffers();
			char[] buf = _nameCopyBuffer;
			if (buf != null)
			{
				_nameCopyBuffer = null;
				_ioContext.releaseNameCopyBuffer(buf);
			}
		}

		/// <summary>Method called when an EOF is encountered between tokens.</summary>
		/// <remarks>
		/// Method called when an EOF is encountered between tokens.
		/// If so, it may be a legitimate EOF, but only iff there
		/// is no open non-root context.
		/// </remarks>
		/// <exception cref="com.fasterxml.jackson.core.JsonParseException"/>
		protected internal override void _handleEOF()
		{
			if (!_parsingContext.inRoot())
			{
				_reportInvalidEOF(": expected close marker for " + _parsingContext.getTypeDesc() 
					+ " (from " + _parsingContext.getStartLocation(_ioContext.getSourceReference()) 
					+ ")");
			}
		}

		/// <since>2.4</since>
		/// <exception cref="com.fasterxml.jackson.core.JsonParseException"/>
		protected internal int _eofAsNextChar()
		{
			_handleEOF();
			return -1;
		}

		/*
		/**********************************************************
		/* Internal/package methods: Error reporting
		/**********************************************************
		*/
		/// <exception cref="com.fasterxml.jackson.core.JsonParseException"/>
		protected internal virtual void _reportMismatchedEndMarker(int actCh, char expCh)
		{
			string startDesc = string.Empty + _parsingContext.getStartLocation(_ioContext.getSourceReference
				());
			_reportError("Unexpected close marker '" + ((char)actCh) + "': expected '" + expCh
				 + "' (for " + _parsingContext.getTypeDesc() + " starting at " + startDesc + ")"
				);
		}

		/*
		/**********************************************************
		/* Internal/package methods: shared/reusable builders
		/**********************************************************
		*/
		public virtual com.fasterxml.jackson.core.util.ByteArrayBuilder _getByteArrayBuilder
			()
		{
			if (_byteArrayBuilder == null)
			{
				_byteArrayBuilder = new com.fasterxml.jackson.core.util.ByteArrayBuilder();
			}
			else
			{
				_byteArrayBuilder.reset();
			}
			return _byteArrayBuilder;
		}

		/*
		/**********************************************************
		/* Methods from former JsonNumericParserBase
		/**********************************************************
		*/
		// // // Life-cycle of number-parsing
		protected internal com.fasterxml.jackson.core.JsonToken reset(bool negative, int 
			intLen, int fractLen, int expLen)
		{
			if (fractLen < 1 && expLen < 1)
			{
				// integer
				return resetInt(negative, intLen);
			}
			return resetFloat(negative, intLen, fractLen, expLen);
		}

		protected internal com.fasterxml.jackson.core.JsonToken resetInt(bool negative, int
			 intLen)
		{
			_numberNegative = negative;
			_intLength = intLen;
			_fractLength = 0;
			_expLength = 0;
			_numTypesValid = NR_UNKNOWN;
			// to force parsing
			return com.fasterxml.jackson.core.JsonToken.VALUE_NUMBER_INT;
		}

		protected internal com.fasterxml.jackson.core.JsonToken resetFloat(bool negative, 
			int intLen, int fractLen, int expLen)
		{
			_numberNegative = negative;
			_intLength = intLen;
			_fractLength = fractLen;
			_expLength = expLen;
			_numTypesValid = NR_UNKNOWN;
			// to force parsing
			return com.fasterxml.jackson.core.JsonToken.VALUE_NUMBER_FLOAT;
		}

		protected internal com.fasterxml.jackson.core.JsonToken resetAsNaN(string valueStr
			, double value)
		{
			_textBuffer.resetWithString(valueStr);
			_numberDouble = value;
			_numTypesValid = NR_DOUBLE;
			return com.fasterxml.jackson.core.JsonToken.VALUE_NUMBER_FLOAT;
		}

		/*
		/**********************************************************
		/* Numeric accessors of public API
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		public override Sharpen.Number getNumberValue()
		{
			if (_numTypesValid == NR_UNKNOWN)
			{
				_parseNumericValue(NR_UNKNOWN);
			}
			// will also check event type
			// Separate types for int types
			if (_currToken == com.fasterxml.jackson.core.JsonToken.VALUE_NUMBER_INT)
			{
				if ((_numTypesValid & NR_INT) != 0)
				{
					return _numberInt;
				}
				if ((_numTypesValid & NR_LONG) != 0)
				{
					return _numberLong;
				}
				if ((_numTypesValid & NR_BIGINT) != 0)
				{
					return _numberBigInt;
				}
				// Shouldn't get this far but if we do
				return _numberBigDecimal;
			}
			/* And then floating point types. But here optimal type
			* needs to be big decimal, to avoid losing any data?
			*/
			if ((_numTypesValid & NR_BIGDECIMAL) != 0)
			{
				return _numberBigDecimal;
			}
			if ((_numTypesValid & NR_DOUBLE) == 0)
			{
				// sanity check
				_throwInternal();
			}
			return _numberDouble;
		}

		/// <exception cref="System.IO.IOException"/>
		public override com.fasterxml.jackson.core.JsonParser.NumberType getNumberType()
		{
			if (_numTypesValid == NR_UNKNOWN)
			{
				_parseNumericValue(NR_UNKNOWN);
			}
			// will also check event type
			if (_currToken == com.fasterxml.jackson.core.JsonToken.VALUE_NUMBER_INT)
			{
				if ((_numTypesValid & NR_INT) != 0)
				{
					return com.fasterxml.jackson.core.JsonParser.NumberType.INT;
				}
				if ((_numTypesValid & NR_LONG) != 0)
				{
					return com.fasterxml.jackson.core.JsonParser.NumberType.LONG;
				}
				return com.fasterxml.jackson.core.JsonParser.NumberType.BIG_INTEGER;
			}
			/* And then floating point types. Here optimal type
			* needs to be big decimal, to avoid losing any data?
			* However... using BD is slow, so let's allow returning
			* double as type if no explicit call has been made to access
			* data as BD?
			*/
			if ((_numTypesValid & NR_BIGDECIMAL) != 0)
			{
				return com.fasterxml.jackson.core.JsonParser.NumberType.BIG_DECIMAL;
			}
			return com.fasterxml.jackson.core.JsonParser.NumberType.DOUBLE;
		}

		/// <exception cref="System.IO.IOException"/>
		public override int getIntValue()
		{
			if ((_numTypesValid & NR_INT) == 0)
			{
				if (_numTypesValid == NR_UNKNOWN)
				{
					// not parsed at all
					return _parseIntValue();
				}
				if ((_numTypesValid & NR_INT) == 0)
				{
					// wasn't an int natively?
					convertNumberToInt();
				}
			}
			// let's make it so, if possible
			return _numberInt;
		}

		/// <exception cref="System.IO.IOException"/>
		public override long getLongValue()
		{
			if ((_numTypesValid & NR_LONG) == 0)
			{
				if (_numTypesValid == NR_UNKNOWN)
				{
					_parseNumericValue(NR_LONG);
				}
				if ((_numTypesValid & NR_LONG) == 0)
				{
					convertNumberToLong();
				}
			}
			return _numberLong;
		}

		/// <exception cref="System.IO.IOException"/>
		public override System.Numerics.BigInteger getBigIntegerValue()
		{
			if ((_numTypesValid & NR_BIGINT) == 0)
			{
				if (_numTypesValid == NR_UNKNOWN)
				{
					_parseNumericValue(NR_BIGINT);
				}
				if ((_numTypesValid & NR_BIGINT) == 0)
				{
					convertNumberToBigInteger();
				}
			}
			return _numberBigInt;
		}

		/// <exception cref="System.IO.IOException"/>
		public override float getFloatValue()
		{
			double value = getDoubleValue();
			/* 22-Jan-2009, tatu: Bounds/range checks would be tricky
			*   here, so let's not bother even trying...
			*/
			/*
			if (value < -Float.MAX_VALUE || value > MAX_FLOAT_D) {
			_reportError("Numeric value ("+getText()+") out of range of Java float");
			}
			*/
			return (float)value;
		}

		/// <exception cref="System.IO.IOException"/>
		public override double getDoubleValue()
		{
			if ((_numTypesValid & NR_DOUBLE) == 0)
			{
				if (_numTypesValid == NR_UNKNOWN)
				{
					_parseNumericValue(NR_DOUBLE);
				}
				if ((_numTypesValid & NR_DOUBLE) == 0)
				{
					convertNumberToDouble();
				}
			}
			return _numberDouble;
		}

		/// <exception cref="System.IO.IOException"/>
		public override java.math.BigDecimal getDecimalValue()
		{
			if ((_numTypesValid & NR_BIGDECIMAL) == 0)
			{
				if (_numTypesValid == NR_UNKNOWN)
				{
					_parseNumericValue(NR_BIGDECIMAL);
				}
				if ((_numTypesValid & NR_BIGDECIMAL) == 0)
				{
					convertNumberToBigDecimal();
				}
			}
			return _numberBigDecimal;
		}

		/*
		/**********************************************************
		/* Conversion from textual to numeric representation
		/**********************************************************
		*/
		/// <summary>
		/// Method that will parse actual numeric value out of a syntactically
		/// valid number value.
		/// </summary>
		/// <remarks>
		/// Method that will parse actual numeric value out of a syntactically
		/// valid number value. Type it will parse into depends on whether
		/// it is a floating point number, as well as its magnitude: smallest
		/// legal type (of ones available) is used for efficiency.
		/// </remarks>
		/// <param name="expType">
		/// Numeric type that we will immediately need, if any;
		/// mostly necessary to optimize handling of floating point numbers
		/// </param>
		/// <exception cref="System.IO.IOException"/>
		protected internal virtual void _parseNumericValue(int expType)
		{
			// Int or float?
			if (_currToken == com.fasterxml.jackson.core.JsonToken.VALUE_NUMBER_INT)
			{
				char[] buf = _textBuffer.getTextBuffer();
				int offset = _textBuffer.getTextOffset();
				int len = _intLength;
				if (_numberNegative)
				{
					++offset;
				}
				if (len <= 9)
				{
					// definitely fits in int
					int i = com.fasterxml.jackson.core.io.NumberInput.parseInt(buf, offset, len);
					_numberInt = _numberNegative ? -i : i;
					_numTypesValid = NR_INT;
					return;
				}
				if (len <= 18)
				{
					// definitely fits AND is easy to parse using 2 int parse calls
					long l = com.fasterxml.jackson.core.io.NumberInput.parseLong(buf, offset, len);
					if (_numberNegative)
					{
						l = -l;
					}
					// [JACKSON-230] Could still fit in int, need to check
					if (len == 10)
					{
						if (_numberNegative)
						{
							if (l >= MIN_INT_L)
							{
								_numberInt = (int)l;
								_numTypesValid = NR_INT;
								return;
							}
						}
						else
						{
							if (l <= MAX_INT_L)
							{
								_numberInt = (int)l;
								_numTypesValid = NR_INT;
								return;
							}
						}
					}
					_numberLong = l;
					_numTypesValid = NR_LONG;
					return;
				}
				_parseSlowInt(expType, buf, offset, len);
				return;
			}
			if (_currToken == com.fasterxml.jackson.core.JsonToken.VALUE_NUMBER_FLOAT)
			{
				_parseSlowFloat(expType);
				return;
			}
			_reportError("Current token (" + _currToken + ") not numeric, can not use numeric value accessors"
				);
		}

		/// <since>2.6</since>
		/// <exception cref="System.IO.IOException"/>
		protected internal virtual int _parseIntValue()
		{
			// Inlined variant of: _parseNumericValue(NR_INT)
			if (_currToken == com.fasterxml.jackson.core.JsonToken.VALUE_NUMBER_INT)
			{
				char[] buf = _textBuffer.getTextBuffer();
				int offset = _textBuffer.getTextOffset();
				int len = _intLength;
				if (_numberNegative)
				{
					++offset;
				}
				if (len <= 9)
				{
					int i = com.fasterxml.jackson.core.io.NumberInput.parseInt(buf, offset, len);
					if (_numberNegative)
					{
						i = -i;
					}
					_numberInt = i;
					_numTypesValid = NR_INT;
					return i;
				}
			}
			_parseNumericValue(NR_INT);
			if ((_numTypesValid & NR_INT) == 0)
			{
				convertNumberToInt();
			}
			return _numberInt;
		}

		/// <exception cref="System.IO.IOException"/>
		private void _parseSlowFloat(int expType)
		{
			/* Nope: floating point. Here we need to be careful to get
			* optimal parsing strategy: choice is between accurate but
			* slow (BigDecimal) and lossy but fast (Double). For now
			* let's only use BD when explicitly requested -- it can
			* still be constructed correctly at any point since we do
			* retain textual representation
			*/
			try
			{
				if (expType == NR_BIGDECIMAL)
				{
					_numberBigDecimal = _textBuffer.contentsAsDecimal();
					_numTypesValid = NR_BIGDECIMAL;
				}
				else
				{
					// Otherwise double has to do
					_numberDouble = _textBuffer.contentsAsDouble();
					_numTypesValid = NR_DOUBLE;
				}
			}
			catch (System.FormatException nex)
			{
				// Can this ever occur? Due to overflow, maybe?
				_wrapError("Malformed numeric value '" + _textBuffer.contentsAsString() + "'", nex
					);
			}
		}

		/// <exception cref="System.IO.IOException"/>
		private void _parseSlowInt(int expType, char[] buf, int offset, int len)
		{
			string numStr = _textBuffer.contentsAsString();
			try
			{
				// [JACKSON-230] Some long cases still...
				if (com.fasterxml.jackson.core.io.NumberInput.inLongRange(buf, offset, len, _numberNegative
					))
				{
					// Probably faster to construct a String, call parse, than to use BigInteger
					_numberLong = System.Convert.ToInt64(numStr);
					_numTypesValid = NR_LONG;
				}
				else
				{
					// nope, need the heavy guns... (rare case)
					_numberBigInt = new System.Numerics.BigInteger(numStr);
					_numTypesValid = NR_BIGINT;
				}
			}
			catch (System.FormatException nex)
			{
				// Can this ever occur? Due to overflow, maybe?
				_wrapError("Malformed numeric value '" + numStr + "'", nex);
			}
		}

		/*
		/**********************************************************
		/* Numeric conversions
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		protected internal virtual void convertNumberToInt()
		{
			// First, converting from long ought to be easy
			if ((_numTypesValid & NR_LONG) != 0)
			{
				// Let's verify it's lossless conversion by simple roundtrip
				int result = (int)_numberLong;
				if (((long)result) != _numberLong)
				{
					_reportError("Numeric value (" + getText() + ") out of range of int");
				}
				_numberInt = result;
			}
			else
			{
				if ((_numTypesValid & NR_BIGINT) != 0)
				{
					if (BI_MIN_INT.compareTo(_numberBigInt) > 0 || BI_MAX_INT.compareTo(_numberBigInt
						) < 0)
					{
						reportOverflowInt();
					}
					_numberInt = _numberBigInt;
				}
				else
				{
					if ((_numTypesValid & NR_DOUBLE) != 0)
					{
						// Need to check boundaries
						if (_numberDouble < MIN_INT_D || _numberDouble > MAX_INT_D)
						{
							reportOverflowInt();
						}
						_numberInt = (int)_numberDouble;
					}
					else
					{
						if ((_numTypesValid & NR_BIGDECIMAL) != 0)
						{
							if (BD_MIN_INT.compareTo(_numberBigDecimal) > 0 || BD_MAX_INT.compareTo(_numberBigDecimal
								) < 0)
							{
								reportOverflowInt();
							}
							_numberInt = _numberBigDecimal;
						}
						else
						{
							_throwInternal();
						}
					}
				}
			}
			_numTypesValid |= NR_INT;
		}

		/// <exception cref="System.IO.IOException"/>
		protected internal virtual void convertNumberToLong()
		{
			if ((_numTypesValid & NR_INT) != 0)
			{
				_numberLong = (long)_numberInt;
			}
			else
			{
				if ((_numTypesValid & NR_BIGINT) != 0)
				{
					if (BI_MIN_LONG.compareTo(_numberBigInt) > 0 || BI_MAX_LONG.compareTo(_numberBigInt
						) < 0)
					{
						reportOverflowLong();
					}
					_numberLong = _numberBigInt;
				}
				else
				{
					if ((_numTypesValid & NR_DOUBLE) != 0)
					{
						// Need to check boundaries
						if (_numberDouble < MIN_LONG_D || _numberDouble > MAX_LONG_D)
						{
							reportOverflowLong();
						}
						_numberLong = (long)_numberDouble;
					}
					else
					{
						if ((_numTypesValid & NR_BIGDECIMAL) != 0)
						{
							if (BD_MIN_LONG.compareTo(_numberBigDecimal) > 0 || BD_MAX_LONG.compareTo(_numberBigDecimal
								) < 0)
							{
								reportOverflowLong();
							}
							_numberLong = _numberBigDecimal;
						}
						else
						{
							_throwInternal();
						}
					}
				}
			}
			_numTypesValid |= NR_LONG;
		}

		/// <exception cref="System.IO.IOException"/>
		protected internal virtual void convertNumberToBigInteger()
		{
			if ((_numTypesValid & NR_BIGDECIMAL) != 0)
			{
				// here it'll just get truncated, no exceptions thrown
				_numberBigInt = _numberBigDecimal.toBigInteger();
			}
			else
			{
				if ((_numTypesValid & NR_LONG) != 0)
				{
					_numberBigInt = System.Convert.ToInt64(_numberLong);
				}
				else
				{
					if ((_numTypesValid & NR_INT) != 0)
					{
						_numberBigInt = System.Convert.ToInt64(_numberInt);
					}
					else
					{
						if ((_numTypesValid & NR_DOUBLE) != 0)
						{
							_numberBigInt = java.math.BigDecimal.valueOf(_numberDouble).toBigInteger();
						}
						else
						{
							_throwInternal();
						}
					}
				}
			}
			_numTypesValid |= NR_BIGINT;
		}

		/// <exception cref="System.IO.IOException"/>
		protected internal virtual void convertNumberToDouble()
		{
			/* 05-Aug-2008, tatus: Important note: this MUST start with
			*   more accurate representations, since we don't know which
			*   value is the original one (others get generated when
			*   requested)
			*/
			if ((_numTypesValid & NR_BIGDECIMAL) != 0)
			{
				_numberDouble = _numberBigDecimal;
			}
			else
			{
				if ((_numTypesValid & NR_BIGINT) != 0)
				{
					_numberDouble = _numberBigInt;
				}
				else
				{
					if ((_numTypesValid & NR_LONG) != 0)
					{
						_numberDouble = (double)_numberLong;
					}
					else
					{
						if ((_numTypesValid & NR_INT) != 0)
						{
							_numberDouble = (double)_numberInt;
						}
						else
						{
							_throwInternal();
						}
					}
				}
			}
			_numTypesValid |= NR_DOUBLE;
		}

		/// <exception cref="System.IO.IOException"/>
		protected internal virtual void convertNumberToBigDecimal()
		{
			/* 05-Aug-2008, tatus: Important note: this MUST start with
			*   more accurate representations, since we don't know which
			*   value is the original one (others get generated when
			*   requested)
			*/
			if ((_numTypesValid & NR_DOUBLE) != 0)
			{
				/* Let's actually parse from String representation,
				* to avoid rounding errors that non-decimal floating operations
				* would incur
				*/
				_numberBigDecimal = com.fasterxml.jackson.core.io.NumberInput.parseBigDecimal(getText
					());
			}
			else
			{
				if ((_numTypesValid & NR_BIGINT) != 0)
				{
					_numberBigDecimal = new java.math.BigDecimal(_numberBigInt);
				}
				else
				{
					if ((_numTypesValid & NR_LONG) != 0)
					{
						_numberBigDecimal = java.math.BigDecimal.valueOf(_numberLong);
					}
					else
					{
						if ((_numTypesValid & NR_INT) != 0)
						{
							_numberBigDecimal = java.math.BigDecimal.valueOf(_numberInt);
						}
						else
						{
							_throwInternal();
						}
					}
				}
			}
			_numTypesValid |= NR_BIGDECIMAL;
		}

		/*
		/**********************************************************
		/* Number handling exceptions
		/**********************************************************
		*/
		/// <exception cref="com.fasterxml.jackson.core.JsonParseException"/>
		protected internal virtual void reportUnexpectedNumberChar(int ch, string comment
			)
		{
			string msg = "Unexpected character (" + _getCharDesc(ch) + ") in numeric value";
			if (comment != null)
			{
				msg += ": " + comment;
			}
			_reportError(msg);
		}

		/// <exception cref="com.fasterxml.jackson.core.JsonParseException"/>
		protected internal virtual void reportInvalidNumber(string msg)
		{
			_reportError("Invalid numeric value: " + msg);
		}

		/// <exception cref="System.IO.IOException"/>
		protected internal virtual void reportOverflowInt()
		{
			_reportError("Numeric value (" + getText() + ") out of range of int (" + int.MinValue
				 + " - " + int.MaxValue + ")");
		}

		/// <exception cref="System.IO.IOException"/>
		protected internal virtual void reportOverflowLong()
		{
			_reportError("Numeric value (" + getText() + ") out of range of long (" + long.MinValue
				 + " - " + long.MaxValue + ")");
		}

		/*
		/**********************************************************
		/* Base64 handling support
		/**********************************************************
		*/
		/// <summary>
		/// Method that sub-classes must implement to support escaped sequences
		/// in base64-encoded sections.
		/// </summary>
		/// <remarks>
		/// Method that sub-classes must implement to support escaped sequences
		/// in base64-encoded sections.
		/// Sub-classes that do not need base64 support can leave this as is
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		protected internal virtual char _decodeEscaped()
		{
			throw new System.NotSupportedException();
		}

		/// <exception cref="System.IO.IOException"/>
		protected internal int _decodeBase64Escape(com.fasterxml.jackson.core.Base64Variant
			 b64variant, int ch, int index)
		{
			// 17-May-2011, tatu: As per [JACKSON-xxx], need to handle escaped chars
			if (ch != '\\')
			{
				throw reportInvalidBase64Char(b64variant, ch, index);
			}
			int unescaped = _decodeEscaped();
			// if white space, skip if first triplet; otherwise errors
			if (unescaped <= INT_SPACE)
			{
				if (index == 0)
				{
					// whitespace only allowed to be skipped between triplets
					return -1;
				}
			}
			// otherwise try to find actual triplet value
			int bits = b64variant.decodeBase64Char(unescaped);
			if (bits < 0)
			{
				throw reportInvalidBase64Char(b64variant, unescaped, index);
			}
			return bits;
		}

		/// <exception cref="System.IO.IOException"/>
		protected internal int _decodeBase64Escape(com.fasterxml.jackson.core.Base64Variant
			 b64variant, char ch, int index)
		{
			// 17-May-2011, tatu: As per [JACKSON-xxx], need to handle escaped chars
			if (ch != '\\')
			{
				throw reportInvalidBase64Char(b64variant, ch, index);
			}
			char unescaped = _decodeEscaped();
			// if white space, skip if first triplet; otherwise errors
			if (unescaped <= INT_SPACE)
			{
				if (index == 0)
				{
					// whitespace only allowed to be skipped between triplets
					return -1;
				}
			}
			// otherwise try to find actual triplet value
			int bits = b64variant.decodeBase64Char(unescaped);
			if (bits < 0)
			{
				throw reportInvalidBase64Char(b64variant, unescaped, index);
			}
			return bits;
		}

		/// <exception cref="System.ArgumentException"/>
		protected internal virtual System.ArgumentException reportInvalidBase64Char(com.fasterxml.jackson.core.Base64Variant
			 b64variant, int ch, int bindex)
		{
			return reportInvalidBase64Char(b64variant, ch, bindex, null);
		}

		/// <param name="bindex">
		/// Relative index within base64 character unit; between 0
		/// and 3 (as unit has exactly 4 characters)
		/// </param>
		/// <exception cref="System.ArgumentException"/>
		protected internal virtual System.ArgumentException reportInvalidBase64Char(com.fasterxml.jackson.core.Base64Variant
			 b64variant, int ch, int bindex, string msg)
		{
			string @base;
			if (ch <= INT_SPACE)
			{
				@base = "Illegal white space character (code 0x" + Sharpen.Extensions.ToHexString
					(ch) + ") as character #" + (bindex + 1) + " of 4-char base64 unit: can only used between units";
			}
			else
			{
				if (b64variant.usesPaddingChar(ch))
				{
					@base = "Unexpected padding character ('" + b64variant.getPaddingChar() + "') as character #"
						 + (bindex + 1) + " of 4-char base64 unit: padding only legal as 3rd or 4th character";
				}
				else
				{
					if (!char.isDefined(ch) || char.isISOControl(ch))
					{
						// Not sure if we can really get here... ? (most illegal xml chars are caught at lower level)
						@base = "Illegal character (code 0x" + Sharpen.Extensions.ToHexString(ch) + ") in base64 content";
					}
					else
					{
						@base = "Illegal character '" + ((char)ch) + "' (code 0x" + Sharpen.Extensions.ToHexString
							(ch) + ") in base64 content";
					}
				}
			}
			if (msg != null)
			{
				@base = @base + ": " + msg;
			}
			return new System.ArgumentException(@base);
		}
	}
}
