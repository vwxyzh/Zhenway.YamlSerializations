using System.Collections.Generic;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.Converters;

namespace Zhenway.YamlSerializations
{
    internal static class YamlTypeConverters
	{
		private static readonly IEnumerable<IYamlTypeConverter> _builtInTypeConverters = new IYamlTypeConverter[]
		{
			new GuidConverter(),
		};

		public static IEnumerable<IYamlTypeConverter> BuiltInConverters { get { return _builtInTypeConverters; } }
	}
}
