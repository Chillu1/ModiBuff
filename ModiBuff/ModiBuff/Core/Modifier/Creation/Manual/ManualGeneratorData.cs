namespace ModiBuff.Core
{
	public readonly struct ManualGeneratorData
	{
		public readonly string Name;
		public readonly ModifierGeneratorFunc CreateFunc;
		public readonly ModifierAddData AddData;

		public ManualGeneratorData(string name, ModifierGeneratorFunc createFunc, ModifierAddData addData)
		{
			Name = name;
			CreateFunc = createFunc;
			AddData = addData;
		}
	}
}