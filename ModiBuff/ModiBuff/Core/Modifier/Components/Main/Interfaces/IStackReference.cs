namespace ModiBuff.Core
{
	public interface IStackReference
	{
		/// <summary>
		///		The current stacks
		/// </summary>
		int Stacks { get; }

		/// <summary>
		///		The max stacks
		/// </summary>
		int MaxStacks { get; }
	}
}