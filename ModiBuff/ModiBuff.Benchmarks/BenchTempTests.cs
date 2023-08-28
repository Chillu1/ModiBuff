using System;
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

		[Benchmark(Baseline = true)]
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

		[Benchmark]
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
	}
}