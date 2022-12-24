namespace ModiBuff.Core
{
	public interface IModifier : IStateReset
	{
		int Id { get; }
		string Name { get; }

		//TODO Temp
		void SetTargets(IUnit target, IUnit owner, IUnit sender);

		void Init();
		void Update(float deltaTime);
		void Refresh();
		void Stack();
	}
}