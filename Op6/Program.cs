using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Op6
{
	class Program
	{
		async static Task Main(string[] args)
		{
			using var file = new FileStream("input.txt", FileMode.Open);
			using var reader = new StreamReader(file);

			var groupAnswers = new List<List<string>>
			{
				new()
			};
			while (true)
			{
				var line = await reader.ReadLineAsync();
				if (line == null) break;

				if (line == string.Empty)
				{
					groupAnswers.Add(new List<string>());
					continue;
				}
				
				groupAnswers.Last().Add(line);
			}

			var questionsPerGroup = groupAnswers
				.Select(gr => gr
					.Select(gr => gr)
					.Aggregate(string.Empty.AsEnumerable(), (a, b) => a.Union(b))
					.ToList())
				.ToList();

			Console.WriteLine(questionsPerGroup.Aggregate(0, (a, b) => a + b.Count));
			
			var questionsPerGroup2 = groupAnswers
				.Select(gr => gr
					.Select(gr => gr)
					.Aggregate("abcdefghijklmnopqrstuvwxyz".AsEnumerable(), (a, b) => a.Intersect(b))
					.ToList())
				.ToList();

			Console.WriteLine(questionsPerGroup2.Aggregate(0, (a, b) => a + b.Count));
		}
	}
}