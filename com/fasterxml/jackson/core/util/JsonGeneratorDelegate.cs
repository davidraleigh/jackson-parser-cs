using Sharpen;

namespace com.fasterxml.jackson.core.util
{
	public class JsonGeneratorDelegate : com.fasterxml.jackson.core.JsonGenerator
	{
		/// <summary>Delegate object that method calls are delegated to.</summary>
		protected internal com.fasterxml.jackson.core.JsonGenerator delegate_;

		/// <summary>
		/// Whether copy methods
		/// (
		/// <see cref="copyCurrentEvent(com.fasterxml.jackson.core.JsonParser)"/>
		/// ,
		/// <see cref="copyCurrentStructure(com.fasterxml.jackson.core.JsonParser)"/>
		/// ,
		/// <see cref="writeTree(com.fasterxml.jackson.core.TreeNode)"/>
		/// and
		/// <see cref="writeObject(object)"/>
		/// )
		/// are to be called (true), or handled by this object (false).
		/// </summary>
		protected internal bool delegateCopyMethods;

		public JsonGeneratorDelegate(com.fasterxml.jackson.core.JsonGenerator d)
			: this(d, true)
		{
		}

		/// <param name="delegateCopyMethods">
		/// Flag assigned to <code>delagateCopyMethod</code>
		/// and which defines whether copy methods are handled locally (false), or
		/// delegated to configured
		/// </param>
		public JsonGeneratorDelegate(com.fasterxml.jackson.core.JsonGenerator d, bool delegateCopyMethods
			)
		{
			/*
			/**********************************************************
			/* Construction, initialization
			/**********************************************************
			*/
			delegate_ = d;
			this.delegateCopyMethods = delegateCopyMethods;
		}

		public override object getCurrentValue()
		{
			return delegate_.getCurrentValue();
		}

		public override void setCurrentValue(object v)
		{
			delegate_.setCurrentValue(v);
		}

		/*
		/**********************************************************
		/* Extended API
		/**********************************************************
		*/
		public virtual com.fasterxml.jackson.core.JsonGenerator getDelegate()
		{
			return delegate_;
		}

		/*
		/**********************************************************
		/* Public API, metadata
		/**********************************************************
		*/
		public override com.fasterxml.jackson.core.ObjectCodec getCodec()
		{
			return delegate_.getCodec();
		}

		public override com.fasterxml.jackson.core.JsonGenerator setCodec(com.fasterxml.jackson.core.ObjectCodec
			 oc)
		{
			delegate_.setCodec(oc);
			return this;
		}

		public override void setSchema(com.fasterxml.jackson.core.FormatSchema schema)
		{
			delegate_.setSchema(schema);
		}

		public override com.fasterxml.jackson.core.FormatSchema getSchema()
		{
			return delegate_.getSchema();
		}

		public override com.fasterxml.jackson.core.Version version()
		{
			return delegate_.version();
		}

		public override object getOutputTarget()
		{
			return delegate_.getOutputTarget();
		}

		public override int getOutputBuffered()
		{
			return delegate_.getOutputBuffered();
		}

		/*
		/**********************************************************
		/* Public API, capability introspection (since 2.3, mostly)
		/**********************************************************
		*/
		public override bool canUseSchema(com.fasterxml.jackson.core.FormatSchema schema)
		{
			return delegate_.canUseSchema(schema);
		}

		public override bool canWriteTypeId()
		{
			return delegate_.canWriteTypeId();
		}

		public override bool canWriteObjectId()
		{
			return delegate_.canWriteObjectId();
		}

		public override bool canWriteBinaryNatively()
		{
			return delegate_.canWriteBinaryNatively();
		}

		public override bool canOmitFields()
		{
			return delegate_.canOmitFields();
		}

		/*
		/**********************************************************
		/* Public API, configuration
		/**********************************************************
		*/
		public override com.fasterxml.jackson.core.JsonGenerator enable(com.fasterxml.jackson.core.JsonGenerator.Feature
			 f)
		{
			delegate_.enable(f);
			return this;
		}

		public override com.fasterxml.jackson.core.JsonGenerator disable(com.fasterxml.jackson.core.JsonGenerator.Feature
			 f)
		{
			delegate_.disable(f);
			return this;
		}

