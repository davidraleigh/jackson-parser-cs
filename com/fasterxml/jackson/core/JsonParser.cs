/* Jackson JSON-processor.
*
* Copyright (c) 2007- Tatu Saloranta, tatu.saloranta@iki.fi
*/
using Sharpen;

namespace com.fasterxml.jackson.core
{
	/// <summary>Base class that defines public API for reading JSON content.</summary>
	/// <remarks>
	/// Base class that defines public API for reading JSON content.
	/// Instances are created using factory methods of
	/// a
	/// <see cref="JsonFactory"/>
	/// instance.
	/// </remarks>
	/// <author>Tatu Saloranta</author>
	public abstract class JsonParser : System.IDisposable, com.fasterxml.jackson.core.Versioned
	{
		private const int MIN_BYTE_I = (int)byte.MinValue;

		private const int MAX_BYTE_I = (int)255;

		private const int MIN_SHORT_I = (int)short.MinValue;

		private const int MAX_SHORT_I = (int)short.MaxValue;

		/// <summary>
		/// Enumeration of possible "native" (optimal) types that can be
		/// used for numbers.
		/// </summary>
		public enum NumberType
		{
			INT,
			LONG,
			BIG_INTEGER,
			FLOAT,
			DOUBLE,
			BIG_DECIMAL
		}

		/// <summary>Enumeration that defines all on/off features for parsers.</summary>
		[System.Serializable]
		public sealed class Feature
		{
			/// <summary>
			/// Feature that determines whether parser will automatically
			/// close underlying input source that is NOT owned by the
			/// parser.
			/// </summary>
			/// <remarks>
			/// Feature that determines whether parser will automatically
			/// close underlying input source that is NOT owned by the
			/// parser. If disabled, calling application has to separately
			/// close the underlying
			/// <see cref="Sharpen.InputStream"/>
			/// and
			/// <see cref="System.IO.StreamReader"/>
			/// instances used to create the parser. If enabled, parser
			/// will handle closing, as long as parser itself gets closed:
			/// this happens when end-of-input is encountered, or parser
			/// is closed by a call to
			/// <see cref="JsonParser.close()"/>
			/// .
			/// <p>
			/// Feature is enabled by default.
			/// </remarks>
			public static readonly com.fasterxml.jackson.core.JsonParser.Feature AUTO_CLOSE_SOURCE
				 = new com.fasterxml.jackson.core.JsonParser.Feature(true);

			/// <summary>
			/// Feature that determines whether parser will allow use
			/// of Java/C++ style comments (both '/'+'*' and
			/// '//' varieties) within parsed content or not.
			/// </summary>
			/// <remarks>
			/// Feature that determines whether parser will allow use
			/// of Java/C++ style comments (both '/'+'*' and
			/// '//' varieties) within parsed content or not.
			/// <p>
			/// Since JSON specification does not mention comments as legal
			/// construct,
			/// this is a non-standard feature; however, in the wild
			/// this is extensively used. As such, feature is
			/// <b>disabled by default</b> for parsers and must be
			/// explicitly enabled.
			/// </remarks>
			public static readonly com.fasterxml.jackson.core.JsonParser.Feature ALLOW_COMMENTS
				 = new com.fasterxml.jackson.core.JsonParser.Feature(false);

			/// <summary>
			/// Feature that determines whether parser will allow use
			/// of YAML comments, ones starting with '#' and continuing
			/// until the end of the line.
			/// </summary>
			/// <remarks>
			/// Feature that determines whether parser will allow use
			/// of YAML comments, ones starting with '#' and continuing
			/// until the end of the line. This commenting style is common
			/// with scripting languages as well.
			/// <p>
			/// Since JSON specification does not mention comments as legal
			/// construct,
			/// this is a non-standard feature. As such, feature is
			/// <b>disabled by default</b> for parsers and must be
			/// explicitly enabled.
			/// </remarks>
			public static readonly com.fasterxml.jackson.core.JsonParser.Feature ALLOW_YAML_COMMENTS
				 = new com.fasterxml.jackson.core.JsonParser.Feature(false);

			/// <summary>
			/// Feature that determines whether parser will allow use
			/// of unquoted field names (which is allowed by Javascript,
			/// but not by JSON specification).
			/// </summary>
			/// <remarks>
			/// Feature that determines whether parser will allow use
			/// of unquoted field names (which is allowed by Javascript,
			/// but not by JSON specification).
			/// <p>
			/// Since JSON specification requires use of double quotes for
			/// field names,
			/// this is a non-standard feature, and as such disabled by default.
			/// </remarks>
			public static readonly com.fasterxml.jackson.core.JsonParser.Feature ALLOW_UNQUOTED_FIELD_NAMES
				 = new com.fasterxml.jackson.core.JsonParser.Feature(false);

			/// <summary>
			/// Feature that determines whether parser will allow use
			/// of single quotes (apostrophe, character '\'') for
			/// quoting Strings (names and String values).
			/// </summary>
			/// <remarks>
			/// Feature that determines whether parser will allow use
			/// of single quotes (apostrophe, character '\'') for
			/// quoting Strings (names and String values). If so,
			/// this is in addition to other acceptabl markers.
			/// but not by JSON specification).
			/// <p>
			/// Since JSON specification requires use of double quotes for
			/// field names,
			/// this is a non-standard feature, and as such disabled by default.
			/// </remarks>
			public static readonly com.fasterxml.jackson.core.JsonParser.Feature ALLOW_SINGLE_QUOTES
				 = new com.fasterxml.jackson.core.JsonParser.Feature(false);

			/// <summary>
			/// Feature that determines whether parser will allow
			/// JSON Strings to contain unquoted control characters
			/// (ASCII characters with value less than 32, including
			/// tab and line feed characters) or not.
			/// </summary>
			/// <remarks>
			/// Feature that determines whether parser will allow
			/// JSON Strings to contain unquoted control characters
			/// (ASCII characters with value less than 32, including
			/// tab and line feed characters) or not.
			/// If feature is set false, an exception is thrown if such a
			/// character is encountered.
			/// <p>
			/// Since JSON specification requires quoting for all control characters,
			/// this is a non-standard feature, and as such disabled by default.
			/// </remarks>
			public static readonly com.fasterxml.jackson.core.JsonParser.Feature ALLOW_UNQUOTED_CONTROL_CHARS
				 = new com.fasterxml.jackson.core.JsonParser.Feature(false);

			/// <summary>
			/// Feature that can be enabled to accept quoting of all character
			/// using backslash quoting mechanism: if not enabled, only characters
			/// that are explicitly listed by JSON specification can be thus
			/// escaped (see JSON spec for small list of these characters)
			/// <p>
			/// Since JSON specification requires quoting for all control characters,
			/// this is a non-standard feature, and as such disabled by default.
			/// </summary>
			public static readonly com.fasterxml.jackson.core.JsonParser.Feature ALLOW_BACKSLASH_ESCAPING_ANY_CHARACTER
				 = new com.fasterxml.jackson.core.JsonParser.Feature(false);

			/// <summary>
			/// Feature that determines whether parser will allow
			/// JSON integral numbers to start with additional (ignorable)
			/// zeroes (like: 000001).
			/// </summary>
			/// <remarks>
			/// Feature that determines whether parser will allow
			/// JSON integral numbers to start with additional (ignorable)
			/// zeroes (like: 000001). If enabled, no exception is thrown, and extra
			/// nulls are silently ignored (and not included in textual representation
			/// exposed via
			/// <see cref="JsonParser.getText()"/>
			/// ).
			/// <p>
			/// Since JSON specification does not allow leading zeroes,
			/// this is a non-standard feature, and as such disabled by default.
			/// </remarks>
			public static readonly com.fasterxml.jackson.core.JsonParser.Feature ALLOW_NUMERIC_LEADING_ZEROS
				 = new com.fasterxml.jackson.core.JsonParser.Feature(false);

			/// <summary>
			/// Feature that allows parser to recognize set of
			/// "Not-a-Number" (NaN) tokens as legal floating number
			/// values (similar to how many other data formats and
			/// programming language source code allows it).
			/// </summary>
			/// <remarks>
			/// Feature that allows parser to recognize set of
			/// "Not-a-Number" (NaN) tokens as legal floating number
			/// values (similar to how many other data formats and
			/// programming language source code allows it).
			/// Specific subset contains values that
			/// <a href="http://www.w3.org/TR/xmlschema-2/">XML Schema</a>
			/// (see section 3.2.4.1, Lexical Representation)
			/// allows (tokens are quoted contents, not including quotes):
			/// <ul>
			/// <li>"INF" (for positive infinity), as well as alias of "Infinity"
			/// <li>"-INF" (for negative infinity), alias "-Infinity"
			/// <li>"NaN" (for other not-a-numbers, like result of division by zero)
			/// </ul>
			/// <p>
			/// Since JSON specification does not allow use of such values,
			/// this is a non-standard feature, and as such disabled by default.
			/// </remarks>
			public static readonly com.fasterxml.jackson.core.JsonParser.Feature ALLOW_NON_NUMERIC_NUMBERS
				 = new com.fasterxml.jackson.core.JsonParser.Feature(false);

