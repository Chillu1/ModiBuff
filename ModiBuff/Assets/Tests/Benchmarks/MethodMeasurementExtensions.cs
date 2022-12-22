using Unity.PerformanceTesting.Measurements;

namespace ModiBuff.Tests
{
	public static class MethodMeasurementExtensions
	{
		public static MethodMeasurement Bench(this MethodMeasurement measurement, int iterations = 5000)
		{
			measurement.WarmupCount(10)
				.MeasurementCount(50)
				.IterationsPerMeasurement(iterations)
				.Run();
			return measurement;
		}

		public static MethodMeasurement BenchGC(this MethodMeasurement measurement, int iterations = 5000)
		{
			measurement.WarmupCount(10)
				.MeasurementCount(50)
				.IterationsPerMeasurement(iterations)
				.GC()
				.Run();
			return measurement;
		}
	}
}