using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreEngine.Cards
{
    public class CardDeck<T> where T : Card
    {
        private readonly Queue<T> cards;
        private readonly Random rng;

        public int DeckSize => cards.Count;
        public IEnumerable<T> AllCards => cards.AsEnumerable();

        /// <summary>Fired when the last card has been drawn (deck is now empty).</summary>
        public event Action? OnDeckDepleted;

        /// <param name="initialCards">Starting cards. Shuffled immediately.</param>
        /// <param name="rng">
        ///   Shared <see cref="Random"/> instance. Pass the same instance to all decks
        ///   and to the <see cref="Game.GameManager"/> so that games are reproducible
        ///   when the seed is fixed.
        /// </param>
        public CardDeck(List<T> initialCards, Random? rng = null)
        {
            this.rng = rng ?? new Random();
            cards = new Queue<T>();
            Init(initialCards);
        }

        public void Init(List<T> initialCards)
        {
            foreach (T card in initialCards)
                cards.Enqueue(card);
            Shuffle();
        }

        public void AddOnBottom(T card)
        {
            if (card == null) throw new ArgumentNullException(nameof(card));
            cards.Enqueue(card);
        }

        /// <summary>Draws the top card. Returns <c>null</c> when the deck is empty and fires <see cref="OnDeckDepleted"/>.</summary>
        public T? DrawCard()
        {
            if (cards.Count == 0)
            {
                OnDeckDepleted?.Invoke();
                return null;
            }
            return cards.Dequeue();
        }

        /// <summary>
        /// Moves every card from <paramref name="discardPile"/> into this deck (clearing the discard pile),
        /// then shuffles this deck.
        /// </summary>
        public void ReshuffleFrom(CardDeck<T> discardPile)
        {
            var toAdd = discardPile.cards.ToList();
            discardPile.cards.Clear();
            foreach (var card in toAdd)
                cards.Enqueue(card);
            Shuffle();
        }

        private void Shuffle()
        {
            var shuffled = cards.OrderBy(_ => rng.Next()).ToList();
            cards.Clear();
            foreach (var card in shuffled)
                cards.Enqueue(card);
        }
    }
}
