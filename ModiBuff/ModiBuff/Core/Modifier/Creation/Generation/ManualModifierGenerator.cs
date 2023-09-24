namespace ModiBuff.Core
{
	public sealed class ManualModifierGenerator : IModifierGenerator
	{
		public int Id { get; }
		public string Name { get; }
		private readonly ModifierGeneratorFunc _createFunc;
		private readonly ModifierAddData _addData;

		private int _genId;

		public ManualModifierGenerator(int id, string name, in ModifierGeneratorFunc createFunc, in ModifierAddData addData)
		{
			Id = id;
			Name = name;
			_createFunc = createFunc;
			_addData = addData;
		}

		public ModifierAddData GetAddData() => _addData;

		public Modifier Create() => _createFunc(Id, _genId++, Name);
	}
}