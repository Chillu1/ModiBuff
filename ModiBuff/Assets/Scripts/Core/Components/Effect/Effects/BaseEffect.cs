using System.Collections.Generic;

namespace ModiBuff.Core
{
	//Hate to use inheritance, but otherwise we have code duplication in each effect
	public abstract class BaseEffect : IEffect
	{
		public abstract void Effect(IUnit target, IUnit source);

		public void Effect(IList<IUnit> targets, IUnit source)
		{
			for (int i = 0; i < targets.Count; i++)
				Effect(targets[i], source);
		}
	}
}