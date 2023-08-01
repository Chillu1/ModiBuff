namespace ModiBuff.Core
{
	public enum Targeting
	{
		/// <summary>
		///		Target gets attacked by Source
		/// </summary>
		TargetSource,

		/// <summary>
		///		Source gets attacked by Target
		/// </summary>
		SourceTarget,

		/// <summary>
		///		Target gets attacked by Target
		/// </summary>
		TargetTarget,

		/// <summary>
		///		Source gets attacked by Source
		/// </summary>
		SourceSource,
	}
}