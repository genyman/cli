using {{ SafePrefix }}.Genyman.{{ SafeToolName }}.Implementation;
using Genyman.Core;

namespace {{ SafePrefix }}.Genyman.{{ SafeToolName }}
{
	internal class Program
	{
		public static void Main(string[] args)
		{
			GenymanApplication.Run<Configuration, NewTemplate, Generator>(args);
		}
	}
}