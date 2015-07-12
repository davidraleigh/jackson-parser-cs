using Sharpen;

namespace com.fasterxml.jackson.core.util
{
	/// <summary>
	/// Helper class that implements
	/// <a href="http://en.wikipedia.org/wiki/Delegation_pattern">delegation pattern</a> for
	/// <see cref="com.fasterxml.jackson.core.JsonParser"/>
	/// ,
	/// to allow for simple overridability of basic parsing functionality.
	/// The idea is that any functionality to be modified can be simply
	/// overridden; and anything else will be delegated by default.
	/// </summary>
	public class JsonParserDelegate : com.fasterxml.jackson.core.JsonParser
	{
		/// <summary>Delegate object that method calls are delegated to.</summary>
		protected internal com.fasterxml.jackson.core.JsonParser delegate_;

		public JsonParserDelegate(com.fasterxml.jackson.core.JsonParser d)
		{
			delegate_ = d;
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
		/* Public API, configuration
		/**********************************************************
		*/
		public override void setCodec(com.fasterxml.jackson.core.ObjectCodec c)
		{
			delegate_.setCodec(c);
		}

		public override com.fasterxml.jackson.core.ObjectCodec getCodec()
		{
			return delegate_.getCodec();
		}

		public override com.fasterxml.jackson.core.JsonParser enable(com.fasterxml.jackson.core.JsonParser.Feature
			 f)
		{
			delegate_.enable(f);
			return this;
		}

		public override com.fasterxml.jackson.core.JsonParser disable(com.fasterxml.jackson.core.JsonParser.Feature
			 f)
		{
			delegate_.disable(f);
			return this;
		}

		public override bool isEnabled(com.fasterxml.jackson.core.JsonParser.Feature f)
		{
			return delegate_.isEnabled(f);
		}

		public override int getFeatureMask()
		{
			return delegate_.getFeatureMask();
		}

		public override com.fasterxml.jackson.core.JsonParser setFeatureMask(int mask)
		{
			delegate_.setFeatureMask(mask);
			return this;
		}

		public override com.fasterxml.jackson.core.FormatSchema getSchema()
		{
			return delegate_.getSchema();
		}

		public override void setSchema(com.fasterxml.jackson.core.FormatSchema schema)
		{
			delegate_.setSchema(schema);
		}

		public override bool canUseSchema(com.fasterxml.jackson.core.FormatSchema schema)
		{
			return delegate_.canUseSchema(schema);
		}

		public override com.fasterxml.jackson.core.Version version()
		{
			return delegate_.version();
		}

		public override object getInputSource()
		{
			return delegate_.getInputSource();
		}

		/*
		/**********************************************************
		/* Capability introspection
		/**********************************************************
		*/
		public override bool requiresCustomCodec()
		{
			return delegate_.requiresCustomCodec();
		}

		/*
		/**********************************************************
		/* Closeable impl
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		public override void close()
		{
			delegate_.close();
		}

		public override bool isClosed()
		{
			return delegate_.isClosed();
		}

		/*
		/**********************************************************
		/* Public API, token accessors
		/**********************************************************
		*/
		public override com.fasterxml.jackson.core.JsonToken getCurrentToken()
		{
			return delegate_.getCurrentToken();
		}

		public override int getCurrentTokenId()
		{
			return delegate_.getCurrentTokenId();
		}

		public override bool hasCurrentToken()
		{
			return delegate_.hasCurrentToken();
		}

		public override bool hasTokenId(int id)
		{
			return delegate_.hasTokenId(id);
		}

		public override bool hasToken(com.fasterxml.jackson.core.JsonToken t)
		{
			return delegate_.hasToken(t);
		}

		/// <exception cref="System.IO.IOException"/>
		public override string getCurrentName()
		{
			return delegate_.getCurrentName();
		}

		public override com.fasterxml.jackson.core.JsonLocation getCurrentLocation()
		{
			return delegate_.getCurrentLocation();
		}

		public override com.fasterxml.jackson.core.JsonStreamContext getParsingContext()
		{
			return delegate_.getParsingContext();
		}

		public override bool isExpectedStartArrayToken()
		{
			return delegate_.isExpectedStartArrayToken();
		}

		public override bool isExpectedStartObjectToken()
		{
			return delegate_.isExpectedStartObjectToken();
		}

		/*
		/**********************************************************
		/* Public API, token state overrides
		/**********************************************************
		*/
		public override void clearCurrentToken()
		{
			delegate_.clearCurrentToken();
		}

		public override com.fasterxml.jackson.core.JsonToken getLastClearedToken()
		{
			return delegate_.getLastClearedToken();
		}

		public override void overrideCurrentName(string name)
		{
			delegate_.overrideCurrentName(name);
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
		public override java.lang.Number getNumberValue()
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

		/// <exception cref="System.IO.IOException"/>
		public override com.fasterxml.jackson.core.JsonToken nextToken()
		{
			return delegate_.nextToken();
		}

		/// <exception cref="System.IO.IOException"/>
		public override com.fasterxml.jackson.core.JsonToken nextValue()
		{
			return delegate_.nextValue();
		}

		/// <exception cref="System.IO.IOException"/>
		public override com.fasterxml.jackson.core.JsonParser skipChildren()
		{
			delegate_.skipChildren();
			// NOTE: must NOT delegate this method to delegate, needs to be self-reference for chaining
			return this;
		}

		/*
		/**********************************************************
		/* Public API, Native Ids (type, object)
		/**********************************************************
		*/
		public override bool canReadObjectId()
		{
			return delegate_.canReadObjectId();
		}

		public override bool canReadTypeId()
		{
			return delegate_.canReadTypeId();
		}

		/// <exception cref="System.IO.IOException"/>
		public override object getObjectId()
		{
			return delegate_.getObjectId();
		}

		/// <exception cref="System.IO.IOException"/>
		public override object getTypeId()
		{
			return delegate_.getTypeId();
		}
	}
}
