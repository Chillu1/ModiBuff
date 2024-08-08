using System;
using System.Collections.Generic;

namespace ModiBuff.Core
{
	/// <summary>
	///		High level API for creating modifiers.
	/// </summary>
	public sealed partial class ModifierRecipe : IModifierRecipe, IEquatable<ModifierRecipe>,
		IComparable<ModifierRecipe>
	{
		public int Id { get; }
		public string Name { get; }
		private readonly string _displayName;
		private readonly string _description;

		public readonly ModifierIdManager IdManager; //TODO Refactor to make it private/not needed
		private readonly EffectTypeIdManager _effectTypeIdManager; //TODO Refactor to make it private/not needed

		private bool _isInstanceStackable;
		private bool _isAura;
		private TagType _tag;

		private bool _oneTimeInit;

		private bool _currentIsInterval;
		private float _interval;
		private float _duration;

		private RemoveEffectWrapper _removeEffectWrapper;
		private EffectWrapper _dispelRegisterWrapper;
		private EffectWrapper _callbackUnitRegisterWrapper;
		private readonly List<EffectWrapper> _callbackEffectRegisterWrappers;
		private EffectWrapper _callbackEffectUnitsRegisterWrapper;

		private readonly List<EffectWrapper> _effectWrappers;

		private bool _refreshDuration, _refreshInterval;

		private WhenStackEffect _whenStackEffect;
		private int _maxStacks;
		private int _everyXStacks;
		private float _singleStackTime;
		private float _independentStackTime;

		private bool _hasApplyChecks;
		private List<ICheck> _applyCheckList;
		private List<Func<IUnit, bool>> _applyFuncCheckList;

		private bool _hasEffectChecks;
		private List<ICheck> _effectCheckList;
		private List<Func<IUnit, bool>> _effectFuncCheckList;

		private ModifierAction _modifierActions;

		private readonly List<SaveInstruction> _saveInstructions;
		private readonly List<Type> _unsavableEffects;

		public ModifierRecipe(int id, string name, string displayName, string description,
			ModifierIdManager idManager, EffectTypeIdManager effectTypeIdManager)
		{
			Id = id;
			Name = name;
			_displayName = displayName;
			_description = description;
			IdManager = idManager;
			_effectTypeIdManager = effectTypeIdManager;

			_tag = (TagType)Config.DefaultTag;

			_effectWrappers = new List<EffectWrapper>(3);
			_callbackEffectRegisterWrappers = new List<EffectWrapper>();

			_saveInstructions = new List<SaveInstruction>
				{ new SaveInstruction.Initialize(name, displayName, description) };
			_unsavableEffects = new List<Type>();
		}

		//---Misc---

		/// <summary>
		///		Makes is possible to stack multiple modifiers of the same type on one target.
		/// </summary>
		public ModifierRecipe InstanceStackable()
		{
			_isInstanceStackable = true;
			_saveInstructions.Add(new SaveInstruction.InstanceStackable());
			return this;
		}

		/// <summary>
		///		Determines if the modifier should use <see cref="SingleTargetComponent"/> or <see cref="MultiTargetComponent"/>.
		/// </summary>
		public ModifierRecipe Aura()
		{
			_isAura = true;
			_saveInstructions.Add(new SaveInstruction.Aura());
			return this;
		}

		public ModifierRecipe Tag(int tag) => Tag((TagType)tag);

		/// <summary>
		///		Ads a tag to the modifier
		/// </summary>
		public ModifierRecipe Tag(TagType tag)
		{
#if DEBUG && !MODIBUFF_PROFILE
			ValidateTag(tag);
#endif
			_tag |= tag;
			_saveInstructions.Add(new SaveInstruction.Tag(SaveInstruction.Tag.Type.Add, tag));
			return this;
		}

		/// <summary>
		///		This will set the tag directly, without adding, it might remove other tags.
		///		Will remove the default tag, if set.
		/// </summary>
		public ModifierRecipe SetTag(TagType tag)
		{
#if DEBUG && !MODIBUFF_PROFILE
			ValidateTag(tag);
#endif
			_tag = tag;
			_saveInstructions.Add(new SaveInstruction.Tag(SaveInstruction.Tag.Type.Set, tag));
			return this;
		}

		public ModifierRecipe RemoveTag(TagType tag)
		{
			_tag &= ~tag;
			_saveInstructions.Add(new SaveInstruction.Tag(SaveInstruction.Tag.Type.Remove, tag));
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
			_saveInstructions.Add(new SaveInstruction.OneTimeInit());
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
			_saveInstructions.Add(new SaveInstruction.Interval(interval));
			return this;
		}

		/// <summary>
		///		How many seconds should pass before the duration effects get triggered (usually modifier removal)
		/// </summary>
		public ModifierRecipe Duration(float duration)
		{
			DurationInternal(duration);
			_saveInstructions.Add(new SaveInstruction.Duration(duration));
			return this;
		}

		/// <summary>
		///		How many seconds should pass before the modifier gets removed.
		/// </summary>
		public ModifierRecipe Remove(float duration)
		{
			DurationInternal(duration);
			AddRemoveEffect(EffectOn.Duration);
			_saveInstructions.Add(new SaveInstruction.Remove(SaveInstruction.Remove.Type.Duration, EffectOn.Duration,
				duration));
			return this;
		}

		/// <summary>
		///		How many seconds should pass before the modifier gets removed.
		/// </summary>
		/// <remarks>OVERWRITES all previous remove effects.</remarks>
		public ModifierRecipe RemoveApplier(float duration, ApplierType applierType, bool hasApplyChecks)
		{
			Duration(duration);
			_removeEffectWrapper =
				new RemoveEffectWrapper(new RemoveEffect(Id, applierType, hasApplyChecks), EffectOn.Duration);
			return this;
		}

		/// <summary>
		///		Adds a basic remove effect, that should be triggered on either stack, or callback
		/// </summary>
		public ModifierRecipe Remove(RemoveEffectOn removeEffectOn)
		{
			AddRemoveEffect(removeEffectOn.ToEffectOn());
			_saveInstructions.Add(new SaveInstruction.Remove(SaveInstruction.Remove.Type.RemoveOn,
				removeEffectOn.ToEffectOn()));
			return this;
		}

		/// <summary>
		///		Adds a basic remove effect, that should be triggered on either stack, or callback
		/// </summary>
		/// <remarks>OVERWRITES all previous remove effects.</remarks>
		//public ModifierRecipe RemoveApplier(RemoveEffectOn removeEffectOn, ApplierType applierType, bool hasApplyChecks)
		//{
		//	_removeEffectWrapper =
		//		new RemoveEffectWrapper(new RemoveEffect(Id, applierType, hasApplyChecks), removeEffectOn.ToEffectOn());
		//	return this;
		//}

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
			}

