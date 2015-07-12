using Sharpen;

namespace com.fasterxml.jackson.core.json
{
	/// <summary>
	/// Extension of
	/// <see cref="com.fasterxml.jackson.core.JsonStreamContext"/>
	/// , which implements
	/// core methods needed, and also exposes
	/// more complete API to generator implementation classes.
	/// </summary>
	public class JsonWriteContext : com.fasterxml.jackson.core.JsonStreamContext
	{
		public const int STATUS_OK_AS_IS = 0;

		public const int STATUS_OK_AFTER_COMMA = 1;

		public const int STATUS_OK_AFTER_COLON = 2;

		public const int STATUS_OK_AFTER_SPACE = 3;

		public const int STATUS_EXPECT_VALUE = 4;

		public const int STATUS_EXPECT_NAME = 5;

		/// <summary>Parent context for this context; null for root context.</summary>
		protected internal readonly com.fasterxml.jackson.core.json.JsonWriteContext _parent;

		protected internal com.fasterxml.jackson.core.json.DupDetector _dups;

		protected internal com.fasterxml.jackson.core.json.JsonWriteContext _child = null;

		/// <summary>
		/// Name of the field of which value is to be parsed; only
		/// used for OBJECT contexts
		/// </summary>
		protected internal string _currentName;

		/// <since>2.5</since>
		protected internal object _currentValue;

		/// <summary>
		/// Marker used to indicate that we just received a name, and
		/// now expect a value
		/// </summary>
		protected internal bool _gotName;

		protected internal JsonWriteContext(int type, com.fasterxml.jackson.core.json.JsonWriteContext
			 parent, com.fasterxml.jackson.core.json.DupDetector dups)
			: base()
		{
			// // // Return values for writeValue()
			// in root context
			// // // Optional duplicate detection
			/*
			/**********************************************************
			/* Simple instance reuse slots; speed up things
			/* a bit (10-15%) for docs with lots of small
			/* arrays/objects
			/**********************************************************
			*/
			/*
			/**********************************************************
			/* Location/state information (minus source reference)
			/**********************************************************
			*/
			/*
			/**********************************************************
			/* Life-cycle
			/**********************************************************
			*/
			_type = type;
			_parent = parent;
			_dups = dups;
			_index = -1;
		}

		protected internal virtual com.fasterxml.jackson.core.json.JsonWriteContext reset
			(int type)
		{
			_type = type;
			_index = -1;
			_currentName = null;
			_gotName = false;
			_currentValue = null;
			if (_dups != null)
			{
				_dups.reset();
			}
			return this;
		}

		public virtual com.fasterxml.jackson.core.json.JsonWriteContext withDupDetector(com.fasterxml.jackson.core.json.DupDetector
			 dups)
		{
			_dups = dups;
			return this;
		}

		public override object getCurrentValue()
		{
			return _currentValue;
		}

		public override void setCurrentValue(object v)
		{
			_currentValue = v;
		}

		/*
		/**********************************************************
		/* Factory methods
		/**********************************************************
		*/
		[System.ObsoleteAttribute(@"Since 2.3; use method that takes argument")]
		public static com.fasterxml.jackson.core.json.JsonWriteContext createRootContext(
			)
		{
			return createRootContext(null);
		}

		public static com.fasterxml.jackson.core.json.JsonWriteContext createRootContext(
			com.fasterxml.jackson.core.json.DupDetector dd)
		{
			return new com.fasterxml.jackson.core.json.JsonWriteContext(TYPE_ROOT, null, dd);
		}

		public virtual com.fasterxml.jackson.core.json.JsonWriteContext createChildArrayContext
			()
		{
			com.fasterxml.jackson.core.json.JsonWriteContext ctxt = _child;
			if (ctxt == null)
			{
				_child = ctxt = new com.fasterxml.jackson.core.json.JsonWriteContext(TYPE_ARRAY, 
					this, (_dups == null) ? null : _dups.child());
				return ctxt;
			}
			return ctxt.reset(TYPE_ARRAY);
		}

		public virtual com.fasterxml.jackson.core.json.JsonWriteContext createChildObjectContext
			()
		{
			com.fasterxml.jackson.core.json.JsonWriteContext ctxt = _child;
			if (ctxt == null)
			{
				_child = ctxt = new com.fasterxml.jackson.core.json.JsonWriteContext(TYPE_OBJECT, 
					this, (_dups == null) ? null : _dups.child());
				return ctxt;
			}
			return ctxt.reset(TYPE_OBJECT);
		}

		public sealed override com.fasterxml.jackson.core.JsonStreamContext getParent()
		{
			return _parent;
		}

		public sealed override string getCurrentName()
		{
			return _currentName;
		}

		public virtual com.fasterxml.jackson.core.json.DupDetector getDupDetector()
		{
			return _dups;
		}

		/// <summary>Method that writer is to call before it writes a field name.</summary>
		/// <returns>Index of the field entry (0-based)</returns>
		/// <exception cref="com.fasterxml.jackson.core.JsonProcessingException"/>
		public virtual int writeFieldName(string name)
		{
			if (_gotName)
			{
				return com.fasterxml.jackson.core.json.JsonWriteContext.STATUS_EXPECT_VALUE;
			}
			_gotName = true;
			_currentName = name;
			if (_dups != null)
			{
				_checkDup(_dups, name);
			}
			return (_index < 0) ? STATUS_OK_AS_IS : STATUS_OK_AFTER_COMMA;
		}

		/// <exception cref="com.fasterxml.jackson.core.JsonProcessingException"/>
		private void _checkDup(com.fasterxml.jackson.core.json.DupDetector dd, string name
			)
		{
			if (dd.isDup(name))
			{
				throw new com.fasterxml.jackson.core.JsonGenerationException("Duplicate field '" 
					+ name + "'");
			}
		}

		public virtual int writeValue()
		{
			// Most likely, object:
			if (_type == TYPE_OBJECT)
			{
				if (!_gotName)
				{
					return STATUS_EXPECT_NAME;
				}
				_gotName = false;
				++_index;
				return STATUS_OK_AFTER_COLON;
			}
			// Ok, array?
			if (_type == TYPE_ARRAY)
			{
				int ix = _index;
				++_index;
				return (ix < 0) ? STATUS_OK_AS_IS : STATUS_OK_AFTER_COMMA;
			}
			// Nope, root context
			// No commas within root context, but need space
			++_index;
			return (_index == 0) ? STATUS_OK_AS_IS : STATUS_OK_AFTER_SPACE;
		}

		// // // Internally used abstract methods
		protected internal virtual void appendDesc(System.Text.StringBuilder sb)
		{
			if (_type == TYPE_OBJECT)
			{
				sb.Append('{');
				if (_currentName != null)
				{
					sb.Append('"');
					// !!! TODO: Name chars should be escaped?
					sb.Append(_currentName);
					sb.Append('"');
				}
				else
				{
					sb.Append('?');
				}
				sb.Append('}');
			}
			else
			{
				if (_type == TYPE_ARRAY)
				{
					sb.Append('[');
					sb.Append(getCurrentIndex());
					sb.Append(']');
				}
				else
				{
					// nah, ROOT:
					sb.Append("/");
				}
			}
		}

		// // // Overridden standard methods
		/// <summary>
		/// Overridden to provide developer writeable "JsonPath" representation
		/// of the context.
		/// </summary>
		public override string ToString()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder(64);
			appendDesc(sb);
			return sb.ToString();
		}
	}
}
