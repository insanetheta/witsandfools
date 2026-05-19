namespace WitsAndFools
{
    public enum ResourceType
    {
        Intel,
        Fury,
        Favor,
        Luck
    }

    public static class ResourceDefinitions
    {
        public static ResourceType? ForArchetype(ArchetypeType a) => a switch
        {
            ArchetypeType.Rogue => ResourceType.Intel,
            ArchetypeType.Brute => ResourceType.Fury,
            ArchetypeType.Diplomat => ResourceType.Favor,
            ArchetypeType.Gambler => ResourceType.Luck,
            _ => null
        };

        public static string DisplayName(this ResourceType r) => r switch
        {
            ResourceType.Intel => "Intel",
            ResourceType.Fury => "Fury",
            ResourceType.Favor => "Favor",
            ResourceType.Luck => "Luck",
            _ => r.ToString()
        };
    }
}
