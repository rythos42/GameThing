using System;
using System.IO;
using System.Reflection;

namespace GameThing.UI.Config
{
	public class EmbeddedResourceConfigurationLoader : ConfigurationLoader
	{
		private readonly Assembly loadingFrom;

		public EmbeddedResourceConfigurationLoader(Assembly loadingFrom)
		{
			this.loadingFrom = loadingFrom;
		}

		public ScreenComponent Load(string name)
		{
			var embeddedResourceStream = loadingFrom.GetManifestResourceStream(name);
			if (embeddedResourceStream == null)
				throw new Exception($"No resource named {name} in assembly {loadingFrom.FullName}.");

			using (var streamReader = new StreamReader(embeddedResourceStream))
				return Load(streamReader);
		}
	}
}
