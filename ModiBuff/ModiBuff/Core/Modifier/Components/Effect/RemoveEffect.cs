namespace ModiBuff.Core
{
	public sealed class RemoveEffect : IModifierGenIdOwner, IEffect, IStackEffect, IModifierIdOwner,
		IShallowClone<IEffect>
	{
		private readonly ApplierType? _applierType;
		private IRevertEffect[]? _revertibleEffects;
		private int _id;
		private int? _genId;

		public RemoveEffect()
		{
		}

		internal RemoveEffect(int id) => _id = id;

		internal RemoveEffect(int id, ApplierType? applierType = null)
		{
			_id = id;
			_applierType = applierType;
		}

		/// <summary>
		///		Manual modifier generation constructor
		/// </summary>
		public static RemoveEffect Create(int id, int? genId, params IRevertEffect[] revertibleEffects)
		{
			var effect = new RemoveEffect(id, genId, null);
			effect.SetRevertibleEffects(revertibleEffects);
			return effect;
		}

		private RemoveEffect(int id, int? genId, ApplierType? applierType)
		{
			_id = id;
			_genId = genId;
			_applierType = applierType;
		}

		public void SetModifierId(int id) => _id = id;

		public void SetRevertibleEffects(IRevertEffect[] revertibleEffects)
		{
			_revertibleEffects = revertibleEffects;
		}

		public void SetGenId(int genId) => _genId = genId;

		public void Effect(IUnit target, IUnit source)
		{
#if DEBUG && !MODIBUFF_PROFILE
			if (_genId == null) //This probably wont matter for not instance stackable modifiers
				Logger.LogWarning("[ModiBuff] RemoveEffect.Effect: genId wasn't set");
#endif

			for (int i = 0; i < _revertibleEffects?.Length; i++)
				_revertibleEffects[i].RevertEffect(target, source);

			if (_applierType != null)
			{
				((IModifierApplierOwner)target).RemoveApplier(_id /*, _genId*/, _applierType.Value);
				//return;
			}

			((IModifierOwner)target).ModifierController.PrepareRemove(_id, _genId!.Value);
		}

		public void StackEffect(int stacks, IUnit target, IUnit source) => Effect(target, source);

		public IEffect ShallowClone() => new RemoveEffect(_id, _genId, _applierType);
		object IShallowClone.ShallowClone() => ShallowClone();
	}
}