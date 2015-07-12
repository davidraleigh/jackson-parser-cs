using Sharpen;

namespace com.fasterxml.jackson.core.@base
{
	/// <summary>
	/// This base class implements part of API that a JSON generator exposes
	/// to applications, adds shared internal methods that sub-classes
	/// can use and adds some abstract methods sub-classes must implement.
	/// </summary>
	public abstract class GeneratorBase : com.fasterxml.jackson.core.JsonGenerator
	{
		public const int SURR1_FIRST = unchecked((int)(0xD800));

		public const int SURR1_LAST = unchecked((int)(0xDBFF));

		public const int SURR2_FIRST = unchecked((int)(0xDC00));

		public const int SURR2_LAST = unchecked((int)(0xDFFF));

		/// <summary>
		/// Set of feature masks related to features that need updates of other
		/// local configuration or state.
		/// </summary>
		/// <since>2.5</since>
		protected internal static readonly int DERIVED_FEATURES_MASK = com.fasterxml.jackson.core.JsonGenerator.Feature
			.WRITE_NUMBERS_AS_STRINGS.getMask() | com.fasterxml.jackson.core.JsonGenerator.Feature
			.ESCAPE_NON_ASCII.getMask() | com.fasterxml.jackson.core.JsonGenerator.Feature.STRICT_DUPLICATE_DETECTION
			.getMask();

		protected internal readonly string WRITE_BINARY = "write a binary value";

		protected internal readonly string WRITE_BOOLEAN = "write a boolean value";

		protected internal readonly string WRITE_NULL = "write a null";

		protected internal readonly string WRITE_NUMBER = "write a number";

		protected internal readonly string WRITE_RAW = "write a raw (unencoded) value";

		protected internal readonly string WRITE_STRING = "write a string";

		protected internal com.fasterxml.jackson.core.ObjectCodec _objectCodec;

		/// <summary>
		/// Bit flag composed of bits that indicate which
		/// <see cref="com.fasterxml.jackson.core.JsonGenerator.Feature"/>
		/// s
		/// are enabled.
		/// </summary>
		protected internal int _features;

		/// <summary>
		/// Flag set to indicate that implicit conversion from number
		/// to JSON String is needed (as per
		/// <see cref="com.fasterxml.jackson.core.JsonGenerator.Feature.WRITE_NUMBERS_AS_STRINGS
		/// 	"/>
		/// ).
		/// </summary>
		protected internal bool _cfgNumbersAsStrings;

		/// <summary>
		/// Object that keeps track of the current contextual state
		/// of the generator.
		/// </summary>
		protected internal com.fasterxml.jackson.core.json.JsonWriteContext _writeContext;

		/// <summary>Flag that indicates whether generator is closed or not.</summary>
		/// <remarks>
		/// Flag that indicates whether generator is closed or not. Gets
		/// set when it is closed by an explicit call
		/// (
		/// <see cref="close()"/>
		/// ).
		/// </remarks>
		protected internal bool _closed;

		protected internal GeneratorBase(int features, com.fasterxml.jackson.core.ObjectCodec
			 codec)
			: base()
		{
			// // // Constants for validation messages (since 2.6)
			/*
			/**********************************************************
			/* Configuration
			/**********************************************************
			*/
			/*
			/**********************************************************
			/* State
			/**********************************************************
			*/
			/*
			/**********************************************************
			/* Life-cycle
			/**********************************************************
			*/
			_features = features;
			_objectCodec = codec;
			com.fasterxml.jackson.core.json.DupDetector dups = com.fasterxml.jackson.core.JsonGenerator.Feature
				.STRICT_DUPLICATE_DETECTION.enabledIn(features) ? com.fasterxml.jackson.core.json.DupDetector
				.rootDetector(this) : null;
			_writeContext = com.fasterxml.jackson.core.json.JsonWriteContext.createRootContext
				(dups);
			_cfgNumbersAsStrings = com.fasterxml.jackson.core.JsonGenerator.Feature.WRITE_NUMBERS_AS_STRINGS
				.enabledIn(features);
		}

