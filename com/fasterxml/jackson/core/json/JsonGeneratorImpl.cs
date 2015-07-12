using Sharpen;

namespace com.fasterxml.jackson.core.json
{
	/// <summary>
	/// Intermediate base class shared by JSON-backed generators
	/// like
	/// <see cref="UTF8JsonGenerator"/>
	/// and
	/// <see cref="WriterBasedJsonGenerator"/>
	/// .
	/// </summary>
	/// <since>2.1</since>
	public abstract class JsonGeneratorImpl : com.fasterxml.jackson.core.@base.GeneratorBase
	{
		/// <summary>
		/// This is the default set of escape codes, over 7-bit ASCII range
		/// (first 128 character codes), used for single-byte UTF-8 characters.
		/// </summary>
		protected internal static readonly int[] sOutputEscapes = com.fasterxml.jackson.core.io.CharTypes
			.get7BitOutputEscapes();

		protected internal readonly com.fasterxml.jackson.core.io.IOContext _ioContext;

		/// <summary>
		/// Currently active set of output escape code definitions (whether
		/// and how to escape or not) for 7-bit ASCII range (first 128
		/// character codes).
		/// </summary>
		/// <remarks>
		/// Currently active set of output escape code definitions (whether
		/// and how to escape or not) for 7-bit ASCII range (first 128
		/// character codes). Defined separately to make potentially
		/// customizable
		/// </remarks>
		protected internal int[] _outputEscapes = sOutputEscapes;

		/// <summary>
		/// Value between 128 (0x80) and 65535 (0xFFFF) that indicates highest
		/// Unicode code point that will not need escaping; or 0 to indicate
		/// that all characters can be represented without escaping.
		/// </summary>
		/// <remarks>
		/// Value between 128 (0x80) and 65535 (0xFFFF) that indicates highest
		/// Unicode code point that will not need escaping; or 0 to indicate
		/// that all characters can be represented without escaping.
		/// Typically used to force escaping of some portion of character set;
		/// for example to always escape non-ASCII characters (if value was 127).
		/// <p>
		/// NOTE: not all sub-classes make use of this setting.
		/// </remarks>
		protected internal int _maximumNonEscapedChar;

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

		/// <summary>Separator to use, if any, between root-level values.</summary>
		/// <since>2.1</since>
		protected internal com.fasterxml.jackson.core.SerializableString _rootValueSeparator
			 = com.fasterxml.jackson.core.util.DefaultPrettyPrinter.DEFAULT_ROOT_VALUE_SEPARATOR;

		public JsonGeneratorImpl(com.fasterxml.jackson.core.io.IOContext ctxt, int features
			, com.fasterxml.jackson.core.ObjectCodec codec)
			: base(features, codec)
		{
			/*
			/**********************************************************
			/* Constants
			/**********************************************************
			*/
			/*
			/**********************************************************
			/* Configuration, basic I/O
			/**********************************************************
			*/
			/*
			/**********************************************************
			/* Configuration, output escaping
			/**********************************************************
			*/
			/*
			/**********************************************************
			/* Configuration, other
			/**********************************************************
			*/
			/*
			/**********************************************************
			/* Life-cycle
			/**********************************************************
			*/
			_ioContext = ctxt;
			if (isEnabled(com.fasterxml.jackson.core.JsonGenerator.Feature.ESCAPE_NON_ASCII))
			{
				setHighestNonEscapedChar(127);
			}
		}

		/*
		/**********************************************************
		/* Overridden configuration methods
		/**********************************************************
		*/
		public override com.fasterxml.jackson.core.JsonGenerator setHighestNonEscapedChar
			(int charCode)
		{
			_maximumNonEscapedChar = (charCode < 0) ? 0 : charCode;
			return this;
		}

		public override int getHighestEscapedChar()
		{
			return _maximumNonEscapedChar;
		}

		public override com.fasterxml.jackson.core.JsonGenerator setCharacterEscapes(com.fasterxml.jackson.core.io.CharacterEscapes
			 esc)
		{
			_characterEscapes = esc;
			if (esc == null)
			{
				// revert to standard escapes
				_outputEscapes = sOutputEscapes;
			}
			else
			{
				_outputEscapes = esc.getEscapeCodesForAscii();
			}
			return this;
		}

		/// <summary>
		/// Method for accessing custom escapes factory uses for
		/// <see cref="com.fasterxml.jackson.core.JsonGenerator"/>
		/// s
		/// it creates.
		/// </summary>
		public override com.fasterxml.jackson.core.io.CharacterEscapes getCharacterEscapes
			()
		{
			return _characterEscapes;
		}

		public override com.fasterxml.jackson.core.JsonGenerator setRootValueSeparator(com.fasterxml.jackson.core.SerializableString
			 sep)
		{
			_rootValueSeparator = sep;
			return this;
		}

		/*
		/**********************************************************
		/* Versioned
		/**********************************************************
		*/
		public override com.fasterxml.jackson.core.Version version()
		{
			return com.fasterxml.jackson.core.util.VersionUtil.versionFor(GetType());
		}

		/*
		/**********************************************************
		/* Partial API
		/**********************************************************
		*/
		// // Overrides just to make things final, to possibly help with inlining
		/// <exception cref="System.IO.IOException"/>
		public sealed override void writeStringField(string fieldName, string value)
		{
			writeFieldName(fieldName);
			writeString(value);
		}
	}
}
