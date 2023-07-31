using BenchmarkDotNet.Running;

namespace ModiBuff.Tests
{
	public class Program
	{
		public static void Main(string[] args)
		{
			//var summary = BenchmarkRunner.Run<BenchAddModifier>();
			//Run all benchmarks in the assembly
			BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
		}
	}
}