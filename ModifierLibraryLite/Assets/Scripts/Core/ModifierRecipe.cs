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

		private readonly ITargetComponent _target;

		private List<IEffect>[] _effectBinds;

		private ModifierInternalRecipe _internalRecipe;

		public ModifierRecipe(string id)
		{
			Id = id;
			_target = new TargetComponent();

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
			_duration = duration;
			Effect(new RemoveEffect(), EffectOn.Duration);
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
			IInitComponent initComponent = null;
			IList<ITimeComponent> timeComponents = new List<ITimeComponent>(2);
			IRefreshComponent refreshComponent = null;
			IStackComponent stackComponent = null;

			for (int i = 0; i < _effectBinds.Length; i++)
			{
				var effects = _effectBinds[i];
				if (effects.Count == 0)
					continue;

				var effectOn = (EffectOn)(1 << i);

				if (effectOn == EffectOn.Init)
				{
					initComponent = new InitComponent(_target, effects.ToArray());
				}

				if (effectOn == EffectOn.Interval)
				{
					Debug.Assert(_interval > 0, "Interval must be greater than 0");
					timeComponents.Add(new IntervalComponent(_interval, _target, effects.ToArray()));
				}

				if (effectOn == EffectOn.Duration)
				{
					Debug.Assert(_duration > 0, "Duration must be greater than 0");
					timeComponents.Add(new DurationComponent(_duration, _target, effects.ToArray()));
				}
			}

			_internalRecipe = new ModifierInternalRecipe(Id, (TargetComponent)_target, initComponent, timeComponents.ToArray(),
				refreshComponent, stackComponent);
		}
	}
}