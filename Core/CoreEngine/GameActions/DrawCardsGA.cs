using CoreEngine.Cards;
using CoreEngine.Game;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreEngine.GameActions
{
    /// <summary>
    /// Draw Train Cards action.
    ///
    /// A player may draw two Train cards per turn, either from the five face-up
    /// cards or blindly from the top of the deck.
    ///
    /// Special rules:
    /// - Picking a face-up Locomotive counts as BOTH draws (turn ends after one pick).
    /// - A Locomotive drawn blindly counts as just one of the two draws.
    /// - If fewer than 2 total cards are available, take what is available.
    /// - After drawing a face-up card, the slot is immediately refilled.
    /// - If ≥3 of the 5 face-up cards are Locomotives, all 5 are discarded and redrawn.
    /// 
    /// Architecture update:
    /// - Converted to event-driven flow (SetPendingChoices) for RL agents.
    /// </summary>
    public class DrawCardsGA : GameAction
    {
        public static readonly DrawCardsGA Instance = new DrawCardsGA();

        private DrawCardsGA()
        {
            Name = "Draw Train Cards";
            Description = "Draw up to 2 train cards from the face-up market or the deck.";
        }

        public override bool CanExecute(Player currentPlayer)
        {
            var gm = GameManager.Instance;
            bool marketHasCards = gm.FaceUpCards.Any(c => c != null);
            bool deckHasCards = gm.TrainsDeck.DeckSize > 0 || gm.UsedCardsDeck.DeckSize > 0;
            return marketHasCards || deckHasCards;
        }

        public override void Execute(Player currentPlayer)
        {
            DrawStep(GameManager.Instance, currentPlayer, isFirstDraw: true);
        }

        // ─────────────────────────────────────────────────────────────────────────
        private void DrawStep(GameManager gm, Player player, bool isFirstDraw)
        {
            // Build choice list: face-up slots + blind deck
            var choices = BuildDrawChoices(gm, allowFaceUpLoco: isFirstDraw);

            // Edge case: Deck and market completely empty mid-turn
            if (choices.Count == 0)
            {
                gm.EndTurn();
                return;
            }

            var callbacks = new List<Action>();

            foreach (var choice in choices)
            {
                var chosen = (DrawChoice)choice.value!;

                callbacks.Add(() =>
                {
                    HandleDrawResolution(gm, player, chosen, isFirstDraw);
                });
            }

            // Suspend the game and wait for the agent/human to pick a card
            gm.SetPendingChoices(choices, idx => callbacks[idx]());
        }

        private void HandleDrawResolution(GameManager gm, Player player, DrawChoice chosen, bool isFirstDraw)
        {
            if (chosen.IsDeck)
            {
                // Blind draw — locomotives drawn blind still allow a second pick
                var card = gm.DrawFromMainDeck();
                if (card != null)
                {
                    player.TrainsHand.Add(card);
                    Console.WriteLine($"  {player} drew a card blindly from the deck.");
                }

                // Trigger a second draw if this was the first one and cards are still available
                if (isFirstDraw && HasCardsAvailable(gm))
                {
                    DrawStep(gm, player, isFirstDraw: false);
                }
                else
                {
                    gm.EndTurn();
                }
            }
            else
            {
                // Face-up draw
                var card = gm.FaceUpCards[chosen.SlotIndex]!;
                player.TrainsHand.Add(card);
                gm.FaceUpCards[chosen.SlotIndex] = null;

                Console.WriteLine($"  {player} drew a face-up {card.Color}.");

                gm.RefillFaceUpCards();

                // Face-up Locomotive consumes both draws (ends turn).
                // Otherwise, if it's the first draw, proceed to the second draw.
                if (!card.IsLocomotive && isFirstDraw && HasCardsAvailable(gm))
                {
                    DrawStep(gm, player, isFirstDraw: false);
                }
                else
                {
                    gm.EndTurn();
                }
            }
        }

        private static List<PlayerChoice> BuildDrawChoices(GameManager gm, bool allowFaceUpLoco)
        {
            var choices = new List<PlayerChoice>();

            for (int i = 0; i < gm.FaceUpCards.Count; i++)
            {
                var card = gm.FaceUpCards[i];
                if (card == null) continue;
                if (card.IsLocomotive && !allowFaceUpLoco) continue;

                string label = card.IsLocomotive
                    ? $"[Slot {i}] Locomotive (face-up) {(allowFaceUpLoco ? "— ends turn" : "")}"
                    : $"[Slot {i}] {card.Color} (face-up)";

                choices.Add(new PlayerChoice(new DrawChoice(SlotIndex: i, IsDeck: false), label));
            }

            bool deckAvailable = gm.TrainsDeck.DeckSize > 0 || gm.UsedCardsDeck.DeckSize > 0;
            if (deckAvailable)
                choices.Add(new PlayerChoice(new DrawChoice(SlotIndex: -1, IsDeck: true), "Blind draw from deck"));

            return choices;
        }

        private static bool HasCardsAvailable(GameManager gm) =>
            gm.FaceUpCards.Any(c => c != null) ||
            gm.TrainsDeck.DeckSize > 0 ||
            gm.UsedCardsDeck.DeckSize > 0;

        // ─────────────────────────────────────────────────────────────────────────
        private record DrawChoice(int SlotIndex, bool IsDeck);
    }
}