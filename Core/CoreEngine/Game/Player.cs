using System.Collections;
using CoreEngine.Cards;
using CoreEngine.Helpers;

namespace CoreEngine.Game
{
    public delegate int PlayerChoiceDelegate(IEnumerable choices);

    public class Player
    {
        // ── Identity ──────────────────────────────────────────────────────────────
        public PlayerColor Color { get; init; }

        // ── Input ─────────────────────────────────────────────────────────────────
        public PlayerChoiceDelegate InputFunc { get; init; }

        // ── Cards in hand ────────────────────────────────────────────────────────
        public List<TicketCard> TicketsHand { get; private set; } = new();
        public List<TrainCard> TrainsHand { get; private set; } = new();

        // ── Tokens ───────────────────────────────────────────────────────────────
        /// <summary>Plastic train cars remaining (start: 45, decremented when claiming routes).</summary>
        public int TrainsRemaining { get; internal set; } = 45;

        /// <summary>Train stations not yet built (start: 3, decremented when building).</summary>
        public int StationsRemaining { get; internal set; } = 3;

        /// <summary>Number of stations already built (0–3).</summary>
        public int StationsBuilt => 3 - StationsRemaining;

        // ── Score ─────────────────────────────────────────────────────────────────
        /// <summary>Running score (immediate route points). Final score is set after end-game scoring.</summary>
        public int Score { get; internal set; } = 0;

        // ── Board state ───────────────────────────────────────────────────────────
        public List<Route> ClaimedRoutes { get; private set; } = new();

        // ── Constructor ──────────────────────────────────────────────────────────
        public Player(PlayerChoiceDelegate inputFunc, PlayerColor color = PlayerColor.Red)
        {
            InputFunc = inputFunc;
            Color = color;
        }

        // ── Card helpers ─────────────────────────────────────────────────────────

        /// <summary>
        /// Returns true if the player can form a payment with <paramref name="n"/> cards of the same color
        /// (counting Locomotives as wild), used by <see cref="BuildTrainStationGA"/>.
        /// </summary>
        internal bool HasNCardOfSameColor(int n)
        {
            var colorMap = Utils.GetCardsColorMap(TrainsHand);
            int locos = colorMap.TryGetValue(TrainColor.Locomotive, out int lc) ? lc : 0;
            int maxNonLoco = colorMap
                .Where(kv => kv.Key != TrainColor.Locomotive)
                .Select(kv => kv.Value)
                .DefaultIfEmpty(0)
                .Max();
            return maxNonLoco + locos >= n;
        }

        /// <summary>
        /// Removes the cards specified by <paramref name="spend"/> from the player's hand
        /// and pushes them onto the discard pile. Throws if the hand does not contain them.
        /// </summary>
        public void CommitSpend(SpendOption spend)
        {
            var toDiscard = new List<TrainCard>();

            // collect color cards
            if (spend.ColorCards > 0 && spend.Color != TrainColor.Locomotive)
            {
                int found = 0;
                foreach (var card in TrainsHand)
                {
                    if (card.Color == spend.Color && found < spend.ColorCards)
                    {
                        toDiscard.Add(card);
                        found++;
                    }
                }
                if (found < spend.ColorCards)
                    throw new InvalidOperationException(
                        $"Player does not have {spend.ColorCards}× {spend.Color} in hand.");
            }

            // collect locomotive cards
            if (spend.Locomotives > 0)
            {
                int found = 0;
                foreach (var card in TrainsHand)
                {
                    if (card.IsLocomotive && !toDiscard.Contains(card) && found < spend.Locomotives)
                    {
                        toDiscard.Add(card);
                        found++;
                    }
                }
                if (found < spend.Locomotives)
                    throw new InvalidOperationException(
                        $"Player does not have {spend.Locomotives}× Locomotive in hand.");
            }

            foreach (var card in toDiscard)
            {
                TrainsHand.Remove(card);
                GameManager.Instance.UsedCardsDeck.AddOnBottom(card);
            }
        }

        public override string ToString() => $"Player({Color})";
    }
}
