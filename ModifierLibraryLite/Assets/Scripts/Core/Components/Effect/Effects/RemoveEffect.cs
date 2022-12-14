using UnityEngine;

namespace ModifierLibraryLite.Core
{
	public class RemoveEffect : IRemoveEffect
	{
		private IRevertEffect[] _revertibleEffects;
		private Modifier _modifier;

		public void SetRevertibleEffects(IRevertEffect[] revertibleEffects)
		{
			_revertibleEffects = revertibleEffects;
		}

		//TODO Not ideal
		public void Setup(Modifier modifier)
		{
			_modifier = modifier;
		}

		public void Effect(IUnit target, IUnit acter)
		{
			//Debug.Log("RemoveEffect Effect, modifier id: " + _modifier.Id);
			for (int i = 0; i < _revertibleEffects.Length; i++)
				_revertibleEffects[i].RevertEffect(target, acter);
			_modifier.SetForRemoval();
		}
	}
}