using BenchmarkDotNet.Attributes;
using ModiBuff.Core;
using ModiBuff.Core.Units;

namespace ModiBuff.Tests
{
	/// <summary>
	///		Only tests for subsequent inits, not new modifier each time
	/// </summary>
	public class BenchAddModifier : BaseModifierBenches
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
			bool result = _unit.TryAddModifier(_initDamageModifierId, _unit);
		}

		[Benchmark]
		public void BenchAddInitStackDamage()
		{
			bool result = _unit.TryAddModifier(_initStackDamageModifierId, _unit);
		}

		[Benchmark]
		public void BenchAddInitHeal()
		{
			bool result = _unit.TryAddModifier(_initHealModifierId, _unit);
		}
	}
}