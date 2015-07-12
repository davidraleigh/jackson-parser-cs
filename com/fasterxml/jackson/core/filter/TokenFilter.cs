using Sharpen;

namespace com.fasterxml.jackson.core.filter
{
	/// <summary>
	/// Strategy class that can be implemented to specify actual inclusion/exclusion
	/// criteria for filtering, used by
	/// <see cref="FilteringGeneratorDelegate"/>
	/// .
	/// </summary>
	/// <since>2.6</since>
	public class TokenFilter
	{
		/// <summary>
		/// Marker value that should be used to indicate inclusion of a structured
		/// value (sub-tree representing Object or Array), or value of a named
		/// property (regardless of type).
		/// </summary>
		/// <remarks>
		/// Marker value that should be used to indicate inclusion of a structured
		/// value (sub-tree representing Object or Array), or value of a named
		/// property (regardless of type).
		/// Note that if this instance is returned, it will used as a marker, and
		/// no actual callbacks need to be made. For this reason, it is more efficient
		/// to return this instance if the whole sub-tree is to be included, instead
		/// of implementing similar filter functionality explicitly.
		/// </remarks>
		public static readonly com.fasterxml.jackson.core.filter.TokenFilter INCLUDE_ALL = 
			new com.fasterxml.jackson.core.filter.TokenFilter();

		protected internal TokenFilter()
		{
		}

		// // Marker values
		// Life-cycle
		/*
		/**********************************************************
		/* API, structured values
		/**********************************************************
		*/
		/// <summary>
		/// Method called to check whether Object value at current output
		/// location should be included in output.
		/// </summary>
		/// <remarks>
		/// Method called to check whether Object value at current output
		/// location should be included in output.
		/// Three kinds of return values may be used as follows:
		/// <ul>
		/// <li><code>null</code> to indicate that the Object should be skipped
		/// </li>
		/// <li>
		/// <see cref="INCLUDE_ALL"/>
		/// to indicate that the Object should be included
		/// completely in output
		/// </li>
		/// <li>Any other
		/// <see cref="TokenFilter"/>
		/// implementation (possibly this one) to mean
		/// that further inclusion calls on return filter object need to be made
		/// on contained properties, as necessary.
		/// <see cref="filterFinishObject()"/>
		/// will
		/// also be called on returned filter object
		/// </li>
		/// </ul>
		/// <p>
		/// Default implementation returns <code>this</code>, which means that checks
		/// are made recursively for properties of the Object to determine possible inclusion.
		/// </remarks>
		/// <returns>
		/// TokenFilter to use for further calls within Array, unless return value
		/// is <code>null</code> or
		/// <see cref="INCLUDE_ALL"/>
		/// (which have simpler semantics)
		/// </returns>
		public virtual com.fasterxml.jackson.core.filter.TokenFilter filterStartObject()
		{
			return this;
		}

		/// <summary>
		/// Method called to check whether Array value at current output
		/// location should be included in output.
		/// </summary>
		/// <remarks>
		/// Method called to check whether Array value at current output
		/// location should be included in output.
		/// Three kinds of return values may be used as follows:
		/// <ul>
		/// <li><code>null</code> to indicate that the Array should be skipped
		/// </li>
		/// <li>
		/// <see cref="INCLUDE_ALL"/>
		/// to indicate that the Array should be included
		/// completely in output
		/// </li>
		/// <li>Any other
		/// <see cref="TokenFilter"/>
		/// implementation (possibly this one) to mean
		/// that further inclusion calls on return filter object need to be made
		/// on contained element values, as necessary.
		/// <see cref="filterFinishArray()"/>
		/// will
		/// also be called on returned filter object
		/// </li>
		/// </ul>
		/// <p>
		/// Default implementation returns <code>this</code>, which means that checks
		/// are made recursively for elements of the array to determine possible inclusion.
		/// </remarks>
		/// <returns>
		/// TokenFilter to use for further calls within Array, unless return value
		/// is <code>null</code> or
		/// <see cref="INCLUDE_ALL"/>
		/// (which have simpler semantics)
		/// </returns>
		public virtual com.fasterxml.jackson.core.filter.TokenFilter filterStartArray()
		{
			return this;
		}

		/// <summary>
		/// Method called to indicate that output of non-filtered Object (one that may
		/// have been included either completely, or in part) is completed,
		/// in cases where filter other that
		/// <see cref="INCLUDE_ALL"/>
		/// was returned.
		/// This occurs when
		/// <see cref="com.fasterxml.jackson.core.JsonGenerator.writeEndObject()"/>
		/// is called.
		/// </summary>
		public virtual void filterFinishObject()
		{
		}

