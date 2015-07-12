using Sharpen;

namespace com.fasterxml.jackson.core.sym
{
	/// <summary>
	/// Specialized implementation of PName: can be used for short Strings
	/// that consists of at most 4 bytes.
	/// </summary>
	/// <remarks>
	/// Specialized implementation of PName: can be used for short Strings
	/// that consists of at most 4 bytes. Usually this means short
	/// ascii-only names.
	/// <p>
	/// The reason for such specialized classes is mostly space efficiency;
	/// and to a lesser degree performance. Both are achieved for short
	/// Strings by avoiding another level of indirection (via quad arrays)
	/// </remarks>
	public sealed class Name1 : com.fasterxml.jackson.core.sym.Name
	{
		private static readonly com.fasterxml.jackson.core.sym.Name1 EMPTY = new com.fasterxml.jackson.core.sym.Name1
			(string.Empty, 0, 0);

		private readonly int q;

		internal Name1(string name, int hash, int quad)
			: base(name, hash)
		{
			q = quad;
		}

		public static com.fasterxml.jackson.core.sym.Name1 getEmptyName()
		{
			return EMPTY;
		}

		public override bool equals(int quad)
		{
			return (quad == q);
		}

		public override bool equals(int quad1, int quad2)
		{
			return (quad1 == q) && (quad2 == 0);
		}

		public override bool equals(int q1, int q2, int q3)
		{
			return false;
		}

		public override bool equals(int[] quads, int qlen)
		{
			return (qlen == 1 && quads[0] == q);
		}
	}
}
