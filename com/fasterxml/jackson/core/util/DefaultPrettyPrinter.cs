using Sharpen;

namespace com.fasterxml.jackson.core.util
{
	/// <summary>
	/// Default
	/// <see cref="com.fasterxml.jackson.core.PrettyPrinter"/>
	/// implementation that uses 2-space
	/// indentation with platform-default linefeeds.
	/// Usually this class is not instantiated directly, but instead
	/// method
	/// <see cref="com.fasterxml.jackson.core.JsonGenerator.useDefaultPrettyPrinter()"/>
	/// is
	/// used, which will use an instance of this class for operation.
	/// </summary>
	[System.Serializable]
	public class DefaultPrettyPrinter : com.fasterxml.jackson.core.PrettyPrinter, com.fasterxml.jackson.core.util.Instantiatable
		<com.fasterxml.jackson.core.util.DefaultPrettyPrinter>
	{
		private const long serialVersionUID = 1;

		/// <summary>
		/// Constant that specifies default "root-level" separator to use between
		/// root values: a single space character.
		/// </summary>
		/// <since>2.1</since>
		public static readonly com.fasterxml.jackson.core.io.SerializedString DEFAULT_ROOT_VALUE_SEPARATOR
			 = new com.fasterxml.jackson.core.io.SerializedString(" ");

		/// <summary>
		/// Interface that defines objects that can produce indentation used
		/// to separate object entries and array values.
		/// </summary>
		/// <remarks>
		/// Interface that defines objects that can produce indentation used
		/// to separate object entries and array values. Indentation in this
		/// context just means insertion of white space, independent of whether
		/// linefeeds are output.
		/// </remarks>
		public interface Indenter
		{
			/// <exception cref="System.IO.IOException"/>
			void writeIndentation(com.fasterxml.jackson.core.JsonGenerator jg, int level);

			/// <returns>
			/// True if indenter is considered inline (does not add linefeeds),
			/// false otherwise
			/// </returns>
			bool isInline();
		}

		/// <summary>By default, let's use only spaces to separate array values.</summary>
		protected internal com.fasterxml.jackson.core.util.DefaultPrettyPrinter.Indenter 
			_arrayIndenter = com.fasterxml.jackson.core.util.DefaultPrettyPrinter.FixedSpaceIndenter
			.instance;

		/// <summary>
		/// By default, let's use linefeed-adding indenter for separate
		/// object entries.
		/// </summary>
		/// <remarks>
		/// By default, let's use linefeed-adding indenter for separate
		/// object entries. We'll further configure indenter to use
		/// system-specific linefeeds, and 2 spaces per level (as opposed to,
		/// say, single tabs)
		/// </remarks>
		protected internal com.fasterxml.jackson.core.util.DefaultPrettyPrinter.Indenter 
			_objectIndenter = com.fasterxml.jackson.core.util.DefaultIndenter.SYSTEM_LINEFEED_INSTANCE;

		/// <summary>String printed between root-level values, if any.</summary>
		protected internal readonly com.fasterxml.jackson.core.SerializableString _rootSeparator;

		/// <summary>
		/// By default we will add spaces around colons used to
		/// separate object fields and values.
		/// </summary>
		/// <remarks>
		/// By default we will add spaces around colons used to
		/// separate object fields and values.
		/// If disabled, will not use spaces around colon.
		/// </remarks>
		protected internal bool _spacesInObjectEntries = true;

		/// <summary>Number of open levels of nesting.</summary>
		/// <remarks>
		/// Number of open levels of nesting. Used to determine amount of
		/// indentation to use.
		/// </remarks>
		[System.NonSerialized]
		protected internal int _nesting = 0;

		public DefaultPrettyPrinter()
			: this(DEFAULT_ROOT_VALUE_SEPARATOR)
		{
		}

		/// <summary>
		/// Constructor that specifies separator String to use between root values;
		/// if null, no separator is printed.
		/// </summary>
		/// <remarks>
		/// Constructor that specifies separator String to use between root values;
		/// if null, no separator is printed.
		/// <p>
		/// Note: simply constructs a
		/// <see cref="com.fasterxml.jackson.core.io.SerializedString"/>
		/// out of parameter,
		/// calls
		/// <see cref="DefaultPrettyPrinter(com.fasterxml.jackson.core.SerializableString)"/>
		/// </remarks>
		/// <param name="rootSeparator"/>
		/// <since>2.1</since>
		public DefaultPrettyPrinter(string rootSeparator)
			: this((rootSeparator == null) ? null : new com.fasterxml.jackson.core.io.SerializedString
				(rootSeparator))
		{
		}

		/// <summary>
		/// Constructor that specifies separator String to use between root values;
		/// if null, no separator is printed.
		/// </summary>
		/// <param name="rootSeparator"/>
		/// <since>2.1</since>
		public DefaultPrettyPrinter(com.fasterxml.jackson.core.SerializableString rootSeparator
			)
		{
			// // // Config, indentation
			// // // Config, other white space configuration
			// // // State:
			/*
			/**********************************************************
			/* Life-cycle (construct, configure)
			/**********************************************************
			*/
			_rootSeparator = rootSeparator;
		}

		public DefaultPrettyPrinter(com.fasterxml.jackson.core.util.DefaultPrettyPrinter 
			@base)
			: this(@base, @base._rootSeparator)
		{
		}

		public DefaultPrettyPrinter(com.fasterxml.jackson.core.util.DefaultPrettyPrinter 
			@base, com.fasterxml.jackson.core.SerializableString rootSeparator)
		{
			_arrayIndenter = @base._arrayIndenter;
			_objectIndenter = @base._objectIndenter;
			_spacesInObjectEntries = @base._spacesInObjectEntries;
			_nesting = @base._nesting;
			_rootSeparator = rootSeparator;
		}

		public virtual com.fasterxml.jackson.core.util.DefaultPrettyPrinter withRootSeparator
			(com.fasterxml.jackson.core.SerializableString rootSeparator)
		{
			if (_rootSeparator == rootSeparator || (rootSeparator != null && rootSeparator.Equals
				(_rootSeparator)))
			{
				return this;
			}
			return new com.fasterxml.jackson.core.util.DefaultPrettyPrinter(this, rootSeparator
				);
		}

		/// <since>2.6.0</since>
		public virtual com.fasterxml.jackson.core.util.DefaultPrettyPrinter withRootSeparator
			(string rootSeparator)
		{
			return withRootSeparator((rootSeparator == null) ? null : new com.fasterxml.jackson.core.io.SerializedString
				(rootSeparator));
		}

		public virtual void indentArraysWith(com.fasterxml.jackson.core.util.DefaultPrettyPrinter.Indenter
			 i)
		{
			_arrayIndenter = (i == null) ? com.fasterxml.jackson.core.util.DefaultPrettyPrinter.NopIndenter
				.instance : i;
		}

		public virtual void indentObjectsWith(com.fasterxml.jackson.core.util.DefaultPrettyPrinter.Indenter
			 i)
		{
			_objectIndenter = (i == null) ? com.fasterxml.jackson.core.util.DefaultPrettyPrinter.NopIndenter
				.instance : i;
		}

		[System.ObsoleteAttribute(@"Since 2.3 use withSpacesInObjectEntries() and withoutSpacesInObjectEntries()"
			)]
		public virtual void spacesInObjectEntries(bool b)
		{
			_spacesInObjectEntries = b;
		}

		/// <since>2.3</since>
		public virtual com.fasterxml.jackson.core.util.DefaultPrettyPrinter withArrayIndenter
			(com.fasterxml.jackson.core.util.DefaultPrettyPrinter.Indenter i)
		{
			if (i == null)
			{
				i = com.fasterxml.jackson.core.util.DefaultPrettyPrinter.NopIndenter.instance;
			}
			if (_arrayIndenter == i)
			{
				return this;
			}
			com.fasterxml.jackson.core.util.DefaultPrettyPrinter pp = new com.fasterxml.jackson.core.util.DefaultPrettyPrinter
				(this);
			pp._arrayIndenter = i;
			return pp;
		}

		/// <since>2.3</since>
		public virtual com.fasterxml.jackson.core.util.DefaultPrettyPrinter withObjectIndenter
			(com.fasterxml.jackson.core.util.DefaultPrettyPrinter.Indenter i)
		{
			if (i == null)
			{
				i = com.fasterxml.jackson.core.util.DefaultPrettyPrinter.NopIndenter.instance;
			}
			if (_objectIndenter == i)
			{
				return this;
			}
			com.fasterxml.jackson.core.util.DefaultPrettyPrinter pp = new com.fasterxml.jackson.core.util.DefaultPrettyPrinter
				(this);
			pp._objectIndenter = i;
			return pp;
		}

		/// <summary>
		/// "Mutant factory" method that will return a pretty printer instance
		/// that does use spaces inside object entries; if 'this' instance already
		/// does this, it is returned; if not, a new instance will be constructed
		/// and returned.
		/// </summary>
		/// <since>2.3</since>
		public virtual com.fasterxml.jackson.core.util.DefaultPrettyPrinter withSpacesInObjectEntries
			()
		{
			return _withSpaces(true);
		}

		/// <summary>
		/// "Mutant factory" method that will return a pretty printer instance
		/// that does not use spaces inside object entries; if 'this' instance already
		/// does this, it is returned; if not, a new instance will be constructed
		/// and returned.
		/// </summary>
		/// <since>2.3</since>
		public virtual com.fasterxml.jackson.core.util.DefaultPrettyPrinter withoutSpacesInObjectEntries
			()
		{
			return _withSpaces(false);
		}

		protected internal virtual com.fasterxml.jackson.core.util.DefaultPrettyPrinter _withSpaces
			(bool state)
		{
			if (_spacesInObjectEntries == state)
			{
				return this;
			}
			com.fasterxml.jackson.core.util.DefaultPrettyPrinter pp = new com.fasterxml.jackson.core.util.DefaultPrettyPrinter
				(this);
			pp._spacesInObjectEntries = state;
			return pp;
		}

		/*
		/**********************************************************
		/* Instantiatable impl
		/**********************************************************
		*/
		public virtual com.fasterxml.jackson.core.util.DefaultPrettyPrinter createInstance
			()
		{
			return new com.fasterxml.jackson.core.util.DefaultPrettyPrinter(this);
		}

		/*
		/**********************************************************
		/* PrettyPrinter impl
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		public virtual void writeRootValueSeparator(com.fasterxml.jackson.core.JsonGenerator
			 jg)
		{
			if (_rootSeparator != null)
			{
				jg.writeRaw(_rootSeparator);
			}
		}

		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		public virtual void writeStartObject(com.fasterxml.jackson.core.JsonGenerator jg)
		{
			jg.writeRaw('{');
			if (!_objectIndenter.isInline())
			{
				++_nesting;
			}
		}

		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		public virtual void beforeObjectEntries(com.fasterxml.jackson.core.JsonGenerator 
			jg)
		{
			_objectIndenter.writeIndentation(jg, _nesting);
		}

		/// <summary>
		/// Method called after an object field has been output, but
		/// before the value is output.
		/// </summary>
		/// <remarks>
		/// Method called after an object field has been output, but
		/// before the value is output.
		/// <p>
		/// Default handling (without pretty-printing) will output a single
		/// colon to separate the two. Pretty-printer is
		/// to output a colon as well, but can surround that with other
		/// (white-space) decoration.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		public virtual void writeObjectFieldValueSeparator(com.fasterxml.jackson.core.JsonGenerator
			 jg)
		{
			if (_spacesInObjectEntries)
			{
				jg.writeRaw(" : ");
			}
			else
			{
				jg.writeRaw(':');
			}
		}

		/// <summary>
		/// Method called after an object entry (field:value) has been completely
		/// output, and before another value is to be output.
		/// </summary>
		/// <remarks>
		/// Method called after an object entry (field:value) has been completely
		/// output, and before another value is to be output.
		/// <p>
		/// Default handling (without pretty-printing) will output a single
		/// comma to separate the two. Pretty-printer is
		/// to output a comma as well, but can surround that with other
		/// (white-space) decoration.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		public virtual void writeObjectEntrySeparator(com.fasterxml.jackson.core.JsonGenerator
			 jg)
		{
			jg.writeRaw(',');
			_objectIndenter.writeIndentation(jg, _nesting);
		}

		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		public virtual void writeEndObject(com.fasterxml.jackson.core.JsonGenerator jg, int
			 nrOfEntries)
		{
			if (!_objectIndenter.isInline())
			{
				--_nesting;
			}
			if (nrOfEntries > 0)
			{
				_objectIndenter.writeIndentation(jg, _nesting);
			}
			else
			{
				jg.writeRaw(' ');
			}
			jg.writeRaw('}');
		}

		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		public virtual void writeStartArray(com.fasterxml.jackson.core.JsonGenerator jg)
		{
			if (!_arrayIndenter.isInline())
			{
				++_nesting;
			}
			jg.writeRaw('[');
		}

		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		public virtual void beforeArrayValues(com.fasterxml.jackson.core.JsonGenerator jg
			)
		{
			_arrayIndenter.writeIndentation(jg, _nesting);
		}

		/// <summary>
		/// Method called after an array value has been completely
		/// output, and before another value is to be output.
		/// </summary>
		/// <remarks>
		/// Method called after an array value has been completely
		/// output, and before another value is to be output.
		/// <p>
		/// Default handling (without pretty-printing) will output a single
		/// comma to separate the two. Pretty-printer is
		/// to output a comma as well, but can surround that with other
		/// (white-space) decoration.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public virtual void writeArrayValueSeparator(com.fasterxml.jackson.core.JsonGenerator
			 gen)
		{
			gen.writeRaw(',');
			_arrayIndenter.writeIndentation(gen, _nesting);
		}

		/// <exception cref="System.IO.IOException"/>
		public virtual void writeEndArray(com.fasterxml.jackson.core.JsonGenerator gen, int
			 nrOfValues)
		{
			if (!_arrayIndenter.isInline())
			{
				--_nesting;
			}
			if (nrOfValues > 0)
			{
				_arrayIndenter.writeIndentation(gen, _nesting);
			}
			else
			{
				gen.writeRaw(' ');
			}
			gen.writeRaw(']');
		}

		/// <summary>Dummy implementation that adds no indentation whatsoever</summary>
		[System.Serializable]
		public class NopIndenter : com.fasterxml.jackson.core.util.DefaultPrettyPrinter.Indenter
		{
			public static readonly com.fasterxml.jackson.core.util.DefaultPrettyPrinter.NopIndenter
				 instance = new com.fasterxml.jackson.core.util.DefaultPrettyPrinter.NopIndenter
				();

			/*
			/**********************************************************
			/* Helper classes
			/**********************************************************
			*/
			/// <exception cref="System.IO.IOException"/>
			public virtual void writeIndentation(com.fasterxml.jackson.core.JsonGenerator jg, 
				int level)
			{
			}

			public virtual bool isInline()
			{
				return true;
			}
		}

