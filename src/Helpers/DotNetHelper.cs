using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Genyman.Core;
using Genyman.Core.Helpers;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.DotNet.Configurer;
using ServiceStack;

namespace Genyman.Cli.Helpers
{
	internal static class DotNetHelper
	{
		static string DotnetCommand { get; }
		static string DotnetStore { get; }

		static DotNetHelper()
		{
			DotnetCommand = DotNetExe.FullPathOrDefault();
		}

		internal static void Pack(string tempFolder)
		{
			ProcessRunner.Create(DotnetCommand)
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
		}

		internal static void NugetPush(string nugetPackage, string nugetSource = null, string nugetApiKey = null)
		{
			var push = ProcessRunner.Create(DotnetCommand)
				.WithArgument("nuget")
				.WithArgument("push")
				.WithArgument(nugetPackage);

			if (!string.IsNullOrEmpty(nugetSource))
			{
				push.WithArgument("--source", nugetSource);
			}
			else
			{
				push.WithArgument("--source", "https://api.nuget.org/v3/index.json");
			}

			if (!string.IsNullOrEmpty(nugetApiKey))
			{
				push.WithArgument("--api-key", nugetApiKey);
			}

			push.ReceiveOutput(s =>
			{
				if (s.Contains("Your package was pushed")) //todo: is this OK? what about localization?
					Log.Information("Push successfull");
				return true;
			});
			push.Execute();
		}

		internal static void InstallOrUpdateLocal(string nupkgFile, string tempFolder)
		{
			var packageId = GetPackageId(nupkgFile);
			var version = GetPackageVersion(nupkgFile);
			
			if (DoesPackageExists(packageId))
			{
				ProcessRunner.Create(DotnetCommand)
					.WithArgument("tool")
					.WithArgument("update")
					.WithArgument("-g")
					.WithArgument("--add-source", tempFolder)
					.WithArgument(packageId)
					.ReceiveOutput(s =>
					{
						Log.Information(s);
						return true;
					})
					.Execute();
			}
			else
			{
				// if not- install with specific version
				// dotnet tool install -g --add-source /Users/stefan/Sources/Github/cbl/Genyman.IOSDeviceIdentifiers/src/bin/Debug CaveBirdLabs.Genyman.IOSDeviceIdentifiers --version 0.0.1
				ProcessRunner.Create(DotnetCommand)
					.WithArgument("tool")
					.WithArgument("install")
					.WithArgument("-g")
					.WithArgument("--add-source", tempFolder)
					.WithArgument(packageId)
					.WithArgument("--version", version)
					.ReceiveOutput(s =>
					{
						Log.Information(s);
						return true;
					})
					.Execute();
			}
		}

		static string GetPackageId(string nupkgFile)
		{
			return string.Join(".", Path.GetFileNameWithoutExtension(nupkgFile).Split('.').Take(3));
		}

		static string GetPackageVersion(string nupkgFile)
		{
			return string.Join(".", Path.GetFileNameWithoutExtension(nupkgFile).Split('.').TakeLast(3));
		}


		static bool DoesPackageExists(string packageId)
		{
			var folder = CliFolderPathCalculator.ToolsPackagePath;
			var packageFolders = Directory.EnumerateDirectories(CliFolderPathCalculator.ToolsPackagePath);
			
			var foundPackage = packageFolders.FirstOrDefault(f => f.ToLower().EndsWith(packageId.ToLower()));
			return foundPackage != null;
		}
	}
}