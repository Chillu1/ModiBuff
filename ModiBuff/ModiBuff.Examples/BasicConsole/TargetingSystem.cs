using ModiBuff.Core;

namespace ModiBuff.Examples.BasicConsole
{
	public sealed class TargetingSystem
	{
		public IUnit AttackTarget { get; private set; }

		public void SetAttackTarget(IUnit target) => AttackTarget = target;
	}
}