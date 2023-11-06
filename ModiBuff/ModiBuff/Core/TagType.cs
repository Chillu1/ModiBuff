using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ModiBuff.Core
{
	/// <summary>
	///		Core internal modifier tags, can be combined with user tags.
	/// </summary>
	[Flags]
	public enum TagType : ulong
	{
		Default = IntervalIgnoresStatusResistance,

		None = 0,
		IsInit = 1ul << 0,
		IsRefresh = 1ul << 1,
		IsStack = 1ul << 2,
		IsInstanceStackable = 1ul << 3,
		IntervalIgnoresStatusResistance = 1ul << 4,
		DurationIgnoresStatusResistance = 1ul << 5,

		//Most likely need to reserve around 8-16 bits, or split internal and user tagging
		//Will be decided/fixed on 1.0 release

		/// <summary>
		///		One stack is always added and triggered on init, this tag nullifies that.
		/// </summary>
		ZeroDefaultStacks = 1ul << 6,
		Reserved2 = ZeroDefaultStacks << 1,
		Reserved3 = Reserved2 << 1,
		Reserved4 = Reserved3 << 1,
		Reserved5 = Reserved4 << 1,
		Reserved6 = Reserved5 << 1,
		Reserved7 = Reserved6 << 1,
		Reserved8 = Reserved7 << 1,
		Reserved9 = Reserved8 << 1,
		Reserved10 = Reserved9 << 1,
		Reserved11 = Reserved10 << 1,
		Reserved12 = Reserved11 << 1,

		LastReserved = Reserved12
	}

	public static class TagTypeExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasTag(this TagType tagType, TagType tag)
		{
			return (tagType & tag) == tag;
		}

		public static void UpdateTagBasedOnModifierComponents(this ref TagType tag, Modifier modifier) =>
			TagTypeUtils.UpdateTagBasedOnModifierComponents(ref tag, modifier);
	}

	public static class TagTypeUtils
	{
		private static readonly FieldInfo[] modifierFields = typeof(Modifier).GetRuntimeFields().ToArray();
		private static readonly FieldInfo hasInitField = modifierFields.First(field => field.Name == "_hasInit");

		private static readonly FieldInfo hasStackComponentField =
			modifierFields.First(field => field.Name == "_stackComponent");

		private static readonly FieldInfo timeComponentsField =
			modifierFields.First(field => field.Name == "_timeComponents");

		private static readonly FieldInfo isRefreshableIntervalField =
			typeof(IntervalComponent).GetRuntimeFields().First(field => field.Name == "_isRefreshable");

		private static readonly FieldInfo isRefreshableDurationField =
			typeof(DurationComponent).GetRuntimeFields().First(field => field.Name == "_isRefreshable");

		public static void UpdateTagBasedOnModifierComponents(ref TagType tag, Modifier modifier)
		{
			if ((bool)hasInitField.GetValue(modifier))
				tag |= TagType.IsInit;
			if (hasStackComponentField.GetValue(modifier) != null)
				tag |= TagType.IsStack;

			var timeComponents = (ITimeComponent[])timeComponentsField.GetValue(modifier);
			if (timeComponents != null)
			{
				foreach (var component in timeComponents)
				{
					switch (component)
					{
						case IntervalComponent intervalComponent:
						{
							if ((bool)isRefreshableIntervalField.GetValue(intervalComponent))
								tag |= TagType.IsRefresh;
							break;
						}
						case DurationComponent durationComponent:
						{
							if ((bool)isRefreshableDurationField.GetValue(durationComponent))
								tag |= TagType.IsRefresh;
							break;
						}
					}
				}
			}
		}
	}
}