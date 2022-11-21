using JetBrains.Annotations;
using ModifierLibraryLite.Utilities;

namespace ModifierLibraryLite
{
	//Basic mods
	//DoT
	//DoT refreshable duration

	public class Modifier : IModifier
	{
		[CanBeNull]
		private readonly IInitComponent _initComponent;
		[CanBeNull]
		private readonly ITimeComponent[] _timeComponents;
		[CanBeNull]
		private readonly IRefreshComponent _refreshComponent;
		[CanBeNull]
		private readonly IStackComponent _stackComponent;

		public Modifier(ModifierParameters parameters)
		{
			_initComponent = parameters.InitComponent;
			_timeComponents = parameters.TimeComponents;
			_stackComponent = parameters.StackComponent;
		}

		public void Init()
		{
			_initComponent?.Init();
		}
		
		public void Update(float deltaTime)
		{
			foreach (var timeComponent in _timeComponents.EmptyIfNull())
				timeComponent.Update(deltaTime);
		}

		public void Refresh()
		{
			_refreshComponent?.Refresh();
		}

		public void Stack()
		{
			_stackComponent?.Stack();
		}
	}
}