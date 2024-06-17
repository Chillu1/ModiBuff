using System;
using System.Linq;
using System.Reflection;

namespace ModiBuff.Core
{
	public partial class ModifierRecipe
	{
		public SaveData SaveState()
		{
			if (_unsavableEffects.Count > 0)
			{
				Logger.LogWarning("[ModiBuff] Saving recipe with unsavable effects, please implement " +
				                  $"{nameof(ISaveableRecipeEffect)} for the following effects: " +
				                  string.Join(", ", _unsavableEffects
					                  .Where(e => !SpecialInstructionEffects.IsSpecialInstructionEffect(e))
					                  .Select(e => e.Name)));
			}

			return new SaveData(_saveInstructions.ToArray());
		}

		public void LoadState(SaveData saveData)
		{
			foreach (var instruction in saveData.Instructions)
			{
				switch (instruction.InstructionId)
				{
					case SaveInstruction.Initialize.Id:
						break;
					case SaveInstruction.Interval.Id:
#if MODIBUFF_SYSTEM_TEXT_JSON
						Interval(instruction.Values.GetDataFromJsonObject<float>());
#endif
						break;
					case SaveInstruction.Duration.Id:
#if MODIBUFF_SYSTEM_TEXT_JSON
						Duration(instruction.Values.GetDataFromJsonObject<float>());
#endif
						break;
					case SaveInstruction.Stack.Id:
#if MODIBUFF_SYSTEM_TEXT_JSON
						Stack(instruction.Values.GetDataFromJsonObject<WhenStackEffect>());
#endif
						break;
					case SaveInstruction.Effect.Id:
#if MODIBUFF_SYSTEM_TEXT_JSON
						var (effect, effectOn) = HandleEffect(instruction);
						if (effect != null)
							Effect(effect, effectOn);
#endif
						break;
					case SaveInstruction.ModifierAction.Id:
#if MODIBUFF_SYSTEM_TEXT_JSON
						object[] values = instruction.GetValues(typeof(ModifierAction), typeof(EffectOn));
						ModifierAction((ModifierAction)values[0], (EffectOn)values[1]);
#endif
						break;
					default:
						Logger.LogError($"Unknown instruction with id {instruction.InstructionId}");
						break;
				}
			}

			(IEffect, EffectOn) HandleEffect(SaveInstruction instruction)
			{
				var values = ((System.Text.Json.JsonElement)instruction.Values).EnumerateObject();
				bool failed = false;
				int? effectId = null;
				ConstructorInfo constructor = null;
				object[] effectStates = null;
				EffectOn? effectOn = null;
				foreach (var value in values)
				{
					//TODO Not hardcode/dynamic order?
					if (value.NameEquals("Item1"))
					{
						effectId = value.Value.GetInt32();
					}

					if (value.NameEquals("Item2"))
					{
						if (effectId == null)
						{
							Logger.LogWarning(
								$"[ModiBuff] Couldn't extract recipe save data because of missing effect id for {Name}");
							continue;
						}

						var effectType = _effectTypeIdManager.GetEffectType(effectId.Value);
						//TODO Find constructor by saved types? Prob not worth
						constructor = effectType.GetConstructors()[0];

						var parameters = constructor.GetParameters();
						effectStates = new object[parameters.Length];
						int i = 0;
						foreach (var property in value.Value.EnumerateObject())
						{
							effectStates[i] = property.Value.GetDataFromJsonObject(parameters[i].ParameterType);
							i++;
						}
					}

					if (value.NameEquals("Item3"))
					{
						effectOn = (EffectOn)value.Value.GetInt32();
					}
				}

				if (effectStates == null)
				{
					Logger.LogError(
						$"[ModiBuff] Failed to load effect state from save data by {Name} {(effectId != -1 ? $"for effect {effectId}" : "")}");
					failed = true;
				}

				if (effectOn == null)
				{
					Logger.LogError($"[ModiBuff] Failed to load effect on from save data by {Name}");
					failed = true;
				}

				if (effectId == null)
				{
					Logger.LogError($"[ModiBuff] Failed to load effect id from save data by {Name}");
					failed = true;
				}

				if (failed)
					return (null, EffectOn.None);

				return ((IEffect)constructor.Invoke(effectStates), effectOn.Value);
			}
		}

		public record SaveInstruction
		{
			public readonly object Values;
			public readonly int InstructionId;

			private const int BaseId = 0;

#if MODIBUFF_SYSTEM_TEXT_JSON
			[System.Text.Json.Serialization.JsonConstructor]
#endif
			public SaveInstruction(object values, int instructionId)
			{
				Values = values;
				InstructionId = instructionId;
			}

			public object[] GetValues(params Type[] types) => ((System.Text.Json.JsonElement)Values).GetValues(types);

			public sealed record Initialize : SaveInstruction
			{
				public const int Id = BaseId;

#if MODIBUFF_SYSTEM_TEXT_JSON
				[System.Text.Json.Serialization.JsonConstructor]
#endif
				public Initialize(string name, string displayName, string description)
					: base((name, displayName, description), Id)
				{
				}
			}

			public sealed record Interval : SaveInstruction
			{
				public const int Id = Initialize.Id + 1;

#if MODIBUFF_SYSTEM_TEXT_JSON
				[System.Text.Json.Serialization.JsonConstructor]
#endif
				public Interval(float interval) : base(interval, Id)
				{
				}
			}

			public sealed record Duration : SaveInstruction
			{
				public const int Id = Interval.Id + 1;

#if MODIBUFF_SYSTEM_TEXT_JSON
				[System.Text.Json.Serialization.JsonConstructor]
#endif
				public Duration(float duration) : base(duration, Id)
				{
				}
			}

			public sealed record Stack : SaveInstruction
			{
				public const int Id = Duration.Id + 1;

#if MODIBUFF_SYSTEM_TEXT_JSON
				[System.Text.Json.Serialization.JsonConstructor]
#endif
				public Stack(WhenStackEffect when) : base(when, Id)
				{
				}
			}

			public sealed record Effect : SaveInstruction
			{
				public const int Id = Stack.Id + 1;

#if MODIBUFF_SYSTEM_TEXT_JSON
				[System.Text.Json.Serialization.JsonConstructor]
#endif
				public Effect(int effectId, object saveData, EffectOn effectOn)
					: base((effectId, saveData, effectOn), Id)
				{
				}
			}

			public sealed record ModifierAction : SaveInstruction
			{
				public const int Id = Effect.Id + 1;

#if MODIBUFF_SYSTEM_TEXT_JSON
				[System.Text.Json.Serialization.JsonConstructor]
#endif
				public ModifierAction(ModiBuff.Core.ModifierAction modifierAction, EffectOn effectOn)
					: base((modifierAction, effectOn), Id)
				{
				}
			}
		}

		public readonly struct SaveData
		{
			public readonly SaveInstruction[] Instructions;

#if MODIBUFF_SYSTEM_TEXT_JSON
			[System.Text.Json.Serialization.JsonConstructor]
#endif
			public SaveData(SaveInstruction[] instructions) => Instructions = instructions;
		}
	}
}