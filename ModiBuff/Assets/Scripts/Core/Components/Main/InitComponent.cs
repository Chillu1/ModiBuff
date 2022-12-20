namespace ModiBuff.Core
{
	public class InitComponent
	{
		private readonly IEffect[] _effects;

		public InitComponent(IEffect effect) : this(new[] { effect })
		{
		}

		public InitComponent(IEffect[] effects)
		{
			_effects = effects;
		}

		public void Init(IUnit target, IUnit owner)
		{
			int length = _effects.Length;
			for (int i = 0; i < length; i++)
				_effects[i].Effect(target, owner);
		}
	}
}