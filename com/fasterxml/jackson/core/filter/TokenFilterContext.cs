using Sharpen;

namespace com.fasterxml.jackson.core.filter
{
	/// <summary>
	/// Alternative variant of
	/// <see cref="com.fasterxml.jackson.core.JsonStreamContext"/>
	/// , used when filtering
	/// content being read or written (based on
	/// <see cref="TokenFilter"/>
	/// ).
	/// </summary>
	/// <since>2.6</since>
	public class TokenFilterContext : com.fasterxml.jackson.core.JsonStreamContext
	{
		/// <summary>Parent context for this context; null for root context.</summary>
		protected internal readonly com.fasterxml.jackson.core.filter.TokenFilterContext 
			_parent;

		protected internal com.fasterxml.jackson.core.filter.TokenFilterContext _child = 
			null;

		/// <summary>
		/// Name of the field of which value is to be parsed; only
		/// used for OBJECT contexts
		/// </summary>
		protected internal string _currentName;

		/// <summary>
		/// Filter to use for items in this state (for properties of Objects,
		/// elements of Arrays, and root-level values of root context)
		/// </summary>
		protected internal com.fasterxml.jackson.core.filter.TokenFilter _filter;

		/// <summary>
		/// Flag that indicates that start token has been read/written,
		/// so that matching close token needs to be read/written as well
		/// when context is getting closed.
		/// </summary>
		protected internal bool _startHandled;

		/// <summary>
		/// Flag that indicates that the current name of this context
		/// still needs to be read/written, if path from root down to
		/// included leaf is to be exposed.
		/// </summary>
		protected internal bool _needToHandleName;

		protected internal TokenFilterContext(int type, com.fasterxml.jackson.core.filter.TokenFilterContext
			 parent, com.fasterxml.jackson.core.filter.TokenFilter filter, bool startHandled
			)
			: base()
		{
			/*
			/**********************************************************
			/* Simple instance reuse slots; speed up things
			/* a bit (10-15%) for docs with lots of small
			/* arrays/objects
			/**********************************************************
			*/
			/*
			/**********************************************************
			/* Location/state information
			/**********************************************************
			*/
			/*
			/**********************************************************
			/* Life-cycle
			/**********************************************************
			*/
			_type = type;
			_parent = parent;
			_filter = filter;
			_index = -1;
			_startHandled = startHandled;
			_needToHandleName = false;
		}

		protected internal virtual com.fasterxml.jackson.core.filter.TokenFilterContext reset
			(int type, com.fasterxml.jackson.core.filter.TokenFilter filter, bool startWritten
			)
		{
			_type = type;
			_filter = filter;
			_index = -1;
			_currentName = null;
			_startHandled = startWritten;
			_needToHandleName = false;
			return this;
		}

		/*
		/**********************************************************
		/* Factory methods
		/**********************************************************
		*/
		public static com.fasterxml.jackson.core.filter.TokenFilterContext createRootContext
			(com.fasterxml.jackson.core.filter.TokenFilter filter)
		{
			// true -> since we have no start/end marker, consider start handled
			return new com.fasterxml.jackson.core.filter.TokenFilterContext(TYPE_ROOT, null, 
				filter, true);
		}

		public virtual com.fasterxml.jackson.core.filter.TokenFilterContext createChildArrayContext
			(com.fasterxml.jackson.core.filter.TokenFilter filter, bool writeStart)
		{
			com.fasterxml.jackson.core.filter.TokenFilterContext ctxt = _child;
			if (ctxt == null)
			{
				_child = ctxt = new com.fasterxml.jackson.core.filter.TokenFilterContext(TYPE_ARRAY
					, this, filter, writeStart);
				return ctxt;
			}
			return ctxt.reset(TYPE_ARRAY, filter, writeStart);
		}

