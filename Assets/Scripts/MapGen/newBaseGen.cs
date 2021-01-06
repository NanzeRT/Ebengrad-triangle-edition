using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using TileSetNS;

namespace GeneratorNS
{
    public class Generator
    {

        private static int iter = 1;
        private static int maxIter = 20;

        public Generator(TileSet _tileSet)
        {
            tileSet = _tileSet;
        }

        private readonly TileSet tileSet;
        private int TileNum = 30;
        private Dictionary<(int, int), TileObj> map = new Dictionary<(int, int), TileObj>();
        private Dictionary<(int, int), ushort> typeMap = new Dictionary<(int, int), ushort>();
        private TileQueue tileQueue = new TileQueue();
        private Random rand;

        public Dictionary<(int, int), ushort> Generate(int seed)
        {
            rand = new Random(seed);
            AddTile((0, 0), 1);

            for (int i = 1; i < TileNum; i++)
            {
                tileQueue.Next().Collapse();
            }

            foreach (KeyValuePair<(int, int), TileObj> pair in map)
            {
                if (pair.Value.Id != 0)
                    typeMap[pair.Key] = pair.Value.Id;
            }
            return typeMap;
        }

        private TileObj MapGet((int, int) pos)
        {
            if (!map.ContainsKey(pos))
                AddTile(pos);
            return map[pos];
        }

        private void AddTile((int, int) pos)
        {
            map[pos] = new TileObj(tileSet, rand, map, pos, this);
            tileQueue.Add(map[pos]);
        }

        private void AddTile((int, int) pos, ushort type)
        {
            map[pos] = new TileObj(tileSet, map, pos, this, type);
        }

        private void MoveTile((int, int) pos)
        {
            tileQueue.Move(map[pos]);
        }

        private class TileQueue : List<TileObj>
        {
            public TileObj Next()
            {
                TileObj a = this[0];
                this.RemoveAt(0);
                return a;
            }

            public new void Add(TileObj tile)
            {
                float priority = tile.Priority;
                for (int i = 0; i < this.Count; i++)
                {
                    if (priority < this[i].Priority)
                    {
                        this.Insert(i, tile);
                        return;
                    }
                }
                base.Add(tile);
            }

            public void Move(TileObj tile)
            {
                this.Remove(tile);
                this.Add(tile);
            }
        }

        private class TileObj
        {
            public TileObj(TileSet tSet, Dictionary<(int, int), TileObj> mp, (int, int) p, Generator g, ushort type) : this(tSet, mp, p, g)
            {
                _setType(type);
            }
            public TileObj(TileSet tSet, Random rnd, Dictionary<(int, int), TileObj> mp, (int, int) p, Generator g) : this(tSet, mp, p, g)
            {
                rand = rnd;
            }
            private TileObj(TileSet tSet, Dictionary<(int, int), TileObj> mp, (int, int) p, Generator g)
            {
                gen = g;
                pos = p;
                map = mp;
                tileSet = tSet;
                tiles = tSet.Tiles;
                rules = tSet.Rules;
                falseRules = new bool[rules.Count];
            }

            private bool done = false;
            private float _priority;
            public float Priority { get => _priority; }
            private ushort _id = 0;
            public ushort Id { get => _id; }
            private readonly Generator gen;
            private readonly Random rand;
            private readonly (int, int) pos;
            private readonly Dictionary<(int, int), TileObj> map;
            private readonly Dictionary<ushort, Tile> tiles;
            private readonly List<Rule> rules;
            private readonly TileSet tileSet;

            private Dictionary<(int, int), ushort> remainRules = new Dictionary<(int, int), ushort>();
            private Dictionary<(int, int), ushort> disqalifiedRules = new Dictionary<(int, int), ushort>();
            private Dictionary<(int, int), Dictionary<ushort, ushort>> disqalifiedTypes = new Dictionary<(int, int), Dictionary<ushort, ushort>>();
            private List<(int, int)> donePoss = new List<(int, int)>();
            private bool[] falseRules;
            private IEnumerable<int> TrueRules { get => Enumerable.Range(0, falseRules.Length).Where(x => !falseRules[x]); }

            public void Collapse()
            {
                ushort type;
                do
                {
                    type = _getType();
                } while (!_isValid(type));
                _setType(type);
            }

            private ushort _getType()
            { // random with priority
                Dictionary<ushort, ushort> prioritys = _getPrioritys();

                int sum = 0;
                foreach (var i in prioritys.Values) sum += i;

                int r = rand.Next(sum);

                foreach (var pair in prioritys)
                {
                    r -= pair.Value;
                    if (r <= 0)
                        return pair.Key;
                }

                throw new Exception("Something wrong");
            }

            private Dictionary<ushort, ushort> _getPrioritys()
            {
                IEnumerable<ushort> types = TrueRules.Select(x => rules[x].Tile.Id).Distinct();

                return types.ToDictionary(k => k, v => _getPriority(v));
            }

