/* Jackson JSON-processor.
*
* Copyright (c) 2007- Tatu Saloranta, tatu.saloranta@iki.fi
*/
using Sharpen;

namespace com.fasterxml.jackson.core
{
	/// <summary>
	/// The main factory class of Jackson package, used to configure and
	/// construct reader (aka parser,
	/// <see cref="JsonParser"/>
	/// )
	/// and writer (aka generator,
	/// <see cref="JsonGenerator"/>
	/// )
	/// instances.
	/// <p>
	/// Factory instances are thread-safe and reusable after configuration
	/// (if any). Typically applications and services use only a single
	/// globally shared factory instance, unless they need differently
	/// configured factories. Factory reuse is important if efficiency matters;
	/// most recycling of expensive construct is done on per-factory basis.
	/// <p>
	/// Creation of a factory instance is a light-weight operation,
	/// and since there is no need for pluggable alternative implementations
	/// (as there is no "standard" JSON processor API to implement),
	/// the default constructor is used for constructing factory
	/// instances.
	/// </summary>
	/// <author>Tatu Saloranta</author>
	[System.Serializable]
	public class JsonFactory : com.fasterxml.jackson.core.Versioned
	{
		private const long serialVersionUID = 1;

		/// <summary>
		/// Enumeration that defines all on/off features that can only be
		/// changed for
		/// <see cref="JsonFactory"/>
		/// .
		/// </summary>
		[System.Serializable]
		public sealed class Feature
		{
			/// <summary>
			/// Feature that determines whether JSON object field names are
			/// to be canonicalized using
			/// <see cref="string.Intern()"/>
			/// or not:
			/// if enabled, all field names will be intern()ed (and caller
			/// can count on this being true for all such names); if disabled,
			/// no intern()ing is done. There may still be basic
			/// canonicalization (that is, same String will be used to represent
			/// all identical object property names for a single document).
			/// <p>
			/// Note: this setting only has effect if
			/// <see cref="CANONICALIZE_FIELD_NAMES"/>
			/// is true -- otherwise no
			/// canonicalization of any sort is done.
			/// <p>
			/// This setting is enabled by default.
			/// </summary>
			public static readonly com.fasterxml.jackson.core.JsonFactory.Feature INTERN_FIELD_NAMES
				 = new com.fasterxml.jackson.core.JsonFactory.Feature(true);

			/// <summary>
			/// Feature that determines whether JSON object field names are
			/// to be canonicalized (details of how canonicalization is done
			/// then further specified by
			/// <see cref="INTERN_FIELD_NAMES"/>
			/// ).
			/// <p>
			/// This setting is enabled by default.
			/// </summary>
			public static readonly com.fasterxml.jackson.core.JsonFactory.Feature CANONICALIZE_FIELD_NAMES
				 = new com.fasterxml.jackson.core.JsonFactory.Feature(true);

			/// <summary>
			/// Feature that determines what happens if we encounter a case in symbol
			/// handling where number of hash collisions exceeds a safety threshold
			/// -- which almost certainly means a denial-of-service attack via generated
			/// duplicate hash codes.
			/// </summary>
			/// <remarks>
			/// Feature that determines what happens if we encounter a case in symbol
			/// handling where number of hash collisions exceeds a safety threshold
			/// -- which almost certainly means a denial-of-service attack via generated
			/// duplicate hash codes.
			/// If feature is enabled, an
			/// <see cref="System.InvalidOperationException"/>
			/// is
			/// thrown to indicate the suspected denial-of-service attack; if disabled, processing continues but
			/// canonicalization (and thereby <code>intern()</code>ing) is disabled) as protective
			/// measure.
			/// <p>
			/// This setting is enabled by default.
			/// </remarks>
			/// <since>2.4</since>
			public static readonly com.fasterxml.jackson.core.JsonFactory.Feature FAIL_ON_SYMBOL_HASH_OVERFLOW
				 = new com.fasterxml.jackson.core.JsonFactory.Feature(true);

			/// <summary>
			/// Feature that determines whether we will use
			/// <see cref="com.fasterxml.jackson.core.util.BufferRecycler"/>
			/// with
			/// <see cref="java.lang.ThreadLocal{T}"/>
			/// and
			/// <see cref="Sharpen.SoftReference{T}"/>
			/// , for efficient reuse of
			/// underlying input/output buffers.
			/// This usually makes sense on normal J2SE/J2EE server-side processing;
			/// but may not make sense on platforms where
			/// <see cref="Sharpen.SoftReference{T}"/>
			/// handling
			/// is broken (like Android), or if there are retention issues due to
			/// <see cref="java.lang.ThreadLocal{T}"/>
			/// (see
			/// <a href="https://github.com/FasterXML/jackson-core/issues/189">Issue #189</a>
			/// for a possible case)
			/// </summary>
			/// <since>2.6</since>
			public static readonly com.fasterxml.jackson.core.JsonFactory.Feature USE_THREAD_LOCAL_FOR_BUFFER_RECYCLING
				 = new com.fasterxml.jackson.core.JsonFactory.Feature(true);

			/// <summary>Whether feature is enabled or disabled by default.</summary>
			private readonly bool _defaultState;

			// since 2.1 (for Android, mostly)
			// since 2.6.0
			/*
			/**********************************************************
			/* Helper types
			/**********************************************************
			*/
			// // // Symbol handling (interning etc)
			/// <summary>
			/// Method that calculates bit set (flags) of all features that
			/// are enabled by default.
			/// </summary>
			public static int collectDefaults()
			{
				int flags = 0;
				foreach (com.fasterxml.jackson.core.JsonFactory.Feature f in values())
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
				com.fasterxml.jackson.core.JsonFactory.Feature._defaultState = defaultState;
			}

			public bool enabledByDefault()
			{
				return com.fasterxml.jackson.core.JsonFactory.Feature._defaultState;
			}

			public bool enabledIn(int flags)
			{
				return (flags & getMask()) != 0;
			}

			public int getMask()
			{
				return (1 << ordinal());
			}
		}

		/// <summary>
		/// Name used to identify JSON format
		/// (and returned by
		/// <see cref="getFormatName()"/>
		/// </summary>
		public const string FORMAT_NAME_JSON = "JSON";

		/// <summary>Bitfield (set of flags) of all factory features that are enabled by default.
		/// 	</summary>
		protected internal static readonly int DEFAULT_FACTORY_FEATURE_FLAGS = com.fasterxml.jackson.core.JsonFactory.Feature
			.collectDefaults();

		/// <summary>
		/// Bitfield (set of flags) of all parser features that are enabled
		/// by default.
		/// </summary>
		protected internal static readonly int DEFAULT_PARSER_FEATURE_FLAGS = com.fasterxml.jackson.core.JsonParser.Feature
			.collectDefaults();

		/// <summary>
		/// Bitfield (set of flags) of all generator features that are enabled
		/// by default.
		/// </summary>
		protected internal static readonly int DEFAULT_GENERATOR_FEATURE_FLAGS = com.fasterxml.jackson.core.JsonGenerator.Feature
			.collectDefaults();

		private static readonly com.fasterxml.jackson.core.SerializableString DEFAULT_ROOT_VALUE_SEPARATOR
			 = com.fasterxml.jackson.core.util.DefaultPrettyPrinter.DEFAULT_ROOT_VALUE_SEPARATOR;

		/// <summary>
		/// This <code>ThreadLocal</code> contains a
		/// <see cref="Sharpen.SoftReference{T}"/>
		/// to a
		/// <see cref="com.fasterxml.jackson.core.util.BufferRecycler"/>
		/// used to provide a low-cost
		/// buffer recycling between reader and writer instances.
		/// </summary>
		protected internal static readonly java.lang.ThreadLocal<Sharpen.SoftReference<com.fasterxml.jackson.core.util.BufferRecycler
			>> _recyclerRef = new java.lang.ThreadLocal<Sharpen.SoftReference<com.fasterxml.jackson.core.util.BufferRecycler
			>>();

