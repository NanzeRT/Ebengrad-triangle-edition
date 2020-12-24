using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace _ {
public class Generator
{
    private static int iter = 1;
    private static int maxIter = 20;
    public Generator(List<(Dictionary<(int, int), ushort[]>, ushort)> r, Dictionary<ushort, TileType> t)
    {
        rules = r;
        tiles = t;
    }
    private readonly List<(Dictionary<(int, int), ushort[]>, ushort)> rules;
    private int TileNum = 1000;
    private readonly Dictionary<ushort, TileType> tiles;
    private Dictionary<(int, int), Tile> map = new Dictionary<(int, int), Tile>();
    private Dictionary<(int, int), ushort> typeMap = new Dictionary<(int, int), ushort>();
    private TileQueue tileQueue = new TileQueue();
    private Random rand;

    public Dictionary<(int, int), ushort> Generate(int seed)
    {
        rand = new Random(seed);
        AddTile ((0, 0), 1);
        try
        {
            for (int i = 1; i < TileNum; i++)
            {
                tileQueue.Next().Collapse();
            }
        } catch(Exception e) {
            if (iter > maxIter)
                throw e;
            iter++;
            UnityEngine.Debug.Log("restart " + map.Count);

            return new Generator(rules, tiles).Generate(rand.Next());
        }

        foreach (KeyValuePair<(int, int), Tile> pair in map)
        {
            if (pair.Value.GetId() != 0)
                typeMap[pair.Key] = pair.Value.GetId();
        }
        return typeMap;
    }

    private void AddTile ((int, int) pos)
    {
        map[pos] = new Tile(rules, tiles, rand, map, pos, this);
        tileQueue.Add(map[pos]);
    }

    private void AddTile ((int, int) pos, ushort type)
    {
        map[pos] = new Tile(rules, tiles, map, pos, this, type);
    }

    private void MoveTile ((int, int) pos)
    {
        tileQueue.Move(map[pos]);
    }

    private class TileQueue : List<Tile>
    {
        public Tile Next()
        {
            Tile a = this[0];
            this.RemoveAt(0);
            return a;
        }

        public new void Add(Tile tile)
        {
            float priority = tile.GetPriority();
            for (int i = 0; i < this.Count; i++)
            {
                if (priority < this[i].GetPriority())
                {
                    this.Insert(i, tile);
                    return;
                }
            }
            base.Add(tile);
        }

        public void Move(Tile tile)
        {
            this.Remove(tile);
            this.Add(tile);
        }
    }

    private class Tile
    {
        public Tile(List<(Dictionary<(int, int), ushort[]>, ushort)> r, Dictionary<ushort, TileType> t, Dictionary<(int, int), Tile> mp, (int, int) p, Generator g, ushort type)
        {
            id = type;
            gen = g;
            pos = p;
            map = mp;
            tiles = t;
            rules = r;
            trueRules = Enumerable.Range(0, r.Count).Select(x => (ushort) x).ToList();
            DoReflect(type);
            Clear();
        }
        public Tile(List<(Dictionary<(int, int), ushort[]>, ushort)> r, Dictionary<ushort, TileType> t, Random rnd, Dictionary<(int, int), Tile> mp, (int, int) p, Generator g)
        {
            gen = g;
            pos = p;
            map = mp;
            rand = rnd;
            tiles = t;
            rules = r;
            trueRules = Enumerable.Range(0, r.Count).Select(x => (ushort) x).ToList();
        }
        private float priority;
        public float GetPriority() => priority;
        private bool done = false;
        private ushort id = 0;
        public ushort GetId() => id;
        private readonly Generator gen;
        private readonly Random rand;
        private readonly (int, int) pos;
        private readonly Dictionary<(int, int), Tile> map;
        private readonly Dictionary<ushort, TileType> tiles;
        private List<Tile> dependent = new List<Tile>();
        private readonly List<(Dictionary<(int, int), ushort[]>, ushort)> rules;
        public List<ushort> trueRules;
        public ushort Collapse()
        {
            //ClearRules();
            IEnumerable<ushort> posibleTypes = trueRules.Select(x => rules[x].Item2).Distinct();
            int sum = 0;
            foreach (ushort type in posibleTypes)
            {
                sum += tiles[type].priority;
            }

            float r = rand.Next(sum);
            
            foreach (ushort type in posibleTypes)
            {
                r -= tiles[type].priority;
                UnityEngine.Debug.Log((type, r));
                if (r <= 0)
                {
                    if (IsValid(type))
                    {
                        //if (LookDepsForType(type))
                        //{
                            UnityEngine.Debug.Log((pos, type));
                            DoReflect(type);
                            id = type;
                            Clear();
                            return type;
                        //} else {
                        //    ClearRules();
                        //}
                    }
                    return Collapse();
                }
            }
            
            throw (new Exception("Empty tile tryed to Collapse() at " + pos));
        }

        public void AddDependent (Tile tile)
        {
            if (!dependent.Contains(tile))
                dependent.Add(tile);
        }

        public bool AskForType (ushort type, (int, int) _pos)
        {
            ClearRules();
            IEnumerable<Dictionary<(int, int), ushort[]>> _rules;
            if (done)
            {
                _rules = trueRules.Select(x => rules[x]).Where(x => x.Item2 == id).Select(x => x.Item1);
            } else {
                _rules = trueRules.Select(x => rules[x]).Select(x => x.Item1);
            }

            (int, int) _lpos = TriMapUtil.GetLocalPos(pos, _pos);

            foreach (var rule in _rules)
            {
                if (rule.TryGetValue(_pos, out ushort[] val)? val.Contains(type) || val[0] == 0 : true)
                    return true;
            }

            return false;
        }

        private bool LookDepsForType (ushort type)
        {
            foreach (var tile in dependent)
            {
                if (!tile.AskForType(type, pos))
                    return false;
            }

            return true;
        }

        private bool IsValid(ushort type)
        {
            IEnumerable<ushort> tRules = trueRules.Where(x => rules[x].Item2 == type).ToArray(); // выбираю номера правил, которые соответствуют типу
            foreach (ushort _ruleNum in tRules)
            {
                bool allZeros = true;
                bool valid = true;
                Dictionary<(int, int), ushort[]> rule = rules[_ruleNum].Item1; // беру правило
                Tile val = null;
                foreach (KeyValuePair<(int, int), ushort[]> pair in rule)
                {
                    if ((map.TryGetValue(TriMapUtil.GetWorldPos(pos, pair.Key), out val)? val.GetId() : 0) == 0 // проверяю на соответствие (0 - любой)
                    || pair.Value.Contains(val.GetId()) || pair.Value.Contains((ushort) 0))
                    {
                        if (val != null && val.GetId() != 0)
                            allZeros = false;
                        continue;
                    } else {
                        trueRules.Remove(_ruleNum); // если нет, то удаляю
                        valid = false;
                        break;
                    }
                }
                if (!tiles[type].allowAllZeros && allZeros)
                {
                    trueRules.Remove(_ruleNum); // если нет, то удаляю
                    continue;
                }

                if (valid)
                {
                    return true;
                }
            }
            return false;
        }

        private void Clear()
        {
            done = true;
        }

        private void ClearRules ()
        {
            foreach (ushort _ruleNum in trueRules.ToList())
            {
                Dictionary<(int, int), ushort[]> rule = rules[_ruleNum].Item1; // беру правило
                foreach (KeyValuePair<(int, int), ushort[]> pair in rule)
                {
                    if (map.TryGetValue(TriMapUtil.GetWorldPos(pos, pair.Key), out Tile val))
                    {
                        if (val.GetId() != 0 // проверяю на соответствие (0 - любой)
                        && !pair.Value.Contains(val.GetId()) && !pair.Value.Contains((ushort) 0))
                        {
                            UnityEngine.Debug.Log(_ruleNum);
                            trueRules.Remove(_ruleNum); // если нет, то удаляю
                            break;
                        }
                    }
                }
            }
            if (trueRules.Count == 0)
                throw new Exception("Imossible Tile at "+ pos);
        }

        private void ReCalcPriority()
        {
            uint sum_weights = 0;
            float sum_log_weights = 0;
            foreach (var num in trueRules)
            {
                sum_weights += tiles[rules[num].Item2].priority;
                sum_log_weights += (float) (tiles[rules[num].Item2].priority * Math.Log(tiles[rules[num].Item2].priority));
            }

            priority = (float) ((Math.Log(sum_weights) * (1 - (float) rand.Next(10) / 100) - sum_log_weights / sum_weights) * Math.Log(Math.Sqrt(Math.Pow(pos.Item1, 2) + Math.Pow(pos.Item2, 2))));
            gen.MoveTile(pos);
        }

        private void DoReflect(ushort type)
        {
            IEnumerable<Dictionary<(int, int), ushort[]>> _rules = trueRules.Select(x => rules[x]).Where(x => x.Item2 == type).Select(x => x.Item1);
            Dictionary<(int, int), List<ushort>> megaRule = new Dictionary<(int, int), List<ushort>>();

            foreach (Dictionary<(int, int), ushort[]> rule in _rules)
            {
                foreach (KeyValuePair<(int, int), ushort[]> pair in rule)
                {
                    if (!megaRule.ContainsKey(pair.Key))
                        megaRule[pair.Key] = new List<ushort>();
                }
            }

            foreach (Dictionary<(int, int), ushort[]> rule in _rules)
            {
                foreach (var pair in megaRule.Keys.ToDictionary(x => x, x => megaRule[x]))
                {
                    if (!rule.ContainsKey(pair.Key))
                        megaRule[pair.Key] = new List<ushort>() {0};
                    else if (megaRule[pair.Key].Count == 0 || megaRule[pair.Key][0] != 0)
                        megaRule[pair.Key] = rule[pair.Key].ToList();
                }
            }

            foreach (KeyValuePair<(int, int), List<ushort>> pair in megaRule)
            {
                (int, int) _pos = TriMapUtil.GetWorldPos(pos, pair.Key);

                if (!map.ContainsKey(_pos))
                {
                    gen.AddTile(_pos);
                }
                map[_pos].AddDependent(this);
                map[_pos].Reflect(pos, type, pair.Value.ToArray());
            }
            SetNeighbors();
        }

        public void Reflect((int, int) _pos, ushort type, ushort[] needTypes)
        {
            if (done)
                return;
            _pos = TriMapUtil.GetLocalPos(pos, _pos);

            if (needTypes.Length != 0 && needTypes.First() != 0)
            foreach (var _ruleNum in trueRules.ToArray())
            {
                if (!needTypes.Contains(rules[_ruleNum].Item2))
                {
                    trueRules.Remove(_ruleNum);
                }
            }

            foreach (var _ruleNum in trueRules.ToArray())
            {
                Dictionary<(int, int), ushort[]> rule = rules[_ruleNum].Item1; // беру правило
                if (!(rule.TryGetValue(_pos, out ushort[] val)? val.Contains(type) || val[0] == 0 : true)) // проверяю на соответствие (0 - любой)
                {
                    trueRules.Remove(_ruleNum);
                }
            }

            ReCalcPriority();
        }
        private void SetNeighbors()
        {
            foreach (var _lpos in new (int, int)[] {(1, 0), (-1, 0), (1, -1)})
            {
                (int, int) _pos = TriMapUtil.GetWorldPos(pos, _lpos);

                if (!map.ContainsKey(_pos))
                {
                    gen.AddTile(_pos);
                }
            }
        }
    }
}
}
