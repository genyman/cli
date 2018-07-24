using Genyman.Cli.Commands;
using Genyman.Cli.Implementation;
using Genyman.Core;

namespace Genyman.Cli
{
	internal class Program
	{
		public static void Main(string[] args)
		{
			Generator.Args = args;
			GenymanApplication.Run<Configuration, NewTemplate, Generator>(args,
				true,
				subcommands =>
				{
					subcommands.Add(new DeployCommand());
					subcommands.Add(new AllCommand());
				}, 
				() => NewPackageIdCommand.Run(args));
		}
	}
}