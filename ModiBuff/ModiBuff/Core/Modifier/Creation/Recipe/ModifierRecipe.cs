using System;
using System.Collections.Generic;
using System.Linq;

namespace ModiBuff.Core
{
	/// <summary>
	///		High level API for creating modifiers.
	/// </summary>
	public sealed class ModifierRecipe : IModifierRecipe, IEquatable<ModifierRecipe>, IComparable<ModifierRecipe>
	{
		public int Id { get; }
		public string Name { get; }
		private readonly string _displayName;
		private readonly string _description;

		public readonly ModifierIdManager IdManager; //TODO Refactor to make it private/not needed

		private bool _isInstanceStackable;
		private bool _isAura;
		private TagType _tag;

		private bool _oneTimeInit;

		private bool _currentIsInterval;
		private float _interval;
		private float _duration;

		private EffectWrapper _removeEffectWrapper;
		private EffectWrapper _eventRegisterWrapper;
		private EffectWrapper _callbackRegisterWrapper;

		private readonly List<EffectWrapper> _effectWrappers;

		private bool _refreshDuration, _refreshInterval;

		private WhenStackEffect _whenStackEffect;
		private int _maxStacks;
		private int _everyXStacks;
		private float _independentStackTime;

		private bool _hasApplyChecks;
		private List<ICheck> _applyCheckList;
		private List<Func<IUnit, bool>> _applyFuncCheckList;

		private bool _hasEffectChecks;
		private List<ICheck> _effectCheckList;
		private List<Func<IUnit, bool>> _effectFuncCheckList;

		public ModifierRecipe(int id, string name, string displayName, string description, ModifierIdManager idManager)
		{
			Id = id;
			Name = name;
			_displayName = displayName;
			_description = description;
			IdManager = idManager;

			_tag = (TagType)Config.DefaultTag;

			_effectWrappers = new List<EffectWrapper>(3);
		}

		//---Misc---

		/// <summary>
		///		Makes is possible to stack multiple modifiers of the same type on one target.
		/// </summary>
		public ModifierRecipe InstanceStackable()
		{
			_isInstanceStackable = true;
			return this;
		}

		/// <summary>
		///		Determines if the modifier should use <see cref="SingleTargetComponent"/> or <see cref="MultiTargetComponent"/>.
		/// </summary>
		public ModifierRecipe Aura()
		{
			_isAura = true;
			return this;
		}

		public ModifierRecipe Tag(int tag) => Tag((TagType)tag);

		/// <summary>
		///		Ads a tag to the modifier
		/// </summary>
		public ModifierRecipe Tag(TagType tag)
		{
			_tag |= tag;
			return this;
		}

		/// <summary>
		///		This will set the tag directly, without adding, it might remove other tags.
		///		Will remove the default tag, if set.
		/// </summary>
		public ModifierRecipe SetTag(TagType tag)
		{
			_tag = tag;
			return this;
		}

		public ModifierRecipe RemoveTag(TagType tag)
		{
			_tag &= ~tag;
			return this;
		}

		//---ApplyChecks---

		public ModifierRecipe ApplyCheck(Func<IUnit, bool> check)
		{
			if (_applyFuncCheckList == null)
				_applyFuncCheckList = new List<Func<IUnit, bool>>();
			_applyFuncCheckList.Add(check);
			_hasApplyChecks = true;
			return this;
		}

		public ModifierRecipe ApplyCheck(ICheck check)
		{
			if (_applyCheckList == null)
				_applyCheckList = new List<ICheck>();
			_applyCheckList.Add(check);
			_hasApplyChecks = true;
			return this;
		}

		//---EffectChecks---

		public ModifierRecipe EffectCheck(Func<IUnit, bool> check)
		{
			if (_effectFuncCheckList == null)
				_effectFuncCheckList = new List<Func<IUnit, bool>>();
			_effectFuncCheckList.Add(check);
			_hasEffectChecks = true;
			return this;
		}

		public ModifierRecipe EffectCheck(ICheck check)
		{
			if (_effectCheckList == null)
				_effectCheckList = new List<ICheck>();
			_effectCheckList.Add(check);
			_hasEffectChecks = true;
			return this;
		}

		//---Actions---

		/// <summary>
		///		Only trigger Init effects once. When adding modifier.
		/// </summary>
		/// <remarks>Works well for auras</remarks>
		public ModifierRecipe OneTimeInit()
		{
			_oneTimeInit = true;
			return this;
		}

		/// <summary>
		///		How many seconds should pass between the interval effects get applied.
		/// </summary>
		/// <param name="affectedByStatusResistance">Should the interval be affected by status resistance</param>
		public ModifierRecipe Interval(float interval)
		{
			_interval = interval;
			_currentIsInterval = true;
			return this;
		}

