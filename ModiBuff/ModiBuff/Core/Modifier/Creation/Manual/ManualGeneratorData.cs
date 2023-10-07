namespace ModiBuff.Core
{
	public readonly struct ManualGeneratorData<TTag>
	{
		public readonly string Name;
		public readonly ModifierGeneratorFunc CreateFunc;
		public readonly ModifierAddData AddData;
		public readonly TTag Tag;

		public ManualGeneratorData(string name, ModifierGeneratorFunc createFunc, ModifierAddData addData, TTag tag)
		{
			Name = name;
			CreateFunc = createFunc;
			AddData = addData;
			Tag = tag;
		}
	}
}