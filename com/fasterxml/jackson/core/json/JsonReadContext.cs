using Sharpen;

namespace com.fasterxml.jackson.core.json
{
	/// <summary>
	/// Extension of
	/// <see cref="com.fasterxml.jackson.core.JsonStreamContext"/>
	/// , which implements
	/// core methods needed, and also exposes
	/// more complete API to parser implementation classes.
	/// </summary>
	public sealed class JsonReadContext : com.fasterxml.jackson.core.JsonStreamContext
	{
		/// <summary>Parent context for this context; null for root context.</summary>
		protected internal readonly com.fasterxml.jackson.core.json.JsonReadContext _parent;

		protected internal com.fasterxml.jackson.core.json.DupDetector _dups;

		protected internal com.fasterxml.jackson.core.json.JsonReadContext _child = null;

		protected internal string _currentName;

		/// <since>2.5</since>
		protected internal object _currentValue;

		protected internal int _lineNr;

		protected internal int _columnNr;

		public JsonReadContext(com.fasterxml.jackson.core.json.JsonReadContext parent, com.fasterxml.jackson.core.json.DupDetector
			 dups, int type, int lineNr, int colNr)
			: base()
		{
			// // // Configuration
			// // // Optional duplicate detection
			/*
			/**********************************************************
			/* Simple instance reuse slots; speeds up things
			/* a bit (10-15%) for docs with lots of small
			/* arrays/objects (for which allocation was
			/* visible in profile stack frames)
			/**********************************************************
			*/
			/*
			/**********************************************************
			/* Location/state information (minus source reference)
			/**********************************************************
			*/
			/*
			/**********************************************************
			/* Instance construction, config, reuse
			/**********************************************************
			*/
			_parent = parent;
			_dups = dups;
			_type = type;
			_lineNr = lineNr;
			_columnNr = colNr;
			_index = -1;
		}

		protected internal void reset(int type, int lineNr, int colNr)
		{
			_type = type;
			_index = -1;
			_lineNr = lineNr;
			_columnNr = colNr;
			_currentName = null;
			_currentValue = null;
			if (_dups != null)
			{
				_dups.reset();
			}
		}

		/*
		public void trackDups(JsonParser jp) {
		_dups = DupDetector.rootDetector(jp);
		}
		*/
		public com.fasterxml.jackson.core.json.JsonReadContext withDupDetector(com.fasterxml.jackson.core.json.DupDetector
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
		[System.Obsolete]
		public static com.fasterxml.jackson.core.json.JsonReadContext createRootContext(int
			 lineNr, int colNr)
		{
			// since 2.3, use variant that takes dup detector
			return createRootContext(lineNr, colNr, null);
		}

		public static com.fasterxml.jackson.core.json.JsonReadContext createRootContext(int
			 lineNr, int colNr, com.fasterxml.jackson.core.json.DupDetector dups)
		{
			return new com.fasterxml.jackson.core.json.JsonReadContext(null, dups, TYPE_ROOT, 
				lineNr, colNr);
		}

		[System.Obsolete]
		public static com.fasterxml.jackson.core.json.JsonReadContext createRootContext()
		{
			// since 2.3, use variant that takes dup detector
			return createRootContext(null);
		}

		public static com.fasterxml.jackson.core.json.JsonReadContext createRootContext(com.fasterxml.jackson.core.json.DupDetector
			 dups)
		{
			return new com.fasterxml.jackson.core.json.JsonReadContext(null, dups, TYPE_ROOT, 
				1, 0);
		}

		public com.fasterxml.jackson.core.json.JsonReadContext createChildArrayContext(int
			 lineNr, int colNr)
		{
			com.fasterxml.jackson.core.json.JsonReadContext ctxt = _child;
			if (ctxt == null)
			{
				_child = ctxt = new com.fasterxml.jackson.core.json.JsonReadContext(this, (_dups 
					== null) ? null : _dups.child(), TYPE_ARRAY, lineNr, colNr);
			}
			else
			{
				ctxt.reset(TYPE_ARRAY, lineNr, colNr);
			}
			return ctxt;
		}

		public com.fasterxml.jackson.core.json.JsonReadContext createChildObjectContext(int
			 lineNr, int colNr)
		{
			com.fasterxml.jackson.core.json.JsonReadContext ctxt = _child;
			if (ctxt == null)
			{
				_child = ctxt = new com.fasterxml.jackson.core.json.JsonReadContext(this, (_dups 
					== null) ? null : _dups.child(), TYPE_OBJECT, lineNr, colNr);
				return ctxt;
			}
			ctxt.reset(TYPE_OBJECT, lineNr, colNr);
			return ctxt;
		}

		/*
		/**********************************************************
		/* Abstract method implementation
		/**********************************************************
		*/
		public override string getCurrentName()
		{
			return _currentName;
		}

		public override com.fasterxml.jackson.core.JsonStreamContext getParent()
		{
			return _parent;
		}

		/*
		/**********************************************************
		/* Extended API
		/**********************************************************
		*/
		/// <returns>
		/// Location pointing to the point where the context
		/// start marker was found
		/// </returns>
		public com.fasterxml.jackson.core.JsonLocation getStartLocation(object srcRef)
		{
			// We don't keep track of offsets at this level (only reader does)
			long totalChars = -1L;
			return new com.fasterxml.jackson.core.JsonLocation(srcRef, totalChars, _lineNr, _columnNr
				);
		}

		public com.fasterxml.jackson.core.json.DupDetector getDupDetector()
		{
			return _dups;
		}

		/*
		/**********************************************************
		/* State changes
		/**********************************************************
		*/
		public bool expectComma()
		{
			/* Assumption here is that we will be getting a value (at least
			* before calling this method again), and
			* so will auto-increment index to avoid having to do another call
			*/
			int ix = ++_index;
			// starts from -1
			return (_type != TYPE_ROOT && ix > 0);
		}

		/// <exception cref="com.fasterxml.jackson.core.JsonProcessingException"/>
		public void setCurrentName(string name)
		{
			_currentName = name;
			if (_dups != null)
			{
				_checkDup(_dups, name);
			}
		}

		/// <exception cref="com.fasterxml.jackson.core.JsonProcessingException"/>
		private void _checkDup(com.fasterxml.jackson.core.json.DupDetector dd, string name
			)
		{
			if (dd.isDup(name))
			{
				throw new com.fasterxml.jackson.core.JsonParseException("Duplicate field '" + name
					 + "'", dd.findLocation());
			}
		}

		/*
		/**********************************************************
		/* Overridden standard methods
		/**********************************************************
		*/
		/// <summary>
		/// Overridden to provide developer readable "JsonPath" representation
		/// of the context.
		/// </summary>
		public override string ToString()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder(64);
			switch (_type)
			{
				case TYPE_ROOT:
				{
					sb.Append("/");
					break;
				}

				case TYPE_ARRAY:
				{
					sb.Append('[');
					sb.Append(getCurrentIndex());
					sb.Append(']');
					break;
				}

				case TYPE_OBJECT:
				{
					sb.Append('{');
					if (_currentName != null)
					{
						sb.Append('"');
						com.fasterxml.jackson.core.io.CharTypes.appendQuoted(sb, _currentName);
						sb.Append('"');
					}
					else
					{
						sb.Append('?');
					}
					sb.Append('}');
					break;
				}
			}
			return sb.ToString();
		}
	}
}
