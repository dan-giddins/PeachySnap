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
		private const int HandSize = 3;
		private const bool AutoPlay = false;
		private const bool PlayCpu = true;

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
				Console.WriteLine($"\nGame {gameCounter}");
				// play game
				(var winner, var roundCounter) = PlayAGame(cards.ToList());
				if (winner == "A")
				{
					aWins++;
				}
				Console.WriteLine($"\nPlayer {winner} wins!");
				roundLengths.Add(roundCounter);
				Console.Write($"Play again? (Y/N): ");
				var valid = false;
				while (!valid)
				{
					var input = Console.ReadLine().ToUpper();
					if (input == "Y")
					{
						exit = false;
						valid = true;
					}
					else if (input == "N")
					{
						exit = true;
						valid = true;
					}
					else
					{
						Console.Write("Please enter either Y or N:");
					}
				}
			}
			PrintStats(gameCounter, aWins, roundLengths);
			WriteCsv(roundLengths, @"C:\temp\rounds.csv");
		}

		private static void WriteCsv(List<int> roundLengths, string path)
		{
			Console.WriteLine($"Writing round lenghts to {path}...");
			File.WriteAllText(path, string.Join(",", roundLengths)[..^1]);
		}

		private static void PrintStats(int gameCounter, int aWins, List<int> roundLengths)
		{
			Console.WriteLine($"\nAfter {gameCounter} games...");
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
			DrawToFull(deckA, handA);
			DrawToFull(deckB, handB);
			// play game
			while (handA.Any() && handB.Any())
			{
				roundCounter++;
				Move(deckA, table, handA, "A");
				Move(deckB, table, handB, "B");
			}
			return (winner: deckA.Any() ? "A" : "B", roundCounter);
		}

		private static void DrawToFull(Queue<int> deck, List<int> hand)
		{
			while (hand.Count < HandSize && deck.Any())
			{
				hand.Add(deck.Dequeue());
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
			Console.WriteLine($"Player A's cards: {string.Join(", ", deckA)}");
			Console.WriteLine($"Player B's cards: {string.Join(", ", deckB)}");
		}

		private static void Move(Queue<int> deck, List<int> table, List<int> hand, string player)
		{
			int index;
			if (!AutoPlay && !(PlayCpu && player == "B") && hand.Count() > 1)
			{
				Console.WriteLine($"\nTable: {string.Join(", ", table)}");
				Console.WriteLine($"Hand: {string.Join(", ", hand)}");
				Console.Write($"Player {player} to move (1-{hand.Count()}): ");
				var valid = false;
				var input = 0;
				while (!valid)
				{
					if (int.TryParse(Console.ReadLine(), out input))
					{
						if (input > 0 && input < hand.Count() + 1)
						{
							valid = true;
						}
						else
						{
							Console.Write($"Please enter a valid number between 1 and {hand.Count()}: ");
						}
					}
					else
					{
						Console.Write("Please enter a number: ");
					}
				}
				index = input - 1;
			}
			else
			{
				index = 0;
			}
			var card = hand[index];
			hand.RemoveAt(index);
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
			// draw back up to three
			DrawToFull(deck, hand);
		}
	}
}
