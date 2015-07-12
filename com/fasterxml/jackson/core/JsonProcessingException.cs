/* Jackson JSON-processor.
*
* Copyright (c) 2007- Tatu Saloranta, tatu.saloranta@iki.fi
*/
using Sharpen;

namespace com.fasterxml.jackson.core
{
	/// <summary>
	/// Intermediate base class for all problems encountered when
	/// processing (parsing, generating) JSON content
	/// that are not pure I/O problems.
	/// </summary>
	/// <remarks>
	/// Intermediate base class for all problems encountered when
	/// processing (parsing, generating) JSON content
	/// that are not pure I/O problems.
	/// Regular
	/// <see cref="System.IO.IOException"/>
	/// s will be passed through as is.
	/// Sub-class of
	/// <see cref="System.IO.IOException"/>
	/// for convenience.
	/// </remarks>
	[System.Serializable]
	public class JsonProcessingException : System.IO.IOException
	{
		internal const long serialVersionUID = 123;

		protected internal com.fasterxml.jackson.core.JsonLocation _location;

		protected internal JsonProcessingException(string msg, com.fasterxml.jackson.core.JsonLocation
			 loc, System.Exception rootCause)
			: base(msg)
		{
			// Stupid eclipse...
			/* Argh. IOException(Throwable,String) is only available starting
			* with JDK 1.6...
			*/
			if (rootCause != null)
			{
				Sharpen.Extensions.InitCause(this, rootCause);
			}
			_location = loc;
		}

		protected internal JsonProcessingException(string msg)
			: base(msg)
		{
		}

		protected internal JsonProcessingException(string msg, com.fasterxml.jackson.core.JsonLocation
			 loc)
			: this(msg, loc, null)
		{
		}

		protected internal JsonProcessingException(string msg, System.Exception rootCause
			)
			: this(msg, null, rootCause)
		{
		}

		protected internal JsonProcessingException(System.Exception rootCause)
			: this(null, null, rootCause)
		{
		}

		public virtual com.fasterxml.jackson.core.JsonLocation getLocation()
		{
			return _location;
		}

		/*
		/**********************************************************
		/* Extended API
		/**********************************************************
		*/
		/// <summary>
		/// Method that allows accessing the original "message" argument,
		/// without additional decorations (like location information)
		/// that overridden
		/// <see cref="Message()"/>
		/// adds.
		/// </summary>
		/// <since>2.1</since>
		public virtual string getOriginalMessage()
		{
			return base.Message;
		}

		/*
		/**********************************************************
		/* Methods for sub-classes to use, override
		/**********************************************************
		*/
		/// <summary>
		/// Accessor that sub-classes can override to append additional
		/// information right after the main message, but before
		/// source location information.
		/// </summary>
		protected internal virtual string getMessageSuffix()
		{
			return null;
		}

		/// <summary>Default method overridden so that we can add location information</summary>
		public override string Message
		{
			get
			{
				/*
				/**********************************************************
				/* Overrides of standard methods
				/**********************************************************
				*/
				string msg = base.Message;
				if (msg == null)
				{
					msg = "N/A";
				}
				com.fasterxml.jackson.core.JsonLocation loc = getLocation();
				string suffix = getMessageSuffix();
				// mild optimization, if nothing extra is needed:
				if (loc != null || suffix != null)
				{
					System.Text.StringBuilder sb = new System.Text.StringBuilder(100);
					sb.Append(msg);
					if (suffix != null)
					{
						sb.Append(suffix);
					}
					if (loc != null)
					{
						sb.Append('\n');
						sb.Append(" at ");
						sb.Append(loc.ToString());
					}
					msg = sb.ToString();
				}
				return msg;
			}
		}

		public override string ToString()
		{
			return GetType().FullName + ": " + Message;
		}
	}
}
