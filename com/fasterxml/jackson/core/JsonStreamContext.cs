/* Jackson JSON-processor.
*
* Copyright (c) 2007- Tatu Saloranta, tatu.saloranta@iki.fi
*/
using Sharpen;

namespace com.fasterxml.jackson.core
{
	/// <summary>
	/// Shared base class for streaming processing contexts used during
	/// reading and writing of Json content using Streaming API.
	/// </summary>
	/// <remarks>
	/// Shared base class for streaming processing contexts used during
	/// reading and writing of Json content using Streaming API.
	/// This context is also exposed to applications:
	/// context object can be used by applications to get an idea of
	/// relative position of the parser/generator within json content
	/// being processed. This allows for some contextual processing: for
	/// example, output within Array context can differ from that of
	/// Object context.
	/// </remarks>
	public abstract class JsonStreamContext
	{
		protected internal const int TYPE_ROOT = 0;

		protected internal const int TYPE_ARRAY = 1;

		protected internal const int TYPE_OBJECT = 2;

		protected internal int _type;

		/// <summary>Index of the currently processed entry.</summary>
		/// <remarks>
		/// Index of the currently processed entry. Starts with -1 to signal
		/// that no entries have been started, and gets advanced each
		/// time a new entry is started, either by encountering an expected
		/// separator, or with new values if no separators are expected
		/// (the case for root context).
		/// </remarks>
		protected internal int _index;

		protected internal JsonStreamContext()
		{
		}

		// // // Type constants used internally
		/*
		/**********************************************************
		/* Life-cycle
		/**********************************************************
		*/
		/*
		/**********************************************************
		/* Public API, accessors
		/**********************************************************
		*/
		/// <summary>
		/// Accessor for finding parent context of this context; will
		/// return null for root context.
		/// </summary>
		public abstract com.fasterxml.jackson.core.JsonStreamContext getParent();

		/// <summary>
		/// Method that returns true if this context is an Array context;
		/// that is, content is being read from or written to a Json Array.
		/// </summary>
		public bool inArray()
		{
			return _type == TYPE_ARRAY;
		}

		/// <summary>
		/// Method that returns true if this context is a Root context;
		/// that is, content is being read from or written to without
		/// enclosing array or object structure.
		/// </summary>
		public bool inRoot()
		{
			return _type == TYPE_ROOT;
		}

		/// <summary>
		/// Method that returns true if this context is an Object context;
		/// that is, content is being read from or written to a Json Object.
		/// </summary>
		public bool inObject()
		{
			return _type == TYPE_OBJECT;
		}

		/// <summary>
		/// Method for accessing simple type description of current context;
		/// either ROOT (for root-level values), OBJECT (for field names and
		/// values of JSON Objects) or ARRAY (for values of JSON Arrays)
		/// </summary>
		public string getTypeDesc()
		{
			switch (_type)
			{
				case TYPE_ROOT:
				{
					return "ROOT";
				}

				case TYPE_ARRAY:
				{
					return "ARRAY";
				}

				case TYPE_OBJECT:
				{
					return "OBJECT";
				}
			}
			return "?";
		}

		/// <returns>Number of entries that are complete and started.</returns>
		public int getEntryCount()
		{
			return _index + 1;
		}

		/// <returns>Index of the currently processed entry, if any</returns>
		public int getCurrentIndex()
		{
			return (_index < 0) ? 0 : _index;
		}

		/// <summary>Method for accessing name associated with the current location.</summary>
		/// <remarks>
		/// Method for accessing name associated with the current location.
		/// Non-null for <code>FIELD_NAME</code> and value events that directly
		/// follow field names; null for root level and array values.
		/// </remarks>
		public abstract string getCurrentName();

		/// <summary>
		/// Method for accessing currently active value being used by data-binding
		/// (as the source of streaming data to write, or destination of data being
		/// read), at this level in hierarchy.
		/// </summary>
		/// <remarks>
		/// Method for accessing currently active value being used by data-binding
		/// (as the source of streaming data to write, or destination of data being
		/// read), at this level in hierarchy.
		/// The value may not exist or be available due to various limitations (at
		/// least during reading of data, as target value object may not have yet
		/// been constructed).
		/// </remarks>
		/// <returns>Currently active value, if one has been assigned.</returns>
		/// <since>2.5</since>
		public virtual object getCurrentValue()
		{
			return null;
		}

		/// <summary>
		/// Method to call to pass value to be returned via
		/// <see cref="getCurrentValue()"/>
		/// ; typically
		/// called indirectly through
		/// <see cref="JsonParser.setCurrentValue(object)"/>
		/// or
		/// <see cref="JsonGenerator.setCurrentValue(object)"/>
		/// ).
		/// </summary>
		/// <since>2.5</since>
		public virtual void setCurrentValue(object v)
		{
		}
	}
}
