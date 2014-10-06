using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ToSic.Eav.DataSources
{
	public static class DataPipelineWiring
	{
		private static readonly Regex WireRegex = new Regex("(?<From>.*):(?<Out>.*)>(?<To>.*):(?<In>.*)");

		public static IEnumerable<WireInfo> Deserialize(string wiringsSerialized)
		{
			var wirings = wiringsSerialized.Split(new[] { "\r\n" }, StringSplitOptions.None);

			return wirings.Select(wire => WireRegex.Match(wire)).Select(match => new WireInfo
			{
				From = match.Groups["From"].Value,
				Out = match.Groups["Out"].Value,
				To = match.Groups["To"].Value,
				In = match.Groups["In"].Value
			});
		}

		public static string Serialize(IEnumerable<WireInfo> wirings)
		{
			return string.Join("\r\n", wirings.Select(w => w.ToString()));
		}
	}

	public struct WireInfo
	{
		public string From { get; set; }
		public string Out { get; set; }
		public string To { get; set; }
		public string In { get; set; }

		public override string ToString()
		{
			return From + ":" + Out + ">" + To + ":" + In;
		}
	}
}