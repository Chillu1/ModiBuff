namespace ModiBuff.Core.Units
{
	public interface IMovable<in TPosition>
	{
		void Move(TPosition position);
	}

	// public interface IMovable<in TX, in TY>
	// {
	// 	void Move(TX x, TY y);
	// }
	//    
	// public interface IMovable<in TX, in TY, in TZ>
	// {
	// 	void Move(TX x, TY y, TZ z);
	// }
}