using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ModiBuff.Core.Units
{
	/// <summary>
	///		Status effect controller that allows multiple instances of the same status effect type.
	/// </summary>
	public sealed class MultiInstanceStatusEffectController : IMultiInstanceStatusEffectController<LegalAction, StatusEffectType>
	{
		//TODO Instead of dict we could preload all the possible status effect instances, and have an array instead
		//But then we'll have a problem with genIds, but then we can have arrays of arrays
		//TODO Check performance is terrible, we could hack it by doing the hash check manually, and having key be an int
		private readonly Dictionary<StatusEffectInstance, float> _legalActionsTimers;
		private readonly List<StatusEffectInstance> _stackEffectInstancesForRemoval;

		//Reference counting of how many timers are active for each legal action type
		private readonly int[] _legalActionTypeCounters;

		private LegalAction _legalActions;

		public MultiInstanceStatusEffectController()
		{
			_legalActionsTimers = new Dictionary<StatusEffectInstance, float>();
			_stackEffectInstancesForRemoval = new List<StatusEffectInstance>();
			_legalActionTypeCounters = new int[LegalActionHelper.BaseCount];

			_legalActions = LegalAction.All;
		}

		public void Update(float delta)
		{
			foreach (var kvp in _legalActionsTimers)
			{
				float timer = kvp.Value - delta;
				if (timer <= 0)
				{
					_stackEffectInstancesForRemoval.Add(kvp.Key);
					//Have to deconstruct the status effect type to legal actions, then decrement the counters on each
					var legalActions = StatusEffectTypeHelper.LegalActions[(int)kvp.Key.StatusEffectType];
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
				_legalActionsTimers.Remove(_stackEffectInstancesForRemoval[i]);

			_stackEffectInstancesForRemoval.Clear();
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
				if ((_legalActions & legalActions[i]) == 0)
					continue;

				return false;
			}

			return true;
		}

		public void ChangeStatusEffect(int id, StatusEffectType statusEffectType, float duration)
		{
			var statusEffectInstance = new StatusEffectInstance(id, statusEffectType);
			//If an instance with same id and status effect type exists, refresh it
			if (_legalActionsTimers.TryGetValue(statusEffectInstance, out float currentDuration))
			{
				if (currentDuration < duration)
					_legalActionsTimers[statusEffectInstance] = duration;
				return;
			}

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
		}

		public void DecreaseStatusEffect(int id, StatusEffectType statusEffectType, float duration)
		{
			var statusEffectInstance = new StatusEffectInstance(id, statusEffectType);
			if (!_legalActionsTimers.TryGetValue(statusEffectInstance, out float currentDuration))
				return;

			currentDuration -= duration;
			if (currentDuration > 0)
			{
				_legalActionsTimers[statusEffectInstance] = currentDuration;
				return;
			}

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
		}

		private struct StatusEffectInstance : IEquatable<StatusEffectInstance>
		{
			public readonly int Id;
			public readonly StatusEffectType StatusEffectType;

			public StatusEffectInstance(int id, StatusEffectType statusEffectType)
			{
				Id = id;
				StatusEffectType = statusEffectType;
			}

			public bool Equals(StatusEffectInstance other) => Id == other.Id && StatusEffectType == other.StatusEffectType;

			public override bool Equals(object obj) => obj is StatusEffectInstance other && Equals(other);

			public override int GetHashCode()
			{
				unchecked
				{
					return (Id * 397) ^ (int)StatusEffectType;
				}
			}
		}
	}
}