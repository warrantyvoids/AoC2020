using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Op7
{
	class Program
	{
		async static Task Main(string[] args)
		{
			var containsRegex =
				new Regex(
					"(?'tag'[a-z\\s]+)\\sbags\\scontain(?:\\s(?'bagcount'[0-9]+)\\s(?'bagtype'[a-z\\s]+)\\sbag[s]?,)*\\s(?'bagcount'[0-9])\\s(?'bagtype'[a-z\\s]+)\\sbag[s]?\\.",
					RegexOptions.Compiled);
			var noBagsRegex = new Regex("(?'tag'[a-z\\s]+)\\sbags\\scontain\\sno\\sother\\sbags\\.",
				RegexOptions.Compiled);

			using var file = new FileStream("input.txt", FileMode.Open);
			using var reader = new StreamReader(file);

			var bagDescriptors = new List<BagDescriptor>();
			
			while (true)
			{
				var line = await reader.ReadLineAsync();
				if (line == null) break;

				var contents = default(BagContent[]);
				var name = default(string?);
				var match = containsRegex.Match(line);
				if (match.Success)
				{
					name = match.Groups["tag"].Value;
					var countCaptures = match.Groups["bagcount"].Captures;
					var typeCaptures = match.Groups["bagtype"].Captures;
					if (countCaptures.Count != typeCaptures.Count) throw new Exception("Bagcontent mismatch");

					contents = countCaptures
						.Zip(typeCaptures, (a, b) => (count: int.Parse(a.Value), type: b.Value))
						.Select(c => new BagContent(c.count, c.type))
						.ToArray();
				}
				else
				{
					match = noBagsRegex.Match(line);
					if (match.Success)
					{
						name = match.Groups["tag"].Value;
						contents = new BagContent[0];
					}
					else
					{
						throw new Exception("Unknown line");
					}
				}

				bagDescriptors.Add(new BagDescriptor(name, contents));
			}

			var bagNodes = bagDescriptors
				.Select(bd => new BagNode {Name = bd.Name})
				.ToDictionary(b => b.Name, b => b);
			foreach (var descriptor in bagDescriptors)
			{
				var outsideBag = bagNodes[descriptor.Name];
				foreach (var content in descriptor.Content)
				{
					var insideBag = bagNodes[content.Bagname];
					var edge = new BagEdge(outsideBag, insideBag, content.Count);
					outsideBag.OutgoingEdges.Add(edge);
					insideBag.IncomingEdges.Add(edge);
				}
			}

			var workQueue = new Queue<BagNode>();
			var initial = bagNodes["shiny gold"];
			workQueue.Enqueue(initial);
			while (workQueue.Any())
			{
				var entry = workQueue.Dequeue();
				entry.Marked = true;
				foreach (var edge in entry.IncomingEdges)
				{
					if (!edge.ContainingNode.Marked)
					{
						workQueue.Enqueue(edge.ContainingNode);
					}
				}
			}

			var numReached = bagNodes.Count(kvp => kvp.Value.Marked) - 1;
			Console.WriteLine(numReached);

			var bagCounter = 0;
			var contentQueue = new Queue<(BagNode item, int multiplier)>();
			contentQueue.Enqueue((initial,1));
			while (contentQueue.Any())
			{
				var entry = contentQueue.Dequeue();
				foreach (var edge in entry.item.OutgoingEdges)
				{
					bagCounter += edge.Count * entry.multiplier;
					contentQueue.Enqueue((edge.ContentNode, entry.multiplier * edge.Count));
				}
			}
			Console.WriteLine(bagCounter);
		}

		class BagNode
		{
			public string Name { get; init; }
			public bool Marked { get; set; } = false;
			public List<BagEdge> IncomingEdges { get; } = new List<BagEdge>();
			public List<BagEdge> OutgoingEdges { get; } = new List<BagEdge>();
		}

		record BagEdge(BagNode ContainingNode, BagNode ContentNode, int Count);
		
		record BagContent(int Count, string Bagname);
		record BagDescriptor(string Name, BagContent[] Content);
	}
}