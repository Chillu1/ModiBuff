using System.Collections.Generic;

namespace ModiBuff.Core
{
	public sealed class MultiTargetComponent : ITargetComponent
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

		public ITargetComponentSaveData SaveState() => new SaveData(Targets, Source);

		public void LoadState(ITargetComponentSaveData saveData)
		{
			var data = (SaveData)saveData;
			Targets.Clear();
			Targets.AddRange(data.Targets);
			Source = data.Source;
		}

		public readonly struct SaveData : ITargetComponentSaveData
		{
			//public readonly int[] TargetsId;
			//public readonly int SourceId;
			public readonly List<IUnit> Targets;
			public readonly IUnit Source;

			public SaveData(List<IUnit> targets, IUnit source)
			{
				Targets = targets;
				Source = source;
			}
		}
	}
}