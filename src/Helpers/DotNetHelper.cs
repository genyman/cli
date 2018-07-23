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
					Log.Debug(s);
					return true;
				})
				.Execute(true);
		}

		internal static int NugetPush(string nugetPackage, string nugetSource = null, string nugetApiKey = null)
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
				Log.Debug(s);
				return true;
			});
			return push.Execute(true);
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
						Log.Debug(s);
						return true;
					})
					.Execute(true);
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
						Log.Debug(s);
						return true;
					})
					.Execute(true);
		}

		internal static bool Install(string packageId, string source, string version)
		{
			var install = ProcessRunner.Create(DotnetCommand)
				.WithArgument("tool")
				.WithArgument("install")
				.WithArgument("-g")
				.WithArgument(packageId)
				.ReceiveOutput(s =>
				{
					Log.Debug(s);
					return true;
				});

			if (!string.IsNullOrEmpty(source))
				install.WithArgument("--add-source", source);

			if (!string.IsNullOrEmpty(version))
				install.WithArgument("--version", version);

			var exitCode = install.Execute(true);
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
					Log.Debug(s);
					return true;
				});

			if (!string.IsNullOrEmpty(source))
				update.WithArgument("--add-source", source);

			var exitCode = update.Execute(true);
			return exitCode == 0;
		}

		internal static bool UnInstall(string packageId)
		{
			var install = ProcessRunner.Create(DotnetCommand)
				.WithArgument("tool")
				.WithArgument("uninstall")
				.WithArgument("-g")
				.WithArgument(packageId)
				.ReceiveOutput(s =>
				{
					Log.Debug(s);
					return true;
				});
			var exitCode = install.Execute(true);
			return exitCode == 0;
		}

		internal static (bool success, string packageId, bool specificVersionInstalled) ResolvePackage(string packageId, string source, bool autoUpdate, string specificVersion)
		{
			var isFullPackageId = true;

			if (!packageId.ToLower().Contains(".genyman."))
			{
				isFullPackageId = false;
				packageId = ".genyman." + packageId;
			}

			var local = DoesPackageExists(packageId);
			var canContinue = false;
			var specificVersionInstalled = false;

			if (!local)
			{
				if (!isFullPackageId)
				{
					Log.Error($"Genyman package {packageId} is not installed. Auto-installation cannot be performed as {packageId} is not a fully qualified package Id.");
					{
						return (false, packageId, false);
					}
				}

				canContinue = Install(packageId, source, null);
			}
			else
			{
				// perform update, we need full package name
				var latest = GetLastestPackageVersion(packageId);
				packageId = latest.packageId; // always get full packageId here

				canContinue = latest.success;

				if (!string.IsNullOrEmpty(specificVersion) && latest.version != specificVersion)
				{
					// uninstall & install
					UnInstall(packageId);
					canContinue = Install(packageId, source, specificVersion);
					specificVersionInstalled = true;
				}
				else
				{
					if (canContinue && isFullPackageId && autoUpdate)
						Update(packageId, source);
				}
			}

			return (canContinue, packageId, specificVersionInstalled);
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
					Log.Debug(e.ToString());
					Log.Debug($"Could not parse version for {subFolder} folder");
				}

			if (highestVersion == "0.0.0") success = false;

			return (success, foundPackageId, highestVersion);
		}
	}
}