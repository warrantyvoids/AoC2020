using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Op11
{
	public record PlaneState
	{
		public virtual bool Equals(PlaneState other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return States.Length == other.States.Length && States.Zip(other.States).All(st => st.First.Length == st.Second.Length && st.First.Zip(st.Second).All(ch => ch.First == ch.Second));
		}

		public override int GetHashCode()
		{
			return States.GetHashCode();
		}

		public PlaneState(IEnumerable<IEnumerable<ChairState>> chairs)
		{
			States = chairs?.Select(ch => ch.ToImmutableArray()).ToImmutableArray() ??
					throw new ArgumentNullException(nameof(chairs));

			if (States.Length == 0) throw new ArgumentException("Empty state provided", nameof(chairs));

			var firstNumColumns = States.First().Length;
			if (firstNumColumns == 0)
			{
				throw new ArgumentException("Empty state provided in rows.", nameof(chairs));
			}

			if (States.Any(row => row.Length != firstNumColumns))
			{
				throw new ArgumentException("2D Array is not square.", nameof(chairs));
			}
		}

		public PlaneState(IEnumerable<Chair> chairs)
		{
			var rows = chairs.GroupBy(ch => ch.Y).ToList();
			var maxRow = rows.Select(l => l.Key).Max();
			if (rows.Count == 0) throw new ArgumentException("Empty state provided.", nameof(chairs));
			if (rows.Count != maxRow + 1) throw new ArgumentException("Missing row.", nameof(chairs));
			var columns = rows.Select(row => row.Select(el => el.X).Max()).ToList();
			var firstColumnCount = columns.First();
			if (columns.Any(col => col != firstColumnCount))
				throw new ArgumentException("2D Array is not square.", nameof(chairs));
			if (rows.Any(row => row.Count() != firstColumnCount + 1))
				throw new ArgumentException("Missing column", nameof(chairs));

			States = rows
				.OrderBy(r => r.Key)
				.Select(r => r
					.OrderBy(ch => ch.X)
					.Select(ch => ch.State)
					.ToImmutableArray())
				.ToImmutableArray();
		}

		private ImmutableArray<ImmutableArray<ChairState>> States { get; }
		public int RowCount => States.Length;
		public int ColumnCount => States.First().Length;

		public IEnumerable<IEnumerable<Chair>> Rows =>
			States.Select((row, y) => row.Select((ch, x) => new Chair(ch, x, y)));

		public IEnumerable<Chair> Chairs => Rows.SelectMany(t => t);

		public Chair this[int x, int y] {
			get 
			{
				if (x < 0 || x >= ColumnCount) throw new ArgumentOutOfRangeException(nameof(x));
				if (y < 0 || y >= RowCount) throw new ArgumentOutOfRangeException(nameof(y));
				return new Chair(States[y][x], x, y);
			}
		}

		public IEnumerable<Chair> GetNeighbours(Chair ch)
		{
			if (ch.Y - 1 >= 0)
			{
				if (ch.X - 1 >= 0) yield return this[ch.X - 1, ch.Y - 1];
				yield return this[ch.X, ch.Y - 1];
				if (ch.X + 1 < ColumnCount) yield return this[ch.X + 1, ch.Y - 1];
			}

			if (ch.X - 1 >= 0) yield return this[ch.X - 1, ch.Y];
			if (ch.X + 1 < ColumnCount) yield return this[ch.X + 1, ch.Y];

			if (ch.Y + 1 < RowCount)
			{
				if (ch.X - 1 >= 0) yield return this[ch.X - 1, ch.Y + 1];
				yield return this[ch.X, ch.Y + 1];
				if (ch.X + 1 < ColumnCount) yield return this[ch.X + 1, ch.Y + 1];
			}
		}
	}

	public enum ChairState
	{
		Floor,
		Empty,
		Occupied
	}

	public static class StateHelper
	{
		public static ChairState Parse(char c)
		{
			return c switch
			{
				'.' => ChairState.Floor,
				'L' => ChairState.Empty,
				'#' => ChairState.Occupied,
				_ => throw new ArgumentOutOfRangeException("c", c, null),
			};
		}

		private static string ConvertToString(this ChairState ch)
		{
			return ch switch
			{
				ChairState.Floor => ".",
				ChairState.Empty => "L",
				ChairState.Occupied => "#",
				_ => throw new ArgumentOutOfRangeException(nameof(ch), ch, null)
			};
		} 

		public static string Format(this PlaneState state)
		{
			return string.Join(
				'\n',
				state.Rows
					.Select(row => string.Join(string.Empty, row.Select(ch => ch.State.ConvertToString()))));
		}
	}

	public record Chair(ChairState State, int X, int Y);
}