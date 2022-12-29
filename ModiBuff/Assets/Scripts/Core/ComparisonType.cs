using System;

namespace ModiBuff.Core
{
	[Flags]
	public enum ComparisonType
	{
		None,
		Greater = 1,
		Equal = 2,
		Less = 4,

		GreaterOrEqual = Greater | Equal,
		LessOrEqual = Less | Equal,
	}
}