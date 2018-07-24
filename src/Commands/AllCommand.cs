using System;
using System.Collections.Generic;
using System.IO;
using Genyman.Cli.Implementation;
using Genyman.Core;
using Genyman.Core.Commands;
using McMaster.Extensions.CommandLineUtils;

namespace Genyman.Cli.Commands
{
	public class AllCommand : BaseCommand
	{
		public AllCommand()
		{
			Name = "all";
			Description = "Auto executes all generators";
			RecursiveOption = Option("--recursive", "Scan all folders and subfolders for Genyman configuration files", CommandOptionType.NoValue, option => { }, true);
		}

		public CommandOption RecursiveOption { get; }

		protected override int Execute()
		{
			var searchOption = RecursiveOption.HasValue() ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
			var allFiles = Directory.EnumerateFiles(WorkingDirectory, "*.json", searchOption); //json only for now
			var currentWorkingDirectory = WorkingDirectory;
			
			foreach (var file in allFiles)
			{
				// load file and check whether it contains a known structure for Genyman
				// if it does, execute it
				var contents = File.ReadAllText(file);
				if (contents.Contains("\"genyman\":"))
				{
					var fileInfo = new FileInfo(file);
					var args = new List<string>(Generator.Args);
					args.Remove("all");
					args.Remove("--recursive");
					args.Insert(0, fileInfo.Name);
					
					Log.Debug($"Switchting to {fileInfo.DirectoryName}");
					Directory.SetCurrentDirectory(fileInfo.DirectoryName);
					
					GenymanApplication.Run<Configuration, NewTemplate, Generator>(args.ToArray(), true,
						subcommands => { }, null);
				}
			}
			Directory.SetCurrentDirectory(currentWorkingDirectory);

			return 0;
		}
	}
}