		/// <summary>Each factory comes equipped with a shared root symbol table.</summary>
		/// <remarks>
		/// Each factory comes equipped with a shared root symbol table.
		/// It should not be linked back to the original blueprint, to
		/// avoid contents from leaking between factories.
		/// </remarks>
		[System.NonSerialized]
		protected internal readonly com.fasterxml.jackson.core.sym.CharsToNameCanonicalizer
			 _rootCharSymbols = com.fasterxml.jackson.core.sym.CharsToNameCanonicalizer.createRoot
			();

		/// <summary>
		/// Alternative to the basic symbol table, some stream-based
		/// parsers use different name canonicalization method.
		/// </summary>
		/// <remarks>
		/// Alternative to the basic symbol table, some stream-based
		/// parsers use different name canonicalization method.
		/// <p>
		/// TODO: should clean up this; looks messy having 2 alternatives
		/// with not very clear differences.
		/// </remarks>
		/// <since>2.6.0</since>
		[System.NonSerialized]
		protected internal readonly com.fasterxml.jackson.core.sym.ByteQuadsCanonicalizer
			 _byteSymbolCanonicalizer = com.fasterxml.jackson.core.sym.ByteQuadsCanonicalizer
			.createRoot();

		/// <summary>Earlier byte-based symbol table; replaced with 2.6 with a new implementation.
		/// 	</summary>
		/// <remarks>
		/// Earlier byte-based symbol table; replaced with 2.6 with a new implementation.
		/// Left in for version 2.6.0: will be removed in 2.7 or later.
		/// </remarks>
		[System.NonSerialized]
		[System.ObsoleteAttribute(@"Since 2.6.0, only use _byteSymbolCanonicalizer")]
		protected internal readonly com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer
			 _rootByteSymbols = com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer.createRoot
			();

		/// <summary>
		/// Object that implements conversion functionality between
		/// Java objects and JSON content.
		/// </summary>
		/// <remarks>
		/// Object that implements conversion functionality between
		/// Java objects and JSON content. For base JsonFactory implementation
		/// usually not set by default, but can be explicitly set.
		/// Sub-classes (like @link org.codehaus.jackson.map.MappingJsonFactory}
		/// usually provide an implementation.
		/// </remarks>
		protected internal com.fasterxml.jackson.core.ObjectCodec _objectCodec;

		/// <summary>Currently enabled factory features.</summary>
		protected internal int _factoryFeatures = DEFAULT_FACTORY_FEATURE_FLAGS;

		/// <summary>Currently enabled parser features.</summary>
		protected internal int _parserFeatures = DEFAULT_PARSER_FEATURE_FLAGS;

		/// <summary>Currently enabled generator features.</summary>
		protected internal int _generatorFeatures = DEFAULT_GENERATOR_FEATURE_FLAGS;

		/// <summary>
		/// Definition of custom character escapes to use for generators created
		/// by this factory, if any.
		/// </summary>
		/// <remarks>
		/// Definition of custom character escapes to use for generators created
		/// by this factory, if any. If null, standard data format specific
		/// escapes are used.
		/// </remarks>
		protected internal com.fasterxml.jackson.core.io.CharacterEscapes _characterEscapes;

		/// <summary>
		/// Optional helper object that may decorate input sources, to do
		/// additional processing on input during parsing.
		/// </summary>
		protected internal com.fasterxml.jackson.core.io.InputDecorator _inputDecorator;

		/// <summary>
		/// Optional helper object that may decorate output object, to do
		/// additional processing on output during content generation.
		/// </summary>
		protected internal com.fasterxml.jackson.core.io.OutputDecorator _outputDecorator;

		/// <summary>
		/// Separator used between root-level values, if any; null indicates
		/// "do not add separator".
		/// </summary>
		/// <remarks>
		/// Separator used between root-level values, if any; null indicates
		/// "do not add separator".
		/// Default separator is a single space character.
		/// </remarks>
		/// <since>2.1</since>
		protected internal com.fasterxml.jackson.core.SerializableString _rootValueSeparator
			 = DEFAULT_ROOT_VALUE_SEPARATOR;

		/// <summary>Default constructor used to create factory instances.</summary>
		/// <remarks>
		/// Default constructor used to create factory instances.
		/// Creation of a factory instance is a light-weight operation,
		/// but it is still a good idea to reuse limited number of
		/// factory instances (and quite often just a single instance):
		/// factories are used as context for storing some reused
		/// processing objects (such as symbol tables parsers use)
		/// and this reuse only works within context of a single
		/// factory instance.
		/// </remarks>
		public JsonFactory()
			: this(null)
		{
		}

		public JsonFactory(com.fasterxml.jackson.core.ObjectCodec oc)
		{
			/*
			/**********************************************************
			/* Constants
			/**********************************************************
			*/
			/*
			/**********************************************************
			/* Buffer, symbol table management
			/**********************************************************
			*/
			/*
			/**********************************************************
			/* Configuration
			/**********************************************************
			*/
			/*
			/**********************************************************
			/* Construction
			/**********************************************************
			*/
			_objectCodec = oc;
		}

		/// <summary>Constructor used when copy()ing a factory instance.</summary>
		/// <since>2.2.1</since>
		protected internal JsonFactory(com.fasterxml.jackson.core.JsonFactory src, com.fasterxml.jackson.core.ObjectCodec
			 codec)
		{
			_objectCodec = null;
			_factoryFeatures = src._factoryFeatures;
			_parserFeatures = src._parserFeatures;
			_generatorFeatures = src._generatorFeatures;
			_characterEscapes = src._characterEscapes;
			_inputDecorator = src._inputDecorator;
			_outputDecorator = src._outputDecorator;
			_rootValueSeparator = src._rootValueSeparator;
		}

		/* 27-Apr-2013, tatu: How about symbol table; should we try to
		*   reuse shared symbol tables? Could be more efficient that way;
		*   although can slightly add to concurrency overhead.
		*/
		/// <summary>
		/// Method for constructing a new
		/// <see cref="JsonFactory"/>
		/// that has
		/// the same settings as this instance, but is otherwise
		/// independent (i.e. nothing is actually shared, symbol tables
		/// are separate).
		/// Note that
		/// <see cref="ObjectCodec"/>
		/// reference is not copied but is
		/// set to null; caller typically needs to set it after calling
		/// this method. Reason for this is that the codec is used for
		/// callbacks, and assumption is that there is strict 1-to-1
		/// mapping between codec, factory. Caller has to, then, explicitly
		/// set codec after making the copy.
		/// </summary>
		/// <since>2.1</since>
		public virtual com.fasterxml.jackson.core.JsonFactory copy()
		{
			_checkInvalidCopy(typeof(com.fasterxml.jackson.core.JsonFactory));
			// as per above, do clear ObjectCodec
			return new com.fasterxml.jackson.core.JsonFactory(this, null);
		}

		/// <since>2.1</since>
		/// <param name="exp"/>
		protected internal virtual void _checkInvalidCopy(System.Type exp)
		{
			if (GetType() != exp)
			{
				throw new System.InvalidOperationException("Failed copy(): " + GetType().FullName
					 + " (version: " + version() + ") does not override copy(); it has to");
			}
		}

		/*
		/**********************************************************
		/* Serializable overrides
		/**********************************************************
		*/
		/// <summary>
		/// Method that we need to override to actually make restoration go
		/// through constructors etc.
		/// </summary>
		/// <remarks>
		/// Method that we need to override to actually make restoration go
		/// through constructors etc.
		/// Also: must be overridden by sub-classes as well.
		/// </remarks>
		protected internal virtual object readResolve()
		{
			return new com.fasterxml.jackson.core.JsonFactory(this, _objectCodec);
		}

