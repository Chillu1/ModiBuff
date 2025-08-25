using System;

namespace ModiBuff.Core
{
	public interface ICheck
	{
	}

	public static class CheckExtensions
	{
		public static bool Check(this ICheck check, IUnit source)
		{
			switch (check)
			{
				case INoUnitCheck noUnitCheck:
					if (!noUnitCheck.Check())
						return false;
					break;
				case IUnitCheck unitCheck:
					if (!unitCheck.Check(source))
						return false;
					break;
				default:
					Logger.LogError("[ModiBuff] Unhandled check type: " + check.GetType());
					throw new ArgumentOutOfRangeException();
			}

			return true;
		}

		public static void Use(this ICheck check, IUnit source)
		{
			switch (check)
			{
				case IUsableCheck usableCheck:
					usableCheck.Use(source);
					break;
			}
		}
	}
}