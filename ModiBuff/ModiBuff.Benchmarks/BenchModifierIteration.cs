using BenchmarkDotNet.Attributes;
using ModiBuff.Core;
using ModiBuff.Core.Units;

namespace ModiBuff.Tests
{
	[MemoryDiagnoser]
	public class BenchModifierIteration : ModifierBenches
	{
		private const int UnitCount = 10_000;

		[Params(0.0167f /*, 1f*/)]
		public float Delta;

		private Unit[] _dotUnits;

		private Unit _unit;

		private int _initDamageId;
		private Unit[] _initDamageUnits;

		public override void GlobalSetup()
		{
			base.GlobalSetup();
			Pool.SetMaxPoolSize(20_000);

			_dotUnits = new Unit[UnitCount];
			for (int i = 0; i < _dotUnits.Length; i++)
			{
				_dotUnits[i] = new Unit();
				_dotUnits[i].TryAddModifierSelf("DoT");
			}

			_unit = new Unit();

			_initDamageId = IdManager.GetId("InitDamage");
			Pool.Allocate(_initDamageId, UnitCount);

			_initDamageUnits = new Unit[UnitCount];
			for (int i = 0; i < _initDamageUnits.Length; i++)
				_initDamageUnits[i] = new Unit();
		}

		[Benchmark]
		public void BenchDoTIteration()
		{
			for (int i = 0; i < _dotUnits.Length; i++)
				_dotUnits[i].Update(Delta);
		}

		[Benchmark]
		public void BenchDoTIterationSingle()
		{
			for (int i = 0; i < UnitCount; i++)
				_unit.Update(Delta);
		}

		[Benchmark]
		public void BenchInitIteration()
		{
			for (int i = 0; i < _initDamageUnits.Length; i++)
			{
				var unit = _initDamageUnits[i];
				unit.TryAddModifier(_initDamageId, unit);
			}
		}
	}
}