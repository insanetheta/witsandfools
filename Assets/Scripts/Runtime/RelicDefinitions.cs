using System.Collections.Generic;

namespace WitsAndFools
{
    public static class RelicDefinitions
    {
        public static IEnumerable<RelicDefinition> All()
        {
            // --- Starting Relics ---
            yield return new RelicDefinition
            {
                Type = RelicType.SpysMonocle, Name = "Spy's Monocle",
                Description = "See the top card of your deck at all times.",
                Effect = "SpysMonocle", SynergyDoctrine = DoctrineType.Schemer,
                Rarity = RelicRarity.Common, IsStarting = true
            };
            yield return new RelicDefinition
            {
                Type = RelicType.IronGauntlet, Name = "Iron Gauntlet",
                Description = "When opponent eats attacks, draw 1 extra card and gain 1 Fury.",
                Effect = "BruteFury", SynergyDoctrine = DoctrineType.Brute,
                Rarity = RelicRarity.Common, IsStarting = true
            };
            yield return new RelicDefinition
            {
                Type = RelicType.TwoFacedCoin, Name = "Two-Faced Coin",
                Description = "Once per match after eating, discard your worst card.",
                Effect = "LuckyDraw", SynergyDoctrine = DoctrineType.Trickster,
                Rarity = RelicRarity.Common, IsStarting = true
            };
            yield return new RelicDefinition
            {
                Type = RelicType.BottomlessPurse, Name = "Bottomless Purse",
                Description = "At bout start, if hand has 3 or fewer cards, draw 1.",
                Effect = "SteadyHand", SynergyDoctrine = DoctrineType.Hoarder,
                Rarity = RelicRarity.Common, IsStarting = true
            };

            // --- Schemer Synergy ---
            yield return new RelicDefinition
            {
                Type = RelicType.ScholarsLens, Name = "Scholar's Lens",
                Description = "Gain Card Counter: see cards remaining per suit in deck.",
                Effect = "CardCounter", SynergyDoctrine = DoctrineType.Schemer,
                Rarity = RelicRarity.Common
            };
            yield return new RelicDefinition
            {
                Type = RelicType.ForkedTongue, Name = "Forked Tongue",
                Description = "Gain 1 Intel whenever opponent uses an ability.",
                Effect = "WebOfLies", SynergyDoctrine = DoctrineType.Schemer,
                Rarity = RelicRarity.Uncommon
            };
            yield return new RelicDefinition
            {
                Type = RelicType.InvisibleInk, Name = "Invisible Ink",
                Description = "Gain Marked Cards: see 3 random opponent hand cards at match start.",
                Effect = "MarkedDeck", SynergyDoctrine = DoctrineType.Schemer,
                Rarity = RelicRarity.Uncommon
            };
            yield return new RelicDefinition
            {
                Type = RelicType.CiphersRing, Name = "Cipher's Ring",
                Description = "Gain 1 Intel at the start of each bout.",
                Effect = "MarkedCards", SynergyDoctrine = DoctrineType.Schemer,
                Rarity = RelicRarity.Rare
            };

            // --- Brute Synergy ---
            yield return new RelicDefinition
            {
                Type = RelicType.WarHammer, Name = "War Hammer",
                Description = "Your attacks have +1 effective rank.",
                Effect = "RazorsEdge", SynergyDoctrine = DoctrineType.Brute,
                Rarity = RelicRarity.Uncommon
            };
            yield return new RelicDefinition
            {
                Type = RelicType.BloodyKnuckles, Name = "Bloody Knuckles",
                Description = "Gain 1 Fury for each card the opponent eats.",
                Effect = "Bloodlust", SynergyDoctrine = DoctrineType.Brute,
                Rarity = RelicRarity.Common
            };
            yield return new RelicDefinition
            {
                Type = RelicType.IronHelm, Name = "Iron Helm",
                Description = "Draw 1 extra card when eating attacks.",
                Effect = "ThickSkin", SynergyDoctrine = DoctrineType.Brute,
                Rarity = RelicRarity.Common
            };
            yield return new RelicDefinition
            {
                Type = RelicType.CondottierisBanner, Name = "Condottieri's Banner",
                Description = "Gain 1 Fury per trump attack played.",
                Effect = "BattleHardened", SynergyDoctrine = DoctrineType.Brute,
                Rarity = RelicRarity.Rare
            };

            // --- Trickster Synergy ---
            yield return new RelicDefinition
            {
                Type = RelicType.MasqueradeMask, Name = "Masquerade Mask",
                Description = "At each bout start, if opponent has more cards, draw 1.",
                Effect = "Equilibrium", SynergyDoctrine = DoctrineType.Trickster,
                Rarity = RelicRarity.Common
            };
            yield return new RelicDefinition
            {
                Type = RelicType.PoisonedChalice, Name = "Poisoned Chalice",
                Description = "Opponent draws 2 extra cards when eating your attacks.",
                Effect = "PoisonedWine", SynergyDoctrine = DoctrineType.Trickster,
                Rarity = RelicRarity.Uncommon
            };
            yield return new RelicDefinition
            {
                Type = RelicType.MirrorShard, Name = "Mirror Shard",
                Description = "At each bout start, better of top 2 deck cards rises to top.",
                Effect = "CourtFavor", SynergyDoctrine = DoctrineType.Trickster,
                Rarity = RelicRarity.Uncommon
            };
            yield return new RelicDefinition
            {
                Type = RelicType.AlchemistsPhial, Name = "Alchemist's Phial",
                Description = "Once per match, use ability without discarding the card.",
                Effect = "QuicksilverVial", SynergyDoctrine = DoctrineType.Trickster,
                Rarity = RelicRarity.Rare
            };

            // --- Hoarder Synergy ---
            yield return new RelicDefinition
            {
                Type = RelicType.MisersLockbox, Name = "Miser's Lockbox",
                Description = "Draw 1 extra card during refill.",
                Effect = "QuickHands", SynergyDoctrine = DoctrineType.Hoarder,
                Rarity = RelicRarity.Common
            };
            yield return new RelicDefinition
            {
                Type = RelicType.RatsNest, Name = "Rat's Nest",
                Description = "After defending all attacks in a bout, gain 2 Luck.",
                Effect = "Jackpot", SynergyDoctrine = DoctrineType.Hoarder,
                Rarity = RelicRarity.Uncommon
            };
            yield return new RelicDefinition
            {
                Type = RelicType.DeepPockets, Name = "Deep Pockets",
                Description = "Gain 1 Luck at the start of each bout.",
                Effect = "SharkInstinct", SynergyDoctrine = DoctrineType.Hoarder,
                Rarity = RelicRarity.Uncommon
            };
            yield return new RelicDefinition
            {
                Type = RelicType.TaxCollectorsLedger, Name = "Tax Collector's Ledger",
                Description = "Gain +1 resource when any ability is activated.",
                Effect = "CatalystGem", SynergyDoctrine = DoctrineType.Hoarder,
                Rarity = RelicRarity.Rare
            };

            // --- Universal ---
            yield return new RelicDefinition
            {
                Type = RelicType.CandleStub, Name = "Candle Stub",
                Description = "See cards remaining per suit in deck.",
                Effect = "CardCounter", SynergyDoctrine = null,
                Rarity = RelicRarity.Common
            };
            yield return new RelicDefinition
            {
                Type = RelicType.MerchantsPurse, Name = "Merchant's Purse",
                Description = "+3 Florins per match win.",
                Effect = "MerchantsPurse", SynergyDoctrine = null,
                Rarity = RelicRarity.Common
            };
            yield return new RelicDefinition
            {
                Type = RelicType.PhoenixMedal, Name = "Phoenix Medal",
                Description = "Restore 1 Prestige once per run.",
                Effect = "PhoenixMedal", SynergyDoctrine = null,
                Rarity = RelicRarity.Rare
            };
            yield return new RelicDefinition
            {
                Type = RelicType.GamblersDie, Name = "Gambler's Die",
                Description = "Peek and rearrange bottom 3 deck cards at match start.",
                Effect = "LoadedDice", SynergyDoctrine = null,
                Rarity = RelicRarity.Common
            };
            yield return new RelicDefinition
            {
                Type = RelicType.VenetianGlass, Name = "Venetian Glass",
                Description = "Once per match, draw 1 fewer card during refill to keep a high card.",
                Effect = "CourtiersFan", SynergyDoctrine = null,
                Rarity = RelicRarity.Uncommon
            };
            yield return new RelicDefinition
            {
                Type = RelicType.ThiefsLantern, Name = "Thief's Lantern",
                Description = "Once per match, use ability without discarding the card.",
                Effect = "QuicksilverVial", SynergyDoctrine = null,
                Rarity = RelicRarity.Uncommon
            };
            yield return new RelicDefinition
            {
                Type = RelicType.PilgrimsCompass, Name = "Pilgrim's Compass",
                Description = "Draw 1 extra card during refill.",
                Effect = "QuickHands", SynergyDoctrine = null,
                Rarity = RelicRarity.Common
            };

            // --- Boss Relics ---
            yield return new RelicDefinition
            {
                Type = RelicType.TitansCrown, Name = "Titan's Crown",
                Description = "+2 hand size in all matches.",
                Effect = "HandSize+2", SynergyDoctrine = null,
                Rarity = RelicRarity.Rare
            };
            yield return new RelicDefinition
            {
                Type = RelicType.SovereignsDecree, Name = "Sovereign's Decree",
                Description = "+1 max attacks per bout.",
                Effect = "MaxAttacks+1", SynergyDoctrine = null,
                Rarity = RelicRarity.Rare
            };
            yield return new RelicDefinition
            {
                Type = RelicType.HeraldsHorn, Name = "Herald's Horn",
                Description = "+3 starting resource each match.",
                Effect = "Resource+3", SynergyDoctrine = null,
                Rarity = RelicRarity.Rare
            };
            yield return new RelicDefinition
            {
                Type = RelicType.WanderersBoots, Name = "Wanderer's Boots",
                Description = "+1 ability slot.",
                Effect = "AbilitySlot+1", SynergyDoctrine = null,
                Rarity = RelicRarity.Rare
            };
            yield return new RelicDefinition
            {
                Type = RelicType.MisersHoard, Name = "Miser's Hoard",
                Description = "+50% Florins from matches.",
                Effect = "FlorinsX1.5", SynergyDoctrine = null,
                Rarity = RelicRarity.Rare
            };
            yield return new RelicDefinition
            {
                Type = RelicType.PhoenixFeather, Name = "Phoenix Feather",
                Description = "Prevents one prestige loss per match.",
                Effect = "PrestigeShield", SynergyDoctrine = null,
                Rarity = RelicRarity.Rare
            };
        }
    }
}
