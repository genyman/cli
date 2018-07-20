using System;
using Genyman.Core;

namespace Genyman.Cli.Implementation
{
	public class Configuration
	{
		public string Prefix { get; set; }
		public string ToolName { get; set; }
		public string Description { get; set; }

		readonly Guid _projectGuid = Guid.NewGuid();

		[GenymanIgnore]
		public string ProjectGuid => $"{{{_projectGuid.ToString().ToUpper()}}}";

		[GenymanIgnore]
		public string SafePrefix => Prefix.Replace(" ", ""); //todo: can be better
		
		[GenymanIgnore]
		public string SafeToolName => ToolName.Replace(" ", "");

	}
}