		public override bool isEnabled(com.fasterxml.jackson.core.JsonGenerator.Feature f
			)
		{
			return delegate_.isEnabled(f);
		}

		// final, can't override (and no need to)
		//public final JsonGenerator configure(Feature f, boolean state)
		public override int getFeatureMask()
		{
			return delegate_.getFeatureMask();
		}

		public override com.fasterxml.jackson.core.JsonGenerator setFeatureMask(int mask)
		{
			delegate_.setFeatureMask(mask);
			return this;
		}

		/*
		/**********************************************************
		/* Configuring generator
		/**********************************************************
		*/
		public override com.fasterxml.jackson.core.JsonGenerator setPrettyPrinter(com.fasterxml.jackson.core.PrettyPrinter
			 pp)
		{
			delegate_.setPrettyPrinter(pp);
			return this;
		}

		public override com.fasterxml.jackson.core.PrettyPrinter getPrettyPrinter()
		{
			return delegate_.getPrettyPrinter();
		}

		public override com.fasterxml.jackson.core.JsonGenerator useDefaultPrettyPrinter(
			)
		{
			delegate_.useDefaultPrettyPrinter();
			return this;
		}

		public override com.fasterxml.jackson.core.JsonGenerator setHighestNonEscapedChar
			(int charCode)
		{
			delegate_.setHighestNonEscapedChar(charCode);
			return this;
		}

		public override int getHighestEscapedChar()
		{
			return delegate_.getHighestEscapedChar();
		}

		public override com.fasterxml.jackson.core.io.CharacterEscapes getCharacterEscapes
			()
		{
			return delegate_.getCharacterEscapes();
		}

		public override com.fasterxml.jackson.core.JsonGenerator setCharacterEscapes(com.fasterxml.jackson.core.io.CharacterEscapes
			 esc)
		{
			delegate_.setCharacterEscapes(esc);
			return this;
		}

		public override com.fasterxml.jackson.core.JsonGenerator setRootValueSeparator(com.fasterxml.jackson.core.SerializableString
			 sep)
		{
			delegate_.setRootValueSeparator(sep);
			return this;
		}

