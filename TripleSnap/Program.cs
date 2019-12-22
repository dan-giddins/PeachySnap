using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TripleSnap
{
	class Program
	{
		// number of cards
		private const int NumberOfCards = 52;
		// half deck (rounded up)
		private const int FirstHalf = (NumberOfCards + 1) / 2;
		// half deck (rounded down)
		private const int SecondHalf = NumberOfCards - FirstHalf;
		private const int SuitSize = 13;
		//private const int NumberOfGames = 100000;

		static void Main()
		{
			Console.WriteLine($"Welcome to TripleSnap!");
			// generate pack of 'cards'
			// mod cards down to 1 - 13
			var cards = Enumerable.Range(1, NumberOfCards).Select(x => (x - 1) % SuitSize + 1);
			var gameCounter = 0;
			var aWins = 0;
			var roundLengths = new List<int>();
			var exit = false;
			while (!exit)
			{
				gameCounter++;
				// play game
				(var winner, var roundCounter) = PlayAGame(cards.ToList());
				if (winner == "A")
				{
					aWins++;
				}
				roundLengths.Add(roundCounter);
				Console.Write($"Play again? (Y/N): ");
				exit = Console.ReadKey().ToString().ToUpper() == "N" ? true : false;
			}
			PrintStats(gameCounter, aWins, roundLengths);
			WriteCsv(roundLengths);
		}

		private static void WriteCsv(List<int> roundLengths)
		{
			var csv = string.Empty;
			roundLengths.ForEach(x => csv += $"{x},");
			csv = csv[..^1];
			File.WriteAllText(@"C:\temp\rounds.csv", csv);
		}

		private static void PrintStats(int gameCounter, int aWins, List<int> roundLengths)
		{
			Console.WriteLine($"After {gameCounter} games...");
			Console.WriteLine($"A won {aWins} times ({aWins / (float) gameCounter * 100}%)");
			var min = roundLengths.Min();
			Console.WriteLine($"Quickest round length: {min} ({roundLengths.Where(x => x == min).Count()})");
			Console.WriteLine($"Mode round length: {roundLengths.GroupBy(x => x).OrderByDescending(x => x.Count()).First().Key}");
			Console.WriteLine($"Median round length: {roundLengths.OrderByDescending(x => x).ToList()[gameCounter / 2]}");
			Console.WriteLine($"Mean round length: {roundLengths.Average()}");
			Console.WriteLine($"Longest round length: {roundLengths.Max()}");
		}

		private static (string winner, int roundCounter) PlayAGame(IList<int> holdingDeck)
		{
			// shuffle deck
			var shuffledDeck = Shuffle(holdingDeck);
			// get players decks
			var deckA = new Queue<int>(shuffledDeck.GetRange(0, FirstHalf));
			var deckB = new Queue<int>(shuffledDeck.GetRange(FirstHalf, SecondHalf));
			// create table
			var table = new List<int>();
			// create hands
			var handA = new List<int>();
			var handB = new List<int>();
			var roundCounter = 0;
			// draw to three
			NewMethod(deckA, handA);
			// play game
			while (handA.Any() && handB.Any())
			{
				roundCounter++;
				Move(deckA, table, handA);
				Move(deckB, table, handB);
			}
			return (winner: deckA.Any() ? "A" : "B", roundCounter);
		}

		private static void NewMethod(Queue<int> deckA, List<int> handA)
		{
			while (handA.Count < 3 && deckA.Any())
			{
				handA.Add(deckA.Dequeue());
			}
		}

		private static List<int> Shuffle(IList<int> holdingDeck)
		{
			var shuffledDeck = new List<int>();
			var random = new Random();
			for (var i = 0; i < NumberOfCards; i++)
			{
				// get random index
				var j = random.Next(holdingDeck.Count());
				// get card
				var card = holdingDeck[j];
				// remove card from deck
				holdingDeck.RemoveAt(j);
				// add to shuffled deck
				shuffledDeck.Add(card);
			}
			return shuffledDeck;
		}

		private static void PrintDecks(Queue<int> deckA, Queue<int> deckB)
		{
			Console.WriteLine("Player A's cards:");
			foreach (var card in deckA)
			{
				Console.Write($"{card} ");
			}
			Console.WriteLine();
			Console.WriteLine("Player B's cards:");
			foreach (var card in deckB)
			{
				Console.Write($"{card} ");
			}
			Console.WriteLine();
		}

		private static void Move(Queue<int> deck, List<int> table, List<int> hand)
		{
			// get the index of the card with the same number
			var rangeIndex = table.IndexOf(card);
			// add card to the table
			table.Add(card);
			// if there is a card on the table that matches
			if (rangeIndex > -1)
			{
				// get the number of cards to take
				var rangeCount = table.Count - rangeIndex;
				// get the range of cards from the table
				var range = table.GetRange(rangeIndex, rangeCount);
				// remove these cards from the table
				table.RemoveRange(rangeIndex, rangeCount);
				// add cards to hand
				range.ForEach(x => deck.Enqueue(x));
			}
		}
	}
}
