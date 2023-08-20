using Godot;
using ModiBuff.Core;

namespace ModiBuff.Extensions.Godot
{
	[GlobalClass]
	public partial class ApplyCostResource : Resource
	{
		/// <summary>
		///		Type that should be used for the cost (ex. mana, health, etc).
		/// </summary>
		[Export]
		public CostType CostType { get; set; }

		[Export] public float Amount { get; set; }
	}
}