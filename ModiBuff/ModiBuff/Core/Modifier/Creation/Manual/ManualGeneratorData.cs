namespace ModiBuff.Core
{
	public readonly struct ManualGeneratorData
	{
		public readonly string Name;
		public readonly ModifierGeneratorFunc CreateFunc;
		public readonly TagType Tag;

		public ManualGeneratorData(string name, ModifierGeneratorFunc createFunc, TagType tag)
		{
			Name = name;
			CreateFunc = createFunc;
			Tag = tag;
		}
	}
}