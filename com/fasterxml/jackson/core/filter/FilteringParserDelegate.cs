using Sharpen;

namespace com.fasterxml.jackson.core.filter
{
	/// <summary>
	/// Specialized
	/// <see cref="com.fasterxml.jackson.core.util.JsonParserDelegate"/>
	/// that allows use of
	/// <see cref="TokenFilter"/>
	/// for outputting a subset of content that
	/// is visible to caller
	/// </summary>
	/// <since>2.6</since>
	public class FilteringParserDelegate : com.fasterxml.jackson.core.util.JsonParserDelegate
	{
		/// <summary>
		/// Object consulted to determine whether to write parts of content generator
		/// is asked to write or not.
		/// </summary>
		protected internal com.fasterxml.jackson.core.filter.TokenFilter rootFilter;

		/// <summary>
		/// Flag that determines whether filtering will continue after the first
		/// match is indicated or not: if `false`, output is based on just the first
		/// full match (returning
		/// <see cref="TokenFilter.INCLUDE_ALL"/>
		/// ) and no more
		/// checks are made; if `true` then filtering will be applied as necessary
		/// until end of content.
		/// </summary>
		protected internal bool _allowMultipleMatches;

		/// <summary>
		/// Flag that determines whether path leading up to included content should
		/// also be automatically included or not.
		/// </summary>
		/// <remarks>
		/// Flag that determines whether path leading up to included content should
		/// also be automatically included or not. If `false`, no path inclusion is
		/// done and only explicitly included entries are output; if `true` then
		/// path from main level down to match is also included as necessary.
		/// </remarks>
		protected internal bool _includePath;

		[System.Obsolete]
		protected internal bool _includeImmediateParent = false;

		/// <summary>
		/// Last token retrieved via
		/// <see cref="nextToken()"/>
		/// , if any.
		/// Null before the first call to <code>nextToken()</code>,
		/// as well as if token has been explicitly cleared
		/// </summary>
		protected internal com.fasterxml.jackson.core.JsonToken _currToken;

		/// <summary>
		/// Last cleared token, if any: that is, value that was in
		/// effect when
		/// <see cref="clearCurrentToken()"/>
		/// was called.
		/// </summary>
		protected internal com.fasterxml.jackson.core.JsonToken _lastClearedToken;

		/// <summary>
		/// During traversal this is the actual "open" parse tree, which sometimes
		/// is the same as
		/// <see cref="_exposedContext"/>
		/// , and at other times is ahead
		/// of it. Note that this context is never null.
		/// </summary>
		protected internal com.fasterxml.jackson.core.filter.TokenFilterContext _headContext;

		/// <summary>
		/// In cases where
		/// <see cref="_headContext"/>
		/// is "ahead" of context exposed to
		/// caller, this context points to what is currently exposed to caller.
		/// When the two are in sync, this context reference will be <code>null</code>.
		/// </summary>
		protected internal com.fasterxml.jackson.core.filter.TokenFilterContext _exposedContext;

		/// <summary>State that applies to the item within container, used where applicable.</summary>
		/// <remarks>
		/// State that applies to the item within container, used where applicable.
		/// Specifically used to pass inclusion state between property name and
		/// property, and also used for array elements.
		/// </remarks>
		protected internal com.fasterxml.jackson.core.filter.TokenFilter _itemFilter;

		/// <summary>
		/// Number of tokens for which
		/// <see cref="TokenFilter.INCLUDE_ALL"/>
		/// has been returned.
		/// </summary>
		protected internal int _matchCount;

		public FilteringParserDelegate(com.fasterxml.jackson.core.JsonParser p, com.fasterxml.jackson.core.filter.TokenFilter
			 f, bool includePath, bool allowMultipleMatches)
			: base(p)
		{
			/*
			/**********************************************************
			/* Configuration
			/**********************************************************
			*/
			/* NOTE: this feature is included in the first version (2.6), but
			* there is no public API to enable it, yet, since there isn't an
			* actual use case. But it seemed possible need could arise, which
			* is feature has not yet been removed. If no use is found within
			* first version or two, just remove.
			*
			* Marked as deprecated since its status is uncertain.
			*/
			/*
			/**********************************************************
			/* State
			/**********************************************************
			*/
			/*
			/**********************************************************
			/* Construction, initialization
			/**********************************************************
			*/
			rootFilter = f;
			// and this is the currently active filter for root values
			_itemFilter = f;
			_headContext = com.fasterxml.jackson.core.filter.TokenFilterContext.createRootContext
				(f);
			_includePath = includePath;
			_allowMultipleMatches = allowMultipleMatches;
		}

