using System;
using System.Runtime.CompilerServices;

namespace ModiBuff.Core
{
	public interface ICheck
	{
	}

	public static class CheckExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Use(this ICheck check, IUnit source)
		{
			if (check is IUsableCheck usableCheck)
				usableCheck.Use(source);

			if (check is IStateCheck stateCheck)
				stateCheck.RestartState();
		}
	}
}