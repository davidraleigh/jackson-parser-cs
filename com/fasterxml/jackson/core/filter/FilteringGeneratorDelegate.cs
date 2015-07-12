using Sharpen;

namespace com.fasterxml.jackson.core.filter
{
	/// <summary>
	/// Specialized
	/// <see cref="com.fasterxml.jackson.core.util.JsonGeneratorDelegate"/>
	/// that allows use of
	/// <see cref="TokenFilter"/>
	/// for outputting a subset of content that
	/// caller tries to generate.
	/// </summary>
	/// <since>2.6</since>
	public class FilteringGeneratorDelegate : com.fasterxml.jackson.core.util.JsonGeneratorDelegate
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
		/// Although delegate has its own output context it is not sufficient since we actually
		/// have to keep track of excluded (filtered out) structures as well as ones delegate
		/// actually outputs.
		/// </summary>
		protected internal com.fasterxml.jackson.core.filter.TokenFilterContext _filterContext;

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
		/// has been returned
		/// </summary>
		protected internal int _matchCount;

		public FilteringGeneratorDelegate(com.fasterxml.jackson.core.JsonGenerator d, com.fasterxml.jackson.core.filter.TokenFilter
			 f, bool includePath, bool allowMultipleMatches)
			: base(d, false)
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
			/* Additional state
			/**********************************************************
			*/
			/*
			/**********************************************************
			/* Construction, initialization
			/**********************************************************
			*/
			// By default, do NOT delegate copy methods
			rootFilter = f;
			// and this is the currently active filter for root values
			_itemFilter = f;
			_filterContext = com.fasterxml.jackson.core.filter.TokenFilterContext.createRootContext
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

