using System.Collections.Generic;

namespace ModiBuff.Core
{
	public sealed class ModifierLessEffects
	{
		public static ModifierLessEffects? Instance { get; private set; }

		private readonly EffectIdManager _idManager;
		private readonly List<ModifierLessEffect> _effectList;

		private ModifierLessEffect[] _effects;

		public ModifierLessEffects(EffectIdManager idManager)
		{
			if (Instance != null)
				return;

			Instance = this;
			_idManager = idManager;
			_effectList = new List<ModifierLessEffect>();
		}

		public void Apply(int id, IUnit target, IUnit source)
		{
#if DEBUG && !MODIBUFF_PROFILE
			if (id >= _effects.Length)
				Logger.LogError($"[ModiBuff] ModifierLessEffects: Effect with id {id} does not exist");
#endif

			_effects[id].Apply(target, source);
		}

		public bool Add(string name, params IEffect[] effects)
		{
			bool valid = true;
			foreach (var effect in effects)
			{
				if (!Validate(effect))
					valid = false;
			}

			if (_idManager.HasId(name))
			{
				valid = false;
				Logger.LogError($"[ModiBuff] ModifierLessEffects: Effect with name {name} already exists");
			}

			if (!valid)
				return false;

			int id = _idManager.GetFreeId(name);
			_effectList.Add(new ModifierLessEffect(effects));
			return true;
		}

		public void Finish()
		{
			_effects = _effectList.ToArray();
		}

		public void Reset()
		{
			_effectList.Clear();
			Instance = null;
		}

		private static bool Validate(IEffect effect)
		{
			bool valid = true;
			if (effect is IRevertEffect revertEffect && revertEffect.IsRevertible)
			{
				valid = false;
				Logger.LogError("[ModiBuff] ModifierLessEffects effects cannot be revertible. " +
				                $"Effect {effect.GetType().Name} is revertible");
			}

			if (effect is IMutableStateEffect mutableStateEffect && mutableStateEffect.UsesMutableState)
			{
				valid = false;
				Logger.LogError("[ModiBuff] ModifierLessEffects effects cannot use mutable state. " +
				                $"Effect {effect.GetType().Name} uses mutable state");
			}

			if (effect is IShallowClone && !(effect is IMutableStateEffect))
			{
				Logger.LogWarning("[ModiBuff] ModifierLessEffects effects shouldn't be clone-only, " +
				                  $"implement IMutableStateEffect if it's not. Effect {effect.GetType().Name}");
			}

			return valid;
		}
	}
}