namespace ModiBuff.Core
{
	public sealed class DispelRegisterEffect : IEffect, IShallowClone<IEffect>, IRegisterEffect
	{
		private DispelType _dispelType;
		private RemoveEffect _removeEffect;

		private bool _isRegistered;

		public DispelRegisterEffect(DispelType dispelType) : this(dispelType, null)
		{
		}

		private DispelRegisterEffect(DispelType dispelType, RemoveEffect removeEffect)
		{
			_dispelType = dispelType;
			_removeEffect = removeEffect;
		}

		public static DispelRegisterEffect Create(DispelType dispelType, RemoveEffect removeEffect)
			=> new DispelRegisterEffect(dispelType, removeEffect);

		public void UpdateDispelType(DispelType dispelType) => _dispelType |= dispelType;

		public void SetRemoveEffect(RemoveEffect removeEffect)
		{
			_isRegistered = false;
			_removeEffect = removeEffect;
		}

		public void Effect(IUnit target, IUnit source)
		{
			if (_isRegistered)
				return;

			_isRegistered = true;
			((IModifierOwner)target).ModifierController.RegisterDispel(_dispelType, _removeEffect);
		}

		public IEffect ShallowClone() => new DispelRegisterEffect(_dispelType, null);
		object IShallowClone.ShallowClone() => ShallowClone();
	}
}