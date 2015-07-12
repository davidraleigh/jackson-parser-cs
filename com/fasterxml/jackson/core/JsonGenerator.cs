/* Jackson JSON-processor.
*
* Copyright (c) 2007- Tatu Saloranta, tatu.saloranta@iki.fi
*/
using Sharpen;

namespace com.fasterxml.jackson.core
{
	/// <summary>Base class that defines public API for writing JSON content.</summary>
	/// <remarks>
	/// Base class that defines public API for writing JSON content.
	/// Instances are created using factory methods of
	/// a
	/// <see cref="JsonFactory"/>
	/// instance.
	/// </remarks>
	/// <author>Tatu Saloranta</author>
	public abstract class JsonGenerator : System.IDisposable, com.fasterxml.jackson.core.Versioned
	{
		/// <summary>Enumeration that defines all togglable features for generators.</summary>
		[System.Serializable]
		public sealed class Feature
		{
			/// <summary>
			/// Feature that determines whether generator will automatically
			/// close underlying output target that is NOT owned by the
			/// generator.
			/// </summary>
			/// <remarks>
			/// Feature that determines whether generator will automatically
			/// close underlying output target that is NOT owned by the
			/// generator.
			/// If disabled, calling application has to separately
			/// close the underlying
			/// <see cref="Sharpen.OutputStream"/>
			/// and
			/// <see cref="System.IO.TextWriter"/>
			/// instances used to create the generator. If enabled, generator
			/// will handle closing, as long as generator itself gets closed:
			/// this happens when end-of-input is encountered, or generator
			/// is closed by a call to
			/// <see cref="JsonGenerator.close()"/>
			/// .
			/// <p>
			/// Feature is enabled by default.
			/// </remarks>
			public static readonly com.fasterxml.jackson.core.JsonGenerator.Feature AUTO_CLOSE_TARGET
				 = new com.fasterxml.jackson.core.JsonGenerator.Feature(true);

			/// <summary>
			/// Feature that determines what happens when the generator is
			/// closed while there are still unmatched
			/// <see cref="JsonToken.START_ARRAY"/>
			/// or
			/// <see cref="JsonToken.START_OBJECT"/>
			/// entries in output content. If enabled, such Array(s) and/or
			/// Object(s) are automatically closed; if disabled, nothing
			/// specific is done.
			/// <p>
			/// Feature is enabled by default.
			/// </summary>
			public static readonly com.fasterxml.jackson.core.JsonGenerator.Feature AUTO_CLOSE_JSON_CONTENT
				 = new com.fasterxml.jackson.core.JsonGenerator.Feature(true);

			/// <summary>
			/// Feature that specifies that calls to
			/// <see cref="#flush"/>
			/// will cause
			/// matching <code>flush()</code> to underlying
			/// <see cref="Sharpen.OutputStream"/>
			/// or
			/// <see cref="System.IO.TextWriter"/>
			/// ; if disabled this will not be done.
			/// Main reason to disable this feature is to prevent flushing at
			/// generator level, if it is not possible to prevent method being
			/// called by other code (like <code>ObjectMapper</code> or third
			/// party libraries).
			/// <p>
			/// Feature is enabled by default.
			/// </summary>
			public static readonly com.fasterxml.jackson.core.JsonGenerator.Feature FLUSH_PASSED_TO_STREAM
				 = new com.fasterxml.jackson.core.JsonGenerator.Feature(true);

			/// <summary>
			/// Feature that determines whether JSON Object field names are
			/// quoted using double-quotes, as specified by JSON specification
			/// or not.
			/// </summary>
			/// <remarks>
			/// Feature that determines whether JSON Object field names are
			/// quoted using double-quotes, as specified by JSON specification
			/// or not. Ability to disable quoting was added to support use
			/// cases where they are not usually expected, which most commonly
			/// occurs when used straight from Javascript.
			/// <p>
			/// Feature is enabled by default (since it is required by JSON specification).
			/// </remarks>
			public static readonly com.fasterxml.jackson.core.JsonGenerator.Feature QUOTE_FIELD_NAMES
				 = new com.fasterxml.jackson.core.JsonGenerator.Feature(true);

			/// <summary>
			/// Feature that determines whether "exceptional" (not real number)
			/// float/double values are output as quoted strings.
			/// </summary>
			/// <remarks>
			/// Feature that determines whether "exceptional" (not real number)
			/// float/double values are output as quoted strings.
			/// The values checked are Double.Nan,
			/// Double.POSITIVE_INFINITY and Double.NEGATIVE_INIFINTY (and
			/// associated Float values).
			/// If feature is disabled, these numbers are still output using
			/// associated literal values, resulting in non-conformant
			/// output.
			/// <p>
			/// Feature is enabled by default.
			/// </remarks>
			public static readonly com.fasterxml.jackson.core.JsonGenerator.Feature QUOTE_NON_NUMERIC_NUMBERS
				 = new com.fasterxml.jackson.core.JsonGenerator.Feature(true);

			/// <summary>Feature that forces all Java numbers to be written as JSON strings.</summary>
			/// <remarks>
			/// Feature that forces all Java numbers to be written as JSON strings.
			/// Default state is 'false', meaning that Java numbers are to
			/// be serialized using basic numeric serialization (as JSON
			/// numbers, integral or floating point). If enabled, all such
			/// numeric values are instead written out as JSON Strings.
			/// <p>
			/// One use case is to avoid problems with Javascript limitations:
			/// since Javascript standard specifies that all number handling
			/// should be done using 64-bit IEEE 754 floating point values,
			/// result being that some 64-bit integer values can not be
			/// accurately represent (as mantissa is only 51 bit wide).
			/// <p>
			/// Feature is disabled by default.
			/// </remarks>
			public static readonly com.fasterxml.jackson.core.JsonGenerator.Feature WRITE_NUMBERS_AS_STRINGS
				 = new com.fasterxml.jackson.core.JsonGenerator.Feature(false);

			/// <summary>
			/// Feature that determines whether
			/// <see cref="java.math.BigDecimal"/>
			/// entries are
			/// serialized using
			/// <see cref="java.math.BigDecimal.toPlainString()"/>
			/// to prevent
			/// values to be written using scientific notation.
			/// <p>
			/// Feature is disabled by default, so default output mode is used; this generally
			/// depends on how
			/// <see cref="java.math.BigDecimal"/>
			/// has been created.
			/// </summary>
			/// <since>2.3</since>
			public static readonly com.fasterxml.jackson.core.JsonGenerator.Feature WRITE_BIGDECIMAL_AS_PLAIN
				 = new com.fasterxml.jackson.core.JsonGenerator.Feature(false);

			/// <summary>
			/// Feature that specifies that all characters beyond 7-bit ASCII
			/// range (i.e.
			/// </summary>
			/// <remarks>
			/// Feature that specifies that all characters beyond 7-bit ASCII
			/// range (i.e. code points of 128 and above) need to be output
			/// using format-specific escapes (for JSON, backslash escapes),
			/// if format uses escaping mechanisms (which is generally true
			/// for textual formats but not for binary formats).
			/// <p>
			/// Note that this setting may not necessarily make sense for all
			/// data formats (for example, binary formats typically do not use
			/// any escaping mechanisms; and some textual formats do not have
			/// general-purpose escaping); if so, settings is simply ignored.
			/// Put another way, effects of this feature are data-format specific.
			/// <p>
			/// Feature is disabled by default.
			/// </remarks>
			public static readonly com.fasterxml.jackson.core.JsonGenerator.Feature ESCAPE_NON_ASCII
				 = new com.fasterxml.jackson.core.JsonGenerator.Feature(false);

			/// <summary>
			/// Feature that determines whether
			/// <see cref="JsonGenerator"/>
			/// will explicitly
			/// check that no duplicate JSON Object field names are written.
			/// If enabled, generator will check all names within context and report
			/// duplicates by throwing a
			/// <see cref="JsonGenerationException"/>
			/// ; if disabled,
			/// no such checking will be done. Assumption in latter case is
			/// that caller takes care of not trying to write duplicate names.
			/// <p>
			/// Note that enabling this feature will incur performance overhead
			/// due to having to store and check additional information.
			/// <p>
			/// Feature is disabled by default.
			/// </summary>
			/// <since>2.3</since>
			public static readonly com.fasterxml.jackson.core.JsonGenerator.Feature STRICT_DUPLICATE_DETECTION
				 = new com.fasterxml.jackson.core.JsonGenerator.Feature(false);

