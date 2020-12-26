using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Op16
{
	class Program
	{
		async static Task Main(string[] args)
		{
			using var file = new FileStream("input.txt", FileMode.Open);
			using var reader = new StreamReader(file);

			var ruleRegex =
				new Regex(
					"(?'tag'[a-zA-Z\\s]+):\\s(?'min1'[0-9]+)-(?'max1'[0-9]+)\\sor\\s(?'min2'[0-9]+)-(?'max2'[0-9]+)",
					RegexOptions.Compiled);

			var rules = new List<RuleDescriptor>();
			while (true)
			{
				var line = await reader.ReadLineAsync();

				if (string.IsNullOrEmpty(line)) break;
				
				var match = ruleRegex.Match(line);
				var tag = match.Groups["tag"].Value;
				var min1 = int.Parse(match.Groups["min1"].Value);
				var max1 = int.Parse(match.Groups["max1"].Value);
				var min2 = int.Parse(match.Groups["min2"].Value);
				var max2 = int.Parse(match.Groups["max2"].Value);
				
				rules.Add(new RuleDescriptor
				{
					Tag = tag,
					Ranges = new RuleRange[]
					{
						new() { Min = min1, Max = max1 },
						new() { Min = min2, Max = max2 },
					}
				});
			}

			{
				var ticketLine = await reader.ReadLineAsync();
				if (ticketLine != "your ticket:") throw new Exception();
			}
			var ticket = (await reader.ReadLineAsync())!.Split(",").Select(int.Parse).ToList();
			
			{
				var newLine = await reader.ReadLineAsync();
				var ticketLine = await reader.ReadLineAsync();
				if (ticketLine != "nearby tickets:") throw new Exception();
			}

			var ticketScanningErrorRate = 0;
			var validNearbyTickets = new List<List<int>>();
			
			while (true)
			{
				var ticketLine = await reader.ReadLineAsync();
				if (ticketLine == null)
					break;
					
				var entries = ticketLine.Split(",").Select(int.Parse).ToList();
				var invalidEntries = entries.Where(entry => !rules.Any(rule => rule.Valid(entry))).ToList();

				foreach (var entry in invalidEntries)
				{
					ticketScanningErrorRate += entry;
				}

				if (!invalidEntries.Any())
				{
					validNearbyTickets.Add(entries);
				}
			}
			Console.WriteLine(ticketScanningErrorRate);

			var validTickets = validNearbyTickets.Append(ticket).ToList();
			
			var options = rules.Select(r =>
				Enumerable.Range(0, validTickets.First().Count)
					.Select(idx => validTickets.All(t => r.Valid(t[idx])))
					.ToList())
				.ToList();

			var mappings = new Dictionary<int, string>();
			while (mappings.Count != validTickets.First().Count)
			{
				var definiteAnswers = options
					.Select((op, idx) => (idx, count: op.Count(op2 => op2), cols: op))
					.Where(cnt => cnt.count == 1)
					.Select(op => (op.idx, op.count, col: op.cols.IndexOf(true)))
					.ToList();
				if (definiteAnswers.Count == 0) throw new Exception("Stuck");

				foreach (var answer in definiteAnswers)
				{
					mappings.Add(answer.col, rules[answer.idx].Tag);
					foreach (var option in options)
					{
						option[answer.col] = false;
					}
				}
			}

			var relevantValues = mappings.Where(kvp => kvp.Value.StartsWith("departure"));
			var multiply = relevantValues
				.Aggregate(1ul, (prev, kvp) => prev * (ulong)ticket[kvp.Key]);
			
			Console.WriteLine(multiply);
		}

		record RuleRange
		{
			public int Min { get; init; }
			public int Max { get; init; }

			public bool Valid(int value) => value >= Min && value <= Max;
		}

		record RuleDescriptor
		{
			public string Tag { get; init; }
			public RuleRange[] Ranges { get; init; }

			public bool Valid(int value) => Ranges.Any(r => r.Valid(value));
		};
	}
}