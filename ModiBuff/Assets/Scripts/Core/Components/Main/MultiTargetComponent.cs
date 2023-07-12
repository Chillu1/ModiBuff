using System.Collections.Generic;

namespace ModiBuff.Core
{
	public sealed class MultiTargetComponent : IMultiTargetComponent
	{
		public IUnit Source { get; private set; }
		public List<IUnit> Targets { get; }


		public MultiTargetComponent(List<IUnit> targets, IUnit source)
		{
			Source = source;
			Targets = targets;
		}

		public void UpdateSource(IUnit source)
		{
			Source = source;
		}
	}
}