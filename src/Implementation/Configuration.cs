using System;
using System.Collections.Generic;
using Genyman.Core;

namespace Genyman.Cli.Implementation
{
	[Documentation(Source = "https://github.com/genyman/cli")]
	public class Configuration
	{
		[Description("The prefix of your Genyman package; your name, company, identifier for Nuget")]
		[Required]
		public string Prefix { get; set; }

		[Description("The name of the tool")]
		[Required]
		public string ToolName { get; set; }

		[Description("A description of what the tool does")]
		[Required]
		public string Description { get; set; }

		readonly Guid _projectGuid = Guid.NewGuid();

		[Ignore]
		public string ProjectGuid => $"{{{_projectGuid.ToString().ToUpper()}}}";

		[Ignore]
		public string SafePrefix => Prefix.Replace(" ", ""); //todo: can be better

		[Ignore]
		public string SafeToolName => ToolName.Replace(" ", "");
	}
}