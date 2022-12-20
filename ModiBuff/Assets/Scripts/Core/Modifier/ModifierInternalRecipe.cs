using JetBrains.Annotations;

namespace ModiBuff.Core
{
	/// <summary>
	///		Lower level recipe for a modifier.
	/// </summary>
	public class ModifierInternalRecipe
	{
		public int Id { get; }
		public string Name { get; }

		[CanBeNull] public InitComponent InitComponent { get; private set; }
		[CanBeNull] public ITimeComponent[] TimeComponents { get; private set; }

		[CanBeNull] public IStackComponent StackComponent { get; private set; }

		public ModifierInternalRecipe(string name)
		{
			Name = name;
		}

		internal ModifierInternalRecipe(int id, string name, InitComponent initComponent, ITimeComponent[] timeComponents,
			IStackComponent stackComponent)
		{
			Id = id;
			Name = name;
			InitComponent = initComponent;
			TimeComponents = timeComponents;
			StackComponent = stackComponent;
		}

		public void SetInitComponent(InitComponent initComponent)
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
	}
}