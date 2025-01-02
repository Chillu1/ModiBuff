namespace ModiBuff.Core.Units
{
	public static class EffectExtensions
	{
		public static object[] GetMetaSaveData<T>(this ISaveableRecipeEffect effect, T[] metaEffects)
			where T : IMetaEffect
		{
			if (metaEffects == null)
				return null;

			object[] metaEffectSaveData = new object[metaEffects.Length];
			for (int i = 0; i < metaEffects.Length; i++)
				metaEffectSaveData[i] = ((ISaveableRecipeEffect)metaEffects[i]).SaveRecipeState();

			return metaEffectSaveData;
		}

		public static object[] GetPostSaveData<T>(this ISaveableRecipeEffect effect, T[] metaEffects)
			where T : IPostEffect
		{
			if (metaEffects == null)
				return null;

			object[] metaEffectSaveData = new object[metaEffects.Length];
			for (int i = 0; i < metaEffects.Length; i++)
				metaEffectSaveData[i] = ((ISaveableRecipeEffect)metaEffects[i]).SaveRecipeState();

			return metaEffectSaveData;
		}
	}
}