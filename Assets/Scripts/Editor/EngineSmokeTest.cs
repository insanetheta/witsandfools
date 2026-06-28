using System;
using System.Collections.Generic;
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

        [MenuItem("Wits and Fools/Smoke Test/Regression Suite")]
        public static void RegressionSuite()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Regression Suite ===");
            int pass = 0, fail = 0;

            void Check(string name, Action test)
            {
                try { test(); pass++; sb.AppendLine($"  PASS: {name}"); }
                catch (Exception e) { fail++; sb.AppendLine($"  FAIL: {name} — {e.Message}"); }
            }

            Check("AbilityPool: all Plus variants registered", () =>
            {
                var plusTypes = new[] {
                    AbilityType.BlockerPlus, AbilityType.ExtraDrawPlus, AbilityType.PeekPlus,
                    AbilityType.FortifyPlus, AbilityType.SecondWindPlus, AbilityType.BracePlus,
                    AbilityType.RipostePlus, AbilityType.SleightOfHandPlus, AbilityType.DoubleTroublePlus,
                    AbilityType.PileOnPlus, AbilityType.HaymakerPlus, AbilityType.DiplomacyPlus
                };
                foreach (var t in plusTypes) AbilityPool.Get(t);
            });

            Check("DeckTopCard: safe when attacker deck empty", () =>
            {
                var config = MatchConfig.Default();
                config.AnyRankAttack = true;
                config.MaxBouts = 50;
                var deck0 = PlayerDeck.CreateStandard(config.Abilities);
                var deck1 = PlayerDeck.CreateStandard(config.Abilities);
                var engine = new GameEngine(999, config, deck0, deck1);
                engine.StartNewGame();
                int safety = 0;
                while (engine.Phase != Phase.GameOver && safety++ < 5000)
                    StepGreedy(engine);
                var _ = engine.DeckTopCard;
            });

            Check("Hand: foreach enumerable", () =>
            {
                var hand = new Hand();
                hand.Add(new Card(Suit.Hearts, Rank.Ace));
                hand.Add(new Card(Suit.Spades, Rank.King));
                int count = 0;
                foreach (var c in hand) count++;
                if (count != 2) throw new Exception($"Expected 2, got {count}");
            });

            Check("RelicPool: all relics registered", () =>
            {
                RelicPool.RegisterAll(RelicDefinitions.All());
                foreach (RelicType r in Enum.GetValues(typeof(RelicType)))
                    RelicPool.TryGet(r, out _);
            });

            Check("MatchSetup: all relic types applied without crash", () =>
            {
                var rng = new System.Random(42);
                foreach (RelicType r in Enum.GetValues(typeof(RelicType)))
                {
                    var run = new RunState();
                    run.PlayerRelics.Add(r);
                    run.PlayerDoctrine = DoctrineType.Schemer;
                    var opponent = new OpponentProfile { Name = "Test", Archetype = AIArchetypeName.Brawler, ActIndex = 0 };
                    MatchSetup.Build(run, opponent, rng);
                }
            });

            Check("MatchSetup: all trinket types applied without crash", () =>
            {
                var rng = new System.Random(42);
                foreach (TrinketType t in Enum.GetValues(typeof(TrinketType)))
                {
                    var run = new RunState();
                    run.PlayerTrinkets.Add(t);
                    run.PlayerDoctrine = DoctrineType.Brute;
                    var opponent = new OpponentProfile { Name = "Test", Archetype = AIArchetypeName.Brawler, ActIndex = 0 };
                    MatchSetup.Build(run, opponent, rng);
                }
            });

            Check("MatchSetup: all burden types applied without crash", () =>
            {
                var rng = new System.Random(42);
                foreach (BurdenType b in Enum.GetValues(typeof(BurdenType)))
                {
                    var run = new RunState();
                    run.PlayerBurdens.Add(b);
                    run.PlayerDoctrine = DoctrineType.Trickster;
                    var opponent = new OpponentProfile { Name = "Test", Archetype = AIArchetypeName.Brawler, ActIndex = 0 };
                    MatchSetup.Build(run, opponent, rng);
                }
            });

            Check("Engine: 10 games with relics + abilities survive", () =>
            {
                var rng = new System.Random(77);
                for (int g = 0; g < 10; g++)
                {
                    var config = MatchConfig.Default();
                    config.AnyRankAttack = true;
                    config.MaxBouts = 12;
                    config.SpysMonocle[0] = true;
                    config.BruteFury[1] = true;
                    config.RazorsEdge[0] = true;
                    config.PoisonedWine[1] = true;
                    config.QuickHands[0] = true;
                    config.Equilibrium[1] = true;
                    config.EmpoweredFoeBonus = 1;
                    config.MaxAttacksPerBout = 4;
                    config.DesperationDiscard = true;
                    var deck0 = PlayerDeck.CreateStandard(config.Abilities);
                    var deck1 = PlayerDeck.CreateStandard(config.Abilities);
                    var engine = new GameEngine(rng.Next(), config, deck0, deck1);
                    engine.StartNewGame();
                    int safety = 0;
                    int stall = 0;
                    Phase lastPhase = engine.Phase;
                    int lastBout = engine.BoutCount;
                    while (engine.Phase != Phase.GameOver && safety++ < 10000)
                    {
                        StepGreedy(engine);
                        if (engine.Phase == lastPhase && engine.BoutCount == lastBout)
                        {
                            if (++stall > 50) engine.TryEat(engine.DefenderIndex);
                        }
                        else { stall = 0; lastPhase = engine.Phase; lastBout = engine.BoutCount; }
                    }
                    if (safety >= 10000) throw new Exception($"Game {g} stalled");
                }
            });

            sb.AppendLine($"Results: {pass} passed, {fail} failed");
            Debug.Log(sb.ToString());
        }

        static void Run(int seed, int games)
        {
            int p0wins = 0, p1wins = 0;
            int totalTurns = 0;
            int safetyCap = 0;

            for (int g = 0; g < games; g++)
            {
                var config = MatchConfig.Default();
                config.AnyRankAttack = true;
                config.MaxBouts = 12;
                config.DesperationDiscard = true;
                var deck0 = PlayerDeck.CreateStandard(config.Abilities);
                var deck1 = PlayerDeck.CreateStandard(config.Abilities);
                var engine = new GameEngine(seed + g, config, deck0, deck1);
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
            for (int p = 0; p < 2; p++)
            {
                if (!engine.AwaitingStackPutBack(p)) continue;
                var hand = engine.HandOf(p);
                if (hand.Count > 0) engine.CompleteStackPutBack(p, hand.Cards[0]);
                return;
            }
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
            Card? best = null;
            foreach (var c in hand.Cards)
            {
                if (!engine.Config.AnyRankAttack && !Rules.CanAttackWith(engine.Bout, c)) continue;
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
