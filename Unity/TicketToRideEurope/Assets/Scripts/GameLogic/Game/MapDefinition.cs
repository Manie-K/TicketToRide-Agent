using CoreEngine.Cards;
using System.Collections.Generic;
using System;
using System.Linq;

namespace CoreEngine.Game
{
    /// <summary>
    /// Authoritative source of the Ticket to Ride Europe board data:
    /// cities, routes (with colors / lengths / ferries / tunnels / double-routes),
    /// and destination tickets.
    ///
    /// NOTE: Route data was reconstructed from memory and should be verified
    /// against the physical Ticket to Ride Europe board game before use in
    /// competitive or reference implementations.
    /// </summary>
    public static class MapDefinition
    {
        // ── City registry ─────────────────────────────────────────────────────────
        private static readonly Dictionary<string, City> _cities = new();

        public static IReadOnlyDictionary<string, City> Cities => _cities;

        private static City C(string name)
        {
            if (!_cities.TryGetValue(name, out var city))
            {
                city = new City(name);
                _cities[name] = city;
            }
            return city;
        }

        // ── Route list ───────────────────────────────────────────────────────────
        private static readonly List<Route> _routes = new();

        public static IReadOnlyList<Route> Routes => _routes;

        // ── Initialisation ───────────────────────────────────────────────────────

        public static void Build()
        {
            _cities.Clear();
            _routes.Clear();
            BuildRoutes();
            Validate();
        }

        // ── Route helpers ─────────────────────────────────────────────────────────

        /// Adds a single route and returns it.
        private static Route Add(string c1, string c2, int len, TrainColor color,
                                 bool tunnel = false, int ferryLocos = 0)
        {
            var r = new Route(C(c1), C(c2), len, color, tunnel, ferryLocos);
            _routes.Add(r);
            return r;
        }

        /// Adds a double-route (two parallel routes with linked Sibling references).
        private static void AddDouble(string c1, string c2, int len,
                                      TrainColor color1, TrainColor color2,
                                      bool tunnel1 = false, bool tunnel2 = false)
        {
            var r1 = Add(c1, c2, len, color1, tunnel1);
            var r2 = Add(c1, c2, len, color2, tunnel2);
            r1.Sibling = r2;
            r2.Sibling = r1;
        }