		/// <summary>
		///		How many seconds should pass before the duration effects get triggered (usually modifier removal)
		/// </summary>
		public ModifierRecipe Duration(float duration)
		{
			_duration = duration;
			_currentIsInterval = false;
			return this;
		}

		/// <summary>
		///		How many seconds should pass before the modifier gets removed.
		/// </summary>
		public ModifierRecipe Remove(float duration)
		{
			Duration(duration);
			_removeEffectWrapper = new EffectWrapper(new RemoveEffect(Id), EffectOn.Duration);
			_effectWrappers.Add(_removeEffectWrapper);
			return this;
		}

		/// <summary>
		///		Adds a basic remove effect, that should be triggered on either stack, or callback
		/// </summary>
		public ModifierRecipe Remove(RemoveEffectOn removeEffectOn = RemoveEffectOn.Callback)
		{
			_removeEffectWrapper = new EffectWrapper(new RemoveEffect(Id), removeEffectOn.ToEffectOn());
			_effectWrappers.Add(_removeEffectWrapper);
			return this;
		}

		/// <summary>
		///		If a modifier gets applied to a target that already has the modifier, should the interval or duration be reset?
		///		Order matters, call after <see cref="Interval(float,bool)"/> or <see cref="Duration(float)"/>
		/// </summary>
		/// <remarks> This is most often used to refresh duration of the modifier, like refreshing DoT modifiers </remarks>
		public ModifierRecipe Refresh()
		{
			if (_interval <= 0 && _duration <= 0)
			{
#if DEBUG && !MODIBUFF_PROFILE
				Logger.LogWarning("[ModiBuff] Refresh() called without a duration or interval set, " +
				                  "defaulting to duration");
#endif
				Refresh(RefreshType.Duration);
				return this;
			}

			if (_currentIsInterval)
			{
				Refresh(RefreshType.Interval);
				return this;
			}

			Refresh(RefreshType.Duration);
			return this;
		}

		/// <summary>
		///		If a modifier gets applied to a target that already has the modifier, should the interval or duration be reset?
		/// </summary>
		/// <remarks> This is most often used to refresh duration of the modifier, like refreshing DoT modifiers </remarks>
		public ModifierRecipe Refresh(RefreshType type)
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
#if DEBUG && !MODIBUFF_PROFILE
					Logger.LogError($"[ModiBuff] Unknown refresh type: {type}");
#endif
					return this;
			}

