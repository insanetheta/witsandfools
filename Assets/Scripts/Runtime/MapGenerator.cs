using System;
using System.Collections.Generic;

namespace WitsAndFools
{
    public static class MapGenerator
    {

        public static List<List<MapNode>> Generate(int actIndex, Random rng, int columnReduction = 0)
        {
            var map = new List<List<MapNode>>();
            int columns = actIndex < 4 ? 3 : 2;
            columns = Math.Max(1, columns - columnReduction);

            for (int col = 0; col < columns; col++)
            {
                var column = new List<MapNode>();
                int rows = actIndex < 4 ? rng.Next(2, 5) : 1;

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
                        node.Type = PickNodeType(col, columns, column, rows, rng);
                        if (node.Type == MapNodeType.RivalMatch || node.Type == MapNodeType.EliteMatch)
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

            EnsureShopAndRest(map, actIndex, rng);
            EnsureRumor(map, actIndex, rng);
            return map;
        }

        static MapNodeType PickNodeType(int col, int totalCols, List<MapNode> columnSoFar, int totalRows, Random rng)
        {
            var used = new HashSet<MapNodeType>();
            foreach (var n in columnSoFar) used.Add(n.Type);

            if (col == 0)
            {
                // Allow one non-combat node in col 0 when 3+ rows and at least one rival already placed
                if (totalRows >= 3 && columnSoFar.Count >= 1 && used.Contains(MapNodeType.RivalMatch))
                {
                    var extras = new List<(MapNodeType type, int weight)>
                    {
                        (MapNodeType.RivalMatch, 50),
                        (MapNodeType.Shop, 20),
                        (MapNodeType.Rumor, 15),
                        (MapNodeType.Rest, 15),
                    };
                    extras.RemoveAll(c => used.Contains(c.type));
                    if (extras.Count > 0)
                        return WeightedPick(extras, rng);
                }
                return MapNodeType.RivalMatch;
            }

            var candidates = new List<(MapNodeType type, int weight)>
            {
                (MapNodeType.RivalMatch, 40),
                (MapNodeType.Shop, 25),
                (MapNodeType.Rumor, 20),
                (MapNodeType.Rest, 15),
            };
            candidates.RemoveAll(c => used.Contains(c.type));
            if (candidates.Count == 0)
                return MapNodeType.RivalMatch;

            return WeightedPick(candidates, rng);
        }

        static MapNodeType WeightedPick(List<(MapNodeType type, int weight)> candidates, Random rng)
        {
            int total = 0;
            foreach (var c in candidates) total += c.weight;
            int roll = rng.Next(total);
            int acc = 0;
            foreach (var c in candidates)
            {
                acc += c.weight;
                if (roll < acc) return c.type;
            }
            return candidates[candidates.Count - 1].type;
        }

        static void EnsureShopAndRest(List<List<MapNode>> map, int actIndex, Random rng)
        {
            if (actIndex == 4) return;

            bool hasShop = false, hasRest = false;
            foreach (var col in map)
                foreach (var node in col)
                {
                    if (node.Type == MapNodeType.Shop) hasShop = true;
                    if (node.Type == MapNodeType.Rest) hasRest = true;
                }

            if (!hasShop) InjectNodeType(map, MapNodeType.Shop, actIndex, rng);
            if (!hasRest) InjectNodeType(map, MapNodeType.Rest, actIndex, rng);
        }

        static void EnsureRumor(List<List<MapNode>> map, int actIndex, Random rng)
        {
            if (actIndex == 4) return;

            bool hasRumor = false;
            foreach (var col in map)
                foreach (var node in col)
                    if (node.Type == MapNodeType.Rumor) hasRumor = true;

            if (!hasRumor) InjectNodeType(map, MapNodeType.Rumor, actIndex, rng);
        }

        static void InjectNodeType(List<List<MapNode>> map, MapNodeType target, int actIndex, Random rng)
        {
            // Find swappable RivalMatch nodes in non-first columns (skip elites/bosses)
            var swappable = new List<MapNode>();
            for (int c = 1; c < map.Count; c++)
            {
                foreach (var node in map[c])
                {
                    if (node.Type == MapNodeType.RivalMatch)
                        swappable.Add(node);
                }
            }
            if (swappable.Count == 0) return;
            var pick = swappable[rng.Next(swappable.Count)];
            pick.Type = target;
            pick.Opponent = null;
        }

        static OpponentProfile GenerateOpponent(int actIndex, bool isElite, bool isBoss, Random rng)
        {
            return DoctrineRoster.IsInitialized
                ? DoctrineRoster.Pick(actIndex, isElite, isBoss, rng)
                : OpponentRoster.Pick(actIndex, isElite, isBoss, rng);
        }
    }
}
