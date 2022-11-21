namespace ModifierLibraryLite.Core
{
	public class InitComponent : IInitComponent
	{
		private readonly ITargetComponent _targetComponent;
		private readonly IEffect[] _effects;

		public InitComponent(ITargetComponent target, IEffect effect) : this(target, new[] { effect })
		{
		}

		public InitComponent(ITargetComponent target, IEffect[] effects)
		{
			_targetComponent = target;
			_effects = effects;
		}

		public void Init()
		{
			int length = _effects.Length;
			for (int i = 0; i < length; i++)
				_effects[i].Effect(_targetComponent.Target, _targetComponent.Owner);
		}
	}
}