using System;
using System.Collections.Generic;
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
			
			var stableState = Enumerable.GenerateWithPrevious(state, Next).SkipWhile(el => el.Previous != el.Current)
				.First().Current;
			Console.WriteLine(stableState.Chairs.Count(ch => ch.State == ChairState.Occupied));
			
			var stableStateTwo = Enumerable.GenerateWithPrevious(state, NextPartTwo).SkipWhile(el => el.Previous != el.Current)
				.First().Current;
			Console.WriteLine(stableStateTwo.Chairs.Count(ch => ch.State == ChairState.Occupied));
		}

		private static PlaneState Next(PlaneState current)
		{
			var nextChairs = current.Chairs.Select(ch =>
			{
				return ch.State switch
				{
					ChairState.Floor => ch,
					ChairState.Empty => current.GetNeighbours(ch).All(n => n.State != ChairState.Occupied)
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
		
		private static PlaneState NextPartTwo(PlaneState current)
		{
			IEnumerable<Chair> VisibleNeighbours(Chair ch)
			{
				return current
					.GetNeighbourLines(ch)
					.Select(line => line
						.SkipWhile(nb => nb.State == ChairState.Floor)
						.FirstOrDefault())
					.Where(el => el != null)
					.Select(el => el!);
			}
			
			var nextChairs = current.Chairs.Select(ch =>
			{
				return ch.State switch
				{
					ChairState.Floor => ch,
					ChairState.Empty => VisibleNeighbours(ch).All(n => n.State != ChairState.Occupied)
						? ch with {State = ChairState.Occupied}
						: ch,
					ChairState.Occupied => VisibleNeighbours(ch).Count(n => n.State == ChairState.Occupied) >= 5
						? ch with {State = ChairState.Empty}
						: ch,
					_ => throw new ArgumentOutOfRangeException()
				};
			});
			return new PlaneState(nextChairs);
		}
	}
}