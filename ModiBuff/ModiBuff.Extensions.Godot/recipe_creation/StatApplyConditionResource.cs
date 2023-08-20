using Godot;
using ModiBuff.Core;

namespace ModiBuff.Extensions.Godot
{
	[GlobalClass]
	public partial class StatApplyConditionResource : Resource
	{
		/// <summary>
		///		Stat type to be compared
		/// </summary>
		[Export]
		public StatType StatType { get; set; }

		[Export] public float Value { get; set; }

		[Export] public ComparisonType ComparisonType { get; set; } = ComparisonType.GreaterOrEqual;
	}
}