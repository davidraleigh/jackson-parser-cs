using Sharpen;

namespace com.fasterxml.jackson.core.io
{
	/// <summary>
	/// String token that can lazily serialize String contained and then reuse that
	/// serialization later on.
	/// </summary>
	/// <remarks>
	/// String token that can lazily serialize String contained and then reuse that
	/// serialization later on. This is similar to JDBC prepared statements, for example,
	/// in that instances should only be created when they are used more than use;
	/// prime candidates are various serializers.
	/// <p>
	/// Class is final for performance reasons and since this is not designed to
	/// be extensible or customizable (customizations would occur in calling code)
	/// </remarks>
	[System.Serializable]
	public class SerializedString : com.fasterxml.jackson.core.SerializableString
	{
		protected internal readonly string _value;

		protected internal byte[] _quotedUTF8Ref;

		protected internal byte[] _unquotedUTF8Ref;

		protected internal char[] _quotedChars;

		public SerializedString(string v)
		{
			/* 13-Dec-2010, tatu: Whether use volatile or not is actually an important
			*   decision for multi-core use cases. Cost of volatility can be non-trivial
			*   for heavy use cases, and serialized-string instances are accessed often.
			*   Given that all code paths with common Jackson usage patterns go through
			*   a few memory barriers (mostly with cache/reuse pool access) it seems safe
			*   enough to omit volatiles here, given how simple lazy initialization is.
			*   This can be compared to how {@link String#hashCode} works; lazily and
			*   without synchronization or use of volatile keyword.
			*
			*   Change to remove volatile was a request by implementors of a high-throughput
			*   search framework; and they believed this is an important optimization for
			*   heaviest, multi-core deployed use cases.
			*/
			/*
			* 22-Sep-2013, tatu: FWIW, there have been no reports of problems in this
			*   area, or anything pointing to it. So I think we are safe up to JDK7
			*   and hopefully beyond.
			*/
			/*volatile*/
			/*volatile*/
			/*volatile*/
			if (v == null)
			{
				throw new System.InvalidOperationException("Null String illegal for SerializedString"
					);
			}
			_value = v;
		}

		/// <summary>
		/// Ugly hack, to work through the requirement that _value is indeed final,
		/// and that JDK serialization won't call ctor(s).
		/// </summary>
		/// <since>2.1</since>
		[System.NonSerialized]
		protected internal string _jdkSerializeValue;

		/*
		/**********************************************************
		/* Serializable overrides
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		private void readObject(Sharpen.ObjectInputStream @in)
		{
			_jdkSerializeValue = @in.readUTF();
		}

		/// <exception cref="System.IO.IOException"/>
		private void writeObject(Sharpen.ObjectOutputStream @out)
		{
			@out.writeUTF(_value);
		}

		protected internal virtual object readResolve()
		{
			return new com.fasterxml.jackson.core.io.SerializedString(_jdkSerializeValue);
		}

		/*
		/**********************************************************
		/* API
		/**********************************************************
		*/
		public string getValue()
		{
			return _value;
		}

		/// <summary>Returns length of the String as characters</summary>
		public int charLength()
		{
			return _value.Length;
		}

		public char[] asQuotedChars()
		{
			char[] result = _quotedChars;
			if (result == null)
			{
				result = com.fasterxml.jackson.core.io.JsonStringEncoder.getInstance().quoteAsString
					(_value);
				_quotedChars = result;
			}
			return result;
		}

		/// <summary>
		/// Accessor for accessing value that has been quoted using JSON
		/// quoting rules, and encoded using UTF-8 encoding.
		/// </summary>
		public byte[] asUnquotedUTF8()
		{
			byte[] result = _unquotedUTF8Ref;
			if (result == null)
			{
				result = com.fasterxml.jackson.core.io.JsonStringEncoder.getInstance().encodeAsUTF8
					(_value);
				_unquotedUTF8Ref = result;
			}
			return result;
		}

		/// <summary>
		/// Accessor for accessing value as is (without JSON quoting)
		/// encoded using UTF-8 encoding.
		/// </summary>
		public byte[] asQuotedUTF8()
		{
			byte[] result = _quotedUTF8Ref;
			if (result == null)
			{
				result = com.fasterxml.jackson.core.io.JsonStringEncoder.getInstance().quoteAsUTF8
					(_value);
				_quotedUTF8Ref = result;
			}
			return result;
		}