		/// <summary>
		/// Method called to indicate that output of non-filtered Array (one that may
		/// have been included either completely, or in part) is completed,
		/// in cases where filter other that
		/// <see cref="INCLUDE_ALL"/>
		/// was returned.
		/// This occurs when
		/// <see cref="com.fasterxml.jackson.core.JsonGenerator.writeEndArray()"/>
		/// is called.
		/// </summary>
		public virtual void filterFinishArray()
		{
		}

		/*
		/**********************************************************
		/* API, properties/elements
		/**********************************************************
		*/
		/// <summary>
		/// Method called to check whether property value with specified name,
		/// at current output location, should be included in output.
		/// </summary>
		/// <remarks>
		/// Method called to check whether property value with specified name,
		/// at current output location, should be included in output.
		/// Three kinds of return values may be used as follows:
		/// <ul>
		/// <li><code>null</code> to indicate that the property and its value should be skipped
		/// </li>
		/// <li>
		/// <see cref="INCLUDE_ALL"/>
		/// to indicate that the property and its value should be included
		/// completely in output
		/// </li>
		/// <li>Any other
		/// <see cref="TokenFilter"/>
		/// implementation (possibly this one) to mean
		/// that further inclusion calls on returned filter object need to be made
		/// as necessary, to determine inclusion.
		/// </li>
		/// </ul>
		/// <p>
		/// The default implementation simply returns <code>this</code> to continue calling
		/// methods on this filter object, without full inclusion or exclusion.
		/// </remarks>
		/// <returns>
		/// TokenFilter to use for further calls within property value, unless return value
		/// is <code>null</code> or
		/// <see cref="INCLUDE_ALL"/>
		/// (which have simpler semantics)
		/// </returns>
		public virtual com.fasterxml.jackson.core.filter.TokenFilter includeProperty(string
			 name)
		{
			return this;
		}

		/// <summary>
		/// Method called to check whether array element with specified index (zero-based),
		/// at current output location, should be included in output.
		/// </summary>
		/// <remarks>
		/// Method called to check whether array element with specified index (zero-based),
		/// at current output location, should be included in output.
		/// Three kinds of return values may be used as follows:
		/// <ul>
		/// <li><code>null</code> to indicate that the Array element should be skipped
		/// </li>
		/// <li>
		/// <see cref="INCLUDE_ALL"/>
		/// to indicate that the Array element should be included
		/// completely in output
		/// </li>
		/// <li>Any other
		/// <see cref="TokenFilter"/>
		/// implementation (possibly this one) to mean
		/// that further inclusion calls on returned filter object need to be made
		/// as necessary, to determine inclusion.
		/// </li>
		/// </ul>
		/// <p>
		/// The default implementation simply returns <code>this</code> to continue calling
		/// methods on this filter object, without full inclusion or exclusion.
		/// </remarks>
		/// <returns>
		/// TokenFilter to use for further calls within element value, unless return value
		/// is <code>null</code> or
		/// <see cref="INCLUDE_ALL"/>
		/// (which have simpler semantics)
		/// </returns>
		public virtual com.fasterxml.jackson.core.filter.TokenFilter includeElement(int index
			)
		{
			return this;
		}

		/// <summary>
		/// Method called to check whether root-level value,
		/// at current output location, should be included in output.
		/// </summary>
		/// <remarks>
		/// Method called to check whether root-level value,
		/// at current output location, should be included in output.
		/// Three kinds of return values may be used as follows:
		/// <ul>
		/// <li><code>null</code> to indicate that the root value should be skipped
		/// </li>
		/// <li>
		/// <see cref="INCLUDE_ALL"/>
		/// to indicate that the root value should be included
		/// completely in output
		/// </li>
		/// <li>Any other
		/// <see cref="TokenFilter"/>
		/// implementation (possibly this one) to mean
		/// that further inclusion calls on returned filter object need to be made
		/// as necessary, to determine inclusion.
		/// </li>
		/// </ul>
		/// <p>
		/// The default implementation simply returns <code>this</code> to continue calling
		/// methods on this filter object, without full inclusion or exclusion.
		/// </remarks>
		/// <returns>
		/// TokenFilter to use for further calls within root value, unless return value
		/// is <code>null</code> or
		/// <see cref="INCLUDE_ALL"/>
		/// (which have simpler semantics)
		/// </returns>
		public virtual com.fasterxml.jackson.core.filter.TokenFilter includeRootValue(int
			 index)
		{
			return this;
		}

		/*
		/**********************************************************
		/* API, scalar values (being read)
		/**********************************************************
		*/
		/// <summary>
		/// Call made when verifying whether a scaler value is being
		/// read from a parser.
		/// </summary>
		/// <remarks>
		/// Call made when verifying whether a scaler value is being
		/// read from a parser.
		/// <p>
		/// Default action is to call <code>_includeScalar()</code> and return
		/// whatever it indicates.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		public virtual bool includeValue(com.fasterxml.jackson.core.JsonParser p)
		{
			return _includeScalar();
		}

