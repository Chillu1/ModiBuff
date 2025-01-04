namespace ModiBuff.Core.Units
{
	public readonly struct PostRecipeSaveData
	{
		public readonly int Id;
		public readonly object Data;

		public PostRecipeSaveData(int id, object data)
		{
			Id = id;
			Data = data;
		}
	}
}