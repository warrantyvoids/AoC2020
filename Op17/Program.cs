using System;
using System.IO;
using System.Linq;

namespace Op17
{
	class Program
	{
		static void Main(string[] args)
		{
			var lines = File.ReadAllLines("input.txt").Select(line => line.Select(ch => ch switch
			{
				'#' => State.Active,
				'.' => State.Inactive,
				_ => throw new Exception()
			}).ToArray())
				.ToArray();

			var state3D = new Grid3D<State>(new[] {lines}, State.Inactive);
			var finalState3D = Enumerable.Range(1, 6).Aggregate(state3D, (prev, idx) => Transform(prev));
			var activeSquares = finalState3D.GetPossibleCells().Count(el => el.Value == State.Active);
			Console.WriteLine(activeSquares);
			
			var state4D = new Grid4D<State>(new[] {new[] { lines }}, State.Inactive);
			var finalState4D = Enumerable.Range(1, 6).Aggregate(state4D, (prev, idx) => Transform(prev));
			var activeSquares4D = finalState4D.GetPossibleCells().Count(el => el.Value == State.Active);
			Console.WriteLine(activeSquares4D);
		}

		static Grid3D<State> Transform(Grid3D<State> previous)
		{
			var elems = previous.GetPossibleCells()
				.Select(el =>
			{
				var activeNeighbours = previous.GetNeighbours(el).Count(t => t.Value == State.Active);
				if (el.Value == State.Active)
				{
					if (activeNeighbours == 2 || activeNeighbours == 3)
					{
						return el;
					}

					return el with {Value = State.Inactive};
				}

				if (activeNeighbours == 3)
				{
					return el with {Value = State.Active};
				}

				return el;
			});
			return new Grid3D<State>(elems);
		}
		static Grid4D<State> Transform(Grid4D<State> previous)
		{
			var elems = previous.GetPossibleCells()
				.Select(el =>
				{
					var activeNeighbours = previous.GetNeighbours(el).Count(t => t.Value == State.Active);
					if (el.Value == State.Active)
					{
						if (activeNeighbours == 2 || activeNeighbours == 3)
						{
							return el;
						}

						return el with {Value = State.Inactive};
					}

					if (activeNeighbours == 3)
					{
						return el with {Value = State.Active};
					}

					return el;
				});
			return new Grid4D<State>(elems);
		}
	}

	enum State
	{
		Inactive,
		Active,
	}
}