using Genyman.Cli.Helpers;
using Genyman.Core;
using Genyman.Core.Commands;
using Genyman.Core.Helpers;
using McMaster.Extensions.CommandLineUtils;

namespace Genyman.Cli.Commands
{
	internal class NewPackageIdCommand : NewCommand
	{
		protected NewPackageIdCommand() : base(true)
		{
		}

		public static int Run(string[] args)
		{
			var command = new CommandLineApplication();
			command.Commands.Add(new NewPackageIdCommand());
			return command.Execute(args);
		}

		protected override int Execute()
		{
			var packageId = PackageIdArgument.ParsedValue;
			var resolvePackageResult = DotNetHelper.ResolvePackage(packageId, SourceOption.ParsedValue, UpdateOption.HasValue(), string.Empty);

			if (resolvePackageResult.success)
			{
				var program = resolvePackageResult.packageId;

				var run = ProcessRunner.Create(program)
					.IsGenerator()
					.WithArgument("new");

				foreach (var option in Options)
					if (option.HasValue())
						run.WithArgument("--" + option.LongName, option.Value());

				return run.Execute(false);
			}

			Log.Error("Could not execute new command for this packageId.");
			return -1;
		}

		
	}
}