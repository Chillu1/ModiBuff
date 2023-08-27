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
		}

		[Benchmark(Baseline = true)]
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

		[Benchmark]
		public void TestDelegate()
		{
			float value = _checkStat(_unit);

			float test = value;
		}
	}
}