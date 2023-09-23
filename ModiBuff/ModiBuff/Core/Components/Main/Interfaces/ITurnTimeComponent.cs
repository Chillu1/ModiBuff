namespace ModiBuff.Core
{
	public interface ITurnTimeComponent : ITimeComponent
	{
		void UpdateTurn(int count = 1);
	}
}