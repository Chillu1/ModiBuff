using System;
using System.Collections.Generic;
using System.Linq;

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

		private readonly ICheck[] _checks; //TODO Probably rethink this

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

			//Check for duplicates, don't include them
			var checkList = new List<ICheck>();
			AddChecks(_updatableChecks);
			AddChecks(_noUnitChecks);
			AddChecks(_unitChecks);
			AddChecks(_usableChecks);
			AddChecks(_stateResetChecks);
			_checks = checkList.ToArray();

			void AddChecks(ICheck[] checks)
			{
				for (int i = 0; i < checks?.Length; i++)
				{
					if (checkList.Contains(checks[i]))
						continue;

					checkList.Add(checks[i]);
				}
			}
		}

		public ICheck[] GetChecks() => _checks;

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

		public SaveData SaveState()
		{
			var stateCheckData = new object[_stateResetChecks?.Length ?? 0];
			for (int i = 0; i < _stateResetChecks?.Length; i++)
				stateCheckData[i] = _stateResetChecks[i].SaveState();
			return new SaveData(stateCheckData);
		}

		public void LoadState(SaveData data)
		{
			for (int i = 0; i < _stateResetChecks?.Length; i++)
			{
#if JSON_SERIALIZATION && (NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_1_OR_GREATER || NET5_0_OR_GREATER || NET462_OR_GREATER || NETCOREAPP2_1_OR_GREATER)
				if (data.StateCheckData[i].FromAnonymousJsonObjectToSaveData(_stateResetChecks[i]))
					continue;
#endif

				_stateResetChecks[i].LoadState(data.StateCheckData[i]);
			}
		}

		public readonly struct SaveData
		{
			public readonly IReadOnlyList<object> StateCheckData;

#if JSON_SERIALIZATION && (NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_1_OR_GREATER || NET5_0_OR_GREATER || NET462_OR_GREATER || NETCOREAPP2_1_OR_GREATER)
			[System.Text.Json.Serialization.JsonConstructor]
#endif
			public SaveData(IReadOnlyList<object> stateCheckData) => StateCheckData = stateCheckData;
		}
	}
}