			/// <summary>
			/// Feature that determines whether
			/// <see cref="JsonParser"/>
			/// will explicitly
			/// check that no duplicate JSON Object field names are encountered.
			/// If enabled, parser will check all names within context and report
			/// duplicates by throwing a
			/// <see cref="JsonParseException"/>
			/// ; if disabled,
			/// parser will not do such checking. Assumption in latter case is
			/// that caller takes care of handling duplicates at a higher level:
			/// data-binding, for example, has features to specify detection to
			/// be done there.
			/// <p>
			/// Note that enabling this feature will incur performance overhead
			/// due to having to store and check additional information: this typically
			/// adds 20-30% to execution time for basic parsing.
			/// </summary>
			/// <since>2.3</since>
			public static readonly com.fasterxml.jackson.core.JsonParser.Feature STRICT_DUPLICATE_DETECTION
				 = new com.fasterxml.jackson.core.JsonParser.Feature(false);

			/// <summary>
			/// Feature that determines what to do if the underlying data format requires knowledge
			/// of all properties to decode (usually via a Schema), and if no definition is
			/// found for a property that input content contains.
			/// </summary>
			/// <remarks>
			/// Feature that determines what to do if the underlying data format requires knowledge
			/// of all properties to decode (usually via a Schema), and if no definition is
			/// found for a property that input content contains.
			/// Typically most textual data formats do NOT require schema information (although
			/// some do, such as CSV), whereas many binary data formats do require definitions
			/// (such as Avro, protobuf), although not all (Smile, CBOR, BSON and MessagePack do not).
			/// Further note that some formats that do require schema information will not be able
			/// to ignore undefined properties: for example, Avro is fully positional and there is
			/// no possibility of undefined data. This leaves formats like Protobuf that have identifiers
			/// that may or may not map; and as such Protobuf format does make use of this feature.
			/// <p>
			/// Note that support for this feature is implemented by individual data format
			/// module, if (and only if) it makes sense for the format in question. For JSON,
			/// for example, this feature has no effect as properties need not be pre-defined.
			/// <p>
			/// Feature is disabled by default, meaning that if the underlying data format
			/// requires knowledge of all properties to output, attempts to read an unknown
			/// property will result in a
			/// <see cref="JsonProcessingException"/>
			/// </remarks>
			/// <since>2.6</since>
			public static readonly com.fasterxml.jackson.core.JsonParser.Feature IGNORE_UNDEFINED
				 = new com.fasterxml.jackson.core.JsonParser.Feature(false);

			/// <summary>Whether feature is enabled or disabled by default.</summary>
			private readonly bool _defaultState;

			private readonly int _mask;

			// as per [JACKSON-804], allow range up to and including 255
			// // // Low-level I/O handling features:
			// // // Support for non-standard data format constructs
			/// <summary>
			/// Method that calculates bit set (flags) of all features that
			/// are enabled by default.
			/// </summary>
			public static int collectDefaults()
			{
				int flags = 0;
				foreach (com.fasterxml.jackson.core.JsonParser.Feature f in values())
				{
					if (f.enabledByDefault())
					{
						flags |= f.getMask();
					}
				}
				return flags;
			}

			private Feature(bool defaultState)
			{
				com.fasterxml.jackson.core.JsonParser.Feature._mask = (1 << ordinal());
				com.fasterxml.jackson.core.JsonParser.Feature._defaultState = defaultState;
			}

			public bool enabledByDefault()
			{
				return com.fasterxml.jackson.core.JsonParser.Feature._defaultState;
			}

			/// <since>2.3</since>
			public bool enabledIn(int flags)
			{
				return (flags & com.fasterxml.jackson.core.JsonParser.Feature._mask) != 0;
			}

			public int getMask()
			{
				return com.fasterxml.jackson.core.JsonParser.Feature._mask;
			}
		}

		/// <summary>
		/// Bit flag composed of bits that indicate which
		/// <see cref="Feature"/>
		/// s
		/// are enabled.
		/// </summary>
		protected internal int _features;

		protected internal JsonParser()
		{
		}

		protected internal JsonParser(int features)
		{
			/*
			/**********************************************************
			/* Minimal configuration state
			/**********************************************************
			*/
			/*
			/**********************************************************
			/* Construction, configuration, initialization
			/**********************************************************
			*/
			_features = features;
		}

		/// <summary>
		/// Accessor for
		/// <see cref="ObjectCodec"/>
		/// associated with this
		/// parser, if any. Codec is used by
		/// <see cref="readValueAs{T}(System.Type{T})"/>
		/// method (and its variants).
		/// </summary>
		public abstract com.fasterxml.jackson.core.ObjectCodec getCodec();

		/// <summary>
		/// Setter that allows defining
		/// <see cref="ObjectCodec"/>
		/// associated with this
		/// parser, if any. Codec is used by
		/// <see cref="readValueAs{T}(System.Type{T})"/>
		/// method (and its variants).
		/// </summary>
		public abstract void setCodec(com.fasterxml.jackson.core.ObjectCodec c);

		/// <summary>
		/// Method that can be used to get access to object that is used
		/// to access input being parsed; this is usually either
		/// <see cref="Sharpen.InputStream"/>
		/// or
		/// <see cref="System.IO.StreamReader"/>
		/// , depending on what
		/// parser was constructed with.
		/// Note that returned value may be null in some cases; including
		/// case where parser implementation does not want to exposed raw
		/// source to caller.
		/// In cases where input has been decorated, object returned here
		/// is the decorated version; this allows some level of interaction
		/// between users of parser and decorator object.
		/// <p>
		/// In general use of this accessor should be considered as
		/// "last effort", i.e. only used if no other mechanism is applicable.
		/// </summary>
		public virtual object getInputSource()
		{
			return null;
		}

		/// <summary>
		/// Helper method, usually equivalent to:
		/// <code>
		/// getParsingContext().getCurrentValue();
		/// </code>
		/// </summary>
		/// <since>2.5</since>
		public virtual object getCurrentValue()
		{
			com.fasterxml.jackson.core.JsonStreamContext ctxt = getParsingContext();
			return (ctxt == null) ? null : ctxt.getCurrentValue();
		}

		/// <summary>
		/// Helper method, usually equivalent to:
		/// <code>
		/// getParsingContext().setCurrentValue(v);
		/// </code>
		/// </summary>
		/// <since>2.5</since>
		public virtual void setCurrentValue(object v)
		{
			com.fasterxml.jackson.core.JsonStreamContext ctxt = getParsingContext();
			if (ctxt != null)
			{
				ctxt.setCurrentValue(v);
			}
		}

		/*
		/**********************************************************
		/* Format support
		/**********************************************************
		*/
		/// <summary>Method to call to make this parser use specified schema.</summary>
		/// <remarks>
		/// Method to call to make this parser use specified schema. Method must
		/// be called before trying to parse any content, right after parser instance
		/// has been created.
		/// Note that not all parsers support schemas; and those that do usually only
		/// accept specific types of schemas: ones defined for data format parser can read.
		/// <p>
		/// If parser does not support specified schema,
		/// <see cref="System.NotSupportedException"/>
		/// is thrown.
		/// </remarks>
		/// <param name="schema">Schema to use</param>
		/// <exception cref="System.NotSupportedException">if parser does not support schema</exception>
		public virtual void setSchema(com.fasterxml.jackson.core.FormatSchema schema)
		{
			throw new System.NotSupportedException("Parser of type " + GetType().FullName + " does not support schema of type '"
				 + schema.getSchemaType() + "'");
		}

		/// <summary>Method for accessing Schema that this parser uses, if any.</summary>
		/// <remarks>
		/// Method for accessing Schema that this parser uses, if any.
		/// Default implementation returns null.
		/// </remarks>
		/// <since>2.1</since>
		public virtual com.fasterxml.jackson.core.FormatSchema getSchema()
		{
			return null;
		}

		/// <summary>
		/// Method that can be used to verify that given schema can be used with
		/// this parser (using
		/// <see cref="setSchema(FormatSchema)"/>
		/// ).
		/// </summary>
		/// <param name="schema">Schema to check</param>
		/// <returns>True if this parser can use given schema; false if not</returns>
		public virtual bool canUseSchema(com.fasterxml.jackson.core.FormatSchema schema)
		{
			return false;
		}

		/*
		/**********************************************************
		/* Capability introspection
		/**********************************************************
		*/
		/// <summary>
		/// Method that can be called to determine if a custom
		/// <see cref="ObjectCodec"/>
		/// is needed for binding data parsed
		/// using
		/// <see cref="JsonParser"/>
		/// constructed by this factory
		/// (which typically also implies the same for serialization
		/// with
		/// <see cref="JsonGenerator"/>
		/// ).
		/// </summary>
		/// <returns>
		/// True if custom codec is needed with parsers and
		/// generators created by this factory; false if a general
		/// <see cref="ObjectCodec"/>
		/// is enough
		/// </returns>
		/// <since>2.1</since>
		public virtual bool requiresCustomCodec()
		{
			return false;
		}