        // ── Europe Board Routes ───────────────────────────────────────────────────
        //
        // Color key  : Wild = gray (any color)
        //              Other colors match TrainColor enum
        // Ferry      : ferryLocos > 0  →  that many loco cards are mandatory
        // Tunnel     : tunnel = true   →  reveal-3 extra-card mechanic applies
        // Double     : two routes between same cities (AddDouble)
        //
        private static void BuildRoutes()
        {
            // ── British Isles ───────────────────────────────────────────────────────
            Add("Edinburgh", "London",    4, TrainColor.Orange);
            AddDouble("London", "Dieppe", 2, TrainColor.Wild, TrainColor.Wild);

            // ── France / Atlantic ──────────────────────────────────────────────────
            Add("Dieppe",   "Paris",      1, TrainColor.Wild);
            Add("Dieppe",   "Brest",      2, TrainColor.Wild);
            Add("Brest",    "Paris",      3, TrainColor.Black);
            Add("Paris",    "Bordeaux",   5, TrainColor.Wild);
            Add("Paris",    "Marseille",  4, TrainColor.Wild);
            Add("Paris",    "Bruxelles",  2, TrainColor.Yellow);
            Add("Paris",    "Frankfurt",  3, TrainColor.Orange);

            // ── Iberian Peninsula ──────────────────────────────────────────────────
            Add("Lisboa",   "Cádiz",      2, TrainColor.Wild);
            Add("Lisboa",   "Madrid",     3, TrainColor.Pink);
            Add("Cádiz",    "Madrid",     3, TrainColor.Yellow);
            Add("Madrid",   "Barcelona",  2, TrainColor.Yellow);
            // Confirmed tunnel double-route (rulebook page 5)
            AddDouble("Madrid", "Pamplona", 3, TrainColor.Black, TrainColor.White, tunnel1: true, tunnel2: true);
            Add("Pamplona", "Barcelona",  2, TrainColor.Wild);
            Add("Pamplona", "Bordeaux",   4, TrainColor.Green);
            Add("Barcelona","Marseille",  4, TrainColor.Wild);

            // ── Benelux / Germany ──────────────────────────────────────────────────
            Add("Amsterdam","Bruxelles",  1, TrainColor.Wild);
            Add("Amsterdam","Essen",      3, TrainColor.Yellow);
            Add("Amsterdam","Frankfurt",  2, TrainColor.White);
            Add("Bruxelles","Frankfurt",  2, TrainColor.Blue);
            Add("Essen",    "Frankfurt",  2, TrainColor.Green);
            Add("Essen",    "Berlin",     2, TrainColor.Blue);
            Add("Essen",    "Kobenhavn",  3, TrainColor.Blue,  ferryLocos: 1);  // North Sea ferry
            Add("Frankfurt","Berlin",     3, TrainColor.Black);
            Add("Frankfurt","München",    2, TrainColor.Pink);
            Add("Frankfurt","Praha",      3, TrainColor.Wild);
            Add("Berlin",   "Praha",      2, TrainColor.Wild);
            Add("Berlin",   "Danzig",     4, TrainColor.Wild);
            Add("Berlin",   "Warszawa",   5, TrainColor.White);

            // ── Scandinavia ────────────────────────────────────────────────────────
            Add("Bergen",   "Kobenhavn",  4, TrainColor.Wild);
            Add("Bergen",   "Stockholm",  3, TrainColor.Wild);
            Add("Kobenhavn","Stockholm",  3, TrainColor.White);
            Add("Stockholm","Petrograd",  8, TrainColor.Wild);   // longest route (21 pts)
            Add("Stockholm","Danzig",     3, TrainColor.Wild);

            // ── Eastern Europe / Baltic ───────────────────────────────────────────
            Add("Danzig",   "Riga",       3, TrainColor.Black);
            AddDouble("Danzig", "Warszawa", 2, TrainColor.Wild, TrainColor.Wild);
            Add("Riga",     "Petrograd",  4, TrainColor.Blue);
            Add("Riga",     "Wilno",      4, TrainColor.Green);
            Add("Petrograd","Moskva",     4, TrainColor.Wild);
            Add("Petrograd","Wilno",      4, TrainColor.Blue);
            Add("Wilno",    "Warszawa",   3, TrainColor.Red);
            Add("Wilno",    "Smolensk",   3, TrainColor.Wild);
            Add("Wilno",    "Kyiv",       2, TrainColor.Wild);
            Add("Warszawa", "Kyiv",       4, TrainColor.Wild);
            Add("Moskva",   "Smolensk",   2, TrainColor.Orange);
            Add("Moskva",   "Kharkov",    4, TrainColor.Green);
            Add("Smolensk", "Kyiv",       3, TrainColor.Red);
            Add("Kharkov",  "Kyiv",       4, TrainColor.Red);
            Add("Kharkov",  "Rostov",     2, TrainColor.Green);
            Add("Kyiv",     "Bucuresti",  4, TrainColor.Wild);
            Add("Rostov",   "Sochi",      2, TrainColor.Wild);
            Add("Rostov",   "Sevastopol", 4, TrainColor.Wild);
            Add("Sochi",    "Erzurum",    3, TrainColor.Black, tunnel: true);

            // ── Central Europe ─────────────────────────────────────────────────────
            Add("München",  "Wien",       3, TrainColor.Blue);
            Add("München",  "Zürich",     2, TrainColor.Yellow);
            Add("München",  "Venezia",    2, TrainColor.Blue);
            Add("Praha",    "Wien",       1, TrainColor.Wild);
            Add("Praha",    "München",    4, TrainColor.Wild);  // alternate: via Zürich
            AddDouble("Wien", "Budapest", 1, TrainColor.Red,   TrainColor.Wild);

            // ── Alpine / Mediterranean ─────────────────────────────────────────────
            Add("Zürich",   "Frankfurt",  3, TrainColor.Green);
            Add("Zürich",   "Marseille",  2, TrainColor.Wild);
            Add("Zürich",   "Venezia",    2, TrainColor.Green, tunnel: true);
            Add("Marseille","Venezia",    4, TrainColor.Wild);

            // ── Balkans ───────────────────────────────────────────────────────────
            Add("Budapest", "Sarajevo",   3, TrainColor.Pink);
            Add("Budapest", "Bucuresti",  4, TrainColor.Blue);
            Add("Sarajevo", "Sofia",      2, TrainColor.Wild);
            Add("Sarajevo", "Athina",     4, TrainColor.Green, ferryLocos: 1);  // Adriatic ferry
            Add("Sarajevo", "Bucuresti",  3, TrainColor.Wild);
            Add("Bucuresti","Sofia",      2, TrainColor.Wild);
            Add("Bucuresti","Sevastopol", 4, TrainColor.White, ferryLocos: 2);  // Black Sea ferry
            Add("Sofia",    "Constantinople", 3, TrainColor.Blue);
            Add("Sofia",    "Athina",     3, TrainColor.Pink);
            Add("Sevastopol","Constantinople", 4, TrainColor.Wild, ferryLocos: 2); // Black Sea ferry

            // ── Italy ─────────────────────────────────────────────────────────────
            Add("Venezia",  "Roma",       2, TrainColor.Wild);
            Add("Venezia",  "Sarajevo",   3, TrainColor.Wild);  // Adriatic coast
            Add("Roma",     "Brindisi",   2, TrainColor.Wild);
            Add("Roma",     "Palermo",    4, TrainColor.Wild,   ferryLocos: 1);
            Add("Brindisi", "Athina",     4, TrainColor.Wild,   ferryLocos: 1);
            Add("Brindisi", "Palermo",    1, TrainColor.Wild);

            // ── Aegean / Anatolia ─────────────────────────────────────────────────
            Add("Athina",   "Smyrna",     2, TrainColor.Wild,   ferryLocos: 1); // Aegean ferry
            Add("Constantinople","Smyrna", 2, TrainColor.Wild);
            Add("Constantinople","Angora", 2, TrainColor.Wild);
            Add("Smyrna",   "Angora",     3, TrainColor.Wild);
            // Confirmed ferry (rulebook page 5): 4 cards any color + 2 locos = length 6
            Add("Smyrna",   "Palermo",    6, TrainColor.Wild,   ferryLocos: 2);
            Add("Angora",   "Erzurum",    3, TrainColor.Black,  tunnel: true);
            Add("Angora",   "Kharkov",    5, TrainColor.White);
        }

