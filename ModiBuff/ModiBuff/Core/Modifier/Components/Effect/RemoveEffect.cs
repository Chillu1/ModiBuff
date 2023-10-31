namespace ModiBuff.Core
{
	public sealed class RemoveEffect : IModifierGenIdOwner, IEffect, IModifierIdOwner, IShallowClone<IEffect>
	{
		private IRevertEffect[] _revertibleEffects;
		private int _id = -1;
		private int _genId = -1;

		public RemoveEffect()
		{
		}

		internal RemoveEffect(int id) => _id = id;

		/// <summary>
		///		Manual modifier generation constructor
		/// </summary>
		public static RemoveEffect Create(int id, int genId, params IRevertEffect[] revertibleEffects)
		{
			var effect = new RemoveEffect(id, genId);
			effect.SetRevertibleEffects(revertibleEffects);
			return effect;
		}

		private RemoveEffect(int id, int genId)
		{
			_id = id;
			_genId = genId;
		}

		public void SetModifierId(int id) => _id = id;

		public void SetRevertibleEffects(IRevertEffect[] revertibleEffects)
		{
			_revertibleEffects = revertibleEffects;
		}

		public void SetGenId(int genId) => _genId = genId;

		public void Effect(IUnit target, IUnit source)
		{
			for (int i = 0; i < _revertibleEffects?.Length; i++)
				_revertibleEffects[i].RevertEffect(target, source);

#if DEBUG && !MODIBUFF_PROFILE
			if (_genId == -1) //This probably wont matter for not instance stackable modifiers
				Logger.LogWarning("[ModiBuff] RemoveEffect.Effect: genId wasn't set");
#endif

			//TODO From which collection? Applier support?
			((IModifierOwner)target).ModifierController.PrepareRemove(_id, _genId);
		}

		public IEffect ShallowClone() => new RemoveEffect(_id, _genId);
		object IShallowClone.ShallowClone() => ShallowClone();
	}
}