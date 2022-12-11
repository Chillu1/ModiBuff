namespace ModifierLibraryLite.Core
{
	public interface ITimeComponent : IDeepClone<ITimeComponent>, ITarget, IComponent
	{
		bool IsRefreshable { get; }
		void Update(in float deltaTime);
		void Refresh();
		void ResetState();
	}
}