using System;
using System.Linq;

namespace ModiBuff.Core
{
	public sealed class ModifierCheck : IStateReset
	{
		public int Id { get; }

		private readonly bool _hasFuncChecks, _hasUpdatableChecks, _hasNoUnitChecks, _hasUnitChecks, _hasUsableChecks, _hasStateResetChecks;

		private readonly Func<IUnit, bool>[] _funcChecks;

		private readonly IUpdatableCheck[] _updatableChecks;
		private readonly INoUnitCheck[] _noUnitChecks;
		private readonly IUnitCheck[] _unitChecks;
		private readonly IUsableCheck[] _usableChecks;
		private readonly IStateCheck[] _stateResetChecks;

		public ModifierCheck(int id, Func<IUnit, bool>[] funcChecks = null, IUpdatableCheck[] updatableChecks = null,
			INoUnitCheck[] noUnitChecks = null, IUnitCheck[] unitChecks = null, IUsableCheck[] usableChecks = null,
			IStateCheck[] stateResetChecks = null)
		{
			Id = id;

			if (stateResetChecks != null)
			{
				foreach (var stateCheck in stateResetChecks) //TODO Refactor, same instance in both state & other check arrays
				{
					if (stateCheck is IUpdatableCheck)
					{
						if (updatableChecks == null)
							updatableChecks = new[] { (IUpdatableCheck)stateCheck };
						else
							updatableChecks = updatableChecks.Concat(new[] { (IUpdatableCheck)stateCheck }).ToArray();
					}
				}
			}

			_funcChecks = funcChecks;

			_updatableChecks = updatableChecks;
			_noUnitChecks = noUnitChecks;
			_unitChecks = unitChecks;
			_usableChecks = usableChecks;
			_stateResetChecks = stateResetChecks;

			_hasFuncChecks = funcChecks != null && funcChecks.Length > 0;
			_hasUpdatableChecks = updatableChecks != null && updatableChecks.Length > 0;
			_hasNoUnitChecks = noUnitChecks != null && noUnitChecks.Length > 0;
			_hasUnitChecks = unitChecks != null && unitChecks.Length > 0;
			_hasUsableChecks = usableChecks != null && usableChecks.Length > 0;
			_hasStateResetChecks = stateResetChecks != null && stateResetChecks.Length > 0;
		}

		public void Update(float delta)
		{
			if (!_hasUpdatableChecks)
				return;

			for (int i = 0; i < _updatableChecks.Length; i++)
				_updatableChecks[i].Update(delta);
		}

		public bool Check(IUnit unit)
		{
			if (_hasFuncChecks)
				for (int i = 0; i < _funcChecks.Length; i++)
				{
					if (!_funcChecks[i](unit))
						return false;
				}

			if (_hasUpdatableChecks)
				for (int i = 0; i < _updatableChecks.Length; i++)
				{
					if (!_updatableChecks[i].Check())
						return false;
				}

			if (_hasNoUnitChecks)
				for (int i = 0; i < _noUnitChecks.Length; i++)
				{
					if (!_noUnitChecks[i].Check())
						return false;
				}

			if (_hasUnitChecks)
				for (int i = 0; i < _unitChecks.Length; i++)
				{
					if (!_unitChecks[i].Check(unit))
						return false;
				}

			if (_hasUsableChecks)
				for (int i = 0; i < _usableChecks.Length; i++)
				{
					if (!_usableChecks[i].Check(unit))
						return false;
				}

			if (_hasStateResetChecks)
				for (int i = 0; i < _stateResetChecks.Length; i++)
					_stateResetChecks[i].ResetState();
			if (_hasUsableChecks)
				for (int i = 0; i < _usableChecks.Length; i++)
					_usableChecks[i].Use(unit);

			return true;
		}

		public void ResetState()
		{
			if (_hasStateResetChecks)
				for (int i = 0; i < _stateResetChecks.Length; i++)
					_stateResetChecks[i].ResetState();
		}
	}
}