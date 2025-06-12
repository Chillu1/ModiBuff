using System.Collections.Generic;

namespace ModiBuff.Core
{
	public sealed class InitComponent : IStateReset
	{
		private readonly IEffect[] _effects;
		private readonly IEffect[]? _registerEffects;
		private readonly IStateReset[]? _stateResetEffects;
		private readonly ModifierCheck? _modifierCheck;

		public InitComponent(IEffect[] effects, ModifierCheck? check)
		{
			_effects = effects;

			//TEMP
			var registerEffectsList = new List<IEffect>();
			for (int i = 0; i < effects.Length; i++)
			{
				var effect = effects[i];
				if (effect is IRegisterEffect)
					registerEffectsList.Add(effect);
			}

			if (registerEffectsList.Count > 0)
			{
				_registerEffects = registerEffectsList.ToArray();
				_effects = new IEffect[effects.Length - registerEffectsList.Count];
				int effectsIndex = 0;
				for (int i = 0; i < effects.Length; i++)
				{
					var effect = effects[i];
					if (effect is IRegisterEffect)
						continue;

					_effects[effectsIndex] = effect;
					effectsIndex++;
				}
			}
			else
				_registerEffects = null;

			//Callbacks with mutable state
			var stateResetEffectsList = new List<IStateReset>();
			for (int i = 0; i < effects.Length; i++)
			{
				if (effects[i] is IStateReset stateResetEffect)
					stateResetEffectsList.Add(stateResetEffect);
			}

			_stateResetEffects = stateResetEffectsList.Count > 0 ? stateResetEffectsList.ToArray() : null;

			_modifierCheck = check;
		}

		public void Init(IUnit target, IUnit owner)
		{
			//TODO Not ideal, especially since init is called often
			for (int i = 0; i < _registerEffects?.Length; i++)
				_registerEffects[i].Effect(target, owner);

			if (_modifierCheck?.Check(owner) == false)
				return;

			for (int i = 0; i < _effects.Length; i++)
				_effects[i].Effect(target, owner);
		}

		public void Init(IList<IUnit> targets, IUnit owner)
		{
			for (int i = 0; i < _registerEffects?.Length; i++)
				_registerEffects[i].Effect(targets, owner);

			if (_modifierCheck != null && !_modifierCheck.Check(owner))
				return;

			for (int i = 0; i < _effects.Length; i++)
				_effects[i].Effect(targets, owner);
		}

		public void InitLoad(IUnit target, IUnit owner)
		{
			for (int i = 0; i < _registerEffects?.Length; i++)
				_registerEffects[i].Effect(target, owner);
		}

		public void InitLoad(IList<IUnit> targets, IUnit owner)
		{
			for (int i = 0; i < _registerEffects?.Length; i++)
				_registerEffects[i].Effect(targets, owner);
		}

		public void ResetState()
		{
			for (int i = 0; i < _stateResetEffects?.Length; i++)
				_stateResetEffects[i].ResetState();
		}
	}
}