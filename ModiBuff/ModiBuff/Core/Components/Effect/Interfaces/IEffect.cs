using System.Collections.Generic;

namespace ModiBuff.Core
{
	public interface IEffect
	{
		/// <param name="source">owner/acter</param>
		void Effect(IUnit target, IUnit source);
	}

	public static class EffectExtensions
	{
		public static void Effect(this IEffect effect, IList<IUnit> targets, IUnit source)
		{
			for (int i = 0; i < targets.Count; i++)
				effect.Effect(targets[i], source);
		}
	}
}