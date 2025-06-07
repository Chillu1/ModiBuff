namespace ModiBuff.Core
{
	public interface ITimeComponent : IStateReset, IUpdateOwner, ITarget, ITimeReference
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
		public readonly bool StatusResistanceImplemented;
		public readonly float? CustomTime;

#if MODIBUFF_SYSTEM_TEXT_JSON
		[System.Text.Json.Serialization.JsonConstructor]
#endif
		public TimeComponentSaveData(float timer, bool statusResistanceImplemented, float? customTime)
		{
			Timer = timer;
			StatusResistanceImplemented = statusResistanceImplemented;
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