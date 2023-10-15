using System.Collections.Generic;

namespace ModiBuff.Core
{
	public static class Config
	{
		public const int DefaultDefaultTag = (int)TagType.Default;

		public const bool DefaultUseDictionaryIndexes = false;
		public const int DefaultPoolSize = 64;
		public const int DefaultMaxPoolSize = 16_384;

		public const int DefaultModifierArraySize = 32;
		public const int DefaultModifierRemoveSize = 4;
		public const int DefaultModifierIndexDictionarySize = 8;

		public const int DefaultMultiTargetComponentInitialCapacity = 4;

		public const int DefaultAttackApplierSize = 4,
			DefaultCastApplierSize = 4,
			DefaultAttackCheckApplierSize = 4,
			DefaultCastCheckApplierSize = 4;

		public const float DefaultDeltaTolerance = 0.001f;


		public static int DefaultTag = DefaultDefaultTag;

		/// <summary>
		///		Whether to use dictionary or an array for modifiers.
		///		Arrays are faster (by 30%±), but use O(n) int32 memory where n is the number of modifiers.
		///		Use it if you have a lot (500+) modifiers, and more than 5000 units,
		///		and you care about using less than 10MB of memory. 
		/// </summary>
		public static bool UseDictionaryIndexes = DefaultUseDictionaryIndexes;

		public static int PoolSize = DefaultPoolSize;
		public static int MaxPoolSize = DefaultMaxPoolSize;

		public static int ModifierArraySize = DefaultModifierArraySize;
		public static int ModifierRemoveSize = DefaultModifierRemoveSize;
		public static int ModifierIndexDictionarySize = DefaultModifierIndexDictionarySize;

		public static int MultiTargetComponentInitialCapacity = DefaultMultiTargetComponentInitialCapacity;

		public static int AttackApplierSize = DefaultAttackApplierSize;
		public static int CastApplierSize = DefaultCastApplierSize;
		public static int AttackCheckApplierSize = DefaultAttackCheckApplierSize;
		public static int CastCheckApplierSize = DefaultCastCheckApplierSize;

		public static float DeltaTolerance = DefaultDeltaTolerance;

		/// <summary>
		///		How many modifiers should the pool allocate for each modifier type. Should be a power of 2.
		/// </summary>
		public static Dictionary<string, int> ModifierAllocationsCount;

		static Config()
		{
			ModifierAllocationsCount = new Dictionary<string, int>();
		}

		public static void Reset()
		{
			DefaultTag = DefaultDefaultTag;

			UseDictionaryIndexes = DefaultUseDictionaryIndexes;
			PoolSize = DefaultPoolSize;
			MaxPoolSize = DefaultMaxPoolSize;

			ModifierArraySize = DefaultModifierArraySize;
			ModifierRemoveSize = DefaultModifierRemoveSize;
			ModifierIndexDictionarySize = DefaultModifierIndexDictionarySize;

			MultiTargetComponentInitialCapacity = DefaultMultiTargetComponentInitialCapacity;

			AttackApplierSize = DefaultAttackApplierSize;
			CastApplierSize = DefaultCastApplierSize;
			AttackCheckApplierSize = DefaultAttackCheckApplierSize;
			CastCheckApplierSize = DefaultCastCheckApplierSize;

			DeltaTolerance = DefaultDeltaTolerance;
		}
	}
}