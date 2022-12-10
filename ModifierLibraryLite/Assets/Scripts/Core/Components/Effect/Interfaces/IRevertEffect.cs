namespace ModifierLibraryLite.Core
{
	public interface IRevertEffect : IEffect
	{
		bool IsRevertible { get; }

		void RevertEffect(IUnit target, IUnit owner);
	}
}