		/*
		/**********************************************************
		/* Versioned
		/**********************************************************
		*/
		/// <summary>Accessor for getting version of the core package, given a parser instance.
		/// 	</summary>
		/// <remarks>
		/// Accessor for getting version of the core package, given a parser instance.
		/// Left for sub-classes to implement.
		/// </remarks>
		public abstract com.fasterxml.jackson.core.Version version();

		/*
		/**********************************************************
		/* Closeable implementation
		/**********************************************************
		*/
		/// <summary>
		/// Closes the parser so that no further iteration or data access
		/// can be made; will also close the underlying input source
		/// if parser either <b>owns</b> the input source, or feature
		/// <see cref="Feature.AUTO_CLOSE_SOURCE"/>
		/// is enabled.
		/// Whether parser owns the input source depends on factory
		/// method that was used to construct instance (so check
		/// <see cref="JsonFactory"/>
		/// for details,
		/// but the general
		/// idea is that if caller passes in closable resource (such
		/// as
		/// <see cref="Sharpen.InputStream"/>
		/// or
		/// <see cref="System.IO.StreamReader"/>
		/// ) parser does NOT
		/// own the source; but if it passes a reference (such as
		/// <see cref="Sharpen.FilePath"/>
		/// or
		/// <see cref="System.Uri"/>
		/// and creates
		/// stream or reader it does own them.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		public abstract void close();

		/*
		/**********************************************************
		/* Buffer handling
		/**********************************************************
		*/
		/// <summary>
		/// Method that can be called to push back any content that
		/// has been read but not consumed by the parser.
		/// </summary>
		/// <remarks>
		/// Method that can be called to push back any content that
		/// has been read but not consumed by the parser. This is usually
		/// done after reading all content of interest using parser.
		/// Content is released by writing it to given stream if possible;
		/// if underlying input is byte-based it can released, if not (char-based)
		/// it can not.
		/// </remarks>
		/// <returns>
		/// -1 if the underlying content source is not byte based
		/// (that is, input can not be sent to
		/// <see cref="Sharpen.OutputStream"/>
		/// ;
		/// otherwise number of bytes released (0 if there was nothing to release)
		/// </returns>
		/// <exception cref="System.IO.IOException">if write to stream threw exception</exception>
		public virtual int releaseBuffered(Sharpen.OutputStream @out)
		{
			return -1;
		}

		/// <summary>
		/// Method that can be called to push back any content that
		/// has been read but not consumed by the parser.
		/// </summary>
		/// <remarks>
		/// Method that can be called to push back any content that
		/// has been read but not consumed by the parser.
		/// This is usually
		/// done after reading all content of interest using parser.
		/// Content is released by writing it to given writer if possible;
		/// if underlying input is char-based it can released, if not (byte-based)
		/// it can not.
		/// </remarks>
		/// <returns>
		/// -1 if the underlying content source is not char-based
		/// (that is, input can not be sent to
		/// <see cref="System.IO.TextWriter"/>
		/// ;
		/// otherwise number of chars released (0 if there was nothing to release)
		/// </returns>
		/// <exception cref="System.IO.IOException">if write using Writer threw exception</exception>
		public virtual int releaseBuffered(System.IO.TextWriter w)
		{
			return -1;
		}

		/*
		/***************************************************
		/* Public API, configuration
		/***************************************************
		*/
		/// <summary>
		/// Method for enabling specified parser feature
		/// (check
		/// <see cref="Feature"/>
		/// for list of features)
		/// </summary>
		public virtual com.fasterxml.jackson.core.JsonParser enable(com.fasterxml.jackson.core.JsonParser.Feature
			 f)
		{
			_features |= f.getMask();
			return this;
		}

		/// <summary>
		/// Method for disabling specified  feature
		/// (check
		/// <see cref="Feature"/>
		/// for list of features)
		/// </summary>
		public virtual com.fasterxml.jackson.core.JsonParser disable(com.fasterxml.jackson.core.JsonParser.Feature
			 f)
		{
			_features &= ~f.getMask();
			return this;
		}

		/// <summary>
		/// Method for enabling or disabling specified feature
		/// (check
		/// <see cref="Feature"/>
		/// for list of features)
		/// </summary>
		public virtual com.fasterxml.jackson.core.JsonParser configure(com.fasterxml.jackson.core.JsonParser.Feature
			 f, bool state)
		{
			if (state)
			{
				enable(f);
			}
			else
			{
				disable(f);
			}
			return this;
		}

		/// <summary>
		/// Method for checking whether specified
		/// <see cref="Feature"/>
		/// is enabled.
		/// </summary>
		public virtual bool isEnabled(com.fasterxml.jackson.core.JsonParser.Feature f)
		{
			return f.enabledIn(_features);
		}

		/// <summary>
		/// Bulk access method for getting state of all standard
		/// <see cref="Feature"/>
		/// s.
		/// </summary>
		/// <returns>
		/// Bit mask that defines current states of all standard
		/// <see cref="Feature"/>
		/// s.
		/// </returns>
		/// <since>2.3</since>
		public virtual int getFeatureMask()
		{
			return _features;
		}

		/// <summary>
		/// Bulk set method for (re)setting states of all standard
		/// <see cref="Feature"/>
		/// s
		/// </summary>
		/// <returns>This parser object, to allow chaining of calls</returns>
		/// <since>2.3</since>
		public virtual com.fasterxml.jackson.core.JsonParser setFeatureMask(int mask)
		{
			_features = mask;
			return this;
		}

		/// <summary>Bulk set method for (re)setting states of features specified by <code>mask</code>.
		/// 	</summary>
		/// <remarks>
		/// Bulk set method for (re)setting states of features specified by <code>mask</code>.
		/// Functionally equivalent to
		/// <code>
		/// int oldState = getFeatureMask();
		/// int newState = (oldState &amp; ~mask) | (values &amp; mask);
		/// setFeatureMask(newState);
		/// </code>
		/// </remarks>
		/// <param name="values">Bit mask of set/clear state for features to change</param>
		/// <param name="mask">Bit mask of features to change</param>
		/// <since>2.6</since>
		public virtual com.fasterxml.jackson.core.JsonParser overrideStdFeatures(int values
			, int mask)
		{
			_features = (_features & ~mask) | (values & mask);
			return this;
		}

		/// <summary>
		/// Bulk access method for getting state of all
		/// <see cref="FormatFeature"/>
		/// s, format-specific
		/// on/off configuration settings.
		/// </summary>
		/// <returns>
		/// Bit mask that defines current states of all standard
		/// <see cref="FormatFeature"/>
		/// s.
		/// </returns>
		/// <since>2.6</since>
		public virtual int getFormatFeatures()
		{
			return 0;
		}

		/// <summary>
		/// Bulk set method for (re)setting states of
		/// <see cref="FormatFeature"/>
		/// s,
		/// by specifying values (set / clear) along with a mask, to determine
		/// which features to change, if any.
		/// <p>
		/// Default implementation will simply throw an exception to indicate that
		/// the generator implementation does not support any
		/// <see cref="FormatFeature"/>
		/// s.
		/// </summary>
		/// <param name="values">Bit mask of set/clear state for features to change</param>
		/// <param name="mask">Bit mask of features to change</param>
		/// <since>2.6</since>
		public virtual com.fasterxml.jackson.core.JsonParser overrideFormatFeatures(int values
			, int mask)
		{
			throw new System.ArgumentException("No FormatFeatures defined for parser of type "
				 + GetType().FullName);
		}

		/*
		_formatFeatures = (_formatFeatures & ~mask) | (values & mask);
		*/
		/*
		/**********************************************************
		/* Public API, traversal
		/**********************************************************
		*/
		/// <summary>
		/// Main iteration method, which will advance stream enough
		/// to determine type of the next token, if any.
		/// </summary>
		/// <remarks>
		/// Main iteration method, which will advance stream enough
		/// to determine type of the next token, if any. If none
		/// remaining (stream has no content other than possible
		/// white space before ending), null will be returned.
		/// </remarks>
		/// <returns>
		/// Next token from the stream, if any found, or null
		/// to indicate end-of-input
		/// </returns>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonParseException"/>
		public abstract com.fasterxml.jackson.core.JsonToken nextToken();