		/// <summary>
		/// This is a very simple indenter that only adds a
		/// single space for indentation.
		/// </summary>
		/// <remarks>
		/// This is a very simple indenter that only adds a
		/// single space for indentation. It is used as the default
		/// indenter for array values.
		/// </remarks>
		[System.Serializable]
		public class FixedSpaceIndenter : com.fasterxml.jackson.core.util.DefaultPrettyPrinter.NopIndenter
		{
			public static readonly com.fasterxml.jackson.core.util.DefaultPrettyPrinter.FixedSpaceIndenter
				 instance = new com.fasterxml.jackson.core.util.DefaultPrettyPrinter.FixedSpaceIndenter
				();

			/// <exception cref="System.IO.IOException"/>
			public override void writeIndentation(com.fasterxml.jackson.core.JsonGenerator jg
				, int level)
			{
				jg.writeRaw(' ');
			}

			public override bool isInline()
			{
				return true;
			}
		}

		[System.Serializable]
		[System.ObsoleteAttribute(@"Since 2.5 use DefaultIndenter instead")]
		public class Lf2SpacesIndenter : com.fasterxml.jackson.core.util.DefaultIndenter
		{
			[System.ObsoleteAttribute(@"Use DefaultIndenter.SYSTEM_LINEFEED_INSTANCE instead."
				)]
			public static readonly com.fasterxml.jackson.core.util.DefaultPrettyPrinter.Lf2SpacesIndenter
				 instance = new com.fasterxml.jackson.core.util.DefaultPrettyPrinter.Lf2SpacesIndenter
				();

			[System.ObsoleteAttribute(@"Use new DefaultIndenter(""  "", DefaultIndenter.SYS_LF) instead"
				)]
			public Lf2SpacesIndenter()
				: base("  ", com.fasterxml.jackson.core.util.DefaultIndenter.SYS_LF)
			{
			}

			[System.ObsoleteAttribute(@"Use new DefaultIndenter(""  "", lf) instead")]
			public Lf2SpacesIndenter(string lf)
				: base("  ", lf)
			{
			}

			/// <summary>
			/// Note: method was accidentally missing from 2.5.0; put back for 2.5.1 and
			/// later 2.5.x versions.
			/// </summary>
			public override com.fasterxml.jackson.core.util.DefaultIndenter withLinefeed(string
				 lf)
			{
				if (lf.Equals(getEol()))
				{
					return this;
				}
				return new com.fasterxml.jackson.core.util.DefaultPrettyPrinter.Lf2SpacesIndenter
					(lf);
			}
		}
	}
}
