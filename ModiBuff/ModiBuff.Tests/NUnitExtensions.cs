using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public static class AssertExtensions
	{
		public static void AreEqual(Vector2 expected, Vector2 actual, float delta = 0.001f)
		{
			Assert.AreEqual(expected.X, actual.X, delta);
			Assert.AreEqual(expected.Y, actual.Y, delta);
		}
	}
}