using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Generator
{
    public Generator(List<(Dictionary<(int, int), ushort[]>, ushort)> r, Dictionary<ushort, TileType> t)
    {
        rules = r;
        tiles = t;
    }
    private readonly List<(Dictionary<(int, int), ushort[]>, ushort)> rules;
    private readonly Dictionary<ushort, TileType> tiles;
    private Dictionary<(int, int), Tile> map;
    private TileQueue tileQueue;
    private Random rand;

    public List<List<ushort>> Generate(int seed)
    {
        rand = new Random(seed);
        return default;
    }

    public void AddTile ((int, int) pos)
    {
        map[pos] = new Tile(rules, tiles, rand, map, pos, this);
        tileQueue.Add(map[pos]);
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
                if (priority > this[i].GetPriority())
                {
                    this.Insert(i, tile);
                    return;
                }
                base.Add(tile);
            }
        }

        public void Move(Tile tile)
        {
            this.Remove(tile);
            this.Add(tile);
        }
    }

    private class Tile
    {
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
        private readonly List<(Dictionary<(int, int), ushort[]>, ushort)> rules;
        public List<ushort> trueRules;
        public ushort Collapse()
        {
            IEnumerable<ushort> posibleTypes = trueRules.Select(x => rules[x].Item2).Distinct();
            int sum = 0;
            foreach (ushort type in posibleTypes)
            {
                sum += tiles[type].priority;
            }

            float r = rand.Next(sum);
            
            foreach (ushort type in posibleTypes)
            {
                sum -= tiles[type].priority;
                if (sum <= 0)
                    if (IsValid(type))
                    {
                        DoReflect(type);
                        id = type;
                        return type;
                    }
                    else
                        return Collapse();
            }

            throw (new Exception("Empty tile tryed to Collapse()"));
        }

        private bool IsValid(ushort type)
        {
            IEnumerable<ushort> tRules = trueRules.Where(x => rules[x].Item2 == type); // выбираю номера правил, которые соответствуют типу
            foreach (ushort _ruleNum in tRules)
            {
                bool valid = true;
                Dictionary<(int, int), ushort[]> rule = rules[_ruleNum].Item1; // беру правило
                foreach (KeyValuePair<(int, int), ushort[]> pair in rule)
                {
                    if ((map.TryGetValue((pair.Key.Item1 + pos.Item1, pair.Key.Item2 + pos.Item2), out Tile val)? val.GetId() : 0) == 0 && tiles[type].allowZeros // проверяю на соответствие (0 - любой)
                    || pair.Value.Contains(val.GetId()))
                    {
                        continue;
                    } else {
                        trueRules.Remove(_ruleNum); // если нет, то удаляю
                        valid = false;
                        break;
                    }
                }
                if (valid)
                    return true;
            }
            return false;
        }

        private void Clear()
        {
            trueRules = null;
            done = true;
        }

        private void DoReflect(ushort type)
        {
            IEnumerable<Dictionary<(int, int), ushort[]>> _rules = trueRules.Select(x => rules[x]).Where(x => x.Item2 == type).Select(x => x.Item1);
            Dictionary<(int, int), List<ushort>> megaRule = new Dictionary<(int, int), List<ushort>>();

            foreach (Dictionary<(int, int), ushort[]> rule in _rules)
            {
                foreach (KeyValuePair<(int, int), ushort[]> pair in rule)
                {
                    if (megaRule.ContainsKey(pair.Key))
                        megaRule[pair.Key].AddRange(pair.Value);
                    else
                        megaRule[pair.Key] = pair.Value.ToList();
                }
            }

            foreach (KeyValuePair<(int, int), List<ushort>> pair in megaRule)
            {
                (int, int) _pos = (pair.Key.Item1 + pos.Item1, pair.Key.Item2 + pos.Item2);

                if (!map.ContainsKey(_pos))
                {
                    gen.AddTile(_pos);
                }
                map[_pos].Reflect(pos, type, pair.Value.ToArray());
            }
        }

        public void Reflect((int, int) _pos, ushort type, ushort[] needTypes)
        {
            if (done)
                return;
            _pos = (_pos.Item1 - pos.Item1, _pos.Item2 - pos.Item2);

            if (needTypes.Length != 0 && needTypes.First() != 0)
            foreach (var _ruleNum in trueRules)
            {
                if (!needTypes.Contains(rules[_ruleNum].Item2))
                {
                    trueRules.Remove(_ruleNum);
                }
            }

            foreach (var _ruleNum in trueRules)
            {
                Dictionary<(int, int), ushort[]> rule = rules[_ruleNum].Item1; // беру правило
                if (!(rule.TryGetValue(_pos, out ushort[] val)? val.Contains(type) || val[0] == 0 : true)) // проверяю на соответствие (0 - любой)
                {
                    trueRules.Remove(_ruleNum);
                }
            }
        }
    }
}

