using UnityEngine;

namespace ModifierLibraryLite.Core
{
	public class GameController : MonoBehaviour
	{
		private Modifier[] _modifiers;

		private void Start()
		{
			_modifiers = new Modifier[20_000];
			for (int i = 0; i < _modifiers.Length; i++)
			{
				//var parameters = new ModifierParameters();
				//parameters.SetTimeComponents(new IntervalComponent(1f, new TargetComponent(), new DamageEffect(5)));
				//_modifiers[i] = new Modifier(parameters);
			}
		}

		private void Update()
		{
			float delta = Time.deltaTime;
			int length = _modifiers.Length;
			for (int i = 0; i < length; i++)
				_modifiers[i].Update(delta);
		}
	}
}