		/*
		/**********************************************************
		/* API, scalar values (being written)
		/**********************************************************
		*/
		/// <summary>
		/// Call made to verify whether leaf-level
		/// boolean value
		/// should be included in output or not.
		/// </summary>
		public virtual bool includeBoolean(bool value)
		{
			return _includeScalar();
		}

		/// <summary>
		/// Call made to verify whether leaf-level
		/// null value
		/// should be included in output or not.
		/// </summary>
		public virtual bool includeNull()
		{
			return _includeScalar();
		}

		/// <summary>
		/// Call made to verify whether leaf-level
		/// String value
		/// should be included in output or not.
		/// </summary>
		public virtual bool includeString(string value)
		{
			return _includeScalar();
		}

		/// <summary>
		/// Call made to verify whether leaf-level
		/// <code>int</code> value
		/// should be included in output or not.
		/// </summary>
		/// <remarks>
		/// Call made to verify whether leaf-level
		/// <code>int</code> value
		/// should be included in output or not.
		/// NOTE: also called for `short`, `byte`
		/// </remarks>
		public virtual bool includeNumber(int v)
		{
			return _includeScalar();
		}

		/// <summary>
		/// Call made to verify whether leaf-level
		/// <code>long</code> value
		/// should be included in output or not.
		/// </summary>
		public virtual bool includeNumber(long v)
		{
			return _includeScalar();
		}

		/// <summary>
		/// Call made to verify whether leaf-level
		/// <code>float</code> value
		/// should be included in output or not.
		/// </summary>
		public virtual bool includeNumber(float v)
		{
			return _includeScalar();
		}

		/// <summary>
		/// Call made to verify whether leaf-level
		/// <code>double</code> value
		/// should be included in output or not.
		/// </summary>
		public virtual bool includeNumber(double v)
		{
			return _includeScalar();
		}

		/// <summary>
		/// Call made to verify whether leaf-level
		/// <see cref="java.math.BigDecimal"/>
		/// value
		/// should be included in output or not.
		/// </summary>
		public virtual bool includeNumber(java.math.BigDecimal v)
		{
			return _includeScalar();
		}

		/// <summary>
		/// Call made to verify whether leaf-level
		/// <see cref="System.Numerics.BigInteger"/>
		/// value
		/// should be included in output or not.
		/// </summary>
		public virtual bool includeNumber(System.Numerics.BigInteger v)
		{
			return _includeScalar();
		}

		/// <summary>
		/// Call made to verify whether leaf-level
		/// Binary value
		/// should be included in output or not.
		/// </summary>
		/// <remarks>
		/// Call made to verify whether leaf-level
		/// Binary value
		/// should be included in output or not.
		/// <p>
		/// NOTE: no binary payload passed; assumption is this won't be of much use.
		/// </remarks>
		public virtual bool includeBinary()
		{
			return _includeScalar();
		}

		/// <summary>
		/// Call made to verify whether leaf-level
		/// raw (pre-encoded, not quoted by generator) value
		/// should be included in output or not.
		/// </summary>
		/// <remarks>
		/// Call made to verify whether leaf-level
		/// raw (pre-encoded, not quoted by generator) value
		/// should be included in output or not.
		/// <p>
		/// NOTE: value itself not passed since it may come on multiple forms
		/// and is unlikely to be of much use in determining inclusion
		/// criteria.
		/// </remarks>
		public virtual bool includeRawValue()
		{
			return _includeScalar();
		}

		/// <summary>
		/// Call made to verify whether leaf-level
		/// embedded (Opaque) value
		/// should be included in output or not.
		/// </summary>
		public virtual bool includeEmbeddedValue(object ob)
		{
			return _includeScalar();
		}

		/*
		/**********************************************************
		/* Overrides
		/**********************************************************
		*/
		public override string ToString()
		{
			if (this == INCLUDE_ALL)
			{
				return "TokenFilter.INCLUDE_ALL";
			}
			return base.ToString();
		}

		/*
		/**********************************************************
		/* Other methods
		/**********************************************************
		*/
		/// <summary>
		/// Overridable default implementation delegated to all scalar value
		/// inclusion check methods.
		/// </summary>
		/// <remarks>
		/// Overridable default implementation delegated to all scalar value
		/// inclusion check methods.
		/// The default implementation simply includes all leaf values.
		/// </remarks>
		protected internal virtual bool _includeScalar()
		{
			return true;
		}
	}
}
