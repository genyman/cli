using System;
using System.IO;
using System.Linq;
using Genyman.Core;
using Genyman.Core.Helpers;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.DotNet.Configurer;

namespace Genyman.Cli.Helpers
{
	internal static class DotNetHelper
	{
		static DotNetHelper()
		{
			DotnetCommand = DotNetExe.FullPathOrDefault();
		}

		static string DotnetCommand { get; }

		internal static void Pack(string tempFolder)
		{
			ProcessRunner.Create(DotnetCommand)
				.WithArgument("pack")
				.WithArgument("-c", "release")
				.WithArgument("-o", tempFolder)
				.ReceiveOutput(s =>
				{
					if (s.Contains("Successfully"))

						Log.Information("Packing was successfull");

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
				push.WithArgument("--source", nugetSource);
			else
				push.WithArgument("--source", "https://api.nuget.org/v3/index.json");

			if (!string.IsNullOrEmpty(nugetApiKey)) push.WithArgument("--api-key", nugetApiKey);

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
			else
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

		internal static bool Install(string packageId, string source)
		{
			var install = ProcessRunner.Create(DotnetCommand)
				.WithArgument("tool")
				.WithArgument("install")
				.WithArgument("-g")
				.WithArgument(packageId)
				.ReceiveOutput(s =>
				{
					Log.Information(s);
					return true;
				});

			if (!string.IsNullOrEmpty(source))
				install.WithArgument("--add-source", source);

			var exitCode = install.Execute();
			return exitCode == 0;
		}

		internal static bool Update(string packageId, string source)
		{
			var update = ProcessRunner.Create(DotnetCommand)
				.WithArgument("tool")
				.WithArgument("update")
				.WithArgument("-g")
				.WithArgument(packageId)
				.ReceiveOutput(s =>
				{
					Log.Information(s);
					return true;
				});

			if (!string.IsNullOrEmpty(source))
				update.WithArgument("--add-source", source);

			var exitCode = update.Execute();
			return exitCode == 0;
		}

		internal static string GetPackageId(string nupkgFile)
		{
			return string.Join(".", Path.GetFileNameWithoutExtension(nupkgFile).Split('.').Take(3));
		}

		internal static string GetPackageVersion(string nupkgFile)
		{
			return string.Join(".", Path.GetFileNameWithoutExtension(nupkgFile).Split('.').TakeLast(3));
		}

		internal static bool DoesPackageExists(string packageId)
		{
			var packageFolders = Directory.EnumerateDirectories(CliFolderPathCalculator.ToolsPackagePath);
			// check upon ending - if packageId is not complete
			var foundPackage = packageFolders.FirstOrDefault(f => f.ToLower().EndsWith(packageId.ToLower()));
			return foundPackage != null;
		}

		internal static (bool success, string packageId, string version) GetLastestPackageVersion(string packageId)
		{
			var packageFolders = Directory.EnumerateDirectories(CliFolderPathCalculator.ToolsPackagePath);
			var foundPackage = packageFolders.FirstOrDefault(f => f.ToLower().EndsWith(packageId.ToLower()));
			var subFolders = Directory.EnumerateDirectories(foundPackage, "*.*", SearchOption.TopDirectoryOnly);

			var foundPackageId = new DirectoryInfo(foundPackage).Name;
			var highestVersion = "0.0.0";
			var success = true;

			foreach (var subFolder in subFolders)
				try
				{
					var version = new DirectoryInfo(subFolder);
					if (new Version(version.Name) > new Version(highestVersion)) highestVersion = version.Name;
				}
				catch (Exception e)
				{
					Log.Debug($"Could not parse version for {subFolder} folder");
				}

			if (highestVersion == "0.0.0") success = false;

			return (success, foundPackageId, highestVersion);
		}
	}
}