using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Op19
{
	class Program
	{
		static void Main(string[] args)
		{
			var allLines = File.ReadAllLines("input.txt");
			var targets = allLines
				.SkipWhile(l => !string.IsNullOrEmpty(l))
				.Skip(1).ToList();

			
			var ruleText = allLines.TakeWhile(l => !string.IsNullOrEmpty(l)).ToList();
			var rule = ParseRules(ruleText);

				
			var matches = targets.Select(t => rule.Matches(t, out _)).Count(c => c);
			Console.WriteLine(matches);

			ruleText[ruleText.IndexOf("8: 42")] = "8: 42 | 42 8";
			ruleText[ruleText.IndexOf("11: 42 31")] = "11: 42 31 | 42 11 31";
			var rule2 = ParseRules(ruleText);
				
			var matches2 = targets.Select(t => rule2.Matches(t, out _)).ToList();
				
			var count = matches2.Count(c => c);
			Console.WriteLine(count);
		}

		public static Rule ParseRules(IEnumerable<string> rules)
		{
			var basicLine = new Regex("(?'label'[0-9]+): (?'rule'.+)", RegexOptions.Compiled);
			var constantRule = new Regex("\"(?'value'[a-z])\"", RegexOptions.Compiled);
			var sequenceRule = new Regex("((?'label'[0-9]+)[\\s]?)+", RegexOptions.Compiled);
			var branchRule = new Regex("(?'a'[0-9\\s]+)\\s?\\|\\s?(?'b'[0-9\\s]+)", RegexOptions.Compiled);

			var lines = rules
				.Select(r => basicLine.Match(r))
				.ToDictionary(r => r.Groups["label"].Value, r => r.Groups["rule"].Value);

			var parsedLines = new Dictionary<string, Rule>();

			Rule ParseRule(string identifier)
			{
				if (parsedLines.TryGetValue(identifier, out var value)) return value;
				var indirectReference = new IndirectReferenceRule();
				parsedLines.Add(identifier, indirectReference);

				var unparsedLine = lines[identifier];
				var cm = constantRule.Match(unparsedLine);
				if (cm.Success)
				{
					var rule = new ConstantRule(cm.Groups["value"].Value.Single(), identifier);
					indirectReference.Rule = rule;
					return indirectReference;
				}
				
				var br = branchRule.Match(unparsedLine);
				if (br.Success)
				{
					var brA = br.Groups["a"].Value;
					var brB = br.Groups["b"].Value;

					var brAMatch = sequenceRule.Match(brA);
					var rulesA = brAMatch.Groups["label"].Captures.Select(c => c.Value).Select(ParseRule);
					var ruleA = new SequenceRule(rulesA, $"{identifier}*a");
					
					var brBMatch = sequenceRule.Match(brB);
					var rulesB = brBMatch.Groups["label"].Captures.Select(c => c.Value).Select(ParseRule);
					var ruleB = new SequenceRule(rulesB, $"{identifier}*b");

					var rule = new BranchRule(new[] {ruleA, ruleB}, identifier);
					indirectReference.Rule = rule;
					return indirectReference;
				}
				
				var sr = sequenceRule.Match(unparsedLine);
				if (sr.Success)
				{
					var rules = sr.Groups["label"].Captures.Select(c => c.Value).Select(ParseRule);
					var rule = new SequenceRule(rules, identifier);
					indirectReference.Rule = rule;
					return indirectReference;
				}

				throw new Exception("Unmatched rule");
			}

			return new LengthCheckingRule(ParseRule("0"));
		}
	}

	abstract class Rule
	{
		public string? Identifier { get; protected set; }
		public abstract bool Matches(ReadOnlySpan<char> span, out IEnumerable<int> idxAfterMatch);
	}

	class IndirectReferenceRule : Rule
	{
		public Rule Rule { get; set; }

		public IndirectReferenceRule()
		{
			Rule = null; 
		}

		public override bool Matches(ReadOnlySpan<char> span, out IEnumerable<int> idxAfterMatch)
		{
			if (Rule == null)
			{
				idxAfterMatch = null;
				throw new Exception();
			}

			return Rule.Matches(span, out idxAfterMatch);
		}
	}
	
	class LengthCheckingRule : Rule
	{
		public Rule Rule { get; }

		public LengthCheckingRule(Rule r)
		{
			Rule = r;
		}

		public override bool Matches(ReadOnlySpan<char> span, out IEnumerable<int> idxAfterMatch)
		{
			var match = Rule.Matches(span, out idxAfterMatch);
			return match && idxAfterMatch.Contains(span.Length);
		}
	}

	class ConstantRule : Rule
	{
		public char Value { get; }

		public ConstantRule(char ch, string? identifier)
		{
			Value = ch;
			Identifier = identifier;
		}

		public override bool Matches(ReadOnlySpan<char> span, out IEnumerable<int> idxAfterMatch)
		{
			if (!span.IsEmpty && span[0] == Value)
			{
				idxAfterMatch = new[] { 1 };
				return true;
			}

			idxAfterMatch = null;
			return false;
		}
	}

	class SequenceRule : Rule
	{
		public IReadOnlyList<Rule> Parts { get; }
		
		public SequenceRule(IEnumerable<Rule> parts, string? identifier)
		{
			Parts = parts.ToList();
			Identifier = identifier;
		}
		
		public override bool Matches(ReadOnlySpan<char> span, out IEnumerable<int> idxAfterMatch)
		{
			var skipCount = 0;
			var skipOptions = new List<int> { 0 };
			foreach (var rule in Parts)
			{
				var newOptions = (IEnumerable<int>) Array.Empty<int>();
				foreach (var instance in skipOptions)
				{
					if (rule.Matches(span.Slice(instance), out var roundOptions))
					{
						newOptions = newOptions.Concat(roundOptions.Select(opt => opt + instance));
					}
				}
				skipOptions = newOptions.ToList();
				if (!skipOptions.Any())
				{
					idxAfterMatch = null;
					return false;
				}
			}

			idxAfterMatch = skipOptions;
			return true;
		}
	}

	class BranchRule : Rule
	{
		public IReadOnlyList<Rule> Options { get; }

		public BranchRule(IEnumerable<Rule> options, string? identifier)
		{
			Options = options.ToList();
			Identifier = identifier;
		}
		
		public override bool Matches(ReadOnlySpan<char> span, out IEnumerable<int> idxAfterMatch)
		{
			var indexes = (IEnumerable<int>) Array.Empty<int>();
			bool matchedOnce = false;
			foreach (var option in Options)
			{
				if (option.Matches(span, out var skip))
				{
					indexes = indexes.Concat(skip);
					matchedOnce = true;
				}
			}

			if (matchedOnce)
			{
				idxAfterMatch = indexes;
				return true;
			}
			
			idxAfterMatch = null;
			return false;
		}
	}
}