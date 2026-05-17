using CoreEngine.Game;

namespace CoreEngine.GameActions
{
    public abstract class GameAction
    {
        public string Name;
        public string Description;

        public abstract bool CanExecute(Player currentPlayer);

        public abstract void Execute(Player currentPlayer);
    }
}