		/*
		/**********************************************************
		/* Capability introspection
		/**********************************************************
		*/
		/// <summary>
		/// Introspection method that higher-level functionality may call
		/// to see whether underlying data format requires a stable ordering
		/// of object properties or not.
		/// </summary>
		/// <remarks>
		/// Introspection method that higher-level functionality may call
		/// to see whether underlying data format requires a stable ordering
		/// of object properties or not.
		/// This is usually used for determining
		/// whether to force a stable ordering (like alphabetic ordering by name)
		/// if no ordering if explicitly specified.
		/// <p>
		/// Default implementation returns <code>false</code> as JSON does NOT
		/// require stable ordering. Formats that require ordering include positional
		/// textual formats like <code>CSV</code>, and schema-based binary formats
		/// like <code>Avro</code>.
		/// </remarks>
		/// <since>2.3</since>
		public virtual bool requiresPropertyOrdering()
		{
			return false;
		}

		/// <summary>
		/// Introspection method that higher-level functionality may call
		/// to see whether underlying data format can read and write binary
		/// data natively; that is, embeded it as-is without using encodings
		/// such as Base64.
		/// </summary>
		/// <remarks>
		/// Introspection method that higher-level functionality may call
		/// to see whether underlying data format can read and write binary
		/// data natively; that is, embeded it as-is without using encodings
		/// such as Base64.
		/// <p>
		/// Default implementation returns <code>false</code> as JSON does not
		/// support native access: all binary content must use Base64 encoding.
		/// Most binary formats (like Smile and Avro) support native binary content.
		/// </remarks>
		/// <since>2.3</since>
		public virtual bool canHandleBinaryNatively()
		{
			return false;
		}

		/// <summary>
		/// Introspection method that can be used by base factory to check
		/// whether access using <code>char[]</code> is something that actual
		/// parser implementations can take advantage of, over having to
		/// use
		/// <see cref="System.IO.StreamReader"/>
		/// . Sub-types are expected to override
		/// definition; default implementation (suitable for JSON) alleges
		/// that optimization are possible; and thereby is likely to try
		/// to access
		/// <see cref="string"/>
		/// content by first copying it into
		/// recyclable intermediate buffer.
		/// </summary>
		/// <since>2.4</since>
		public virtual bool canUseCharArrays()
		{
			return true;
		}

		/// <summary>
		/// Method for accessing kind of
		/// <see cref="FormatFeature"/>
		/// that a parser
		/// <see cref="JsonParser"/>
		/// produced by this factory would accept, if any;
		/// <code>null</code> returned if none.
		/// </summary>
		/// <since>2.6</since>
		public virtual System.Type getFormatReadFeatureType()
		{
			return null;
		}

		/// <summary>
		/// Method for accessing kind of
		/// <see cref="FormatFeature"/>
		/// that a parser
		/// <see cref="JsonGenerator"/>
		/// produced by this factory would accept, if any;
		/// <code>null</code> returned if none.
		/// </summary>
		/// <since>2.6</since>
		public virtual System.Type getFormatWriteFeatureType()
		{
			return null;
		}

		/*
		/**********************************************************
		/* Format detection functionality
		/**********************************************************
		*/
		/// <summary>
		/// Method that can be used to quickly check whether given schema
		/// is something that parsers and/or generators constructed by this
		/// factory could use.
		/// </summary>
		/// <remarks>
		/// Method that can be used to quickly check whether given schema
		/// is something that parsers and/or generators constructed by this
		/// factory could use. Note that this means possible use, at the level
		/// of data format (i.e. schema is for same data format as parsers and
		/// generators this factory constructs); individual schema instances
		/// may have further usage restrictions.
		/// </remarks>
		/// <since>2.1</since>
		public virtual bool canUseSchema(com.fasterxml.jackson.core.FormatSchema schema)
		{
			string ourFormat = getFormatName();
			return (ourFormat != null) && ourFormat.Equals(schema.getSchemaType());
		}

		/// <summary>
		/// Method that returns short textual id identifying format
		/// this factory supports.
		/// </summary>
		/// <remarks>
		/// Method that returns short textual id identifying format
		/// this factory supports.
		/// <p>
		/// Note: sub-classes should override this method; default
		/// implementation will return null for all sub-classes
		/// </remarks>
		public virtual string getFormatName()
		{
			/* Somewhat nasty check: since we can't make this abstract
			* (due to backwards compatibility concerns), need to prevent
			* format name "leakage"
			*/
			if (GetType() == typeof(com.fasterxml.jackson.core.JsonFactory))
			{
				return FORMAT_NAME_JSON;
			}
			return null;
		}

		/// <summary>
		/// Convenience method for trying to determine whether input via given accessor
		/// is of format type supported by this factory.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		public virtual com.fasterxml.jackson.core.format.MatchStrength hasFormat(com.fasterxml.jackson.core.format.InputAccessor
			 acc)
		{
			// since we can't keep this abstract, only implement for "vanilla" instance
			if (GetType() == typeof(com.fasterxml.jackson.core.JsonFactory))
			{
				return hasJSONFormat(acc);
			}
			return null;
		}

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

		/// <summary>
		/// Helper method that can be called to determine if content accessed
		/// using given accessor seems to be JSON content.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		protected internal virtual com.fasterxml.jackson.core.format.MatchStrength hasJSONFormat
			(com.fasterxml.jackson.core.format.InputAccessor acc)
		{
			return com.fasterxml.jackson.core.json.ByteSourceJsonBootstrapper.hasJSONFormat(acc
				);
		}

		/*
		/**********************************************************
		/* Versioned
		/**********************************************************
		*/
		public virtual com.fasterxml.jackson.core.Version version()
		{
			return com.fasterxml.jackson.core.json.PackageVersion.VERSION;
		}

		/*
		/**********************************************************
		/* Configuration, factory features
		/**********************************************************
		*/
		/// <summary>
		/// Method for enabling or disabling specified parser feature
		/// (check
		/// <see cref="Feature"/>
		/// for list of features)
		/// </summary>
		public com.fasterxml.jackson.core.JsonFactory configure(com.fasterxml.jackson.core.JsonFactory.Feature
			 f, bool state)
		{
			return state ? enable(f) : disable(f);
		}

		/// <summary>
		/// Method for enabling specified parser feature
		/// (check
		/// <see cref="Feature"/>
		/// for list of features)
		/// </summary>
		public virtual com.fasterxml.jackson.core.JsonFactory enable(com.fasterxml.jackson.core.JsonFactory.Feature
			 f)
		{
			_factoryFeatures |= f.getMask();
			return this;
		}

		/// <summary>
		/// Method for disabling specified parser features
		/// (check
		/// <see cref="Feature"/>
		/// for list of features)
		/// </summary>
		public virtual com.fasterxml.jackson.core.JsonFactory disable(com.fasterxml.jackson.core.JsonFactory.Feature
			 f)
		{
			_factoryFeatures &= ~f.getMask();
			return this;
		}

		/// <summary>Checked whether specified parser feature is enabled.</summary>
		public bool isEnabled(com.fasterxml.jackson.core.JsonFactory.Feature f)
		{
			return (_factoryFeatures & f.getMask()) != 0;
		}

		/*
		/**********************************************************
		/* Configuration, parser configuration
		/**********************************************************
		*/
		/// <summary>
		/// Method for enabling or disabling specified parser feature
		/// (check
		/// <see cref="Feature"/>
		/// for list of features)
		/// </summary>
		public com.fasterxml.jackson.core.JsonFactory configure(com.fasterxml.jackson.core.JsonParser.Feature
			 f, bool state)
		{
			return state ? enable(f) : disable(f);
		}

		/// <summary>
		/// Method for enabling specified parser feature
		/// (check
		/// <see cref="Feature"/>
		/// for list of features)
		/// </summary>
		public virtual com.fasterxml.jackson.core.JsonFactory enable(com.fasterxml.jackson.core.JsonParser.Feature
			 f)
		{
			_parserFeatures |= f.getMask();
			return this;
		}

		/// <summary>
		/// Method for disabling specified parser features
		/// (check
		/// <see cref="Feature"/>
		/// for list of features)
		/// </summary>
		public virtual com.fasterxml.jackson.core.JsonFactory disable(com.fasterxml.jackson.core.JsonParser.Feature
			 f)
		{
			_parserFeatures &= ~f.getMask();
			return this;
		}

