using System.Collections.Generic;

namespace ModiBuff.Core
{
	public static class Config
	{
		private const int DefaultDefaultTag = (int)TagType.Default;

		private const bool DefaultUseDictionaryIndexes = false;
		public const int DefaultPoolSize = 64;
		private const int DefaultMaxPoolSize = 16_384;

		private const int DefaultModifierControllerPoolSize = 256;
		private const int DefaultMaxModifierControllerPoolSize = 32_768;
		private const int DefaultModifierApplierControllerPoolSize = 64;

		public const int DefaultModifierArraySize = 32;
		private const int DefaultDispellableSize = 4;
		private const int DefaultModifierRemoveSize = 4;
		private const int DefaultModifierIndexDictionarySize = 8;

		private const int DefaultMultiTargetComponentInitialCapacity = 4;

		private const int DefaultAttackApplierSize = 4,
			DefaultCastApplierSize = 4,
			DefaultAttackCheckApplierSize = 4,
			DefaultCastCheckApplierSize = 4;

		private const int DefaultEffectCastsSize = 4;

		private const float DefaultDeltaTolerance = 0.001f;


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

		public static int ModifierControllerPoolSize = DefaultModifierControllerPoolSize;
		public static int MaxModifierControllerPoolSize = DefaultMaxModifierControllerPoolSize;
		public static int ModifierApplierControllerPoolSize = DefaultModifierApplierControllerPoolSize;

		public static int ModifierArraySize = DefaultModifierArraySize;
		public static int DispellableSize = DefaultDispellableSize;
		public static int ModifierRemoveSize = DefaultModifierRemoveSize;
		public static int ModifierIndexDictionarySize = DefaultModifierIndexDictionarySize;

		public static int MultiTargetComponentInitialCapacity = DefaultMultiTargetComponentInitialCapacity;

		public static int AttackApplierSize = DefaultAttackApplierSize;
		public static int CastApplierSize = DefaultCastApplierSize;
		public static int AttackCheckApplierSize = DefaultAttackCheckApplierSize;
		public static int CastCheckApplierSize = DefaultCastCheckApplierSize;

		public static int EffectCastsSize = DefaultEffectCastsSize;

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

			ModifierControllerPoolSize = DefaultModifierControllerPoolSize;
			MaxModifierControllerPoolSize = DefaultMaxModifierControllerPoolSize;
			ModifierApplierControllerPoolSize = DefaultModifierApplierControllerPoolSize;

			ModifierArraySize = DefaultModifierArraySize;
			DispellableSize = DefaultDispellableSize;
			ModifierRemoveSize = DefaultModifierRemoveSize;
			ModifierIndexDictionarySize = DefaultModifierIndexDictionarySize;

			MultiTargetComponentInitialCapacity = DefaultMultiTargetComponentInitialCapacity;

			AttackApplierSize = DefaultAttackApplierSize;
			CastApplierSize = DefaultCastApplierSize;
			AttackCheckApplierSize = DefaultAttackCheckApplierSize;
			CastCheckApplierSize = DefaultCastCheckApplierSize;

			EffectCastsSize = DefaultEffectCastsSize;

			DeltaTolerance = DefaultDeltaTolerance;
		}
	}
}