		/// <summary>
		/// Iteration method that will advance stream enough
		/// to determine type of the next token that is a value type
		/// (including JSON Array and Object start/end markers).
		/// </summary>
		/// <remarks>
		/// Iteration method that will advance stream enough
		/// to determine type of the next token that is a value type
		/// (including JSON Array and Object start/end markers).
		/// Or put another way, nextToken() will be called once,
		/// and if
		/// <see cref="JsonToken.FIELD_NAME"/>
		/// is returned, another
		/// time to get the value for the field.
		/// Method is most useful for iterating over value entries
		/// of JSON objects; field name will still be available
		/// by calling
		/// <see cref="getCurrentName()"/>
		/// when parser points to
		/// the value.
		/// </remarks>
		/// <returns>
		/// Next non-field-name token from the stream, if any found,
		/// or null to indicate end-of-input (or, for non-blocking
		/// parsers,
		/// <see cref="JsonToken.NOT_AVAILABLE"/>
		/// if no tokens were
		/// available yet)
		/// </returns>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonParseException"/>
		public abstract com.fasterxml.jackson.core.JsonToken nextValue();

		/// <summary>
		/// Method that fetches next token (as if calling
		/// <see cref="nextToken()"/>
		/// ) and
		/// verifies whether it is
		/// <see cref="JsonToken.FIELD_NAME"/>
		/// with specified name
		/// and returns result of that comparison.
		/// It is functionally equivalent to:
		/// <pre>
		/// return (nextToken() == JsonToken.FIELD_NAME) &amp;&amp; str.getValue().equals(getCurrentName());
		/// </pre>
		/// but may be faster for parser to verify, and can therefore be used if caller
		/// expects to get such a property name from input next.
		/// </summary>
		/// <param name="str">
		/// Property name to compare next token to (if next token is
		/// <code>JsonToken.FIELD_NAME</code>)
		/// </param>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonParseException"/>
		public virtual bool nextFieldName(com.fasterxml.jackson.core.SerializableString str
			)
		{
			return (nextToken() == com.fasterxml.jackson.core.JsonToken.FIELD_NAME) && str.getValue
				().Equals(getCurrentName());
		}

		/// <summary>
		/// Method that fetches next token (as if calling
		/// <see cref="nextToken()"/>
		/// ) and
		/// verifies whether it is
		/// <see cref="JsonToken.FIELD_NAME"/>
		/// ; if it is,
		/// returns same as
		/// <see cref="getCurrentName()"/>
		/// , otherwise null.
		/// </summary>
		/// <since>2.5</since>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonParseException"/>
		public virtual string nextFieldName()
		{
			return (nextToken() == com.fasterxml.jackson.core.JsonToken.FIELD_NAME) ? getCurrentName
				() : null;
		}

		/// <summary>
		/// Method that fetches next token (as if calling
		/// <see cref="nextToken()"/>
		/// ) and
		/// if it is
		/// <see cref="JsonToken.VALUE_STRING"/>
		/// returns contained String value;
		/// otherwise returns null.
		/// It is functionally equivalent to:
		/// <pre>
		/// return (nextToken() == JsonToken.VALUE_STRING) ? getText() : null;
		/// </pre>
		/// but may be faster for parser to process, and can therefore be used if caller
		/// expects to get a String value next from input.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonParseException"/>
		public virtual string nextTextValue()
		{
			return (nextToken() == com.fasterxml.jackson.core.JsonToken.VALUE_STRING) ? getText
				() : null;
		}

		/// <summary>
		/// Method that fetches next token (as if calling
		/// <see cref="nextToken()"/>
		/// ) and
		/// if it is
		/// <see cref="JsonToken.VALUE_NUMBER_INT"/>
		/// returns 32-bit int value;
		/// otherwise returns specified default value
		/// It is functionally equivalent to:
		/// <pre>
		/// return (nextToken() == JsonToken.VALUE_NUMBER_INT) ? getIntValue() : defaultValue;
		/// </pre>
		/// but may be faster for parser to process, and can therefore be used if caller
		/// expects to get a String value next from input.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonParseException"/>
		public virtual int nextIntValue(int defaultValue)
		{
			return (nextToken() == com.fasterxml.jackson.core.JsonToken.VALUE_NUMBER_INT) ? getIntValue
				() : defaultValue;
		}

		/// <summary>
		/// Method that fetches next token (as if calling
		/// <see cref="nextToken()"/>
		/// ) and
		/// if it is
		/// <see cref="JsonToken.VALUE_NUMBER_INT"/>
		/// returns 64-bit long value;
		/// otherwise returns specified default value
		/// It is functionally equivalent to:
		/// <pre>
		/// return (nextToken() == JsonToken.VALUE_NUMBER_INT) ? getLongValue() : defaultValue;
		/// </pre>
		/// but may be faster for parser to process, and can therefore be used if caller
		/// expects to get a String value next from input.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonParseException"/>
		public virtual long nextLongValue(long defaultValue)
		{
			return (nextToken() == com.fasterxml.jackson.core.JsonToken.VALUE_NUMBER_INT) ? getLongValue
				() : defaultValue;
		}

		/// <summary>
		/// Method that fetches next token (as if calling
		/// <see cref="nextToken()"/>
		/// ) and
		/// if it is
		/// <see cref="JsonToken.VALUE_TRUE"/>
		/// or
		/// <see cref="JsonToken.VALUE_FALSE"/>
		/// returns matching Boolean value; otherwise return null.
		/// It is functionally equivalent to:
		/// <pre>
		/// JsonToken t = nextToken();
		/// if (t == JsonToken.VALUE_TRUE) return Boolean.TRUE;
		/// if (t == JsonToken.VALUE_FALSE) return Boolean.FALSE;
		/// return null;
		/// </pre>
		/// but may be faster for parser to process, and can therefore be used if caller
		/// expects to get a String value next from input.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonParseException"/>
		public virtual bool nextBooleanValue()
		{
			com.fasterxml.jackson.core.JsonToken t = nextToken();
			if (t == com.fasterxml.jackson.core.JsonToken.VALUE_TRUE)
			{
				return true;
			}
			if (t == com.fasterxml.jackson.core.JsonToken.VALUE_FALSE)
			{
				return false;
			}
			return null;
		}

		/// <summary>
		/// Method that will skip all child tokens of an array or
		/// object token that the parser currently points to,
		/// iff stream points to
		/// <see cref="JsonToken.START_OBJECT"/>
		/// or
		/// <see cref="JsonToken.START_ARRAY"/>
		/// .
		/// If not, it will do nothing.
		/// After skipping, stream will point to <b>matching</b>
		/// <see cref="JsonToken.END_OBJECT"/>
		/// or
		/// <see cref="JsonToken.END_ARRAY"/>
		/// (possibly skipping nested pairs of START/END OBJECT/ARRAY tokens
		/// as well as value tokens).
		/// The idea is that after calling this method, application
		/// will call
		/// <see cref="nextToken()"/>
		/// to point to the next
		/// available token, if any.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonParseException"/>
		public abstract com.fasterxml.jackson.core.JsonParser skipChildren();

		/// <summary>
		/// Method that can be called to determine whether this parser
		/// is closed or not.
		/// </summary>
		/// <remarks>
		/// Method that can be called to determine whether this parser
		/// is closed or not. If it is closed, no new tokens can be
		/// retrieved by calling
		/// <see cref="nextToken()"/>
		/// (and the underlying
		/// stream may be closed). Closing may be due to an explicit
		/// call to
		/// <see cref="close()"/>
		/// or because parser has encountered
		/// end of input.
		/// </remarks>
		public abstract bool isClosed();

		/*
		/**********************************************************
		/* Public API, token accessors
		/**********************************************************
		*/
		/// <summary>
		/// Accessor to find which token parser currently points to, if any;
		/// null will be returned if none.
		/// </summary>
		/// <remarks>
		/// Accessor to find which token parser currently points to, if any;
		/// null will be returned if none.
		/// If return value is non-null, data associated with the token
		/// is available via other accessor methods.
		/// </remarks>
		/// <returns>
		/// Type of the token this parser currently points to,
		/// if any: null before any tokens have been read, and
		/// after end-of-input has been encountered, as well as
		/// if the current token has been explicitly cleared.
		/// </returns>
		public abstract com.fasterxml.jackson.core.JsonToken getCurrentToken();

		/// <summary>
		/// Method similar to
		/// <see cref="getCurrentToken()"/>
		/// but that returns an
		/// <code>int</code> instead of
		/// <see cref="JsonToken"/>
		/// (enum value).
		/// <p>
		/// Use of int directly is typically more efficient on switch statements,
		/// so this method may be useful when building low-overhead codecs.
		/// Note, however, that effect may not be big enough to matter: make sure
		/// to profile performance before deciding to use this method.
		/// </summary>
		/// <since>2.3</since>
		/// <returns>
		/// <code>int</code> matching one of constants from
		/// <see cref="JsonTokenId"/>
		/// .
		/// </returns>
		public abstract int getCurrentTokenId();

