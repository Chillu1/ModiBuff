using System.Collections.Generic;

namespace ModiBuff.Core
{
	public sealed class ModifierLessEffects
	{
		public static ModifierLessEffects Instance { get; private set; }

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
				Logger.LogError($"ModifierLessEffects: Effect with id {id} does not exist");
#endif

			_effects[id].Apply(target, source);
		}

		public void Add(string name, params IEffect[] effects)
		{
			int id = _idManager.GetFreeId(name);
			_effectList.Add(new ModifierLessEffect(effects));
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
	}
}