		/*
		/**********************************************************
		/* Public API, write methods, structural
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		public override void writeStartArray()
		{
			delegate_.writeStartArray();
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeStartArray(int size)
		{
			delegate_.writeStartArray(size);
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeEndArray()
		{
			delegate_.writeEndArray();
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeStartObject()
		{
			delegate_.writeStartObject();
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeEndObject()
		{
			delegate_.writeEndObject();
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeFieldName(string name)
		{
			delegate_.writeFieldName(name);
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeFieldName(com.fasterxml.jackson.core.SerializableString
			 name)
		{
			delegate_.writeFieldName(name);
		}

		/*
		/**********************************************************
		/* Public API, write methods, text/String values
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		public override void writeString(string text)
		{
			delegate_.writeString(text);
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeString(char[] text, int offset, int len)
		{
			delegate_.writeString(text, offset, len);
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeString(com.fasterxml.jackson.core.SerializableString text
			)
		{
			delegate_.writeString(text);
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeRawUTF8String(byte[] text, int offset, int length)
		{
			delegate_.writeRawUTF8String(text, offset, length);
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeUTF8String(byte[] text, int offset, int length)
		{
			delegate_.writeUTF8String(text, offset, length);
		}

		/*
		/**********************************************************
		/* Public API, write methods, binary/raw content
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		public override void writeRaw(string text)
		{
			delegate_.writeRaw(text);
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeRaw(string text, int offset, int len)
		{
			delegate_.writeRaw(text, offset, len);
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeRaw(com.fasterxml.jackson.core.SerializableString raw)
		{
			delegate_.writeRaw(raw);
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeRaw(char[] text, int offset, int len)
		{
			delegate_.writeRaw(text, offset, len);
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeRaw(char c)
		{
			delegate_.writeRaw(c);
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeRawValue(string text)
		{
			delegate_.writeRawValue(text);
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeRawValue(string text, int offset, int len)
		{
			delegate_.writeRawValue(text, offset, len);
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeRawValue(char[] text, int offset, int len)
		{
			delegate_.writeRawValue(text, offset, len);
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeBinary(com.fasterxml.jackson.core.Base64Variant b64variant
			, byte[] data, int offset, int len)
		{
			delegate_.writeBinary(b64variant, data, offset, len);
		}

		/// <exception cref="System.IO.IOException"/>
		public override int writeBinary(com.fasterxml.jackson.core.Base64Variant b64variant
			, Sharpen.InputStream data, int dataLength)
		{
			return delegate_.writeBinary(b64variant, data, dataLength);
		}

		/*
		/**********************************************************
		/* Public API, write methods, other value types
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		public override void writeNumber(short v)
		{
			delegate_.writeNumber(v);
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeNumber(int v)
		{
			delegate_.writeNumber(v);
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeNumber(long v)
		{
			delegate_.writeNumber(v);
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeNumber(System.Numerics.BigInteger v)
		{
			delegate_.writeNumber(v);
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeNumber(double v)
		{
			delegate_.writeNumber(v);
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeNumber(float v)
		{
			delegate_.writeNumber(v);
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeNumber(java.math.BigDecimal v)
		{
			delegate_.writeNumber(v);
		}

		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="System.NotSupportedException"/>
		public override void writeNumber(string encodedValue)
		{
			delegate_.writeNumber(encodedValue);
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeBoolean(bool state)
		{
			delegate_.writeBoolean(state);
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeNull()
		{
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
			delegate_.writeOmittedField(fieldName);
		}

		/*
		/**********************************************************
		/* Public API, write methods, Native Ids
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		public override void writeObjectId(object id)
		{
			delegate_.writeObjectId(id);
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeObjectRef(object id)
		{
			delegate_.writeObjectRef(id);
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeTypeId(object id)
		{
			delegate_.writeTypeId(id);
		}

		/*
		/**********************************************************
		/* Public API, write methods, serializing Java objects
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonProcessingException"/>
		public override void writeObject(object pojo)
		{
			if (delegateCopyMethods)
			{
				delegate_.writeObject(pojo);
				return;
			}
			// NOTE: copied from 
			if (pojo == null)
			{
				writeNull();
			}
			else
			{
				if (getCodec() != null)
				{
					getCodec().writeValue(this, pojo);
					return;
				}
				_writeSimpleObject(pojo);
			}
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeTree(com.fasterxml.jackson.core.TreeNode rootNode)
		{
			if (delegateCopyMethods)
			{
				delegate_.writeTree(rootNode);
				return;
			}
			// As with 'writeObject()', we are not check if write would work
			if (rootNode == null)
			{
				writeNull();
			}
			else
			{
				if (getCodec() == null)
				{
					throw new System.InvalidOperationException("No ObjectCodec defined");
				}
				getCodec().writeValue(this, rootNode);
			}
		}

		/*
		/**********************************************************
		/* Public API, convenience field write methods
		/**********************************************************
		*/
		// // These are fine, just delegate to other methods...
		/*
		/**********************************************************
		/* Public API, copy-through methods
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		public override void copyCurrentEvent(com.fasterxml.jackson.core.JsonParser jp)
		{
			if (delegateCopyMethods)
			{
				delegate_.copyCurrentEvent(jp);
			}
			else
			{
				base.copyCurrentEvent(jp);
			}
		}

		/// <exception cref="System.IO.IOException"/>
		public override void copyCurrentStructure(com.fasterxml.jackson.core.JsonParser jp
			)
		{
			if (delegateCopyMethods)
			{
				delegate_.copyCurrentStructure(jp);
			}
			else
			{
				base.copyCurrentStructure(jp);
			}
		}

		/*
		/**********************************************************
		/* Public API, context access
		/**********************************************************
		*/
		public override com.fasterxml.jackson.core.JsonStreamContext getOutputContext()
		{
			return delegate_.getOutputContext();
		}

		/*
		/**********************************************************
		/* Public API, buffer handling
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		public override void flush()
		{
			delegate_.flush();
		}

		/// <exception cref="System.IO.IOException"/>
		public override void close()
		{
			delegate_.close();
		}

		/*
		/**********************************************************
		/* Closeable implementation
		/**********************************************************
		*/
		public override bool isClosed()
		{
			return delegate_.isClosed();
		}
	}
}
