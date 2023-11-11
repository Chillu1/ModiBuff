namespace ModiBuff.Core.Units
{
	public sealed class AttackActionEffect : IEffect, IPostEffectOwner<AttackActionEffect, float>
	{
		private readonly Targeting _targeting;
		private IPostEffect<float>[] _postEffects;

		public AttackActionEffect(Targeting targeting = Targeting.TargetSource) => _targeting = targeting;

		public AttackActionEffect SetPostEffects(params IPostEffect<float>[] postEffects)
		{
			_postEffects = postEffects;
			return this;
		}

		public void Effect(IUnit target, IUnit source)
		{
			_targeting.UpdateTargetSource(ref target, ref source);
			if (!(source is IAttacker<float, float> attackerSource))
			{
#if MODIBUFF_EFFECT_CHECK
				EffectHelper.LogImplError(target, nameof(IAttacker<float, float>));
#endif
				return;
			}

			float returnDamage = attackerSource.Attack(target);

			if (_postEffects != null)
				foreach (var postEffect in _postEffects)
					postEffect.Effect(returnDamage, target, source);
		}
	}
}