using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace GameThing.UI.Config
{
	public abstract class ConfigurationLoader
	{
		protected ScreenComponent Load(TextReader reader)
		{
			var knownTypes = typeof(ScreenComponent)
				.Assembly
				.GetTypes()
				.Where(t => typeof(UIComponent).IsAssignableFrom(t))
				.ToArray();

			var screenComponentType = typeof(ScreenComponent);
			var ser = new XmlSerializer(screenComponentType, knownTypes);
			return (ScreenComponent) ser.Deserialize(reader);
		}
	}
}
