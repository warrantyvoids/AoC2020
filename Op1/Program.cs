using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Op1
{
	class Program
	{
		async static Task Main(string[] args)
		{
			using var stream = new FileStream("input.txt", FileMode.Open);
			using var reader = new StreamReader(stream);

			var numbers = new List<int>();
			string? line;
			while ((line = await reader.ReadLineAsync()) != null)
			{
				numbers.Add(int.Parse(line));
			}

			foreach (var numOuter in numbers)
			{
				var expectedVal = 2020 - numOuter;
				var match = numbers
					.Where(v => v == expectedVal)
					.ToList();

				if (match.Any())
				{
					Console.WriteLine($"{numOuter} + {match.Single()} = 2020");
					Console.WriteLine($"{numOuter * match.Single()}");
					break;
				}
			}
			foreach (var numOuter in numbers.Select((val, idx) => (val, idx)))
			{
				var expectedValOuter = 2020 - numOuter.val;
				foreach (var numInner in numbers.Skip(numOuter.idx).Select((val, idx) => (val, idx)))
				{
					var expectedInner = expectedValOuter - numInner.val;
					var match = numbers.Where(v => v == expectedInner).ToList();
					if (match.Any())
					{
						Console.WriteLine($"{numOuter.val} + {numInner.val} + {match.Single()} = 2020");
						Console.WriteLine($"{numOuter.val * numInner.val * match.Single()}");
						break;
					}
				}
			}
			Console.WriteLine("???");
		}
	}
}