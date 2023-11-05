using System.Reflection;

namespace ModiBuff.Core
{
	public static class EffectMethods
	{
		public static readonly MethodInfo Effect;
		public static readonly MethodInfo StackEffect;

		public static readonly object[] EffectParameters;
		public static readonly object[] StackEffectParameters;

		static EffectMethods()
		{
			Effect = typeof(IEffect).GetRuntimeMethod("Effect", new[] { typeof(IUnit), typeof(IUnit) });
			StackEffect = typeof(IStackEffect).GetRuntimeMethod("StackEffect",
				new[] { typeof(int), typeof(IUnit), typeof(IUnit) });
			EffectParameters = new object[2];
			StackEffectParameters = new object[3];
		}

		public static object[] SetEffectParameters(IUnit target, IUnit source)
		{
			EffectParameters[0] = target;
			EffectParameters[1] = source;
			return EffectParameters;
		}

		public static object[] SetStackParameters(int stacks, IUnit target, IUnit source)
		{
			StackEffectParameters[0] = stacks;
			StackEffectParameters[1] = target;
			StackEffectParameters[2] = source;
			return StackEffectParameters;
		}
	}
}