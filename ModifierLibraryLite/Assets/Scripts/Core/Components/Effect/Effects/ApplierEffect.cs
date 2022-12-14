namespace ModifierLibraryLite.Core
{
	public sealed class ApplierEffect : IEffect
	{
		private readonly int _modifierId;

		public void Effect(IUnit target, IUnit acter)
		{
			target.TryAddModifier(_modifierId, target, acter);
		}
	}
}