		/// <summary>Checked whether specified parser feature is enabled.</summary>
		public bool isEnabled(com.fasterxml.jackson.core.JsonParser.Feature f)
		{
			return (_parserFeatures & f.getMask()) != 0;
		}

		/// <summary>
		/// Method for getting currently configured input decorator (if any;
		/// there is no default decorator).
		/// </summary>
		public virtual com.fasterxml.jackson.core.io.InputDecorator getInputDecorator()
		{
			return _inputDecorator;
		}

		/// <summary>Method for overriding currently configured input decorator</summary>
		public virtual com.fasterxml.jackson.core.JsonFactory setInputDecorator(com.fasterxml.jackson.core.io.InputDecorator
			 d)
		{
			_inputDecorator = d;
			return this;
		}

		/*
		/**********************************************************
		/* Configuration, generator settings
		/**********************************************************
		*/
		/// <summary>
		/// Method for enabling or disabling specified generator feature
		/// (check
		/// <see cref="Feature"/>
		/// for list of features)
		/// </summary>
		public com.fasterxml.jackson.core.JsonFactory configure(com.fasterxml.jackson.core.JsonGenerator.Feature
			 f, bool state)
		{
			return state ? enable(f) : disable(f);
		}

		/// <summary>
		/// Method for enabling specified generator features
		/// (check
		/// <see cref="Feature"/>
		/// for list of features)
		/// </summary>
		public virtual com.fasterxml.jackson.core.JsonFactory enable(com.fasterxml.jackson.core.JsonGenerator.Feature
			 f)
		{
			_generatorFeatures |= f.getMask();
			return this;
		}

		/// <summary>
		/// Method for disabling specified generator feature
		/// (check
		/// <see cref="Feature"/>
		/// for list of features)
		/// </summary>
		public virtual com.fasterxml.jackson.core.JsonFactory disable(com.fasterxml.jackson.core.JsonGenerator.Feature
			 f)
		{
			_generatorFeatures &= ~f.getMask();
			return this;
		}

		/// <summary>Check whether specified generator feature is enabled.</summary>
		public bool isEnabled(com.fasterxml.jackson.core.JsonGenerator.Feature f)
		{
			return (_generatorFeatures & f.getMask()) != 0;
		}

		/// <summary>
		/// Method for accessing custom escapes factory uses for
		/// <see cref="JsonGenerator"/>
		/// s
		/// it creates.
		/// </summary>
		public virtual com.fasterxml.jackson.core.io.CharacterEscapes getCharacterEscapes
			()
		{
			return _characterEscapes;
		}

		/// <summary>
		/// Method for defining custom escapes factory uses for
		/// <see cref="JsonGenerator"/>
		/// s
		/// it creates.
		/// </summary>
		public virtual com.fasterxml.jackson.core.JsonFactory setCharacterEscapes(com.fasterxml.jackson.core.io.CharacterEscapes
			 esc)
		{
			_characterEscapes = esc;
			return this;
		}

		/// <summary>
		/// Method for getting currently configured output decorator (if any;
		/// there is no default decorator).
		/// </summary>
		public virtual com.fasterxml.jackson.core.io.OutputDecorator getOutputDecorator()
		{
			return _outputDecorator;
		}

		/// <summary>Method for overriding currently configured output decorator</summary>
		public virtual com.fasterxml.jackson.core.JsonFactory setOutputDecorator(com.fasterxml.jackson.core.io.OutputDecorator
			 d)
		{
			_outputDecorator = d;
			return this;
		}

		/// <summary>
		/// Method that allows overriding String used for separating root-level
		/// JSON values (default is single space character)
		/// </summary>
		/// <param name="sep">
		/// Separator to use, if any; null means that no separator is
		/// automatically added
		/// </param>
		/// <since>2.1</since>
		public virtual com.fasterxml.jackson.core.JsonFactory setRootValueSeparator(string
			 sep)
		{
			_rootValueSeparator = (sep == null) ? null : new com.fasterxml.jackson.core.io.SerializedString
				(sep);
			return this;
		}

		/// <since>2.1</since>
		public virtual string getRootValueSeparator()
		{
			return (_rootValueSeparator == null) ? null : _rootValueSeparator.getValue();
		}

		/*
		/**********************************************************
		/* Configuration, other
		/**********************************************************
		*/
		/// <summary>
		/// Method for associating a
		/// <see cref="ObjectCodec"/>
		/// (typically
		/// a <code>com.fasterxml.jackson.databind.ObjectMapper</code>)
		/// with this factory (and more importantly, parsers and generators
		/// it constructs). This is needed to use data-binding methods
		/// of
		/// <see cref="JsonParser"/>
		/// and
		/// <see cref="JsonGenerator"/>
		/// instances.
		/// </summary>
		public virtual com.fasterxml.jackson.core.JsonFactory setCodec(com.fasterxml.jackson.core.ObjectCodec
			 oc)
		{
			_objectCodec = oc;
			return this;
		}

		public virtual com.fasterxml.jackson.core.ObjectCodec getCodec()
		{
			return _objectCodec;
		}

		/*
		/**********************************************************
		/* Parser factories (new ones, as per [Issue-25])
		/**********************************************************
		*/
		/// <summary>
		/// Method for constructing JSON parser instance to parse
		/// contents of specified file.
		/// </summary>
		/// <remarks>
		/// Method for constructing JSON parser instance to parse
		/// contents of specified file. Encoding is auto-detected
		/// from contents according to JSON specification recommended
		/// mechanism.
		/// <p>
		/// Underlying input stream (needed for reading contents)
		/// will be <b>owned</b> (and managed, i.e. closed as need be) by
		/// the parser, since caller has no access to it.
		/// </remarks>
		/// <param name="f">File that contains JSON content to parse</param>
		/// <since>2.1</since>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonParseException"/>
		public virtual com.fasterxml.jackson.core.JsonParser createParser(Sharpen.FilePath
			 f)
		{
			// true, since we create InputStream from File
			com.fasterxml.jackson.core.io.IOContext ctxt = _createContext(f, true);
			Sharpen.InputStream @in = new java.io.FileInputStream(f);
			return _createParser(_decorate(@in, ctxt), ctxt);
		}

		/// <summary>
		/// Method for constructing JSON parser instance to parse
		/// contents of resource reference by given URL.
		/// </summary>
		/// <remarks>
		/// Method for constructing JSON parser instance to parse
		/// contents of resource reference by given URL.
		/// Encoding is auto-detected
		/// from contents according to JSON specification recommended
		/// mechanism.
		/// <p>
		/// Underlying input stream (needed for reading contents)
		/// will be <b>owned</b> (and managed, i.e. closed as need be) by
		/// the parser, since caller has no access to it.
		/// </remarks>
		/// <param name="url">URL pointing to resource that contains JSON content to parse</param>
		/// <since>2.1</since>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonParseException"/>
		public virtual com.fasterxml.jackson.core.JsonParser createParser(System.Uri url)
		{
			// true, since we create InputStream from URL
			com.fasterxml.jackson.core.io.IOContext ctxt = _createContext(url, true);
			Sharpen.InputStream @in = _optimizedStreamFromURL(url);
			return _createParser(_decorate(@in, ctxt), ctxt);
		}

		/// <summary>
		/// Method for constructing JSON parser instance to parse
		/// the contents accessed via specified input stream.
		/// </summary>
		/// <remarks>
		/// Method for constructing JSON parser instance to parse
		/// the contents accessed via specified input stream.
		/// <p>
		/// The input stream will <b>not be owned</b> by
		/// the parser, it will still be managed (i.e. closed if
		/// end-of-stream is reacher, or parser close method called)
		/// if (and only if)
		/// <see cref="Feature.AUTO_CLOSE_SOURCE"/>
		/// is enabled.
		/// <p>
		/// Note: no encoding argument is taken since it can always be
		/// auto-detected as suggested by JSON RFC.
		/// </remarks>
		/// <param name="in">InputStream to use for reading JSON content to parse</param>
		/// <since>2.1</since>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonParseException"/>
		public virtual com.fasterxml.jackson.core.JsonParser createParser(Sharpen.InputStream
			 @in)
		{
			com.fasterxml.jackson.core.io.IOContext ctxt = _createContext(@in, false);
			return _createParser(_decorate(@in, ctxt), ctxt);
		}

