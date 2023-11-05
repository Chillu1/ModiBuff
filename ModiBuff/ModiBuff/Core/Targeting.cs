using System;
using System.Runtime.CompilerServices;

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

	//Possible alternative to targeting (harder to work with?), also simplifies targeting code
	/*
	   TargetIsTarget = 1,
	   TargetIsSource = 2,
	   SourceIsSource = 4,
	   SourceIsTarget = 8,

	   Default = TargetIsTarget | SourceIsSource,
	 */

	public static class TargetingExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void UpdateTargetSource(this Targeting targeting, ref IUnit target, ref IUnit source)
		{
			IUnit finalTarget, finalSource;

			switch (targeting)
			{
				case Targeting.TargetSource:
					finalTarget = target;
					finalSource = source;
					break;
				case Targeting.SourceTarget:
					finalTarget = source;
					finalSource = target;
					break;
				case Targeting.TargetTarget:
					finalTarget = target;
					finalSource = target;
					break;
				case Targeting.SourceSource:
					finalTarget = source;
					finalSource = source;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			target = finalTarget;
			source = finalSource;
		}

		/// <summary>
		///		Use if source is not being used in the effect
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void UpdateTarget(this Targeting targeting, ref IUnit target, IUnit source)
		{
			switch (targeting)
			{
				case Targeting.TargetSource:
				case Targeting.TargetTarget:
					break;
				case Targeting.SourceTarget:
				case Targeting.SourceSource:
					target = source;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}