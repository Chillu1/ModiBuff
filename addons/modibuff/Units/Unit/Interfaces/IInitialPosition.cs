namespace ModiBuff.Core.Units
{
	public interface IInitialPosition<out TPosition>
	{
		TPosition InitialPosition { get; }
	}
}