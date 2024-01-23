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

	public static class EffectHelper
	{
		public static void LogImplError(IUnit target, string interfaceName)
		{
			Logger.LogError($"[ModiBuff] Target {target} does not implement {interfaceName}");
		}

		public static void LogImplErrorSource(IUnit source, string interfaceName)
		{
			Logger.LogError($"[ModiBuff] Source {source} does not implement {interfaceName}");
		}
	}
}