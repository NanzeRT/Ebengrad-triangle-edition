using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

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
            if (pair.Value.id != 0)
                typeMap[pair.Key] = pair.Value.id;
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
            float priority = tile.priority;
            for (int i = 0; i < this.Count; i++)
            {
                if (priority < this[i].priority)
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
            _id = type;
            gen = g;
            pos = p;
            map = mp;
            tiles = t;
            rules = r;
            trueRules = Enumerable.Range(0, r.Count).Select(x => (ushort) x).ToList();
            //DoReflect(type);
            //Clear();
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

        private float _priority;
        public float priority {get => _priority;}
        private ushort _id = 0;
        public ushort id {get => _id;}
        private readonly Generator gen;
        private readonly Random rand;
        private readonly (int, int) pos;
        private readonly Dictionary<(int, int), Tile> map;
        private readonly Dictionary<ushort, TileType> tiles;
        private List<Tile> dependent = new List<Tile>();
        private readonly List<(Dictionary<(int, int), ushort[]>, ushort)> rules;
        public List<ushort> trueRules;

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
            IEnumerable<ushort> types = trueRules.Select(x => rules[x].Item2).Distinct();
            
            return types.ToDictionary(k => k, v => _getPriority(v));
        }

        private ushort _getPriority(ushort type)
        {
            return tiles[type].priority;
        }

        private bool _isValid(ushort type) => true;

        private void _setType(ushort type)
        {
            _id = type;
            // _doReflect();
        }

        private void _doReflect()
        {
            
        }
    }
}
