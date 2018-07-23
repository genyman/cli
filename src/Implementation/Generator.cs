using System.IO;
using Genyman.Cli.Helpers;
using Genyman.Core;
using Genyman.Core.Commands;
using Genyman.Core.Helpers;
using McMaster.Extensions.CommandLineUtils;

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
				var resolvePackageResult = DotNetHelper.ResolvePackage(packageId, ConfigurationMetadata.NugetSource, false, ConfigurationMetadata.Version);

				if (resolvePackageResult.success)
				{
					var generator = new PackageGenerator();
					generator.InputFileName = InputFileName;
					generator.PackageId = resolvePackageResult.packageId;
					generator.Execute(Args);

					// in case a specific version was requested, install latest again
					if (!string.IsNullOrEmpty(ConfigurationMetadata.Version))
					{
						DotNetHelper.ResolvePackage(packageId, ConfigurationMetadata.NugetSource, true, null);
					}
				}
			}
		}

		public class PackageGenerator : BaseCommand
		{
			protected override int Execute()
			{
				var program = PackageId;

				var run = ProcessRunner.Create(program)
					.IsGenerator()
					.WithArgument(InputFileName);

				foreach (var option in Options)
					if (option.HasValue())
						run.WithArgument("--" + option.LongName, option.Value());

				run.Execute(false);

				return 0;
			}

			public string InputFileName { get; set; }
			public string PackageId { get; set; }
		}
	}
}