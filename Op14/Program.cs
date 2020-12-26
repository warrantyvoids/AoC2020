using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Op14
{
	class Program
	{
		async static Task Main(string[] args)
		{
			using var file = new FileStream("input.txt", FileMode.Open);
			using var reader = new StreamReader(file);

			var dict1 = new Dictionary<ulong, ulong>();
			var dict2 = new Dictionary<ulong, ulong>();

			ulong andMask = 0;
			ulong orMask = 0;

			var idxRegex = new Regex("mem\\[(?'idx'[0-9]+)\\] = (?'val'[0-9]+)", RegexOptions.Compiled);
			var maskRegex = new Regex("mask = (?'mask'[0-9X]+)", RegexOptions.Compiled);
			var lastMask = string.Empty;

			var ctr = 0;
			do
			{
				var line = await reader.ReadLineAsync();

				if (line == null)
					break;

				var maskMatch = maskRegex.Match(line);
				var idxMatch = idxRegex.Match(line);
				if (maskMatch.Success)
				{
					var mask = maskMatch.Groups["mask"].Value;
					Console.WriteLine($"M= '{mask}'");
					lastMask = mask;

					andMask = ~(0ul);
					orMask = 0ul;
					foreach (var mCh in mask.Reverse().Select((c, idx) => (c, idx)))
					{
						if (mCh.c == 'X')
							continue;

						if (mCh.c == '0')
							andMask &= ~(1ul << mCh.idx);
						
						if (mCh.c == '1')
							orMask |= (1ul << mCh.idx);
					}
					Console.WriteLine($"&m: '{andMask}'");
					Console.WriteLine($"|m: '{orMask}'");
				}
				else if (idxMatch.Success)
				{
					var idx = ulong.Parse(idxMatch.Groups["idx"].Value);
					var origValue = ulong.Parse(idxMatch.Groups["val"].Value);

					var value = (origValue & andMask) | orMask;

					dict1[idx] = value;

					foreach (var addr in PermutateAddresses(idx, lastMask))
					{
						dict2[addr] = origValue;
						ctr++;
					}
				}
			} while (!reader.EndOfStream);

			var sum = dict1.Values.Aggregate(0ul, (a, b) => a + b);
			Console.WriteLine($"Part 1: {sum}");
			
			var sum2 = dict2.Values.Aggregate(0ul, (a, b) => a + b);
			Console.WriteLine($"Part 2: {sum2}");

			Console.WriteLine(ctr);
			Console.WriteLine(dict2.Count);
		}

		private static IEnumerable<ulong> PermutateAddresses(ulong addr, string mask)
		{
			var xPositions = mask
				.Reverse()
				.Select((mCh, idx) => (consider: mCh == 'X', idx))
				.Where(v => v.consider)
				.ToList();

			var andMask = ~(0ul);
			var orMask = 0ul;
			foreach (var mCh in mask.Reverse().Select((c, idx) => (c, idx)))
			{
				if (mCh.c == 'X')
					andMask &= ~(1ul << mCh.idx);
						
				if (mCh.c == '1')
					orMask |= (1ul << mCh.idx);
			}

			var baseAddr = (addr & andMask) | orMask;
			for (int i = 0; i < (1 << xPositions.Count); i++)
			{
				var addrThisRound = baseAddr;
				for (int j = 0; j < xPositions.Count; j++)
				{
					if ((i & (1 << j)) != 0)
					{
						addrThisRound |= (1ul << xPositions[j].idx);
					}
				}

				yield return addrThisRound;
			}
		}
	}
}