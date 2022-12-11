namespace ModifierLibraryLite.Core
{
	public class InitComponent : IInitComponent
	{
		private ITargetComponent _targetComponent;
		private readonly IEffect[] _effects;

		public InitComponent(IEffect effect) : this(new[] { effect })
		{
		}

		public InitComponent(IEffect[] effects)
		{
			_effects = effects;
		}

		public void SetupTarget(ITargetComponent targetComponent) => _targetComponent = targetComponent;

		public void Init()
		{
			int length = _effects.Length;
			for (int i = 0; i < length; i++)
				_effects[i].Effect(_targetComponent.Target, _targetComponent.Owner);
		}
	}
}