using System.Collections.Generic;
using ModiBuff.Core;

namespace ModiBuff.Examples.SimpleSolo
{
	public sealed class TargetingSystem
	{
		public IUnit AttackTarget { get; private set; }
		public IUnit CastTarget { get; private set; } //TODO Enemy and friendly

		private readonly List<IUnit> _closeTargets;

		public TargetingSystem()
		{
			_closeTargets = new List<IUnit>();
		}

		public void SetAttackTarget(IUnit target) => AttackTarget = target;
		public void SetCastTarget(IUnit target) => CastTarget = target;

		public void AddCloseTargets(params IUnit[] target) => _closeTargets.AddRange(target);

		public IReadOnlyList<IUnit> GetCloseTargets() => _closeTargets;
	}
}