		/// <summary>
		/// Method for checking whether parser currently points to
		/// a token (and data for that token is available).
		/// </summary>
		/// <remarks>
		/// Method for checking whether parser currently points to
		/// a token (and data for that token is available).
		/// Equivalent to check for <code>parser.getCurrentToken() != null</code>.
		/// </remarks>
		/// <returns>
		/// True if the parser just returned a valid
		/// token via
		/// <see cref="nextToken()"/>
		/// ; false otherwise (parser
		/// was just constructed, encountered end-of-input
		/// and returned null from
		/// <see cref="nextToken()"/>
		/// , or the token
		/// has been consumed)
		/// </returns>
		public abstract bool hasCurrentToken();

		/// <summary>
		/// Method that is functionally equivalent to:
		/// <code>
		/// return getCurrentTokenId() == id
		/// </code>
		/// but may be more efficiently implemented.
		/// </summary>
		/// <remarks>
		/// Method that is functionally equivalent to:
		/// <code>
		/// return getCurrentTokenId() == id
		/// </code>
		/// but may be more efficiently implemented.
		/// <p>
		/// Note that no traversal or conversion is performed; so in some
		/// cases calling method like
		/// <see cref="isExpectedStartArrayToken()"/>
		/// is necessary instead.
		/// </remarks>
		/// <since>2.5</since>
		public abstract bool hasTokenId(int id);

		/// <summary>
		/// Method that is functionally equivalent to:
		/// <code>
		/// return getCurrentTokenId() == id
		/// </code>
		/// but may be more efficiently implemented.
		/// </summary>
		/// <remarks>
		/// Method that is functionally equivalent to:
		/// <code>
		/// return getCurrentTokenId() == id
		/// </code>
		/// but may be more efficiently implemented.
		/// <p>
		/// Note that no traversal or conversion is performed; so in some
		/// cases calling method like
		/// <see cref="isExpectedStartArrayToken()"/>
		/// is necessary instead.
		/// </remarks>
		/// <since>2.6</since>
		public abstract bool hasToken(com.fasterxml.jackson.core.JsonToken t);

		/// <summary>
		/// Method that can be called to get the name associated with
		/// the current token: for
		/// <see cref="JsonToken.FIELD_NAME"/>
		/// s it will
		/// be the same as what
		/// <see cref="getText()"/>
		/// returns;
		/// for field values it will be preceding field name;
		/// and for others (array values, root-level values) null.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		public abstract string getCurrentName();

		/// <summary>
		/// Method that can be used to access current parsing context reader
		/// is in.
		/// </summary>
		/// <remarks>
		/// Method that can be used to access current parsing context reader
		/// is in. There are 3 different types: root, array and object contexts,
		/// with slightly different available information. Contexts are
		/// hierarchically nested, and can be used for example for figuring
		/// out part of the input document that correspond to specific
		/// array or object (for highlighting purposes, or error reporting).
		/// Contexts can also be used for simple xpath-like matching of
		/// input, if so desired.
		/// </remarks>
		public abstract com.fasterxml.jackson.core.JsonStreamContext getParsingContext();

		/// <summary>
		/// Method that return the <b>starting</b> location of the current
		/// token; that is, position of the first character from input
		/// that starts the current token.
		/// </summary>
		public abstract com.fasterxml.jackson.core.JsonLocation getTokenLocation();

		/// <summary>
		/// Method that returns location of the last processed character;
		/// usually for error reporting purposes.
		/// </summary>
		public abstract com.fasterxml.jackson.core.JsonLocation getCurrentLocation();

		/// <summary>
		/// Specialized accessor that can be used to verify that the current
		/// token indicates start array (usually meaning that current token
		/// is
		/// <see cref="JsonToken.START_ARRAY"/>
		/// ) when start array is expected.
		/// For some specialized parsers this can return true for other cases
		/// as well; this is usually done to emulate arrays in cases underlying
		/// format is ambiguous (XML, for example, has no format-level difference
		/// between Objects and Arrays; it just has elements).
		/// <p>
		/// Default implementation is equivalent to:
		/// <pre>
		/// getCurrentToken() == JsonToken.START_ARRAY
		/// </pre>
		/// but may be overridden by custom parser implementations.
		/// </summary>
		/// <returns>
		/// True if the current token can be considered as a
		/// start-array marker (such
		/// <see cref="JsonToken.START_ARRAY"/>
		/// );
		/// false if not.
		/// </returns>
		public virtual bool isExpectedStartArrayToken()
		{
			return getCurrentToken() == com.fasterxml.jackson.core.JsonToken.START_ARRAY;
		}

		/// <summary>
		/// Similar to
		/// <see cref="isExpectedStartArrayToken()"/>
		/// , but checks whether stream
		/// currently points to
		/// <see cref="JsonToken.START_OBJECT"/>
		/// .
		/// </summary>
		/// <since>2.5</since>
		public virtual bool isExpectedStartObjectToken()
		{
			return getCurrentToken() == com.fasterxml.jackson.core.JsonToken.START_OBJECT;
		}

		/*
		/**********************************************************
		/* Public API, token state overrides
		/**********************************************************
		*/
		/// <summary>
		/// Method called to "consume" the current token by effectively
		/// removing it so that
		/// <see cref="hasCurrentToken()"/>
		/// returns false, and
		/// <see cref="getCurrentToken()"/>
		/// null).
		/// Cleared token value can still be accessed by calling
		/// <see cref="getLastClearedToken()"/>
		/// (if absolutely needed), but
		/// usually isn't.
		/// <p>
		/// Method was added to be used by the optional data binder, since
		/// it has to be able to consume last token used for binding (so that
		/// it will not be used again).
		/// </summary>
		public abstract void clearCurrentToken();

		/// <summary>
		/// Method that can be called to get the last token that was
		/// cleared using
		/// <see cref="clearCurrentToken()"/>
		/// . This is not necessarily
		/// the latest token read.
		/// Will return null if no tokens have been cleared,
		/// or if parser has been closed.
		/// </summary>
		public abstract com.fasterxml.jackson.core.JsonToken getLastClearedToken();

		/// <summary>
		/// Method that can be used to change what is considered to be
		/// the current (field) name.
		/// </summary>
		/// <remarks>
		/// Method that can be used to change what is considered to be
		/// the current (field) name.
		/// May be needed to support non-JSON data formats or unusual binding
		/// conventions; not needed for typical processing.
		/// <p>
		/// Note that use of this method should only be done as sort of last
		/// resort, as it is a work-around for regular operation.
		/// </remarks>
		/// <param name="name">Name to use as the current name; may be null.</param>
		public abstract void overrideCurrentName(string name);

		/*
		/**********************************************************
		/* Public API, access to token information, text
		/**********************************************************
		*/
		/// <summary>
		/// Method for accessing textual representation of the current token;
		/// if no current token (before first call to
		/// <see cref="nextToken()"/>
		/// , or
		/// after encountering end-of-input), returns null.
		/// Method can be called for any token type.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		public abstract string getText();

		/// <summary>
		/// Method similar to
		/// <see cref="getText()"/>
		/// , but that will return
		/// underlying (unmodifiable) character array that contains
		/// textual value, instead of constructing a String object
		/// to contain this information.
		/// Note, however, that:
		/// <ul>
		/// <li>Textual contents are not guaranteed to start at
		/// index 0 (rather, call
		/// <see cref="getTextOffset()"/>
		/// ) to
		/// know the actual offset
		/// </li>
		/// <li>Length of textual contents may be less than the
		/// length of returned buffer: call
		/// <see cref="getTextLength()"/>
		/// for actual length of returned content.
		/// </li>
		/// </ul>
		/// <p>
		/// Note that caller <b>MUST NOT</b> modify the returned
		/// character array in any way -- doing so may corrupt
		/// current parser state and render parser instance useless.
		/// <p>
		/// The only reason to call this method (over
		/// <see cref="getText()"/>
		/// )
		/// is to avoid construction of a String object (which
		/// will make a copy of contents).
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		public abstract char[] getTextCharacters();

		/// <summary>
		/// Accessor used with
		/// <see cref="getTextCharacters()"/>
		/// , to know length
		/// of String stored in returned buffer.
		/// </summary>
		/// <returns>
		/// Number of characters within buffer returned
		/// by
		/// <see cref="getTextCharacters()"/>
		/// that are part of
		/// textual content of the current token.
		/// </returns>
		/// <exception cref="System.IO.IOException"/>
		public abstract int getTextLength();

		/// <summary>
		/// Accessor used with
		/// <see cref="getTextCharacters()"/>
		/// , to know offset
		/// of the first text content character within buffer.
		/// </summary>
		/// <returns>
		/// Offset of the first character within buffer returned
		/// by
		/// <see cref="getTextCharacters()"/>
		/// that is part of
		/// textual content of the current token.
		/// </returns>
		/// <exception cref="System.IO.IOException"/>
		public abstract int getTextOffset();

