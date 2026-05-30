namespace WitsAndFools
{
    public static class Ascension
    {
        public const int MaxLevel = 10;

        public static string ModifierName(int level) => level switch
        {
            1 => "Lean Purse",
            2 => "Armed Elites",
            3 => "Spartan Rest",
            4 => "Rattled Start",
            5 => "Inflated Prices",
            6 => "Empowered Foes",
            7 => "Shorter Roads",
            8 => "Brutal Defeats",
            9 => "Narrow Mind",
            10 => "Relentless Assault",
            _ => ""
        };

        public static string ModifierDescription(int level) => level switch
        {
            1 => "Earn 3 fewer Florins from match victories",
            2 => "Elite opponents gain +1 relic",
            3 => "Rest quietly grants +2 Florins instead of +3",
            4 => "Start each run with Rattled Nerves",
            5 => "Shop prices increased by 25%",
            6 => "Opponents start with +1 resource",
            7 => "One fewer map node column per act",
            8 => "Lose 2 Prestige on defeat instead of 1",
            9 => "Start with 3 ability slots instead of 4",
            10 => "Opponents get +1 max attacks per bout",
            _ => ""
        };

        public static bool LeanPurse(int level) => level >= 1;
        public static bool ArmedElites(int level) => level >= 2;
        public static bool SpartanRest(int level) => level >= 3;
        public static bool RattledStart(int level) => level >= 4;
        public static bool InflatedPrices(int level) => level >= 5;
        public static bool EmpoweredFoes(int level) => level >= 6;
        public static bool ShorterRoads(int level) => level >= 7;
        public static bool BrutalDefeats(int level) => level >= 8;
        public static bool NarrowMind(int level) => level >= 9;
        public static bool RelentlessAssault(int level) => level >= 10;
    }
}
