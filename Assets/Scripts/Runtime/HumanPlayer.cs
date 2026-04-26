namespace WitsAndFools
{
    // The human player just waits for the UI to arm itself. The UI calls engine.TryAttack/etc directly.
    public sealed class HumanPlayer : IPlayerController
    {
        public PlayerKind Kind => PlayerKind.Human;
        public string DisplayName { get; }

        public HumanPlayer(string name = "You") { DisplayName = name; }

        public void RequestAction(GameEngine engine, int playerIndex)
        {
            // No-op: input is driven by UI clicks. The presence of this turn is enough; UI listens to engine events.
        }
    }
}