		/// <summary>
		/// Method that can be used to determine whether calling of
		/// <see cref="getTextCharacters()"/>
		/// would be the most efficient
		/// way to access textual content for the event parser currently
		/// points to.
		/// <p>
		/// Default implementation simply returns false since only actual
		/// implementation class has knowledge of its internal buffering
		/// state.
		/// Implementations are strongly encouraged to properly override
		/// this method, to allow efficient copying of content by other
		/// code.
		/// </summary>
		/// <returns>
		/// True if parser currently has character array that can
		/// be efficiently returned via
		/// <see cref="getTextCharacters()"/>
		/// ; false
		/// means that it may or may not exist
		/// </returns>
		public abstract bool hasTextCharacters();

		/*
		/**********************************************************
		/* Public API, access to token information, numeric
		/**********************************************************
		*/
		/// <summary>
		/// Generic number value accessor method that will work for
		/// all kinds of numeric values.
		/// </summary>
		/// <remarks>
		/// Generic number value accessor method that will work for
		/// all kinds of numeric values. It will return the optimal
		/// (simplest/smallest possible) wrapper object that can
		/// express the numeric value just parsed.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public abstract java.lang.Number getNumberValue();

		/// <summary>
		/// If current token is of type
		/// <see cref="JsonToken.VALUE_NUMBER_INT"/>
		/// or
		/// <see cref="JsonToken.VALUE_NUMBER_FLOAT"/>
		/// , returns
		/// one of
		/// <see cref="NumberType"/>
		/// constants; otherwise returns null.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		public abstract com.fasterxml.jackson.core.JsonParser.NumberType getNumberType();

		/// <summary>
		/// Numeric accessor that can be called when the current
		/// token is of type
		/// <see cref="JsonToken.VALUE_NUMBER_INT"/>
		/// and
		/// it can be expressed as a value of Java byte primitive type.
		/// It can also be called for
		/// <see cref="JsonToken.VALUE_NUMBER_FLOAT"/>
		/// ;
		/// if so, it is equivalent to calling
		/// <see cref="getDoubleValue()"/>
		/// and then casting; except for possible overflow/underflow
		/// exception.
		/// <p>
		/// Note: if the resulting integer value falls outside range of
		/// Java byte, a
		/// <see cref="JsonParseException"/>
		/// will be thrown to indicate numeric overflow/underflow.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		public virtual byte getByteValue()
		{
			int value = getIntValue();
			// So far so good: but does it fit?
			// [JACKSON-804]: Let's actually allow range of [-128, 255], as those are uniquely mapped
			//  (instead of just signed range of [-128, 127])
			if (value < MIN_BYTE_I || value > MAX_BYTE_I)
			{
				throw _constructError("Numeric value (" + getText() + ") out of range of Java byte"
					);
			}
			return unchecked((byte)value);
		}

		/// <summary>
		/// Numeric accessor that can be called when the current
		/// token is of type
		/// <see cref="JsonToken.VALUE_NUMBER_INT"/>
		/// and
		/// it can be expressed as a value of Java short primitive type.
		/// It can also be called for
		/// <see cref="JsonToken.VALUE_NUMBER_FLOAT"/>
		/// ;
		/// if so, it is equivalent to calling
		/// <see cref="getDoubleValue()"/>
		/// and then casting; except for possible overflow/underflow
		/// exception.
		/// <p>
		/// Note: if the resulting integer value falls outside range of
		/// Java short, a
		/// <see cref="JsonParseException"/>
		/// will be thrown to indicate numeric overflow/underflow.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		public virtual short getShortValue()
		{
			int value = getIntValue();
			if (value < MIN_SHORT_I || value > MAX_SHORT_I)
			{
				throw _constructError("Numeric value (" + getText() + ") out of range of Java short"
					);
			}
			return (short)value;
		}

		/// <summary>
		/// Numeric accessor that can be called when the current
		/// token is of type
		/// <see cref="JsonToken.VALUE_NUMBER_INT"/>
		/// and
		/// it can be expressed as a value of Java int primitive type.
		/// It can also be called for
		/// <see cref="JsonToken.VALUE_NUMBER_FLOAT"/>
		/// ;
		/// if so, it is equivalent to calling
		/// <see cref="getDoubleValue()"/>
		/// and then casting; except for possible overflow/underflow
		/// exception.
		/// <p>
		/// Note: if the resulting integer value falls outside range of
		/// Java int, a
		/// <see cref="JsonParseException"/>
		/// may be thrown to indicate numeric overflow/underflow.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		public abstract int getIntValue();

		/// <summary>
		/// Numeric accessor that can be called when the current
		/// token is of type
		/// <see cref="JsonToken.VALUE_NUMBER_INT"/>
		/// and
		/// it can be expressed as a Java long primitive type.
		/// It can also be called for
		/// <see cref="JsonToken.VALUE_NUMBER_FLOAT"/>
		/// ;
		/// if so, it is equivalent to calling
		/// <see cref="getDoubleValue()"/>
		/// and then casting to int; except for possible overflow/underflow
		/// exception.
		/// <p>
		/// Note: if the token is an integer, but its value falls
		/// outside of range of Java long, a
		/// <see cref="JsonParseException"/>
		/// may be thrown to indicate numeric overflow/underflow.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		public abstract long getLongValue();

		/// <summary>
		/// Numeric accessor that can be called when the current
		/// token is of type
		/// <see cref="JsonToken.VALUE_NUMBER_INT"/>
		/// and
		/// it can not be used as a Java long primitive type due to its
		/// magnitude.
		/// It can also be called for
		/// <see cref="JsonToken.VALUE_NUMBER_FLOAT"/>
		/// ;
		/// if so, it is equivalent to calling
		/// <see cref="getDecimalValue()"/>
		/// and then constructing a
		/// <see cref="System.Numerics.BigInteger"/>
		/// from that value.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		public abstract System.Numerics.BigInteger getBigIntegerValue();

		/// <summary>
		/// Numeric accessor that can be called when the current
		/// token is of type
		/// <see cref="JsonToken.VALUE_NUMBER_FLOAT"/>
		/// and
		/// it can be expressed as a Java float primitive type.
		/// It can also be called for
		/// <see cref="JsonToken.VALUE_NUMBER_INT"/>
		/// ;
		/// if so, it is equivalent to calling
		/// <see cref="getLongValue()"/>
		/// and then casting; except for possible overflow/underflow
		/// exception.
		/// <p>
		/// Note: if the value falls
		/// outside of range of Java float, a
		/// <see cref="JsonParseException"/>
		/// will be thrown to indicate numeric overflow/underflow.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		public abstract float getFloatValue();

		/// <summary>
		/// Numeric accessor that can be called when the current
		/// token is of type
		/// <see cref="JsonToken.VALUE_NUMBER_FLOAT"/>
		/// and
		/// it can be expressed as a Java double primitive type.
		/// It can also be called for
		/// <see cref="JsonToken.VALUE_NUMBER_INT"/>
		/// ;
		/// if so, it is equivalent to calling
		/// <see cref="getLongValue()"/>
		/// and then casting; except for possible overflow/underflow
		/// exception.
		/// <p>
		/// Note: if the value falls
		/// outside of range of Java double, a
		/// <see cref="JsonParseException"/>
		/// will be thrown to indicate numeric overflow/underflow.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		public abstract double getDoubleValue();

		/// <summary>
		/// Numeric accessor that can be called when the current
		/// token is of type
		/// <see cref="JsonToken.VALUE_NUMBER_FLOAT"/>
		/// or
		/// <see cref="JsonToken.VALUE_NUMBER_INT"/>
		/// . No under/overflow exceptions
		/// are ever thrown.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		public abstract java.math.BigDecimal getDecimalValue();

		/*
		/**********************************************************
		/* Public API, access to token information, other
		/**********************************************************
		*/
		/// <summary>
		/// Convenience accessor that can be called when the current
		/// token is
		/// <see cref="JsonToken.VALUE_TRUE"/>
		/// or
		/// <see cref="JsonToken.VALUE_FALSE"/>
		/// .
		/// <p>
		/// Note: if the token is not of above-mentioned boolean types,
		/// an integer, but its value falls
		/// outside of range of Java long, a
		/// <see cref="JsonParseException"/>
		/// may be thrown to indicate numeric overflow/underflow.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		public virtual bool getBooleanValue()
		{
			com.fasterxml.jackson.core.JsonToken t = getCurrentToken();
			if (t == com.fasterxml.jackson.core.JsonToken.VALUE_TRUE)
			{
				return true;
			}
			if (t == com.fasterxml.jackson.core.JsonToken.VALUE_FALSE)
			{
				return false;
			}
			throw new com.fasterxml.jackson.core.JsonParseException("Current token (" + t + ") not of boolean type"
				, getCurrentLocation());
		}

		/// <summary>
		/// Accessor that can be called if (and only if) the current token
		/// is
		/// <see cref="JsonToken.VALUE_EMBEDDED_OBJECT"/>
		/// . For other token types,
		/// null is returned.
		/// <p>
		/// Note: only some specialized parser implementations support
		/// embedding of objects (usually ones that are facades on top
		/// of non-streaming sources, such as object trees).
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		public abstract object getEmbeddedObject();

