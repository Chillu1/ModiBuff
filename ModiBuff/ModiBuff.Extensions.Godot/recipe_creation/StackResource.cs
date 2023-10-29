using Godot;
using ModiBuff.Core;

namespace ModiBuff.Extensions.Godot
{
	[GlobalClass]
	public sealed partial class StackResource : Resource
	{
		/// <summary>
		///		When should the stack effects be triggered.
		/// </summary>
		[Export]
		public WhenStackEffect WhenStackEffect { get; set; }

		/// <summary>
		///		Max amount of stacks that can be applied.
		/// </summary>
		[Export]
		public int MaxStacks { get; set; } = -1;

		/// <summary>
		///		If <see cref="WhenStackEffect"/> is set to <see cref="WhenStackEffect.EveryXStacks"/>, this value will be used to determine when the stack effects should be triggered.
		/// </summary>
		[Export]
		public int EveryXStacks { get; set; } = -1;
	}
}