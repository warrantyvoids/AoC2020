using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Op3
{
	class Program
	{
		async static Task Main(string[] args)
		{
			using var file = new FileStream("input.txt", FileMode.Open);
			using var reader = new StreamReader(file);

			var map = new List<string>();
			while (true)
			{
				var line = await reader.ReadLineAsync();
				if (line == null) break;
				
				map.Add(line);
			}

			Func<int, int, char> valueAt = (x, y) =>
			{
				var line = map[y];
				var xMod = x % line.Length;
				return line[xMod];
			};

			Func<int, int, int> countSlope = (dx, dy) =>
			{
				var currX = 0;
				var currY = 0;

				var treeCount = 0;
				while (currY < map.Count)
				{
					var val = valueAt(currX, currY);
					if (val == '#')
					{
						treeCount++;
					}

					currX += dx;
					currY += dy;
				}

				return treeCount;
			};

			var slopes = new (int dx, int dy)[]
			{
				(1, 1),
				(3, 1),
				(5, 1),
				(7, 1),
				(1, 2)
			};

			var vals = slopes.Select(el => (el.dx, el.dy, trees: countSlope(el.dx, el.dy))).ToList();

			foreach (var val in vals)
			{
				Console.WriteLine($"{val.dx}, {val.dy} = {val.trees}");
			}

			var multiplied = vals.Aggregate(1ul, (i, tuple) => i * (ulong)tuple.trees);
			Console.WriteLine(multiplied);
		}
	}
}