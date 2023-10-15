namespace ModiBuff.Core.Units
{
	public enum EffectOnEvent
	{
		None,

		//When you're the target of X
		WhenAttacked, //When being attacked (doesn't matter if it gets blocked, evaded or deals no damage)

		//WhenEvaded,
		//WhenHit, //When attack connects
		//WhenDamaged, //When attack deals damage
		WhenCast,
		WhenKilled,
		WhenHealed,

		//When you're the source/acter of X
		BeforeAttack,
		OnAttack,

		//OnEvade,
		//OnHit,
		OnCast,
		OnKill,
		OnHeal,
	}
}