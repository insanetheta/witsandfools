namespace WitsAndFools
{
    public enum PlayerKind { Human, AI }

    public interface IPlayerController
    {
        PlayerKind Kind { get; }
        string DisplayName { get; }

        // Called by GameLoop when it's this player's turn to act.
        // Implementations should call back into GameEngine via TryAttack/TryDefend/TryEat/TryEndBout.
        // For human controllers, this typically arms UI input handlers; for AI it computes and acts.
        void RequestAction(GameEngine engine, int playerIndex);
    }
}
