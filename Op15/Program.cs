using System;
using System.Collections.Generic;
using System.Linq;

namespace Op15
{
	class Program
	{
		static void Main(string[] args)
		{
			var numbers = new Dictionary<int, int>();

			var currRound = 1;
			var lastNum = 0;

			var startingNumbers = "0,1,4,13,15,12,16"
				.Split(',')
				.Select(el => int.Parse(el));

			Func<int, int> findNext = el =>
			{
				if (numbers.TryGetValue(el, out var lastRound))
				{
					return lastRound - currRound;
				}
				return 0;
			};
			Console.WriteLine($"{currRound}: {lastNum}");

			foreach (var num in startingNumbers)
			{
				currRound++;
				lastNum = findNext(num);
				numbers[num] = currRound;
			}

			do
			{
				currRound++;
				if (!numbers.TryGetValue(lastNum, out var lastRoundSpoken))
				{
					numbers[lastNum] = currRound;
					lastNum = 0;
				}
				else
				{
					numbers[lastNum] = currRound;
					var diff = currRound - lastRoundSpoken;
					lastNum = diff;
				}
				//Console.WriteLine($"{currRound}: {lastNum}");
			} while (currRound < 30000000);
			
			Console.WriteLine(lastNum);
		}
	}
}