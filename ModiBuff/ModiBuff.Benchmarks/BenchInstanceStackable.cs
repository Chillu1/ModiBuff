using BenchmarkDotNet.Attributes;
using ModiBuff.Core;
using ModiBuff.Core.Units;

namespace ModiBuff.Tests
{
	[MemoryDiagnoser]
	public class BenchInstanceStackable : ModifierBenches
	{
		private const int Iterations = (int)1e4;

		private Unit _unit;
		private int _instanceStackableDoTModifierId;

		public override void GlobalSetup()
		{
			base.GlobalSetup();

			_instanceStackableDoTModifierId = IdManager.GetId("InstanceStackableDoT");

			Pool.SetMaxPoolSize((int)1e5);
		}

		[IterationSetup]
		public void IterationSetup()
		{
			_unit = new Unit(1_000_000_000, 5);

			Pool.Clear();

			Pool.Allocate(_instanceStackableDoTModifierId, Iterations);
		}

		[Benchmark(OperationsPerInvoke = Iterations)]
		public void BenchAddInstanceStackableDoT()
		{
			for (int i = 0; i < Iterations; i++)
			{
				_unit.ModifierController.Add(_instanceStackableDoTModifierId, _unit, _unit);
			}
		}
	}
}