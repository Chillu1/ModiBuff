using System;
using System.Linq;

namespace ModiBuff.Core
{
	public partial class ModifierRecipe
	{
		public SaveData SaveState()
		{
			if (_unsavableEffects.Count > 0)
			{
				var unsupportedInstructionEffects = _unsavableEffects
					.Where(SpecialInstructionEffects.IsUnsupportedEffectType).ToArray();
				var nonSpecialInstructionEffects = _unsavableEffects
					.Where(e => !SpecialInstructionEffects.IsSpecialInstructionEffect(e)).ToArray();

				if (unsupportedInstructionEffects.Length > 0)
					Logger.LogError("[ModiBuff] Saving recipe with unsupported effects: " +
					                string.Join(", ", unsupportedInstructionEffects.Select(e => e.Name)));

				if (nonSpecialInstructionEffects.Length > 0)
					Logger.LogWarning("[ModiBuff] Saving recipe with unsavable effects, please implement " +
					                  $"{nameof(ISaveableRecipeEffect)} for the following effects: " +
					                  string.Join(", ", nonSpecialInstructionEffects.Select(e => e.Name)));
			}

			return new SaveData(_saveInstructions.ToArray());
		}

		public void LoadState<TUnitCallback>(SaveData saveData)
		{
			foreach (var instruction in saveData.Instructions)
			{
				switch (instruction.InstructionId)
				{
					case SaveInstruction.Initialize.Id:
						break;
					case SaveInstruction.Interval.Id:
#if MODIBUFF_SYSTEM_TEXT_JSON
						Interval(((SaveInstruction.Interval)instruction).Value);
#endif
						break;
					case SaveInstruction.Duration.Id:
#if MODIBUFF_SYSTEM_TEXT_JSON
						Duration(((SaveInstruction.Duration)instruction).Value);
#endif
						break;
					case SaveInstruction.Remove.Id:
#if MODIBUFF_SYSTEM_TEXT_JSON
						var remove = (SaveInstruction.Remove)instruction;
						switch (remove.RemoveType)
						{
							case SaveInstruction.Remove.Type.RemoveOn:
								Remove(remove.EffectOn.ToRemoveEffectOn());
								break;
							case SaveInstruction.Remove.Type.Duration:
								Remove(remove.Duration);
								break;
							default:
								Logger.LogError(
									$"[ModiBuff] Loaded remove instruction with unknown type {remove.RemoveType}");
								break;
						}
#endif
						break;
					case SaveInstruction.Refresh.Id:
#if MODIBUFF_SYSTEM_TEXT_JSON
						Refresh(((SaveInstruction.Refresh)instruction).RefreshType);
#endif
						break;
					case SaveInstruction.Stack.Id:
#if MODIBUFF_SYSTEM_TEXT_JSON
						var stack = (SaveInstruction.Stack)instruction;
						Stack(stack.WhenStackEffect, stack.MaxStacks, stack.EveryXStacks, stack.SingleStackTime,
							stack.IndependentStackTime);
#endif
						break;
					case SaveInstruction.Dispel.Id:
#if MODIBUFF_SYSTEM_TEXT_JSON
						Dispel(((SaveInstruction.Dispel)instruction).Type);
#endif
						break;
					case SaveInstruction.Tag.Id:
#if MODIBUFF_SYSTEM_TEXT_JSON
						var tag = (SaveInstruction.Tag)instruction;
						_ = tag.InstructionTagType switch
						{
							SaveInstruction.Tag.Type.Add => Tag(tag.TagType),
							SaveInstruction.Tag.Type.Remove => RemoveTag(tag.TagType),
							SaveInstruction.Tag.Type.Set => SetTag(tag.TagType),
							_ => throw new ArgumentOutOfRangeException()
						};
#endif
						break;
					case SaveInstruction.CallbackUnit.Id:
#if MODIBUFF_SYSTEM_TEXT_JSON
						CallbackUnit(
							(TUnitCallback)(object)((SaveInstruction.CallbackUnit)instruction).CallbackUnitType);
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
						var action = (SaveInstruction.ModifierAction)instruction;
						ModifierAction(action.ModifierActionFlags, action.EffectOn);
#endif
						break;
					default:
						Logger.LogError($"Unknown instruction with id {instruction.InstructionId}");
						break;
				}
			}

			(IEffect, EffectOn) HandleEffect(SaveInstruction instruction)
			{
				var effect = (SaveInstruction.Effect)instruction;
				bool failed = false;

				int effectId = effect.EffectId;
				EffectOn effectOn = effect.EffectOn;

				var effectType = _effectTypeIdManager.GetEffectType(effectId);
				//TODO Find constructor by saved types? Prob not worth
				var constructor = effectType.GetConstructors()[0];

				var parameters = constructor.GetParameters();
				object[] effectStates = new object[parameters.Length];
				int i = 0;
				foreach (var property in ((System.Text.Json.JsonElement)effect.SaveData).EnumerateObject())
				{
					object value = property.Value.ToValue(parameters[i].ParameterType);
					if (value == null)
					{
						Logger.LogError(
							$"[ModiBuff] Failed to load effect state from save data by {Name} for effect {effectId}");
						failed = true;
					}

					effectStates[i] = value;
					i++;
				}

				if (failed)
					return (null, EffectOn.None);

				return ((IEffect)constructor.Invoke(effectStates), effectOn);
			}
		}

