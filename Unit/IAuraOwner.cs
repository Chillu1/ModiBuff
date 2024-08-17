using System.Collections.Generic;

namespace ModiBuff.Core
{
	public interface IAuraOwner
	{
		IList<IUnit> GetAuraTargets(int auraId);
	}
}