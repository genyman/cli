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

			var isFullPackageId = true;

			if (!packageId.ToLower().Contains(".genyman."))
			{
				isFullPackageId = false;
				packageId = ".genyman." + packageId;
			}

			var local = DotNetHelper.DoesPackageExists(packageId); // does check upon ENDING
			var canContinue = false;

			if (!local)
			{
				if (!isFullPackageId)
				{
					Log.Error($"Genyman package {packageId} is not installed. Auto-installation cannot be performed as {packageId} is not a fully qualified package Id.");
					return -1;
				}

				canContinue = DotNetHelper.Install(packageId, SourceOption.ParsedValue);
			}
			else
			{
				// perform update, we need full package name
				var latest = DotNetHelper.GetLastestPackageVersion(packageId);
				packageId = latest.packageId; // always get full packageId here

				canContinue = latest.success;
				if (canContinue && isFullPackageId && UpdateOption.HasValue()) DotNetHelper.Update(packageId, SourceOption.ParsedValue);
			}

			if (canContinue)
			{
				var program = packageId;

				var run = ProcessRunner.Create(program)
					.WithArgument("new");

				foreach (var option in Options)
					if (option.HasValue())
						run.WithArgument("--" + option.LongName, option.Value());


				return run.Execute();
			}

			Log.Error("Could not execute new command for this packageId.");
			return -1;
		}
	}
}