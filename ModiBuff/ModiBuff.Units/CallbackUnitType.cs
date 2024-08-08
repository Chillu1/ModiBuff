namespace ModiBuff.Core.Units
{
	public enum CallbackUnitType
	{
		//When you're the target of X
		WhenAttacked = 1, //When being attacked (doesn't matter if it gets blocked, evaded or deals no damage)
		AfterAttacked,

		//WhenEvaded,
		//WhenHit, //When attack connects
		//WhenDamaged, //When attack deals damage
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

		StrongDispel,
		StrongHit,
	}
}