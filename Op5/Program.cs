using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Op5
{
	class Program
	{
		async static Task Main(string[] args)
		{
			using var file = new FileStream("input.txt", FileMode.Open);
			using var reader = new StreamReader(file);

			Func<string, Entry> parse = txt =>
			{
				var row = 0;
				var seat = 0;

				foreach (var ch in txt.Take(7))
				{
					row <<= 1;
					if (ch == 'B')
					{
						row |= 0x01;
					}
				}

				foreach (var ch in txt.Skip(7))
				{
					seat <<= 1;
					if (ch == 'R')
					{
						seat |= 0x01;
					}
				}

				return new Entry(row, seat);
			};

			var seats = new List<Entry>();
			while (true)
			{
				var line = await reader.ReadLineAsync();
				if (line == null) break;
				
				seats.Add(parse(line));
			}

			var indexes = seats.Select(el => 8 * el.Row + el.Seat).ToList();

			Console.WriteLine(indexes.Max());
			
			var indexSet = indexes.ToHashSet();
			var options = Enumerable.Range(0, 128 * 8).Except(indexSet);

			foreach (var option in options)
			{
				if (indexSet.Contains(option + 1) && indexSet.Contains(option - 1))
				{
					Console.WriteLine(option);
				}
			}
		}

		record Entry(int Row, int Seat);
	}
}