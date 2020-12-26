using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Op14_yolanda_postproc
{
	class Program
	{
		async static Task Main(string[] args)
		{
			using var file = new FileStream("Input 14_2.txt", FileMode.Open);
			using var reader = new StreamReader(file);

			var dict1 = new Dictionary<ulong, ulong>();

			var resRegex = new Regex("(?'count'[0-9]+)\\s+(?'memads'[0-9]+)\\s+(?'memval'[0-9]+)", RegexOptions.Compiled);

           

			Console.WriteLine("Hello World!");
            

			var ctr = 0;
			do
			{
				var line = await reader.ReadLineAsync();

				if (line == null)
					break;

				var resMatch = resRegex.Match(line);
				var count = resMatch.Groups["count"].Value;
				var memads = resMatch.Groups["memads"].Value;
				var memval = resMatch.Groups["memval"].Value;

				Console.WriteLine($"Count: {count}");

			} while (!reader.EndOfStream);
			Console.WriteLine("Part 1 done!");
		}   
	}
}