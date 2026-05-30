using CoreEngine.Game;

namespace CoreEngine.GameActions
{
    /// <summary>
    /// Draw Destination Tickets action.
    ///
    /// The player draws 3 tickets from the top of the Ticket deck (or fewer if the
    /// deck has less than 3). They must keep at least 1; any not kept go to the
    /// bottom of the Ticket deck.
    /// </summary>
    public class DrawTicketsGA : GameAction
    {
        public static readonly DrawTicketsGA Instance = new DrawTicketsGA();

        private DrawTicketsGA()
        {
            Name = "Draw Destination Tickets";
            Description = "Draw 3 destination tickets and keep at least 1.";
        }

        public override bool CanExecute(Player currentPlayer) =>
            GameManager.Instance.TicketsDeck.DeckSize > 0;

        public override void Execute(Player currentPlayer)
        {
            GameManager.Instance.DealTicketsToPlayer(
                currentPlayer, count: 3, minKeep: 1, returnToBottom: true);
        }
    }
}
