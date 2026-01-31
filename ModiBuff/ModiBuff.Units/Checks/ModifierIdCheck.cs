namespace ModiBuff.Core.Units
{
	public sealed class ModifierIdCheck : IUnitCheck
	{
		private readonly int _modifierId;

		public ModifierIdCheck(int modifierId) => _modifierId = modifierId;

		public bool Check(IUnit source) => ((IModifierOwner)source).ModifierController.Contains(_modifierId);
	}
}