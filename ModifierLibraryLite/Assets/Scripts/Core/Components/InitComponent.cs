namespace ModifierLibraryLite.Core
{
	public class InitComponent : IInitComponent
	{
		private readonly  ITargetComponent _targetComponent;
		private readonly IEffect[] _effects;
		
		public void Init()
		{
			foreach (var effect in _effects)
				effect.Effect(_targetComponent.Target);
		}
	}
}