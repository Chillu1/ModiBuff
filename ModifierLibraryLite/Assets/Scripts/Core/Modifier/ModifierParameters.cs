using JetBrains.Annotations;

namespace ModifierLibraryLite.Core
{
	public class ModifierParameters
	{
		[CanBeNull] public IInitComponent InitComponent { get; private set; }
		[CanBeNull] public ITimeComponent[] TimeComponents { get; private set; }
		[CanBeNull] public IStackComponent StackComponent { get; private set; }

		public void SetInitComponent(IInitComponent initComponent)
		{
			InitComponent = initComponent;
			//If init != null, log warning
		}

		public void SetTimeComponents(params ITimeComponent[] timeComponents)
		{
			TimeComponents = timeComponents;
		}
		
		public void SetStackComponent(IStackComponent stackComponent)
		{
			StackComponent = stackComponent;
		}

		public void SetRefreshable()
		{
			//TODO
		}
	}
}