using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Op18
{
	public static class LexerP2
	{
		public static AstNode Lex(IEnumerable<ParseToken> tokens)
		{
			var tokenList = tokens.ToList();
			var parsed = Next(tokenList);
			if (parsed.numRead != tokenList.Count)
			{
				throw new Exception("Unconsumed tokens?");
			}

			return parsed.node;
		}
		
		public static (AstNode node, int numRead) Next(IEnumerable<ParseToken> tokens)
		{
			var originalTokens = tokens.ToList();
			var toParse = originalTokens;
			var (node, skipCount) = NextValue(toParse);
			toParse = toParse.Skip(skipCount).ToList();

			var valueList = new List<AstNode> { node };
			var operators = new List<ParseToken>();
			
			while (toParse.Any())
			{
				if (toParse[0].Type != ParseTokenType.Add && toParse[0].Type != ParseTokenType.Multiply)
					throw new Exception("Unexpected value.");
				
				operators.Add(toParse[0]);
				toParse = toParse.Skip(1).ToList();
				(node, skipCount) = NextValue(toParse);
				valueList.Add(node);
				toParse = toParse.Skip(skipCount).ToList();
			}

			foreach (var opToReplace in operators
				.Select((op, idx) => (op.Type, idx))
				.Reverse()
				.Where(p => p.Type == ParseTokenType.Add)
				.ToList())
			{
				var lhs = valueList[opToReplace.idx];
				var rhs = valueList[opToReplace.idx + 1];

				var newOp = new AstAdd(lhs, rhs);
				operators.RemoveAt(opToReplace.idx);
				valueList.RemoveAt(opToReplace.idx + 1);
				valueList[opToReplace.idx] = newOp;
			}

			var firstValue = valueList.First();
			return (valueList.Skip(1).Aggregate(firstValue, (lhs, rhs) => new AstMultiply(lhs, rhs)), originalTokens.Count);
		}

		public static (AstNode node, int numRead) NextValue(List<ParseToken> tokens)
		{
			if (tokens[0].Type == ParseTokenType.Constant)
			{
				return (new AstConstant(ulong.Parse(tokens[0].Value)), 1);
			}
			if (tokens[0].Type == ParseTokenType.OpenParenthesis)
			{
				var openFound = 1;
				var ctr = 1;
				foreach (var token in tokens.Skip(1))
				{
					if (token.Type == ParseTokenType.OpenParenthesis)
					{
						openFound++;
					} else if (token.Type == ParseTokenType.CloseParenthesis)
					{
						openFound--;
						if (openFound == 0)
						{
							return (new AstParenthesis(Next(tokens.Skip(1).Take(ctr - 1)).node), ctr + 1);
						}
					}
					ctr++;
				}
			}

			throw new Exception("Invalid parse state.");
		}
	}

	public static class LexerP1
	{
		public static AstNode Lex(IEnumerable<ParseToken> tokens)
		{
			var tokenList = tokens.ToList();
			var parsed = Next(tokenList);
			if (parsed.numRead != tokenList.Count)
			{
				throw new Exception("Unconsumed tokens?");
			}

			return parsed.node;
		}
		
		public static (AstNode node, int numRead) Next(IEnumerable<ParseToken> tokens)
		{
			var originalTokens = tokens.ToList();
			var toParse = originalTokens;
			var (lhs, skipCount) = NextValue(toParse);
			toParse = toParse.Skip(skipCount).ToList();
			while (toParse.Any())
			{
				if (toParse[0].Type == ParseTokenType.Add || toParse[0].Type == ParseTokenType.Multiply)
				{
					var type = toParse[0].Type;
					toParse = toParse.Skip(1).ToList();
					var rhs = NextValue(toParse);
					if (type == ParseTokenType.Add)
					{
						lhs = new AstAdd(lhs, rhs.node);
					}
					else
					{
						lhs = new AstMultiply(lhs, rhs.node);
					}

					toParse = toParse.Skip(rhs.numRead).ToList();
				}
				else throw new Exception("Unexpected token type.");
			}

			return (lhs, originalTokens.Count);
		}

		public static (AstNode node, int numRead) NextValue(List<ParseToken> tokens)
		{
			if (tokens[0].Type == ParseTokenType.Constant)
			{
				return (new AstConstant(ulong.Parse(tokens[0].Value)), 1);
			}
			if (tokens[0].Type == ParseTokenType.OpenParenthesis)
			{
				var openFound = 1;
				var ctr = 1;
				foreach (var token in tokens.Skip(1))
				{
					if (token.Type == ParseTokenType.OpenParenthesis)
					{
						openFound++;
					} else if (token.Type == ParseTokenType.CloseParenthesis)
					{
						openFound--;
						if (openFound == 0)
						{
							return (new AstParenthesis(Next(tokens.Skip(1).Take(ctr - 1)).node), ctr + 1);
						}
					}
					ctr++;
				}
			}

			throw new Exception("Invalid parse state.");
		}
	}
	
	public enum ParseTokenType
	{
		None,
		Constant,
		Add,
		Multiply,
		OpenParenthesis,
		CloseParenthesis
	}

	public record ParseToken(ParseTokenType Type, string Value);
	
	public static class Tokenizer
	{
		public static IEnumerable<ParseToken> Tokenize(string input)
		{
			var currentTokenType = ParseTokenType.None;
			var builder = new StringBuilder();
			for (int i = 0; i < input.Length; i++)
			{
				var nextChar = input[i];
				if (char.IsDigit(nextChar))
				{
					if (currentTokenType != ParseTokenType.Constant)
					{
						if (currentTokenType != ParseTokenType.None)
						{
							yield return new ParseToken(currentTokenType, builder.ToString());
							builder.Clear();
						}

						currentTokenType = ParseTokenType.Constant;
					}
					builder.Append(nextChar);
				}
				else if (nextChar == '+')
				{
					if (currentTokenType != ParseTokenType.None)
					{
						yield return new ParseToken(currentTokenType, builder.ToString());
						builder.Clear();
					}
					yield return new ParseToken(ParseTokenType.Add, "+");
					currentTokenType = ParseTokenType.None;
				} 
				else if (nextChar == '*')
				{
					if (currentTokenType != ParseTokenType.None)
					{
						yield return new ParseToken(currentTokenType, builder.ToString());
						builder.Clear();
					}
					yield return new ParseToken(ParseTokenType.Multiply, "*");
					currentTokenType = ParseTokenType.None;
				}
				else if (nextChar == '(')
				{
					if (currentTokenType != ParseTokenType.None)
					{
						yield return new ParseToken(currentTokenType, builder.ToString());
						builder.Clear();
					}
					yield return new ParseToken(ParseTokenType.OpenParenthesis, "(");
					currentTokenType = ParseTokenType.None;
				}
				else if (nextChar == ')')
				{
					if (currentTokenType != ParseTokenType.None)
					{
						yield return new ParseToken(currentTokenType, builder.ToString());
						builder.Clear();
					}
					yield return new ParseToken(ParseTokenType.CloseParenthesis, ")");
					currentTokenType = ParseTokenType.None;
				}
				else if (char.IsWhiteSpace(nextChar))
				{
					if (currentTokenType != ParseTokenType.None)
					{
						yield return new ParseToken(currentTokenType, builder.ToString());
						builder.Clear();
						currentTokenType = ParseTokenType.None;
					}
				}
				else
				{
					throw new Exception($"Unknown character {nextChar} at position {i}.");
				}
			}

			if (currentTokenType != ParseTokenType.None)
			{
				yield return new ParseToken(currentTokenType, builder.ToString());
			}
		}
	}
}