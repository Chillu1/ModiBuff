namespace ModiBuff.Core.Units
{
	public sealed class RemoveApplierEffect : IModifierGenIdOwner, IEffect, IStackEffect, IModifierIdOwner,
		IShallowClone<IEffect>
	{
		private readonly ApplierType _applierType;
		private int _id;
		private int? _genId;

		public RemoveApplierEffect(ApplierType applierType) => _applierType = applierType;

		/// <summary>
		///		Manual modifier generation constructor
		/// </summary>
		public static RemoveApplierEffect Create(int id, int? genId, ApplierType applierType)
		{
			var effect = new RemoveApplierEffect(id, genId, applierType);
			return effect;
		}

		private RemoveApplierEffect(int id, int? genId, ApplierType applierType)
		{
			_id = id;
			_genId = genId;
			_applierType = applierType;
		}

		public void SetModifierId(int id) => _id = id;

		public void SetGenId(int genId) => _genId = genId;

		public void Effect(IUnit target, IUnit source)
		{
#if DEBUG && !MODIBUFF_PROFILE
			if (_genId == null) //This probably wont matter for not instance stackable modifiers
				Logger.LogWarning("[ModiBuff] RemoveEffect.Effect: genId wasn't set");
#endif

			((IModifierApplierOwner)target).RemoveApplier(_id /*, _genId*/, _applierType);
		}

		public void StackEffect(int stacks, IUnit target, IUnit source) => Effect(target, source);

		public IEffect ShallowClone() => new RemoveApplierEffect(_id, _genId, _applierType);
		object IShallowClone.ShallowClone() => ShallowClone();
	}
}