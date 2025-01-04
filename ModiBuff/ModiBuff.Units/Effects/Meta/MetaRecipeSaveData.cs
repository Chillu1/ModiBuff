namespace ModiBuff.Core.Units
{
	public readonly struct MetaRecipeSaveData
	{
		public readonly int Id;
		public readonly object Data;

		public MetaRecipeSaveData(int id, object data)
		{
			Id = id;
			Data = data;
		}
	}
}