using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Op17
{
	public record Grid3D<T>
	{
		public int MinX { get; }
		public int MinY { get; }
		public int MinZ { get; }
		public int MaxX { get; }
		public int MaxY { get; }
		public int MaxZ { get; }
		public T Default { get; }
		
		private Dictionary<int, Dictionary<int, Dictionary<int, T>>> Values { get; }

		public Grid3D(T[][][] values, T def = default(T))
		{
			MinX = MinY = MinZ = 0;
			MaxX = values.Length - 1;
			MaxY = values.Select(v => v.Length).Max() - 1;
			MaxZ = values.SelectMany(v => v.Select(w => w.Length)).Max() - 1;
			Default = def;
			Values = values
				.Select((val, idx) => (val, idx))
				.ToDictionary(
					t => t.idx,
					t => t.val
						.Select((val, idx) => (val, idx))
						.ToDictionary(
							v => v.idx,
							v => v.val
								.Select((val, idx) => (val, idx))
								.ToDictionary(w => w.idx, w => w.val)
						));
		}

		public Grid3D(IEnumerable<Cell3D<T>> values, T def = default(T))
		{
			Values = values.GroupBy(gr => gr.X)
				.Select(l => (X: l.Key, Values: l
					.GroupBy(gr => gr.Y)
					.Select(gr2 => (Y: gr2.Key, Dict: gr2.ToDictionary(g => g.Z, g => g.Value)))
					.ToDictionary(gr => gr.Y, gr => gr.Dict)))
				.ToDictionary(g => g.X, g => g.Values);

			MinX = Values.Keys.Min();
			MaxX = Values.Keys.Max();
			MinY = Values.Values.SelectMany(k => k.Keys).Min();
			MaxY = Values.Values.SelectMany(k => k.Keys).Max();
			MinZ = Values.Values.SelectMany(k => k.Values.SelectMany(l => l.Keys)).Min();
			MaxZ = Values.Values.SelectMany(k => k.Values.SelectMany(l => l.Keys)).Max();
			Default = def;
		}

		public IEnumerable<Cell3D<T>> GetNeighbours(Cell3D<T> cell)
		{
			var options = new[] {-1, 0, 1};
			foreach (var dx in options)
			{
				foreach (var dy in options)
				{
					foreach (var dz in options)
					{
						if (dx != 0 || dy != 0 || dz != 0)
						{
							yield return this[cell.X + dx, cell.Y + dy, cell.Z + dz];
						}
					}
				}
			}
		}

		public IEnumerable<Cell3D<T>> GetPossibleCells()
		{
			for (int x = MinX - 1; x <= MaxX + 1; x++)
			{
				for (int y = MinY - 1; y <= MaxY + 1; y++)
				{
					for (int z = MinZ - 1; z <= MaxZ + 1; z++)
					{
						yield return this[x, y, z];
					}
				}
			}
		}
		
		public Cell3D<T> this[int x, int y, int z]
		{
			get
			{
				if (Values.TryGetValue(x, out var ys) && ys.TryGetValue(y, out var zs) && zs.TryGetValue(z, out var t))
				{
					return new Cell3D<T> {X = x, Y = y, Z = z, Value = t};
				}

				return new Cell3D<T> {X = x, Y = y, Z = z, Value = Default};
			}
		}
	}
	public record Grid4D<T>
	{
		public int MinX { get; }
		public int MinY { get; }
		public int MinZ { get; }
		public int MinW { get; }
		public int MaxX { get; }
		public int MaxY { get; }
		public int MaxZ { get; }
		public int MaxW { get; }
		public T Default { get; }
		
		private Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, T>>>> Values { get; }

		public Grid4D(T[][][][] values, T def = default(T))
		{
			MinX = MinY = MinZ = MinW = 0;
			MaxX = values.Length - 1;
			MaxY = values.Select(v => v.Length).Max() - 1;
			MaxZ = values.SelectMany(v => v.Select(w => w.Length)).Max() - 1;
			MaxW = values.SelectMany(v => v.SelectMany(w => w.Select(u => u.Length))).Max() - 1;
			Default = def;
			Values = values
				.Select((val, idx) => (val, idx))
				.ToDictionary(
					t => t.idx,
					t => t.val
						.Select((val, idx) => (val, idx))
						.ToDictionary(
							v => v.idx,
							v => v.val
								.Select((val, idx) => (val, idx))
								.ToDictionary(w => w.idx, w => w.val
									.Select((val, idx) => (val, idx))
									.ToDictionary(x => x.idx, x => x.val))
						));
		}

		public Grid4D(IEnumerable<Cell4D<T>> values, T def = default(T))
		{
			Values = values.GroupBy(gr => gr.X)
				.Select(l => (X: l.Key, Values: l
					.GroupBy(gr => gr.Y)
					.Select(gr2 => (Y: gr2.Key, Values: gr2
						.GroupBy(g => g.Z)
						.ToDictionary(g => g.Key, g => g
							.ToDictionary(g3 => g3.W, g3 => g3.Value))))
					.ToDictionary(gr => gr.Y, gr => gr.Values)))
				.ToDictionary(g => g.X, g => g.Values);

			MinX = Values.Keys.Min();
			MaxX = Values.Keys.Max();
			MinY = Values.Values.SelectMany(k => k.Keys).Min();
			MaxY = Values.Values.SelectMany(k => k.Keys).Max();
			MinZ = Values.Values.SelectMany(k => k.Values.SelectMany(l => l.Keys)).Min();
			MaxZ = Values.Values.SelectMany(k => k.Values.SelectMany(l => l.Keys)).Max();
			MinW = Values.Values.SelectMany(k => k.Values.SelectMany(l => l.Values.SelectMany(m => m.Keys))).Min();
			MaxW = Values.Values.SelectMany(k => k.Values.SelectMany(l => l.Values.SelectMany(m => m.Keys))).Max();
			Default = def;
		}

		public IEnumerable<Cell4D<T>> GetNeighbours(Cell4D<T> cell)
		{
			var options = new[] {-1, 0, 1};
			foreach (var dx in options)
			{
				foreach (var dy in options)
				{
					foreach (var dz in options)
					{
						foreach (var dw in options)
						{
							if (dx != 0 || dy != 0 || dz != 0 || dw != 0)
							{
								yield return this[cell.X + dx, cell.Y + dy, cell.Z + dz, cell.W + dw];
							}
						}
					}
				}
			}
		}

		public IEnumerable<Cell4D<T>> GetPossibleCells()
		{
			for (int x = MinX - 1; x <= MaxX + 1; x++)
			{
				for (int y = MinY - 1; y <= MaxY + 1; y++)
				{
					for (int z = MinZ - 1; z <= MaxZ + 1; z++)
					{
						for (int w = MinW - 1; w <= MaxW + 1; w++)
						{
							yield return this[x, y, z, w];
						}
					}
				}
			}
		}
		
		public Cell4D<T> this[int x, int y, int z, int w]
		{
			get
			{
				if (Values.TryGetValue(x, out var ys) && 
					ys.TryGetValue(y, out var zs) && 
					zs.TryGetValue(z, out var ws) &&
					ws.TryGetValue(w, out var t))
				{
					return new Cell4D<T> {X = x, Y = y, Z = z, W = w, Value = t};
				}

				return new Cell4D<T> {X = x, Y = y, Z = z, W = w, Value = Default};
			}
		}
	}

	public record Cell3D<T>
	{
		public int X { get; init; }
		public int Y { get; init; }
		public int Z { get; init; }
		public T Value { get; init; }
	}
	public record Cell4D<T>
	{
		public int X { get; init; }
		public int Y { get; init; }
		public int Z { get; init; }
		public int W { get; init; }
		public T Value { get; init; }
	}
}