		/*
		/**********************************************************
		/* Public API, access to token information, binary
		/**********************************************************
		*/
		/// <summary>
		/// Method that can be used to read (and consume -- results
		/// may not be accessible using other methods after the call)
		/// base64-encoded binary data
		/// included in the current textual JSON value.
		/// </summary>
		/// <remarks>
		/// Method that can be used to read (and consume -- results
		/// may not be accessible using other methods after the call)
		/// base64-encoded binary data
		/// included in the current textual JSON value.
		/// It works similar to getting String value via
		/// <see cref="getText()"/>
		/// and decoding result (except for decoding part),
		/// but should be significantly more performant.
		/// <p>
		/// Note that non-decoded textual contents of the current token
		/// are not guaranteed to be accessible after this method
		/// is called. Current implementation, for example, clears up
		/// textual content during decoding.
		/// Decoded binary content, however, will be retained until
		/// parser is advanced to the next event.
		/// </remarks>
		/// <param name="bv">
		/// Expected variant of base64 encoded
		/// content (see
		/// <see cref="Base64Variants"/>
		/// for definitions
		/// of "standard" variants).
		/// </param>
		/// <returns>Decoded binary data</returns>
		/// <exception cref="System.IO.IOException"/>
		public abstract byte[] getBinaryValue(com.fasterxml.jackson.core.Base64Variant bv
			);

		/// <summary>
		/// Convenience alternative to
		/// <see cref="getBinaryValue(Base64Variant)"/>
		/// that defaults to using
		/// <see cref="Base64Variants.getDefaultVariant()"/>
		/// as the default encoding.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		public virtual byte[] getBinaryValue()
		{
			return getBinaryValue(com.fasterxml.jackson.core.Base64Variants.getDefaultVariant
				());
		}

		/// <summary>
		/// Method that can be used as an alternative to
		/// <see cref="getBigIntegerValue()"/>
		/// ,
		/// especially when value can be large. The main difference (beyond method
		/// of returning content using
		/// <see cref="Sharpen.OutputStream"/>
		/// instead of as byte array)
		/// is that content will NOT remain accessible after method returns: any content
		/// processed will be consumed and is not buffered in any way. If caller needs
		/// buffering, it has to implement it.
		/// </summary>
		/// <param name="out">Output stream to use for passing decoded binary data</param>
		/// <returns>
		/// Number of bytes that were decoded and written via
		/// <see cref="Sharpen.OutputStream"/>
		/// </returns>
		/// <since>2.1</since>
		/// <exception cref="System.IO.IOException"/>
		public virtual int readBinaryValue(Sharpen.OutputStream @out)
		{
			return readBinaryValue(com.fasterxml.jackson.core.Base64Variants.getDefaultVariant
				(), @out);
		}

		/// <summary>
		/// Similar to
		/// <see cref="readBinaryValue(Sharpen.OutputStream)"/>
		/// but allows explicitly
		/// specifying base64 variant to use.
		/// </summary>
		/// <param name="bv">base64 variant to use</param>
		/// <param name="out">Output stream to use for passing decoded binary data</param>
		/// <returns>
		/// Number of bytes that were decoded and written via
		/// <see cref="Sharpen.OutputStream"/>
		/// </returns>
		/// <since>2.1</since>
		/// <exception cref="System.IO.IOException"/>
		public virtual int readBinaryValue(com.fasterxml.jackson.core.Base64Variant bv, Sharpen.OutputStream
			 @out)
		{
			_reportUnsupportedOperation();
			return 0;
		}

		// never gets here
		/*
		/**********************************************************
		/* Public API, access to token information, coercion/conversion
		/**********************************************************
		*/
		/// <summary>
		/// Method that will try to convert value of current token to a
		/// <b>int</b>.
		/// </summary>
		/// <remarks>
		/// Method that will try to convert value of current token to a
		/// <b>int</b>.
		/// Numbers are coerced using default Java rules; booleans convert to 0 (false)
		/// and 1 (true), and Strings are parsed using default Java language integer
		/// parsing rules.
		/// <p>
		/// If representation can not be converted to an int (including structured type
		/// markers like start/end Object/Array)
		/// default value of <b>0</b> will be returned; no exceptions are thrown.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public virtual int getValueAsInt()
		{
			return getValueAsInt(0);
		}

		/// <summary>
		/// Method that will try to convert value of current token to a
		/// <b>int</b>.
		/// </summary>
		/// <remarks>
		/// Method that will try to convert value of current token to a
		/// <b>int</b>.
		/// Numbers are coerced using default Java rules; booleans convert to 0 (false)
		/// and 1 (true), and Strings are parsed using default Java language integer
		/// parsing rules.
		/// <p>
		/// If representation can not be converted to an int (including structured type
		/// markers like start/end Object/Array)
		/// specified <b>def</b> will be returned; no exceptions are thrown.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public virtual int getValueAsInt(int def)
		{
			return def;
		}

		/// <summary>
		/// Method that will try to convert value of current token to a
		/// <b>long</b>.
		/// </summary>
		/// <remarks>
		/// Method that will try to convert value of current token to a
		/// <b>long</b>.
		/// Numbers are coerced using default Java rules; booleans convert to 0 (false)
		/// and 1 (true), and Strings are parsed using default Java language integer
		/// parsing rules.
		/// <p>
		/// If representation can not be converted to an int (including structured type
		/// markers like start/end Object/Array)
		/// default value of <b>0</b> will be returned; no exceptions are thrown.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public virtual long getValueAsLong()
		{
			return getValueAsLong(0);
		}

		/// <summary>
		/// Method that will try to convert value of current token to a
		/// <b>long</b>.
		/// </summary>
		/// <remarks>
		/// Method that will try to convert value of current token to a
		/// <b>long</b>.
		/// Numbers are coerced using default Java rules; booleans convert to 0 (false)
		/// and 1 (true), and Strings are parsed using default Java language integer
		/// parsing rules.
		/// <p>
		/// If representation can not be converted to an int (including structured type
		/// markers like start/end Object/Array)
		/// specified <b>def</b> will be returned; no exceptions are thrown.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public virtual long getValueAsLong(long def)
		{
			return def;
		}

		/// <summary>
		/// Method that will try to convert value of current token to a Java
		/// <b>double</b>.
		/// </summary>
		/// <remarks>
		/// Method that will try to convert value of current token to a Java
		/// <b>double</b>.
		/// Numbers are coerced using default Java rules; booleans convert to 0.0 (false)
		/// and 1.0 (true), and Strings are parsed using default Java language integer
		/// parsing rules.
		/// <p>
		/// If representation can not be converted to an int (including structured types
		/// like Objects and Arrays),
		/// default value of <b>0.0</b> will be returned; no exceptions are thrown.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public virtual double getValueAsDouble()
		{
			return getValueAsDouble(0.0);
		}

		/// <summary>
		/// Method that will try to convert value of current token to a
		/// Java <b>double</b>.
		/// </summary>
		/// <remarks>
		/// Method that will try to convert value of current token to a
		/// Java <b>double</b>.
		/// Numbers are coerced using default Java rules; booleans convert to 0.0 (false)
		/// and 1.0 (true), and Strings are parsed using default Java language integer
		/// parsing rules.
		/// <p>
		/// If representation can not be converted to an int (including structured types
		/// like Objects and Arrays),
		/// specified <b>def</b> will be returned; no exceptions are thrown.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public virtual double getValueAsDouble(double def)
		{
			return def;
		}

		/// <summary>
		/// Method that will try to convert value of current token to a
		/// <b>boolean</b>.
		/// </summary>
		/// <remarks>
		/// Method that will try to convert value of current token to a
		/// <b>boolean</b>.
		/// JSON booleans map naturally; integer numbers other than 0 map to true, and
		/// 0 maps to false
		/// and Strings 'true' and 'false' map to corresponding values.
		/// <p>
		/// If representation can not be converted to a boolean value (including structured types
		/// like Objects and Arrays),
		/// default value of <b>false</b> will be returned; no exceptions are thrown.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public virtual bool getValueAsBoolean()
		{
			return getValueAsBoolean(false);
		}

		/// <summary>
		/// Method that will try to convert value of current token to a
		/// <b>boolean</b>.
		/// </summary>
		/// <remarks>
		/// Method that will try to convert value of current token to a
		/// <b>boolean</b>.
		/// JSON booleans map naturally; integer numbers other than 0 map to true, and
		/// 0 maps to false
		/// and Strings 'true' and 'false' map to corresponding values.
		/// <p>
		/// If representation can not be converted to a boolean value (including structured types
		/// like Objects and Arrays),
		/// specified <b>def</b> will be returned; no exceptions are thrown.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public virtual bool getValueAsBoolean(bool def)
		{
			return def;
		}

