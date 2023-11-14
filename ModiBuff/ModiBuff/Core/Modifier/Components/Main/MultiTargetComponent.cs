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

		public object SaveState() => new SaveData(Targets, Source);

		public void LoadState(object saveData)
		{
			var data = (SaveData)saveData;
			Targets.Clear();
			Targets.AddRange(data.Targets);
			Source = data.Source;
		}

		public readonly struct SaveData
		{
			//public readonly int[] TargetsId;
			//public readonly int SourceId;
			public readonly List<IUnit> Targets;
			public readonly IUnit Source;

#if JSON_SERIALIZATION && (NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_1_OR_GREATER || NET5_0_OR_GREATER)
			[System.Text.Json.Serialization.JsonConstructor]
#endif
			public SaveData(List<IUnit> targets, IUnit source)
			{
				Targets = targets;
				Source = source;
			}
		}
	}
}