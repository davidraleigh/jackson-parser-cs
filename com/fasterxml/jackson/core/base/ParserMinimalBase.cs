using Sharpen;

namespace com.fasterxml.jackson.core.@base
{
	/// <summary>
	/// Intermediate base class used by all Jackson
	/// <see cref="com.fasterxml.jackson.core.JsonParser"/>
	/// implementations, but does not add any additional fields that depend
	/// on particular method of obtaining input.
	/// <p>
	/// Note that 'minimal' here mostly refers to minimal number of fields
	/// (size) and functionality that is specific to certain types
	/// of parser implementations; but not necessarily to number of methods.
	/// </summary>
	public abstract class ParserMinimalBase : com.fasterxml.jackson.core.JsonParser
	{
		protected internal const int INT_TAB = '\t';

		protected internal const int INT_LF = '\n';

		protected internal const int INT_CR = '\r';

		protected internal const int INT_SPACE = unchecked((int)(0x0020));

		protected internal const int INT_LBRACKET = '[';

		protected internal const int INT_RBRACKET = ']';

		protected internal const int INT_LCURLY = '{';

		protected internal const int INT_RCURLY = '}';

		protected internal const int INT_QUOTE = '"';

		protected internal const int INT_BACKSLASH = '\\';

		protected internal const int INT_SLASH = '/';

		protected internal const int INT_COLON = ':';

		protected internal const int INT_COMMA = ',';

		protected internal const int INT_HASH = '#';

		protected internal const int INT_PERIOD = '.';

		protected internal const int INT_e = 'e';

		protected internal const int INT_E = 'E';

		/// <summary>
		/// Last token retrieved via
		/// <see cref="nextToken()"/>
		/// , if any.
		/// Null before the first call to <code>nextToken()</code>,
		/// as well as if token has been explicitly cleared
		/// </summary>
		protected internal com.fasterxml.jackson.core.JsonToken _currToken;

		/// <summary>
		/// Last cleared token, if any: that is, value that was in
		/// effect when
		/// <see cref="clearCurrentToken()"/>
		/// was called.
		/// </summary>
		protected internal com.fasterxml.jackson.core.JsonToken _lastClearedToken;

		protected internal ParserMinimalBase()
		{
		}

		protected internal ParserMinimalBase(int features)
			: base(features)
		{
		}

		// Control chars:
		// Markup
		// fp numbers
		/*
		/**********************************************************
		/* Minimal generally useful state
		/**********************************************************
		*/
		/*
		/**********************************************************
		/* Life-cycle
		/**********************************************************
		*/
		// NOTE: had base impl in 2.3 and before; but shouldn't
		// public abstract Version version();
		/*
		/**********************************************************
		/* Configuration overrides if any
		/**********************************************************
		*/
		// from base class:
		//public void enableFeature(Feature f)
		//public void disableFeature(Feature f)
		//public void setFeature(Feature f, boolean state)
		//public boolean isFeatureEnabled(Feature f)
		/*
		/**********************************************************
		/* JsonParser impl
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		public abstract override com.fasterxml.jackson.core.JsonToken nextToken();

		public override com.fasterxml.jackson.core.JsonToken getCurrentToken()
		{
			return _currToken;
		}

		public override int getCurrentTokenId()
		{
			com.fasterxml.jackson.core.JsonToken t = _currToken;
			return (t == null) ? JsonTokenIdConstants.ID_NO_TOKEN : t.id();
		}

		public override bool hasCurrentToken()
		{
			return _currToken != null;
		}

		public override bool hasTokenId(int id)
		{
			com.fasterxml.jackson.core.JsonToken t = _currToken;
			if (t == null)
			{
				return (JsonTokenIdConstants.ID_NO_TOKEN == id);
			}
			return t.id() == id;
		}

		public override bool hasToken(com.fasterxml.jackson.core.JsonToken t)
		{
			return (_currToken == t);
		}

		public override bool isExpectedStartArrayToken()
		{
			return _currToken == com.fasterxml.jackson.core.JsonToken.START_ARRAY;
		}

		public override bool isExpectedStartObjectToken()
		{
			return _currToken == com.fasterxml.jackson.core.JsonToken.START_OBJECT;
		}

		/// <exception cref="System.IO.IOException"/>
		public override com.fasterxml.jackson.core.JsonToken nextValue()
		{
			/* Implementation should be as trivial as follows; only
			* needs to change if we are to skip other tokens (for
			* example, if comments were exposed as tokens)
			*/
			com.fasterxml.jackson.core.JsonToken t = nextToken();
			if (t == com.fasterxml.jackson.core.JsonToken.FIELD_NAME)
			{
				t = nextToken();
			}
			return t;
		}

