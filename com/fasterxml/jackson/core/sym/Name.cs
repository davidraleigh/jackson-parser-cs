using Sharpen;

namespace com.fasterxml.jackson.core.sym
{
	/// <summary>
	/// Base class for tokenized names (key strings in objects) that have
	/// been tokenized from byte-based input sources (like
	/// <see cref="Sharpen.InputStream"/>
	/// .
	/// </summary>
	/// <author>Tatu Saloranta</author>
	public abstract class Name
	{
		protected internal readonly string _name;

		protected internal readonly int _hashCode;

		protected internal Name(string name, int hashCode)
		{
			_name = name;
			_hashCode = hashCode;
		}

		public virtual string getName()
		{
			return _name;
		}

		/*
		/**********************************************************
		/* Methods for package/core parser
		/**********************************************************
		*/
		public abstract bool equals(int q1);

		public abstract bool equals(int q1, int q2);

		/// <since>2.6</since>
		public abstract bool equals(int q1, int q2, int q3);

		public abstract bool equals(int[] quads, int qlen);

		/*
		/**********************************************************
		/* Overridden standard methods
		/**********************************************************
		*/
		public override string ToString()
		{
			return _name;
		}

		public sealed override int GetHashCode()
		{
			return _hashCode;
		}

		public override bool Equals(object o)
		{
			// Canonical instances, can usually just do identity comparison
			return (o == this);
		}
	}
}
