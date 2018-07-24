using System.IO;
using System.Runtime.InteropServices;
using Genyman.Cli.Helpers;
using Genyman.Core;
using Genyman.Core.Commands;
using Genyman.Core.Helpers;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.DotNet.Configurer;

namespace Genyman.Cli.Implementation
{
	public class Generator : GenymanGenerator<Configuration>
	{
		internal static string[] Args { get; set; }

		public override void Execute()
		{
			if (ConfigurationMetadata.PackageId == Metadata.PackageId)
			{
				// create NEW Genyman configuration file
				Log.Information($"Generating a new genyman generator solution");
				ProcessHandlebarTemplates();
			}
			else
			{
				var packageId = ConfigurationMetadata.PackageId;
				var resolvePackageResult = DotNetHelper.ResolvePackage(packageId, ConfigurationMetadata.NugetSource, Update, ConfigurationMetadata.Version);

				if (resolvePackageResult.success)
				{
					var generator = new PackageGenerator();
					generator.InputFileName = InputFileName;
					generator.PackageId = resolvePackageResult.packageId;
					generator.Execute(Args);
				}
			}
		}

		public class PackageGenerator : BaseCommand
		{
			protected override int Execute()
			{
				var program = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
					? Path.Combine(CliFolderPathCalculator.ToolsShimPath, PackageId)
					: PackageId;

				var run = ProcessRunner.Create(program)
					.IsGenerator()
					.WithArgument(InputFileName);

				foreach (var args in RemainingArguments)
					if (args.StartsWith("--"))
						run.WithArgument(args);

				run.Execute(false);

				return 0;
			}

			public string InputFileName { get; set; }
			public string PackageId { get; set; }
		}
	}
}