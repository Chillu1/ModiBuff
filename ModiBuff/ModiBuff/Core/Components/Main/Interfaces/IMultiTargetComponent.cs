using System.Collections.Generic;

namespace ModiBuff.Core
{
	public interface IMultiTargetComponent : ITargetComponent
	{
		List<IUnit> Targets { get; }
	}
}