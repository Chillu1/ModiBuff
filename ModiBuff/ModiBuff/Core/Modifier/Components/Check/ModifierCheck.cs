using System;

namespace ModiBuff.Core
{
	public sealed class ModifierCheck : IStateReset
	{
		public int Id { get; }

		private readonly Func<IUnit, bool>[] _funcChecks;

		private readonly IUpdatableCheck[] _updatableChecks;
		private readonly INoUnitCheck[] _noUnitChecks;
		private readonly IUnitCheck[] _unitChecks;
		private readonly IUsableCheck[] _usableChecks;
		private readonly IStateCheck[] _stateResetChecks;

		public ModifierCheck(int id, Func<IUnit, bool>[] funcChecks, IUpdatableCheck[] updatableChecks,
			INoUnitCheck[] noUnitChecks, IUnitCheck[] unitChecks, IUsableCheck[] usableChecks,
			IStateCheck[] stateResetChecks)
		{
			Id = id;

			_funcChecks = funcChecks;

			_updatableChecks = updatableChecks;
			_noUnitChecks = noUnitChecks;
			_unitChecks = unitChecks;
			_usableChecks = usableChecks;
			_stateResetChecks = stateResetChecks;
		}

		public void Update(float delta)
		{
			for (int i = 0; i < _updatableChecks?.Length; i++)
				_updatableChecks[i].Update(delta);
		}

		public bool Check(IUnit unit)
		{
			for (int i = 0; i < _funcChecks?.Length; i++)
			{
				if (!_funcChecks[i](unit))
					return false;
			}

			for (int i = 0; i < _noUnitChecks?.Length; i++)
			{
				if (!_noUnitChecks[i].Check())
					return false;
			}

			for (int i = 0; i < _unitChecks?.Length; i++)
			{
				if (!_unitChecks[i].Check(unit))
					return false;
			}

			//All checks passed
			for (int i = 0; i < _stateResetChecks?.Length; i++)
				_stateResetChecks[i].RestartState();

			for (int i = 0; i < _usableChecks?.Length; i++)
				_usableChecks[i].Use(unit);

			return true;
		}

		public void ResetState()
		{
			for (int i = 0; i < _stateResetChecks?.Length; i++)
				_stateResetChecks[i].ResetState();
		}
	}
}