		public virtual com.fasterxml.jackson.core.filter.TokenFilterContext createChildObjectContext
			(com.fasterxml.jackson.core.filter.TokenFilter filter, bool writeStart)
		{
			com.fasterxml.jackson.core.filter.TokenFilterContext ctxt = _child;
			if (ctxt == null)
			{
				_child = ctxt = new com.fasterxml.jackson.core.filter.TokenFilterContext(TYPE_OBJECT
					, this, filter, writeStart);
				return ctxt;
			}
			return ctxt.reset(TYPE_OBJECT, filter, writeStart);
		}

		/*
		/**********************************************************
		/* State changes
		/**********************************************************
		*/
		/// <exception cref="com.fasterxml.jackson.core.JsonProcessingException"/>
		public virtual com.fasterxml.jackson.core.filter.TokenFilter setFieldName(string 
			name)
		{
			_currentName = name;
			_needToHandleName = true;
			return _filter;
		}

		/// <summary>
		/// Method called to check whether value is to be included at current output
		/// position, either as Object property, Array element, or root value.
		/// </summary>
		public virtual com.fasterxml.jackson.core.filter.TokenFilter checkValue(com.fasterxml.jackson.core.filter.TokenFilter
			 filter)
		{
			// First, checks for Object properties have been made earlier:
			if (_type == TYPE_OBJECT)
			{
				return filter;
			}
			// We increase it first because at the beginning of array, value is -1
			int ix = ++_index;
			if (_type == TYPE_ARRAY)
			{
				return filter.includeElement(ix);
			}
			return filter.includeRootValue(ix);
		}

		/// <summary>
		/// Method called to ensure that parent path from root is written up to
		/// and including this node.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		public virtual void writePath(com.fasterxml.jackson.core.JsonGenerator gen)
		{
			if ((_filter == null) || (_filter == com.fasterxml.jackson.core.filter.TokenFilter
				.INCLUDE_ALL))
			{
				return;
			}
			if (_parent != null)
			{
				_parent._writePath(gen);
			}
			if (_startHandled)
			{
				// even if Object started, need to start leaf-level name
				if (_needToHandleName)
				{
					gen.writeFieldName(_currentName);
				}
			}
			else
			{
				_startHandled = true;
				if (_type == TYPE_OBJECT)
				{
					gen.writeStartObject();
					gen.writeFieldName(_currentName);
				}
				else
				{
					// we know name must be written
					if (_type == TYPE_ARRAY)
					{
						gen.writeStartArray();
					}
				}
			}
		}

		/// <summary>
		/// Variant of
		/// <see cref="writePath(com.fasterxml.jackson.core.JsonGenerator)"/>
		/// called when all we
		/// need is immediately surrounding Object. Method typically called
		/// when including a single property but not including full path
		/// to root.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		public virtual void writeImmediatePath(com.fasterxml.jackson.core.JsonGenerator gen
			)
		{
			if ((_filter == null) || (_filter == com.fasterxml.jackson.core.filter.TokenFilter
				.INCLUDE_ALL))
			{
				return;
			}
			if (_startHandled)
			{
				// even if Object started, need to start leaf-level name
				if (_needToHandleName)
				{
					gen.writeFieldName(_currentName);
				}
			}
			else
			{
				_startHandled = true;
				if (_type == TYPE_OBJECT)
				{
					gen.writeStartObject();
					if (_needToHandleName)
					{
						gen.writeFieldName(_currentName);
					}
				}
				else
				{
					if (_type == TYPE_ARRAY)
					{
						gen.writeStartArray();
					}
				}
			}
		}

