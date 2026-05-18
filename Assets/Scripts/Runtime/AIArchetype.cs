namespace WitsAndFools
{
    public enum AIArchetypeName
    {
        Brawler,
        Miser,
        Fox,
        Noble,
        Scholar,
        Assassin
    }

    public static class AIArchetypes
    {
        public static void Apply(AIPlayer ai, AIArchetypeName archetype, int actIndex)
        {
            float baseRandom = actIndex switch
            {
                0 => 0.10f,
                1 => 0.05f,
                _ => 0f
            };
            ai.RandomMoveChance = baseRandom;
            ai.AbilityEagerness = 1f;

            switch (archetype)
            {
                case AIArchetypeName.Brawler:
                    ai.AbilityEagerness = 1.5f;
                    break;
                case AIArchetypeName.Miser:
                    ai.AbilityEagerness = 0.3f;
                    break;
                case AIArchetypeName.Fox:
                    ai.AbilityEagerness = 1f;
                    break;
                case AIArchetypeName.Noble:
                    ai.AbilityEagerness = 0.8f;
                    break;
                case AIArchetypeName.Scholar:
                    ai.AbilityEagerness = 1.2f;
                    break;
                case AIArchetypeName.Assassin:
                    ai.AbilityEagerness = 0.5f;
                    break;
            }
        }

        public static string DisplayName(AIArchetypeName a) => a switch
        {
            AIArchetypeName.Brawler => "The Brawler",
            AIArchetypeName.Miser => "The Miser",
            AIArchetypeName.Fox => "The Fox",
            AIArchetypeName.Noble => "The Noble",
            AIArchetypeName.Scholar => "The Scholar",
            AIArchetypeName.Assassin => "The Assassin",
            _ => a.ToString()
        };

        public static AIArchetypeName[] ForAct(int actIndex) => actIndex switch
        {
            0 => new[] { AIArchetypeName.Brawler, AIArchetypeName.Miser },
            1 => new[] { AIArchetypeName.Brawler, AIArchetypeName.Miser, AIArchetypeName.Fox },
            2 => new[] { AIArchetypeName.Fox, AIArchetypeName.Noble, AIArchetypeName.Scholar },
            3 => new[] { AIArchetypeName.Scholar, AIArchetypeName.Assassin, AIArchetypeName.Fox },
            _ => new[] { AIArchetypeName.Fox, AIArchetypeName.Assassin, AIArchetypeName.Scholar }
        };
    }
}
