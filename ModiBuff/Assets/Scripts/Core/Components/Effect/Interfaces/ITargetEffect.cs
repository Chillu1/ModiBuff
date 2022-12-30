namespace ModiBuff.Core
{
	/// <summary>
	///		When an effect can be applied to the acter, or switch target and acter.
	/// </summary>
	public interface ITargetEffect
	{
		void SetTargeting(Targeting targeting);
	}
}