using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
			SourceOption = Option("--source", "Deploys to custom nuget server", CommandOptionType.SingleOrNoValue, option =>
			{	
			}, false);
			SourceOption = Option("--apikey", "Use ApiKey to deploy to nuget server", CommandOptionType.SingleOrNoValue, option =>
			{	
			}, false);
		}
		
		public CommandOption SourceOption { get; }
		public CommandOption ApiKeyOption { get; }
		

		protected override int Execute()
		{
			base.Execute();
			
			// Finding dotnet cli
			
			var dotnet = McMaster.Extensions.CommandLineUtils.DotNetExe.FullPath;
			if (dotnet.IsNullOrEmpty())
			{
				Log.Fatal($"Could not find dotnet cli");
				return -1;
			}		
			
			// Creating temporary folder
			var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			Log.Debug($"Creating temp folder {tempFolder}");
			Directory.CreateDirectory(tempFolder);

			
			// Execute pack command
			// 
			
			Log.Information($"Packing generator...");
			
			ProcessRunner.Create(dotnet)
				.WithArgument("pack")
				.WithArgument("-c", "release")
				.WithArgument("-o", tempFolder)
				.ReceiveOutput(s =>
				{
					if (s.Contains("Successfully"))
					
						Log.Information($"Packing was successfull");
					
					return true;
				})
				.Execute();
			
			var nupkg = Directory.GetFiles(tempFolder, "*.nupkg").FirstOrDefault();
			var nupkgFile = new FileInfo(nupkg).Name;
			Log.Information($"Pushing generator {nupkgFile}");

			var push = ProcessRunner.Create(dotnet)
				.WithArgument("nuget")
				.WithArgument("push")
				.WithArgument(nupkg);

			if (SourceOption.HasValue())
			{
				if (string.IsNullOrEmpty(SourceOption.Value()))
				{
					Log.Fatal("When specifying --source your need to add a valid source (--source=https://{YourUrl})");
				}
				push.WithArgument("--source", SourceOption.Value());
			}
			
			if (ApiKeyOption.HasValue())
			{
				if (string.IsNullOrEmpty(ApiKeyOption.Value()))
				{
					Log.Fatal("When specifying --apikey your need to add a valid api key (--apikey=YourKey");
				}
				push.WithArgument("--api-key", ApiKeyOption.Value());
			}

			push.ReceiveOutput(s =>
			{
				if (s.Contains("Your package was pushed"))
					Log.Information("Push successfull");
				return true;
			});
			push.Execute();
			
			
			
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