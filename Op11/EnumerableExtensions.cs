using System;
using System.Collections;
using System.Collections.Generic;

namespace Op11
{
	public static class Enumerable
	{
		public static IEnumerable<(T Previous, T Current)> GenerateWithPrevious<T>(T initial, Func<T, T> transform)
		{
			var previous = initial;
			while (true)
			{
				var current = transform(previous);
				yield return (previous, current);
				previous = current;
			}
		}
	}
}