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
			var programs = (await lines)
				.Select(l => regex.Match(l))
				.Select(m =>
				{
					
				});
		}
	}

	public enum InstructionCode
	{
		Nop,
		Acc,
		Jmp,
	}

	public record Instruction(InstructionCode OpCode, int Operand);
	
	public class StateMachine
	{
		public int ProgramCounter { get; set; }
		public int Accumulator { get; set; }
		
		public List<Instruction> Program { get; set; }

		public void Execute(int instructions)
		{
			for (int i = 0; i < instructions; i++)
			{
				var currInstr = Program[ProgramCounter];
				switch (currInstr.OpCode)
				{
					case InstructionCode.Nop:
						ProgramCounter++;
						break;
					case InstructionCode.Acc:
						ProgramCounter++;
						Accumulator += currInstr.Operand;
						break;
					case InstructionCode.Jmp:
						ProgramCounter += currInstr.Operand;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}
	}
}