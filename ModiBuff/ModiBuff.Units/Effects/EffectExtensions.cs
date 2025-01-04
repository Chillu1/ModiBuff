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
			{
				int id = EffectTypeIdManager.Instance.GetMetaId(metaEffects[i].GetType());
				metaEffectSaveData[i] =
					new MetaRecipeSaveData(id, ((ISaveableRecipeEffect)metaEffects[i]).SaveRecipeState());
			}

			return metaEffectSaveData;
		}

		public static object[] GetPostSaveData<T>(this ISaveableRecipeEffect effect, T[] postEffects)
			where T : IPostEffect
		{
			if (postEffects == null)
				return null;

			object[] metaEffectSaveData = new object[postEffects.Length];
			for (int i = 0; i < postEffects.Length; i++)
				metaEffectSaveData[i] = ((ISaveableRecipeEffect)postEffects[i]).SaveRecipeState();

			return metaEffectSaveData;
		}
	}
}