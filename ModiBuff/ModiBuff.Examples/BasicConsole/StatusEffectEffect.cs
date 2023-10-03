using ModiBuff.Core;
using ModiBuff.Core.Units;
using ModiBuff.Core.Units.Interfaces.NonGeneric;

namespace ModiBuff.Examples.BasicConsole
{
	/// <summary>
	///		Basic implementation of a statusEffect effect
	///		We supply what kind of status effect we want to apply, and for how long
	/// </summary>
	public class StatusEffectEffect : IEffect
	{
		private readonly StatusEffectType _statusEffectType;
		private readonly float _duration;

		public StatusEffectEffect(StatusEffectType statusEffectType, float duration)
		{
			_statusEffectType = statusEffectType;
			_duration = duration;
		}

		public void Effect(IUnit target, IUnit source)
		{
			Console.GameMessage($"Applied {_statusEffectType} to {target} for " + _duration + " seconds");
			((ISingleStatusEffectOwner)target).StatusEffectController
				.ChangeStatusEffect(_statusEffectType, _duration);
		}
	}
}