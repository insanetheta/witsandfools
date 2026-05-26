namespace WitsAndFools
{
    public enum DoctrineType
    {
        Schemer,
        Brute,
        Trickster,
        Hoarder,
        Neutral
    }

    public static class DoctrineExtensions
    {
        public static string DisplayName(this DoctrineType d) => d switch
        {
            DoctrineType.Schemer => "The Schemer",
            DoctrineType.Brute => "The Brute",
            DoctrineType.Trickster => "The Trickster",
            DoctrineType.Hoarder => "The Hoarder",
            DoctrineType.Neutral => "Neutral",
            _ => d.ToString()
        };

        public static string Philosophy(this DoctrineType d) => d switch
        {
            DoctrineType.Schemer => "Knowledge is the sharpest blade.",
            DoctrineType.Brute => "Overwhelming force needs no subtlety.",
            DoctrineType.Trickster => "The rules are just suggestions.",
            DoctrineType.Hoarder => "The one with the most cards wins... eventually.",
            _ => ""
        };

        public static string ColorHex(this DoctrineType d) => d switch
        {
            DoctrineType.Schemer => "#2DD4BF",
            DoctrineType.Brute => "#EF4444",
            DoctrineType.Trickster => "#A78BFA",
            DoctrineType.Hoarder => "#F59E0B",
            DoctrineType.Neutral => "#9CA3AF",
            _ => "#FFFFFF"
        };

        public static DoctrineType? FromArchetype(ArchetypeType a) => a switch
        {
            ArchetypeType.Rogue => DoctrineType.Schemer,
            ArchetypeType.Brute => DoctrineType.Brute,
            ArchetypeType.Diplomat => DoctrineType.Trickster,
            ArchetypeType.Gambler => DoctrineType.Hoarder,
            _ => null
        };

        public static ResourceType? Resource(this DoctrineType d) => d switch
        {
            DoctrineType.Schemer => ResourceType.Intel,
            DoctrineType.Brute => ResourceType.Fury,
            DoctrineType.Trickster => ResourceType.Favor,
            DoctrineType.Hoarder => ResourceType.Luck,
            _ => null
        };
    }
}
