using System.Runtime.CompilerServices;

namespace ModiBuff.Core
{
	public struct ModifierLessEffect
	{
		private readonly IEffect[] _effects;

		public ModifierLessEffect(params IEffect[] effects)
		{
			_effects = effects;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Apply(IUnit target, IUnit source)
		{
			for (int i = 0; i < _effects.Length; i++)
				_effects[i].Effect(target, source);
		}
	}
}