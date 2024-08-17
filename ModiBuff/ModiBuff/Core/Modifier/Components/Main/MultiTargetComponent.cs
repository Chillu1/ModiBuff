using System.Collections.Generic;
using System.Linq;

namespace ModiBuff.Core
{
	public sealed class MultiTargetComponent : ITargetComponent, ISavable<MultiTargetComponent.SaveData>
	{
		public IUnit Source { get; set; }
		public IList<IUnit> Targets { get; private set; }

		public MultiTargetComponent()
		{
			Targets = new List<IUnit>(Config.MultiTargetComponentInitialCapacity);
		}

		public MultiTargetComponent(IList<IUnit> targets, IUnit source)
		{
			Source = source;
			Targets = targets;
		}

		public void UpdateTargets(IList<IUnit> targets) => Targets = targets;

		public void ResetState()
		{
			Source = null;
			Targets = null;
		}

		public object SaveState() =>
			new SaveData(Targets.Select(t => ((IIdOwner)t).Id).ToArray(), ((IIdOwner)Source).Id);

		public void LoadState(object saveData)
		{
			//Might want to clear in case we add ourselves as aura target on load
			//Then we add it back after
			Targets.Clear();

			var data = (SaveData)saveData;
			for (int i = 0; i < data.TargetsId.Count; i++)
				Targets.Add(UnitHelper.GetUnit(data.TargetsId[i]));
			Source = UnitHelper.GetUnit(data.SourceId);
		}

		public readonly struct SaveData
		{
			public readonly IReadOnlyList<int> TargetsId;
			public readonly int SourceId;

#if MODIBUFF_SYSTEM_TEXT_JSON
			[System.Text.Json.Serialization.JsonConstructor]
#endif
			public SaveData(IReadOnlyList<int> targets, int source)
			{
				TargetsId = targets;
				SourceId = source;
			}
		}
	}
}