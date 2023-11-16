using System;
using System.Collections.Generic;

namespace ModiBuff.Core
{
	/// <summary>
	///		ModifierController is the class that handles all modifiers for a unit
	///		It allows us to add and update modifier by id
	/// </summary>
	/// <remarks>This version supports multiple modifiers of the same type</remarks>
	public sealed class ModifierController
	{
		//A dict with multikey can be used, but we run into problems with modifiers that don't use instance stacking
		private Modifier[] _modifiers;
		private readonly int[] _modifierIndexes;
		private readonly Dictionary<int, int> _modifierIndexesDict;
		private int _modifiersTop;

		private readonly List<DispelReference> _dispellableReferences;

		private readonly List<ModifierReference> _modifiersToRemove;

		public ModifierController()
		{
			_modifiers = new Modifier[Config.ModifierArraySize];
			if (Config.UseDictionaryIndexes)
				_modifierIndexesDict = new Dictionary<int, int>(Config.ModifierIndexDictionarySize);
			else
			{
				_modifierIndexes = new int[ModifierRecipes.GeneratorCount];
				for (int i = 0; i < _modifierIndexes.Length; i++)
					_modifierIndexes[i] = -1;
			}

			_dispellableReferences = new List<DispelReference>(Config.DispellableSize);

			_modifiersToRemove = new List<ModifierReference>(Config.ModifierRemoveSize);
		}

		public void Update(float delta)
		{
			int modifiersTop = _modifiersTop;
			for (int i = 0; i < modifiersTop; i++)
				_modifiers[i].Update(delta);

			int removeCount = _modifiersToRemove.Count;
			if (removeCount == 0)
				return;

			for (int i = 0; i < removeCount; i++)
				Remove(_modifiersToRemove[i]);

			_modifiersToRemove.Clear();
		}

		public ModifierReference[] GetModifierReferences()
		{
			var modifierReferences = new ModifierReference[_modifiersTop];
			for (int i = 0; i < _modifiersTop; i++)
				modifierReferences[i] = new ModifierReference(_modifiers[i].Id, _modifiers[i].GenId);

			return modifierReferences;
		}

		public IModifierDataReference GetModifierDataReference(int id, int genId)
		{
			return GetModifier(id, genId);
		}

		/// <summary>
		///		Gets timer reference, used to update UI/UX
		/// </summary>
		/// <param name="timeComponentNumber">Which timer should be returned, first = 0</param>
		public ITimeReference GetTimer<TTimeComponent>(int id, int genId = 0, int timeComponentNumber = 0)
			where TTimeComponent : ITimeComponent
		{
			return GetModifier(id, genId)?.GetTimer<TTimeComponent>(timeComponentNumber);
		}

		/// <summary>
		///		Get stack reference, used to update UI/UX
		/// </summary>
		public IStackReference GetStackReference(int id, int genId = 0)
		{
			return GetModifier(id, genId)?.GetStackReference();
		}

		/// <summary>
		///		Gets state from effect, used to display values in UI
		/// </summary>
		/// <param name="stateNumber">Which state should be returned, 0 = first</param>
		public TData GetEffectState<TData>(int id, int genId = 0, int stateNumber = 0) where TData : struct
		{
			var modifier = GetModifier(id, genId);
			if (modifier != null)
				return modifier.GetEffectState<TData>(stateNumber);

			Logger.LogWarning($"[ModiBuff] Couldn't find state info, {typeof(TData)} at number {stateNumber}, " +
			                  $"id: {id}, genId: {genId}");
			return default;
		}

		public void Add(int id, IUnit target, IUnit source)
		{
			ref readonly var tag = ref ModifierRecipes.GetTag(id);

			if (!tag.HasTag(TagType.IsInstanceStackable))
			{
				bool useDictionaryIndexes = Config.UseDictionaryIndexes;

				bool exists;
				int index;
				if (useDictionaryIndexes)
					exists = _modifierIndexesDict.TryGetValue(id, out index);
				else
				{
					index = _modifierIndexes[id];
					exists = index != -1;
				}

				if (exists)
				{
					var existingModifier = _modifiers[index];
					//TODO should we update the modifier targets when init/refreshing/stacking?
					existingModifier.UpdateSource(source);
					if ((tag & TagType.IsInit) != 0)
						existingModifier.Init();
					if ((tag & TagType.IsRefresh) != 0)
						existingModifier.Refresh();
					if ((tag & TagType.IsStack) != 0)
						existingModifier.Stack();

					return;
				}

				if (useDictionaryIndexes)
					_modifierIndexesDict.Add(id, _modifiersTop);
				else
					_modifierIndexes[id] = _modifiersTop;
			}

			if (_modifiersTop == _modifiers.Length)
				Array.Resize(ref _modifiers, _modifiers.Length << 1);

			var modifier = ModifierPool.Instance.Rent(id);

			//TODO Do we want to save the sender of the original modifier? Ex. for thorns. Because owner is always the owner of the modifier instance
			modifier.UpdateSingleTargetSource(target, source);

			_modifiers[_modifiersTop++] = modifier;
			if (tag.HasTag(TagType.IsInit))
				modifier.Init();
			if (tag.HasTag(TagType.IsStack) && !tag.HasTag(TagType.ZeroDefaultStacks))
				modifier.Stack();
		}

