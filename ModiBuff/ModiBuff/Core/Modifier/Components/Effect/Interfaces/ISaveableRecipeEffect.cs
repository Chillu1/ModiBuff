namespace ModiBuff.Core
{
	public interface ISaveableRecipeEffect<TSaveData> : ISaveableRecipeEffect
	{
	}

	public interface ISaveableRecipeEffect
	{
		object SaveRecipeState();
	}
}