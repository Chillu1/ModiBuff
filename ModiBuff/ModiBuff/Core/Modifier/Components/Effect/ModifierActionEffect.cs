namespace ModiBuff.Core
{
	public sealed class ModifierActionEffect : IModifierGenIdOwner, IModifierIdOwner, IEffect, IShallowClone<ModifierActionEffect>
	{
		private readonly ModifierAction _modifierAction;

		private int _id = -1;
		private int _genId = -1;

		public ModifierActionEffect(ModifierAction modifierAction)
		{
			_modifierAction = modifierAction;
		}

		public ModifierActionEffect(ModifierAction modifierAction, int id)
		{
			_modifierAction = modifierAction;
			_id = id;
		}

		public void SetModifierId(int id) => _id = id;
		public void SetGenId(int genId) => _genId = genId;

		public void Effect(IUnit target, IUnit source)
		{
#if DEBUG && !MODIBUFF_PROFILE
			if (_id == -1)
				Logger.LogError("ModifierActionEffect.Effect: id wasn't set");
			if (_genId == -1) //This probably wont matter for not instance stackable modifiers
				Logger.LogWarning("ModifierActionEffect.Effect: genId wasn't set");
#endif

			((IModifierOwner)target).ModifierController.ModifierAction(_id, _genId, _modifierAction);
		}

		public ModifierActionEffect ShallowClone() => new ModifierActionEffect(_modifierAction, _id);
		object IShallowClone.ShallowClone() => ShallowClone();
	}
}