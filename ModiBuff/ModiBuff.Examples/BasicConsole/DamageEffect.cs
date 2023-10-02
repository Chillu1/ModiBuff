using ModiBuff.Core;
using ModiBuff.Core.Units.Interfaces.NonGeneric;

namespace ModiBuff.Examples.BasicConsole
{
	/// <summary>
	///		Simplest possible damage effect, we don't have any mutable state
	///		so we don't need to inherit from IStateEffect.
	///		It simply will deal x damage to the target on effect
	/// </summary>
	public class DamageEffect : IEffect
	{
		private readonly float _baseDamage;

		public DamageEffect(float baseDamage)
		{
			_baseDamage = baseDamage;
		}

		//Every effect needs to implement the Effect method
		//It will be called every time the effect is triggered
		//This can happen on Init, Interval, Duration, Callback, etc
		public void Effect(IUnit target, IUnit source)
		{
			((IDamagable)target).TakeDamage(_baseDamage, source);
		}
	}
}