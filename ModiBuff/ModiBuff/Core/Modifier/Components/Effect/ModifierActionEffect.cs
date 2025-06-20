namespace ModiBuff.Core
{
	public sealed class ModifierActionEffect : IModifierGenIdOwner, IModifierIdOwner, IEffect,
		IStackEffect, ICallbackEffect, IShallowClone<IEffect>
	{
		private readonly ModifierAction _modifierAction;

		private int? _id;
		private int? _genId;

		public ModifierActionEffect(ModifierAction modifierAction)
		{
			_modifierAction = modifierAction;
		}

		/// <summary>
		///		Manual modifier generation constructor
		/// </summary>
		public static ModifierActionEffect Create(int id, int genId, ModifierAction modifierAction) =>
			new ModifierActionEffect(modifierAction, id, genId);

		public ModifierActionEffect(ModifierAction modifierAction, int id)
		{
			_modifierAction = modifierAction;
			_id = id;
		}

		private ModifierActionEffect(ModifierAction modifierAction, int id, int genId)
		{
			_modifierAction = modifierAction;
			_id = id;
			_genId = genId;
		}

		public void SetModifierId(int id) => _id = id;
		public void SetGenId(int genId) => _genId = genId;

		public void Effect(IUnit target, IUnit source)
		{
#if DEBUG && !MODIBUFF_PROFILE
			if (_id == null)
				Logger.LogError("[ModiBuff] ModifierActionEffect.Effect: id wasn't set");
			if (_genId == null) //This probably wont matter for not instance stackable modifiers
				Logger.LogWarning("[ModiBuff] ModifierActionEffect.Effect: genId wasn't set");
#endif

			((IModifierOwner)target).ModifierController.ModifierAction(_id!.Value, _genId!.Value, _modifierAction);
		}

		public void StackEffect(int stacks, IUnit target, IUnit source) => Effect(target, source);
		public void CallbackEffect(IUnit target, IUnit source) => Effect(target, source);

		public IEffect ShallowClone() => new ModifierActionEffect(_modifierAction, _id!.Value);
		object IShallowClone.ShallowClone() => ShallowClone();
	}
}