		[System.Text.Json.Serialization.JsonPolymorphic]
		[System.Text.Json.Serialization.JsonDerivedType(typeof(Initialize), Initialize.Id)]
		[System.Text.Json.Serialization.JsonDerivedType(typeof(Interval), Interval.Id)]
		[System.Text.Json.Serialization.JsonDerivedType(typeof(Duration), Duration.Id)]
		[System.Text.Json.Serialization.JsonDerivedType(typeof(Remove), Remove.Id)]
		[System.Text.Json.Serialization.JsonDerivedType(typeof(Refresh), Refresh.Id)]
		[System.Text.Json.Serialization.JsonDerivedType(typeof(Stack), Stack.Id)]
		[System.Text.Json.Serialization.JsonDerivedType(typeof(Dispel), Dispel.Id)]
		[System.Text.Json.Serialization.JsonDerivedType(typeof(Tag), Tag.Id)]
		[System.Text.Json.Serialization.JsonDerivedType(typeof(CallbackUnit), CallbackUnit.Id)]
		[System.Text.Json.Serialization.JsonDerivedType(typeof(Effect), Effect.Id)]
		[System.Text.Json.Serialization.JsonDerivedType(typeof(ModifierAction), ModifierAction.Id)]
		public record SaveInstruction
		{
			public readonly int InstructionId;

			private const int BaseId = 0;

#if MODIBUFF_SYSTEM_TEXT_JSON
			[System.Text.Json.Serialization.JsonConstructor]
#endif
			public SaveInstruction(int instructionId)
			{
				InstructionId = instructionId;
			}

			public record Initialize : SaveInstruction
			{
				public const int Id = BaseId;

				public readonly string Name;
				public readonly string DisplayName;
				public readonly string Description;

#if MODIBUFF_SYSTEM_TEXT_JSON
				[System.Text.Json.Serialization.JsonConstructor]
#endif
				public Initialize(string name, string displayName, string description) : base(Id)
				{
					Name = name;
					DisplayName = displayName;
					Description = description;
				}
			}

			public sealed record Interval : SaveInstruction
			{
				public const int Id = Initialize.Id + 1;

				public readonly float Value;

#if MODIBUFF_SYSTEM_TEXT_JSON
				[System.Text.Json.Serialization.JsonConstructor]
#endif
				public Interval(float value) : base(Id)
				{
					Value = value;
				}
			}

			public sealed record Duration : SaveInstruction
			{
				public const int Id = Interval.Id + 1;

				public readonly float Value;

#if MODIBUFF_SYSTEM_TEXT_JSON
				[System.Text.Json.Serialization.JsonConstructor]
#endif
				public Duration(float value) : base(Id)
				{
					Value = value;
				}
			}

			public sealed record Remove : SaveInstruction
			{
				public const int Id = SaveInstruction.Duration.Id + 1;