			/// <summary>
			/// Feature that determines what to do if the underlying data format requires knowledge
			/// of all properties to output, and if no definition is found for a property that
			/// caller tries to write.
			/// </summary>
			/// <remarks>
			/// Feature that determines what to do if the underlying data format requires knowledge
			/// of all properties to output, and if no definition is found for a property that
			/// caller tries to write. If enabled, such properties will be quietly ignored;
			/// if disabled, a
			/// <see cref="JsonProcessingException"/>
			/// will be thrown to indicate the
			/// problem.
			/// Typically most textual data formats do NOT require schema information (although
			/// some do, such as CSV), whereas many binary data formats do require definitions
			/// (such as Avro, protobuf), although not all (Smile, CBOR, BSON and MessagePack do not).
			/// <p>
			/// Note that support for this feature is implemented by individual data format
			/// module, if (and only if) it makes sense for the format in question. For JSON,
			/// for example, this feature has no effect as properties need not be pre-defined.
			/// <p>
			/// Feature is disabled by default, meaning that if the underlying data format
			/// requires knowledge of all properties to output, attempts to write an unknown
			/// property will result in a
			/// <see cref="JsonProcessingException"/>
			/// </remarks>
			/// <since>2.5</since>
			public static readonly com.fasterxml.jackson.core.JsonGenerator.Feature IGNORE_UNKNOWN
				 = new com.fasterxml.jackson.core.JsonGenerator.Feature(false);

			private readonly bool _defaultState;

			private readonly int _mask;

			// // Low-level I/O / content features
			// // Quoting-related features
			// // Schema/Validity support features
			/// <summary>
			/// Method that calculates bit set (flags) of all features that
			/// are enabled by default.
			/// </summary>
			public static int collectDefaults()
			{
				int flags = 0;
				foreach (com.fasterxml.jackson.core.JsonGenerator.Feature f in values())
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
				com.fasterxml.jackson.core.JsonGenerator.Feature._defaultState = defaultState;
				com.fasterxml.jackson.core.JsonGenerator.Feature._mask = (1 << ordinal());
			}

			public bool enabledByDefault()
			{
				return com.fasterxml.jackson.core.JsonGenerator.Feature._defaultState;
			}

			/// <since>2.3</since>
			public bool enabledIn(int flags)
			{
				return (flags & com.fasterxml.jackson.core.JsonGenerator.Feature._mask) != 0;
			}

			public int getMask()
			{
				return com.fasterxml.jackson.core.JsonGenerator.Feature._mask;
			}
		}

		/// <summary>
		/// Object that handles pretty-printing (usually additional
		/// white space to make results more human-readable) during
		/// output.
		/// </summary>
		/// <remarks>
		/// Object that handles pretty-printing (usually additional
		/// white space to make results more human-readable) during
		/// output. If null, no pretty-printing is done.
		/// </remarks>
		protected internal com.fasterxml.jackson.core.PrettyPrinter _cfgPrettyPrinter;

		protected internal JsonGenerator()
		{
		}

		/*
		/**********************************************************
		/* Configuration
		/**********************************************************
		*/
		/*
		/**********************************************************
		/* Construction, initialization
		/**********************************************************
		*/
		/// <summary>
		/// Method that can be called to set or reset the object to
		/// use for writing Java objects as JsonContent
		/// (using method
		/// <see cref="writeObject(object)"/>
		/// ).
		/// </summary>
		/// <returns>Generator itself (this), to allow chaining</returns>
		public abstract com.fasterxml.jackson.core.JsonGenerator setCodec(com.fasterxml.jackson.core.ObjectCodec
			 oc);

		/// <summary>
		/// Method for accessing the object used for writing Java
		/// object as Json content
		/// (using method
		/// <see cref="writeObject(object)"/>
		/// ).
		/// </summary>
		public abstract com.fasterxml.jackson.core.ObjectCodec getCodec();

		/// <summary>Accessor for finding out version of the bundle that provided this generator instance.
		/// 	</summary>
		public abstract com.fasterxml.jackson.core.Version version();

		/*
		/**********************************************************
		/* Public API, Feature configuration
		/**********************************************************
		*/
		/// <summary>
		/// Method for enabling specified parser features:
		/// check
		/// <see cref="Feature"/>
		/// for list of available features.
		/// </summary>
		/// <returns>Generator itself (this), to allow chaining</returns>
		public abstract com.fasterxml.jackson.core.JsonGenerator enable(com.fasterxml.jackson.core.JsonGenerator.Feature
			 f);

		/// <summary>
		/// Method for disabling specified  features
		/// (check
		/// <see cref="Feature"/>
		/// for list of features)
		/// </summary>
		/// <returns>Generator itself (this), to allow chaining</returns>
		public abstract com.fasterxml.jackson.core.JsonGenerator disable(com.fasterxml.jackson.core.JsonGenerator.Feature
			 f);

