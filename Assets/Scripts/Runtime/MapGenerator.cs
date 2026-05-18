using System;
using System.Collections.Generic;

namespace WitsAndFools
{
    public static class MapGenerator
    {
        static readonly string[][] ActNames = new[]
        {
            new[] { "Barnacle Bill", "Salty Pete", "Dock Rat", "Fishy Meg" },
            new[] { "Merchant Luca", "Trader Yun", "Silk Marco", "Coin Bianca" },
            new[] { "Guildmaster Voss", "Lady Ashton", "Baron Kell", "Fixer Tomas" },
            new[] { "Cardinal Enzo", "Sister Agatha", "Spymaster Grey", "Scholar Ruiz" },
            new[] { "The Champion" }
        };

        public static List<List<MapNode>> Generate(int actIndex, Random rng)
        {
            var map = new List<List<MapNode>>();
            int columns = actIndex < 4 ? 3 : 2;

            for (int col = 0; col < columns; col++)
            {
                var column = new List<MapNode>();
                int rows = actIndex < 4 ? rng.Next(2, 4) : 1;

                for (int row = 0; row < rows; row++)
                {
                    var node = new MapNode { Column = col, Row = row };

                    if (actIndex == 4 && col == columns - 1)
                    {
                        node.Type = MapNodeType.BossMatch;
                        node.Opponent = GenerateOpponent(actIndex, true, true, rng);
                    }
                    else if (col == 1 && row == 0 && actIndex > 0)
                    {
                        node.Type = MapNodeType.EliteMatch;
                        node.Opponent = GenerateOpponent(actIndex, true, false, rng);
                    }
                    else
                    {
                        node.Type = PickNodeType(col, columns, rng);
                        if (node.Type == MapNodeType.RivalMatch)
                            node.Opponent = GenerateOpponent(actIndex, false, false, rng);
                    }

                    column.Add(node);
                }
                map.Add(column);
            }

            if (actIndex == 4)
            {
                var bossCol = new List<MapNode>();
                var boss = new MapNode
                {
                    Type = MapNodeType.BossMatch,
                    Column = columns,
                    Row = 0,
                    Opponent = GenerateOpponent(4, false, true, rng)
                };
                bossCol.Add(boss);
                map.Add(bossCol);
            }

            return map;
        }

        static MapNodeType PickNodeType(int col, int totalCols, Random rng)
        {
            if (col == 0) return MapNodeType.RivalMatch;
            int roll = rng.Next(100);
            if (roll < 50) return MapNodeType.RivalMatch;
            if (roll < 70) return MapNodeType.Shop;
            if (roll < 85) return MapNodeType.Rumor;
            return MapNodeType.Rest;
        }

        static OpponentProfile GenerateOpponent(int actIndex, bool isElite, bool isBoss, Random rng)
        {
            var archetypes = AIArchetypes.ForAct(actIndex);
            var archetype = archetypes[rng.Next(archetypes.Length)];

            int abilitySlots = actIndex switch
            {
                0 => 2,
                1 => 3,
                2 => 4,
                3 => 5,
                _ => 6
            };
            if (isElite) abilitySlots++;

            var abilities = PickAbilities(abilitySlots, actIndex, rng);

            var trinkets = new List<TrinketType>();
            int trinketCount = actIndex switch
            {
                0 => 0,
                1 => rng.Next(0, 2),
                2 => rng.Next(1, 3),
                3 => rng.Next(2, 4),
                _ => rng.Next(3, 5)
            };
            for (int i = 0; i < trinketCount; i++)
            {
                var allTrinkets = (TrinketType[])Enum.GetValues(typeof(TrinketType));
                trinkets.Add(allTrinkets[rng.Next(allTrinkets.Length)]);
            }

            var houseRule = HouseRuleType.None;
            if (isElite || isBoss)
            {
                var rules = (HouseRuleType[])Enum.GetValues(typeof(HouseRuleType));
                houseRule = rules[rng.Next(1, rules.Length)];
            }

            string name;
            if (isBoss)
                name = "The Champion";
            else
            {
                var names = ActNames[Math.Min(actIndex, ActNames.Length - 1)];
                name = names[rng.Next(names.Length)];
            }

            return new OpponentProfile
            {
                Name = name,
                Archetype = archetype,
                Abilities = abilities,
                Trinkets = trinkets,
                HouseRule = houseRule,
                ActIndex = actIndex,
                IsElite = isElite,
                IsBoss = isBoss
            };
        }

        static List<AbilityType> PickAbilities(int count, int actIndex, Random rng)
        {
            var picked = new List<AbilityType>();
            var pool = new List<AbilityDefinition>();

            foreach (var def in AbilityPool.All)
            {
                if (def.IsPassive && actIndex < 2) continue;
                if (def.Rarity == AbilityRarity.Rare && actIndex < 2) continue;
                if (def.Rarity == AbilityRarity.Uncommon && actIndex < 1) continue;
                pool.Add(def);
            }

            for (int i = 0; i < count && pool.Count > 0; i++)
            {
                int idx = rng.Next(pool.Count);
                picked.Add(pool[idx].Type);
                pool.RemoveAt(idx);
            }
            return picked;
        }
    }
}
