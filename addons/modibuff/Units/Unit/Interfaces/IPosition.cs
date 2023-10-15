namespace ModiBuff.Core.Units
{
	public interface IPosition<out TPosition>
	{
		TPosition Position { get; }
	}
}