		/// <exception cref="System.IO.IOException"/>
		public override com.fasterxml.jackson.core.JsonParser skipChildren()
		{
			if (_currToken != com.fasterxml.jackson.core.JsonToken.START_OBJECT && _currToken
				 != com.fasterxml.jackson.core.JsonToken.START_ARRAY)
			{
				return this;
			}
			int open = 1;
			/* Since proper matching of start/end markers is handled
			* by nextToken(), we'll just count nesting levels here
			*/
			while (true)
			{
				com.fasterxml.jackson.core.JsonToken t = nextToken();
				if (t == null)
				{
					_handleEOF();
					/* given constraints, above should never return;
					* however, FindBugs doesn't know about it and
					* complains... so let's add dummy break here
					*/
					return this;
				}
				if (t.isStructStart())
				{
					++open;
				}
				else
				{
					if (t.isStructEnd())
					{
						if (--open == 0)
						{
							return this;
						}
					}
				}
			}
		}

		/// <summary>Method sub-classes need to implement</summary>
		/// <exception cref="com.fasterxml.jackson.core.JsonParseException"/>
		protected internal abstract void _handleEOF();

		//public JsonToken getCurrentToken()
		//public boolean hasCurrentToken()
		/// <exception cref="System.IO.IOException"/>
		public abstract override string getCurrentName();

		/// <exception cref="System.IO.IOException"/>
		public abstract override void close();

		public abstract override bool isClosed();

		public abstract override com.fasterxml.jackson.core.JsonStreamContext getParsingContext
			();

		//    public abstract JsonLocation getTokenLocation();
		//   public abstract JsonLocation getCurrentLocation();
		/*
		/**********************************************************
		/* Public API, token state overrides
		/**********************************************************
		*/
		public override void clearCurrentToken()
		{
			if (_currToken != null)
			{
				_lastClearedToken = _currToken;
				_currToken = null;
			}
		}

		public override com.fasterxml.jackson.core.JsonToken getLastClearedToken()
		{
			return _lastClearedToken;
		}

		public abstract override void overrideCurrentName(string name);

