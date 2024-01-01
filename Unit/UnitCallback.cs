namespace ModiBuff.Core
{
	//TODO any reason for this to be supplied by the user? Makes it easier with casting for needed behaviour
	//But not every IUnit might be of the same type, a source IUnit might be of type X, while the target IUnit might be of type Y
	public delegate void UnitCallback(IUnit target, IUnit source);

	public delegate void UnitCallback<in TUnit>(TUnit target, TUnit source);
}