				public readonly Type RemoveType;
				public readonly EffectOn EffectOn;
				public readonly float Duration;

				public enum Type
				{
					RemoveOn,
					Duration,
				}

#if MODIBUFF_SYSTEM_TEXT_JSON
				[System.Text.Json.Serialization.JsonConstructor]
#endif
				public Remove(Type removeType, EffectOn effectOn, float duration = 0) : base(Id)
				{
					RemoveType = removeType;
					EffectOn = effectOn;
					Duration = duration;
				}
			}

			public sealed record Refresh : SaveInstruction
			{
				public const int Id = Remove.Id + 1;

				public readonly RefreshType RefreshType;

#if MODIBUFF_SYSTEM_TEXT_JSON
				[System.Text.Json.Serialization.JsonConstructor]
#endif
				public Refresh(RefreshType refreshType) : base(Id)
				{
					RefreshType = refreshType;
				}
			}

			public sealed record Stack : SaveInstruction
			{
				public const int Id = Refresh.Id + 1;

				public readonly WhenStackEffect WhenStackEffect;
				public readonly int MaxStacks;
				public readonly int EveryXStacks;
				public readonly float SingleStackTime;
				public readonly float IndependentStackTime;

#if MODIBUFF_SYSTEM_TEXT_JSON
				[System.Text.Json.Serialization.JsonConstructor]
#endif
				public Stack(WhenStackEffect whenStackEffect, int maxStacks, int everyXStacks, float singleStackTime,
					float independentStackTime) : base(Id)
				{
					WhenStackEffect = whenStackEffect;
					MaxStacks = maxStacks;
					EveryXStacks = everyXStacks;
					SingleStackTime = singleStackTime;
					IndependentStackTime = independentStackTime;
				}
			}

			public sealed record Dispel : SaveInstruction
			{
				public const int Id = Stack.Id + 1;

				public readonly DispelType Type;

#if MODIBUFF_SYSTEM_TEXT_JSON
				[System.Text.Json.Serialization.JsonConstructor]
#endif
				public Dispel(DispelType type) : base(Id) => Type = type;
			}

			public sealed record Tag : SaveInstruction
			{
				public const int Id = Dispel.Id + 1;

				public enum Type
				{
					Add,
					Remove,
					Set,
				}

				public readonly Type InstructionTagType;
				public readonly TagType TagType;

#if MODIBUFF_SYSTEM_TEXT_JSON
				[System.Text.Json.Serialization.JsonConstructor]
#endif
				public Tag(Type instructionTagType, TagType tagType) : base(Id)
				{
					InstructionTagType = instructionTagType;
					TagType = tagType;
				}
			}

			public sealed record CallbackUnit : SaveInstruction
			{
				public const int Id = Tag.Id + 1;

				public readonly int CallbackUnitType;

#if MODIBUFF_SYSTEM_TEXT_JSON
				[System.Text.Json.Serialization.JsonConstructor]
#endif
				public CallbackUnit(int callbackUnitType) : base(Id)
				{
					CallbackUnitType = callbackUnitType;
				}
			}

			public sealed record Effect : SaveInstruction
			{
				public const int Id = CallbackUnit.Id + 1;

				public readonly int EffectId;
				public readonly object SaveData;
				public readonly EffectOn EffectOn;

#if MODIBUFF_SYSTEM_TEXT_JSON
				[System.Text.Json.Serialization.JsonConstructor]
#endif
				public Effect(int effectId, object saveData, EffectOn effectOn) : base(Id)
				{
					EffectId = effectId;
					SaveData = saveData;
					EffectOn = effectOn;
				}
			}

			public sealed record ModifierAction : SaveInstruction
			{
				public const int Id = Effect.Id + 1;

				public readonly ModiBuff.Core.ModifierAction ModifierActionFlags;
				public readonly EffectOn EffectOn;

#if MODIBUFF_SYSTEM_TEXT_JSON
				[System.Text.Json.Serialization.JsonConstructor]
#endif
				public ModifierAction(ModiBuff.Core.ModifierAction modifierActionFlags, EffectOn effectOn) : base(Id)
				{
					ModifierActionFlags = modifierActionFlags;
					EffectOn = effectOn;
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