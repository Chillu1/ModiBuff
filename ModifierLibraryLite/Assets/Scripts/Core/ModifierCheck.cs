namespace ModifierLibraryLite.Core
{
	public sealed class ModifierCheck
	{
		public int IntId { get; }
		public string Name { get; }

		private readonly CooldownCheck _cooldown;
		private readonly ChanceCheck _chance;

		public ModifierCheck(int intId, string name, ChanceCheck chance = null)
		{
			IntId = intId;
			Name = name;
			//_cooldown = cooldown;
			_chance = chance;
		}

		public bool Check()
		{
			bool result = true;

			//result = _cooldown != null && _cooldown.IsReady;
			result = _chance == null || _chance.Roll();

			return result;
		}
	}
}