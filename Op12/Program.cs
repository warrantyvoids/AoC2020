using System;
using System.IO;
using System.Linq;

namespace Op12
{
	class Program
	{
		static void Main(string[] args)
		{
			var instructions = File.ReadAllLines("input.txt").Select(line => Instruction.Parse(line)).ToList();
			var endState = instructions.Aggregate(
				new State( X: 0, Y: 0, Rotation: 90 ),
				NavigationMachine.NextState);
			Console.WriteLine($"X: {endState.X}, Y: {endState.Y}, MH = {Math.Abs(endState.X) + Math.Abs(endState.Y)}");

			var endState2 = instructions.Aggregate(new StateWithWaypoint(0, 0, 1, 10), NavigationMachine.NextState);
			Console.WriteLine($"X: {endState2.X}, Y: {endState2.Y}, MH = {Math.Abs(endState2.X) + Math.Abs(endState2.Y)}");
		}
	}
	
	static class NavigationMachine {
		private static int NormalizeDirection(int orig)
		{
			while (orig < 0) orig += 360;
			while (orig >= 360) orig -= 360;
			return orig;
		}
		
		public static State NextState(State previous, Instruction instruction)
		{
			switch (instruction.Type)
			{
				case InstructionType.North:
					return previous with {Y = previous.Y + instruction.Operand };
				case InstructionType.South:
					return previous with {Y = previous.Y - instruction.Operand };
				case InstructionType.East:
					return previous with {X = previous.X + instruction.Operand };
				case InstructionType.West:
					return previous with {X = previous.X - instruction.Operand };
				case InstructionType.Left:
					return previous with {Rotation = NormalizeDirection(previous.Rotation - instruction.Operand)};
				case InstructionType.Right:
					return previous with {Rotation = NormalizeDirection(previous.Rotation + instruction.Operand)};
				case InstructionType.Forward:
					return NextState(previous, instruction with
					{
						Type = previous.Rotation switch
						{
							0 => InstructionType.North,
							90 => InstructionType.East,
							180 => InstructionType.South,
							270 => InstructionType.West,
							_ => throw new ArgumentOutOfRangeException(),
						}
					});
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private static (int x, int y) Rotate(int x, int y, int rotation)
		{
			switch (NormalizeDirection(rotation))
			{
				case 0:
					return (x, y);
				case 90:
					return (-y, x);
				case 180:
					return (-x, -y);
				case 270:
					return (y, -x);
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		
		public static StateWithWaypoint NextState(StateWithWaypoint previous, Instruction instruction)
		{
			switch (instruction.Type)
			{
				case InstructionType.North:
					return previous with {WaypointRelativeX = previous.WaypointRelativeX + instruction.Operand};
				case InstructionType.South:
					return previous with {WaypointRelativeX = previous.WaypointRelativeX - instruction.Operand};
				case InstructionType.East:
					return previous with {WaypointRelativeY = previous.WaypointRelativeY + instruction.Operand};
				case InstructionType.West:
					return previous with {WaypointRelativeY = previous.WaypointRelativeY - instruction.Operand};
				case InstructionType.Left:
					var leftRotated = Rotate(previous.WaypointRelativeX, previous.WaypointRelativeY,
						-instruction.Operand);
					return previous with {WaypointRelativeX = leftRotated.x, WaypointRelativeY = leftRotated.y};
				case InstructionType.Right:
					var rightRotated = Rotate(previous.WaypointRelativeX, previous.WaypointRelativeY,
						instruction.Operand);
					return previous with {WaypointRelativeX = rightRotated.x, WaypointRelativeY = rightRotated.y};
				case InstructionType.Forward:
					return previous with
					{
						X = previous.X + previous.WaypointRelativeX * instruction.Operand,
						Y = previous.Y + previous.WaypointRelativeY * instruction.Operand
					};
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}

	enum InstructionType
	{
		North,
		South,
		East,
		West,
		Left,
		Right,
		Forward
	}
	
	record Instruction
	{
		public InstructionType Type { get; init; }
		public int Operand { get; init; }

		public static Instruction Parse(string s)
		{
			var mnemonic = s[0] switch
			{
				'N' => InstructionType.North,
				'S' => InstructionType.South,
				'E' => InstructionType.East,
				'W' => InstructionType.West,
				'L' => InstructionType.Left,
				'R' => InstructionType.Right,
				'F' => InstructionType.Forward,
				_ => throw new ArgumentException("Invalid mnemonic", nameof(s)),
			};
			var operand = int.Parse(s.Substring(1));
			return new Instruction
			{
				Type = mnemonic,
				Operand = operand,
			};
		}
	}

	record State(int X, int Y, int Rotation);

	record StateWithWaypoint(int X, int Y, int WaypointRelativeX, int WaypointRelativeY);
}