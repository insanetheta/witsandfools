namespace WitsAndFools
{
    public enum RelicType
    {
        // --- Starting Relics (one per doctrine) ---
        SpysMonocle,
        IronGauntlet,
        TwoFacedCoin,
        BottomlessPurse,

        // --- Schemer Synergy ---
        ScholarsLens,
        ForkedTongue,
        InvisibleInk,
        CiphersRing,

        // --- Brute Synergy ---
        WarHammer,
        BloodyKnuckles,
        IronHelm,
        CondottierisBanner,

        // --- Trickster Synergy ---
        MasqueradeMask,
        PoisonedChalice,
        MirrorShard,
        AlchemistsPhial,

        // --- Hoarder Synergy ---
        MisersLockbox,
        RatsNest,
        DeepPockets,
        TaxCollectorsLedger,

        // --- Universal ---
        CandleStub,
        MerchantsPurse,
        PhoenixMedal,
        GamblersDie,
        VenetianGlass,
        ThiefsLantern,
        PilgrimsCompass,

        // --- Boss Relics (elite/boss rewards) ---
        TitansCrown,
        SovereignsDecree,
        HeraldsHorn,
        WanderersBoots,
        MisersHoard,
        PhoenixFeather
    }

    public enum RelicRarity { Common, Uncommon, Rare }

    public sealed class RelicDefinition
    {
        public RelicType Type;
        public string Name;
        public string Description;
        public string Effect;
        public DoctrineType? SynergyDoctrine;
        public RelicRarity Rarity;
        public string[] Acquisition;
        public int ShopPrice;
        public string FlavorText;
        public bool IsStarting;
    }

    public static class RelicPool
    {
        static readonly System.Collections.Generic.Dictionary<RelicType, RelicDefinition> _defs = new();
        static bool _initialized;

        public static void Register(RelicDefinition def) => _defs[def.Type] = def;

        public static void RegisterAll(System.Collections.Generic.IEnumerable<RelicDefinition> defs)
        {
            foreach (var d in defs) _defs[d.Type] = d;
            _initialized = true;
        }

        public static RelicDefinition Get(RelicType type)
        {
            if (_defs.TryGetValue(type, out var def)) return def;
            throw new System.Collections.Generic.KeyNotFoundException($"Relic not found: {type}");
        }

        public static bool TryGet(RelicType type, out RelicDefinition def) =>
            _defs.TryGetValue(type, out def);

        public static System.Collections.Generic.IReadOnlyList<RelicDefinition> All() =>
            new System.Collections.Generic.List<RelicDefinition>(_defs.Values);

        public static System.Collections.Generic.IReadOnlyList<RelicDefinition> ForDoctrine(DoctrineType doctrine) =>
            new System.Collections.Generic.List<RelicDefinition>(
                System.Linq.Enumerable.Where(_defs.Values, r => r.SynergyDoctrine == doctrine));

        public static System.Collections.Generic.IReadOnlyList<RelicDefinition> Universal() =>
            new System.Collections.Generic.List<RelicDefinition>(
                System.Linq.Enumerable.Where(_defs.Values, r => r.SynergyDoctrine == null));

        public static System.Collections.Generic.IReadOnlyList<RelicDefinition> StartingRelics() =>
            new System.Collections.Generic.List<RelicDefinition>(
                System.Linq.Enumerable.Where(_defs.Values, r => r.IsStarting));

        public static RelicType? StartingRelicFor(DoctrineType doctrine) => doctrine switch
        {
            DoctrineType.Schemer => RelicType.SpysMonocle,
            DoctrineType.Brute => RelicType.IronGauntlet,
            DoctrineType.Trickster => RelicType.TwoFacedCoin,
            DoctrineType.Hoarder => RelicType.BottomlessPurse,
            _ => null
        };

        public static int Count => _defs.Count;
        public static bool IsInitialized => _initialized;

        public static void Clear()
        {
            _defs.Clear();
            _initialized = false;
        }
    }
}
