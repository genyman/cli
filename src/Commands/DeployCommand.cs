using System;
using System.IO;
using System.Linq;
using Genyman.Core;
using Genyman.Core.Commands;
using Genyman.Core.Helpers;
using Genyman.Core.MSBuild;
using McMaster.Extensions.CommandLineUtils;

namespace Genyman.Cli.Commands
{
	internal class DeployCommand : BaseCommand
	{
		public DeployCommand()
		{
			Name = "deploy";
			Description = "Deploys the generator";
			SourceOption = Option<string>("--source", "Deploys to custom nuget server", CommandOptionType.SingleValue, option => { }, false);
			ApiKeyOption = Option<string>("--apikey", "Use ApiKey to deploy to nuget server", CommandOptionType.SingleValue, option => { }, false);
			
			MajorOption = Option("--major", "Increase Major part of version", CommandOptionType.NoValue, option => { }, false);
			MinorOption = Option("--minor", "Increase Minor part of version", CommandOptionType.NoValue, option => { }, false);
			BuildOption = Option("--build", "Increase Build part of version (default)", CommandOptionType.NoValue, option => { }, false);

		}

		public CommandOption<string> SourceOption { get; }
		public CommandOption<string> ApiKeyOption { get; }
		
		public CommandOption MajorOption { get; }
		public CommandOption MinorOption { get; }
		public CommandOption BuildOption { get; }


		protected override int Execute()
		{
			base.Execute();
			
			// find csproj
			var csproj = Directory.GetFiles(WorkingDirectory, "*.csproj", SearchOption.AllDirectories).FirstOrDefault();
			if (csproj == null)
			{
				Log.Fatal($"Could not find .csproj in underlying folders of {WorkingDirectory}");
			}

			// increase
			var addBuild = !(!BuildOption.HasValue() && (MajorOption.HasValue() || MinorOption.HasValue()));
			FluentMSBuild.Use(csproj).IncrementVersion(MajorOption.HasValue(), MinorOption.HasValue(), addBuild);
			
			// Creating temporary folder
			var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			Log.Debug($"Creating temp folder {tempFolder}");
			Directory.CreateDirectory(tempFolder);

			Log.Information("Packing generator...");
			DotNetRunner.Pack(tempFolder);

			var nupkg = Directory.GetFiles(tempFolder, "*.nupkg").FirstOrDefault();
			var nupkgFile = new FileInfo(nupkg).Name;
			Log.Information($"Pushing generator {nupkgFile}");

			var source = string.Empty;
			var apiKey = string.Empty;

			if (SourceOption.HasValue())
			{
				source = SourceOption.ParsedValue.FromEnvironmentOrDefault();
				if (string.IsNullOrEmpty(source))
					Log.Fatal("When specifying --source your need to add a valid source (--source https://{YourUrl})");
			}

			if (ApiKeyOption.HasValue())
			{
				apiKey = ApiKeyOption.ParsedValue.FromEnvironmentOrDefault();
				if (string.IsNullOrEmpty(apiKey))
					Log.Fatal("When specifying --apikey your need to add a valid api key (--apikey YourKey");
			}

			var nugetPush = DotNetRunner.NugetPush(nupkg, source, apiKey);

			if (nugetPush == 0)
			{
				Log.Information("Package was pushed");
				DotNetRunner.InstallOrUpdateLocal(nupkgFile, tempFolder);
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