using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Op9
{
	class Program
	{
		async static Task Main(string[] args)
		{
			var lines = (await File.ReadAllLinesAsync("input.txt"))
				.Select(ulong.Parse)
				.ToList();

			int window = 25;
			var nonMatchingItem = default(ulong?);
			foreach (var currWindow in MovingWindow(lines, window + 1))
			{
				var curr = currWindow.ToList();
				var itemToCheck = curr.Last();
				if (!GetPossibleCombinations(curr.Take(window)).Select(el => el.Item1 + el.Item2).Contains(itemToCheck))
				{
					Console.WriteLine($"Non-matching: {itemToCheck}");
					nonMatchingItem = itemToCheck;
					break;
				}
			}

			foreach (var offset in Enumerable.Range(0, lines.Count))
			{
				ulong sum = 0;
				foreach (var item in lines.Skip(offset))
				{
					sum += item;
					if (sum >= nonMatchingItem) break;
				}

				if (sum == nonMatchingItem)
				{
					var items = new List<ulong>();
					sum = 0;
					foreach (var item in lines.Skip(offset).TakeWhile(el => sum < nonMatchingItem))
					{
						items.Add(item);
						sum += item;
					}

					var min = items.Min();
					var max = items.Max();
					Console.WriteLine($"min = {min}, max = {max}, p = {min+max}");
					break;
				}
			}
		}

		static IEnumerable<IEnumerable<T>> MovingWindow<T>(List<T> source, int history)
		{
			for (int i = history; i < source.Count; i++)
			{
				yield return source.Skip(i - history).Take(history);
			}
		}
		
		static IEnumerable<(T,T)> GetPossibleCombinations<T>(IEnumerable<T> nums)
		{
			var ns = nums.ToList();

			foreach (var outer in ns.Select((val, idx) => (val, idx)))
			{
				foreach (var inner in ns.Skip(outer.idx + 1))
				{
					yield return (outer.val,inner);
				}
			}
		}
	}
}