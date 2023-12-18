namespace ModiBuff.Core
{
	public readonly struct ModifierAddData
	{
		public bool IsValid => DurationTime > 0;

		public readonly float DurationTime;

		public ModifierAddData(float durationTime)
		{
			DurationTime = durationTime;
		}
	}
}