			_saveInstructions.Add(new SaveInstruction.Refresh(type));
			return this;
		}

		/// <summary>
		/// 	Adds stack functionality to the modifier. A stack is added every time the modifier gets re-added to the target.
		/// </summary>
		/// <param name="whenStackEffect">When should the stack effects be triggered.</param>
		/// <param name="maxStacks">Max amount of stacks that can be applied.</param>
		/// <param name="everyXStacks">If <see cref="whenStackEffect"/> is set to
		/// <see cref="whenStackEffect.EveryXStacks"/>, this value will be used to determine when the stack effects should be triggered.</param>
		/// <param name="singleStackTime">Adds a single timer, and removes and reverts all stacks after the timer expires</param>
		/// <param name="independentStackTime">Adds a timer for each stack, and removes a stack after a timer expires</param>
		public ModifierRecipe Stack(WhenStackEffect whenStackEffect, int maxStacks = -1,
			int everyXStacks = -1, float singleStackTime = -1, float independentStackTime = -1)
		{
			_whenStackEffect = whenStackEffect;
			_maxStacks = maxStacks;
			_everyXStacks = everyXStacks;
			_singleStackTime = singleStackTime;
			_independentStackTime = independentStackTime;
			_saveInstructions.Add(new SaveInstruction.Stack(whenStackEffect, maxStacks, everyXStacks,
				singleStackTime, independentStackTime));
			return this;
		}

		public ModifierRecipe Dispel(DispelType dispelType = DispelType.Basic)
		{
			AddRemoveEffect(EffectOn.None);

			var dispelRegister = new DispelRegisterEffect(dispelType);
			_dispelRegisterWrapper = new EffectWrapper(dispelRegister, EffectOn.Init);
			_effectWrappers.Add(_dispelRegisterWrapper);
			_saveInstructions.Add(new SaveInstruction.Dispel(dispelType));
			return this;
		}

		private void AddRemoveEffect(EffectOn effectOn)
		{
			if (_removeEffectWrapper != null)
			{
				_removeEffectWrapper.AddEffectOn(effectOn);
				return;
			}

			_removeEffectWrapper = new RemoveEffectWrapper(new RemoveEffect(Id), effectOn);
		}

		//---Effects---

		/// <summary>
		///		Add an effect to the modifier.
		/// </summary>
		/// <param name="effect">Effects that get applied on specific actions (init, stack, interval, duration). </param>
		/// <param name="effectOn">When the effect should trigger (init, stack, interval, duration). Can be multiple.</param>
		/// <param name="targeting">Who should be the target and owner of the applied modifier. For further information, see <see cref="ModiBuff.Core.Targeting"/></param>
		public ModifierRecipe Effect(IEffect effect, EffectOn effectOn)
		{
			if (ManualOnlyEffects.IsManualOnlyEffect(effect))
			{
				Logger.LogError($"[ModiBuff] Effect: {effect} isn't supported in ModifierRecipes yet, " +
				                "use manual modifier generation if the effect is needed");
				return this;
			}

			if (effect is ISaveableRecipeEffect savableRecipe)
			{
				if (SpecialInstructionEffects.IsSpecialInstructionEffect(effect))
					Logger.LogWarning("[ModiBuff] Saving recipe for a special instruction effect, " +
					                  $"remove {nameof(ISaveableRecipeEffect)} implementation from the effect");

				_saveInstructions.Add(new SaveInstruction.Effect(_effectTypeIdManager.GetId(effect.GetType()),
					savableRecipe.SaveRecipeState(), effectOn));
			}
			else
			{
				_unsavableEffects.Add(effect.GetType());
			}

			if (effect is IModifierIdOwner modifierIdOwner)
				modifierIdOwner.SetModifierId(Id);

			if (effect is RemoveEffect)
			{
#if DEBUG && !MODIBUFF_PROFILE
				Logger.LogWarning("[ModiBuff] Adding a remove effect through Effect() is not recommended, " +
				                  "use Remove(RemoveEffectOn) or Remove(float) instead");
#endif
				AddRemoveEffect(effectOn);
				_saveInstructions.Add(new SaveInstruction.Remove(SaveInstruction.Remove.Type.RemoveOn, effectOn));
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
			_modifierActions |= modifierAction;
#endif
			Effect(new ModifierActionEffect(modifierAction, Id), effectOn);
			_saveInstructions.Add(new SaveInstruction.ModifierAction(modifierAction, effectOn));
			return this;
		}

		/// <summary>
		///		Registers a callback register effect to a unit, will trigger all <see cref="EffectOn.CallbackUnit"/>
		///		effects when <see cref="callbackType"/> is triggered.
		///		Only ONE CallbackUnit can be registered per modifier.
		/// </summary>
		public ModifierRecipe CallbackUnit<TCallbackUnit>(TCallbackUnit callbackType)
		{
			if (_callbackUnitRegisterWrapper != null)
			{
				Logger.LogError("[ModiBuff] Multiple CallbackUnit effects registered, " +
				                "only one is allowed per modifier, ignoring.");
				return this;
			}

			var effect = new CallbackUnitRegisterEffect<TCallbackUnit>(callbackType);
			_callbackUnitRegisterWrapper = new EffectWrapper(effect, EffectOn.Init);
			_effectWrappers.Add(_callbackUnitRegisterWrapper);
			_saveInstructions.Add(new SaveInstruction.CallbackUnit((int)(object)callbackType));
			return this;
		}

		/// <summary>
		///		Registers a callback effect to a unit, will trigger the callback when <see cref="callbackType"/> is triggered.
		///		It will NOT trigger any EffectOn.<see cref="EffectOn.CallbackEffect"/> effects, only the supplied callback.
		/// </summary>
		public ModifierRecipe Callback<TCallback>(TCallback callbackType, UnitCallback callback)
		{
			return Effect(new CallbackRegisterEffect<TCallback>(
				new Callback<TCallback>(callbackType, callback)), EffectOn.Init);
		}

		/// <summary>
		///		Registers callbacks to a unit, this callback supports custom callback signatures.
		///		It will NOT trigger any EffectOn.<see cref="EffectOn.CallbackUnit"/> or <see cref="EffectOn.CallbackEffect"/> effects, only the supplied callbacks.
		///		Can be used with other signatures than <see cref="UnitCallback"/>.
		/// </summary>
		public ModifierRecipe Callback<TCallback>(params Callback<TCallback>[] callbacks)
		{
			return Effect(new CallbackRegisterEffect<TCallback>(callbacks), EffectOn.Init);
		}

		/// <summary>
		///		Registers callbacks to a unit, this callback supports custom callback signatures.
		///		It will NOT trigger any EffectOn.<see cref="EffectOn.CallbackUnit"/> or <see cref="EffectOn.CallbackEffect"/> effects, only the supplied callbacks.
		///		Can be used with other signatures than <see cref="UnitCallback"/>.
		/// </summary>
		public ModifierRecipe Callback<TCallback>(TCallback callbackType, object callback)
		{
			return Callback(new Callback<TCallback>(callbackType, callback));
		}

		/// <summary>
		///		Registers a callback that can have unique state for each modifier instance, and has savable data
		///		It will NOT trigger any EffectOn.<see cref="EffectOn.CallbackUnit"/> or <see cref="EffectOn.CallbackEffect"/> effects, only the supplied callback.
		///		Can be used with other signatures than <see cref="UnitCallback"/>. 
		/// </summary>
		public ModifierRecipe Callback<TCallback, TStateData>(TCallback callbackType,
			Func<CallbackStateContext<TStateData>> @event)
		{
			return Effect(new CallbackStateSaveRegisterEffect<TCallback, TStateData>(callbackType, @event),
				EffectOn.Init);
		}

		/// <summary>
		///		Special callbacks, all EffectOn.<see cref="EffectOn.CallbackEffect"/> effects will
		///		trigger when <see cref="callbackType"/> is triggered.
		///		Supports custom callback signatures (beside <see cref="UnitCallback"/>.
		///		Callback effects now have to be in order
		/// </summary>
		public ModifierRecipe CallbackEffect<TCallbackEffect>(TCallbackEffect callbackType,
			Func<IEffect, object> @event)
		{
			var effect = new CallbackEffectRegisterEffect<TCallbackEffect>(callbackType, @event);
			var wrapper = new EffectWrapper(effect, EffectOn.Init);
			_callbackEffectRegisterWrappers.Add(wrapper);
			_effectWrappers.Add(wrapper);
			return this;
		}

		/// <summary>
		///		Special callbacks, all EffectOn.<see cref="EffectOn.CallbackEffect"/> effects will
		///		trigger when <see cref="callbackType"/> is triggered.
		///		Supports custom callback signatures (beside <see cref="UnitCallback"/>.
		///		Allows to save state through state context.
		/// </summary>
		public ModifierRecipe CallbackEffect<TCallbackEffect, TStateData>(TCallbackEffect callbackType,
			Func<IEffect, CallbackStateContext<TStateData>> @event)
		{
			var effect = new CallbackStateEffectRegisterEffect<TCallbackEffect, TStateData>(callbackType, @event);
			var wrapper = new EffectWrapper(effect, EffectOn.Init);
			_callbackEffectRegisterWrappers.Add(wrapper);
			_effectWrappers.Add(wrapper);
			return this;
		}

		/// <summary>
		///		Special callbacks, all EffectOn.<see cref="EffectOn.CallbackEffectUnits"/> effects will
		///		trigger when <see cref="callbackType"/> is triggered.
		///		Supports custom callback signatures (beside <see cref="UnitCallback"/>.
		///		Only ONE CallbackEffect can be registered per modifier.
		///		Allows to store the target and source when registering the callback, for further access.
		/// </summary>
		public ModifierRecipe CallbackEffectUnits<TCallbackEffect>(TCallbackEffect callbackType,
			Func<IEffect, Func<IUnit, IUnit, object>> @event)
		{
			if (_callbackEffectUnitsRegisterWrapper != null)
			{
				Logger.LogError("[ModiBuff] Multiple CallbackEffectUnits effects registered, " +
				                "only one is allowed per modifier, ignoring.");
				return this;
			}

			var effect = new CallbackEffectRegisterEffectUnits<TCallbackEffect>(callbackType, @event);
			_callbackEffectUnitsRegisterWrapper = new EffectWrapper(effect, EffectOn.Init);
			_effectWrappers.Add(_callbackEffectUnitsRegisterWrapper);
			return this;
		}

		//---Modifier Generation---

		public IModifierGenerator CreateModifierGenerator()
		{
#if DEBUG && !MODIBUFF_PROFILE
			Validate();
#endif
			EffectWrapper finalRemoveEffectWrapper = null;
			if (_removeEffectWrapper != null)
			{
				finalRemoveEffectWrapper =
					new EffectWrapper(_removeEffectWrapper.GetEffect(), _removeEffectWrapper.EffectOn);
				_effectWrappers.Add(finalRemoveEffectWrapper);
			}

			//Update tag and dispel based on settings
			var dispel = DispelType.None;
			for (int i = 0; i < _effectWrappers.Count; i++)
			{
				var effectOn = _effectWrappers[i].EffectOn;
				if (effectOn.HasFlag(EffectOn.Init))
					_tag |= TagType.IsInit;
				if (effectOn.HasFlag(EffectOn.Interval))
					dispel |= DispelType.Interval;
				if (effectOn.HasFlag(EffectOn.Duration))
					dispel |= DispelType.Duration;
				if (effectOn.HasFlag(EffectOn.Stack))
				{
					_tag |= TagType.IsStack;
					dispel |= DispelType.Stack;
				}
			}

			if (_refreshDuration || _refreshInterval)
				_tag |= TagType.IsRefresh;
			if (_isInstanceStackable)
				_tag |= TagType.IsInstanceStackable;

			_dispelRegisterWrapper?.GetEffectAs<DispelRegisterEffect>().UpdateDispelType(dispel);

			var data = new ModifierRecipeData(Id, Name, _effectWrappers, finalRemoveEffectWrapper,
				_dispelRegisterWrapper, _callbackUnitRegisterWrapper, _callbackEffectRegisterWrappers.ToArray(),
				_callbackEffectUnitsRegisterWrapper, _hasApplyChecks, _applyCheckList, _hasEffectChecks,
				_effectCheckList, _applyFuncCheckList, _effectFuncCheckList, _isAura, _tag, _oneTimeInit, _interval,
				_duration, _refreshDuration, _refreshInterval, _whenStackEffect, _maxStacks, _everyXStacks,
				_singleStackTime, _independentStackTime);
			return new ModifierGenerator(in data);
		}

		public ModifierInfo CreateModifierInfo()
		{
			return new ModifierInfo(Id, Name, _displayName, _description);
		}

		public TagType GetTag() => _tag;

		private ModifierRecipe DurationInternal(float duration)
		{
			_duration = duration;
			_currentIsInterval = false;
			return this;
		}

		private static void ValidateTag(TagType tag)
		{
			if (tag.IsInternalRecipeTag())
				Logger.LogWarning("[ModiBuff] Setting internal tags directly is not recommended for recipes, " +
				                  "they're automatically set based on the recipe settings");
		}

		private static void ValidateModifierAction(ModifierAction modifierAction, EffectOn effectOn)
		{
			string initialMessage = $"[ModiBuff] ModifierAction set to {modifierAction}, and effectOn to {effectOn}. ";

			if (modifierAction.HasFlag(Core.ModifierAction.Refresh) && effectOn == EffectOn.Init)
				Logger.LogError(initialMessage +
				                "Time components always get refreshed on init (if refreshable), no need to add a modifier action for it");

			if (modifierAction.HasFlag(Core.ModifierAction.ResetStacks) && effectOn == EffectOn.Init)
				Logger.LogError(initialMessage +
				                "Stack component will always reset on init, removing the purpose of it, use init effects instead");

			if (modifierAction.HasFlag(Core.ModifierAction.Stack) && effectOn == EffectOn.Init)
				Logger.LogError(initialMessage +
				                "Stack component will always stack on init, unless you want to stack twice");
		}

		private void Validate()
		{
			bool validRecipe = true;

			ValidateTimeAction(EffectOn.Interval, _interval);
			ValidateTimeAction(EffectOn.Duration, _duration);

			if (WrappersHaveFlag(EffectOn.Stack) && _whenStackEffect == WhenStackEffect.None)
			{
				validRecipe = false;
				Logger.LogError("[ModiBuff] Stack effects set, but no stack effect type set, for modifier: "
				                + Name + " id: " + Id);
			}

			if (NoWrappersHaveFlag(EffectOn.Stack) && _whenStackEffect != WhenStackEffect.None)
			{
				validRecipe = false;
				Logger.LogWarning("[ModiBuff] Stack effect type set, but no stack effects set, for modifier: "
				                  + Name + " id: " + Id);
			}

			if (_refreshInterval && _interval == 0)
			{
				validRecipe = false;
				Logger.LogError("[ModiBuff] Refresh interval set, but interval is 0, for modifier: "
				                + Name + " id: " + Id);
			}

			if (_refreshDuration && _duration == 0)
			{
				validRecipe = false;
				Logger.LogError("[ModiBuff] Refresh duration set, but duration is 0, for modifier: "
				                + Name + " id: " + Id);
			}

			ValidateCallbacks(EffectOn.CallbackUnit, _callbackUnitRegisterWrapper);
			for (int i = 0; i < _callbackEffectRegisterWrappers.Count; i++)
				ValidateCallbacks(EffectOnCallbackEffectData.AllCallbackEffectData[i],
					_callbackEffectRegisterWrappers[i]);
			ValidateCallbacks(EffectOn.CallbackEffectUnits, _callbackEffectUnitsRegisterWrapper);

			if (_effectWrappers.Exists(w =>
				    w.GetEffect() is ApplierEffect applierEffect && applierEffect.HasApplierType))
			{
				Logger.LogWarning(
					"[ModiBuff] ApplierEffect ApplierType set in a modifier, adding this modifier will add " +
					"the applier effect to the owner because of how modifiers work, use effect (modifier-less-effects) " +
					"if not desired in modifier: " + Name + " id: " + Id);
			}

			if (_tag.HasTag(TagType.CustomStack) && !_modifierActions.HasFlag(Core.ModifierAction.Stack))
			{
				validRecipe = false;
				Logger.LogError(
					"[ModiBuff] CustomStack tag set, but no custom stack modifier action set, for modifier: " + Name +
					" id: " + Id);
			}

			if (!validRecipe)
				Logger.LogError($"[ModiBuff] Recipe validation failed for {Name}, with Id: {Id}, " +
				                "see above for more info.");

			return;

			bool NoWrappersHaveFlag(EffectOn flag)
			{
				if (_removeEffectWrapper != null && _removeEffectWrapper.EffectOn.HasFlag(flag))
					return false;

				return _effectWrappers.TrueForAll(w => !w.EffectOn.HasFlag(flag));
			}

			bool WrappersHaveFlag(EffectOn flag)
			{
				if (_removeEffectWrapper != null && _removeEffectWrapper.EffectOn.HasFlag(flag))
					return true;

				return _effectWrappers.Exists(w => w.EffectOn.HasFlag(flag));
			}

			void ValidateTimeAction(EffectOn effectOn, float actionTimer)
			{
				if (WrappersHaveFlag(effectOn) && actionTimer == 0)
				{
					validRecipe = false;
					Logger.LogError(
						$"[ModiBuff] {effectOn.ToString()} not set, but we have {effectOn.ToString()} effects, for modifier: {Name} id: {Id}");
				}

				if (NoWrappersHaveFlag(effectOn) && actionTimer != 0)
				{
					validRecipe = false;
					Logger.LogError(
						$"[ModiBuff] {effectOn.ToString()} set, but no {effectOn.ToString()} effects set, for modifier: {Name} id: {Id}");
				}
			}

			void ValidateCallbacks(EffectOn effectOn, EffectWrapper callbackWrapper)
			{
				if (WrappersHaveFlag(effectOn) && callbackWrapper == null)
				{
					validRecipe = false;
					Logger.LogError(
						$"[ModiBuff] Effects on {effectOn.ToString()} set, but no callback registration type set, " +
						"for modifier: " + Name + " id: " + Id);
				}

				if (callbackWrapper != null && NoWrappersHaveFlag(effectOn))
				{
					validRecipe = false;
					Logger.LogError(
						$"[ModiBuff] {effectOn.ToString()} registration type set, but no effects on callback set, " +
						"for modifier: " + Name + " id: " + Id);
				}
			}
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