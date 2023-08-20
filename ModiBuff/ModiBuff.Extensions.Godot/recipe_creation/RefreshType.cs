using Godot;

namespace ModiBuff.Extensions.Godot
{
	public enum RefreshType
	{
		Invalid,
		Interval,
		Duration,
	}

	public static class RefreshTypeExtensions
	{
		public static ModiBuff.Core.RefreshType GetModiBuffRefreshType(this RefreshType refreshType)
		{
			switch (refreshType)
			{
				case RefreshType.Interval:
					return ModiBuff.Core.RefreshType.Interval;
				case RefreshType.Duration:
					return ModiBuff.Core.RefreshType.Duration;
				default:
					GD.PushError($"Invalid RefreshType: {refreshType}");
					return ModiBuff.Core.RefreshType.Interval;
			}
		}
	}
}