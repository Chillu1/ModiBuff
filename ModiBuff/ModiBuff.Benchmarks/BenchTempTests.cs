using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using ModiBuff.Core;
using ModiBuff.Core.Units;

namespace ModiBuff.Tests
{
	public class BenchTempTests : ModifierBenches
	{
		private const int Iterations = (int)1e5;

		private Unit _unit;
		private StatType _statType;
		private Func<IUnit, float> _checkStat;

		private ITargetComponent[] _targetComponents;
		private bool _hasTarget;
		private ITimeComponent[] _timeComponents;
		private bool _hasTime;

		private const float Delta = 0.0167f;
		private Dictionary<int, ModifierCheck> _modifierCastChecksAppliers;
		private Dictionary<int, ModifierCheck> _modifierAttackChecksAppliers;

		private int _timeComponentCount;
		private int _timeComponentIndex;
		private Func<ITimeComponent[]> _timeComponentFunc;

		public override void GlobalSetup()
		{
			base.GlobalSetup();

			_unit = new Unit();
			_statType = StatType.Mana;

			_checkStat = (unit) =>
			{
				switch (_statType)
				{
					case StatType.Health:
						var damagable = (IDamagable<float, float>)unit;
						return damagable.Health / damagable.MaxHealth;
					case StatType.Mana:
						var manaOwner = (IManaOwner<float, float>)unit;
						return manaOwner.Mana / manaOwner.MaxMana;
					default:
						throw new ArgumentOutOfRangeException();
				}
			};

			_targetComponents = null; // new ITargetComponent[] { new SingleTargetComponent(), new MultiTargetComponent() };
			_hasTarget = false;
			_timeComponents = new ITimeComponent[]
			{
				new IntervalComponent(5, false, (IEffect)null, null, false), new IntervalComponent(5, false, (IEffect)null, null, false),
				new DurationComponent(5, false, (IEffect)null),
			};
			_hasTime = true;

			int applierId = IdManager.GetId("InitDamage_CostMana"),
				applierTwoId = IdManager.GetId("InitDamage_ApplyCondition_HealthAbove100");
			_modifierCastChecksAppliers = new Dictionary<int, ModifierCheck>();
			_modifierAttackChecksAppliers = new Dictionary<int, ModifierCheck>();
			_modifierAttackChecksAppliers.Add(applierId, Pool.RentModifierCheck(applierId));
			_modifierAttackChecksAppliers.Add(applierTwoId, Pool.RentModifierCheck(applierTwoId));

			_timeComponentCount = 0; //2
			_timeComponentFunc = () => null;
			//_timeComponentFunc = () =>
			//{
			//_timeComponentIndex = 0;
			//return new ITimeComponent[_timeComponentCount];
			//};
		}

		//[Benchmark(Baseline = true)]
		public void TestSwitch()
		{
			float value;

			switch (_statType)
			{
				case StatType.Health:
					var damagable = (IDamagable<float, float>)_unit;
					value = damagable.Health / damagable.MaxHealth;
					break;
				case StatType.Mana:
					var manaOwner = (IManaOwner<float, float>)_unit;
					value = manaOwner.Mana / manaOwner.MaxMana;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			float test = value;
		}

		//[Benchmark]
		public void TestDelegate()
		{
			float value = _checkStat(_unit);

			float test = value;
		}

		//[Benchmark(Baseline = true)]
		public void ForLoopNotNull()
		{
			if (_targetComponents != null)
				for (int i = 0; i < _targetComponents.Length; i++)
				{
					var targetComponent = _targetComponents[i];
				}

			if (_timeComponents != null)
				for (int i = 0; i < _timeComponents.Length; i++)
				{
					var timeComponent = _timeComponents[i];
				}
		}

		//[Benchmark]
		public void ForLoopBoolCheck()
		{
			if (_hasTarget)
				for (int i = 0; i < _targetComponents.Length; i++)
				{
					var targetComponent = _targetComponents[i];
				}

			if (_hasTime)
				for (int i = 0; i < _timeComponents.Length; i++)
				{
					var timeComponent = _timeComponents[i];
				}
		}

		//[Benchmark(Baseline = true)]
		public void ForEachLoopNoCheck()
		{
			foreach (var check in _modifierCastChecksAppliers.Values)
				check.Update(Delta);

			foreach (var check in _modifierAttackChecksAppliers.Values)
				check.Update(Delta);
		}

		//[Benchmark]
		public void ForEachLoopCheck()
		{
			if (_modifierCastChecksAppliers.Count > 0)
				foreach (var check in _modifierCastChecksAppliers.Values)
					check.Update(Delta);

			if (_modifierAttackChecksAppliers.Count > 0)
				foreach (var check in _modifierAttackChecksAppliers.Values)
					check.Update(Delta);
		}

		//[Benchmark]
		public void ForEachLoopNoCheckKey()
		{
			foreach (var check in _modifierCastChecksAppliers)
				check.Value.Update(Delta);

			foreach (var check in _modifierAttackChecksAppliers)
				check.Value.Update(Delta);
		}

		[Benchmark(Baseline = true)]
		public void IfTimeComponentCount()
		{
			ITimeComponent[] timeComponents = null;
			if (_timeComponentCount > 0)
			{
				_timeComponentIndex = 0;
				timeComponents = new ITimeComponent[_timeComponentCount];
			}

			var test = timeComponents;
		}

		[Benchmark]
		public void FuncTimeComponentCount()
		{
			var timeComponents = _timeComponentFunc();

			var test = timeComponents;
		}
	}
}