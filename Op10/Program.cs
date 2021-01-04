using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Op10
{
	class Program
	{
		static void Main(string[] args)
		{
			var lines = File.ReadAllLines("input.txt").Select(int.Parse).Append(0);

			var sorted = lines.OrderBy(el => el).ToList();
			sorted.Add(sorted.Last() + 3);

			var diffs = sorted
				.Zip(sorted.Skip(1), (a, b) => b - a)
				.ToLookup(a => a, a => a)
				.ToDictionary(a => a.Key, a => a.ToList());

			foreach (var diff in diffs.OrderBy(d => d.Key))
			{
				Console.WriteLine($"Diff: {diff.Key} Count: {diff.Value.Count}");
			}

			Console.WriteLine(diffs[1].Count() * diffs[3].Count());

			var options = sorted
				.Select((el, idx) => (el, idx))
				.Select(tup => sorted.Skip(tup.idx + 1).TakeWhile(v => v - tup.el <= 3).Count())
				.Where(opts => opts > 1)
				.ToList();

			var skipOptions = sorted
				.Select((val,idx)=>(val,idx))
				.Zip(sorted.Skip(2), (a, b) => (val: b - a.val, a.idx))
				.Where(d => d.val <= 3)
				.ToDictionary(d => d.idx, d => d.val);
			
			var groups = new List<List<(int idx, int skips)>>();
			var currGroup = new List<(int idx, int skips)>();
			groups.Add(currGroup);

			foreach (var idx in Enumerable.Range(0, sorted.Count))
			{
				if (skipOptions.TryGetValue(idx, out _))
				{
					currGroup.Add((idx+1, 1));
				}
				else
				{
					if (currGroup.Count > 0)
					{
						currGroup = new List<(int idx, int skips)>();
						groups.Add(currGroup);
					}
				}
			}

			if (groups.Last().Count == 0)
			{
				groups.RemoveAt(groups.Count-1);
			}

			var grs = new List<int>();
			var rejected = 0;
			foreach (var group in groups)
			{
				var toConsider = Enumerable
					.Range(0, 1 << group.Count)
					.Select(el => Enumerable
						.Range(0, group.Count)
						.Select(b => (el & (1 << b)) != 0)
						.Append(true)
						.Prepend(true));
				var partialToCheck = sorted
					.Skip(group.First().idx - 1)
					.Take(group.Count + 2)
					.ToList();

				var count = 0;
				foreach (var round in toConsider)
				{
					var afterRemoval = partialToCheck
						.Zip(round, Tuple.Create)
						.Where(el => el.Item2)
						.ToList();

					var consider = afterRemoval
						.Zip(afterRemoval.Skip(1), (a, b) => b.Item1 - a.Item1)
						.All(g => g <= 3);
					if (consider)
					{
						count++;
					}
					else
					{
						rejected++;
					}

				}
				grs.Add(count);
			}
			Console.WriteLine(grs.Aggregate(1ul, (a,b) => a*(ulong)b));
		}
	}
}