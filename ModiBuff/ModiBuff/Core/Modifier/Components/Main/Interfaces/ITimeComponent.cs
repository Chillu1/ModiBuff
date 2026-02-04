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

	public interface ITimeTickComponent : IStateReset, ITarget, ITimeTickReference
	{
		void Update();
		void Refresh();
		void SetData(ModifierData data);

		TimeTickComponentSaveData SaveState();
		void LoadState(TimeTickComponentSaveData saveData);
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

	public readonly struct TimeTickComponentSaveData
	{
		public readonly int CurrentTicks;
		public readonly int? CustomTicks;

#if MODIBUFF_SYSTEM_TEXT_JSON
		[System.Text.Json.Serialization.JsonConstructor]
#endif
		public TimeTickComponentSaveData(int currentTicks, int? customTicks)
		{
			CurrentTicks = currentTicks;
			CustomTicks = customTicks;
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

	public interface ITimeTickReference
	{
		float CurrentTicks { get; }

		/// <summary>
		///		Duration/Interval of the timer
		/// </summary>
		float Ticks { get; }
	}
}