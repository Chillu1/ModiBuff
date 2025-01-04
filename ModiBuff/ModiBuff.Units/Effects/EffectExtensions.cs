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
				var metaEffect = metaEffects[i];
				int id = EffectTypeIdManager.Instance.GetMetaId(metaEffect.GetType());
				if (metaEffect is not ISaveableRecipeEffect recipeEffect)
				{
					//TODO
					//Logger.Log($"[ModiBuff] Tried to save meta effect recipe data for {metaEffect.GetType()}, " +
					//           "which doesn't implement ISaveableRecipeEffect");
					continue;
				}

				metaEffectSaveData[i] = new MetaRecipeSaveData(id, recipeEffect.SaveRecipeState());
			}

			return metaEffectSaveData;
		}

		public static object[] GetPostSaveData<T>(this ISaveableRecipeEffect effect, T[] postEffects)
			where T : IPostEffect
		{
			if (postEffects == null)
				return null;

			object[] postEffectSaveData = new object[postEffects.Length];
			for (int i = 0; i < postEffects.Length; i++)
			{
				var postEffect = postEffects[i];
				int id = EffectTypeIdManager.Instance.GetPostId(postEffect.GetType());
				if (postEffect is not ISaveableRecipeEffect recipeEffect)
				{
					//TODO
					//Logger.Log($"[ModiBuff] Tried to save post effect recipe data for {metaEffect.GetType()}, " +
					//           "which doesn't implement ISaveableRecipeEffect");
					continue;
				}

				postEffectSaveData[i] = new MetaRecipeSaveData(id, recipeEffect.SaveRecipeState());
			}

			return postEffectSaveData;
		}
	}
}