using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PeachySnap
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
		private const int NumberOfGames = 100000;

		static void Main()
		{
			// generate pack of 'cards'
			var cards = Enumerable.Range(1, NumberOfCards);
			var gameCounter = 0;
			var aWins = 0;
			var roundLengths = new List<int>();
			Console.WriteLine($"Playing games...");
			while (gameCounter < NumberOfGames)
			{
				gameCounter++;
				// play game
				(var winner, var roundCounter) = PlayAGame(cards.ToList());
				if (winner == "A")
				{
					aWins++;
				}
				roundLengths.Add(roundCounter);
			}
			Console.WriteLine($"After {NumberOfGames} games...");
			Console.WriteLine($"A won {aWins} times ({aWins / (float) NumberOfGames * 100}%)");
			var min = roundLengths.Min();
			Console.WriteLine($"Quickest round length: {min} ({roundLengths.Where(x => x == min).Count()})");
			Console.WriteLine($"Mode round length: {roundLengths.GroupBy(x => x).OrderByDescending(x => x.Count()).First().Key}");
			Console.WriteLine($"Median round length: {roundLengths.OrderByDescending(x => x).ToList()[NumberOfGames/2]}");
			Console.WriteLine($"Mean round length: {roundLengths.Average()}");
			Console.WriteLine($"Longest round length: {roundLengths.Max()}");
			var csv = string.Empty;
			roundLengths.ForEach(x => csv += $"{x},");
			csv = csv[..^1];
			File.WriteAllText(@"C:\temp\rounds.csv", csv);
		}

		private static (string winner, int roundCounter) PlayAGame(IList<int> holdingDeck)
		{
			// shuffle deck
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
			// get players decks
			var deckA = new Queue<int>(shuffledDeck.GetRange(0, FirstHalf));
			var deckB = new Queue<int>(shuffledDeck.GetRange(FirstHalf, SecondHalf));
			// create table
			var table = new List<int>();
			var roundCounter = 0;
			// play game
			while (deckA.Any() && deckB.Any())
			{
				roundCounter++;
				Move(deckA, table);
				Move(deckB, table);
			}
			return (winner: deckA.Any() ? "A" : "B", roundCounter);
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

		private static void Move(Queue<int> deck, List<int> table)
		{
			// play a card, modded down to 1 - 13
			var card = (deck.Dequeue() - 1) % SuitSize + 1;
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
