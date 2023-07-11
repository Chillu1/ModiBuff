namespace ModiBuff.Core
{
	public interface ITimeComponent : IDeepClone<ITimeComponent>, IStateReset, ITarget, IComponent
	{
		bool IsRefreshable { get; }
		void Update(float deltaTime);
		void Refresh();
	}
}