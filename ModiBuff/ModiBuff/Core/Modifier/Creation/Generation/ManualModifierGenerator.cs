namespace ModiBuff.Core
{
	public sealed class ManualModifierGenerator : IModifierGenerator
	{
		public int Id { get; }
		public string Name { get; }
		public int Tag { get; }
		private readonly ModifierGeneratorFunc _createFunc;
		private readonly ModifierAddData _addData;

		private int _genId;

		public ManualModifierGenerator(int id, string name, in ModifierGeneratorFunc createFunc,
			in ModifierAddData addData, int tag)
		{
			Id = id;
			Name = name;
			_createFunc = createFunc;
			_addData = addData;
			Tag = tag;
		}

		public ModifierAddData GetAddData() => _addData;

		public Modifier Create() => _createFunc(Id, _genId++, Name);
	}
}