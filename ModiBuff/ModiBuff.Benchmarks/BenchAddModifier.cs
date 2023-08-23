using BenchmarkDotNet.Attributes;
using ModiBuff.Core;
using ModiBuff.Core.Units;

namespace ModiBuff.Tests
{
	/// <summary>
	///		Only tests for subsequent inits, not new modifier each time
	/// </summary>
	public class BenchAddModifier : ModifierBenches
	{
		private Unit _unit;

		private int _initDamageModifierId;
		private int _initStackDamageModifierId;
		private int _initHealModifierId;

		public override void GlobalSetup()
		{
			base.GlobalSetup();

			_unit = new Unit(1_000_000_000, 5);

			_initDamageModifierId = IdManager.GetId("InitDamage");
			_initStackDamageModifierId = IdManager.GetId("InitStackDamage");
			_initHealModifierId = IdManager.GetId("InitHeal");
		}

		[Benchmark]
		public void BenchAddInitDamageBench()
		{
			_unit.AddModifier(_initDamageModifierId, _unit);
		}

		[Benchmark]
		public void BenchAddInitStackDamage()
		{
			_unit.AddModifier(_initStackDamageModifierId, _unit);
		}

		[Benchmark]
		public void BenchAddInitHeal()
		{
			_unit.AddModifier(_initHealModifierId, _unit);
		}
	}
}