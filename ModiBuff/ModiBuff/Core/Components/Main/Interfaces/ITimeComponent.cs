namespace ModiBuff.Core
{
	public interface ITimeComponent : IStateReset, IUpdateOwner, ITarget, IComponent
	{
		bool IsRefreshable { get; }
		void Update(float deltaTime);
		void Refresh();
	}
}