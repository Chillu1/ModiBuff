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
	}
}