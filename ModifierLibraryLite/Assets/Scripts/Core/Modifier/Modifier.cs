using JetBrains.Annotations;
using ModifierLibraryLite.Utilities;

namespace ModifierLibraryLite.Core
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
			if (_timeComponents != null)
			{
				int length = _timeComponents.Length;
			
				for (int i = 0; i < length; i++)
					_timeComponents[i].Update(deltaTime);
			}
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