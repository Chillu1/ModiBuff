namespace ModiBuff.Core
{
	public interface ITimeComponent : IStateReset, IUpdateOwner, ITarget
	{
		void Update(float deltaTime);
		void Refresh();
	}
}