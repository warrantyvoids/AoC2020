using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Op4
{
	class Program
	{
		async static Task Main(string[] args)
		{
			var regex = new Regex("(?'key'[a-zA-Z]{1,3}):(?'value'[a-zA-Z0-9#]+)", RegexOptions.Compiled);
			var hclRgex = new Regex("#[0-9a-f]{6}", RegexOptions.Compiled);
			var eclOptions = new[] {"amb", "blu", "brn", "gry", "grn", "hzl", "oth"};

			var keyDescr = new Dictionary<string,(string key, bool required, Func<string, bool> validator)>
			{
				["byr"] = ("byr", true, s => s.Length == 4 && int.TryParse(s, out var byr) && byr >= 1920 && byr <= 2002),
				["iyr"] = ("iyr", true, s => s.Length == 4 && int.TryParse(s, out var iyr) && iyr >= 2010 && iyr <= 2020),
				["eyr"] = ("eyr", true, s => s.Length == 4 && int.TryParse(s, out var iyr) && iyr >= 2020 && iyr <= 2030),
				["hgt"] = ("hgt", true, s =>
				{
					if (s.EndsWith("cm"))
					{
						var hgtStr = s.Substring(0, s.Length - 2);
						return int.TryParse(hgtStr, out var hgt) && hgt >= 150 && hgt <= 193;
					} 
					if (s.EndsWith("in"))
					{
						var hgtStr = s.Substring(0, s.Length - 2);
						return int.TryParse(hgtStr, out var hgt) && hgt >= 59 && hgt <= 76;
					}

					return false;
				}),
				["hcl"] = ("hcl", true, s => hclRgex.IsMatch(s)),
				["ecl"] = ("ecl", true, s => eclOptions.Any(t => t == s)),
				["pid"] = ("pid", true, s => s.Length == 9 && int.TryParse(s, out var _)),
				["cid"] = ("cid", false, s => true)
			};
			int reqCount = keyDescr.Count(k => k.Value.required);

			using var file = new FileStream("input.txt", FileMode.Open);
			using var reader = new StreamReader(file);

			var validPassports = 0;
			var validPassportsWithValidation = 0;
			while (true)
			{
				string lastLine;
				var lines = new List<string>();
				while(true)
				{
					lastLine = await reader.ReadLineAsync();
					if (string.IsNullOrEmpty(lastLine))
						break;
					lines.Add(lastLine);
				}

				if (lastLine == null) break;

				var elems = new List<(string key, string value)>();
				foreach (var line in lines)
				{
					var match = regex.Match(line);
					while (match.Success)
					{
						elems.Add((match.Groups["key"].Value, match.Groups["value"].Value));
						match = match.NextMatch();
					}
				}

				var foundCount = new Dictionary<string, int>();
				var spares = new List<string>();
				foreach (var elem in elems)
				{
					if (keyDescr.ContainsKey(elem.key))
					{
						if (foundCount.ContainsKey(elem.key))
						{
							foundCount[elem.key]++;
						}
						else
						{
							foundCount[elem.key] = 1;
						}
						
					}
					else
					{
						spares.Add(elem.key);
					}
				}

				if (spares.Count > 0) continue;
				if (foundCount.Values.Any(t => t > 1)) continue;

				var foundRequiredKeys = 0;
				foreach (var key in foundCount.Keys)
				{
					if (keyDescr.TryGetValue(key, out var descr))
					{
						if (descr.required) foundRequiredKeys++;
					}
				}

				if (foundRequiredKeys == reqCount)
				{
					validPassports++;
					if (foundCount
						.All(kvp => 
							keyDescr[kvp.Key].validator(elems.Single(el => el.key == kvp.Key).value)))
						validPassportsWithValidation++;
				}
				
				
			}
			
			Console.WriteLine(validPassports);
			Console.WriteLine(validPassportsWithValidation);
		}
	}
}