		/// <summary>
		/// Method for enabling or disabling specified feature:
		/// check
		/// <see cref="Feature"/>
		/// for list of available features.
		/// </summary>
		/// <returns>Generator itself (this), to allow chaining</returns>
		public com.fasterxml.jackson.core.JsonGenerator configure(com.fasterxml.jackson.core.JsonGenerator.Feature
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

		/// <summary>Method for checking whether given feature is enabled.</summary>
		/// <remarks>
		/// Method for checking whether given feature is enabled.
		/// Check
		/// <see cref="Feature"/>
		/// for list of available features.
		/// </remarks>
		public abstract bool isEnabled(com.fasterxml.jackson.core.JsonGenerator.Feature f
			);

		/// <summary>
		/// Bulk access method for getting state of all standard (non-dataformat-specific)
		/// <see cref="Feature"/>
		/// s.
		/// </summary>
		/// <returns>
		/// Bit mask that defines current states of all standard
		/// <see cref="Feature"/>
		/// s.
		/// </returns>
		/// <since>2.3</since>
		public abstract int getFeatureMask();

		/// <summary>
		/// Bulk set method for (re)setting states of all standard
		/// <see cref="Feature"/>
		/// s
		/// </summary>
		/// <since>2.3</since>
		/// <param name="values">
		/// Bitmask that defines which
		/// <see cref="Feature"/>
		/// s are enabled
		/// and which disabled
		/// </param>
		/// <returns>This parser object, to allow chaining of calls</returns>
		public abstract com.fasterxml.jackson.core.JsonGenerator setFeatureMask(int values
			);

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
		public virtual com.fasterxml.jackson.core.JsonGenerator overrideStdFeatures(int values
			, int mask)
		{
			int oldState = getFeatureMask();
			int newState = (oldState & ~mask) | (values & mask);
			return setFeatureMask(newState);
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
		public virtual com.fasterxml.jackson.core.JsonGenerator overrideFormatFeatures(int
			 values, int mask)
		{
			throw new System.ArgumentException("No FormatFeatures defined for generator of type "
				 + GetType().FullName);
		}

		/*
		int oldState = getFeatureMask();
		int newState = (oldState & ~mask) | (values & mask);
		return setFeatureMask(newState);
		*/
		/*
		/**********************************************************
		/* Public API, Schema configuration
		/**********************************************************
		*/
		/// <summary>Method to call to make this generator use specified schema.</summary>
		/// <remarks>
		/// Method to call to make this generator use specified schema.
		/// Method must be called before generating any content, right after instance
		/// has been created.
		/// Note that not all generators support schemas; and those that do usually only
		/// accept specific types of schemas: ones defined for data format this generator
		/// produces.
		/// <p>
		/// If generator does not support specified schema,
		/// <see cref="System.NotSupportedException"/>
		/// is thrown.
		/// </remarks>
		/// <param name="schema">Schema to use</param>
		/// <exception cref="System.NotSupportedException">if generator does not support schema
		/// 	</exception>
		public virtual void setSchema(com.fasterxml.jackson.core.FormatSchema schema)
		{
			throw new System.NotSupportedException("Generator of type " + GetType().FullName 
				+ " does not support schema of type '" + schema.getSchemaType() + "'");
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

		/*
		/**********************************************************
		/* Public API, other configuration
		/**********************************************************
		*/
		/// <summary>
		/// Method for setting a custom pretty printer, which is usually
		/// used to add indentation for improved human readability.
		/// </summary>
		/// <remarks>
		/// Method for setting a custom pretty printer, which is usually
		/// used to add indentation for improved human readability.
		/// By default, generator does not do pretty printing.
		/// <p>
		/// To use the default pretty printer that comes with core
		/// Jackson distribution, call
		/// <see cref="useDefaultPrettyPrinter()"/>
		/// instead.
		/// </remarks>
		/// <returns>Generator itself (this), to allow chaining</returns>
		public virtual com.fasterxml.jackson.core.JsonGenerator setPrettyPrinter(com.fasterxml.jackson.core.PrettyPrinter
			 pp)
		{
			_cfgPrettyPrinter = pp;
			return this;
		}

		/// <summary>
		/// Accessor for checking whether this generator has a configured
		/// <see cref="PrettyPrinter"/>
		/// ; returns it if so, null if none configured.
		/// </summary>
		/// <since>2.1</since>
		public virtual com.fasterxml.jackson.core.PrettyPrinter getPrettyPrinter()
		{
			return _cfgPrettyPrinter;
		}

		/// <summary>
		/// Convenience method for enabling pretty-printing using
		/// the default pretty printer
		/// (
		/// <see cref="com.fasterxml.jackson.core.util.DefaultPrettyPrinter"/>
		/// ).
		/// </summary>
		/// <returns>Generator itself (this), to allow chaining</returns>
		public abstract com.fasterxml.jackson.core.JsonGenerator useDefaultPrettyPrinter(
			);

		/// <summary>
		/// Method that can be called to request that generator escapes
		/// all character codes above specified code point (if positive value);
		/// or, to not escape any characters except for ones that must be
		/// escaped for the data format (if -1).
		/// </summary>
		/// <remarks>
		/// Method that can be called to request that generator escapes
		/// all character codes above specified code point (if positive value);
		/// or, to not escape any characters except for ones that must be
		/// escaped for the data format (if -1).
		/// To force escaping of all non-ASCII characters, for example,
		/// this method would be called with value of 127.
		/// <p>
		/// Note that generators are NOT required to support setting of value
		/// higher than 127, because there are other ways to affect quoting
		/// (or lack thereof) of character codes between 0 and 127.
		/// Not all generators support concept of escaping, either; if so,
		/// calling this method will have no effect.
		/// <p>
		/// Default implementation does nothing; sub-classes need to redefine
		/// it according to rules of supported data format.
		/// </remarks>
		/// <param name="charCode">
		/// Either -1 to indicate that no additional escaping
		/// is to be done; or highest code point not to escape (meaning higher
		/// ones will be), if positive value.
		/// </param>
		public virtual com.fasterxml.jackson.core.JsonGenerator setHighestNonEscapedChar(
			int charCode)
		{
			return this;
		}

		/// <summary>
		/// Accessor method for testing what is the highest unescaped character
		/// configured for this generator.
		/// </summary>
		/// <remarks>
		/// Accessor method for testing what is the highest unescaped character
		/// configured for this generator. This may be either positive value
		/// (when escaping configuration has been set and is in effect), or
		/// 0 to indicate that no additional escaping is in effect.
		/// Some generators may not support additional escaping: for example,
		/// generators for binary formats that do not use escaping should
		/// simply return 0.
		/// </remarks>
		/// <returns>
		/// Currently active limitation for highest non-escaped character,
		/// if defined; or -1 to indicate no additional escaping is performed.
		/// </returns>
		public virtual int getHighestEscapedChar()
		{
			return 0;
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
			return null;
		}

		/// <summary>
		/// Method for defining custom escapes factory uses for
		/// <see cref="JsonGenerator"/>
		/// s
		/// it creates.
		/// <p>
		/// Default implementation does nothing and simply returns this instance.
		/// </summary>
		public virtual com.fasterxml.jackson.core.JsonGenerator setCharacterEscapes(com.fasterxml.jackson.core.io.CharacterEscapes
			 esc)
		{
			return this;
		}

		/// <summary>
		/// Method that allows overriding String used for separating root-level
		/// JSON values (default is single space character)
		/// <p>
		/// Default implementation throws
		/// <see cref="System.NotSupportedException"/>
		/// .
		/// </summary>
		/// <param name="sep">
		/// Separator to use, if any; null means that no separator is
		/// automatically added
		/// </param>
		/// <since>2.1</since>
		public virtual com.fasterxml.jackson.core.JsonGenerator setRootValueSeparator(com.fasterxml.jackson.core.SerializableString
			 sep)
		{
			throw new System.NotSupportedException();
		}

		/*
		/**********************************************************
		/* Public API, output state access
		/**********************************************************
		*/
		/// <summary>
		/// Method that can be used to get access to object that is used
		/// as target for generated output; this is usually either
		/// <see cref="Sharpen.OutputStream"/>
		/// or
		/// <see cref="System.IO.TextWriter"/>
		/// , depending on what
		/// generator was constructed with.
		/// Note that returned value may be null in some cases; including
		/// case where implementation does not want to exposed raw
		/// source to caller.
		/// In cases where output has been decorated, object returned here
		/// is the decorated version; this allows some level of interaction
		/// between users of generator and decorator object.
		/// <p>
		/// In general use of this accessor should be considered as
		/// "last effort", i.e. only used if no other mechanism is applicable.
		/// </summary>
		public virtual object getOutputTarget()
		{
			return null;
		}

		/// <summary>
		/// Method for verifying amount of content that is buffered by generator
		/// but not yet flushed to the underlying target (stream, writer),
		/// in units (byte, char) that the generator implementation uses for buffering;
		/// or -1 if this information is not available.
		/// </summary>
		/// <remarks>
		/// Method for verifying amount of content that is buffered by generator
		/// but not yet flushed to the underlying target (stream, writer),
		/// in units (byte, char) that the generator implementation uses for buffering;
		/// or -1 if this information is not available.
		/// Unit used is often the same as the unit of underlying target (that is,
		/// `byte` for
		/// <see cref="Sharpen.OutputStream"/>
		/// , `char` for
		/// <see cref="System.IO.TextWriter"/>
		/// ),
		/// but may differ if buffering is done before encoding.
		/// Default JSON-backed implementations do use matching units.
		/// <p>
		/// Note: non-JSON implementations will be retrofitted for 2.6 and beyond;
		/// please report if you see -1 (missing override)
		/// </remarks>
		/// <returns>
		/// Amount of content buffered in internal units, if amount known and
		/// accessible; -1 if not accessible.
		/// </returns>
		/// <since>2.6</since>
		public virtual int getOutputBuffered()
		{
			return -1;
		}

		/// <summary>
		/// Helper method, usually equivalent to:
		/// <code>
		/// getOutputContext().getCurrentValue();
		/// </code>
		/// </summary>
		/// <since>2.5</since>
		public virtual object getCurrentValue()
		{
			com.fasterxml.jackson.core.JsonStreamContext ctxt = getOutputContext();
			return (ctxt == null) ? null : ctxt.getCurrentValue();
		}

		/// <summary>
		/// Helper method, usually equivalent to:
		/// <code>
		/// getOutputContext().setCurrentValue(v);
		/// </code>
		/// </summary>
		/// <since>2.5</since>
		public virtual void setCurrentValue(object v)
		{
			com.fasterxml.jackson.core.JsonStreamContext ctxt = getOutputContext();
			if (ctxt != null)
			{
				ctxt.setCurrentValue(v);
			}
		}

		/*
		/**********************************************************
		/* Public API, capability introspection methods
		/**********************************************************
		*/
		/// <summary>
		/// Method that can be used to verify that given schema can be used with
		/// this generator (using
		/// <see cref="setSchema(FormatSchema)"/>
		/// ).
		/// </summary>
		/// <param name="schema">Schema to check</param>
		/// <returns>True if this generator can use given schema; false if not</returns>
		public virtual bool canUseSchema(com.fasterxml.jackson.core.FormatSchema schema)
		{
			return false;
		}

		/// <summary>
		/// Introspection method that may be called to see if the underlying
		/// data format supports some kind of Object Ids natively (many do not;
		/// for example, JSON doesn't).
		/// </summary>
		/// <remarks>
		/// Introspection method that may be called to see if the underlying
		/// data format supports some kind of Object Ids natively (many do not;
		/// for example, JSON doesn't).
		/// This method <b>must</b> be called prior to calling
		/// <see cref="writeObjectId(object)"/>
		/// or
		/// <see cref="writeObjectRef(object)"/>
		/// .
		/// <p>
		/// Default implementation returns false; overridden by data formats
		/// that do support native Object Ids. Caller is expected to either
		/// use a non-native notation (explicit property or such), or fail,
		/// in case it can not use native object ids.
		/// </remarks>
		/// <since>2.3</since>
		public virtual bool canWriteObjectId()
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
		/// This method <b>must</b> be called prior to calling
		/// <see cref="writeTypeId(object)"/>
		/// .
		/// <p>
		/// Default implementation returns false; overridden by data formats
		/// that do support native Type Ids. Caller is expected to either
		/// use a non-native notation (explicit property or such), or fail,
		/// in case it can not use native type ids.
		/// </remarks>
		/// <since>2.3</since>
		public virtual bool canWriteTypeId()
		{
			return false;
		}

		/// <summary>
		/// Introspection method that may be called to see if the underlying
		/// data format supports "native" binary data; that is, an efficient
		/// output of binary content without encoding.
		/// </summary>
		/// <remarks>
		/// Introspection method that may be called to see if the underlying
		/// data format supports "native" binary data; that is, an efficient
		/// output of binary content without encoding.
		/// <p>
		/// Default implementation returns false; overridden by data formats
		/// that do support native binary content.
		/// </remarks>
		/// <since>2.3</since>
		public virtual bool canWriteBinaryNatively()
		{
			return false;
		}

		/// <summary>
		/// Introspection method to call to check whether it is ok to omit
		/// writing of Object fields or not.
		/// </summary>
		/// <remarks>
		/// Introspection method to call to check whether it is ok to omit
		/// writing of Object fields or not. Most formats do allow omission,
		/// but certain positional formats (such as CSV) require output of
		/// placeholders, even if no real values are to be emitted.
		/// </remarks>
		/// <since>2.3</since>
		public virtual bool canOmitFields()
		{
			return true;
		}

		/*
		/**********************************************************
		/* Public API, write methods, structural
		/**********************************************************
		*/
		/// <summary>
		/// Method for writing starting marker of a Array value
		/// (for JSON this is character '['; plus possible white space decoration
		/// if pretty-printing is enabled).
		/// </summary>
		/// <remarks>
		/// Method for writing starting marker of a Array value
		/// (for JSON this is character '['; plus possible white space decoration
		/// if pretty-printing is enabled).
		/// <p>
		/// Array values can be written in any context where values
		/// are allowed: meaning everywhere except for when
		/// a field name is expected.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public abstract void writeStartArray();

		/// <summary>
		/// Method for writing start marker of an Array value, similar
		/// to
		/// <see cref="writeStartArray()"/>
		/// , but also specifying how many
		/// elements will be written for the array before calling
		/// <see cref="writeEndArray()"/>
		/// .
		/// <p>
		/// Default implementation simply calls
		/// <see cref="writeStartArray()"/>
		/// .
		/// </summary>
		/// <param name="size">
		/// Number of elements this array will have: actual
		/// number of values written (before matching call to
		/// <see cref="writeEndArray()"/>
		/// MUST match; generator MAY verify
		/// this is the case.
		/// </param>
		/// <since>2.4</since>
		/// <exception cref="System.IO.IOException"/>
		public virtual void writeStartArray(int size)
		{
			writeStartArray();
		}

		/// <summary>
		/// Method for writing closing marker of a JSON Array value
		/// (character ']'; plus possible white space decoration
		/// if pretty-printing is enabled).
		/// </summary>
		/// <remarks>
		/// Method for writing closing marker of a JSON Array value
		/// (character ']'; plus possible white space decoration
		/// if pretty-printing is enabled).
		/// <p>
		/// Marker can be written if the innermost structured type
		/// is Array.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public abstract void writeEndArray();

		/// <summary>
		/// Method for writing starting marker of a JSON Object value
		/// (character '{'; plus possible white space decoration
		/// if pretty-printing is enabled).
		/// </summary>
		/// <remarks>
		/// Method for writing starting marker of a JSON Object value
		/// (character '{'; plus possible white space decoration
		/// if pretty-printing is enabled).
		/// <p>
		/// Object values can be written in any context where values
		/// are allowed: meaning everywhere except for when
		/// a field name is expected.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public abstract void writeStartObject();

		/// <summary>
		/// Method for writing closing marker of a JSON Object value
		/// (character '}'; plus possible white space decoration
		/// if pretty-printing is enabled).
		/// </summary>
		/// <remarks>
		/// Method for writing closing marker of a JSON Object value
		/// (character '}'; plus possible white space decoration
		/// if pretty-printing is enabled).
		/// <p>
		/// Marker can be written if the innermost structured type
		/// is Object, and the last written event was either a
		/// complete value, or START-OBJECT marker (see JSON specification
		/// for more details).
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public abstract void writeEndObject();

		/// <summary>
		/// Method for writing a field name (JSON String surrounded by
		/// double quotes: syntactically identical to a JSON String value),
		/// possibly decorated by white space if pretty-printing is enabled.
		/// </summary>
		/// <remarks>
		/// Method for writing a field name (JSON String surrounded by
		/// double quotes: syntactically identical to a JSON String value),
		/// possibly decorated by white space if pretty-printing is enabled.
		/// <p>
		/// Field names can only be written in Object context (check out
		/// JSON specification for details), when field name is expected
		/// (field names alternate with values).
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public abstract void writeFieldName(string name);

		/// <summary>
		/// Method similar to
		/// <see cref="writeFieldName(string)"/>
		/// , main difference
		/// being that it may perform better as some of processing (such as
		/// quoting of certain characters, or encoding into external encoding
		/// if supported by generator) can be done just once and reused for
		/// later calls.
		/// <p>
		/// Default implementation simple uses unprocessed name container in
		/// serialized String; implementations are strongly encouraged to make
		/// use of more efficient methods argument object has.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		public abstract void writeFieldName(com.fasterxml.jackson.core.SerializableString
			 name);

		/*
		/**********************************************************
		/* Public API, write methods, text/String values
		/**********************************************************
		*/
		/// <summary>Method for outputting a String value.</summary>
		/// <remarks>
		/// Method for outputting a String value. Depending on context
		/// this means either array element, (object) field value or
		/// a stand alone String; but in all cases, String will be
		/// surrounded in double quotes, and contents will be properly
		/// escaped as required by JSON specification.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public abstract void writeString(string text);

		/// <summary>Method for outputting a String value.</summary>
		/// <remarks>
		/// Method for outputting a String value. Depending on context
		/// this means either array element, (object) field value or
		/// a stand alone String; but in all cases, String will be
		/// surrounded in double quotes, and contents will be properly
		/// escaped as required by JSON specification.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public abstract void writeString(char[] text, int offset, int len);

		/// <summary>
		/// Method similar to
		/// <see cref="writeString(string)"/>
		/// , but that takes
		/// <see cref="SerializableString"/>
		/// which can make this potentially
		/// more efficient to call as generator may be able to reuse
		/// quoted and/or encoded representation.
		/// <p>
		/// Default implementation just calls
		/// <see cref="writeString(string)"/>
		/// ;
		/// sub-classes should override it with more efficient implementation
		/// if possible.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		public abstract void writeString(com.fasterxml.jackson.core.SerializableString text
			);

		/// <summary>
		/// Method similar to
		/// <see cref="writeString(string)"/>
		/// but that takes as
		/// its input a UTF-8 encoded String that is to be output as-is, without additional
		/// escaping (type of which depends on data format; backslashes for JSON).
		/// However, quoting that data format requires (like double-quotes for JSON) will be added
		/// around the value if and as necessary.
		/// <p>
		/// Note that some backends may choose not to support this method: for
		/// example, if underlying destination is a
		/// <see cref="System.IO.TextWriter"/>
		/// using this method would require UTF-8 decoding.
		/// If so, implementation may instead choose to throw a
		/// <see cref="System.NotSupportedException"/>
		/// due to ineffectiveness
		/// of having to decode input.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		public abstract void writeRawUTF8String(byte[] text, int offset, int length);

		/// <summary>
		/// Method similar to
		/// <see cref="writeString(string)"/>
		/// but that takes as its input
		/// a UTF-8 encoded String which has <b>not</b> been escaped using whatever
		/// escaping scheme data format requires (for JSON that is backslash-escaping
		/// for control characters and double-quotes; for other formats something else).
		/// This means that textual JSON backends need to check if value needs
		/// JSON escaping, but otherwise can just be copied as is to output.
		/// Also, quoting that data format requires (like double-quotes for JSON) will be added
		/// around the value if and as necessary.
		/// <p>
		/// Note that some backends may choose not to support this method: for
		/// example, if underlying destination is a
		/// <see cref="System.IO.TextWriter"/>
		/// using this method would require UTF-8 decoding.
		/// In this case
		/// generator implementation may instead choose to throw a
		/// <see cref="System.NotSupportedException"/>
		/// due to ineffectiveness
		/// of having to decode input.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		public abstract void writeUTF8String(byte[] text, int offset, int length);

		/*
		/**********************************************************
		/* Public API, write methods, binary/raw content
		/**********************************************************
		*/
		/// <summary>
		/// Method that will force generator to copy
		/// input text verbatim with <b>no</b> modifications (including
		/// that no escaping is done and no separators are added even
		/// if context [array, object] would otherwise require such).
		/// </summary>
		/// <remarks>
		/// Method that will force generator to copy
		/// input text verbatim with <b>no</b> modifications (including
		/// that no escaping is done and no separators are added even
		/// if context [array, object] would otherwise require such).
		/// If such separators are desired, use
		/// <see cref="writeRawValue(string)"/>
		/// instead.
		/// <p>
		/// Note that not all generator implementations necessarily support
		/// such by-pass methods: those that do not will throw
		/// <see cref="System.NotSupportedException"/>
		/// .
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public abstract void writeRaw(string text);

		/// <summary>
		/// Method that will force generator to copy
		/// input text verbatim with <b>no</b> modifications (including
		/// that no escaping is done and no separators are added even
		/// if context [array, object] would otherwise require such).
		/// </summary>
		/// <remarks>
		/// Method that will force generator to copy
		/// input text verbatim with <b>no</b> modifications (including
		/// that no escaping is done and no separators are added even
		/// if context [array, object] would otherwise require such).
		/// If such separators are desired, use
		/// <see cref="writeRawValue(string)"/>
		/// instead.
		/// <p>
		/// Note that not all generator implementations necessarily support
		/// such by-pass methods: those that do not will throw
		/// <see cref="System.NotSupportedException"/>
		/// .
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public abstract void writeRaw(string text, int offset, int len);

		/// <summary>
		/// Method that will force generator to copy
		/// input text verbatim with <b>no</b> modifications (including
		/// that no escaping is done and no separators are added even
		/// if context [array, object] would otherwise require such).
		/// </summary>
		/// <remarks>
		/// Method that will force generator to copy
		/// input text verbatim with <b>no</b> modifications (including
		/// that no escaping is done and no separators are added even
		/// if context [array, object] would otherwise require such).
		/// If such separators are desired, use
		/// <see cref="writeRawValue(string)"/>
		/// instead.
		/// <p>
		/// Note that not all generator implementations necessarily support
		/// such by-pass methods: those that do not will throw
		/// <see cref="System.NotSupportedException"/>
		/// .
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public abstract void writeRaw(char[] text, int offset, int len);

		/// <summary>
		/// Method that will force generator to copy
		/// input text verbatim with <b>no</b> modifications (including
		/// that no escaping is done and no separators are added even
		/// if context [array, object] would otherwise require such).
		/// </summary>
		/// <remarks>
		/// Method that will force generator to copy
		/// input text verbatim with <b>no</b> modifications (including
		/// that no escaping is done and no separators are added even
		/// if context [array, object] would otherwise require such).
		/// If such separators are desired, use
		/// <see cref="writeRawValue(string)"/>
		/// instead.
		/// <p>
		/// Note that not all generator implementations necessarily support
		/// such by-pass methods: those that do not will throw
		/// <see cref="System.NotSupportedException"/>
		/// .
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public abstract void writeRaw(char c);

		/// <summary>
		/// Method that will force generator to copy
		/// input text verbatim with <b>no</b> modifications (including
		/// that no escaping is done and no separators are added even
		/// if context [array, object] would otherwise require such).
		/// </summary>
		/// <remarks>
		/// Method that will force generator to copy
		/// input text verbatim with <b>no</b> modifications (including
		/// that no escaping is done and no separators are added even
		/// if context [array, object] would otherwise require such).
		/// If such separators are desired, use
		/// <see cref="writeRawValue(string)"/>
		/// instead.
		/// <p>
		/// Note that not all generator implementations necessarily support
		/// such by-pass methods: those that do not will throw
		/// <see cref="System.NotSupportedException"/>
		/// .
		/// <p>
		/// The default implementation delegates to
		/// <see cref="writeRaw(string)"/>
		/// ;
		/// other backends that support raw inclusion of text are encouraged
		/// to implement it in more efficient manner (especially if they
		/// use UTF-8 encoding).
		/// </remarks>
		/// <since>2.1</since>
		/// <exception cref="System.IO.IOException"/>
		public virtual void writeRaw(com.fasterxml.jackson.core.SerializableString raw)
		{
			writeRaw(raw.getValue());
		}

		/// <summary>
		/// Method that will force generator to copy
		/// input text verbatim without any modifications, but assuming
		/// it must constitute a single legal JSON value (number, string,
		/// boolean, null, Array or List).
		/// </summary>
		/// <remarks>
		/// Method that will force generator to copy
		/// input text verbatim without any modifications, but assuming
		/// it must constitute a single legal JSON value (number, string,
		/// boolean, null, Array or List). Assuming this, proper separators
		/// are added if and as needed (comma or colon), and generator
		/// state updated to reflect this.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public abstract void writeRawValue(string text);

		/// <exception cref="System.IO.IOException"/>
		public abstract void writeRawValue(string text, int offset, int len);

		/// <exception cref="System.IO.IOException"/>
		public abstract void writeRawValue(char[] text, int offset, int len);

		/// <summary>
		/// Method similar to
		/// <see cref="writeRawValue(string)"/>
		/// , but potentially more
		/// efficient as it may be able to use pre-encoded content (similar to
		/// <see cref="writeRaw(SerializableString)"/>
		/// .
		/// </summary>
		/// <since>2.5</since>
		/// <exception cref="System.IO.IOException"/>
		public virtual void writeRawValue(com.fasterxml.jackson.core.SerializableString raw
			)
		{
			writeRawValue(raw.getValue());
		}

		/// <summary>
		/// Method that will output given chunk of binary data as base64
		/// encoded, as a complete String value (surrounded by double quotes).
		/// </summary>
		/// <remarks>
		/// Method that will output given chunk of binary data as base64
		/// encoded, as a complete String value (surrounded by double quotes).
		/// This method defaults
		/// <p>
		/// Note: because Json Strings can not contain unescaped linefeeds,
		/// if linefeeds are included (as per last argument), they must be
		/// escaped. This adds overhead for decoding without improving
		/// readability.
		/// Alternatively if linefeeds are not included,
		/// resulting String value may violate the requirement of base64
		/// RFC which mandates line-length of 76 characters and use of
		/// linefeeds. However, all
		/// <see cref="JsonParser"/>
		/// implementations
		/// are required to accept such "long line base64"; as do
		/// typical production-level base64 decoders.
		/// </remarks>
		/// <param name="bv">
		/// Base64 variant to use: defines details such as
		/// whether padding is used (and if so, using which character);
		/// what is the maximum line length before adding linefeed,
		/// and also the underlying alphabet to use.
		/// </param>
		/// <exception cref="System.IO.IOException"/>
		public abstract void writeBinary(com.fasterxml.jackson.core.Base64Variant bv, byte
			[] data, int offset, int len);

		/// <summary>
		/// Similar to
		/// <see cref="writeBinary(Base64Variant, byte[], int, int)"/>
		/// ,
		/// but default to using the Jackson default Base64 variant
		/// (which is
		/// <see cref="Base64Variants.MIME_NO_LINEFEEDS"/>
		/// ).
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		public virtual void writeBinary(byte[] data, int offset, int len)
		{
			writeBinary(com.fasterxml.jackson.core.Base64Variants.getDefaultVariant(), data, 
				offset, len);
		}

		/// <summary>
		/// Similar to
		/// <see cref="writeBinary(Base64Variant, byte[], int, int)"/>
		/// ,
		/// but assumes default to using the Jackson default Base64 variant
		/// (which is
		/// <see cref="Base64Variants.MIME_NO_LINEFEEDS"/>
		/// ). Also
		/// assumes that whole byte array is to be output.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		public virtual void writeBinary(byte[] data)
		{
			writeBinary(com.fasterxml.jackson.core.Base64Variants.getDefaultVariant(), data, 
				0, data.Length);
		}

		/// <summary>
		/// Similar to
		/// <see cref="writeBinary(Base64Variant, Sharpen.InputStream, int)"/>
		/// ,
		/// but assumes default to using the Jackson default Base64 variant
		/// (which is
		/// <see cref="Base64Variants.MIME_NO_LINEFEEDS"/>
		/// ).
		/// </summary>
		/// <param name="data">
		/// InputStream to use for reading binary data to write.
		/// Will not be closed after successful write operation
		/// </param>
		/// <param name="dataLength">
		/// (optional) number of bytes that will be available;
		/// or -1 to be indicate it is not known. Note that implementations
		/// need not support cases where length is not known in advance; this
		/// depends on underlying data format: JSON output does NOT require length,
		/// other formats may
		/// </param>
		/// <exception cref="System.IO.IOException"/>
		public virtual int writeBinary(Sharpen.InputStream data, int dataLength)
		{
			return writeBinary(com.fasterxml.jackson.core.Base64Variants.getDefaultVariant(), 
				data, dataLength);
		}

		/// <summary>
		/// Method similar to
		/// <see cref="writeBinary(Base64Variant, byte[], int, int)"/>
		/// ,
		/// but where input is provided through a stream, allowing for incremental
		/// writes without holding the whole input in memory.
		/// </summary>
		/// <param name="bv">Base64 variant to use</param>
		/// <param name="data">
		/// InputStream to use for reading binary data to write.
		/// Will not be closed after successful write operation
		/// </param>
		/// <param name="dataLength">
		/// (optional) number of bytes that will be available;
		/// or -1 to be indicate it is not known.
		/// If a positive length is given, <code>data</code> MUST provide at least
		/// that many bytes: if not, an exception will be thrown.
		/// Note that implementations
		/// need not support cases where length is not known in advance; this
		/// depends on underlying data format: JSON output does NOT require length,
		/// other formats may.
		/// </param>
		/// <returns>Number of bytes read from <code>data</code> and written as binary payload
		/// 	</returns>
		/// <since>2.1</since>
		/// <exception cref="System.IO.IOException"/>
		public abstract int writeBinary(com.fasterxml.jackson.core.Base64Variant bv, Sharpen.InputStream
			 data, int dataLength);

		/*
		/**********************************************************
		/* Public API, write methods, other value types
		/**********************************************************
		*/
		/// <summary>Method for outputting given value as JSON number.</summary>
		/// <remarks>
		/// Method for outputting given value as JSON number.
		/// Can be called in any context where a value is expected
		/// (Array value, Object field value, root-level value).
		/// Additional white space may be added around the value
		/// if pretty-printing is enabled.
		/// </remarks>
		/// <param name="v">Number value to write</param>
		/// <since>2.2</since>
		/// <exception cref="System.IO.IOException"/>
		public virtual void writeNumber(short v)
		{
			writeNumber((int)v);
		}

		/// <summary>Method for outputting given value as JSON number.</summary>
		/// <remarks>
		/// Method for outputting given value as JSON number.
		/// Can be called in any context where a value is expected
		/// (Array value, Object field value, root-level value).
		/// Additional white space may be added around the value
		/// if pretty-printing is enabled.
		/// </remarks>
		/// <param name="v">Number value to write</param>
		/// <exception cref="System.IO.IOException"/>
		public abstract void writeNumber(int v);

		/// <summary>Method for outputting given value as JSON number.</summary>
		/// <remarks>
		/// Method for outputting given value as JSON number.
		/// Can be called in any context where a value is expected
		/// (Array value, Object field value, root-level value).
		/// Additional white space may be added around the value
		/// if pretty-printing is enabled.
		/// </remarks>
		/// <param name="v">Number value to write</param>
		/// <exception cref="System.IO.IOException"/>
		public abstract void writeNumber(long v);

		/// <summary>Method for outputting given value as JSON number.</summary>
		/// <remarks>
		/// Method for outputting given value as JSON number.
		/// Can be called in any context where a value is expected
		/// (Array value, Object field value, root-level value).
		/// Additional white space may be added around the value
		/// if pretty-printing is enabled.
		/// </remarks>
		/// <param name="v">Number value to write</param>
		/// <exception cref="System.IO.IOException"/>
		public abstract void writeNumber(System.Numerics.BigInteger v);

		/// <summary>Method for outputting indicate JSON numeric value.</summary>
		/// <remarks>
		/// Method for outputting indicate JSON numeric value.
		/// Can be called in any context where a value is expected
		/// (Array value, Object field value, root-level value).
		/// Additional white space may be added around the value
		/// if pretty-printing is enabled.
		/// </remarks>
		/// <param name="v">Number value to write</param>
		/// <exception cref="System.IO.IOException"/>
		public abstract void writeNumber(double v);

		/// <summary>Method for outputting indicate JSON numeric value.</summary>
		/// <remarks>
		/// Method for outputting indicate JSON numeric value.
		/// Can be called in any context where a value is expected
		/// (Array value, Object field value, root-level value).
		/// Additional white space may be added around the value
		/// if pretty-printing is enabled.
		/// </remarks>
		/// <param name="v">Number value to write</param>
		/// <exception cref="System.IO.IOException"/>
		public abstract void writeNumber(float v);

		/// <summary>Method for outputting indicate JSON numeric value.</summary>
		/// <remarks>
		/// Method for outputting indicate JSON numeric value.
		/// Can be called in any context where a value is expected
		/// (Array value, Object field value, root-level value).
		/// Additional white space may be added around the value
		/// if pretty-printing is enabled.
		/// </remarks>
		/// <param name="v">Number value to write</param>
		/// <exception cref="System.IO.IOException"/>
		public abstract void writeNumber(java.math.BigDecimal v);

		/// <summary>
		/// Write method that can be used for custom numeric types that can
		/// not be (easily?) converted to "standard" Java number types.
		/// </summary>
		/// <remarks>
		/// Write method that can be used for custom numeric types that can
		/// not be (easily?) converted to "standard" Java number types.
		/// Because numbers are not surrounded by double quotes, regular
		/// <see cref="writeString(string)"/>
		/// method can not be used; nor
		/// <see cref="writeRaw(string)"/>
		/// because that does not properly handle
		/// value separators needed in Array or Object contexts.
		/// <p>
		/// Note: because of lack of type safety, some generator
		/// implementations may not be able to implement this
		/// method. For example, if a binary JSON format is used,
		/// it may require type information for encoding; similarly
		/// for generator-wrappers around Java objects or JSON nodes.
		/// If implementation does not implement this method,
		/// it needs to throw
		/// <see cref="System.NotSupportedException"/>
		/// .
		/// </remarks>
		/// <exception cref="System.NotSupportedException">
		/// If underlying data format does not
		/// support numbers serialized textually AND if generator is not allowed
		/// to just output a String instead (Schema-based formats may require actual
		/// number, for example)
		/// </exception>
		/// <exception cref="System.IO.IOException"/>
		public abstract void writeNumber(string encodedValue);

		/// <summary>
		/// Method for outputting literal Json boolean value (one of
		/// Strings 'true' and 'false').
		/// </summary>
		/// <remarks>
		/// Method for outputting literal Json boolean value (one of
		/// Strings 'true' and 'false').
		/// Can be called in any context where a value is expected
		/// (Array value, Object field value, root-level value).
		/// Additional white space may be added around the value
		/// if pretty-printing is enabled.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public abstract void writeBoolean(bool state);

		/// <summary>Method for outputting literal Json null value.</summary>
		/// <remarks>
		/// Method for outputting literal Json null value.
		/// Can be called in any context where a value is expected
		/// (Array value, Object field value, root-level value).
		/// Additional white space may be added around the value
		/// if pretty-printing is enabled.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public abstract void writeNull();

		/*
		/**********************************************************
		/* Public API, write methods, Native Ids (type, object)
		/**********************************************************
		*/
		/// <summary>Method that can be called to output so-called native Object Id.</summary>
		/// <remarks>
		/// Method that can be called to output so-called native Object Id.
		/// Note that it may only be called after ensuring this is legal
		/// (with
		/// <see cref="canWriteObjectId()"/>
		/// ), as not all data formats
		/// have native type id support; and some may only allow them in
		/// certain positions or locations.
		/// If output is not allowed by the data format in this position,
		/// a
		/// <see cref="JsonGenerationException"/>
		/// will be thrown.
		/// </remarks>
		/// <since>2.3</since>
		/// <exception cref="System.IO.IOException"/>
		public virtual void writeObjectId(object id)
		{
			throw new com.fasterxml.jackson.core.JsonGenerationException("No native support for writing Object Ids"
				);
		}

		/// <summary>Method that can be called to output references to native Object Ids.</summary>
		/// <remarks>
		/// Method that can be called to output references to native Object Ids.
		/// Note that it may only be called after ensuring this is legal
		/// (with
		/// <see cref="canWriteObjectId()"/>
		/// ), as not all data formats
		/// have native type id support; and some may only allow them in
		/// certain positions or locations.
		/// If output is not allowed by the data format in this position,
		/// a
		/// <see cref="JsonGenerationException"/>
		/// will be thrown.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public virtual void writeObjectRef(object id)
		{
			throw new com.fasterxml.jackson.core.JsonGenerationException("No native support for writing Object Ids"
				);
		}

		/// <summary>Method that can be called to output so-called native Type Id.</summary>
		/// <remarks>
		/// Method that can be called to output so-called native Type Id.
		/// Note that it may only be called after ensuring this is legal
		/// (with
		/// <see cref="canWriteTypeId()"/>
		/// ), as not all data formats
		/// have native type id support; and some may only allow them in
		/// certain positions or locations.
		/// If output is not allowed by the data format in this position,
		/// a
		/// <see cref="JsonGenerationException"/>
		/// will be thrown.
		/// </remarks>
		/// <since>2.3</since>
		/// <exception cref="System.IO.IOException"/>
		public virtual void writeTypeId(object id)
		{
			throw new com.fasterxml.jackson.core.JsonGenerationException("No native support for writing Type Ids"
				);
		}

		/*
		/**********************************************************
		/* Public API, write methods, serializing Java objects
		/**********************************************************
		*/
		/// <summary>Method for writing given Java object (POJO) as Json.</summary>
		/// <remarks>
		/// Method for writing given Java object (POJO) as Json.
		/// Exactly how the object gets written depends on object
		/// in question (ad on codec, its configuration); for most
		/// beans it will result in Json object, but for others Json
		/// array, or String or numeric value (and for nulls, Json
		/// null literal.
		/// <b>NOTE</b>: generator must have its <b>object codec</b>
		/// set to non-null value; for generators created by a mapping
		/// factory this is the case, for others not.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public abstract void writeObject(object pojo);

		/// <summary>
		/// Method for writing given JSON tree (expressed as a tree
		/// where given JsonNode is the root) using this generator.
		/// </summary>
		/// <remarks>
		/// Method for writing given JSON tree (expressed as a tree
		/// where given JsonNode is the root) using this generator.
		/// This will generally just call
		/// <see cref="writeObject(object)"/>
		/// with given node, but is added
		/// for convenience and to make code more explicit in cases
		/// where it deals specifically with trees.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public abstract void writeTree(com.fasterxml.jackson.core.TreeNode rootNode);

		/*
		/**********************************************************
		/* Public API, convenience field write methods
		/**********************************************************
		*/
		/// <summary>
		/// Convenience method for outputting a field entry ("member")
		/// that has a String value.
		/// </summary>
		/// <remarks>
		/// Convenience method for outputting a field entry ("member")
		/// that has a String value. Equivalent to:
		/// <pre>
		/// writeFieldName(fieldName);
		/// writeString(value);
		/// </pre>
		/// <p>
		/// Note: many performance-sensitive implementations override this method
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public virtual void writeStringField(string fieldName, string value)
		{
			writeFieldName(fieldName);
			writeString(value);
		}

		/// <summary>
		/// Convenience method for outputting a field entry ("member")
		/// that has a boolean value.
		/// </summary>
		/// <remarks>
		/// Convenience method for outputting a field entry ("member")
		/// that has a boolean value. Equivalent to:
		/// <pre>
		/// writeFieldName(fieldName);
		/// writeBoolean(value);
		/// </pre>
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public void writeBooleanField(string fieldName, bool value)
		{
			writeFieldName(fieldName);
			writeBoolean(value);
		}

		/// <summary>
		/// Convenience method for outputting a field entry ("member")
		/// that has JSON literal value null.
		/// </summary>
		/// <remarks>
		/// Convenience method for outputting a field entry ("member")
		/// that has JSON literal value null. Equivalent to:
		/// <pre>
		/// writeFieldName(fieldName);
		/// writeNull();
		/// </pre>
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public void writeNullField(string fieldName)
		{
			writeFieldName(fieldName);
			writeNull();
		}

		/// <summary>
		/// Convenience method for outputting a field entry ("member")
		/// that has the specified numeric value.
		/// </summary>
		/// <remarks>
		/// Convenience method for outputting a field entry ("member")
		/// that has the specified numeric value. Equivalent to:
		/// <pre>
		/// writeFieldName(fieldName);
		/// writeNumber(value);
		/// </pre>
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public void writeNumberField(string fieldName, int value)
		{
			writeFieldName(fieldName);
			writeNumber(value);
		}

		/// <summary>
		/// Convenience method for outputting a field entry ("member")
		/// that has the specified numeric value.
		/// </summary>
		/// <remarks>
		/// Convenience method for outputting a field entry ("member")
		/// that has the specified numeric value. Equivalent to:
		/// <pre>
		/// writeFieldName(fieldName);
		/// writeNumber(value);
		/// </pre>
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public void writeNumberField(string fieldName, long value)
		{
			writeFieldName(fieldName);
			writeNumber(value);
		}

		/// <summary>
		/// Convenience method for outputting a field entry ("member")
		/// that has the specified numeric value.
		/// </summary>
		/// <remarks>
		/// Convenience method for outputting a field entry ("member")
		/// that has the specified numeric value. Equivalent to:
		/// <pre>
		/// writeFieldName(fieldName);
		/// writeNumber(value);
		/// </pre>
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public void writeNumberField(string fieldName, double value)
		{
			writeFieldName(fieldName);
			writeNumber(value);
		}

		/// <summary>
		/// Convenience method for outputting a field entry ("member")
		/// that has the specified numeric value.
		/// </summary>
		/// <remarks>
		/// Convenience method for outputting a field entry ("member")
		/// that has the specified numeric value. Equivalent to:
		/// <pre>
		/// writeFieldName(fieldName);
		/// writeNumber(value);
		/// </pre>
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public void writeNumberField(string fieldName, float value)
		{
			writeFieldName(fieldName);
			writeNumber(value);
		}

		/// <summary>
		/// Convenience method for outputting a field entry ("member")
		/// that has the specified numeric value.
		/// </summary>
		/// <remarks>
		/// Convenience method for outputting a field entry ("member")
		/// that has the specified numeric value.
		/// Equivalent to:
		/// <pre>
		/// writeFieldName(fieldName);
		/// writeNumber(value);
		/// </pre>
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public void writeNumberField(string fieldName, java.math.BigDecimal value)
		{
			writeFieldName(fieldName);
			writeNumber(value);
		}

		/// <summary>
		/// Convenience method for outputting a field entry ("member")
		/// that contains specified data in base64-encoded form.
		/// </summary>
		/// <remarks>
		/// Convenience method for outputting a field entry ("member")
		/// that contains specified data in base64-encoded form.
		/// Equivalent to:
		/// <pre>
		/// writeFieldName(fieldName);
		/// writeBinary(value);
		/// </pre>
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public void writeBinaryField(string fieldName, byte[] data)
		{
			writeFieldName(fieldName);
			writeBinary(data);
		}

		/// <summary>
		/// Convenience method for outputting a field entry ("member")
		/// (that will contain a JSON Array value), and the START_ARRAY marker.
		/// </summary>
		/// <remarks>
		/// Convenience method for outputting a field entry ("member")
		/// (that will contain a JSON Array value), and the START_ARRAY marker.
		/// Equivalent to:
		/// <pre>
		/// writeFieldName(fieldName);
		/// writeStartArray();
		/// </pre>
		/// <p>
		/// Note: caller still has to take care to close the array
		/// (by calling {#link #writeEndArray}) after writing all values
		/// of the value Array.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public void writeArrayFieldStart(string fieldName)
		{
			writeFieldName(fieldName);
			writeStartArray();
		}

		/// <summary>
		/// Convenience method for outputting a field entry ("member")
		/// (that will contain a JSON Object value), and the START_OBJECT marker.
		/// </summary>
		/// <remarks>
		/// Convenience method for outputting a field entry ("member")
		/// (that will contain a JSON Object value), and the START_OBJECT marker.
		/// Equivalent to:
		/// <pre>
		/// writeFieldName(fieldName);
		/// writeStartObject();
		/// </pre>
		/// <p>
		/// Note: caller still has to take care to close the Object
		/// (by calling {#link #writeEndObject}) after writing all
		/// entries of the value Object.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public void writeObjectFieldStart(string fieldName)
		{
			writeFieldName(fieldName);
			writeStartObject();
		}

		/// <summary>
		/// Convenience method for outputting a field entry ("member")
		/// that has contents of specific Java object as its value.
		/// </summary>
		/// <remarks>
		/// Convenience method for outputting a field entry ("member")
		/// that has contents of specific Java object as its value.
		/// Equivalent to:
		/// <pre>
		/// writeFieldName(fieldName);
		/// writeObject(pojo);
		/// </pre>
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public void writeObjectField(string fieldName, object pojo)
		{
			writeFieldName(fieldName);
			writeObject(pojo);
		}

		/// <summary>
		/// Method called to indicate that a property in this position was
		/// skipped.
		/// </summary>
		/// <remarks>
		/// Method called to indicate that a property in this position was
		/// skipped. It is usually only called for generators that return
		/// <code>false</code> from
		/// <see cref="canOmitFields()"/>
		/// .
		/// <p>
		/// Default implementation does nothing.
		/// </remarks>
		/// <since>2.3</since>
		/// <exception cref="System.IO.IOException"/>
		public virtual void writeOmittedField(string fieldName)
		{
		}

		/*
		/**********************************************************
		/* Public API, copy-through methods
		/**********************************************************
		*/
		/// <summary>
		/// Method for copying contents of the current event that
		/// the given parser instance points to.
		/// </summary>
		/// <remarks>
		/// Method for copying contents of the current event that
		/// the given parser instance points to.
		/// Note that the method <b>will not</b> copy any other events,
		/// such as events contained within Json Array or Object structures.
		/// <p>
		/// Calling this method will not advance the given
		/// parser, although it may cause parser to internally process
		/// more data (if it lazy loads contents of value events, for example)
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public virtual void copyCurrentEvent(com.fasterxml.jackson.core.JsonParser jp)
		{
			com.fasterxml.jackson.core.JsonToken t = jp.getCurrentToken();
			// sanity check; what to do?
			if (t == null)
			{
				_reportError("No current event to copy");
			}
			switch (t.id())
			{
				case JsonTokenIdConstants.ID_NOT_AVAILABLE:
				{
					_reportError("No current event to copy");
					goto case JsonTokenIdConstants.ID_START_OBJECT;
				}

				case JsonTokenIdConstants.ID_START_OBJECT:
				{
					writeStartObject();
					break;
				}

				case JsonTokenIdConstants.ID_END_OBJECT:
				{
					writeEndObject();
					break;
				}

				case JsonTokenIdConstants.ID_START_ARRAY:
				{
					writeStartArray();
					break;
				}

				case JsonTokenIdConstants.ID_END_ARRAY:
				{
					writeEndArray();
					break;
				}

				case JsonTokenIdConstants.ID_FIELD_NAME:
				{
					writeFieldName(jp.getCurrentName());
					break;
				}

				case JsonTokenIdConstants.ID_STRING:
				{
					if (jp.hasTextCharacters())
					{
						writeString(jp.getTextCharacters(), jp.getTextOffset(), jp.getTextLength());
					}
					else
					{
						writeString(jp.getText());
					}
					break;
				}

				case JsonTokenIdConstants.ID_NUMBER_INT:
				{
					com.fasterxml.jackson.core.JsonParser.NumberType n = jp.getNumberType();
					if (n == com.fasterxml.jackson.core.JsonParser.NumberType.INT)
					{
						writeNumber(jp.getIntValue());
					}
					else
					{
						if (n == com.fasterxml.jackson.core.JsonParser.NumberType.BIG_INTEGER)
						{
							writeNumber(jp.getBigIntegerValue());
						}
						else
						{
							writeNumber(jp.getLongValue());
						}
					}
					break;
				}

				case JsonTokenIdConstants.ID_NUMBER_FLOAT:
				{
					com.fasterxml.jackson.core.JsonParser.NumberType n = jp.getNumberType();
					if (n == com.fasterxml.jackson.core.JsonParser.NumberType.BIG_DECIMAL)
					{
						writeNumber(jp.getDecimalValue());
					}
					else
					{
						if (n == com.fasterxml.jackson.core.JsonParser.NumberType.FLOAT)
						{
							writeNumber(jp.getFloatValue());
						}
						else
						{
							writeNumber(jp.getDoubleValue());
						}
					}
					break;
				}

				case JsonTokenIdConstants.ID_TRUE:
				{
					writeBoolean(true);
					break;
				}

				case JsonTokenIdConstants.ID_FALSE:
				{
					writeBoolean(false);
					break;
				}

				case JsonTokenIdConstants.ID_NULL:
				{
					writeNull();
					break;
				}

				case JsonTokenIdConstants.ID_EMBEDDED_OBJECT:
				{
					writeObject(jp.getEmbeddedObject());
					break;
				}

				default:
				{
					_throwInternal();
					break;
				}
			}
		}

		/// <summary>
		/// Method for copying contents of the current event
		/// <b>and following events that it encloses</b>
		/// the given parser instance points to.
		/// </summary>
		/// <remarks>
		/// Method for copying contents of the current event
		/// <b>and following events that it encloses</b>
		/// the given parser instance points to.
		/// <p>
		/// So what constitutes enclosing? Here is the list of
		/// events that have associated enclosed events that will
		/// get copied:
		/// <ul>
		/// <li>
		/// <see cref="JsonToken.START_OBJECT"/>
		/// :
		/// all events up to and including matching (closing)
		/// <see cref="JsonToken.END_OBJECT"/>
		/// will be copied
		/// </li>
		/// <li>
		/// <see cref="JsonToken.START_ARRAY"/>
		/// all events up to and including matching (closing)
		/// <see cref="JsonToken.END_ARRAY"/>
		/// will be copied
		/// </li>
		/// <li>
		/// <see cref="JsonToken.FIELD_NAME"/>
		/// the logical value (which
		/// can consist of a single scalar value; or a sequence of related
		/// events for structured types (Json Arrays, Objects)) will
		/// be copied along with the name itself. So essentially the
		/// whole <b>field entry</b> (name and value) will be copied.
		/// </li>
		/// </ul>
		/// <p>
		/// After calling this method, parser will point to the
		/// <b>last event</b> that was copied. This will either be
		/// the event parser already pointed to (if there were no
		/// enclosed events), or the last enclosed event copied.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public virtual void copyCurrentStructure(com.fasterxml.jackson.core.JsonParser jp
			)
		{
			com.fasterxml.jackson.core.JsonToken t = jp.getCurrentToken();
			if (t == null)
			{
				_reportError("No current event to copy");
			}
			// Let's handle field-name separately first
			int id = t.id();
			if (id == JsonTokenIdConstants.ID_FIELD_NAME)
			{
				writeFieldName(jp.getCurrentName());
				t = jp.nextToken();
				id = t.id();
			}
			switch (id)
			{
				case JsonTokenIdConstants.ID_START_OBJECT:
				{
					// fall-through to copy the associated value
					writeStartObject();
					while (jp.nextToken() != com.fasterxml.jackson.core.JsonToken.END_OBJECT)
					{
						copyCurrentStructure(jp);
					}
					writeEndObject();
					break;
				}

				case JsonTokenIdConstants.ID_START_ARRAY:
				{
					writeStartArray();
					while (jp.nextToken() != com.fasterxml.jackson.core.JsonToken.END_ARRAY)
					{
						copyCurrentStructure(jp);
					}
					writeEndArray();
					break;
				}

				default:
				{
					copyCurrentEvent(jp);
					break;
				}
			}
		}

