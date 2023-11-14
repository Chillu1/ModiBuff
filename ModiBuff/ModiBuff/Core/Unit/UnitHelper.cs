using System;

namespace ModiBuff.Core
{
	public sealed class UnitHelper
	{
		private static UnitHelper _instance;

		private readonly Func<int, IUnit> _unitGetter;

		public static void Setup(Func<int, IUnit> unitGetter)
		{
			_ = new UnitHelper(unitGetter);
		}

		public UnitHelper(Func<int, IUnit> unitGetter)
		{
			_instance = this;
			_unitGetter = unitGetter;
		}

		public static IUnit GetUnit(int id) => _instance._unitGetter(id);

		public static void Clear() => _instance = null;
	}
}