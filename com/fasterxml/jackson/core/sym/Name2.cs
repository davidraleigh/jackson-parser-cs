using Sharpen;

namespace com.fasterxml.jackson.core.sym
{
	/// <summary>
	/// Specialized implementation of PName: can be used for short Strings
	/// that consists of 5 to 8 bytes.
	/// </summary>
	/// <remarks>
	/// Specialized implementation of PName: can be used for short Strings
	/// that consists of 5 to 8 bytes. Usually this means relatively short
	/// ascii-only names.
	/// <p>
	/// The reason for such specialized classes is mostly space efficiency;
	/// and to a lesser degree performance. Both are achieved for short
	/// Strings by avoiding another level of indirection (via quad arrays)
	/// </remarks>
	public sealed class Name2 : com.fasterxml.jackson.core.sym.Name
	{
		private readonly int q1;

		private readonly int q2;

		internal Name2(string name, int hash, int quad1, int quad2)
			: base(name, hash)
		{
			q1 = quad1;
			q2 = quad2;
		}

		public override bool equals(int quad)
		{
			return false;
		}

		public override bool equals(int quad1, int quad2)
		{
			return (quad1 == q1) && (quad2 == q2);
		}

		public override bool equals(int quad1, int quad2, int q3)
		{
			return false;
		}

		public override bool equals(int[] quads, int qlen)
		{
			return (qlen == 2 && quads[0] == q1 && quads[1] == q2);
		}
	}
}
