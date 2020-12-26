using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Op2
{
	class Program
	{
		async static Task Main(string[] args)
		{
			var regex = new Regex("(?'min'[0-9]+)-(?'max'[0-9]+) (?'sym'[0-9a-zA-Z]+): (?'entry'[0-9a-zA-Z]+)",
				RegexOptions.Compiled);

			using var fileStream = new FileStream("input.txt", FileMode.Open);
			using var reader = new StreamReader(fileStream);

			var valid = 0;
			var valid2 = 0;
			
			while (true)
			{
				var line = await reader.ReadLineAsync();
				if (line == null) break;

				var match = regex.Match(line);
				if (!match.Success) throw new Exception();

				var min = int.Parse(match.Groups["min"].Value);
				var max = int.Parse(match.Groups["max"].Value);
				var sym = match.Groups["sym"].Value;
				var entry = match.Groups["entry"].Value;

				var matches = entry
					.Where(ch => ch == sym.Single())
					.ToList();

				if (matches.Count >= min && matches.Count <= max)
				{
					valid++;
				}

				if ((entry[min - 1] == sym.Single()) != (entry[max - 1] == sym.Single()))
				{
					valid2++;
				}
			}
			Console.WriteLine(valid);
			Console.WriteLine(valid2);
		}
	}
}