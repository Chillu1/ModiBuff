namespace ModifierLibraryLite.Core
{
	public interface ITimeComponent : IDeepClone<ITimeComponent>, ITarget, IComponent
	{
		void Update(in float deltaTime);
	}
}