using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModifierLibraryLite.Core
{
	/// <summary>
	///		High level API for creating modifiers.
	/// </summary>
	public sealed class ModifierRecipe
	{
		public string Id { get; }

		public LegalTargetType LegalTargetType { get; private set; } = LegalTargetType.Self;

		private bool _init;
		private float _interval;
		private float _duration;

		private RemoveEffect _removeEffect;

		private List<IEffect>[] _effectBinds;

		private bool _refreshDuration, _refreshInterval;
		//private List<bool> _refreshables;

		private ModifierInternalRecipe _internalRecipe;

		public ModifierRecipe(string id)
		{
			Id = id;

			var allEffectOns = Enum.GetValues(typeof(EffectOn)).Cast<EffectOn>().ToArray();
			_effectBinds = new List<IEffect>[allEffectOns.Length];
			for (int i = 0; i < allEffectOns.Length; i++)
				_effectBinds[i] = new List<IEffect>(2);
		}

		public Modifier Create() => new Modifier(_internalRecipe);

		//---Actions---

		public ModifierRecipe Interval(float interval)
		{
			_interval = interval;
			return this;
		}

		public ModifierRecipe Duration(float duration)
		{
			_duration = duration;
			return this;
		}

		public ModifierRecipe Remove(float duration)
		{
			Duration(duration);
			_removeEffect = new RemoveEffect();
			Effect(_removeEffect, EffectOn.Duration);
			return this;
		}

		public ModifierRecipe Refresh(RefreshType type = RefreshType.Duration)
		{
			switch (type)
			{
				case RefreshType.Duration:
					_refreshDuration = true;
					break;
				case RefreshType.Interval:
					_refreshInterval = true;
					break;
				default:
					Debug.LogError($"Unknown refresh type: {type}");
					return this;
			}

			return this;
		}

		//---Effects---

		public ModifierRecipe Effect(IEffect effect, EffectOn effectOn)
		{
			if (!Utilities.Utilities.IsPowerOfTwo((int)effectOn))
			{
				foreach (var baseEffectOn in Enum.GetValues(typeof(EffectOn)).Cast<EffectOn>())
				{
					if ((effectOn & baseEffectOn) != 0)
						Effect(effect, baseEffectOn);
				}

				return this;
			}

			_effectBinds[Utilities.Utilities.FastLog2((double)effectOn)].Add(effect);
			return this;
		}

		//---Target---
		public ModifierRecipe LegalTarget(LegalTargetType legalTargetType)
		{
			LegalTargetType = legalTargetType;
			return this;
		}

		public void Finish()
		{
			if (_internalRecipe != null)
				Debug.LogError("Modifier recipe already finished, finishing again. Not intended?");

			IInitComponent initComponent = null;
			IList<ITimeComponent> timeComponents = new List<ITimeComponent>(2);
			IStackComponent stackComponent = null;

			var revertibleList = new List<IRevertEffect>(2);

			for (int i = 0; i < _effectBinds.Length; i++)
			{
				var effects = _effectBinds[i];
				if (effects.Count == 0)
					continue;

				for (int j = 0; j < effects.Count; j++)
				{
					if (effects[j] is IRevertEffect { IsRevertible: true } revertEffect)
						revertibleList.Add(revertEffect);
				}

				var effectOn = (EffectOn)(1 << i);

				if (effectOn == EffectOn.Init)
				{
					initComponent = new InitComponent(effects.ToArray());
				}

				if (effectOn == EffectOn.Interval)
				{
					Debug.Assert(_interval > 0, "Interval must be greater than 0");
					timeComponents.Add(new IntervalComponent(_interval, _refreshInterval, effects.ToArray()));
				}

				if (effectOn == EffectOn.Duration)
				{
					Debug.Assert(_duration > 0, "Duration must be greater than 0");
					timeComponents.Add(new DurationComponent(_duration, _refreshDuration, effects.ToArray()));
				}
			}

			_removeEffect?.SetRevertibleEffects(revertibleList.ToArray());

			_internalRecipe = new ModifierInternalRecipe(Id, initComponent, timeComponents.ToArray(), stackComponent, _removeEffect);
		}
	}
}