		/*
		/**********************************************************
		/* Extended API
		/**********************************************************
		*/
		public virtual com.fasterxml.jackson.core.filter.TokenFilter getFilter()
		{
			return rootFilter;
		}

		/// <summary>
		/// Accessor for finding number of matches, where specific token and sub-tree
		/// starting (if structured type) are passed.
		/// </summary>
		public virtual int getMatchCount()
		{
			return _matchCount;
		}

		/*
		/**********************************************************
		/* Public API, token accessors
		/**********************************************************
		*/
		public override com.fasterxml.jackson.core.JsonToken getCurrentToken()
		{
			return _currToken;
		}

		public sealed override int getCurrentTokenId()
		{
			com.fasterxml.jackson.core.JsonToken t = _currToken;
			return (t == null) ? JsonTokenIdConstants.ID_NO_TOKEN : t.id();
		}

		public override bool hasCurrentToken()
		{
			return _currToken != null;
		}

		public override bool hasTokenId(int id)
		{
			com.fasterxml.jackson.core.JsonToken t = _currToken;
			if (t == null)
			{
				return (JsonTokenIdConstants.ID_NO_TOKEN == id);
			}
			return t.id() == id;
		}

		public sealed override bool hasToken(com.fasterxml.jackson.core.JsonToken t)
		{
			return (_currToken == t);
		}

		public override bool isExpectedStartArrayToken()
		{
			return _currToken == com.fasterxml.jackson.core.JsonToken.START_ARRAY;
		}

		public override bool isExpectedStartObjectToken()
		{
			return _currToken == com.fasterxml.jackson.core.JsonToken.START_OBJECT;
		}

		public override com.fasterxml.jackson.core.JsonLocation getCurrentLocation()
		{
			return delegate_.getCurrentLocation();
		}

		public override com.fasterxml.jackson.core.JsonStreamContext getParsingContext()
		{
			return _filterContext();
		}

		// !!! TODO: Verify it works as expected: copied from standard JSON parser impl
		/// <exception cref="System.IO.IOException"/>
		public override string getCurrentName()
		{
			com.fasterxml.jackson.core.JsonStreamContext ctxt = _filterContext();
			if (_currToken == com.fasterxml.jackson.core.JsonToken.START_OBJECT || _currToken
				 == com.fasterxml.jackson.core.JsonToken.START_ARRAY)
			{
				com.fasterxml.jackson.core.JsonStreamContext parent = ctxt.getParent();
				return (parent == null) ? null : parent.getCurrentName();
			}
			return ctxt.getCurrentName();
		}

		/*
		/**********************************************************
		/* Public API, token state overrides
		/**********************************************************
		*/
		public override void clearCurrentToken()
		{
			if (_currToken != null)
			{
				_lastClearedToken = _currToken;
				_currToken = null;
			}
		}

		public override com.fasterxml.jackson.core.JsonToken getLastClearedToken()
		{
			return _lastClearedToken;
		}

		public override void overrideCurrentName(string name)
		{
			/* 14-Apr-2015, tatu: Not sure whether this can be supported, and if so,
			*    what to do with it... Delegation won't work for sure, so let's for
			*    now throw an exception
			*/
			throw new System.NotSupportedException("Can not currently override name during filtering read"
				);
		}