		/// <since>2.5</since>
		protected internal GeneratorBase(int features, com.fasterxml.jackson.core.ObjectCodec
			 codec, com.fasterxml.jackson.core.json.JsonWriteContext ctxt)
			: base()
		{
			_features = features;
			_objectCodec = codec;
			_writeContext = ctxt;
			_cfgNumbersAsStrings = com.fasterxml.jackson.core.JsonGenerator.Feature.WRITE_NUMBERS_AS_STRINGS
				.enabledIn(features);
		}

		/// <summary>
		/// Implemented with standard version number detection algorithm, typically using
		/// a simple generated class, with information extracted from Maven project file
		/// during build.
		/// </summary>
		public override com.fasterxml.jackson.core.Version version()
		{
			return com.fasterxml.jackson.core.util.VersionUtil.versionFor(GetType());
		}

		public override object getCurrentValue()
		{
			return _writeContext.getCurrentValue();
		}

		public override void setCurrentValue(object v)
		{
			_writeContext.setCurrentValue(v);
		}

		/*
		/**********************************************************
		/* Configuration
		/**********************************************************
		*/
		public sealed override bool isEnabled(com.fasterxml.jackson.core.JsonGenerator.Feature
			 f)
		{
			return (_features & f.getMask()) != 0;
		}

		public override int getFeatureMask()
		{
			return _features;
		}

		//public JsonGenerator configure(Feature f, boolean state) { }
		public override com.fasterxml.jackson.core.JsonGenerator enable(com.fasterxml.jackson.core.JsonGenerator.Feature
			 f)
		{
			int mask = f.getMask();
			_features |= mask;
			if ((mask & DERIVED_FEATURES_MASK) != 0)
			{
				if (f == com.fasterxml.jackson.core.JsonGenerator.Feature.WRITE_NUMBERS_AS_STRINGS)
				{
					_cfgNumbersAsStrings = true;
				}
				else
				{
					if (f == com.fasterxml.jackson.core.JsonGenerator.Feature.ESCAPE_NON_ASCII)
					{
						setHighestNonEscapedChar(127);
					}
					else
					{
						if (f == com.fasterxml.jackson.core.JsonGenerator.Feature.STRICT_DUPLICATE_DETECTION)
						{
							if (_writeContext.getDupDetector() == null)
							{
								// but only if disabled currently
								_writeContext = _writeContext.withDupDetector(com.fasterxml.jackson.core.json.DupDetector
									.rootDetector(this));
							}
						}
					}
				}
			}
			return this;
		}

		public override com.fasterxml.jackson.core.JsonGenerator disable(com.fasterxml.jackson.core.JsonGenerator.Feature
			 f)
		{
			int mask = f.getMask();
			_features &= ~mask;
			if ((mask & DERIVED_FEATURES_MASK) != 0)
			{
				if (f == com.fasterxml.jackson.core.JsonGenerator.Feature.WRITE_NUMBERS_AS_STRINGS)
				{
					_cfgNumbersAsStrings = false;
				}
				else
				{
					if (f == com.fasterxml.jackson.core.JsonGenerator.Feature.ESCAPE_NON_ASCII)
					{
						setHighestNonEscapedChar(0);
					}
					else
					{
						if (f == com.fasterxml.jackson.core.JsonGenerator.Feature.STRICT_DUPLICATE_DETECTION)
						{
							_writeContext = _writeContext.withDupDetector(null);
						}
					}
				}
			}
			return this;
		}

