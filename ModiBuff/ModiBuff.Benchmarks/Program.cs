using System.Linq;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace ModiBuff.Tests
{
	public class Program
	{
		public static void Main(string[] args)
		{
			/*var config = new ManualConfig();
			//Single
			//config.AddJob(Job.MediumRun.WithUnrollFactor(1).WithInvocationCount(1));
			//Fast
			//config.AddJob(Job.ShortRun.WithInvocationCount(131072).WithIterationCount(10));
			//Normal
			config.AddJob(Job.ShortRun.WithLaunchCount(1).WithIterationCount(15));

			config.AddLogger(DefaultConfig.Instance.GetLoggers().ToArray());
			config.AddExporter(DefaultConfig.Instance.GetExporters().ToArray());
			config.AddDiagnoser(DefaultConfig.Instance.GetDiagnosers().ToArray());
			config.AddColumnProvider(DefaultConfig.Instance.GetColumnProviders().ToArray());
			config.AddAnalyser(DefaultConfig.Instance.GetAnalysers().ToArray());
			config.AddValidator(DefaultConfig.Instance.GetValidators().ToArray());
			config.AddHardwareCounters(DefaultConfig.Instance.GetHardwareCounters().ToArray());
			config.AddFilter(DefaultConfig.Instance.GetFilters().ToArray());*/
			BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

			//BenchmarkRunner.Run<BenchTempTests>(/*config*/);
		}
	}
}