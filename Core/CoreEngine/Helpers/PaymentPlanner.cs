using CoreEngine.Cards;

namespace CoreEngine.Helpers
{
    /// <summary>One concrete way to pay for a route claim.</summary>
    /// <param name="Color">
    ///   The non-locomotive color being played.
    ///   <see cref="TrainColor.Locomotive"/> when paying with locomotives only.
    /// </param>
    /// <param name="ColorCards">Number of non-locomotive cards of <see cref="Color"/> to spend.</param>
    /// <param name="Locomotives">Number of Locomotive cards to spend.</param>
    public record SpendOption(TrainColor Color, int ColorCards, int Locomotives)
    {
        public string Description =>
            (ColorCards == 0)
                ? $"{Locomotives}× Locomotive"
                : (Locomotives == 0)
                    ? $"{ColorCards}× {Color}"
                    : $"{ColorCards}× {Color} + {Locomotives}× Locomotive";
    }

    /// <summary>
    /// Pure (side-effect-free) helper that enumerates valid payment combinations
    /// for claiming a route, respecting Locomotive wild-card rules and minimum loco
    /// requirements for ferry routes.
    /// </summary>
    public static class PaymentPlanner
    {
        /// <summary>
        /// Returns every distinct <see cref="SpendOption"/> that satisfies the route cost
        /// given the player's current hand.
        /// </summary>
        /// <param name="hand">Player's current train cards.</param>
        /// <param name="routeLength">Total spaces in the route.</param>
        /// <param name="routeColor">
        ///   Required color. <see cref="TrainColor.Wild"/> = gray route (player picks any color).
        /// </param>
        /// <param name="minLocomotives">Hard minimum locomotives (> 0 for ferry routes).</param>
        public static List<SpendOption> EnumerateSpends(
            List<TrainCard> hand,
            int routeLength,
            TrainColor routeColor,
            int minLocomotives = 0)
        {
            var colorMap = Utils.GetCardsColorMap(hand);
            int locoCount = colorMap.TryGetValue(TrainColor.Locomotive, out int lc) ? lc : 0;

            var options = new List<SpendOption>();

            // ── All-locomotive payment ──────────────────────────────────────────────────
            if (locoCount >= routeLength && routeLength >= minLocomotives)
                options.Add(new SpendOption(TrainColor.Locomotive, 0, routeLength));

            // ── Mixed color + locomotive payments ──────────────────────────────────────
            IEnumerable<TrainColor> candidateColors = routeColor == TrainColor.Wild
                ? colorMap.Keys.Where(c => c != TrainColor.Locomotive)
                : Enumerable.Repeat(routeColor, 1);

            foreach (TrainColor color in candidateColors)
            {
                int held = colorMap.TryGetValue(color, out int hc) ? hc : 0;

                // usedColor must be at least 1 (all-loco handled above)
                for (int usedColor = 1; usedColor <= held; usedColor++)
                {
                    int locosNeeded = routeLength - usedColor;
                    if (locosNeeded < 0) continue;
                    if (locosNeeded < minLocomotives) continue;
                    if (locosNeeded > locoCount) continue;
                    options.Add(new SpendOption(color, usedColor, locosNeeded));
                }
            }

            return options
                .Distinct()
                .OrderBy(o => o.Locomotives)
                .ThenBy(o => o.Color.ToString())
                .ToList();
        }

        /// <summary>Returns true if the player can afford the route at all.</summary>
        public static bool CanAfford(
            List<TrainCard> hand,
            int routeLength,
            TrainColor routeColor,
            int minLocomotives = 0)
            => EnumerateSpends(hand, routeLength, routeColor, minLocomotives).Count > 0;
    }
}
