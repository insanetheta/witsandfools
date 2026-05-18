using System;
using System.Collections.Generic;

namespace WitsAndFools
{
    public static class MapGenerator
    {

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
                        node.Type = PickNodeType(col, columns, column, rng);
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

        static MapNodeType PickNodeType(int col, int totalCols, List<MapNode> columnSoFar, Random rng)
        {
            if (col == 0) return MapNodeType.RivalMatch;

            var used = new HashSet<MapNodeType>();
            foreach (var n in columnSoFar) used.Add(n.Type);

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

        static OpponentProfile GenerateOpponent(int actIndex, bool isElite, bool isBoss, Random rng)
        {
            return OpponentRoster.Pick(actIndex, isElite, isBoss, rng);
        }
    }
}
