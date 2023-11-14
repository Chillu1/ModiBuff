using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ModiBuff.Core.Units
{
	/// <summary>
	///		Status effect controller that allows multiple instances of the same status effect type.
	///		Note that this approach is much slower than having a simple timer for each legal action.
	///		But allows for infinite unique status effect instances.
	/// </summary>
	public sealed class MultiInstanceStatusEffectController : IStateReset,
		IMultiInstanceStatusEffectController<LegalAction, StatusEffectType>
	{
		private readonly IUnit _owner;
		private readonly List<StatusEffectEvent> _statusEffectAddedEvents;
		private readonly List<StatusEffectEvent> _statusEffectRemovedEvents;

		//TODO Instead of dict we could preload all the possible status effect instances, and have an array instead
		//But then we'll have a problem with genIds, but then we can have arrays of arrays
		//TODO Check if performance is terrible, we could hack it by doing the hash check manually, and having key be an int
		//But then we'll have an issue with getting the underlying status effect type
		private readonly Dictionary<StatusEffectInstance, float> _legalActionsTimers;
		private readonly List<StatusEffectInstance> _stackEffectInstancesForRemoval;

		//Reference counting of how many timers are active for each legal action type
		private readonly int[] _legalActionTypeCounters;

		private LegalAction _legalActions;

		public MultiInstanceStatusEffectController(IUnit owner, List<StatusEffectEvent> statusEffectAddedEvents = null,
			List<StatusEffectEvent> statusEffectRemovedEvents = null)
		{
			_owner = owner;
			_statusEffectAddedEvents = statusEffectAddedEvents;
			_statusEffectRemovedEvents = statusEffectRemovedEvents;

			_legalActionsTimers = new Dictionary<StatusEffectInstance, float>();
			_stackEffectInstancesForRemoval = new List<StatusEffectInstance>();
			_legalActionTypeCounters = new int[LegalActionHelper.BaseCount];

			_legalActions = LegalAction.All;
		}

		public void Update(float delta)
		{
			var oldLegalActions = _legalActions;
			foreach (var kvp in _legalActionsTimers)
			{
				float timer = kvp.Value - delta;
				if (timer <= 0)
				{
					_stackEffectInstancesForRemoval.Add(kvp.Key);
					//Have to deconstruct the status effect type to legal actions, then decrement the counters on each
					var legalActions = StatusEffectTypeHelper.LegalActions[kvp.Key.StatusEffectTypeInt];
					for (int i = 0; i < legalActions.Length; i++)
					{
						var legalAction = legalActions[i];
						int legalActionIndex = StatusEffectTypeHelper.LegalActionToIndex[(int)legalAction];
						int counter = --_legalActionTypeCounters[legalActionIndex];
						if (counter <= 0)
							_legalActions |= legalAction; //No more references, set the legal action to true
					}
				}

				_legalActionsTimers[kvp.Key] = timer;
			}

			int count = _stackEffectInstancesForRemoval.Count;
			if (count <= 0)
				return;

			for (int i = 0; i < count; i++)
			{
				var statusEffectInstance = _stackEffectInstancesForRemoval[i];
				_legalActionsTimers.Remove(statusEffectInstance);
				//TODO, we might remove a status effect type, but not all, so we shouldn't send that one as if it was removed 
				CallRemoveEvents((StatusEffectType)statusEffectInstance.StatusEffectTypeInt, oldLegalActions, _owner);
			}

			_stackEffectInstancesForRemoval.Clear();
		}

		public void DispelStatusEffect(StatusEffectType statusEffectType, IUnit source)
		{
			var oldLegalActions = _legalActions;
			//Remove all instances of the status effect type
			foreach (var kvp in _legalActionsTimers)
			{
				//Do we want to dispel status effect combinations as well?
				//if ((kvp.Key.StatusEffectTypeInt & (int)statusEffectType) == 0)
				if (kvp.Key.StatusEffectTypeInt != (int)statusEffectType)
					continue;

				_stackEffectInstancesForRemoval.Add(kvp.Key);
				var legalActions = StatusEffectTypeHelper.LegalActions[kvp.Key.StatusEffectTypeInt];
				for (int i = 0; i < legalActions.Length; i++)
				{
					var legalAction = legalActions[i];
					int legalActionIndex = StatusEffectTypeHelper.LegalActionToIndex[(int)legalAction];
					int counter = --_legalActionTypeCounters[legalActionIndex];
					if (counter <= 0)
						_legalActions |= legalAction; //No more references, set the legal action to true
				}

				_legalActionsTimers[kvp.Key] = 0;
			}

			int count = _stackEffectInstancesForRemoval.Count;
			if (count <= 0)
				return;

			for (int i = 0; i < count; i++)
				_legalActionsTimers.Remove(_stackEffectInstancesForRemoval[i]);

			_stackEffectInstancesForRemoval.Clear();
			CallRemoveEvents(statusEffectType, oldLegalActions, source);
		}

		public void DispelAll(IUnit source)
		{
			var oldLegalActions = _legalActions;
			var statusEffectTypesDispelled = StatusEffectType.None;
			foreach (var kvp in _legalActionsTimers)
			{
				statusEffectTypesDispelled |= (StatusEffectType)kvp.Key.StatusEffectTypeInt;
				_legalActionsTimers[kvp.Key] = 0;
			}

			for (int i = 0; i < _legalActionTypeCounters.Length; i++)
				_legalActionTypeCounters[i] = 0;

			_legalActions = LegalAction.All;
			CallRemoveEvents(statusEffectTypesDispelled, oldLegalActions, source);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasLegalAction(LegalAction legalAction) => (_legalActions & legalAction) != 0;

		public bool HasStatusEffect(StatusEffectType statusEffectType)
		{
			//Get all indexes of the status effect type
			LegalAction[] legalActions = StatusEffectTypeHelper.LegalActions[(int)statusEffectType];
			//Check if all of them are bigger than 0
			for (int i = 0; i < legalActions.Length; i++)
			{
				if ((_legalActions & legalActions[i]) != 0)
					return false;
			}

			return true;
		}

		public void ChangeStatusEffect(int id, int genId, StatusEffectType statusEffectType, float duration,
			IUnit source)
		{
			var statusEffectInstance = new StatusEffectInstance(id, genId, statusEffectType);
			//If an instance with same id and status effect type exists, refresh it
			if (_legalActionsTimers.TryGetValue(statusEffectInstance, out float currentDuration))
			{
				if (currentDuration < duration)
					_legalActionsTimers[statusEffectInstance] = duration;
				CallAddEvents(statusEffectType, _legalActions, source);
				return;
			}

			var oldLegalAction = _legalActions;
			//add a new instance
			_legalActionsTimers.Add(statusEffectInstance, duration);
			var legalActions = StatusEffectTypeHelper.LegalActions[(int)statusEffectType];
			for (int i = 0; i < legalActions.Length; i++)
			{
				var legalAction = legalActions[i];
				int legalActionIndex = StatusEffectTypeHelper.LegalActionToIndex[(int)legalAction];
				_legalActionTypeCounters[legalActionIndex]++;
				_legalActions &= ~legalAction;
			}

			CallAddEvents(statusEffectType, oldLegalAction, source);
		}

		public void DecreaseStatusEffect(int id, int genId, StatusEffectType statusEffectType, float duration,
			IUnit source)
		{
			var statusEffectInstance = new StatusEffectInstance(id, genId, statusEffectType);
			if (!_legalActionsTimers.TryGetValue(statusEffectInstance, out float currentDuration))
				return;

			currentDuration -= duration;
			if (currentDuration > 0)
			{
				_legalActionsTimers[statusEffectInstance] = currentDuration;
				return;
			}

			var oldLegalAction = _legalActions;
			_legalActionsTimers.Remove(statusEffectInstance);
			var legalActions = StatusEffectTypeHelper.LegalActions[(int)statusEffectType];
			for (int i = 0; i < legalActions.Length; i++)
			{
				var legalAction = legalActions[i];
				int legalActionIndex = StatusEffectTypeHelper.LegalActionToIndex[(int)legalAction];
				int counter = --_legalActionTypeCounters[legalActionIndex];
				if (counter <= 0)
					_legalActions |= legalAction; //No more references, set the legal action to true
			}

			CallRemoveEvents(statusEffectType, oldLegalAction, source);
		}

		public void TriggerEvent(StatusEffectEvent @event) =>
			@event.Invoke(_owner, _owner, StatusEffectType.None, _legalActions, _legalActions);

		private void CallAddEvents(StatusEffectType statusEffectType, LegalAction oldLegalAction, IUnit source)
		{
			if (_statusEffectAddedEvents == null)
				return;

			for (int i = 0; i < _statusEffectAddedEvents.Count; i++)
				_statusEffectAddedEvents[i].Invoke(_owner, source, statusEffectType, oldLegalAction, _legalActions);
		}

		private void CallRemoveEvents(StatusEffectType statusEffectType, LegalAction oldLegalAction, IUnit source)
		{
			if (_statusEffectRemovedEvents == null)
				return;

			for (int i = 0; i < _statusEffectRemovedEvents.Count; i++)
				_statusEffectRemovedEvents[i].Invoke(_owner, source, statusEffectType, oldLegalAction, _legalActions);
		}

		public void ResetState()
		{
			_legalActionsTimers.Clear();
			_stackEffectInstancesForRemoval.Clear();
			for (int i = 0; i < _legalActionTypeCounters.Length; i++)
				_legalActionTypeCounters[i] = 0;

			_legalActions = LegalAction.All;
		}

		public SaveData SaveState() => new SaveData(_legalActionsTimers, _legalActionTypeCounters, _legalActions);

		public void LoadState(SaveData data)
		{
			foreach (var instance in data.LegalActionsTimers)
				_legalActionsTimers.Add(instance.Key, instance.Value);
			for (int i = 0; i < data.LegalActionTypeCounters.Length; i++)
				_legalActionTypeCounters[i] = data.LegalActionTypeCounters[i];
			_legalActions = data.LegalActions;
		}

		public readonly struct StatusEffectInstance : IEquatable<StatusEffectInstance>
		{
			private readonly int _id;
			private readonly int _genId;
			public readonly int StatusEffectTypeInt;
			//public readonly StatusEffectType StatusEffectType;

			public StatusEffectInstance(int id, int genId, StatusEffectType statusEffectType)
			{
				_id = id;
				_genId = genId;
				StatusEffectTypeInt = (int)statusEffectType;
			}

			public bool Equals(StatusEffectInstance other)
			{
				return _id == other._id && _genId == other._genId && StatusEffectTypeInt == other.StatusEffectTypeInt;
			}

			public override bool Equals(object obj)
			{
				return obj is StatusEffectInstance other && Equals(other);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					int hashCode = _id;
					hashCode = (hashCode * 397) ^ _genId;
					hashCode = (hashCode * 397) ^ StatusEffectTypeInt;
					return hashCode;
				}
			}
		}

		public readonly struct SaveData
		{
			//TODO GenId of this will be wrong, since we refresh the genId counters
			public readonly Dictionary<StatusEffectInstance, float> LegalActionsTimers;
			public readonly int[] LegalActionTypeCounters;
			public readonly LegalAction LegalActions;

#if JSON_SERIALIZATION && (NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_1_OR_GREATER || NET5_0_OR_GREATER)
			[System.Text.Json.Serialization.JsonConstructor]
#endif
			public SaveData(Dictionary<StatusEffectInstance, float> legalActionsTimers, int[] legalActionTypeCounters,
				LegalAction legalActions)
			{
				LegalActionsTimers = legalActionsTimers;
				LegalActionTypeCounters = legalActionTypeCounters;
				LegalActions = legalActions;
			}
		}
	}
}