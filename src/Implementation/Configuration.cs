using System;
using Genyman.Core;

namespace Genyman.Cli.Implementation
{
	/// <summary>
	/// Genyman New Configuration
	/// </summary>
	public class Configuration
	{
		/// <summary>
		/// The prefix of your Genyman package; your name, company, identifier for Nuget
		/// </summary>
		public string Prefix { get; set; }
		
		/// <summary>
		/// The name of the tool
		/// </summary>
		public string ToolName { get; set; }
		
		/// <summary>
		/// A description of what the tool does
		/// </summary>
		public string Description { get; set; }

#pragma warning disable 1591

		readonly Guid _projectGuid = Guid.NewGuid();

		[GenymanIgnore]
		public string ProjectGuid => $"{{{_projectGuid.ToString().ToUpper()}}}";

		[GenymanIgnore]
		public string SafePrefix => Prefix.Replace(" ", ""); //todo: can be better
		
		[GenymanIgnore]
		public string SafeToolName => ToolName.Replace(" ", "");
		
#pragma warning restore 1591

	}
}