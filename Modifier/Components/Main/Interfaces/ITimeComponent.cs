namespace ModiBuff.Core
{
	public interface ITimeComponent : IStateReset, IUpdateOwner, ITarget, ITimeReference
	{
		void Update(float deltaTime);
		void Refresh();

		TimeComponentSaveData SaveState();
		void LoadState(TimeComponentSaveData saveData);
	}

	public readonly struct TimeComponentSaveData
	{
		public readonly float Timer;
		public readonly bool StatusResistanceImplemented;

#if JSON_SERIALIZATION && (NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_1_OR_GREATER || NET5_0_OR_GREATER || NET462_OR_GREATER || NETCOREAPP2_1_OR_GREATER)
		[System.Text.Json.Serialization.JsonConstructor]
#endif
		public TimeComponentSaveData(float timer, bool statusResistanceImplemented)
		{
			Timer = timer;
			StatusResistanceImplemented = statusResistanceImplemented;
		}
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