namespace ModiBuff.Core.Units
{
	public interface ITravelDistance<out TDistance> //TODO Rename
	{
		TDistance DistanceTraveled { get; }
	}
}