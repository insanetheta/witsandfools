namespace WitsAndFools
{
    // Wires a GameEngine to two IPlayerControllers and drives the request-action flow on each event.
    // The UI layer subscribes to engine events for visuals; this class only coordinates "whose turn is it".
    public sealed class GameLoop
    {
        public readonly GameEngine Engine;
        public readonly IPlayerController[] Controllers;

        public GameLoop(IPlayerController p0, IPlayerController p1, int? seed = null)
        {
            Engine = new GameEngine(seed);
            Controllers = new[] { p0, p1 };
            Engine.OnTurnBegan += OnTurnBegan;
            Engine.OnAttackPlayed += (_, _) => Pump();
            Engine.OnDefensePlayed += (_, _, _) => Pump();
            Engine.OnBoutResolved += _ => { /* OnTurnBegan fires next inside ResolveBout */ };
        }

        public void Start() => Engine.StartNewGame();

        void OnTurnBegan(int playerIndex)
        {
            Pump();
        }

        // Ask the active controller to act. For the AI side, that triggers immediate actions.
        // For human, it's a no-op; the UI handles input directly.
        void Pump()
        {
            if (Engine.Phase == Phase.GameOver) return;
            int active = Engine.Phase == Phase.Defense ? Engine.DefenderIndex : Engine.AttackerIndex;
            Controllers[active].RequestAction(Engine, active);
        }

        public IPlayerController CurrentController =>
            Engine.Phase == Phase.Defense ? Controllers[Engine.DefenderIndex] : Controllers[Engine.AttackerIndex];
    }
}
