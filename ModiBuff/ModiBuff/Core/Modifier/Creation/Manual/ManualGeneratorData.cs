namespace ModiBuff.Core
{
	public readonly struct ManualGeneratorData
	{
		public readonly string Name;
		public readonly ModifierGeneratorFunc CreateFunc;
		public readonly ModifierAddData AddData;
		public readonly TagType Tag;

		public ManualGeneratorData(string name, ModifierGeneratorFunc createFunc,
			ModifierAddData addData, TagType tag)
		{
			Name = name;
			CreateFunc = createFunc;
			AddData = addData;
			Tag = tag;
		}
	}
}