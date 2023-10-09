namespace ModiBuff.Core
{
	public interface ITimeComponent : IStateReset, IUpdateOwner, ITarget, ITimeReference
	{
		void Update(float deltaTime);
		void Refresh();
	}

	public interface ITimeReference
	{
		/// <summary>
		///		The ticking timer
		/// </summary>
		float Timer { get; }

		/// <summary>
		///		Duration/Interval of the timer
		/// </summary>
		float Time { get; }
	}
}