			return this;
		}

		/// <summary>
		/// 	Adds stack functionality to the modifier. A stack is added every time the modifier gets re-added to the target.
		/// </summary>
		/// <param name="whenStackEffect">When should the stack effects be triggered.</param>
		/// <param name="value">Values that can be used by the stack effects.</param>
		/// <param name="maxStacks">Max amount of stacks that can be applied.</param>
		/// <param name="everyXStacks">If <see cref="whenStackEffect"/> is set to
		/// <see cref="whenStackEffect.EveryXStacks"/>, this value will be used to determine when the stack effects should be triggered.</param>
		/// <param name="independentStackTime">If set, will add a timer for each stack, and remove a stack after a timer expires</param>
		public ModifierRecipe Stack(WhenStackEffect whenStackEffect, int maxStacks = -1,
			int everyXStacks = -1, float independentStackTime = -1)
		{
			_whenStackEffect = whenStackEffect;
			_maxStacks = maxStacks;
			_everyXStacks = everyXStacks;
			_independentStackTime = independentStackTime;
			return this;
		}

		//---Effects---

		/// <summary>
		///		Add an effect to the modifier.
		/// </summary>
		/// <param name="effect">Effects that get applied on specific actions (init, stack, interval, duration). </param>
		/// <param name="effectOn">When the effect should trigger (init, stack, interval, duration). Can be multiple.</param>
		/// <param name="targeting">Who should be the target and owner of the applied modifier. For further information, see <see cref="ModiBuff.Core.Targeting"/></param>
		public ModifierRecipe Effect(IEffect effect, EffectOn effectOn, Targeting targeting = Targeting.TargetSource)
		{
			if (ManualOnlyEffects.IsManualOnlyEffect(effect))
			{
				Logger.LogError($"[ModiBuff] Effect: {effect} isn't supported in ModifierRecipes yet, " +
				                "use manual modifier generation if the effect is needed");
				return this;
			}

			if (effect is IModifierIdOwner modifierIdOwner)
				modifierIdOwner.SetModifierId(Id);

			if (effect is RemoveEffect)
			{
#if DEBUG && !MODIBUFF_PROFILE
				Logger.LogWarning("[ModiBuff] Adding a remove effect through Effect() is not recommended, " +
				                  "use Remove(RemoveEffectOn) or Remove(float) instead");
				if (effectOn.HasFlag(EffectOn.Stack))
				{
					Logger.LogError("[ModiBuff] Adding a remove effect through stack is not currently supported");
					effectOn &= ~EffectOn.Stack;
				}
#endif
				_removeEffectWrapper = new EffectWrapper(effect, effectOn);
				_effectWrappers.Add(_removeEffectWrapper);
				return this;
			}

			_effectWrappers.Add(new EffectWrapper(effect, effectOn));
			return this;
		}

		/// <summary>
		///		Trigger a modifier action on the target on <see cref="effectOn"/>.
		/// </summary>
		/// <example> Usually used to refresh the duration/interval timers or reset stacks on game logic callbacks </example>
		public ModifierRecipe ModifierAction(ModifierAction modifierAction, EffectOn effectOn)
		{
#if DEBUG && !MODIBUFF_PROFILE
			ValidateModifierAction(modifierAction, effectOn);
#endif
			Effect(new ModifierActionEffect(modifierAction, Id), effectOn);
			return this;
		}

		public ModifierRecipe Event<TEvent>(TEvent @event)
		{
			_eventRegisterWrapper = new EffectWrapper(new EventRegisterEffect<TEvent>(@event), EffectOn.Init);
			_effectWrappers.Add(_eventRegisterWrapper);
			return this;
		}

		/// <summary>
		///		Registers a callback register effect to a unit, will trigger all <see cref="EffectOn.Callback"/>
		///		effects when <see cref="callbackType"/> is triggered.
		/// </summary>
		public ModifierRecipe Callback<TCallback>(TCallback callbackType)
		{
			var effect = new CallbackRegisterEffect<TCallback>(callbackType);
			_callbackRegisterWrapper = new EffectWrapper(effect, EffectOn.Init);
			_effectWrappers.Add(_callbackRegisterWrapper);
			return this;
		}

		/// <summary>
		///		Registers a callback effect to a unit, will trigger the callback when <see cref="callbackType"/> is triggered.
		///		It will NOT trigger any EffectOn.<see cref="EffectOn.Callback"/> effects, only the supplied callback.
		/// </summary>
		public ModifierRecipe Callback<TCallback>(TCallback callbackType, UnitCallback callback,
			bool isRevertible = true)
		{
			Effect(new CallbackRegisterDelegateEffect<TCallback>(callbackType, callback, isRevertible), EffectOn.Init);
			return this;
		}

		// public ModifierRecipe ReactCallback<TReact, TEvent>(TReact reactType, Func<ReactState, IEffect, TEvent> func)
		// {
		// 	return this;
		// }

		//---Modifier Generation---

		public IModifierGenerator CreateModifierGenerator()
		{
#if DEBUG && !MODIBUFF_PROFILE
			Validate();
#endif
			//Update tag based on settings
			for (int i = 0; i < _effectWrappers.Count; i++)
			{
				var effectWrapper = _effectWrappers[i];
				if (effectWrapper.EffectOn.HasFlag(EffectOn.Init))
					_tag |= TagType.IsInit;
				if (effectWrapper.EffectOn.HasFlag(EffectOn.Stack))
					_tag |= TagType.IsStack;
			}

			if (_refreshDuration || _refreshInterval)
				_tag |= TagType.IsRefresh;
			if (_isInstanceStackable)
				_tag |= TagType.IsInstanceStackable;

			var data = new ModifierRecipeData(Id, Name, _effectWrappers, _removeEffectWrapper, _eventRegisterWrapper,
				_callbackRegisterWrapper, _hasApplyChecks, _applyCheckList, _hasEffectChecks, _effectCheckList,
				_applyFuncCheckList, _effectFuncCheckList, _isAura, _tag, _oneTimeInit, _interval, _duration,
				_refreshDuration, _refreshInterval, _whenStackEffect, _maxStacks, _everyXStacks, _independentStackTime);
			return new ModifierGenerator(in data);
		}

		public ModifierInfo CreateModifierInfo()
		{
			return new ModifierInfo(Id, Name, _displayName, _description);
		}

		public TagType GetTag() => _tag;

		private static void ValidateModifierAction(ModifierAction modifierAction, EffectOn effectOn)
		{
			string initialMessage = $"[ModiBuff] ModifierAction set to {modifierAction}, and effectOn to {effectOn}. ";

			if (modifierAction == Core.ModifierAction.Refresh && effectOn == EffectOn.Init)
				Logger.LogError(initialMessage +
				                "Time components always get refreshed on init (if refreshable), no need to add a modifier action for it");

			if (modifierAction == Core.ModifierAction.ResetStacks && effectOn == EffectOn.Init)
				Logger.LogError(initialMessage +
				                "Stack component will always reset on init, removing the purpose of it, use init effects instead");
		}

		private void Validate()
		{
			bool validRecipe = true;

			if (_effectWrappers.Any(w => w.EffectOn.HasFlag(EffectOn.Interval)) && _interval == 0)
			{
				validRecipe = false;
				Logger.LogError("[ModiBuff] Interval not set, but we have interval effects, for modifier: " +
				                "" + Name + " id: " + Id);
			}

			if (_effectWrappers.All(w => !w.EffectOn.HasFlag(EffectOn.Interval)) && _interval != 0)
			{
				validRecipe = false;
				Logger.LogError("[ModiBuff] Interval set, but no interval effects set, for modifier: " +
				                "" + Name + " id: " + Id);
			}

			if (_effectWrappers.Any(w => w.EffectOn.HasFlag(EffectOn.Duration)) && _duration == 0)
			{
				validRecipe = false;
				Logger.LogError("[ModiBuff] Duration not set, but we have duration effects, for modifier: " +
				                "" + Name + " id: " + Id);
			}

			if (_effectWrappers.All(w => !w.EffectOn.HasFlag(EffectOn.Duration)) && _duration != 0)
			{
				validRecipe = false;
				Logger.LogError("[ModiBuff] Duration set, but no duration effects set, for modifier: " +
				                "" + Name + " id: " + Id);
			}

			if (_effectWrappers.Any(w => w.EffectOn.HasFlag(EffectOn.Stack)) &&
			    _whenStackEffect == WhenStackEffect.None)
			{
				validRecipe = false;
				Logger.LogError("[ModiBuff] Stack effects set, but no stack effect type set, for modifier: " +
				                "" + Name + " id: " + Id);
			}

			if (_effectWrappers.All(w => !w.EffectOn.HasFlag(EffectOn.Stack)) &&
			    _whenStackEffect != WhenStackEffect.None)
			{
				validRecipe = false;
				Logger.LogError("[ModiBuff] Stack effect type set, but no stack effects set, for modifier: " +
				                "" + Name + " id: " + Id);
			}

			if (_refreshInterval && _interval == 0)
			{
				validRecipe = false;
				Logger.LogError("[ModiBuff] Refresh interval set, but interval is 0, for modifier: " +
				                "" + Name + " id: " + Id);
			}

			if (_refreshDuration && _duration == 0)
			{
				validRecipe = false;
				Logger.LogError("[ModiBuff] Refresh duration set, but duration is 0, for modifier: " +
				                "" + Name + " id: " + Id);
			}

			if (_effectWrappers.Any(w => w.EffectOn.HasFlag(EffectOn.Event)) && _eventRegisterWrapper == null)
			{
				validRecipe = false;
				Logger.LogError("[ModiBuff] Effects on event set, but no event registration type set, " +
				                "for modifier: " + Name + " id: " + Id);
			}

			if (_eventRegisterWrapper != null && !_effectWrappers.Any(w => w.EffectOn.HasFlag(EffectOn.Event)) &&
			    _removeEffectWrapper?.EffectOn != EffectOn.Event)
			{
				validRecipe = false;
				Logger.LogError("[ModiBuff] Event registration type set, but no effects on event set, " +
				                "for modifier: " + Name + " id: " + Id);
			}

			if (_effectWrappers.Any(w => w.EffectOn.HasFlag(EffectOn.Callback)) && _callbackRegisterWrapper == null)
			{
				validRecipe = false;
				Logger.LogError("[ModiBuff] Effects on callback set, but no callback registration type set, " +
				                "for modifier: " + Name + " id: " + Id);
			}

			if (_callbackRegisterWrapper != null && !_effectWrappers.Any(w => w.EffectOn.HasFlag(EffectOn.Callback)) &&
			    _removeEffectWrapper?.EffectOn != EffectOn.Callback)
			{
				validRecipe = false;
				Logger.LogError("[ModiBuff] Callback registration type set, but no effects on callback set, " +
				                "for modifier: " + Name + " id: " + Id);
			}

			if (!validRecipe)
				Logger.LogError($"[ModiBuff] Recipe validation failed for {Name}, with Id: {Id}, " +
				                $"see above for more info.");
		}

		public int CompareTo(ModifierRecipe other) => Id.CompareTo(other.Id);

		public bool Equals(ModifierRecipe other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Id == other.Id;
		}

		public override bool Equals(object obj)
		{
			return ReferenceEquals(this, obj) || obj is ModifierRecipe other && Equals(other);
		}

		public override int GetHashCode() => Id;
	}
}