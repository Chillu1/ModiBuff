using System.Collections.Generic;

namespace ModiBuff.Core
{
	public sealed class InitComponent : IStateReset
	{
		private readonly IEffect[] _effects;
		private readonly IEffect[]? _registerEffects;
		private readonly IStateReset[]? _stateResetEffects;
		private readonly bool _oneTime;
		private readonly ModifierCheck? _modifierCheck;

		private bool _isInitialized;

		public InitComponent(bool oneTimeInit, IEffect[] effects, ModifierCheck? check)
		{
			_oneTime = oneTimeInit;
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
				var effect = effects[i];
				if (effect is IRegisterEffect && effect is IStateReset stateResetEffect)
					stateResetEffectsList.Add(stateResetEffect);
			}

			_stateResetEffects = stateResetEffectsList.Count > 0 ? stateResetEffectsList.ToArray() : null;

			_modifierCheck = check;

			_isInitialized = false;
		}

		public void Init(IUnit target, IUnit owner)
		{
			if (_oneTime && _isInitialized)
				return;

			//TODO Not ideal, especially since init is called often
			for (int i = 0; i < _registerEffects?.Length; i++)
				_registerEffects[i].Effect(target, owner);

			if (_modifierCheck?.Check(owner) == false)
				return;

			for (int i = 0; i < _effects.Length; i++)
				_effects[i].Effect(target, owner);

			_isInitialized = true;
		}

		public void Init(IList<IUnit> targets, IUnit owner)
		{
			if (_oneTime && _isInitialized)
				return;

			for (int i = 0; i < _registerEffects?.Length; i++)
				_registerEffects[i].Effect(targets, owner);

			if (_modifierCheck != null && !_modifierCheck.Check(owner))
				return;

			for (int i = 0; i < _effects.Length; i++)
				_effects[i].Effect(targets, owner);

			_isInitialized = true;
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
			_isInitialized = false;
			for (int i = 0; i < _stateResetEffects?.Length; i++)
				_stateResetEffects[i].ResetState();
		}

		public SaveData SaveState() => new SaveData(_isInitialized);
		public void LoadState(SaveData data) => _isInitialized = data.IsInitialized;

		public readonly struct SaveData
		{
			public readonly bool IsInitialized;

#if MODIBUFF_SYSTEM_TEXT_JSON
			[System.Text.Json.Serialization.JsonConstructor]
#endif
			public SaveData(bool isInitialized) => IsInitialized = isInitialized;
		}
	}
}