namespace ModiBuff.Core
{
	public interface IRevertEffect
	{
		bool IsRevertible { get; }

		void RevertEffect(IUnit target, IUnit acter);
	}
}