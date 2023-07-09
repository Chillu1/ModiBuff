using UnityEngine;

namespace ModiBuff.Core.Units
{
	public class GameController : MonoBehaviour
	{
		private ModifierRecipe _recipe;
		private Unit[] _units;
		private ModifierPool _pool;

		private int _recipeId;

		private void Start()
		{
			var coreSystem = new CoreSystem();
			_pool = coreSystem.Pool;
			_recipeId = ModifierIdManager.GetId("DoT");
			//var _recipeId2 = ModifierIdManager.GetId("InitDoTSeparateDamageRemove");

			//_modifiers = new Modifier[20_000];
			//_unit = new Unit();
			_units = new Unit[5_000];
			for (int i = 0; i < _units.Length; i++)
			{
				_units[i] = new Unit();
				_units[i].TryAddModifier(_recipeId, _units[i]);
				//_units[i].TryAddModifier(_recipeId2, _units[i], _units[i]);
			}
		}

		private void Update()
		{
			float delta = Time.deltaTime;
			int length = _units.Length;
			for (int i = 0; i < length; i++)
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