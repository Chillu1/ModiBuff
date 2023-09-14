using BenchmarkDotNet.Attributes;
using ModiBuff.Core.Units;

namespace ModiBuff.Tests
{
	/// <summary>
	///		Only tests for subsequent inits, not new modifier each time
	/// </summary>
	public class BenchAddModifier : ModifierBenches
	{
		private Unit _unit;

		private int _noOpModifierId;
		private int _initDamageModifierId;
		private int _initStackDamageModifierId;

		public override void GlobalSetup()
		{
			base.GlobalSetup();

			_unit = new Unit(1_000_000_000, 5);

			_noOpModifierId = IdManager.GetId("NoOpEffect");
			_initDamageModifierId = IdManager.GetId("InitDamage");
			_initStackDamageModifierId = IdManager.GetId("InitStackDamage");
		}

		[Benchmark]
		public void BenchAddNoOpEffectBench()
		{
			_unit.ModifierController.Add(_noOpModifierId, _unit, _unit);
		}

		public void BenchAddInitDamageBench()
		{
			_unit.ModifierController.Add(_initDamageModifierId, _unit, _unit);
		}

		[Benchmark]
		public void BenchAddInitStackDamage()
		{
			_unit.ModifierController.Add(_initStackDamageModifierId, _unit, _unit);
		}
	}
}