		/// <exception cref="System.IO.IOException"/>
		private void _writePath(com.fasterxml.jackson.core.JsonGenerator gen)
		{
			if ((_filter == null) || (_filter == com.fasterxml.jackson.core.filter.TokenFilter
				.INCLUDE_ALL))
			{
				return;
			}
			if (_parent != null)
			{
				_parent._writePath(gen);
			}
			if (_startHandled)
			{
				// even if Object started, need to start leaf-level name
				if (_needToHandleName)
				{
					_needToHandleName = false;
					// at parent must explicitly clear
					gen.writeFieldName(_currentName);
				}
			}
			else
			{
				_startHandled = true;
				if (_type == TYPE_OBJECT)
				{
					gen.writeStartObject();
					if (_needToHandleName)
					{
						_needToHandleName = false;
						// at parent must explicitly clear
						gen.writeFieldName(_currentName);
					}
				}
				else
				{
					if (_type == TYPE_ARRAY)
					{
						gen.writeStartArray();
					}
				}
			}
		}

		/// <exception cref="System.IO.IOException"/>
		public virtual com.fasterxml.jackson.core.filter.TokenFilterContext closeArray(com.fasterxml.jackson.core.JsonGenerator
			 gen)
		{
			if (_startHandled)
			{
				gen.writeEndArray();
			}
			if ((_filter != null) && (_filter != com.fasterxml.jackson.core.filter.TokenFilter
				.INCLUDE_ALL))
			{
				_filter.filterFinishArray();
			}
			return _parent;
		}

		/// <exception cref="System.IO.IOException"/>
		public virtual com.fasterxml.jackson.core.filter.TokenFilterContext closeObject(com.fasterxml.jackson.core.JsonGenerator
			 gen)
		{
			if (_startHandled)
			{
				gen.writeEndObject();
			}
			if ((_filter != null) && (_filter != com.fasterxml.jackson.core.filter.TokenFilter
				.INCLUDE_ALL))
			{
				_filter.filterFinishObject();
			}
			return _parent;
		}

		public virtual void skipParentChecks()
		{
			_filter = null;
			for (com.fasterxml.jackson.core.filter.TokenFilterContext ctxt = _parent; ctxt !=
				 null; ctxt = ctxt._parent)
			{
				_parent._filter = null;
			}
		}

		/*
		/**********************************************************
		/* Accessors, mutators
		/**********************************************************
		*/
		public override object getCurrentValue()
		{
			return null;
		}

		public override void setCurrentValue(object v)
		{
		}

		public sealed override com.fasterxml.jackson.core.JsonStreamContext getParent()
		{
			return _parent;
		}

		public sealed override string getCurrentName()
		{
			return _currentName;
		}

		public virtual com.fasterxml.jackson.core.filter.TokenFilter getFilter()
		{
			return _filter;
		}

		public virtual bool isStartHandled()
		{
			return _startHandled;
		}

		public virtual com.fasterxml.jackson.core.JsonToken nextTokenToRead()
		{
			if (!_startHandled)
			{
				_startHandled = true;
				if (_type == TYPE_OBJECT)
				{
					return com.fasterxml.jackson.core.JsonToken.START_OBJECT;
				}
				// Note: root should never be unhandled
				return com.fasterxml.jackson.core.JsonToken.START_ARRAY;
			}
			// But otherwise at most might have FIELD_NAME
			if (_needToHandleName && (_type == TYPE_OBJECT))
			{
				_needToHandleName = false;
				return com.fasterxml.jackson.core.JsonToken.FIELD_NAME;
			}
			return null;
		}

		public virtual com.fasterxml.jackson.core.filter.TokenFilterContext findChildOf(com.fasterxml.jackson.core.filter.TokenFilterContext
			 parent)
		{
			if (_parent == parent)
			{
				return this;
			}
			com.fasterxml.jackson.core.filter.TokenFilterContext curr = _parent;
			while (curr != null)
			{
				com.fasterxml.jackson.core.filter.TokenFilterContext p = curr._parent;
				if (p == parent)
				{
					return curr;
				}
				curr = p;
			}
			// should never occur but...
			return null;
		}

		// // // Internally used abstract methods
		protected internal virtual void appendDesc(System.Text.StringBuilder sb)
		{
			if (_parent != null)
			{
				_parent.appendDesc(sb);
			}
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
