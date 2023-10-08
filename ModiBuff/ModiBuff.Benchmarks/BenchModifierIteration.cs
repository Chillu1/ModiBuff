using BenchmarkDotNet.Attributes;
using ModiBuff.Core;
using ModiBuff.Core.Units;

namespace ModiBuff.Tests
{
	[MemoryDiagnoser]
	public class BenchModifierIteration : ModifierBenches
	{
		private const int UnitCount = 10_000;

		//[Params(0.0167f /*, 1f*/)]
		public const float Delta = 0.0167f;

		private Unit[] _dotUnits;

		private Unit _unit;
		private Unit _instanceUnit;

		private int _initDamageId;
		private Unit[] _initDamageUnits;

		public override void GlobalSetup()
		{
			base.GlobalSetup();
			Pool.SetMaxPoolSize(20_000);

			_dotUnits = new Unit[UnitCount];
			int dotId = IdManager.GetId("DoT");
			for (int i = 0; i < _dotUnits.Length; i++)
			{
				var unit = new Unit();
				unit.ModifierController.Add(dotId, unit, unit);
				_dotUnits[i] = unit;
			}

			_unit = new Unit(int.MaxValue);
			_instanceUnit = new Unit(int.MaxValue);
			int instanceDoTId = IdManager.GetId("InstanceStackableDoTNoRemove");
			for (int i = 0; i < UnitCount; i++)
				_instanceUnit.ModifierController.Add(instanceDoTId, _instanceUnit, _instanceUnit);

			_initDamageId = IdManager.GetId("InitDamage");
			Pool.Allocate(_initDamageId, UnitCount);

			_initDamageUnits = new Unit[UnitCount];
			for (int i = 0; i < _initDamageUnits.Length; i++)
				_initDamageUnits[i] = new Unit();
		}

		[Benchmark(OperationsPerInvoke = UnitCount)]
		public void BenchDoTIteration()
		{
			for (int i = 0; i < _dotUnits.Length; i++)
				_dotUnits[i].Update(Delta);
		}

		//[Benchmark(OperationsPerInvoke = UnitCount)]
		public void BenchDoTIterationSingle()
		{
			for (int i = 0; i < UnitCount; i++)
				_unit.Update(Delta);
		}

		//[Benchmark(OperationsPerInvoke = UnitCount)]
		public void BenchInitIteration()
		{
			for (int i = 0; i < _initDamageUnits.Length; i++)
			{
				var unit = _initDamageUnits[i];
				unit.ModifierController.Add(_initDamageId, unit, unit);
			}
		}

		[Benchmark]
		public void BenchDoTIterationSingleInstanceStackable()
		{
			_instanceUnit.Update(Delta);
		}
	}
}