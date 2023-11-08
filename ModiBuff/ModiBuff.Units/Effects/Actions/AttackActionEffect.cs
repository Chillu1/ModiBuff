namespace ModiBuff.Core.Units
{
	public sealed class AttackActionEffect : IEffect
	{
		private readonly Targeting _targeting;

		public AttackActionEffect(Targeting targeting = Targeting.TargetSource)
		{
			_targeting = targeting;
		}

		public void Effect(IUnit target, IUnit source)
		{
			_targeting.UpdateTargetSource(ref target, ref source);
			if (!(source is IAttacker<float, float> attackerSource))
				return;

			attackerSource.Attack(target);
		}
	}
}