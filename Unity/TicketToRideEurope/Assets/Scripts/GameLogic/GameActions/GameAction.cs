using CoreEngine.Game;

namespace CoreEngine.GameActions
{
    public abstract class GameAction
    {
        public string Name        { get; protected init; } = "";
        public string Description { get; protected init; } = "";

        public abstract bool CanExecute(Player currentPlayer);

        public abstract void Execute(Player currentPlayer);

        public override string ToString() => Name;
    }
}
