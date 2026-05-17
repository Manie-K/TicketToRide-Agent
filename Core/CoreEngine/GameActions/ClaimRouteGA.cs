using CoreEngine.Game;

namespace CoreEngine.GameActions
{
    public class ClaimRouteGA : GameAction
    {
        public static readonly ClaimRouteGA Instance = new ClaimRouteGA();

        public override bool CanExecute(Player currentPlayer)
        {
            return true;
        }

        public override void Execute(Player currentPlayer)
        {
            return;
        }
    }
}