		/// <summary>
		/// Method for constructing parser for parsing
		/// the contents accessed via specified Reader.
		/// </summary>
		/// <remarks>
		/// Method for constructing parser for parsing
		/// the contents accessed via specified Reader.
		/// <p>
		/// The read stream will <b>not be owned</b> by
		/// the parser, it will still be managed (i.e. closed if
		/// end-of-stream is reacher, or parser close method called)
		/// if (and only if)
		/// <see cref="Feature.AUTO_CLOSE_SOURCE"/>
		/// is enabled.
		/// </remarks>
		/// <param name="r">Reader to use for reading JSON content to parse</param>
		/// <since>2.1</since>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonParseException"/>
		public virtual com.fasterxml.jackson.core.JsonParser createParser(System.IO.StreamReader
			 r)
		{
			// false -> we do NOT own Reader (did not create it)
			com.fasterxml.jackson.core.io.IOContext ctxt = _createContext(r, false);
			return _createParser(_decorate(r, ctxt), ctxt);
		}

		/// <summary>
		/// Method for constructing parser for parsing
		/// the contents of given byte array.
		/// </summary>
		/// <since>2.1</since>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonParseException"/>
		public virtual com.fasterxml.jackson.core.JsonParser createParser(byte[] data)
		{
			com.fasterxml.jackson.core.io.IOContext ctxt = _createContext(data, true);
			if (_inputDecorator != null)
			{
				Sharpen.InputStream @in = _inputDecorator.decorate(ctxt, data, 0, data.Length);
				if (@in != null)
				{
					return _createParser(@in, ctxt);
				}
			}
			return _createParser(data, 0, data.Length, ctxt);
		}

		/// <summary>
		/// Method for constructing parser for parsing
		/// the contents of given byte array.
		/// </summary>
		/// <param name="data">Buffer that contains data to parse</param>
		/// <param name="offset">Offset of the first data byte within buffer</param>
		/// <param name="len">Length of contents to parse within buffer</param>
		/// <since>2.1</since>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonParseException"/>
		public virtual com.fasterxml.jackson.core.JsonParser createParser(byte[] data, int
			 offset, int len)
		{
			com.fasterxml.jackson.core.io.IOContext ctxt = _createContext(data, true);
			// [JACKSON-512]: allow wrapping with InputDecorator
			if (_inputDecorator != null)
			{
				Sharpen.InputStream @in = _inputDecorator.decorate(ctxt, data, offset, len);
				if (@in != null)
				{
					return _createParser(@in, ctxt);
				}
			}
			return _createParser(data, offset, len, ctxt);
		}

		/// <summary>
		/// Method for constructing parser for parsing
		/// contents of given String.
		/// </summary>
		/// <since>2.1</since>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonParseException"/>
		public virtual com.fasterxml.jackson.core.JsonParser createParser(string content)
		{
			int strLen = content.Length;
			// Actually, let's use this for medium-sized content, up to 64kB chunk (32kb char)
			if (_inputDecorator != null || strLen > unchecked((int)(0x8000)) || !canUseCharArrays
				())
			{
				// easier to just wrap in a Reader than extend InputDecorator; or, if content
				// is too long for us to copy it over
				return createParser(new Sharpen.StringReader(content));
			}
			com.fasterxml.jackson.core.io.IOContext ctxt = _createContext(content, true);
			char[] buf = ctxt.allocTokenBuffer(strLen);
			Sharpen.Runtime.getCharsForString(content, 0, strLen, buf, 0);
			return _createParser(buf, 0, strLen, ctxt, true);
		}

		/// <summary>
		/// Method for constructing parser for parsing
		/// contents of given char array.
		/// </summary>
		/// <since>2.4</since>
		/// <exception cref="System.IO.IOException"/>
		public virtual com.fasterxml.jackson.core.JsonParser createParser(char[] content)
		{
			return createParser(content, 0, content.Length);
		}

		/// <summary>Method for constructing parser for parsing contents of given char array.
		/// 	</summary>
		/// <since>2.4</since>
		/// <exception cref="System.IO.IOException"/>
		public virtual com.fasterxml.jackson.core.JsonParser createParser(char[] content, 
			int offset, int len)
		{
			if (_inputDecorator != null)
			{
				// easier to just wrap in a Reader than extend InputDecorator
				return createParser(new java.io.CharArrayReader(content, offset, len));
			}
			return _createParser(content, offset, len, _createContext(content, true), false);
		}

		// important: buffer is NOT recyclable, as it's from caller
		/*
		/**********************************************************
		/* Parser factories (old ones, as per [Issue-25])
		/**********************************************************
		*/
		/// <summary>
		/// Method for constructing JSON parser instance to parse
		/// contents of specified file.
		/// </summary>
		/// <remarks>
		/// Method for constructing JSON parser instance to parse
		/// contents of specified file. Encoding is auto-detected
		/// from contents according to JSON specification recommended
		/// mechanism.
		/// <p>
		/// Underlying input stream (needed for reading contents)
		/// will be <b>owned</b> (and managed, i.e. closed as need be) by
		/// the parser, since caller has no access to it.
		/// </remarks>
		/// <param name="f">File that contains JSON content to parse</param>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonParseException"/>
		[System.ObsoleteAttribute(@"Since 2.2, use createParser(Sharpen.FilePath) instead."
			)]
		public virtual com.fasterxml.jackson.core.JsonParser createJsonParser(Sharpen.FilePath
			 f)
		{
			return createParser(f);
		}

		/// <summary>
		/// Method for constructing JSON parser instance to parse
		/// contents of resource reference by given URL.
		/// </summary>
		/// <remarks>
		/// Method for constructing JSON parser instance to parse
		/// contents of resource reference by given URL.
		/// Encoding is auto-detected
		/// from contents according to JSON specification recommended
		/// mechanism.
		/// <p>
		/// Underlying input stream (needed for reading contents)
		/// will be <b>owned</b> (and managed, i.e. closed as need be) by
		/// the parser, since caller has no access to it.
		/// </remarks>
		/// <param name="url">URL pointing to resource that contains JSON content to parse</param>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonParseException"/>
		[System.ObsoleteAttribute(@"Since 2.2, use createParser(System.Uri) instead.")]
		public virtual com.fasterxml.jackson.core.JsonParser createJsonParser(System.Uri 
			url)
		{
			return createParser(url);
		}

		/// <summary>
		/// Method for constructing JSON parser instance to parse
		/// the contents accessed via specified input stream.
		/// </summary>
		/// <remarks>
		/// Method for constructing JSON parser instance to parse
		/// the contents accessed via specified input stream.
		/// <p>
		/// The input stream will <b>not be owned</b> by
		/// the parser, it will still be managed (i.e. closed if
		/// end-of-stream is reacher, or parser close method called)
		/// if (and only if)
		/// <see cref="Feature.AUTO_CLOSE_SOURCE"/>
		/// is enabled.
		/// <p>
		/// Note: no encoding argument is taken since it can always be
		/// auto-detected as suggested by JSON RFC.
		/// </remarks>
		/// <param name="in">InputStream to use for reading JSON content to parse</param>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonParseException"/>
		[System.ObsoleteAttribute(@"Since 2.2, use createParser(Sharpen.InputStream) instead."
			)]
		public virtual com.fasterxml.jackson.core.JsonParser createJsonParser(Sharpen.InputStream
			 @in)
		{
			return createParser(@in);
		}

