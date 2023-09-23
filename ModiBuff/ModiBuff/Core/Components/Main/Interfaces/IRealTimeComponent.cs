namespace ModiBuff.Core
{
	public interface IRealTimeComponent : ITimeComponent
	{
		void Update(float deltaTime);
	}
}