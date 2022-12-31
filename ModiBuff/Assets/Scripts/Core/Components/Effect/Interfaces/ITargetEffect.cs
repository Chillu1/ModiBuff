namespace ModiBuff.Core
{
	/// <summary>
	///		When an effect can be applied to the source, or switch target and source.
	/// </summary>
	public interface ITargetEffect
	{
		void SetTargeting(Targeting targeting);
	}
}