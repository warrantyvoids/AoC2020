using System;
using System.IO;
using System.Linq;

namespace Op11
{
	class Program
	{
		static void Main(string[] args)
		{
			var state = new PlaneState(
				File.ReadAllLines("input.txt")
					     .Select(line => line
							.Select(c => StateHelper.Parse(c))));

			foreach (var st in Enumerable.GenerateWithPrevious(state, Next).Take(10))
			{
				Console.WriteLine(st.Current.Format());
				Console.WriteLine();
			}
			
			var stableState = Enumerable.GenerateWithPrevious(state, Next).SkipWhile(el => el.Previous != el.Current)
				.First().Current;
			Console.WriteLine(stableState.Chairs.Count(ch => ch.State == ChairState.Occupied));
		}

		private static PlaneState Next(PlaneState current)
		{
			var nextChairs = current.Chairs.Select(ch =>
			{
				return ch.State switch
				{
					ChairState.Floor => ch,
					ChairState.Empty => (!current.GetNeighbours(ch).Any(n => n.State == ChairState.Occupied))
						? ch with {State = ChairState.Occupied}
						: ch,
					ChairState.Occupied => current.GetNeighbours(ch).Count(n => n.State == ChairState.Occupied) >= 4
						? ch with {State = ChairState.Empty}
						: ch,
					_ => throw new ArgumentOutOfRangeException()
				};
			});
			return new PlaneState(nextChairs);
		}
	}
}