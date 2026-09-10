using CoreEngine.Cards;
using System.Collections.Generic;
using System.Linq;

namespace CoreEngine.Game
{
    /// <summary>
    /// End-game scoring algorithms:
    ///   • Destination-ticket completion (with station borrowing)
    ///   • Longest continuous path (European Express bonus)
    ///   • Winner determination with tie-breakers
    /// </summary>
    public static class Scorer
    {
        // ── Destination Tickets ───────────────────────────────────────────────────

        /// <summary>
        /// Returns the net ticket score for <paramref name="player"/>:
        /// +Points for each completed ticket, −Points for each incomplete one.
        /// Applies the station-borrowing rule: each of the player's built stations
        /// (up to 3) may contribute one opponent route incident to that station's city.
        /// We brute-force all combinations of borrowed edges to maximise the net score.
        /// </summary>
        public static int ScoreTickets(Player player, List<Player> allPlayers, List<Route> allRoutes)
        {
            // Collect cities where the player has built stations
            var stationCities = GameManager.Instance.Cities
                .Where(c => c.StationOwner == player)
                .ToList();

            // For each station city, collect all opponent routes incident to it
            // (the player may borrow exactly one route per station city)
            var borrowOptions = new List<List<Route>>();
            foreach (var city in stationCities)
            {
                var opponentRoutes = allRoutes
                    .Where(r => (r.Origin == city || r.Destination == city)
                                && r.ClaimedBy != null
                                && r.ClaimedBy != player)
                    .ToList();
                if (opponentRoutes.Count > 0)
                    borrowOptions.Add(opponentRoutes);  // one entry per station city
            }

            // Brute-force all borrow combinations (≤3 station cities, ≤10 routes each → fast)
            int bestNet = ComputeTicketNet(player, player.ClaimedRoutes, new List<Route>());

            foreach (var borrowed in AllBorrowCombinations(borrowOptions))
            {
                int net = ComputeTicketNet(player, player.ClaimedRoutes, borrowed);
                if (net > bestNet) bestNet = net;
            }

            return bestNet;
        }

        private static int ComputeTicketNet(Player player, List<Route> ownRoutes, List<Route> borrowed)
        {
            // Build adjacency from player's routes + borrowed routes
            var edges = ownRoutes.Concat(borrowed).ToList();
            int net = 0;
            foreach (var ticket in player.TicketsHand)
            {
                bool connected = IsConnected(ticket.Origin, ticket.Destination, edges);
                net += connected ? ticket.Points : -ticket.Points;
            }
            return net;
        }

        /// <summary>Union-Find connectivity check on an edge set.</summary>
        private static bool IsConnected(City a, City b, List<Route> edges)
        {
            if (a == b) return true;

            // Collect all cities in the edge set
            var cities = edges
                .SelectMany(r => new[] { r.Origin, r.Destination })
                .Distinct()
                .ToList();

            if (!cities.Contains(a) || !cities.Contains(b)) return false;

            // BFS
            var visited = new HashSet<City> { a };
            var queue = new Queue<City>();
            queue.Enqueue(a);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current == b) return true;

                foreach (var r in edges)
                {
                    City? neighbour = null;
                    if (r.Origin == current)      neighbour = r.Destination;
                    else if (r.Destination == current) neighbour = r.Origin;

                    if (neighbour != null && !visited.Contains(neighbour))
                    {
                        visited.Add(neighbour);
                        queue.Enqueue(neighbour);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Yields every combination of borrowed routes: one route chosen per
        /// entry in <paramref name="options"/> (or no route for a station city
        /// that ends up not being used).
        /// </summary>
        private static IEnumerable<List<Route>> AllBorrowCombinations(List<List<Route>> options)
        {
            if (options.Count == 0) { yield return new List<Route>(); yield break; }

            // Include "borrow nothing from this station city" as option 0 (null)
            // Cast to nullable so we can prepend null
            var withNone = options
                .Select(lst => lst.Select(r => (Route?)r).Prepend(null).ToList())
                .ToList();

            foreach (var combo in CartesianProduct(withNone))
            {
                yield return combo.Where(r => r != null).Select(r => r!).ToList();
            }
        }

        private static IEnumerable<List<Route?>> CartesianProduct(List<List<Route?>> sequences)
        {
            IEnumerable<List<Route?>> seed = new[] { new List<Route?>() };
            return sequences.Aggregate(seed, (acc, seq) =>
                acc.SelectMany(existing =>
                    seq.Select(item =>
                        existing.Concat(new[] { item }).ToList())));
        }

        // ── Completed ticket count (for tie-breaking / display) ───────────────────

        /// <summary>
        /// Simple ticket completion count without station borrowing
        /// (used for display and tie-breaking only).
        /// </summary>
        public static int CompletedTicketCount(Player player)
        {
            int count = 0;
            foreach (var ticket in player.TicketsHand)
                if (IsConnected(ticket.Origin, ticket.Destination, player.ClaimedRoutes))
                    count++;
            return count;
        }

        // ── Longest Continuous Path ───────────────────────────────────────────────

        /// <summary>
        /// Returns the longest trail length (in route-segments sum, i.e. train cars placed)
        /// per player. Stations and borrowed routes are excluded.
        /// </summary>
        public static Dictionary<Player, int> FindLongestPaths(List<Player> players)
        {
            var result = new Dictionary<Player, int>();
            foreach (var player in players)
                result[player] = FindLongestTrail(player.ClaimedRoutes);
            return result;
        }

        /// <summary>
        /// Backtracking DFS to find the longest trail in an undirected multigraph
        /// (each edge — route segment — may be used only once, but cities may be revisited).
        /// Returns the maximum sum of route lengths across any trail.
        /// </summary>
        private static int FindLongestTrail(List<Route> routes)
        {
            if (routes.Count == 0) return 0;

            var cities = routes
                .SelectMany(r => new[] { r.Origin, r.Destination })
                .Distinct()
                .ToList();

            int best = 0;
            var usedEdges = new bool[routes.Count];

            void Dfs(City current, int pathLength)
            {
                bool extended = false;
                for (int i = 0; i < routes.Count; i++)
                {
                    if (usedEdges[i]) continue;
                    var r = routes[i];
                    City? next = null;
                    if (r.Origin == current)      next = r.Destination;
                    else if (r.Destination == current) next = r.Origin;
                    if (next == null) continue;

                    extended = true;
                    usedEdges[i] = true;
                    Dfs(next, pathLength + r.Length);
                    usedEdges[i] = false;
                }
                // Update best at every node (handles both dead-ends and partial paths)
                if (pathLength > best)
                    best = pathLength;
            }

            foreach (var start in cities)
                Dfs(start, 0);

            return best;
        }

        // ── Winner determination ──────────────────────────────────────────────────

        /// <summary>
        /// Returns the winner using tie-breakers:
        ///   1. Highest total score
        ///   2. Most completed destination tickets
        ///   3. Fewest stations used (most stations remaining)
        ///   4. Has the European Express (longest path) — resolved outside; same winner returned
        /// </summary>
        public static Player DetermineWinner(List<Player> players, List<Route> allRoutes)
        {
            var all = players.ToList();
            var paths = FindLongestPaths(all);

            return all
                .OrderByDescending(p => p.Score)
                .ThenByDescending(p => CompletedTicketCount(p))
                .ThenByDescending(p => p.StationsRemaining)   // more remaining = fewer used
                .ThenByDescending(p => paths[p])              // European Express holder
                .First();
        }
    }
}
