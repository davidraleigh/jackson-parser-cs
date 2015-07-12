using Sharpen;

namespace com.fasterxml.jackson.core.util
{
	/// <summary>
	/// Default linefeed-based indenter, used by
	/// <see cref="DefaultPrettyPrinter"/>
	/// (unless
	/// overridden). Uses system-specific linefeeds and 2 spaces for indentation per level.
	/// </summary>
	/// <since>2.5</since>
	[System.Serializable]
	public class DefaultIndenter : com.fasterxml.jackson.core.util.DefaultPrettyPrinter.NopIndenter
	{
		private const long serialVersionUID = 1L;

		public static readonly string SYS_LF;

		static DefaultIndenter()
		{
			string lf;
			try
			{
				lf = Sharpen.Runtime.getProperty("line.separator");
			}
			catch
			{
				lf = "\n";
			}
			// fallback when security manager denies access
			SYS_LF = lf;
		}

		public static readonly com.fasterxml.jackson.core.util.DefaultIndenter SYSTEM_LINEFEED_INSTANCE
			 = new com.fasterxml.jackson.core.util.DefaultIndenter("  ", SYS_LF);

		/// <summary>
		/// We expect to rarely get indentation deeper than this number of levels,
		/// and try not to pre-generate more indentations than needed.
		/// </summary>
		private const int INDENT_LEVELS = 16;

		private readonly char[] indents;

		private readonly int charsPerLevel;

		private readonly string eol;

		/// <summary>Indent with two spaces and the system's default line feed</summary>
		public DefaultIndenter()
			: this("  ", SYS_LF)
		{
		}

		/// <summary>
		/// Create an indenter which uses the <code>indent</code> string to indent one level
		/// and the <code>eol</code> string to separate lines.
		/// </summary>
		public DefaultIndenter(string indent, string eol)
		{
			charsPerLevel = indent.Length;
			indents = new char[indent.Length * INDENT_LEVELS];
			int offset = 0;
			for (int i = 0; i < INDENT_LEVELS; i++)
			{
				Sharpen.Runtime.getCharsForString(indent, 0, indent.Length, indents, offset);
				offset += indent.Length;
			}
			this.eol = eol;
		}

		public virtual com.fasterxml.jackson.core.util.DefaultIndenter withLinefeed(string
			 lf)
		{
			if (lf.Equals(eol))
			{
				return this;
			}
			return new com.fasterxml.jackson.core.util.DefaultIndenter(getIndent(), lf);
		}

		public virtual com.fasterxml.jackson.core.util.DefaultIndenter withIndent(string 
			indent)
		{
			if (indent.Equals(getIndent()))
			{
				return this;
			}
			return new com.fasterxml.jackson.core.util.DefaultIndenter(indent, eol);
		}

		public override bool isInline()
		{
			return false;
		}

		/// <exception cref="System.IO.IOException"/>
		public override void writeIndentation(com.fasterxml.jackson.core.JsonGenerator jg
			, int level)
		{
			jg.writeRaw(eol);
			if (level > 0)
			{
				// should we err on negative values (as there's some flaw?)
				level *= charsPerLevel;
				while (level > indents.Length)
				{
					// unlike to happen but just in case
					jg.writeRaw(indents, 0, indents.Length);
					level -= indents.Length;
				}
				jg.writeRaw(indents, 0, level);
			}
		}

		public virtual string getEol()
		{
			return eol;
		}

		public virtual string getIndent()
		{
			return new string(indents, 0, charsPerLevel);
		}
	}
}