		/*
		/**********************************************************
		/* Additional 2.0 methods for appending/writing contents
		/**********************************************************
		*/
		public virtual int appendQuotedUTF8(byte[] buffer, int offset)
		{
			byte[] result = _quotedUTF8Ref;
			if (result == null)
			{
				result = com.fasterxml.jackson.core.io.JsonStringEncoder.getInstance().quoteAsUTF8
					(_value);
				_quotedUTF8Ref = result;
			}
			int length = result.Length;
			if ((offset + length) > buffer.Length)
			{
				return -1;
			}
			System.Array.Copy(result, 0, buffer, offset, length);
			return length;
		}

		public virtual int appendQuoted(char[] buffer, int offset)
		{
			char[] result = _quotedChars;
			if (result == null)
			{
				result = com.fasterxml.jackson.core.io.JsonStringEncoder.getInstance().quoteAsString
					(_value);
				_quotedChars = result;
			}
			int length = result.Length;
			if ((offset + length) > buffer.Length)
			{
				return -1;
			}
			System.Array.Copy(result, 0, buffer, offset, length);
			return length;
		}

		public virtual int appendUnquotedUTF8(byte[] buffer, int offset)
		{
			byte[] result = _unquotedUTF8Ref;
			if (result == null)
			{
				result = com.fasterxml.jackson.core.io.JsonStringEncoder.getInstance().encodeAsUTF8
					(_value);
				_unquotedUTF8Ref = result;
			}
			int length = result.Length;
			if ((offset + length) > buffer.Length)
			{
				return -1;
			}
			System.Array.Copy(result, 0, buffer, offset, length);
			return length;
		}

		public virtual int appendUnquoted(char[] buffer, int offset)
		{
			string str = _value;
			int length = str.Length;
			if ((offset + length) > buffer.Length)
			{
				return -1;
			}
			Sharpen.Runtime.getCharsForString(str, 0, length, buffer, offset);
			return length;
		}

		/// <exception cref="System.IO.IOException"/>
		public virtual int writeQuotedUTF8(Sharpen.OutputStream @out)
		{
			byte[] result = _quotedUTF8Ref;
			if (result == null)
			{
				result = com.fasterxml.jackson.core.io.JsonStringEncoder.getInstance().quoteAsUTF8
					(_value);
				_quotedUTF8Ref = result;
			}
			int length = result.Length;
			@out.write(result, 0, length);
			return length;
		}

		/// <exception cref="System.IO.IOException"/>
		public virtual int writeUnquotedUTF8(Sharpen.OutputStream @out)
		{
			byte[] result = _unquotedUTF8Ref;
			if (result == null)
			{
				result = com.fasterxml.jackson.core.io.JsonStringEncoder.getInstance().encodeAsUTF8
					(_value);
				_unquotedUTF8Ref = result;
			}
			int length = result.Length;
			@out.write(result, 0, length);
			return length;
		}

		public virtual int putQuotedUTF8(Sharpen.ByteBuffer buffer)
		{
			byte[] result = _quotedUTF8Ref;
			if (result == null)
			{
				result = com.fasterxml.jackson.core.io.JsonStringEncoder.getInstance().quoteAsUTF8
					(_value);
				_quotedUTF8Ref = result;
			}
			int length = result.Length;
			if (length > buffer.remaining())
			{
				return -1;
			}
			buffer.put(result, 0, length);
			return length;
		}

		public virtual int putUnquotedUTF8(Sharpen.ByteBuffer buffer)
		{
			byte[] result = _unquotedUTF8Ref;
			if (result == null)
			{
				result = com.fasterxml.jackson.core.io.JsonStringEncoder.getInstance().encodeAsUTF8
					(_value);
				_unquotedUTF8Ref = result;
			}
			int length = result.Length;
			if (length > buffer.remaining())
			{
				return -1;
			}
			buffer.put(result, 0, length);
			return length;
		}

		/*
		/**********************************************************
		/* Standard method overrides
		/**********************************************************
		*/
		public sealed override string ToString()
		{
			return _value;
		}

		public sealed override int GetHashCode()
		{
			return _value.GetHashCode();
		}

		public sealed override bool Equals(object o)
		{
			if (o == this)
			{
				return true;
			}
			if (o == null || o.GetType() != GetType())
			{
				return false;
			}
			com.fasterxml.jackson.core.io.SerializedString other = (com.fasterxml.jackson.core.io.SerializedString
				)o;
			return _value.Equals(other._value);
		}
	}
}
