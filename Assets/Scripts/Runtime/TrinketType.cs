namespace WitsAndFools
{
    public enum TrinketType
    {
        MerchantsPurse,
        MisersRing,
        FoolsGold,
        TailorsThimble,
        JugglersBalls,
        LoadedDice,
        CourtiersFan,
        DuelistsGlove,
        ShieldBrooch,
        PoisonedWine,
        SpysMonocle,
        MarkedDeck,
        AlchemistsStone,
        CrownOfThorns,
        HereticsBrand,
        ScholarsTome,
        ForgersKit,
        QuicksilverVial,
        VentriloquistsDummy,
        DevilsBargain,
        PhoenixMedal,

        // Compound synergy trinkets
        CatalystGem,
        EchoStone,
        RazorsEdge,
        Bloodstone
    }

    public static class TrinketTypeExtensions
    {
        public static string DisplayName(this TrinketType t) => t switch
        {
            TrinketType.MerchantsPurse => "The Merchant's Purse",
            TrinketType.MisersRing => "The Miser's Ring",
            TrinketType.FoolsGold => "Fool's Gold",
            TrinketType.TailorsThimble => "The Tailor's Thimble",
            TrinketType.JugglersBalls => "The Juggler's Balls",
            TrinketType.LoadedDice => "Loaded Dice",
            TrinketType.CourtiersFan => "The Courtier's Fan",
            TrinketType.DuelistsGlove => "The Duelist's Glove",
            TrinketType.ShieldBrooch => "The Shield Brooch",
            TrinketType.PoisonedWine => "Poisoned Wine",
            TrinketType.SpysMonocle => "The Spy's Monocle",
            TrinketType.MarkedDeck => "Marked Deck",
            TrinketType.AlchemistsStone => "The Alchemist's Stone",
            TrinketType.CrownOfThorns => "Crown of Thorns",
            TrinketType.HereticsBrand => "The Heretic's Brand",
            TrinketType.ScholarsTome => "The Scholar's Tome",
            TrinketType.ForgersKit => "The Forger's Kit",
            TrinketType.QuicksilverVial => "Quicksilver Vial",
            TrinketType.VentriloquistsDummy => "The Ventriloquist's Dummy",
            TrinketType.DevilsBargain => "The Devil's Bargain",
            TrinketType.PhoenixMedal => "The Phoenix Medal",
            TrinketType.CatalystGem => "Catalyst Gem",
            TrinketType.EchoStone => "Echo Stone",
            TrinketType.RazorsEdge => "Razor's Edge",
            TrinketType.Bloodstone => "Bloodstone",
            _ => t.ToString()
        };

        public static string Description(this TrinketType t) => t switch
        {
            TrinketType.MerchantsPurse => "+3 Florins after each match.",
            TrinketType.MisersRing => "+1 Florin per bout where you successfully defend.",
            TrinketType.FoolsGold => "Start each match with a temporary Gold Card (7 of any suit).",
            TrinketType.TailorsThimble => "Starting hand size is 5 instead of 6.",
            TrinketType.JugglersBalls => "After the first bout, discard 1 card for free.",
            TrinketType.LoadedDice => "At match start, peek at and rearrange bottom 3 deck cards.",
            TrinketType.CourtiersFan => "Once per match, draw 1 fewer card during refill.",
            TrinketType.DuelistsGlove => "First attack each bout ignores rank-match rule.",
            TrinketType.ShieldBrooch => "Once per match, auto-beat the first undefended attack.",
            TrinketType.PoisonedWine => "When opponent eats, they draw 2 extra from deck.",
            TrinketType.SpysMonocle => "See the top card of the deck at all times.",
            TrinketType.MarkedDeck => "At match start, reveal 3 random cards in opponent's hand.",
            TrinketType.AlchemistsStone => "Choose the trump suit at match start.",
            TrinketType.CrownOfThorns => "Trump cards played in defense count for next bout's rank-match.",
            TrinketType.HereticsBrand => "Opponent's trump cards are treated as 1 rank lower for defense.",
            TrinketType.ScholarsTome => "+1 ability slot.",
            TrinketType.ForgersKit => "Abilities that bind to 1 card now bind to 2.",
            TrinketType.QuicksilverVial => "Once per match, use an ability without discarding the card.",
            TrinketType.VentriloquistsDummy => "Once per match, use one of the opponent's abilities.",
            TrinketType.DevilsBargain => "At match start, choose: +3 Florins or draw 1 fewer starting card.",
            TrinketType.PhoenixMedal => "Restore 1 Prestige once per run.",
            TrinketType.CatalystGem => "When you activate an ability, gain +1 resource.",
            TrinketType.EchoStone => "Upgraded abilities trigger their effect twice.",
            TrinketType.RazorsEdge => "Your attacks deal +1 effective rank.",
            TrinketType.Bloodstone => "Each bout you win (defender), gain +1 resource next bout.",
            _ => ""
        };

        public static bool AffectsEngine(this TrinketType t) => t switch
        {
            TrinketType.TailorsThimble => true,
            TrinketType.DuelistsGlove => true,
            TrinketType.PoisonedWine => true,
            TrinketType.AlchemistsStone => true,
            TrinketType.HereticsBrand => true,
            TrinketType.ShieldBrooch => true,
            TrinketType.CourtiersFan => true,
            TrinketType.JugglersBalls => true,
            TrinketType.LoadedDice => true,
            TrinketType.QuicksilverVial => true,
            TrinketType.CrownOfThorns => true,
            TrinketType.DevilsBargain => true,
            TrinketType.FoolsGold => true,
            TrinketType.VentriloquistsDummy => true,
            TrinketType.CatalystGem => true,
            TrinketType.EchoStone => true,
            TrinketType.RazorsEdge => true,
            TrinketType.Bloodstone => true,
            _ => false
        };
    }
}
