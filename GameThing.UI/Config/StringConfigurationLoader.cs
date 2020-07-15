using System.IO;
using System.Text;

namespace GameThing.UI.Config
{
	public class StringConfigurationLoader : ConfigurationLoader
	{
		public ScreenComponent Load(string xml)
		{
			var xmlBytes = Encoding.Default.GetBytes(xml);
			using (var memoryStream = new MemoryStream(xmlBytes))
			using (var reader = new StreamReader(memoryStream))
				return Load(reader);
		}
	}
}
