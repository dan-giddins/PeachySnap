using System;
using System.Collections.Generic;
using System.Linq;

namespace PeachySnap
{
	class Program
	{
		// number of cards
		private const int NumberOfCards = 52;
		// half deck (rounded up)
		private const int FirstHalf = (NumberOfCards + 1)/2;
		// half deck (rounded down)
		private const int SecondHalf = NumberOfCards - FirstHalf;
		private const int SuitSize = 13;

		static void Main()
		{
			// generate pack of 'cards'
			var cards = Enumerable.Range(1, NumberOfCards);
			// copy to temporary deck
			var holdingDeck = cards.ToList();
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
			Console.WriteLine("Player A's cards:");
			Console.WriteLine(deckA);
			Console.WriteLine("Player B's cards:");
			Console.WriteLine(deckB);
			// play game
			while (deckA.Any() && deckB.Any())
			{
				roundCounter++;
				Console.WriteLine($"Round {roundCounter}");
				Console.WriteLine("Player A's cards:");
				Console.WriteLine(deckA);
				Console.WriteLine("Player B's cards:");
				Console.WriteLine(deckB);
				Move(deckA, table);
				Move(deckB, table);
			}
		}

		private static void Move(Queue<int> deck, List<int> table)
		{
			// play a card, modded down to 1 - 13
			var card = (deck.Dequeue() - 1) % SuitSize + 1;
			// add card to the table
			table.Add(card);
			// get the index of the card with the same number
			var rangeIndex = table.IndexOf(card);
			// if there is a card on the table that matches
			if (rangeIndex > -1)
			{
				// get the number of cards to take
				var rangeCount = table.Count - rangeIndex;
				// get the range of cards from the table
				var range = table.GetRange(rangeIndex, rangeCount);
				// remove these cards from the table
				table.GetRange(rangeIndex, rangeCount);
				// add cards to hand
				range.ForEach(x => deck.Enqueue(x));
			}
		}
	}
}