		/// <summary>
		/// Method for constructing parser for parsing
		/// the contents accessed via specified Reader.
		/// </summary>
		/// <remarks>
		/// Method for constructing parser for parsing
		/// the contents accessed via specified Reader.
		/// <p>
		/// The read stream will <b>not be owned</b> by
		/// the parser, it will still be managed (i.e. closed if
		/// end-of-stream is reacher, or parser close method called)
		/// if (and only if)
		/// <see cref="Feature.AUTO_CLOSE_SOURCE"/>
		/// is enabled.
		/// </remarks>
		/// <param name="r">Reader to use for reading JSON content to parse</param>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonParseException"/>
		[System.ObsoleteAttribute(@"Since 2.2, use createParser(System.IO.StreamReader) instead."
			)]
		public virtual com.fasterxml.jackson.core.JsonParser createJsonParser(System.IO.StreamReader
			 r)
		{
			return createParser(r);
		}

		/// <summary>Method for constructing parser for parsing the contents of given byte array.
		/// 	</summary>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonParseException"/>
		[System.ObsoleteAttribute(@"Since 2.2, use createParser(byte[]) instead.")]
		public virtual com.fasterxml.jackson.core.JsonParser createJsonParser(byte[] data
			)
		{
			return createParser(data);
		}

		/// <summary>
		/// Method for constructing parser for parsing
		/// the contents of given byte array.
		/// </summary>
		/// <param name="data">Buffer that contains data to parse</param>
		/// <param name="offset">Offset of the first data byte within buffer</param>
		/// <param name="len">Length of contents to parse within buffer</param>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonParseException"/>
		[System.ObsoleteAttribute(@"Since 2.2, use createParser(byte[], int, int) instead."
			)]
		public virtual com.fasterxml.jackson.core.JsonParser createJsonParser(byte[] data
			, int offset, int len)
		{
			return createParser(data, offset, len);
		}

		/// <summary>
		/// Method for constructing parser for parsing
		/// contents of given String.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonParseException"/>
		[System.ObsoleteAttribute(@"Since 2.2, use createParser(string) instead.")]
		public virtual com.fasterxml.jackson.core.JsonParser createJsonParser(string content
			)
		{
			return createParser(content);
		}

		/*
		/**********************************************************
		/* Generator factories, new (as per [Issue-25]
		/**********************************************************
		*/
		/// <summary>
		/// Method for constructing JSON generator for writing JSON content
		/// using specified output stream.
		/// </summary>
		/// <remarks>
		/// Method for constructing JSON generator for writing JSON content
		/// using specified output stream.
		/// Encoding to use must be specified, and needs to be one of available
		/// types (as per JSON specification).
		/// <p>
		/// Underlying stream <b>is NOT owned</b> by the generator constructed,
		/// so that generator will NOT close the output stream when
		/// <see cref="JsonGenerator.close()"/>
		/// is called (unless auto-closing
		/// feature,
		/// <see cref="Feature.AUTO_CLOSE_TARGET"/>
		/// is enabled).
		/// Using application needs to close it explicitly if this is the case.
		/// <p>
		/// Note: there are formats that use fixed encoding (like most binary data formats)
		/// and that ignore passed in encoding.
		/// </remarks>
		/// <param name="out">OutputStream to use for writing JSON content</param>
		/// <param name="enc">Character encoding to use</param>
		/// <since>2.1</since>
		/// <exception cref="System.IO.IOException"/>
		public virtual com.fasterxml.jackson.core.JsonGenerator createGenerator(Sharpen.OutputStream
			 @out, com.fasterxml.jackson.core.JsonEncoding enc)
		{
			// false -> we won't manage the stream unless explicitly directed to
			com.fasterxml.jackson.core.io.IOContext ctxt = _createContext(@out, false);
			ctxt.setEncoding(enc);
			if (enc == com.fasterxml.jackson.core.JsonEncoding.UTF8)
			{
				return _createUTF8Generator(_decorate(@out, ctxt), ctxt);
			}
			System.IO.TextWriter w = _createWriter(@out, enc, ctxt);
			return _createGenerator(_decorate(w, ctxt), ctxt);
		}

		/// <summary>
		/// Convenience method for constructing generator that uses default
		/// encoding of the format (UTF-8 for JSON and most other data formats).
		/// </summary>
		/// <remarks>
		/// Convenience method for constructing generator that uses default
		/// encoding of the format (UTF-8 for JSON and most other data formats).
		/// <p>
		/// Note: there are formats that use fixed encoding (like most binary data formats).
		/// </remarks>
		/// <since>2.1</since>
		/// <exception cref="System.IO.IOException"/>
		public virtual com.fasterxml.jackson.core.JsonGenerator createGenerator(Sharpen.OutputStream
			 @out)
		{
			return createGenerator(@out, com.fasterxml.jackson.core.JsonEncoding.UTF8);
		}

		/// <summary>
		/// Method for constructing JSON generator for writing JSON content
		/// using specified Writer.
		/// </summary>
		/// <remarks>
		/// Method for constructing JSON generator for writing JSON content
		/// using specified Writer.
		/// <p>
		/// Underlying stream <b>is NOT owned</b> by the generator constructed,
		/// so that generator will NOT close the Reader when
		/// <see cref="JsonGenerator.close()"/>
		/// is called (unless auto-closing
		/// feature,
		/// <see cref="Feature.AUTO_CLOSE_TARGET"/>
		/// is enabled).
		/// Using application needs to close it explicitly.
		/// </remarks>
		/// <since>2.1</since>
		/// <param name="w">Writer to use for writing JSON content</param>
		/// <exception cref="System.IO.IOException"/>
		public virtual com.fasterxml.jackson.core.JsonGenerator createGenerator(System.IO.TextWriter
			 w)
		{
			com.fasterxml.jackson.core.io.IOContext ctxt = _createContext(w, false);
			return _createGenerator(_decorate(w, ctxt), ctxt);
		}

		/// <summary>
		/// Method for constructing JSON generator for writing JSON content
		/// to specified file, overwriting contents it might have (or creating
		/// it if such file does not yet exist).
		/// </summary>
		/// <remarks>
		/// Method for constructing JSON generator for writing JSON content
		/// to specified file, overwriting contents it might have (or creating
		/// it if such file does not yet exist).
		/// Encoding to use must be specified, and needs to be one of available
		/// types (as per JSON specification).
		/// <p>
		/// Underlying stream <b>is owned</b> by the generator constructed,
		/// i.e. generator will handle closing of file when
		/// <see cref="JsonGenerator.close()"/>
		/// is called.
		/// </remarks>
		/// <param name="f">File to write contents to</param>
		/// <param name="enc">Character encoding to use</param>
		/// <since>2.1</since>
		/// <exception cref="System.IO.IOException"/>
		public virtual com.fasterxml.jackson.core.JsonGenerator createGenerator(Sharpen.FilePath
			 f, com.fasterxml.jackson.core.JsonEncoding enc)
		{
			Sharpen.OutputStream @out = new java.io.FileOutputStream(f);
			// true -> yes, we have to manage the stream since we created it
			com.fasterxml.jackson.core.io.IOContext ctxt = _createContext(@out, true);
			ctxt.setEncoding(enc);
			if (enc == com.fasterxml.jackson.core.JsonEncoding.UTF8)
			{
				return _createUTF8Generator(_decorate(@out, ctxt), ctxt);
			}
			System.IO.TextWriter w = _createWriter(@out, enc, ctxt);
			return _createGenerator(_decorate(w, ctxt), ctxt);
		}

