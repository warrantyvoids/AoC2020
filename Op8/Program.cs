using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Op8
{
	class Program
	{
		async static Task Main(string[] args)
		{
			var lines = File.ReadAllLinesAsync("input.txt");
			var regex = new Regex("(?'mnemonic'[a-z]{3})\\s(?'operand'[+\\-][0-9]+)", RegexOptions.Compiled);
			var program = (await lines)
				.Select(l => regex.Match(l))
				.Select(m =>
				{
					var mn = m.Groups["mnemonic"].Value switch
					{
						"nop" => InstructionCode.Nop,
						"acc" => InstructionCode.Acc,
						"jmp" => InstructionCode.Jmp,
						_ => throw new Exception("Invalid opcode."),
					};
					var op = int.Parse(m.Groups["operand"].Value);
					return new Instruction(mn, op);
				})
				.ToList();

			var runner = new StateTransformer
			{
				Program = program
			};

			var pcsVisited = new HashSet<int>();
			var state = new State(0, 0);
			while (!pcsVisited.Contains(state.ProgramCounter))
			{
				pcsVisited.Add(state.ProgramCounter);
				Console.WriteLine($"PC={state.ProgramCounter} A={state.Accumulator}");
				state = runner.Next(state);
			}

			foreach (var mutation in program
				.Select((instr, idx) => (instr, idx))
				.Where(p =>
					p.instr.OpCode == InstructionCode.Jmp ||
					p.instr.OpCode == InstructionCode.Nop)
			)
			{
				var prog = program.ToList();
				prog[mutation.idx] = prog[mutation.idx] with
				{
					OpCode = mutation.instr.OpCode switch
					{
						InstructionCode.Nop => InstructionCode.Jmp,
						InstructionCode.Jmp => InstructionCode.Nop,
						_ => throw new ArgumentOutOfRangeException()
					}
				};

				var mutRunner = new StateTransformer
				{
					Program = prog
				};
				var mutPcVisited = new HashSet<int>();
				var mutState = new State(0, 0);
				while (mutState.ProgramCounter != prog.Count)
				{
					mutPcVisited.Add(mutState.ProgramCounter);
					mutState = mutRunner.Next(mutState);
					if (mutPcVisited.Contains(mutState.ProgramCounter))
					{
						break;
					}
				}

				if (mutState.ProgramCounter == prog.Count)
				{
					Console.WriteLine("Excecuted Mutation.");
					Console.WriteLine($"PC={mutState.ProgramCounter} A={mutState.Accumulator}");
				}
			}
		}
	}

	public enum InstructionCode
	{
		Nop,
		Acc,
		Jmp,
	}

	public record Instruction(InstructionCode OpCode, int Operand);

	public record State(int ProgramCounter, int Accumulator);
	
	public class StateTransformer
	{
		public IReadOnlyList<Instruction> Program { get; init; }

		public State Next(State state)
		{
			var op = Program[state.ProgramCounter];
			switch (op.OpCode)
			{
				case InstructionCode.Nop:
					return state with
					{
						ProgramCounter = state.ProgramCounter + 1
					};
				case InstructionCode.Acc:
					return state with
					{
						ProgramCounter = state.ProgramCounter + 1,
						Accumulator = state.Accumulator + op.Operand
					};
				case InstructionCode.Jmp:
					return state with
					{
						ProgramCounter = state.ProgramCounter + op.Operand,
					};
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public IEnumerable<State> Execute(State initial)
		{
			var state = initial;
			while (true)
			{
				state = Next(state);
				yield return state;
			}
		}
	}
}