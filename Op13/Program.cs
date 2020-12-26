using System;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace Op13
{
	class Program
	{
		async static Task Main(string[] args)
		{
			var timestamp = 1008169;
			var lineDef =
				"29,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,41,x,x,x,37,x,x,x,x,x,653,x,x,x,x,x,x,x,x,x,x,x,x,13,x,x,x,17,x,x,x,x,x,23,x,x,x,x,x,x,x,823,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,19";

			var lines =
				lineDef
					.Split(",")
					.Select((el,idx) => (valid: int.TryParse(el, out var val), num: val, idx: idx))
					.Where(kv => kv.valid)
					.Select(kv => (kv.num, kv.idx))
					.ToList();

			var minMod = lines.Select(el => (dt: timestamp % el.num, dv: timestamp / el.num, el.num))
				.OrderBy(el => el.dv * el.num)
				.ToList();

			foreach (var elems in minMod)
			{
				Console.WriteLine(
					$"m{elems.num - elems.dt} dv{elems.dv} line {elems.num} => {(elems.num - elems.dt) * elems.num}");
			}

			var orderedLines = lines.OrderByDescending(el => el.num)
				.Select(c => (idx: (ulong)c.idx, num: (ulong)c.num))
				.ToList();
			var firstSiever = orderedLines.First();
			var others = orderedLines.Skip(1).ToList();

			const int cpuCount = 16;

			Func<int, ulong> siever = offset =>
			{
				var idx = (ulong)offset;
				while (true)
				{
					var ts = firstSiever.num * idx - firstSiever.idx;

					if (others.All(el => (ts + el.idx) % el.num == 0))
					{
						Console.WriteLine($"Answer: {ts}");
						return ts;
					}
					if (idx % 251658240 == 0) Console.WriteLine(idx);
					if (idx > 4503599627370495ul) return 0;

					idx += cpuCount;
				}
			};

			var threads = Enumerable.Range(0, cpuCount)
				.Select(el => new Task<ulong>(() => siever(el)))
				.ToArray();

			foreach (var thread in threads)
			{
				thread.Start();
			}

			var task = await Task.WhenAll(threads);
			Environment.Exit(0);
		}
	}
}