		/*
		/**********************************************************
		/* Generator factories, old (as per [Issue-25]
		/**********************************************************
		*/
		/// <summary>
		/// Method for constructing JSON generator for writing JSON content
		/// using specified output stream.
		/// </summary>
		/// <remarks>
		/// Method for constructing JSON generator for writing JSON content
		/// using specified output stream.
		/// Encoding to use must be specified, and needs to be one of available
		/// types (as per JSON specification).
		/// <p>
		/// Underlying stream <b>is NOT owned</b> by the generator constructed,
		/// so that generator will NOT close the output stream when
		/// <see cref="JsonGenerator.close()"/>
		/// is called (unless auto-closing
		/// feature,
		/// <see cref="Feature.AUTO_CLOSE_TARGET"/>
		/// is enabled).
		/// Using application needs to close it explicitly if this is the case.
		/// <p>
		/// Note: there are formats that use fixed encoding (like most binary data formats)
		/// and that ignore passed in encoding.
		/// </remarks>
		/// <param name="out">OutputStream to use for writing JSON content</param>
		/// <param name="enc">Character encoding to use</param>
		/// <exception cref="System.IO.IOException"/>
		[System.ObsoleteAttribute(@"Since 2.2, use createGenerator(Sharpen.OutputStream, JsonEncoding) instead."
			)]
		public virtual com.fasterxml.jackson.core.JsonGenerator createJsonGenerator(Sharpen.OutputStream
			 @out, com.fasterxml.jackson.core.JsonEncoding enc)
		{
			return createGenerator(@out, enc);
		}

		/// <summary>
		/// Method for constructing JSON generator for writing JSON content
		/// using specified Writer.
		/// </summary>
		/// <remarks>
		/// Method for constructing JSON generator for writing JSON content
		/// using specified Writer.
		/// <p>
		/// Underlying stream <b>is NOT owned</b> by the generator constructed,
		/// so that generator will NOT close the Reader when
		/// <see cref="JsonGenerator.close()"/>
		/// is called (unless auto-closing
		/// feature,
		/// <see cref="Feature.AUTO_CLOSE_TARGET"/>
		/// is enabled).
		/// Using application needs to close it explicitly.
		/// </remarks>
		/// <param name="out">Writer to use for writing JSON content</param>
		/// <exception cref="System.IO.IOException"/>
		[System.ObsoleteAttribute(@"Since 2.2, use createGenerator(System.IO.TextWriter) instead."
			)]
		public virtual com.fasterxml.jackson.core.JsonGenerator createJsonGenerator(System.IO.TextWriter
			 @out)
		{
			return createGenerator(@out);
		}

		/// <summary>
		/// Convenience method for constructing generator that uses default
		/// encoding of the format (UTF-8 for JSON and most other data formats).
		/// </summary>
		/// <remarks>
		/// Convenience method for constructing generator that uses default
		/// encoding of the format (UTF-8 for JSON and most other data formats).
		/// <p>
		/// Note: there are formats that use fixed encoding (like most binary data formats).
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		[System.ObsoleteAttribute(@"Since 2.2, use createGenerator(Sharpen.OutputStream) instead."
			)]
		public virtual com.fasterxml.jackson.core.JsonGenerator createJsonGenerator(Sharpen.OutputStream
			 @out)
		{
			return createGenerator(@out, com.fasterxml.jackson.core.JsonEncoding.UTF8);
		}

		/// <summary>
		/// Method for constructing JSON generator for writing JSON content
		/// to specified file, overwriting contents it might have (or creating
		/// it if such file does not yet exist).
		/// </summary>
		/// <remarks>
		/// Method for constructing JSON generator for writing JSON content
		/// to specified file, overwriting contents it might have (or creating
		/// it if such file does not yet exist).
		/// Encoding to use must be specified, and needs to be one of available
		/// types (as per JSON specification).
		/// <p>
		/// Underlying stream <b>is owned</b> by the generator constructed,
		/// i.e. generator will handle closing of file when
		/// <see cref="JsonGenerator.close()"/>
		/// is called.
		/// </remarks>
		/// <param name="f">File to write contents to</param>
		/// <param name="enc">Character encoding to use</param>
		/// <exception cref="System.IO.IOException"/>
		[System.ObsoleteAttribute(@"Since 2.2, use createGenerator(Sharpen.FilePath, JsonEncoding) instead."
			)]
		public virtual com.fasterxml.jackson.core.JsonGenerator createJsonGenerator(Sharpen.FilePath
			 f, com.fasterxml.jackson.core.JsonEncoding enc)
		{
			return createGenerator(f, enc);
		}

		/*
		/**********************************************************
		/* Factory methods used by factory for creating parser instances,
		/* overridable by sub-classes
		/**********************************************************
		*/
		/// <summary>
		/// Overridable factory method that actually instantiates desired parser
		/// given
		/// <see cref="Sharpen.InputStream"/>
		/// and context object.
		/// <p>
		/// This method is specifically designed to remain
		/// compatible between minor versions so that sub-classes can count
		/// on it being called as expected. That is, it is part of official
		/// interface from sub-class perspective, although not a public
		/// method available to users of factory implementations.
		/// </summary>
		/// <since>2.1</since>
		/// <exception cref="System.IO.IOException"/>
		protected internal virtual com.fasterxml.jackson.core.JsonParser _createParser(Sharpen.InputStream
			 @in, com.fasterxml.jackson.core.io.IOContext ctxt)
		{
			// As per [JACKSON-259], may want to fully disable canonicalization:
			return new com.fasterxml.jackson.core.json.ByteSourceJsonBootstrapper(ctxt, @in).
				constructParser(_parserFeatures, _objectCodec, _byteSymbolCanonicalizer, _rootCharSymbols
				, _factoryFeatures);
		}

		/// <summary>
		/// Overridable factory method that actually instantiates parser
		/// using given
		/// <see cref="System.IO.StreamReader"/>
		/// object for reading content.
		/// <p>
		/// This method is specifically designed to remain
		/// compatible between minor versions so that sub-classes can count
		/// on it being called as expected. That is, it is part of official
		/// interface from sub-class perspective, although not a public
		/// method available to users of factory implementations.
		/// </summary>
		/// <since>2.1</since>
		/// <exception cref="System.IO.IOException"/>
		protected internal virtual com.fasterxml.jackson.core.JsonParser _createParser(System.IO.StreamReader
			 r, com.fasterxml.jackson.core.io.IOContext ctxt)
		{
			return new com.fasterxml.jackson.core.json.ReaderBasedJsonParser(ctxt, _parserFeatures
				, r, _objectCodec, _rootCharSymbols.makeChild(_factoryFeatures));
		}

		/// <summary>
		/// Overridable factory method that actually instantiates parser
		/// using given <code>char[]</code> object for accessing content.
		/// </summary>
		/// <since>2.4</since>
		/// <exception cref="System.IO.IOException"/>
		protected internal virtual com.fasterxml.jackson.core.JsonParser _createParser(char
			[] data, int offset, int len, com.fasterxml.jackson.core.io.IOContext ctxt, bool
			 recyclable)
		{
			return new com.fasterxml.jackson.core.json.ReaderBasedJsonParser(ctxt, _parserFeatures
				, null, _objectCodec, _rootCharSymbols.makeChild(_factoryFeatures), data, offset
				, offset + len, recyclable);
		}

