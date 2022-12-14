using UnityEngine;

namespace ModifierLibraryLite.Core.Units
{
	public class GameController : MonoBehaviour
	{
		private Modifier[] _modifiers;
		private Unit _unit;
		private ModifierRecipe _recipe;
		private Unit[] _units;

		private void Start()
		{
			var coreSystem = new CoreSystem();

			_modifiers = new Modifier[20_000];
			_unit = new Unit();
			_units = new Unit[1_00];
			for (int i = 0; i < _units.Length; i++)
			{
				_units[i] = new Unit();
				_units[i].TryAddModifier("InitDamage_RemoveFast", _units[i]);
			}
		}

		private void Update()
		{
			float delta = Time.deltaTime;
			for (int i = 0; i < _units.Length; i++)
			{
				//if (Time.frameCount % 10 == 0)
				//	_unit.TryAddModifier(_recipe.Id, _unit);
				_units[i].Update(delta);
				//_unit.TryAddModifier(_recipe.Id, _unit);
				//_unit.RemoveModifier(_recipe.Id);
			}

			//_unit.Update(delta);
			//int length = _modifiers.Length;
			//for (int i = 0; i < length; i++)
			//	_modifiers[i].Update(delta);
		}
	}
}