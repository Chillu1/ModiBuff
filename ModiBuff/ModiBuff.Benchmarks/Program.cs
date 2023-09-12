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
			//Normal
			config.AddJob(Job.MediumRun.WithLaunchCount(1));

			config.AddLogger(DefaultConfig.Instance.GetLoggers().ToArray());
			config.AddExporter(DefaultConfig.Instance.GetExporters().ToArray());
			config.AddDiagnoser(DefaultConfig.Instance.GetDiagnosers().ToArray());
			config.AddColumnProvider(DefaultConfig.Instance.GetColumnProviders().ToArray());
			config.AddAnalyser(DefaultConfig.Instance.GetAnalysers().ToArray());
			config.AddValidator(DefaultConfig.Instance.GetValidators().ToArray());
			config.AddHardwareCounters(DefaultConfig.Instance.GetHardwareCounters().ToArray());
			config.AddFilter(DefaultConfig.Instance.GetFilters().ToArray());*/
			BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

			//BenchmarkRunner.Run<BenchInitialization>(config);
		}
	}
}