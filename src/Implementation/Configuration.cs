using System;
using Genyman.Core;

namespace Genyman.Cli.Implementation
{
	public class Configuration
	{
		public string Prefix { get; set; }
		public string ToolName { get; set; }

		readonly Guid _projectGuid = Guid.NewGuid();

		[GenymanIgnore]
		public string ProjectGuid => $"{{{_projectGuid.ToString().ToUpper()}}}";
	}
}