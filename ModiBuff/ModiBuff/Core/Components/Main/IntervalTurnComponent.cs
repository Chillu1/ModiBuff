namespace ModiBuff.Core
{
	public sealed class IntervalTurnComponent : ITurnTimeComponent
	{
		private readonly int _interval;
		private readonly bool _isRefreshable;
		private int _turnCount;

		public void UpdateTargetStatusResistance()
		{
		}

		public void SetupTarget(ITargetComponent targetComponent)
		{
		}

		public void UpdateTurn(int count = 1)
		{
			_turnCount += count;

			if (_turnCount < _interval)
				return;

			_turnCount -= _interval;

			//Effect
		}

		public void ResetState()
		{
		}

		public void Refresh()
		{
		}
	}
}