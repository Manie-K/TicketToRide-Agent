using CoreEngine.Game;

namespace CoreEngine.GameActions
{
    public class DrawCardsGA : GameAction
    {
        public static readonly DrawCardsGA Instance = new DrawCardsGA();

        public override bool CanExecute(Player currentPlayer)
        {
            return false;
        }

        public override void Execute(Player currentPlayer)
        {
            return;
        }
    }
}
