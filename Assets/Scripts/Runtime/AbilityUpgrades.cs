using System.Collections.Generic;

namespace WitsAndFools
{
    public static class AbilityUpgrades
    {
        static readonly Dictionary<AbilityType, AbilityType> UpgradeTable = new()
        {
            { AbilityType.Brace, AbilityType.BracePlus },
            { AbilityType.ExtraDraw, AbilityType.ExtraDrawPlus },
            { AbilityType.PileOn, AbilityType.PileOnPlus },
            { AbilityType.Peek, AbilityType.PeekPlus },
            { AbilityType.Blocker, AbilityType.BlockerPlus },
            { AbilityType.Fortify, AbilityType.FortifyPlus },
            { AbilityType.SecondWind, AbilityType.SecondWindPlus },
            { AbilityType.Haymaker, AbilityType.HaymakerPlus },
            { AbilityType.Riposte, AbilityType.RipostePlus },
            { AbilityType.Diplomacy, AbilityType.DiplomacyPlus },
            { AbilityType.DoubleTrouble, AbilityType.DoubleTroublePlus },
            { AbilityType.SleightOfHand, AbilityType.SleightOfHandPlus },
        };

        public static bool CanUpgrade(AbilityType ability) => UpgradeTable.ContainsKey(ability);

        public static bool TryGetUpgrade(AbilityType ability, out AbilityType upgraded)
            => UpgradeTable.TryGetValue(ability, out upgraded);

        public static bool IsUpgraded(AbilityType ability)
        {
            foreach (var kv in UpgradeTable)
                if (kv.Value == ability) return true;
            return false;
        }

        public static List<AbilityType> GetUpgradeableAbilities(IList<AbilityType> owned)
        {
            var result = new List<AbilityType>();
            foreach (var a in owned)
                if (CanUpgrade(a)) result.Add(a);
            return result;
        }
    }
}