		public override com.fasterxml.jackson.core.JsonGenerator setFeatureMask(int newMask
			)
		{
			int changed = newMask ^ _features;
			_features = newMask;
			if ((changed & DERIVED_FEATURES_MASK) != 0)
			{
				_cfgNumbersAsStrings = com.fasterxml.jackson.core.JsonGenerator.Feature.WRITE_NUMBERS_AS_STRINGS
					.enabledIn(newMask);
				if (com.fasterxml.jackson.core.JsonGenerator.Feature.ESCAPE_NON_ASCII.enabledIn(changed
					))
				{
					if (com.fasterxml.jackson.core.JsonGenerator.Feature.ESCAPE_NON_ASCII.enabledIn(newMask
						))
					{
						setHighestNonEscapedChar(127);
					}
					else
					{
						setHighestNonEscapedChar(0);
					}
				}
				if (com.fasterxml.jackson.core.JsonGenerator.Feature.STRICT_DUPLICATE_DETECTION.enabledIn
					(changed))
				{
					if (com.fasterxml.jackson.core.JsonGenerator.Feature.STRICT_DUPLICATE_DETECTION.enabledIn
						(newMask))
					{
						// enabling
						if (_writeContext.getDupDetector() == null)
						{
							// but only if disabled currently
							_writeContext = _writeContext.withDupDetector(com.fasterxml.jackson.core.json.DupDetector
								.rootDetector(this));
						}
					}
					else
					{
						// disabling
						_writeContext = _writeContext.withDupDetector(null);
					}
				}
			}
			return this;
		}

		public override com.fasterxml.jackson.core.JsonGenerator useDefaultPrettyPrinter(
			)
		{
			/* 28-Sep-2012, tatu: As per [Issue#84], should not override a
			*  pretty printer if one already assigned.
			*/
			if (getPrettyPrinter() != null)
			{
				return this;
			}
			return setPrettyPrinter(_constructDefaultPrettyPrinter());
		}

		public override com.fasterxml.jackson.core.JsonGenerator setCodec(com.fasterxml.jackson.core.ObjectCodec
			 oc)
		{
			_objectCodec = oc;
			return this;
		}

		public sealed override com.fasterxml.jackson.core.ObjectCodec getCodec()
		{
			return _objectCodec;
		}

		/*
		/**********************************************************
		/* Public API, accessors
		/**********************************************************
		*/
		/// <summary>Note: co-variant return type.</summary>
		public sealed override com.fasterxml.jackson.core.JsonStreamContext getOutputContext
			()
		{
			return _writeContext;
		}

		/*
		/**********************************************************
		/* Public API, write methods, structural
		/**********************************************************
		*/
		//public void writeStartArray() throws IOException
		//public void writeEndArray() throws IOException
		//public void writeStartObject() throws IOException
		//public void writeEndObject() throws IOException
		/*
		/**********************************************************
		/* Public API, write methods, textual
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		public override void writeFieldName(com.fasterxml.jackson.core.SerializableString
			 name)
		{
			writeFieldName(name.getValue());
		}

		//public abstract void writeString(String text) throws IOException;
		//public abstract void writeString(char[] text, int offset, int len) throws IOException;
		//public abstract void writeRaw(String text) throws IOException,;
		//public abstract void writeRaw(char[] text, int offset, int len) throws IOException;
		/// <exception cref="System.IO.IOException"/>
		public override void writeString(com.fasterxml.jackson.core.SerializableString text
			)
		{
			writeString(text.getValue());
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeRawValue(string text)
		{
			_verifyValueWrite("write raw value");
			writeRaw(text);
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeRawValue(string text, int offset, int len)
		{
			_verifyValueWrite("write raw value");
			writeRaw(text, offset, len);
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeRawValue(char[] text, int offset, int len)
		{
			_verifyValueWrite("write raw value");
			writeRaw(text, offset, len);
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeRawValue(com.fasterxml.jackson.core.SerializableString 
			text)
		{
			_verifyValueWrite("write raw value");
			writeRaw(text);
		}

		/// <exception cref="System.IO.IOException"/>
		public override int writeBinary(com.fasterxml.jackson.core.Base64Variant b64variant
			, Sharpen.InputStream data, int dataLength)
		{
			// Let's implement this as "unsupported" to make it easier to add new parser impls
			_reportUnsupportedOperation();
			return 0;
		}

		/*
		/**********************************************************
		/* Public API, write methods, primitive
		/**********************************************************
		*/
		// Not implemented at this level, added as placeholders
		/*
		public abstract void writeNumber(int i)
		public abstract void writeNumber(long l)
		public abstract void writeNumber(double d)
		public abstract void writeNumber(float f)
		public abstract void writeNumber(BigDecimal dec)
		public abstract void writeBoolean(boolean state)
		public abstract void writeNull()
		*/
		/*
		/**********************************************************
		/* Public API, write methods, POJOs, trees
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		public override void writeObject(object value)
		{
			if (value == null)
			{
				// important: call method that does check value write:
				writeNull();
			}
			else
			{
				/* 02-Mar-2009, tatu: we are NOT to call _verifyValueWrite here,
				*   because that will be done when codec actually serializes
				*   contained POJO. If we did call it it would advance state
				*   causing exception later on
				*/
				if (_objectCodec != null)
				{
					_objectCodec.writeValue(this, value);
					return;
				}
				_writeSimpleObject(value);
			}
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeTree(com.fasterxml.jackson.core.TreeNode rootNode)
		{
			// As with 'writeObject()', we are not check if write would work
			if (rootNode == null)
			{
				writeNull();
			}
			else
			{
				if (_objectCodec == null)
				{
					throw new System.InvalidOperationException("No ObjectCodec defined");
				}
				_objectCodec.writeValue(this, rootNode);
			}
		}

