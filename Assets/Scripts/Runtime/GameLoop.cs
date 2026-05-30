namespace WitsAndFools
{
    // Wires a GameEngine to two IPlayerControllers. Coordinates "whose turn is it" without
    // re-entering the engine inside an event handler — the host (GameManager) calls
    // PumpDeferred() once per frame so all UI subscribers get to run their visuals before
    // the AI takes its turn.
    public sealed class GameLoop
    {
        public readonly GameEngine Engine;
        public readonly IPlayerController[] Controllers;

        bool _pumpRequested;

        public GameLoop(IPlayerController p0, IPlayerController p1, GameEngine engine)
        {
            Engine = engine;
            Controllers = new[] { p0, p1 };
            Engine.OnTurnBegan += _ => RequestPump();
            Engine.OnAttackPlayed += (_, _) => RequestPump();
            Engine.OnDefensePlayed += (_, _, _) => RequestPump();
            Engine.OnAbilityUsed += (_, _, _) => RequestPump();
        }

        public void Start() => Engine.StartNewGame();

        void RequestPump() => _pumpRequested = true;

        // Called by GameManager on Update.
        public void Tick()
        {
            if (!_pumpRequested) return;
            _pumpRequested = false;
            if (Engine.Phase == Phase.GameOver) return;
            int active = Engine.Phase == Phase.Defense ? Engine.DefenderIndex : Engine.AttackerIndex;
            Controllers[active].RequestAction(Engine, active);
        }

        public IPlayerController CurrentController =>
            Engine.Phase == Phase.Defense ? Controllers[Engine.DefenderIndex] : Controllers[Engine.AttackerIndex];
    }
}
