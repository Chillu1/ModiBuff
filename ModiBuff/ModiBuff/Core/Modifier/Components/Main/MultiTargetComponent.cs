using System.Collections.Generic;
using System.Linq;

namespace ModiBuff.Core
{
	public sealed class MultiTargetComponent : ITargetComponent, ISavable<MultiTargetComponent.SaveData>
	{
		public IUnit Source { get; set; }
		public List<IUnit> Targets { get; }

		public MultiTargetComponent()
		{
			Targets = new List<IUnit>(Config.MultiTargetComponentInitialCapacity);
		}

		public MultiTargetComponent(List<IUnit> targets, IUnit source)
		{
			Source = source;
			Targets = targets;
		}

		public void UpdateTargets(List<IUnit> targets)
		{
			Targets.Clear();
			Targets.AddRange(targets);
		}

		public void ResetState()
		{
			Source = null;
			Targets.Clear();
		}

		public object SaveState() =>
			new SaveData(Targets.Select(t => ((IIdOwner)t).Id).ToArray(), ((IIdOwner)Source).Id);

		public void LoadState(object saveData)
		{
			//Might want to clear in case we add ourselves as aura target on load
			//Then we add it back after
			Targets.Clear();

			var data = (SaveData)saveData;
			for (int i = 0; i < data.TargetsId.Length; i++)
				Targets.Add(UnitHelper.GetUnit(data.TargetsId[i]));
			Source = UnitHelper.GetUnit(data.SourceId);
		}

		public readonly struct SaveData
		{
			public readonly int[] TargetsId;
			public readonly int SourceId;

#if JSON_SERIALIZATION && (NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_1_OR_GREATER || NET5_0_OR_GREATER || NET462_OR_GREATER || NETCOREAPP2_1_OR_GREATER)
			[System.Text.Json.Serialization.JsonConstructor]
#endif
			public SaveData(int[] targets, int source)
			{
				TargetsId = targets;
				SourceId = source;
			}
		}
	}
}