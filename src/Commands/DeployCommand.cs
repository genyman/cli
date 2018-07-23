using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Genyman.Cli.Helpers;
using Genyman.Core;
using Genyman.Core.Commands;
using Genyman.Core.Helpers;
using McMaster.Extensions.CommandLineUtils;
using ServiceStack;

namespace Genyman.Cli.Commands
{
	public class DeployCommand : BaseCommand
	{
		public DeployCommand()
		{
			Name = "deploy";
			Description = "Deploys the generator";
			SourceOption = Option<string>("--source", "Deploys to custom nuget server", CommandOptionType.SingleValue, option => { }, false);
			ApiKeyOption = Option<string>("--apikey", "Use ApiKey to deploy to nuget server", CommandOptionType.SingleValue, option => { }, false);
		}

		public CommandOption<string> SourceOption { get; }
		public CommandOption<string> ApiKeyOption { get; }

		protected override int Execute()
		{
			base.Execute();

			// Creating temporary folder
			var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			Log.Debug($"Creating temp folder {tempFolder}");
			Directory.CreateDirectory(tempFolder);

			Log.Information($"Packing generator...");
			DotNetHelper.Pack(tempFolder);

			var nupkg = Directory.GetFiles(tempFolder, "*.nupkg").FirstOrDefault();
			var nupkgFile = new FileInfo(nupkg).Name;
			Log.Information($"Pushing generator {nupkgFile}");

			var source = string.Empty;
			var apiKey = string.Empty;

			if (SourceOption.HasValue())
			{
				if (string.IsNullOrEmpty(SourceOption.Value()))
					Log.Fatal("When specifying --source your need to add a valid source (--source https://{YourUrl})");
				source = SourceOption.ParsedValue;
			}

			if (ApiKeyOption.HasValue())
			{
				if (string.IsNullOrEmpty(ApiKeyOption.Value()))
					Log.Fatal("When specifying --apikey your need to add a valid api key (--apikey YourKey");
				apiKey = ApiKeyOption.ParsedValue;
			}

			var nugetPush = DotNetHelper.NugetPush(nupkg, source, apiKey);

			if (nugetPush == 0)
			{
				Log.Information("Package was pushed");
				DotNetHelper.InstallOrUpdateLocal(nupkgFile, tempFolder);
				Log.Information("Genyman generator was installed locally");
			}
			else
			{
				Log.Information("Skipping local installation of Genyman generator - Nuget Failed");
			}

			// Cleanup

			var files = Directory.GetFiles(tempFolder);
			foreach (var file in files)
			{
				Log.Debug($"Cleanup. Deleting file {file}");
				File.Delete(file);
			}

			Log.Debug($"Cleanup. Deleting folder {tempFolder}");
			Directory.Delete(tempFolder);


			return 0;
		}
	}
}