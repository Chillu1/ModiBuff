using System;

namespace ModiBuff.Core.Units
{
	public sealed class UnitPool
	{
		public static UnitPool Instance { get; private set; }

		private readonly Unit[][] _pools;
		private readonly int[] _poolTops;

		private readonly Tuple<float, float, float, float, UnitType, UnitTag>[] _unitData;

		public UnitPool()
		{
			if (Instance != null)
				return;

			Instance = this;

			int lastUnitType = (int)UnitTypeTypeHelper.Last;
			_pools = new Unit[lastUnitType][];
			_poolTops = new int[lastUnitType];

			_unitData = new[]
			{
				Tuple.Create(500f, 10f, 5f, 1000f, UnitType.Good, UnitTag.Default),
				Tuple.Create(1000f, 20f, 10f, 1000f, UnitType.Bad, UnitTag.Default),
			};

			for (int i = 0; i < lastUnitType; i++)
			{
				var uData = _unitData[i];
				_pools[i] = new Unit[10];
				for (int j = 0; j < 10; j++)
				{
					_pools[i][j] = new Unit(uData.Item1, uData.Item2, uData.Item3, uData.Item4, uData.Item5,
						uData.Item6);
				}

				_poolTops[i] = 10;
			}
		}

		public Unit Rent(UnitType unitType)
		{
			int index = (int)unitType;
			if (_poolTops[index] == 0)
			{
				var uData = _unitData[index];
				return new Unit(uData.Item1, uData.Item2, uData.Item3, uData.Item4, uData.Item5, uData.Item6);
			}

			return _pools[index][--_poolTops[index]];
		}

		public void Return(Unit unit)
		{
			unit.ResetState();

			int index = (int)unit.UnitType;
			if (_poolTops[index] == _pools[index].Length)
				return;

			_pools[index][_poolTops[index]++] = unit;
		}
	}
}