		/// <summary>
		/// Overridable factory method that actually instantiates parser
		/// using given
		/// <see cref="System.IO.StreamReader"/>
		/// object for reading content
		/// passed as raw byte array.
		/// <p>
		/// This method is specifically designed to remain
		/// compatible between minor versions so that sub-classes can count
		/// on it being called as expected. That is, it is part of official
		/// interface from sub-class perspective, although not a public
		/// method available to users of factory implementations.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		protected internal virtual com.fasterxml.jackson.core.JsonParser _createParser(byte
			[] data, int offset, int len, com.fasterxml.jackson.core.io.IOContext ctxt)
		{
			return new com.fasterxml.jackson.core.json.ByteSourceJsonBootstrapper(ctxt, data, 
				offset, len).constructParser(_parserFeatures, _objectCodec, _byteSymbolCanonicalizer
				, _rootCharSymbols, _factoryFeatures);
		}

		/*
		/**********************************************************
		/* Factory methods used by factory for creating generator instances,
		/* overridable by sub-classes
		/**********************************************************
		*/
		/// <summary>
		/// Overridable factory method that actually instantiates generator for
		/// given
		/// <see cref="System.IO.TextWriter"/>
		/// and context object.
		/// <p>
		/// This method is specifically designed to remain
		/// compatible between minor versions so that sub-classes can count
		/// on it being called as expected. That is, it is part of official
		/// interface from sub-class perspective, although not a public
		/// method available to users of factory implementations.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		protected internal virtual com.fasterxml.jackson.core.JsonGenerator _createGenerator
			(System.IO.TextWriter @out, com.fasterxml.jackson.core.io.IOContext ctxt)
		{
			com.fasterxml.jackson.core.json.WriterBasedJsonGenerator gen = new com.fasterxml.jackson.core.json.WriterBasedJsonGenerator
				(ctxt, _generatorFeatures, _objectCodec, @out);
			if (_characterEscapes != null)
			{
				gen.setCharacterEscapes(_characterEscapes);
			}
			com.fasterxml.jackson.core.SerializableString rootSep = _rootValueSeparator;
			if (rootSep != DEFAULT_ROOT_VALUE_SEPARATOR)
			{
				gen.setRootValueSeparator(rootSep);
			}
			return gen;
		}

		/// <summary>
		/// Overridable factory method that actually instantiates generator for
		/// given
		/// <see cref="Sharpen.OutputStream"/>
		/// and context object, using UTF-8 encoding.
		/// <p>
		/// This method is specifically designed to remain
		/// compatible between minor versions so that sub-classes can count
		/// on it being called as expected. That is, it is part of official
		/// interface from sub-class perspective, although not a public
		/// method available to users of factory implementations.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		protected internal virtual com.fasterxml.jackson.core.JsonGenerator _createUTF8Generator
			(Sharpen.OutputStream @out, com.fasterxml.jackson.core.io.IOContext ctxt)
		{
			com.fasterxml.jackson.core.json.UTF8JsonGenerator gen = new com.fasterxml.jackson.core.json.UTF8JsonGenerator
				(ctxt, _generatorFeatures, _objectCodec, @out);
			if (_characterEscapes != null)
			{
				gen.setCharacterEscapes(_characterEscapes);
			}
			com.fasterxml.jackson.core.SerializableString rootSep = _rootValueSeparator;
			if (rootSep != DEFAULT_ROOT_VALUE_SEPARATOR)
			{
				gen.setRootValueSeparator(rootSep);
			}
			return gen;
		}

		/// <exception cref="System.IO.IOException"/>
		protected internal virtual System.IO.TextWriter _createWriter(Sharpen.OutputStream
			 @out, com.fasterxml.jackson.core.JsonEncoding enc, com.fasterxml.jackson.core.io.IOContext
			 ctxt)
		{
			// note: this should not get called any more (caller checks, dispatches)
			if (enc == com.fasterxml.jackson.core.JsonEncoding.UTF8)
			{
				// We have optimized writer for UTF-8
				return new com.fasterxml.jackson.core.io.UTF8Writer(ctxt, @out);
			}
			// not optimal, but should do unless we really care about UTF-16/32 encoding speed
			return new java.io.OutputStreamWriter(@out, enc.getJavaName());
		}

		/*
		/**********************************************************
		/* Internal factory methods, decorator handling
		/**********************************************************
		*/
		/// <since>2.4</since>
		/// <exception cref="System.IO.IOException"/>
		protected internal Sharpen.InputStream _decorate(Sharpen.InputStream @in, com.fasterxml.jackson.core.io.IOContext
			 ctxt)
		{
			if (_inputDecorator != null)
			{
				Sharpen.InputStream in2 = _inputDecorator.decorate(ctxt, @in);
				if (in2 != null)
				{
					return in2;
				}
			}
			return @in;
		}

		/// <since>2.4</since>
		/// <exception cref="System.IO.IOException"/>
		protected internal System.IO.StreamReader _decorate(System.IO.StreamReader @in, com.fasterxml.jackson.core.io.IOContext
			 ctxt)
		{
			if (_inputDecorator != null)
			{
				System.IO.StreamReader in2 = _inputDecorator.decorate(ctxt, @in);
				if (in2 != null)
				{
					return in2;
				}
			}
			return @in;
		}

		/// <since>2.4</since>
		/// <exception cref="System.IO.IOException"/>
		protected internal Sharpen.OutputStream _decorate(Sharpen.OutputStream @out, com.fasterxml.jackson.core.io.IOContext
			 ctxt)
		{
			if (_outputDecorator != null)
			{
				Sharpen.OutputStream out2 = _outputDecorator.decorate(ctxt, @out);
				if (out2 != null)
				{
					return out2;
				}
			}
			return @out;
		}

		/// <since>2.4</since>
		/// <exception cref="System.IO.IOException"/>
		protected internal System.IO.TextWriter _decorate(System.IO.TextWriter @out, com.fasterxml.jackson.core.io.IOContext
			 ctxt)
		{
			if (_outputDecorator != null)
			{
				System.IO.TextWriter out2 = _outputDecorator.decorate(ctxt, @out);
				if (out2 != null)
				{
					return out2;
				}
			}
			return @out;
		}

		/*
		/**********************************************************
		/* Internal factory methods, other
		/**********************************************************
		*/
		/// <summary>
		/// Method used by factory to create buffer recycler instances
		/// for parsers and generators.
		/// </summary>
		/// <remarks>
		/// Method used by factory to create buffer recycler instances
		/// for parsers and generators.
		/// <p>
		/// Note: only public to give access for <code>ObjectMapper</code>
		/// </remarks>
		public virtual com.fasterxml.jackson.core.util.BufferRecycler _getBufferRecycler(
			)
		{
			com.fasterxml.jackson.core.util.BufferRecycler br;
			/* 23-Apr-2015, tatu: Let's allow disabling of buffer recycling
			*   scheme, for cases where it is considered harmful (possibly
			*   on Android, for example)
			*/
			if (isEnabled(com.fasterxml.jackson.core.JsonFactory.Feature.USE_THREAD_LOCAL_FOR_BUFFER_RECYCLING
				))
			{
				Sharpen.SoftReference<com.fasterxml.jackson.core.util.BufferRecycler> @ref = _recyclerRef
					.get();
				br = (@ref == null) ? null : @ref.get();
				if (br == null)
				{
					br = new com.fasterxml.jackson.core.util.BufferRecycler();
					_recyclerRef.set(new Sharpen.SoftReference<com.fasterxml.jackson.core.util.BufferRecycler
						>(br));
				}
			}
			else
			{
				br = new com.fasterxml.jackson.core.util.BufferRecycler();
			}
			return br;
		}

		/// <summary>
		/// Overridable factory method that actually instantiates desired
		/// context object.
		/// </summary>
		protected internal virtual com.fasterxml.jackson.core.io.IOContext _createContext
			(object srcRef, bool resourceManaged)
		{
			return new com.fasterxml.jackson.core.io.IOContext(_getBufferRecycler(), srcRef, 
				resourceManaged);
		}

		/// <summary>
		/// Helper methods used for constructing an optimal stream for
		/// parsers to use, when input is to be read from an URL.
		/// </summary>
		/// <remarks>
		/// Helper methods used for constructing an optimal stream for
		/// parsers to use, when input is to be read from an URL.
		/// This helps when reading file content via URL.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		protected internal virtual Sharpen.InputStream _optimizedStreamFromURL(System.Uri
			 url)
		{
			if ("file".Equals(url.Scheme))
			{
				/* Can not do this if the path refers
				* to a network drive on windows. This fixes the problem;
				* might not be needed on all platforms (NFS?), but should not
				* matter a lot: performance penalty of extra wrapping is more
				* relevant when accessing local file system.
				*/
				string host = url.getHost();
				if (host == null || host.Length == 0)
				{
					// [Issue#48]: Let's try to avoid probs with URL encoded stuff
					string path = url.AbsolutePath;
					if (path.IndexOf('%') < 0)
					{
						return new java.io.FileInputStream(url.AbsolutePath);
					}
				}
			}
			// otherwise, let's fall through and let URL decoder do its magic
			return url.openStream();
		}
	}
}
