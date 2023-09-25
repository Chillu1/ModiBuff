namespace ModiBuff.Core
{
	public interface IModifier : IStateReset
	{
		int Id { get; }
		int GenId { get; }
		string Name { get; }

		void Init();
		void Update(float deltaTime);
		void Refresh();
		void Stack();
	}
}