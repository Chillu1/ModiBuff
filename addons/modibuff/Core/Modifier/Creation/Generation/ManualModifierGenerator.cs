namespace ModiBuff.Core
{
	public sealed class ManualModifierGenerator : IModifierGenerator
	{
		public int Id { get; }
		public string Name { get; }
		public string DisplayName { get; }
		public string Description { get; }
		public TagType Tag { get; }
		private readonly ModifierGeneratorFunc _createFunc;

		private int _genId;

		public ManualModifierGenerator(int id, string name, string displayName, string description,
			ModifierGeneratorFunc createFunc, TagType tag)
		{
			Id = id;
			Name = name;
			DisplayName = displayName;
			Description = description;
			_createFunc = createFunc;

			//Updates tags based on modifier state
			//Generates a dummy modifier, to check for state
			tag = tag.UpdateTagBasedOnModifierComponents(createFunc(Id, _genId, Name, tag));
			Tag = tag;
		}

		public Modifier Create() => _createFunc(Id, _genId++, Name, Tag);
	}
}