		/*
		/**********************************************************
		/* Public API, context access
		/**********************************************************
		*/
		/// <returns>
		/// Context object that can give information about logical
		/// position within generated json content.
		/// </returns>
		public abstract com.fasterxml.jackson.core.JsonStreamContext getOutputContext();

		/*
		/**********************************************************
		/* Public API, buffer handling
		/**********************************************************
		*/
		/// <summary>
		/// Method called to flush any buffered content to the underlying
		/// target (output stream, writer), and to flush the target itself
		/// as well.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		public abstract void flush();

		/// <summary>
		/// Method that can be called to determine whether this generator
		/// is closed or not.
		/// </summary>
		/// <remarks>
		/// Method that can be called to determine whether this generator
		/// is closed or not. If it is closed, no more output can be done.
		/// </remarks>
		public abstract bool isClosed();

		/*
		/**********************************************************
		/* Closeable implementation
		/**********************************************************
		*/
		/// <summary>
		/// Method called to close this generator, so that no more content
		/// can be written.
		/// </summary>
		/// <remarks>
		/// Method called to close this generator, so that no more content
		/// can be written.
		/// <p>
		/// Whether the underlying target (stream, writer) gets closed depends
		/// on whether this generator either manages the target (i.e. is the
		/// only one with access to the target -- case if caller passes a
		/// reference to the resource such as File, but not stream); or
		/// has feature
		/// <see cref="Feature.AUTO_CLOSE_TARGET"/>
		/// enabled.
		/// If either of above is true, the target is also closed. Otherwise
		/// (not managing, feature not enabled), target is not closed.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public abstract void close();