		/// <summary>
		/// Method that will try to convert value of current token to a
		/// <see cref="string"/>
		/// .
		/// JSON Strings map naturally; scalar values get converted to
		/// their textual representation.
		/// If representation can not be converted to a String value (including structured types
		/// like Objects and Arrays and null token), default value of
		/// <b>null</b> will be returned; no exceptions are thrown.
		/// </summary>
		/// <since>2.1</since>
		/// <exception cref="System.IO.IOException"/>
		public virtual string getValueAsString()
		{
			return getValueAsString(null);
		}

		/// <summary>
		/// Method that will try to convert value of current token to a
		/// <see cref="string"/>
		/// .
		/// JSON Strings map naturally; scalar values get converted to
		/// their textual representation.
		/// If representation can not be converted to a String value (including structured types
		/// like Objects and Arrays and null token), specified default value
		/// will be returned; no exceptions are thrown.
		/// </summary>
		/// <since>2.1</since>
		/// <exception cref="System.IO.IOException"/>
		public abstract string getValueAsString(string def);

		/*
		/**********************************************************
		/* Public API, Native Ids (type, object)
		/**********************************************************
		*/
		/// <summary>
		/// Introspection method that may be called to see if the underlying
		/// data format supports some kind of Object Ids natively (many do not;
		/// for example, JSON doesn't).
		/// </summary>
		/// <remarks>
		/// Introspection method that may be called to see if the underlying
		/// data format supports some kind of Object Ids natively (many do not;
		/// for example, JSON doesn't).
		/// <p>
		/// Default implementation returns true; overridden by data formats
		/// that do support native Object Ids. Caller is expected to either
		/// use a non-native notation (explicit property or such), or fail,
		/// in case it can not use native object ids.
		/// </remarks>
		/// <since>2.3</since>
		public virtual bool canReadObjectId()
		{
			return false;
		}

		/// <summary>
		/// Introspection method that may be called to see if the underlying
		/// data format supports some kind of Type Ids natively (many do not;
		/// for example, JSON doesn't).
		/// </summary>
		/// <remarks>
		/// Introspection method that may be called to see if the underlying
		/// data format supports some kind of Type Ids natively (many do not;
		/// for example, JSON doesn't).
		/// <p>
		/// Default implementation returns true; overridden by data formats
		/// that do support native Type Ids. Caller is expected to either
		/// use a non-native notation (explicit property or such), or fail,
		/// in case it can not use native type ids.
		/// </remarks>
		/// <since>2.3</since>
		public virtual bool canReadTypeId()
		{
			return false;
		}

		/// <summary>
		/// Method that can be called to check whether current token
		/// (one that was just read) has an associated Object id, and if
		/// so, return it.
		/// </summary>
		/// <remarks>
		/// Method that can be called to check whether current token
		/// (one that was just read) has an associated Object id, and if
		/// so, return it.
		/// Note that while typically caller should check with
		/// <see cref="canReadObjectId()"/>
		/// first, it is not illegal to call this method even if that method returns
		/// true; but if so, it will return null. This may be used to simplify calling
		/// code.
		/// <p>
		/// Default implementation will simply return null.
		/// </remarks>
		/// <since>2.3</since>
		/// <exception cref="System.IO.IOException"/>
		public virtual object getObjectId()
		{
			return null;
		}

		/// <summary>
		/// Method that can be called to check whether current token
		/// (one that was just read) has an associated type id, and if
		/// so, return it.
		/// </summary>
		/// <remarks>
		/// Method that can be called to check whether current token
		/// (one that was just read) has an associated type id, and if
		/// so, return it.
		/// Note that while typically caller should check with
		/// <see cref="canReadTypeId()"/>
		/// first, it is not illegal to call this method even if that method returns
		/// true; but if so, it will return null. This may be used to simplify calling
		/// code.
		/// <p>
		/// Default implementation will simply return null.
		/// </remarks>
		/// <since>2.3</since>
		/// <exception cref="System.IO.IOException"/>
		public virtual object getTypeId()
		{
			return null;
		}

		/*
		/**********************************************************
		/* Public API, optional data binding functionality
		/**********************************************************
		*/
		/// <summary>
		/// Method to deserialize JSON content into a non-container
		/// type (it can be an array type, however): typically a bean, array
		/// or a wrapper type (like
		/// <see cref="bool"/>
		/// ).
		/// <b>Note</b>: method can only be called if the parser has
		/// an object codec assigned; this is true for parsers constructed
		/// by <code>MappingJsonFactory</code> (from "jackson-databind" jar)
		/// but not for
		/// <see cref="JsonFactory"/>
		/// (unless its <code>setCodec</code>
		/// method has been explicitly called).
		/// <p>
		/// This method may advance the event stream, for structured types
		/// the current token will be the closing end marker (END_ARRAY,
		/// END_OBJECT) of the bound structure. For non-structured Json types
		/// (and for
		/// <see cref="JsonToken.VALUE_EMBEDDED_OBJECT"/>
		/// )
		/// stream is not advanced.
		/// <p>
		/// Note: this method should NOT be used if the result type is a
		/// container (
		/// <see cref="System.Collections.ICollection{E}"/>
		/// or
		/// <see cref="System.Collections.IDictionary{K, V}"/>
		/// .
		/// The reason is that due to type erasure, key and value types
		/// can not be introspected when using this method.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		public virtual T readValueAs<T>()
		{
			System.Type valueType = typeof(T);
			return _codec().readValue(this, valueType);
		}

		/// <summary>
		/// Method to deserialize JSON content into a Java type, reference
		/// to which is passed as argument.
		/// </summary>
		/// <remarks>
		/// Method to deserialize JSON content into a Java type, reference
		/// to which is passed as argument. Type is passed using so-called
		/// "super type token"
		/// and specifically needs to be used if the root type is a
		/// parameterized (generic) container type.
		/// <b>Note</b>: method can only be called if the parser has
		/// an object codec assigned; this is true for parsers constructed
		/// by <code>MappingJsonFactory</code> (defined in 'jackson-databind' bundle)
		/// but not for
		/// <see cref="JsonFactory"/>
		/// (unless its <code>setCodec</code>
		/// method has been explicitly called).
		/// <p>
		/// This method may advance the event stream, for structured types
		/// the current token will be the closing end marker (END_ARRAY,
		/// END_OBJECT) of the bound structure. For non-structured Json types
		/// (and for
		/// <see cref="JsonToken.VALUE_EMBEDDED_OBJECT"/>
		/// )
		/// stream is not advanced.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public virtual T readValueAs<T, _T1>(com.fasterxml.jackson.core.type.TypeReference
			<_T1> valueTypeRef)
		{
			return (T)_codec().readValue(this, valueTypeRef);
		}

		/// <summary>
		/// Method for reading sequence of Objects from parser stream,
		/// all with same specified value type.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		public virtual System.Collections.Generic.IEnumerator<T> readValuesAs<T>()
		{
			System.Type valueType = typeof(T);
			return _codec().readValues(this, valueType);
		}

		/// <summary>
		/// Method for reading sequence of Objects from parser stream,
		/// all with same specified value type.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		public virtual System.Collections.Generic.IEnumerator<T> readValuesAs<T, _T1>(com.fasterxml.jackson.core.type.TypeReference
			<_T1> valueTypeRef)
		{
			return _codec().readValues(this, valueTypeRef);
		}

		/// <summary>
		/// Method to deserialize JSON content into equivalent "tree model",
		/// represented by root
		/// <see cref="TreeNode"/>
		/// of resulting model.
		/// For JSON Arrays it will an array node (with child nodes),
		/// for objects object node (with child nodes), and for other types
		/// matching leaf node type. Empty or whitespace documents are null.
		/// </summary>
		/// <returns>root of the document, or null if empty or whitespace.</returns>
		/// <exception cref="System.IO.IOException"/>
		public virtual T readValueAsTree<T>()
			where T : com.fasterxml.jackson.core.TreeNode
		{
			return (T)((com.fasterxml.jackson.core.TreeNode)_codec().readTree(this));
		}

		protected internal virtual com.fasterxml.jackson.core.ObjectCodec _codec()
		{
			com.fasterxml.jackson.core.ObjectCodec c = getCodec();
			if (c == null)
			{
				throw new System.InvalidOperationException("No ObjectCodec defined for parser, needed for deserialization"
					);
			}
			return c;
		}

		/*
		/**********************************************************
		/* Internal methods
		/**********************************************************
		*/
		/// <summary>
		/// Helper method for constructing
		/// <see cref="JsonParseException"/>
		/// s
		/// based on current state of the parser
		/// </summary>
		protected internal virtual com.fasterxml.jackson.core.JsonParseException _constructError
			(string msg)
		{
			return new com.fasterxml.jackson.core.JsonParseException(msg, getCurrentLocation(
				));
		}

		/// <summary>
		/// Helper method to call for operations that are not supported by
		/// parser implementation.
		/// </summary>
		/// <since>2.1</since>
		protected internal virtual void _reportUnsupportedOperation()
		{
			throw new System.NotSupportedException("Operation not supported by parser of type "
				 + GetType().FullName);
		}
	}
}