		public virtual com.fasterxml.jackson.core.JsonStreamContext getFilterContext()
		{
			return _filterContext;
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
		/* Public API, accessors
		/**********************************************************
		*/
		public override com.fasterxml.jackson.core.JsonStreamContext getOutputContext()
		{
			/* 11-Apr-2015, tatu: Choice is between pre- and post-filter context;
			*   let's expose post-filter context that correlates with the view
			*   of caller.
			*/
			return _filterContext;
		}

		/*
		/**********************************************************
		/* Public API, write methods, structural
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		public override void writeStartArray()
		{
			// First things first: whole-sale skipping easy
			if (_itemFilter == null)
			{
				_filterContext = _filterContext.createChildArrayContext(null, false);
				return;
			}
			if (_itemFilter == com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
			{
				// include the whole sub-tree?
				_filterContext = _filterContext.createChildArrayContext(_itemFilter, true);
				delegate_.writeStartArray();
				return;
			}
			// Ok; regular checking state then
			_itemFilter = _filterContext.checkValue(_itemFilter);
			if (_itemFilter == null)
			{
				_filterContext = _filterContext.createChildArrayContext(null, false);
				return;
			}
			if (_itemFilter != com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
			{
				_itemFilter = _itemFilter.filterStartArray();
			}
			if (_itemFilter == com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
			{
				_checkParentPath();
				_filterContext = _filterContext.createChildArrayContext(_itemFilter, true);
				delegate_.writeStartArray();
			}
			else
			{
				_filterContext = _filterContext.createChildArrayContext(_itemFilter, false);
			}
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeStartArray(int size)
		{
			if (_itemFilter == null)
			{
				_filterContext = _filterContext.createChildArrayContext(null, false);
				return;
			}
			if (_itemFilter == com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
			{
				_filterContext = _filterContext.createChildArrayContext(_itemFilter, true);
				delegate_.writeStartArray(size);
				return;
			}
			_itemFilter = _filterContext.checkValue(_itemFilter);
			if (_itemFilter == null)
			{
				_filterContext = _filterContext.createChildArrayContext(null, false);
				return;
			}
			if (_itemFilter != com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
			{
				_itemFilter = _itemFilter.filterStartArray();
			}
			if (_itemFilter == com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
			{
				_checkParentPath();
				_filterContext = _filterContext.createChildArrayContext(_itemFilter, true);
				delegate_.writeStartArray(size);
			}
			else
			{
				_filterContext = _filterContext.createChildArrayContext(_itemFilter, false);
			}
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeEndArray()
		{
			_filterContext = _filterContext.closeArray(delegate_);
			if (_filterContext != null)
			{
				_itemFilter = _filterContext.getFilter();
			}
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeStartObject()
		{
			if (_itemFilter == null)
			{
				_filterContext = _filterContext.createChildObjectContext(_itemFilter, false);
				return;
			}
			if (_itemFilter == com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
			{
				_filterContext = _filterContext.createChildObjectContext(_itemFilter, true);
				delegate_.writeStartObject();
				return;
			}
			com.fasterxml.jackson.core.filter.TokenFilter f = _filterContext.checkValue(_itemFilter
				);
			if (f == null)
			{
				return;
			}
			if (f != com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
			{
				f = f.filterStartObject();
			}
			if (f == com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
			{
				_checkParentPath();
				_filterContext = _filterContext.createChildObjectContext(f, true);
				delegate_.writeStartObject();
			}
			else
			{
				// filter out
				_filterContext = _filterContext.createChildObjectContext(f, false);
			}
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeEndObject()
		{
			_filterContext = _filterContext.closeObject(delegate_);
			if (_filterContext != null)
			{
				_itemFilter = _filterContext.getFilter();
			}
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeFieldName(string name)
		{
			com.fasterxml.jackson.core.filter.TokenFilter state = _filterContext.setFieldName
				(name);
			if (state == null)
			{
				_itemFilter = null;
				return;
			}
			if (state == com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
			{
				_itemFilter = state;
				delegate_.writeFieldName(name);
				return;
			}
			state = state.includeProperty(name);
			_itemFilter = state;
			if (state == com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
			{
				_checkPropertyParentPath();
			}
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeFieldName(com.fasterxml.jackson.core.SerializableString
			 name)
		{
			com.fasterxml.jackson.core.filter.TokenFilter state = _filterContext.setFieldName
				(name.getValue());
			if (state == null)
			{
				_itemFilter = null;
				return;
			}
			if (state == com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
			{
				_itemFilter = state;
				delegate_.writeFieldName(name);
				return;
			}
			state = state.includeProperty(name.getValue());
			_itemFilter = state;
			if (state == com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
			{
				_checkPropertyParentPath();
			}
		}

		/*
		/**********************************************************
		/* Public API, write methods, text/String values
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		public override void writeString(string value)
		{
			if (_itemFilter == null)
			{
				return;
			}
			if (_itemFilter != com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
			{
				com.fasterxml.jackson.core.filter.TokenFilter state = _filterContext.checkValue(_itemFilter
					);
				if (state == null)
				{
					return;
				}
				if (state != com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
				{
					if (!state.includeString(value))
					{
						return;
					}
				}
				_checkParentPath();
			}
			delegate_.writeString(value);
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeString(char[] text, int offset, int len)
		{
			if (_itemFilter == null)
			{
				return;
			}
			if (_itemFilter != com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
			{
				string value = new string(text, offset, len);
				com.fasterxml.jackson.core.filter.TokenFilter state = _filterContext.checkValue(_itemFilter
					);
				if (state == null)
				{
					return;
				}
				if (state != com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
				{
					if (!state.includeString(value))
					{
						return;
					}
				}
				_checkParentPath();
			}
			delegate_.writeString(text, offset, len);
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeString(com.fasterxml.jackson.core.SerializableString value
			)
		{
			if (_itemFilter == null)
			{
				return;
			}
			if (_itemFilter != com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
			{
				com.fasterxml.jackson.core.filter.TokenFilter state = _filterContext.checkValue(_itemFilter
					);
				if (state == null)
				{
					return;
				}
				if (state != com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
				{
					if (!state.includeString(value.getValue()))
					{
						return;
					}
				}
				_checkParentPath();
			}
			delegate_.writeString(value);
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeRawUTF8String(byte[] text, int offset, int length)
		{
			if (_checkRawValueWrite())
			{
				delegate_.writeRawUTF8String(text, offset, length);
			}
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeUTF8String(byte[] text, int offset, int length)
		{
			// not exact match, but best we can do
			if (_checkRawValueWrite())
			{
				delegate_.writeRawUTF8String(text, offset, length);
			}
		}

		/*
		/**********************************************************
		/* Public API, write methods, binary/raw content
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		public override void writeRaw(string text)
		{
			if (_checkRawValueWrite())
			{
				delegate_.writeRaw(text);
			}
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeRaw(string text, int offset, int len)
		{
			if (_checkRawValueWrite())
			{
				delegate_.writeRaw(text);
			}
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeRaw(com.fasterxml.jackson.core.SerializableString text)
		{
			if (_checkRawValueWrite())
			{
				delegate_.writeRaw(text);
			}
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeRaw(char[] text, int offset, int len)
		{
			if (_checkRawValueWrite())
			{
				delegate_.writeRaw(text, offset, len);
			}
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeRaw(char c)
		{
			if (_checkRawValueWrite())
			{
				delegate_.writeRaw(c);
			}
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeRawValue(string text)
		{
			if (_checkRawValueWrite())
			{
				delegate_.writeRaw(text);
			}
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeRawValue(string text, int offset, int len)
		{
			if (_checkRawValueWrite())
			{
				delegate_.writeRaw(text, offset, len);
			}
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeRawValue(char[] text, int offset, int len)
		{
			if (_checkRawValueWrite())
			{
				delegate_.writeRaw(text, offset, len);
			}
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeBinary(com.fasterxml.jackson.core.Base64Variant b64variant
			, byte[] data, int offset, int len)
		{
			if (_checkBinaryWrite())
			{
				delegate_.writeBinary(b64variant, data, offset, len);
			}
		}

		/// <exception cref="System.IO.IOException"/>
		public override int writeBinary(com.fasterxml.jackson.core.Base64Variant b64variant
			, Sharpen.InputStream data, int dataLength)
		{
			if (_checkBinaryWrite())
			{
				return delegate_.writeBinary(b64variant, data, dataLength);
			}
			return -1;
		}

		/*
		/**********************************************************
		/* Public API, write methods, other value types
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		public override void writeNumber(short v)
		{
			if (_itemFilter == null)
			{
				return;
			}
			if (_itemFilter != com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
			{
				com.fasterxml.jackson.core.filter.TokenFilter state = _filterContext.checkValue(_itemFilter
					);
				if (state == null)
				{
					return;
				}
				if (state != com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
				{
					if (!state.includeNumber(v))
					{
						return;
					}
				}
				_checkParentPath();
			}
			delegate_.writeNumber(v);
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeNumber(int v)
		{
			if (_itemFilter == null)
			{
				return;
			}
			if (_itemFilter != com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
			{
				com.fasterxml.jackson.core.filter.TokenFilter state = _filterContext.checkValue(_itemFilter
					);
				if (state == null)
				{
					return;
				}
				if (state != com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
				{
					if (!state.includeNumber(v))
					{
						return;
					}
				}
				_checkParentPath();
			}
			delegate_.writeNumber(v);
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeNumber(long v)
		{
			if (_itemFilter == null)
			{
				return;
			}
			if (_itemFilter != com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
			{
				com.fasterxml.jackson.core.filter.TokenFilter state = _filterContext.checkValue(_itemFilter
					);
				if (state == null)
				{
					return;
				}
				if (state != com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
				{
					if (!state.includeNumber(v))
					{
						return;
					}
				}
				_checkParentPath();
			}
			delegate_.writeNumber(v);
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeNumber(System.Numerics.BigInteger v)
		{
			if (_itemFilter == null)
			{
				return;
			}
			if (_itemFilter != com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
			{
				com.fasterxml.jackson.core.filter.TokenFilter state = _filterContext.checkValue(_itemFilter
					);
				if (state == null)
				{
					return;
				}
				if (state != com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
				{
					if (!state.includeNumber(v))
					{
						return;
					}
				}
				_checkParentPath();
			}
			delegate_.writeNumber(v);
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeNumber(double v)
		{
			if (_itemFilter == null)
			{
				return;
			}
			if (_itemFilter != com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
			{
				com.fasterxml.jackson.core.filter.TokenFilter state = _filterContext.checkValue(_itemFilter
					);
				if (state == null)
				{
					return;
				}
				if (state != com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
				{
					if (!state.includeNumber(v))
					{
						return;
					}
				}
				_checkParentPath();
			}
			delegate_.writeNumber(v);
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeNumber(float v)
		{
			if (_itemFilter == null)
			{
				return;
			}
			if (_itemFilter != com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
			{
				com.fasterxml.jackson.core.filter.TokenFilter state = _filterContext.checkValue(_itemFilter
					);
				if (state == null)
				{
					return;
				}
				if (state != com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
				{
					if (!state.includeNumber(v))
					{
						return;
					}
				}
				_checkParentPath();
			}
			delegate_.writeNumber(v);
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeNumber(java.math.BigDecimal v)
		{
			if (_itemFilter == null)
			{
				return;
			}
			if (_itemFilter != com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
			{
				com.fasterxml.jackson.core.filter.TokenFilter state = _filterContext.checkValue(_itemFilter
					);
				if (state == null)
				{
					return;
				}
				if (state != com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
				{
					if (!state.includeNumber(v))
					{
						return;
					}
				}
				_checkParentPath();
			}
			delegate_.writeNumber(v);
		}

		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="System.NotSupportedException"/>
		public override void writeNumber(string encodedValue)
		{
			if (_itemFilter == null)
			{
				return;
			}
			if (_itemFilter != com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
			{
				com.fasterxml.jackson.core.filter.TokenFilter state = _filterContext.checkValue(_itemFilter
					);
				if (state == null)
				{
					return;
				}
				if (state != com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
				{
					if (!state.includeRawValue())
					{
						// close enough?
						return;
					}
				}
				_checkParentPath();
			}
			delegate_.writeNumber(encodedValue);
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeBoolean(bool v)
		{
			if (_itemFilter == null)
			{
				return;
			}
			if (_itemFilter != com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
			{
				com.fasterxml.jackson.core.filter.TokenFilter state = _filterContext.checkValue(_itemFilter
					);
				if (state == null)
				{
					return;
				}
				if (state != com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
				{
					if (!state.includeBoolean(v))
					{
						return;
					}
				}
				_checkParentPath();
			}
			delegate_.writeBoolean(v);
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeNull()
		{
			if (_itemFilter == null)
			{
				return;
			}
			if (_itemFilter != com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
			{
				com.fasterxml.jackson.core.filter.TokenFilter state = _filterContext.checkValue(_itemFilter
					);
				if (state == null)
				{
					return;
				}
				if (state != com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
				{
					if (!state.includeNull())
					{
						return;
					}
				}
				_checkParentPath();
			}
			delegate_.writeNull();
		}

		/*
		/**********************************************************
		/* Overridden field methods
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		public override void writeOmittedField(string fieldName)
		{
			// Hmmh. Not sure how this would work but...
			if (_itemFilter != null)
			{
				delegate_.writeOmittedField(fieldName);
			}
		}

		/*
		/**********************************************************
		/* Public API, write methods, Native Ids
		/**********************************************************
		*/
		// 25-Mar-2015, tatu: These are tricky as they sort of predate actual filtering calls.
		//   Let's try to use current state as a clue at least...
		/// <exception cref="System.IO.IOException"/>
		public override void writeObjectId(object id)
		{
			if (_itemFilter != null)
			{
				delegate_.writeObjectId(id);
			}
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeObjectRef(object id)
		{
			if (_itemFilter != null)
			{
				delegate_.writeObjectRef(id);
			}
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeTypeId(object id)
		{
			if (_itemFilter != null)
			{
				delegate_.writeTypeId(id);
			}
		}

		/*
		/**********************************************************
		/* Public API, write methods, serializing Java objects
		/**********************************************************
		*/
		// Base class definitions for these seems correct to me, iff not directly delegating:
		/*
		@Override
		public void writeObject(Object pojo) throws IOException,JsonProcessingException {
		if (delegateCopyMethods) {
		delegate.writeObject(pojo);
		return;
		}
		// NOTE: copied from
		if (pojo == null) {
		writeNull();
		} else {
		if (getCodec() != null) {
		getCodec().writeValue(this, pojo);
		return;
		}
		_writeSimpleObject(pojo);
		}
		}
		
		@Override
		public void writeTree(TreeNode rootNode) throws IOException {
		if (delegateCopyMethods) {
		delegate.writeTree(rootNode);
		return;
		}
		// As with 'writeObject()', we are not check if write would work
		if (rootNode == null) {
		writeNull();
		} else {
		if (getCodec() == null) {
		throw new IllegalStateException("No ObjectCodec defined");
		}
		getCodec().writeValue(this, rootNode);
		}
		}
		*/
		/*
		/**********************************************************
		/* Public API, copy-through methods
		/**********************************************************
		*/
		// Base class definitions for these seems correct to me, iff not directly delegating:
		/*
		@Override
		public void copyCurrentEvent(JsonParser jp) throws IOException {
		if (delegateCopyMethods) delegate.copyCurrentEvent(jp);
		else super.copyCurrentEvent(jp);
		}
		
		@Override
		public void copyCurrentStructure(JsonParser jp) throws IOException {
		if (delegateCopyMethods) delegate.copyCurrentStructure(jp);
		else super.copyCurrentStructure(jp);
		}
		*/
		/*
		/**********************************************************
		/* Helper methods
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		protected internal virtual void _checkParentPath()
		{
			++_matchCount;
			// only need to construct path if parent wasn't written
			if (_includePath)
			{
				_filterContext.writePath(delegate_);
			}
			// also: if no multiple matches desired, short-cut checks
			if (!_allowMultipleMatches)
			{
				// Mark parents as "skip" so that further check calls are not made
				_filterContext.skipParentChecks();
			}
		}

		/// <summary>
		/// Specialized variant of
		/// <see cref="_checkParentPath()"/>
		/// used when checking
		/// parent for a property name to be included with value: rules are slightly
		/// different.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		protected internal virtual void _checkPropertyParentPath()
		{
			++_matchCount;
			if (_includePath)
			{
				_filterContext.writePath(delegate_);
			}
			else
			{
				if (_includeImmediateParent)
				{
					// 21-Apr-2015, tatu: Note that there is no API to enable this currently...
					//    retained for speculative future use
					_filterContext.writeImmediatePath(delegate_);
				}
			}
			// also: if no multiple matches desired, short-cut checks
			if (!_allowMultipleMatches)
			{
				// Mark parents as "skip" so that further check calls are not made
				_filterContext.skipParentChecks();
			}
		}

		/// <exception cref="System.IO.IOException"/>
		protected internal virtual bool _checkBinaryWrite()
		{
			if (_itemFilter == null)
			{
				return false;
			}
			if (_itemFilter == com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
			{
				return true;
			}
			if (_itemFilter.includeBinary())
			{
				// close enough?
				_checkParentPath();
				return true;
			}
			return false;
		}

		/// <exception cref="System.IO.IOException"/>
		protected internal virtual bool _checkRawValueWrite()
		{
			if (_itemFilter == null)
			{
				return false;
			}
			if (_itemFilter == com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL)
			{
				return true;
			}
			if (_itemFilter.includeRawValue())
			{
				// close enough?
				_checkParentPath();
				return true;
			}
			return false;
		}
	}
}