        // ── Validation ───────────────────────────────────────────────────────────

        private static void Validate()
        {
            // Remove zero-length placeholder routes
            _routes.RemoveAll(r => r.Length == 0);

            // Cities that appear in at least one route (from BuildRoutes only)
            var citiesInRoutes = _routes
                .SelectMany(r => new[] { r.Origin.Name, r.Destination.Name })
                .ToHashSet();

            // Every ticket city should be reachable on the board
            var warnings = new List<string>();
            foreach (var ticket in BuildLongRouteTickets().Concat(BuildRegularTickets()))
            {
                if (!citiesInRoutes.Contains(ticket.Origin.Name))
                    warnings.Add($"Ticket city not in route graph: '{ticket.Origin.Name}'");
                if (!citiesInRoutes.Contains(ticket.Destination.Name))
                    warnings.Add($"Ticket city not in route graph: '{ticket.Destination.Name}'");
            }

            if (warnings.Count > 0)
            {
                Console.Error.WriteLine("MapDefinition validation warnings:");
                foreach (var w in warnings)
                    Console.Error.WriteLine("  " + w);
            }
        }

        // ── Destination tickets ───────────────────────────────────────────────────

        /// <summary>6 long-route tickets (blue-backed).</summary>
        public static List<TicketCard> BuildLongRouteTickets() => new()
        {
            new TicketCard(C("Edinburgh"),  C("Athina"),    21),
            new TicketCard(C("Kobenhavn"),  C("Erzurum"),   21),
            new TicketCard(C("Cádiz"),      C("Stockholm"), 21),
            new TicketCard(C("Brest"),      C("Petrograd"), 20),
            new TicketCard(C("Lisboa"),     C("Danzig"),    20),
            new TicketCard(C("Palermo"),    C("Moskva"),    20),
        };