            private ushort _getPriority(ushort type)
            {
                return tiles[type].Priority;
            }

            private bool _isValid(ushort type)
            {
                if (_checkZeros(type) && _checkNeighbors(type))
                {
                    return true;
                }
                else
                {
                    _removeType(type);
                    return false;
                }
            }

            private void _removeType(ushort type)
            {
                foreach (var num in TrueRules.Where(num => rules[num].Tile.Id == type))
                    _removeRule(num);
            }

            private void _removeRule(int ruleNum)
            {
                foreach (var pos in rules[ruleNum].GetPoss())
                {
                    if (!disqalifiedTypes.ContainsKey(pos))
                        disqalifiedTypes[pos] = new Dictionary<ushort, ushort>();

                    foreach (var type in rules[ruleNum].Types[pos])
                    {
                        if (!disqalifiedTypes[pos].ContainsKey(type))
                            disqalifiedTypes[pos][type] = 0;
                        disqalifiedTypes[pos][type]++;
                    }

                    if (!disqalifiedRules.ContainsKey(pos))
                        disqalifiedRules[pos] = 0;
                    disqalifiedRules[pos]++;

                    if (disqalifiedRules[pos] == tileSet.RuleCountOnPos[pos])
                    {
                        donePoss.Add(pos);
                    }
                    remainRules[pos] = (ushort) (tileSet.RuleCountOnPos[pos] - disqalifiedRules[pos]);
                    
                }

                falseRules[ruleNum] = true;
            }

            private bool _checkZeros(ushort type)
            {
                if (!tiles[type].AllowAllZeros)
                {
                    foreach (var num in TrueRules.Where(num => rules[num].Tile.Id == type))
                    {
                        bool AllZeros = true;
                        foreach (var pair in rules[num].Pairs)
                        {
                            if ((map.TryGetValue(TriMapUtil.GetWorldPos(pos, pair.Pos), out var tile) ? tile.Id : 0) != 0)
                            {
                                AllZeros = false;
                                break;
                            }
                        }

                        if (AllZeros)
                        {
                            falseRules[num] = true;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    return false;
                }
                return true;
            }

            private bool _checkNeighbors(ushort type)
            {
                foreach (var lpos in tileSet.PosToRuleNums.Keys)
                {
                    if (!gen.MapGet(TriMapUtil.GetWorldPosRev(pos, lpos)).AskForType(lpos, type))
                        return false;
                }
                return true;
            }

            private bool AskForType((int, int) localPos, ushort type)
            {
                if (donePoss.Contains(localPos) || done)
                    return true;

                foreach (var num in tileSet.PosToRuleNums[localPos].Where(num => !falseRules[num] && rules[num].Types[localPos].Contains(type)))
                {
                    return true;
                }
                return false;
            }

            private void _setType(ushort type)
            {
                done = true;
                _id = type;
                _doReflect();
            }

            private void _doReflect()
            {
                foreach (var lpos in tileSet.PosToRuleNums.Keys)
                {
                    gen.MapGet(TriMapUtil.GetWorldPosRev(pos, lpos)).ReflectLocal(lpos, Id);
                }
                SetNeighbors();
            }

            private void SetNeighbors()
            {
                foreach (var _lpos in new (int, int)[] { (1, 0), (-1, 0), (1, -1) })
                {
                    (int, int) _pos = TriMapUtil.GetWorldPos(pos, _lpos);

                    if (!map.ContainsKey(_pos))
                    {
                        gen.AddTile(_pos);
                    }
                }
            }

            private void Reflect((int, int) worldPos, ushort type) => ReflectLocal(TriMapUtil.GetLocalPos(pos, worldPos), type);

            private void ReflectLocal((int, int) localPos, ushort type)
            {
                if (donePoss.Contains(localPos) || done)
                    return;

                foreach (var num in tileSet.PosToRuleNums[localPos].Where(num => !falseRules[num] && !rules[num].Types[localPos].Contains(type)))
                {
                    falseRules[num] = true;
                }

                ReCalcPriority();
            }

            private void ReCalcPriority()
            {
                uint sum_weights = 0;
                float sum_log_weights = 0;
                foreach (var num in TrueRules)
                {
                    sum_weights += rules[num].Tile.Priority;
                    sum_log_weights += (float)(rules[num].Tile.Priority * Math.Log(rules[num].Tile.Priority));
                }

                _priority = (float)((Math.Log(sum_weights) * (1 - (float)rand.Next(10) / 100) - sum_log_weights / sum_weights) * Math.Log(Math.Sqrt(Math.Pow(pos.Item1, 2) + Math.Pow(pos.Item2, 2))));
                gen.MoveTile(pos);
            }
        }
    }
}