		/*
		/**********************************************************
		/* Public API, traversal
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		public override com.fasterxml.jackson.core.JsonToken nextToken()
		{
			// Anything buffered?
			com.fasterxml.jackson.core.filter.TokenFilterContext ctxt = _exposedContext;
			if (ctxt != null)
			{
				while (true)
				{
					com.fasterxml.jackson.core.JsonToken t = ctxt.nextTokenToRead();
					if (t != null)
					{
						_currToken = t;
						return t;
					}
					// all done with buffered stuff?
					if (ctxt == _headContext)
					{
						_exposedContext = null;
						if (ctxt.inArray())
						{
							t = delegate_.getCurrentToken();
							// Is this guaranteed to work without further checks?
							//                        if (t != JsonToken.START_ARRAY) {
							_currToken = t;
							return t;
						}
						// Almost! Most likely still have the current token;
						// with the sole exception of 
						/*
						t = delegate.getCurrentToken();
						if (t != JsonToken.FIELD_NAME) {
						_currToken = t;
						return t;
						}
						*/
						break;
					}
					// If not, traverse down the context chain
					ctxt = _headContext.findChildOf(ctxt);
					_exposedContext = ctxt;
					if (ctxt == null)
					{
						// should never occur
						throw _constructError("Unexpected problem: chain of filtered context broken");
					}
				}
			}
			// If not, need to read more. If we got any:
			com.fasterxml.jackson.core.JsonToken t_1 = delegate_.nextToken();
			if (t_1 == null)
			{
				// no strict need to close, since we have no state here
				return (_currToken = t_1);
			}
			// otherwise... to include or not?
			com.fasterxml.jackson.core.filter.TokenFilter f;
			switch (t_1.id())
			{
				case JsonTokenIdConstants.ID_START_ARRAY:
				{
					f = _itemFilter;
					if (f == com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
					{
						_headContext = _headContext.createChildArrayContext(f, true);
						return (_currToken = t_1);
					}
					if (f == null)
					{
						// does this occur?
						delegate_.skipChildren();
						break;
					}
					// Otherwise still iffy, need to check
					f = _headContext.checkValue(f);
					if (f == null)
					{
						delegate_.skipChildren();
						break;
					}
					if (f != com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
					{
						f = f.filterStartArray();
					}
					_itemFilter = f;
					if (f == com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
					{
						_headContext = _headContext.createChildArrayContext(f, true);
						return (_currToken = t_1);
					}
					_headContext = _headContext.createChildArrayContext(f, false);
					// Also: only need buffering if parent path to be included
					if (_includePath)
					{
						t_1 = _nextTokenWithBuffering(_headContext);
						if (t_1 != null)
						{
							_currToken = t_1;
							return t_1;
						}
					}
					break;
				}

				case JsonTokenIdConstants.ID_START_OBJECT:
				{
					f = _itemFilter;
					if (f == com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
					{
						_headContext = _headContext.createChildObjectContext(f, true);
						return (_currToken = t_1);
					}
					if (f == null)
					{
						// does this occur?
						delegate_.skipChildren();
						break;
					}
					// Otherwise still iffy, need to check
					f = _headContext.checkValue(f);
					if (f == null)
					{
						delegate_.skipChildren();
						break;
					}
					if (f != com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
					{
						f = f.filterStartObject();
					}
					_itemFilter = f;
					if (f == com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
					{
						_headContext = _headContext.createChildObjectContext(f, true);
						return (_currToken = t_1);
					}
					_headContext = _headContext.createChildObjectContext(f, false);
					// Also: only need buffering if parent path to be included
					if (_includePath)
					{
						t_1 = _nextTokenWithBuffering(_headContext);
						if (t_1 != null)
						{
							_currToken = t_1;
							return t_1;
						}
					}
					// note: inclusion of surrounding Object handled separately via
					// FIELD_NAME
					break;
				}

				case JsonTokenIdConstants.ID_END_ARRAY:
				case JsonTokenIdConstants.ID_END_OBJECT:
				{
					bool returnEnd = _headContext.isStartHandled();
					f = _headContext.getFilter();
					if ((f != null) && (f != com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL
						))
					{
						f.filterFinishArray();
					}
					_headContext = ((com.fasterxml.jackson.core.filter.TokenFilterContext)_headContext
						.getParent());
					_itemFilter = _headContext.getFilter();
					if (returnEnd)
					{
						return (_currToken = t_1);
					}
					break;
				}

				case JsonTokenIdConstants.ID_FIELD_NAME:
				{
					string name = delegate_.getCurrentName();
					// note: this will also set 'needToHandleName'
					f = _headContext.setFieldName(name);
					if (f == com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
					{
						_itemFilter = f;
						if (!_includePath)
						{
							// Minor twist here: if parent NOT included, may need to induce output of
							// surrounding START_OBJECT/END_OBJECT
							if (_includeImmediateParent && !_headContext.isStartHandled())
							{
								t_1 = _headContext.nextTokenToRead();
								// returns START_OBJECT but also marks it handled
								_exposedContext = _headContext;
							}
						}
						return (_currToken = t_1);
					}
					if (f == null)
					{
						delegate_.nextToken();
						delegate_.skipChildren();
						break;
					}
					f = f.includeProperty(name);
					if (f == null)
					{
						delegate_.nextToken();
						delegate_.skipChildren();
						break;
					}
					_itemFilter = f;
					if (f == com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
					{
						if (_includePath)
						{
							return (_currToken = t_1);
						}
					}
					if (_includePath)
					{
						t_1 = _nextTokenWithBuffering(_headContext);
						if (t_1 != null)
						{
							_currToken = t_1;
							return t_1;
						}
					}
					break;
				}

				default:
				{
					// scalar value
					f = _itemFilter;
					if (f == com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
					{
						return (_currToken = t_1);
					}
					if (f != null)
					{
						f = _headContext.checkValue(f);
						if ((f == com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL) || ((f != null
							) && f.includeValue(delegate_)))
						{
							return (_currToken = t_1);
						}
					}
					// Otherwise not included (leaves must be explicitly included)
					break;
				}
			}
			// We get here if token was not yet found; offlined handling
			return _nextToken2();
		}

		/// <summary>
		/// Offlined handling for cases where there was no buffered token to
		/// return, and the token read next could not be returned as-is,
		/// at least not yet, but where we have not yet established that
		/// buffering is needed.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		protected internal com.fasterxml.jackson.core.JsonToken _nextToken2()
		{
			while (true)
			{
				com.fasterxml.jackson.core.JsonToken t = delegate_.nextToken();
				if (t == null)
				{
					// is this even legal?
					return (_currToken = t);
				}
				com.fasterxml.jackson.core.filter.TokenFilter f;
				switch (t.id())
				{
					case JsonTokenIdConstants.ID_START_ARRAY:
					{
						f = _itemFilter;
						if (f == com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
						{
							_headContext = _headContext.createChildArrayContext(f, true);
							return (_currToken = t);
						}
						if (f == null)
						{
							// does this occur?
							delegate_.skipChildren();
							goto main_loop_continue;
						}
						// Otherwise still iffy, need to check
						f = _headContext.checkValue(f);
						if (f == null)
						{
							delegate_.skipChildren();
							goto main_loop_continue;
						}
						if (f != com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
						{
							f = f.filterStartArray();
						}
						_itemFilter = f;
						if (f == com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
						{
							_headContext = _headContext.createChildArrayContext(f, true);
							return (_currToken = t);
						}
						_headContext = _headContext.createChildArrayContext(f, false);
						// but if we didn't figure it out yet, need to buffer possible events
						if (_includePath)
						{
							t = _nextTokenWithBuffering(_headContext);
							if (t != null)
							{
								_currToken = t;
								return t;
							}
						}
						goto main_loop_continue;
					}

					case JsonTokenIdConstants.ID_START_OBJECT:
					{
						f = _itemFilter;
						if (f == com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
						{
							_headContext = _headContext.createChildObjectContext(f, true);
							return (_currToken = t);
						}
						if (f == null)
						{
							// does this occur?
							delegate_.skipChildren();
							goto main_loop_continue;
						}
						// Otherwise still iffy, need to check
						f = _headContext.checkValue(f);
						if (f == null)
						{
							delegate_.skipChildren();
							goto main_loop_continue;
						}
						if (f != com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
						{
							f = f.filterStartObject();
						}
						_itemFilter = f;
						if (f == com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
						{
							_headContext = _headContext.createChildObjectContext(f, true);
							return (_currToken = t);
						}
						_headContext = _headContext.createChildObjectContext(f, false);
						if (_includePath)
						{
							t = _nextTokenWithBuffering(_headContext);
							if (t != null)
							{
								_currToken = t;
								return t;
							}
						}
						goto main_loop_continue;
					}

					case JsonTokenIdConstants.ID_END_ARRAY:
					case JsonTokenIdConstants.ID_END_OBJECT:
					{
						bool returnEnd = _headContext.isStartHandled();
						f = _headContext.getFilter();
						if ((f != null) && (f != com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL
							))
						{
							f.filterFinishArray();
						}
						_headContext = ((com.fasterxml.jackson.core.filter.TokenFilterContext)_headContext
							.getParent());
						_itemFilter = _headContext.getFilter();
						if (returnEnd)
						{
							return (_currToken = t);
						}
						goto main_loop_continue;
					}

					case JsonTokenIdConstants.ID_FIELD_NAME:
					{
						string name = delegate_.getCurrentName();
						f = _headContext.setFieldName(name);
						if (f == com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
						{
							_itemFilter = f;
							return (_currToken = t);
						}
						if (f == null)
						{
							// filter out the value
							delegate_.nextToken();
							delegate_.skipChildren();
							goto main_loop_continue;
						}
						f = f.includeProperty(name);
						if (f == null)
						{
							// filter out the value
							delegate_.nextToken();
							delegate_.skipChildren();
							goto main_loop_continue;
						}
						_itemFilter = f;
						if (f == com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
						{
							if (_includePath)
							{
								return (_currToken = t);
							}
							//                        if (_includeImmediateParent) { ...
							goto main_loop_continue;
						}
						if (_includePath)
						{
							t = _nextTokenWithBuffering(_headContext);
							if (t != null)
							{
								_currToken = t;
								return t;
							}
						}
						goto main_loop_continue;
					}

					default:
					{
						// scalar value
						f = _itemFilter;
						if (f == com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
						{
							return (_currToken = t);
						}
						if (f != null)
						{
							f = _headContext.checkValue(f);
							if ((f == com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL) || ((f != null
								) && f.includeValue(delegate_)))
							{
								return (_currToken = t);
							}
						}
						// Otherwise not included (leaves must be explicitly included)
						break;
					}
				}
main_loop_continue: ;
			}
main_loop_break: ;
		}

		/// <summary>Method called when a new potentially included context is found.</summary>
		/// <exception cref="System.IO.IOException"/>
		protected internal com.fasterxml.jackson.core.JsonToken _nextTokenWithBuffering(com.fasterxml.jackson.core.filter.TokenFilterContext
			 buffRoot)
		{
			while (true)
			{
				com.fasterxml.jackson.core.JsonToken t = delegate_.nextToken();
				if (t == null)
				{
					// is this even legal?
					return t;
				}
				com.fasterxml.jackson.core.filter.TokenFilter f;
				switch (t.id())
				{
					case JsonTokenIdConstants.ID_START_ARRAY:
					{
						// One simplification here: we know for a fact that the item filter is
						// neither null nor 'include all', for most cases; the only exception
						// being FIELD_NAME handling
						f = _headContext.checkValue(_itemFilter);
						if (f == null)
						{
							delegate_.skipChildren();
							goto main_loop_continue;
						}
						if (f != com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
						{
							f = f.filterStartArray();
						}
						_itemFilter = f;
						if (f == com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
						{
							_headContext = _headContext.createChildArrayContext(f, true);
							return _nextBuffered(buffRoot);
						}
						_headContext = _headContext.createChildArrayContext(f, false);
						goto main_loop_continue;
					}

					case JsonTokenIdConstants.ID_START_OBJECT:
					{
						f = _itemFilter;
						if (f == com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
						{
							_headContext = _headContext.createChildObjectContext(f, true);
							return t;
						}
						if (f == null)
						{
							// does this occur?
							delegate_.skipChildren();
							goto main_loop_continue;
						}
						// Otherwise still iffy, need to check
						f = _headContext.checkValue(f);
						if (f == null)
						{
							delegate_.skipChildren();
							goto main_loop_continue;
						}
						if (f != com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
						{
							f = f.filterStartObject();
						}
						_itemFilter = f;
						if (f == com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
						{
							_headContext = _headContext.createChildObjectContext(f, true);
							return _nextBuffered(buffRoot);
						}
						_headContext = _headContext.createChildObjectContext(f, false);
						goto main_loop_continue;
					}

					case JsonTokenIdConstants.ID_END_ARRAY:
					case JsonTokenIdConstants.ID_END_OBJECT:
					{
						// Unlike with other loops, here we know that content was NOT
						// included (won't get this far otherwise)
						f = _headContext.getFilter();
						if ((f != null) && (f != com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL
							))
						{
							f.filterFinishArray();
						}
						bool gotEnd = (_headContext == buffRoot);
						bool returnEnd = gotEnd && _headContext.isStartHandled();
						_headContext = ((com.fasterxml.jackson.core.filter.TokenFilterContext)_headContext
							.getParent());
						_itemFilter = _headContext.getFilter();
						if (returnEnd)
						{
							return t;
						}
						// Hmmh. Do we need both checks, or should above suffice?
						if (gotEnd || (_headContext == buffRoot))
						{
							return null;
						}
						goto main_loop_continue;
					}

					case JsonTokenIdConstants.ID_FIELD_NAME:
					{
						string name = delegate_.getCurrentName();
						f = _headContext.setFieldName(name);
						if (f == com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
						{
							_itemFilter = f;
							return _nextBuffered(buffRoot);
						}
						if (f == null)
						{
							// filter out the value
							delegate_.nextToken();
							delegate_.skipChildren();
							goto main_loop_continue;
						}
						f = f.includeProperty(name);
						if (f == null)
						{
							// filter out the value
							delegate_.nextToken();
							delegate_.skipChildren();
							goto main_loop_continue;
						}
						_itemFilter = f;
						if (f == com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
						{
							return _nextBuffered(buffRoot);
						}
						goto main_loop_continue;
					}

					default:
					{
						// scalar value
						f = _itemFilter;
						if (f == com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
						{
							return _nextBuffered(buffRoot);
						}
						if (f != null)
						{
							f = _headContext.checkValue(f);
							if ((f == com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL) || ((f != null
								) && f.includeValue(delegate_)))
							{
								return _nextBuffered(buffRoot);
							}
						}
						// Otherwise not included (leaves must be explicitly included)
						goto main_loop_continue;
					}
				}
main_loop_continue: ;
			}
main_loop_break: ;
		}

		/// <exception cref="System.IO.IOException"/>
		private com.fasterxml.jackson.core.JsonToken _nextBuffered(com.fasterxml.jackson.core.filter.TokenFilterContext
			 buffRoot)
		{
			_exposedContext = buffRoot;
			com.fasterxml.jackson.core.filter.TokenFilterContext ctxt = buffRoot;
			com.fasterxml.jackson.core.JsonToken t = ctxt.nextTokenToRead();
			if (t != null)
			{
				return t;
			}
			while (true)
			{
				// all done with buffered stuff?
				if (ctxt == _headContext)
				{
					throw _constructError("Internal error: failed to locate expected buffered tokens"
						);
				}
				/*
				_exposedContext = null;
				break;
				*/
				// If not, traverse down the context chain
				ctxt = _exposedContext.findChildOf(ctxt);
				_exposedContext = ctxt;
				if (ctxt == null)
				{
					// should never occur
					throw _constructError("Unexpected problem: chain of filtered context broken");
				}
				t = _exposedContext.nextTokenToRead();
				if (t != null)
				{
					return t;
				}
			}
		}

		/// <exception cref="System.IO.IOException"/>
		public override com.fasterxml.jackson.core.JsonToken nextValue()
		{
			// Re-implemented same as ParserMinimalBase:
			com.fasterxml.jackson.core.JsonToken t = nextToken();
			if (t == com.fasterxml.jackson.core.JsonToken.FIELD_NAME)
			{
				t = nextToken();
			}
			return t;
		}

		/// <summary>
		/// Need to override, re-implement similar to how method defined in
		/// <see cref="com.fasterxml.jackson.core.@base.ParserMinimalBase"/>
		/// , to keep
		/// state correct here.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		public override com.fasterxml.jackson.core.JsonParser skipChildren()
		{
			if ((_currToken != com.fasterxml.jackson.core.JsonToken.START_OBJECT) && (_currToken
				 != com.fasterxml.jackson.core.JsonToken.START_ARRAY))
			{
				return this;
			}
			int open = 1;
			// Since proper matching of start/end markers is handled
			// by nextToken(), we'll just count nesting levels here
			while (true)
			{
				com.fasterxml.jackson.core.JsonToken t = nextToken();
				if (t == null)
				{
					// not ideal but for now, just return
					return this;
				}
				if (t.isStructStart())
				{
					++open;
				}
				else
				{
					if (t.isStructEnd())
					{
						if (--open == 0)
						{
							return this;
						}
					}
				}
			}
		}

		/*
		/**********************************************************
		/* Public API, access to token information, text
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		public override string getText()
		{
			return delegate_.getText();
		}

		public override bool hasTextCharacters()
		{
			return delegate_.hasTextCharacters();
		}

		/// <exception cref="System.IO.IOException"/>
		public override char[] getTextCharacters()
		{
			return delegate_.getTextCharacters();
		}

		/// <exception cref="System.IO.IOException"/>
		public override int getTextLength()
		{
			return delegate_.getTextLength();
		}

		/// <exception cref="System.IO.IOException"/>
		public override int getTextOffset()
		{
			return delegate_.getTextOffset();
		}

		/*
		/**********************************************************
		/* Public API, access to token information, numeric
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		public override System.Numerics.BigInteger getBigIntegerValue()
		{
			return delegate_.getBigIntegerValue();
		}

		/// <exception cref="System.IO.IOException"/>
		public override bool getBooleanValue()
		{
			return delegate_.getBooleanValue();
		}

		/// <exception cref="System.IO.IOException"/>
		public override byte getByteValue()
		{
			return delegate_.getByteValue();
		}

		/// <exception cref="System.IO.IOException"/>
		public override short getShortValue()
		{
			return delegate_.getShortValue();
		}

		/// <exception cref="System.IO.IOException"/>
		public override java.math.BigDecimal getDecimalValue()
		{
			return delegate_.getDecimalValue();
		}

		/// <exception cref="System.IO.IOException"/>
		public override double getDoubleValue()
		{
			return delegate_.getDoubleValue();
		}

		/// <exception cref="System.IO.IOException"/>
		public override float getFloatValue()
		{
			return delegate_.getFloatValue();
		}

		/// <exception cref="System.IO.IOException"/>
		public override int getIntValue()
		{
			return delegate_.getIntValue();
		}

		/// <exception cref="System.IO.IOException"/>
		public override long getLongValue()
		{
			return delegate_.getLongValue();
		}

		/// <exception cref="System.IO.IOException"/>
		public override com.fasterxml.jackson.core.JsonParser.NumberType getNumberType()
		{
			return delegate_.getNumberType();
		}

		/// <exception cref="System.IO.IOException"/>
		public override Sharpen.Number getNumberValue()
		{
			return delegate_.getNumberValue();
		}

		/*
		/**********************************************************
		/* Public API, access to token information, coercion/conversion
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		public override int getValueAsInt()
		{
			return delegate_.getValueAsInt();
		}

		/// <exception cref="System.IO.IOException"/>
		public override int getValueAsInt(int defaultValue)
		{
			return delegate_.getValueAsInt(defaultValue);
		}

		/// <exception cref="System.IO.IOException"/>
		public override long getValueAsLong()
		{
			return delegate_.getValueAsLong();
		}

		/// <exception cref="System.IO.IOException"/>
		public override long getValueAsLong(long defaultValue)
		{
			return delegate_.getValueAsLong(defaultValue);
		}

		/// <exception cref="System.IO.IOException"/>
		public override double getValueAsDouble()
		{
			return delegate_.getValueAsDouble();
		}

		/// <exception cref="System.IO.IOException"/>
		public override double getValueAsDouble(double defaultValue)
		{
			return delegate_.getValueAsDouble(defaultValue);
		}

		/// <exception cref="System.IO.IOException"/>
		public override bool getValueAsBoolean()
		{
			return delegate_.getValueAsBoolean();
		}

		/// <exception cref="System.IO.IOException"/>
		public override bool getValueAsBoolean(bool defaultValue)
		{
			return delegate_.getValueAsBoolean(defaultValue);
		}

		/// <exception cref="System.IO.IOException"/>
		public override string getValueAsString()
		{
			return delegate_.getValueAsString();
		}

		/// <exception cref="System.IO.IOException"/>
		public override string getValueAsString(string defaultValue)
		{
			return delegate_.getValueAsString(defaultValue);
		}

		/*
		/**********************************************************
		/* Public API, access to token values, other
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		public override object getEmbeddedObject()
		{
			return delegate_.getEmbeddedObject();
		}

		/// <exception cref="System.IO.IOException"/>
		public override byte[] getBinaryValue(com.fasterxml.jackson.core.Base64Variant b64variant
			)
		{
			return delegate_.getBinaryValue(b64variant);
		}

		/// <exception cref="System.IO.IOException"/>
		public override int readBinaryValue(com.fasterxml.jackson.core.Base64Variant b64variant
			, Sharpen.OutputStream @out)
		{
			return delegate_.readBinaryValue(b64variant, @out);
		}

		public override com.fasterxml.jackson.core.JsonLocation getTokenLocation()
		{
			return delegate_.getTokenLocation();
		}

		/*
		/**********************************************************
		/* Internal helper methods
		/**********************************************************
		*/
		protected internal virtual com.fasterxml.jackson.core.JsonStreamContext _filterContext
			()
		{
			if (_exposedContext != null)
			{
				return _exposedContext;
			}
			return _headContext;
		}
	}
}
