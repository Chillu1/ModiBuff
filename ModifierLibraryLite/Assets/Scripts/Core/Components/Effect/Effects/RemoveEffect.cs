using UnityEngine;

namespace ModifierLibraryLite.Core
{
	public class RemoveEffect : IRemoveEffect
	{
		private IRevertEffect[] _revertibleEffects;
		private readonly int _id;

		public RemoveEffect()
		{
			_id = ModifierIdManager.CurrentId;
		}

		public void SetRevertibleEffects(IRevertEffect[] revertibleEffects)
		{
			_revertibleEffects = revertibleEffects;
		}

		public void Effect(IUnit target, IUnit acter)
		{
			//Debug.Log("RemoveEffect Effect, modifier id: " + _modifier.Id);
			for (int i = 0; i < _revertibleEffects.Length; i++)
				_revertibleEffects[i].RevertEffect(target, acter);

			//Still not fully ideal, but fixed the state issue 
			target.PrepareRemoveModifier(_id); //TODO From which collection? Applier support?
		}
	}
}