		/*
		/**********************************************************
		/* Public API, low-level output handling
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		public abstract override void flush();

		/// <exception cref="System.IO.IOException"/>
		public override void close()
		{
			_closed = true;
		}

		public override bool isClosed()
		{
			return _closed;
		}

		/*
		/**********************************************************
		/* Package methods for this, sub-classes
		/**********************************************************
		*/
		/// <summary>
		/// Method called to release any buffers generator may be holding,
		/// once generator is being closed.
		/// </summary>
		protected internal abstract void _releaseBuffers();

		/// <summary>
		/// Method called before trying to write a value (scalar or structured),
		/// to verify that this is legal in current output state, as well as to
		/// output separators if and as necessary.
		/// </summary>
		/// <param name="typeMsg">
		/// Additional message used for generating exception message
		/// if value output is NOT legal in current generator output state.
		/// </param>
		/// <exception cref="System.IO.IOException"/>
		protected internal abstract void _verifyValueWrite(string typeMsg);

		/// <summary>
		/// Overridable factory method called to instantiate an appropriate
		/// <see cref="com.fasterxml.jackson.core.PrettyPrinter"/>
		/// for case of "just use the default one", when
		/// <see cref="useDefaultPrettyPrinter()"/>
		/// is called.
		/// </summary>
		/// <since>2.6</since>
		protected internal virtual com.fasterxml.jackson.core.PrettyPrinter _constructDefaultPrettyPrinter
			()
		{
			return new com.fasterxml.jackson.core.util.DefaultPrettyPrinter();
		}

		/*
		/**********************************************************
		/* UTF-8 related helper method(s)
		/**********************************************************
		*/
		/// <since>2.5</since>
		/// <exception cref="System.IO.IOException"/>
		protected internal int _decodeSurrogate(int surr1, int surr2)
		{
			// First is known to be valid, but how about the other?
			if (surr2 < SURR2_FIRST || surr2 > SURR2_LAST)
			{
				string msg = "Incomplete surrogate pair: first char 0x" + Sharpen.Extensions.ToHexString
					(surr1) + ", second 0x" + Sharpen.Extensions.ToHexString(surr2);
				_reportError(msg);
			}
			int c = unchecked((int)(0x10000)) + ((surr1 - SURR1_FIRST) << 10) + (surr2 - SURR2_FIRST
				);
			return c;
		}
	}
}
