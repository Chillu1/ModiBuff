namespace ModiBuff.Core
{
	//TODO Rethink
	public interface IDataCheck<out TData> : ICheck where TData : struct
	{
		TData GetData();
	}
}