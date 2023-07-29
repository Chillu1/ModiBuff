namespace ModiBuff.Examples.SimpleSolo
{
	public interface IBlockOwner
	{
		int BlockInstance { get; }

		void AddBlock(int amount);
		void RemoveBlock(int amount);
	}
}