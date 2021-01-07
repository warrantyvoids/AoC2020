using System;
using System.IO;
using System.Linq;

namespace Op18
{
	class Program
	{
		static void Main(string[] args)
		{
			var lines = File.ReadAllLines("input.txt").Select(line => LexerP1.Lex(Tokenizer.Tokenize(line)).Value);
			Console.WriteLine(lines.Aggregate(0ul, (a,b) => (a+b)));
			var linesP2 = File.ReadAllLines("input.txt").Select(line => LexerP2.Lex(Tokenizer.Tokenize(line)).Value);
			Console.WriteLine(linesP2.Aggregate(0ul, (a,b) => (a+b)));
		}
	}
}