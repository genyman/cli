using {{ Prefix }}.Genyman.{{ ToolName }}.Implementation;
using Genyman.Core;

namespace {{ Prefix }}.Genyman.{{ ToolName }}
{
	internal class Program
	{
		public static void Main(string[] args)
		{
			GenymanApplication.Run<Configuration, NewTemplate, Generator>(args);
		}
	}
}