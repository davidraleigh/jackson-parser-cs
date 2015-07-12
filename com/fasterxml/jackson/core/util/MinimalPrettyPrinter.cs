using Sharpen;

namespace com.fasterxml.jackson.core.util
{
	/// <summary>
	/// <see cref="com.fasterxml.jackson.core.PrettyPrinter"/>
	/// implementation that adds no indentation,
	/// just implements everything necessary for value output to work
	/// as expected, and provide simpler extension points to allow
	/// for creating simple custom implementations that add specific
	/// decoration or overrides. Since behavior then is very similar
	/// to using no pretty printer at all, usually sub-classes are used.
	/// <p>
	/// Beyond purely minimal implementation, there is limited amount of
	/// configurability which may be useful for actual use: for example,
	/// it is possible to redefine separator used between root-level
	/// values (default is single space; can be changed to line-feed).
	/// <p>
	/// Note: does NOT implement
	/// <see cref="Instantiatable{T}"/>
	/// since this is
	/// a stateless implementation; that is, a single instance can be
	/// shared between threads.
	/// </summary>
	[System.Serializable]
	public class MinimalPrettyPrinter : com.fasterxml.jackson.core.PrettyPrinter
	{
		private const long serialVersionUID = 1L;

		/// <summary>Default String used for separating root values is single space.</summary>
		public const string DEFAULT_ROOT_VALUE_SEPARATOR = " ";

		protected internal string _rootValueSeparator = DEFAULT_ROOT_VALUE_SEPARATOR;

		public MinimalPrettyPrinter()
			: this(DEFAULT_ROOT_VALUE_SEPARATOR)
		{
		}

		public MinimalPrettyPrinter(string rootValueSeparator)
		{
			/*
			/**********************************************************
			/* Life-cycle, construction, configuration
			/**********************************************************
			*/
			_rootValueSeparator = rootValueSeparator;
		}

		public virtual void setRootValueSeparator(string sep)
		{
			_rootValueSeparator = sep;
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
			if (_rootValueSeparator != null)
			{
				jg.writeRaw(_rootValueSeparator);
			}
		}

		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		public virtual void writeStartObject(com.fasterxml.jackson.core.JsonGenerator jg)
		{
			jg.writeRaw('{');
		}

		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		public virtual void beforeObjectEntries(com.fasterxml.jackson.core.JsonGenerator 
			jg)
		{
		}

		// nothing special, since no indentation is added
		/// <summary>
		/// Method called after an object field has been output, but
		/// before the value is output.
		/// </summary>
		/// <remarks>
		/// Method called after an object field has been output, but
		/// before the value is output.
		/// <p>
		/// Default handling will just output a single
		/// colon to separate the two, without additional spaces.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		public virtual void writeObjectFieldValueSeparator(com.fasterxml.jackson.core.JsonGenerator
			 jg)
		{
			jg.writeRaw(':');
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
		/// comma to separate the two.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		public virtual void writeObjectEntrySeparator(com.fasterxml.jackson.core.JsonGenerator
			 jg)
		{
			jg.writeRaw(',');
		}

		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		public virtual void writeEndObject(com.fasterxml.jackson.core.JsonGenerator jg, int
			 nrOfEntries)
		{
			jg.writeRaw('}');
		}

		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		public virtual void writeStartArray(com.fasterxml.jackson.core.JsonGenerator jg)
		{
			jg.writeRaw('[');
		}

		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		public virtual void beforeArrayValues(com.fasterxml.jackson.core.JsonGenerator jg
			)
		{
		}

		// nothing special, since no indentation is added
		/// <summary>
		/// Method called after an array value has been completely
		/// output, and before another value is to be output.
		/// </summary>
		/// <remarks>
		/// Method called after an array value has been completely
		/// output, and before another value is to be output.
		/// <p>
		/// Default handling (without pretty-printing) will output a single
		/// comma to separate values.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		public virtual void writeArrayValueSeparator(com.fasterxml.jackson.core.JsonGenerator
			 jg)
		{
			jg.writeRaw(',');
		}

		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		public virtual void writeEndArray(com.fasterxml.jackson.core.JsonGenerator jg, int
			 nrOfValues)
		{
			jg.writeRaw(']');
		}
	}
}
