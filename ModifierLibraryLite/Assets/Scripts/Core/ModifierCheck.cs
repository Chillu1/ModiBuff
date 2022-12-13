namespace ModifierLibraryLite.Core
{
	public sealed class ModifierCheck
	{
		public string Id { get; }

		private readonly CooldownCheck _cooldown;
		private readonly ChanceCheck _chance;

		public ModifierCheck(string id, ChanceCheck chance = null)
		{
			Id = id;
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