namespace WitsAndFools
{
    public enum CardRarity { Common, Uncommon, Rare }

    public sealed class CardDefinition
    {
        public string Id;
        public string Name;
        public Suit Suit;
        public Rank Rank;
        public DoctrineType Doctrine;
        public TriggerTiming Trigger;
        public AbilityType? Ability;
        public string AbilityText;
        public string FlavorText;
        public CardRarity Rarity;
        public bool InStartingDeck;

        public bool HasAbility => Ability.HasValue;
        public bool IsNeutral => Doctrine == DoctrineType.Neutral;

        public Card ToRuntimeCard() => new Card(Suit, Rank, Ability, Trigger, Id);

        public override string ToString() => $"{Name} ({Rank.Label()}{Suit.Glyph()}) [{Doctrine}]";
    }
}