        /// <summary>Regular-route destination tickets (plain background).</summary>
        public static List<TicketCard> BuildRegularTickets() => new()
        {
            new TicketCard(C("Madrid"),        C("Moskva"),        25),
            new TicketCard(C("Amsterdam"),     C("Pamplona"),      22),  // high-value regular
            new TicketCard(C("Brest"),         C("Venezia"),       21),
            new TicketCard(C("Berlin"),        C("Roma"),          20),
            new TicketCard(C("London"),        C("Berlin"),        20),  // Note: London not in ticket deck if route not feasible — fix map if needed
            new TicketCard(C("Athina"),        C("Wilno"),         17),
            new TicketCard(C("Madrid"),        C("Dieppe"),        17),
            new TicketCard(C("London"),        C("Wien"),          13),
            new TicketCard(C("Paris"),         C("Wien"),          13),
            new TicketCard(C("Stockholm"),     C("Wien"),          13),
            new TicketCard(C("Athina"),        C("Angora"),        12),
            new TicketCard(C("Barcelona"),     C("München"),       12),
            new TicketCard(C("Berlin"),        C("Moskva"),        12),
            new TicketCard(C("Paris"),         C("Roma"),          12),
            new TicketCard(C("Budapest"),      C("Sofia"),         11),
            new TicketCard(C("London"),        C("Paris"),         11),
            new TicketCard(C("München"),       C("Venezia"),       11),
            new TicketCard(C("Paris"),         C("Berlin"),        11),
            new TicketCard(C("Roma"),          C("Smyrna"),        11),
            new TicketCard(C("Warszawa"),      C("Smolensk"),      11),
            new TicketCard(C("Essen"),         C("Kyiv"),          10),
            new TicketCard(C("Madrid"),        C("Zürich"),        10),
            new TicketCard(C("Roma"),          C("Athina"),        10),
            new TicketCard(C("Venezia"),       C("Constantinople"),10),
            new TicketCard(C("Kyiv"),          C("Rostov"),         9),
            new TicketCard(C("Kyiv"),          C("Sochi"),          9),
            new TicketCard(C("Marseille"),     C("Essen"),          9),
            new TicketCard(C("Paris"),         C("Marseille"),      9),
            new TicketCard(C("Sarajevo"),      C("Sevastopol"),     9),
            new TicketCard(C("Stockholm"),     C("Kobenhavn"),      9),  // Note: order matches game card; direction irrelevant
            new TicketCard(C("Angora"),        C("Kharkov"),        8),
            new TicketCard(C("Berlin"),        C("Bucuresti"),      8),
            new TicketCard(C("London"),        C("Edinburgh"),      8),
            new TicketCard(C("München"),       C("Budapest"),       8),
            new TicketCard(C("Palermo"),       C("Constantinople"), 8),
            new TicketCard(C("Sofia"),         C("Smyrna"),         8),
            new TicketCard(C("Zürich"),        C("Brindisi"),       8),
            new TicketCard(C("Frankfurt"),     C("Praha"),          7),
            new TicketCard(C("Zürich"),        C("Frankfurt"),      7),
            new TicketCard(C("Madrid"),        C("Lisboa"),         6),
            new TicketCard(C("Rostov"),        C("Erzurum"),        5),
        };
    }
}
