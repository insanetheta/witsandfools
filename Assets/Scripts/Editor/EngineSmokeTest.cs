using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace WitsAndFools.EditorTools
{
    public static class EngineSmokeTest
    {
        [MenuItem("Wits and Fools/Smoke Test/Play 1 Game (Greedy AIs)")]
        public static void PlayOne() => Run(seed: 12345, games: 1);

        [MenuItem("Wits and Fools/Smoke Test/Play 50 Games (Greedy AIs)")]
        public static void PlayFifty() => Run(seed: 1, games: 50);

        static void Run(int seed, int games)
        {
            int p0wins = 0, p1wins = 0;
            int totalTurns = 0;
            int safetyCap = 0;

            for (int g = 0; g < games; g++)
            {
                var engine = new GameEngine(seed + g);
                int turns = 0;
                engine.OnTurnBegan += _ => turns++;
                engine.StartNewGame();

                int safety = 0;
                while (engine.Phase != Phase.GameOver && safety++ < 5000)
                {
                    StepGreedy(engine);
                }

                if (safety >= 5000) safetyCap++;
                totalTurns += turns;
                if (engine.WinnerIndex == 0) p0wins++;
                else if (engine.WinnerIndex == 1) p1wins++;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"=== Smoke Test: {games} game(s), seed base={seed} ===");
            sb.AppendLine($"P0 wins: {p0wins}   P1 wins: {p1wins}   Stalled (safety cap): {safetyCap}");
            sb.AppendLine($"Avg turns/game: {(games > 0 ? (totalTurns / (double)games) : 0):0.0}");
            Debug.Log(sb.ToString());
        }

        // --- A trivially greedy policy used only for testing the engine. ---

        static void StepGreedy(GameEngine engine)
        {
            if (TryUseFirstAbility(engine)) return;

            switch (engine.Phase)
            {
                case Phase.Attack:
                    StepAttack(engine);
                    return;
                case Phase.Defense:
                    StepDefense(engine);
                    return;
            }
        }

        static bool TryUseFirstAbility(GameEngine engine)
        {
            int p = engine.Phase == Phase.Defense ? engine.DefenderIndex : engine.AttackerIndex;
            var hand = engine.HandOf(p);
            if (hand.Count <= 2) return false;
            foreach (var c in hand.Cards)
            {
                if (!c.HasAbility) continue;
                int slot = engine.Phase == Phase.Defense ? engine.Bout.FirstUndefendedSlot() : -1;
                if (engine.TryUseAbility(p, c, slot)) return true;
            }
            return false;
        }

        static void StepAttack(GameEngine engine)
        {
            int p = engine.AttackerIndex;
            var hand = engine.HandOf(p);

            Card? attack = LegalAttack(engine, hand);

            if (!engine.Bout.IsEmpty && engine.Bout.FullyDefended)
            {
                if (attack == null || !engine.TryAttack(p, attack.Value))
                    engine.TryEndBout(p);
                return;
            }

            if (attack != null && engine.TryAttack(p, attack.Value)) return;
            // Pathological: nothing legal and the bout isn't fully defended. End or eat to make progress.
            if (!engine.Bout.IsEmpty && engine.Bout.FullyDefended) engine.TryEndBout(p);
            else engine.TryEat(engine.DefenderIndex);
        }

        static Card? LegalAttack(GameEngine engine, Hand hand)
        {
            // Prefer lowest non-trump.
            Card? best = null;
            foreach (var c in hand.Cards)
            {
                if (!Rules.CanAttackWith(engine.Bout, c)) continue;
                if (best == null) { best = c; continue; }
                if (BetterAttack(c, best.Value, engine.Trump)) best = c;
            }
            return best;
        }

        static bool BetterAttack(Card candidate, Card current, Suit trump)
        {
            bool ct = candidate.Suit == trump;
            bool cu = current.Suit == trump;
            if (cu && !ct) return true;             // prefer non-trump
            if (ct && !cu) return false;
            return (int)candidate.Rank < (int)current.Rank; // lower is "less valuable"
        }

        static void StepDefense(GameEngine engine)
        {
            int p = engine.DefenderIndex;
            int slot = engine.Bout.FirstUndefendedSlot();
            if (slot < 0) { engine.TryEat(p); return; }

            var hand = engine.HandOf(p);
            Card? best = null;
            foreach (var c in hand.Cards)
            {
                if (!Rules.CanDefendSlotWith(engine.Bout, slot, c, engine.Trump)) continue;
                if (best == null) { best = c; continue; }
                if (CheaperDefense(c, best.Value, engine.Trump)) best = c;
            }
            if (best == null) { engine.TryEat(p); return; }
            engine.TryDefend(p, slot, best.Value);
        }

        static bool CheaperDefense(Card candidate, Card current, Suit trump)
        {
            bool ct = candidate.Suit == trump;
            bool cu = current.Suit == trump;
            if (cu && !ct) return true;
            if (ct && !cu) return false;
            return (int)candidate.Rank < (int)current.Rank;
        }
    }
}