		public bool Contains(int id, int genId = -1)
		{
			if (!ModifierRecipes.GetTag(id).HasTag(TagType.IsInstanceStackable))
				return Config.UseDictionaryIndexes ? _modifierIndexesDict.ContainsKey(id) : _modifierIndexes[id] != -1;

			if (genId == -1)
			{
				for (int i = 0; i < _modifiersTop; i++)
					if (_modifiers[i].Id == id)
						return true;
			}
			else
			{
				for (int i = 0; i < _modifiersTop; i++)
				{
					var modifier = _modifiers[i];
					if (modifier.Id == id && modifier.GenId == genId)
						return true;
				}
			}

			return false;
		}

		internal void RegisterDispel(DispelType dispel, RemoveEffect effect)
		{
			_dispellableReferences.Add(new DispelReference(effect, dispel));
		}

		public void Dispel(DispelType dispelType, IUnit target, IUnit source)
		{
			for (int i = 0; i < _dispellableReferences.Count;)
			{
				var dispelReference = _dispellableReferences[i];
				if (dispelReference.Type.HasAny(dispelType))
				{
					dispelReference.Effect.Effect(target, source);
					_dispellableReferences.RemoveAt(i);
					continue;
				}

				i++;
			}
		}

		public void PrepareRemove(int id, int genId)
		{
			_modifiersToRemove.Add(new ModifierReference(id, genId));
		}

		public void ModifierAction(int id, int genId, ModifierAction action)
		{
#if DEBUG && !MODIBUFF_PROFILE
			ref readonly var tag = ref ModifierRecipes.GetTag(id);
#endif
			var modifier = GetModifier(id, genId);

			if (modifier == null)
			{
#if DEBUG && !MODIBUFF_PROFILE
				Logger.LogError($"[ModiBuff] Tried to call modifier action {action} on a modifier that " +
				                $"doesn't exist, id: {id}, genId: {genId}");
#endif
				return;
			}

			if ((action & Core.ModifierAction.Refresh) != 0)
			{
#if DEBUG && !MODIBUFF_PROFILE
				if (!tag.HasTag(TagType.IsRefresh))
					Logger.LogWarning("[ModiBuff] ModifierAction: Refresh was called on a " +
					                  "modifier that doesn't have a refresh flag set");
#endif
				modifier.Refresh();
			}

			if ((action & Core.ModifierAction.ResetStacks) != 0)
			{
#if DEBUG && !MODIBUFF_PROFILE
				if (!tag.HasTag(TagType.IsStack))
					Logger.LogWarning("[ModiBuff] ModifierAction: ResetStacks was called on a " +
					                  "modifier that doesn't have a stack flag set");
#endif
				modifier.ResetStacks();
			}

			if ((action & Core.ModifierAction.Stack) != 0)
			{
#if DEBUG && !MODIBUFF_PROFILE
				if (!tag.HasTag(TagType.IsStack))
					Logger.LogWarning("[ModiBuff] ModifierAction: Stack was called on a " +
					                  "modifier that doesn't have a stack flag set");
#endif
				modifier.Stack();
			}
		}

