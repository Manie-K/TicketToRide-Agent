using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreEngine
{
    public class CardDeck<T> where T : Card
    {
        private readonly Queue<T> cards;


        public int DeckSize => cards.Count;
        public IEnumerable<T> AllCards => cards.AsEnumerable();


        public event Action OnDeckDepleted;


        public CardDeck(List<T> initialCards)
        {
            cards = new Queue<T>();
            Init(initialCards);
        }


        public void Init(List<T> initialCards)
        {
            foreach (T card in initialCards) 
            {
               cards.Enqueue(card);
            }

            Shuffle();
        }

        public void AddOnBottom(T card) 
        {
            if (card  == null)
                throw new ArgumentNullException(nameof(card));
            cards.Enqueue(card);
        }

        public T? DrawCard()
        {
            if (cards.Count == 0)
                return null;

            if (cards.Count == 1)
                OnDeckDepleted?.Invoke();

            return cards.Dequeue();
        }


        private void Shuffle()
        {
            var rng = new Random();

            var shuffled = cards
                .OrderBy(_ => rng.Next())
                .ToList();

            cards.Clear();

            foreach (var card in shuffled)
                cards.Enqueue(card);
        }

    }

}