		/*
		/**********************************************************
		/* Public API, access to token information, text
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		public abstract override string getText();

		/// <exception cref="System.IO.IOException"/>
		public abstract override char[] getTextCharacters();

		public abstract override bool hasTextCharacters();

		/// <exception cref="System.IO.IOException"/>
		public abstract override int getTextLength();

		/// <exception cref="System.IO.IOException"/>
		public abstract override int getTextOffset();

		/*
		/**********************************************************
		/* Public API, access to token information, binary
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		public abstract override byte[] getBinaryValue(com.fasterxml.jackson.core.Base64Variant
			 b64variant);

		/*
		/**********************************************************
		/* Public API, access with conversion/coercion
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		public override bool getValueAsBoolean(bool defaultValue)
		{
			com.fasterxml.jackson.core.JsonToken t = _currToken;
			if (t != null)
			{
				switch (t.id())
				{
					case JsonTokenIdConstants.ID_STRING:
					{
						string str = Sharpen.Extensions.Trim(getText());
						if ("true".Equals(str))
						{
							return true;
						}
						if ("false".Equals(str))
						{
							return false;
						}
						if (_hasTextualNull(str))
						{
							return false;
						}
						break;
					}

					case JsonTokenIdConstants.ID_NUMBER_INT:
					{
						return getIntValue() != 0;
					}

					case JsonTokenIdConstants.ID_TRUE:
					{
						return true;
					}

					case JsonTokenIdConstants.ID_FALSE:
					case JsonTokenIdConstants.ID_NULL:
					{
						return false;
					}

					case JsonTokenIdConstants.ID_EMBEDDED_OBJECT:
					{
						object value = this.getEmbeddedObject();
						if (value is bool)
						{
							return (bool)value;
						}
						break;
					}

					default:
					{
						break;
					}
				}
			}
			return defaultValue;
		}

		/// <exception cref="System.IO.IOException"/>
		public override int getValueAsInt()
		{
			com.fasterxml.jackson.core.JsonToken t = _currToken;
			if (t == com.fasterxml.jackson.core.JsonToken.VALUE_NUMBER_INT)
			{
				return getIntValue();
			}
			if (t == com.fasterxml.jackson.core.JsonToken.VALUE_NUMBER_FLOAT)
			{
				return getIntValue();
			}
			return getValueAsInt(0);
		}

		/// <exception cref="System.IO.IOException"/>
		public override int getValueAsInt(int defaultValue)
		{
			com.fasterxml.jackson.core.JsonToken t = _currToken;
			if (t == com.fasterxml.jackson.core.JsonToken.VALUE_NUMBER_INT)
			{
				return getIntValue();
			}
			if (t == com.fasterxml.jackson.core.JsonToken.VALUE_NUMBER_FLOAT)
			{
				return getIntValue();
			}
			if (t != null)
			{
				switch (t.id())
				{
					case JsonTokenIdConstants.ID_STRING:
					{
						string str = getText();
						if (_hasTextualNull(str))
						{
							return 0;
						}
						return com.fasterxml.jackson.core.io.NumberInput.parseAsInt(str, defaultValue);
					}

					case JsonTokenIdConstants.ID_TRUE:
					{
						return 1;
					}

					case JsonTokenIdConstants.ID_FALSE:
					{
						return 0;
					}

					case JsonTokenIdConstants.ID_NULL:
					{
						return 0;
					}

					case JsonTokenIdConstants.ID_EMBEDDED_OBJECT:
					{
						object value = this.getEmbeddedObject();
						if (value is java.lang.Number)
						{
							return ((java.lang.Number)value);
						}
						break;
					}
				}
			}
			return defaultValue;
		}

		/// <exception cref="System.IO.IOException"/>
		public override long getValueAsLong()
		{
			com.fasterxml.jackson.core.JsonToken t = _currToken;
			if (t == com.fasterxml.jackson.core.JsonToken.VALUE_NUMBER_INT)
			{
				return getLongValue();
			}
			if (t == com.fasterxml.jackson.core.JsonToken.VALUE_NUMBER_FLOAT)
			{
				return getLongValue();
			}
			return getValueAsLong(0L);
		}

		/// <exception cref="System.IO.IOException"/>
		public override long getValueAsLong(long defaultValue)
		{
			com.fasterxml.jackson.core.JsonToken t = _currToken;
			if (t == com.fasterxml.jackson.core.JsonToken.VALUE_NUMBER_INT)
			{
				return getLongValue();
			}
			if (t == com.fasterxml.jackson.core.JsonToken.VALUE_NUMBER_FLOAT)
			{
				return getLongValue();
			}
			if (t != null)
			{
				switch (t.id())
				{
					case JsonTokenIdConstants.ID_STRING:
					{
						string str = getText();
						if (_hasTextualNull(str))
						{
							return 0L;
						}
						return com.fasterxml.jackson.core.io.NumberInput.parseAsLong(str, defaultValue);
					}

					case JsonTokenIdConstants.ID_TRUE:
					{
						return 1L;
					}

					case JsonTokenIdConstants.ID_FALSE:
					case JsonTokenIdConstants.ID_NULL:
					{
						return 0L;
					}

					case JsonTokenIdConstants.ID_EMBEDDED_OBJECT:
					{
						object value = this.getEmbeddedObject();
						if (value is java.lang.Number)
						{
							return ((java.lang.Number)value);
						}
						break;
					}
				}
			}
			return defaultValue;
		}

		/// <exception cref="System.IO.IOException"/>
		public override double getValueAsDouble(double defaultValue)
		{
			com.fasterxml.jackson.core.JsonToken t = _currToken;
			if (t != null)
			{
				switch (t.id())
				{
					case JsonTokenIdConstants.ID_STRING:
					{
						string str = getText();
						if (_hasTextualNull(str))
						{
							return 0L;
						}
						return com.fasterxml.jackson.core.io.NumberInput.parseAsDouble(str, defaultValue);
					}

					case JsonTokenIdConstants.ID_NUMBER_INT:
					case JsonTokenIdConstants.ID_NUMBER_FLOAT:
					{
						return getDoubleValue();
					}

					case JsonTokenIdConstants.ID_TRUE:
					{
						return 1.0;
					}

					case JsonTokenIdConstants.ID_FALSE:
					case JsonTokenIdConstants.ID_NULL:
					{
						return 0.0;
					}

					case JsonTokenIdConstants.ID_EMBEDDED_OBJECT:
					{
						object value = this.getEmbeddedObject();
						if (value is java.lang.Number)
						{
							return ((java.lang.Number)value);
						}
						break;
					}
				}
			}
			return defaultValue;
		}

		/// <exception cref="System.IO.IOException"/>
		public override string getValueAsString()
		{
			if (_currToken == com.fasterxml.jackson.core.JsonToken.VALUE_STRING)
			{
				return getText();
			}
			if (_currToken == com.fasterxml.jackson.core.JsonToken.FIELD_NAME)
			{
				return getCurrentName();
			}
			return getValueAsString(null);
		}

		/// <exception cref="System.IO.IOException"/>
		public override string getValueAsString(string defaultValue)
		{
			if (_currToken == com.fasterxml.jackson.core.JsonToken.VALUE_STRING)
			{
				return getText();
			}
			if (_currToken == com.fasterxml.jackson.core.JsonToken.FIELD_NAME)
			{
				return getCurrentName();
			}
			if (_currToken == null || _currToken == com.fasterxml.jackson.core.JsonToken.VALUE_NULL
				 || !_currToken.isScalarValue())
			{
				return defaultValue;
			}
			return getText();
		}

		/*
		/**********************************************************
		/* Base64 decoding
		/**********************************************************
		*/
		/// <summary>
		/// Helper method that can be used for base64 decoding in cases where
		/// encoded content has already been read as a String.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		protected internal virtual void _decodeBase64(string str, com.fasterxml.jackson.core.util.ByteArrayBuilder
			 builder, com.fasterxml.jackson.core.Base64Variant b64variant)
		{
			// just call helper method introduced in 2.2.3
			try
			{
				b64variant.decode(str, builder);
			}
			catch (System.ArgumentException e)
			{
				_reportError(e.Message);
			}
		}

		/*
		/**********************************************************
		/* Coercion helper methods (overridable)
		/**********************************************************
		*/
		/// <summary>
		/// Helper method used to determine whether we are currently pointing to
		/// a String value of "null" (NOT a null token); and, if so, that parser
		/// is to recognize and return it similar to if it was real null token.
		/// </summary>
		/// <since>2.3</since>
		protected internal virtual bool _hasTextualNull(string value)
		{
			return "null".Equals(value);
		}

		/*
		/**********************************************************
		/* Error reporting
		/**********************************************************
		*/
		/// <exception cref="com.fasterxml.jackson.core.JsonParseException"/>
		protected internal virtual void _reportUnexpectedChar(int ch, string comment)
		{
			if (ch < 0)
			{
				// sanity check
				_reportInvalidEOF();
			}
			string msg = "Unexpected character (" + _getCharDesc(ch) + ")";
			if (comment != null)
			{
				msg += ": " + comment;
			}
			_reportError(msg);
		}

		/// <exception cref="com.fasterxml.jackson.core.JsonParseException"/>
		protected internal virtual void _reportInvalidEOF()
		{
			_reportInvalidEOF(" in " + _currToken);
		}

		/// <exception cref="com.fasterxml.jackson.core.JsonParseException"/>
		protected internal virtual void _reportInvalidEOF(string msg)
		{
			_reportError("Unexpected end-of-input" + msg);
		}

		/// <exception cref="com.fasterxml.jackson.core.JsonParseException"/>
		protected internal virtual void _reportInvalidEOFInValue()
		{
			_reportInvalidEOF(" in a value");
		}

		/// <exception cref="com.fasterxml.jackson.core.JsonParseException"/>
		protected internal virtual void _reportMissingRootWS(int ch)
		{
			_reportUnexpectedChar(ch, "Expected space separating root-level values");
		}

		/// <exception cref="com.fasterxml.jackson.core.JsonParseException"/>
		protected internal virtual void _throwInvalidSpace(int i)
		{
			char c = (char)i;
			string msg = "Illegal character (" + _getCharDesc(c) + "): only regular white space (\\r, \\n, \\t) is allowed between tokens";
			_reportError(msg);
		}

		/// <summary>Method called to report a problem with unquoted control character.</summary>
		/// <remarks>
		/// Method called to report a problem with unquoted control character.
		/// Note: starting with version 1.4, it is possible to suppress
		/// exception by enabling
		/// <see cref="com.fasterxml.jackson.core.JsonParser.Feature.ALLOW_UNQUOTED_CONTROL_CHARS
		/// 	"/>
		/// .
		/// </remarks>
		/// <exception cref="com.fasterxml.jackson.core.JsonParseException"/>
		protected internal virtual void _throwUnquotedSpace(int i, string ctxtDesc)
		{
			// JACKSON-208; possible to allow unquoted control chars:
			if (!isEnabled(com.fasterxml.jackson.core.JsonParser.Feature.ALLOW_UNQUOTED_CONTROL_CHARS
				) || i > INT_SPACE)
			{
				char c = (char)i;
				string msg = "Illegal unquoted character (" + _getCharDesc(c) + "): has to be escaped using backslash to be included in "
					 + ctxtDesc;
				_reportError(msg);
			}
		}

		/// <exception cref="com.fasterxml.jackson.core.JsonProcessingException"/>
		protected internal virtual char _handleUnrecognizedCharacterEscape(char ch)
		{
			// as per [JACKSON-300]
			if (isEnabled(com.fasterxml.jackson.core.JsonParser.Feature.ALLOW_BACKSLASH_ESCAPING_ANY_CHARACTER
				))
			{
				return ch;
			}
			// and [JACKSON-548]
			if (ch == '\'' && isEnabled(com.fasterxml.jackson.core.JsonParser.Feature.ALLOW_SINGLE_QUOTES
				))
			{
				return ch;
			}
			_reportError("Unrecognized character escape " + _getCharDesc(ch));
			return ch;
		}

		/*
		/**********************************************************
		/* Error reporting, generic
		/**********************************************************
		*/
		protected internal static string _getCharDesc(int ch)
		{
			char c = (char)ch;
			if (char.isISOControl(c))
			{
				return "(CTRL-CHAR, code " + ch + ")";
			}
			if (ch > 255)
			{
				return "'" + c + "' (code " + ch + " / 0x" + Sharpen.Extensions.ToHexString(ch) +
					 ")";
			}
			return "'" + c + "' (code " + ch + ")";
		}

		/// <exception cref="com.fasterxml.jackson.core.JsonParseException"/>
		protected internal void _reportError(string msg)
		{
			throw _constructError(msg);
		}

		/// <exception cref="com.fasterxml.jackson.core.JsonParseException"/>
		protected internal void _wrapError(string msg, System.Exception t)
		{
			throw _constructError(msg, t);
		}

		protected internal void _throwInternal()
		{
			com.fasterxml.jackson.core.util.VersionUtil.throwInternal();
		}

		protected internal com.fasterxml.jackson.core.JsonParseException _constructError(
			string msg, System.Exception t)
		{
			return new com.fasterxml.jackson.core.JsonParseException(msg, getCurrentLocation(
				), t);
		}

		protected internal static byte[] _asciiBytes(string str)
		{
			byte[] b = new byte[str.Length];
			for (int i = 0; i < len; ++i)
			{
				b[i] = unchecked((byte)str[i]);
			}
			return b;
		}

		protected internal static string _ascii(byte[] b)
		{
			try
			{
				return Sharpen.Runtime.getStringForBytes(b, "US-ASCII");
			}
			catch (System.IO.IOException e)
			{
				// never occurs
				throw new Sharpen.RuntimeException(e);
			}
		}
	}
}