		/*
		/**********************************************************
		/* Helper methods for sub-classes
		/**********************************************************
		*/
		/// <summary>
		/// Helper method used for constructing and throwing
		/// <see cref="JsonGenerationException"/>
		/// with given base message.
		/// <p>
		/// Note that sub-classes may override this method to add more detail
		/// or use a
		/// <see cref="JsonGenerationException"/>
		/// sub-class.
		/// </summary>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		protected internal virtual void _reportError(string msg)
		{
			throw new com.fasterxml.jackson.core.JsonGenerationException(msg);
		}

		protected internal void _throwInternal()
		{
			com.fasterxml.jackson.core.util.VersionUtil.throwInternal();
		}

		protected internal virtual void _reportUnsupportedOperation()
		{
			throw new System.NotSupportedException("Operation not supported by generator of type "
				 + GetType().FullName);
		}

		/// <summary>
		/// Helper method to try to call appropriate write method for given
		/// untyped Object.
		/// </summary>
		/// <remarks>
		/// Helper method to try to call appropriate write method for given
		/// untyped Object. At this point, no structural conversions should be done,
		/// only simple basic types are to be coerced as necessary.
		/// </remarks>
		/// <param name="value">Non-null value to write</param>
		/// <exception cref="System.IO.IOException"/>
		protected internal virtual void _writeSimpleObject(object value)
		{
			/* 31-Dec-2009, tatu: Actually, we could just handle some basic
			*    types even without codec. This can improve interoperability,
			*    and specifically help with TokenBuffer.
			*/
			if (value == null)
			{
				writeNull();
				return;
			}
			if (value is string)
			{
				writeString((string)value);
				return;
			}
			if (value is Sharpen.Number)
			{
				Sharpen.Number n = (Sharpen.Number)value;
				if (n is int)
				{
					writeNumber(n);
					return;
				}
				else
				{
					if (n is long)
					{
						writeNumber(n);
						return;
					}
					else
					{
						if (n is double)
						{
							writeNumber(n);
							return;
						}
						else
						{
							if (n is float)
							{
								writeNumber(n);
								return;
							}
							else
							{
								if (n is short)
								{
									writeNumber(n);
									return;
								}
								else
								{
									if (n is byte)
									{
										writeNumber(n);
										return;
									}
									else
									{
										if (n is System.Numerics.BigInteger)
										{
											writeNumber((System.Numerics.BigInteger)n);
											return;
										}
										else
										{
											if (n is java.math.BigDecimal)
											{
												writeNumber((java.math.BigDecimal)n);
												return;
											}
											else
											{
												// then Atomic types
												if (n is java.util.concurrent.atomic.AtomicInteger)
												{
													writeNumber(((java.util.concurrent.atomic.AtomicInteger)n).get());
													return;
												}
												else
												{
													if (n is java.util.concurrent.atomic.AtomicLong)
													{
														writeNumber(((java.util.concurrent.atomic.AtomicLong)n).get());
														return;
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
			else
			{
				if (value is byte[])
				{
					writeBinary((byte[])value);
					return;
				}
				else
				{
					if (value is bool)
					{
						writeBoolean((bool)value);
						return;
					}
					else
					{
						if (value is java.util.concurrent.atomic.AtomicBoolean)
						{
							writeBoolean(((java.util.concurrent.atomic.AtomicBoolean)value).get());
							return;
						}
					}
				}
			}
			throw new System.InvalidOperationException("No ObjectCodec defined for the generator, can only serialize simple wrapper types (type passed "
				 + value.GetType().FullName + ")");
		}
	}
}
