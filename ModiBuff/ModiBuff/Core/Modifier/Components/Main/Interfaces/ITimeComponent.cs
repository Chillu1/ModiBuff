namespace ModiBuff.Core
{
	public interface ITimeComponent : IStateReset, ITarget, ITimeReference
	{
		void Update(float deltaTime);
		void Refresh();
		void SetData(ModifierData data);

		TimeComponentSaveData SaveState();
		void LoadState(TimeComponentSaveData saveData);
	}

	public readonly struct TimeComponentSaveData
	{
		public readonly float Timer;
		public readonly float? CustomTime;

#if MODIBUFF_SYSTEM_TEXT_JSON
		[System.Text.Json.Serialization.JsonConstructor]
#endif
		public TimeComponentSaveData(float timer, float? customTime)
		{
			Timer = timer;
			CustomTime = customTime;
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