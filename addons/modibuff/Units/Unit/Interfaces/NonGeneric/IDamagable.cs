namespace ModiBuff.Core.Units.Interfaces.NonGeneric
{
	public interface IAttackable : IAttackable<float, float>
	{
	}

	public interface IDamagable : IDamagable<float, float, float, float>, IAttackable
	{
	}
}