		public void Remove(in ModifierReference modifierReference)
		{
			if (!ModifierRecipes.GetTag(modifierReference.Id).HasTag(TagType.IsInstanceStackable))
			{
#if DEBUG && !MODIBUFF_PROFILE
				bool modifierExists = Config.UseDictionaryIndexes
					? _modifierIndexesDict.ContainsKey(modifierReference.Id)
					: _modifierIndexes[modifierReference.Id] != -1;

				if (!modifierExists)
				{
					Logger.LogError("[ModiBuff] Tried to remove a modifier that doesn't exist on entity, id: " +
					                $"{modifierReference.Id}, genId: {modifierReference.GenId}");
					return;
				}
#endif

				int modifierIndex;
				if (Config.UseDictionaryIndexes)
				{
					modifierIndex = _modifierIndexesDict[modifierReference.Id];
					ModifierPool.Instance.Return(_modifiers[modifierIndex]);
					_modifiers[modifierIndex] = _modifiers[--_modifiersTop];
					_modifiers[_modifiersTop] = null;
					_modifierIndexesDict.Remove(modifierReference.Id);
					return;
				}

				modifierIndex = _modifierIndexes[modifierReference.Id];
				ModifierPool.Instance.Return(_modifiers[modifierIndex]);
				_modifiers[modifierIndex] = _modifiers[--_modifiersTop];
				_modifiers[_modifiersTop] = null;
				_modifierIndexes[modifierReference.Id] = -1;
				return;
			}

			for (int i = 0; i < _modifiersTop; i++)
			{
				var modifier = _modifiers[i];
				if (modifier.Id == modifierReference.Id && modifier.GenId == modifierReference.GenId)
				{
					ModifierPool.Instance.Return(modifier);
					_modifiers[i] = _modifiers[--_modifiersTop]; //TODO This switching might cause some order issues
					_modifiers[_modifiersTop] = null;
					break;
				}
			}
		}

		/// <summary>
		///		Returns all modifiers back to the pool
		/// </summary>
		public void Clear()
		{
			for (int i = 0; i < _modifiersTop; i++)
			{
				ModifierPool.Instance.Return(_modifiers[i]);
				_modifiers[i] = null;
			}

			if (Config.UseDictionaryIndexes)
				_modifierIndexesDict.Clear();
			else
				for (int i = 0; i < _modifierIndexes.Length; i++)
					_modifierIndexes[i] = -1;

			_modifiersTop = 0;

			_modifiersToRemove.Clear();
		}

		public SaveData SaveState()
		{
			//There's a chance(?) we have modifiers to remove when we're saving. We should remove them before saving
			if (_modifiersToRemove.Count > 0)
			{
				for (int i = 0; i < _modifiersToRemove.Count; i++)
					Remove(_modifiersToRemove[i]);
				_modifiersToRemove.Clear();
			}

			Modifier.SaveData[] modifiersSaveData = new Modifier.SaveData[_modifiersTop];
			for (int i = 0; i < _modifiersTop; i++)
				modifiersSaveData[i] = _modifiers[i].SaveState();
			return new SaveData(modifiersSaveData);
		}

		public void LoadState(SaveData saveData, IUnit owner)
		{
			for (int i = 0; i < saveData.ModifiersSaveData.Count; i++)
			{
				var modifierSaveData = saveData.ModifiersSaveData[i];
				int id = ModifierIdManager.GetNewId(modifierSaveData.Id);

				ref readonly var tag = ref ModifierRecipes.GetTag(id);

				if (!tag.HasTag(TagType.IsInstanceStackable))
				{
					if (Config.UseDictionaryIndexes)
						_modifierIndexesDict.Add(id, _modifiersTop);
					else
						_modifierIndexes[id] = _modifiersTop;
				}

				var modifier = ModifierPool.Instance.Rent(id);
				modifier.LoadState(modifierSaveData, owner);
				_modifiers[_modifiersTop++] = modifier;
				if (tag.HasTag(TagType.IsInit))
					modifier.InitLoad();
			}
		}

		private Modifier GetModifier(int id, int genId)
		{
			if (!ModifierRecipes.GetTag(id).HasTag(TagType.IsInstanceStackable))
			{
				if (Config.UseDictionaryIndexes)
				{
					return _modifierIndexesDict.TryGetValue(id, out int modifierIndex)
						? _modifiers[modifierIndex]
						: null;
				}

				return _modifierIndexes[id] != -1 ? _modifiers[_modifierIndexes[id]] : null;
			}

			for (int i = 0; i < _modifiersTop; i++)
			{
				var modifier = _modifiers[i];
				if (modifier.Id == id && modifier.GenId == genId)
					return modifier;
			}

			return null;
		}

		public readonly struct SaveData
		{
			public readonly IReadOnlyList<Modifier.SaveData> ModifiersSaveData;

#if JSON_SERIALIZATION && (NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_1_OR_GREATER || NET5_0_OR_GREATER || NET462_OR_GREATER || NETCOREAPP2_1_OR_GREATER)
			[System.Text.Json.Serialization.JsonConstructor]
#endif
			public SaveData(IReadOnlyList<Modifier.SaveData> modifiersSaveData)
			{
				ModifiersSaveData = modifiersSaveData;
			}
		}
	}
}