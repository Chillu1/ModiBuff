using System;

namespace ModiBuff.Core
{
	public sealed class RevertActionEffect : IRevertEffect
	{
		public bool IsRevertible => true;
		private readonly Action _action;

		public RevertActionEffect(Action action) => _action = action;

		public void RevertEffect(IUnit target, IUnit source) => _action.Invoke();
	}
}