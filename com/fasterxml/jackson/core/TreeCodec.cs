using Sharpen;

namespace com.fasterxml.jackson.core
{
	/// <summary>
	/// Interface that defines objects that can read and write
	/// <see cref="TreeNode"/>
	/// instances using Streaming API.
	/// </summary>
	/// <since>2.3</since>
	public abstract class TreeCodec
	{
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonProcessingException"/>
		public abstract T readTree<T>(com.fasterxml.jackson.core.JsonParser jp)
			where T : com.fasterxml.jackson.core.TreeNode;

		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonProcessingException"/>
		public abstract void writeTree(com.fasterxml.jackson.core.JsonGenerator jg, com.fasterxml.jackson.core.TreeNode
			 tree);

		public abstract com.fasterxml.jackson.core.TreeNode createArrayNode();

		public abstract com.fasterxml.jackson.core.TreeNode createObjectNode();

		public abstract com.fasterxml.jackson.core.JsonParser treeAsTokens(com.fasterxml.jackson.core.TreeNode
			 node);
	}
}
