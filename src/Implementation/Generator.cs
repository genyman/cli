using Genyman.Core;

namespace Genyman.Cli.Implementation
{
	public class Generator : GenymanGenerator<Configuration>
	{
		public override void Execute()
		{
			if (ConfigurationMetadata.PackageId == Metadata.PackageId)
			{
				CreateTemplate();
			}
			else
			{
				// todo:
				// - try to resolve packageId
				// - execute package id with same json file
				
				// determine if the INPUT is
				// - filename = config file
				// - packageId (with new?)
				
				// add some extra options here?
				// genyman packageId (of is dit gelijk aan NEW?)
				// genyman packageId (--new)    --json --yaml --xaml -- FORWARD these to the packages!
			}
		}

		void CreateTemplate()
		{
			Log.Information($"Generating a new configuration file for {Metadata.PackageId}");
			ProcessHandlebarTemplates();
		}
	}
}