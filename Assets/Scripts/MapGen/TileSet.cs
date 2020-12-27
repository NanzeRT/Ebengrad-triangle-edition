using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace TileSetNS
{
    public class TileSet
    {
        public TileSet() {}

        public void AddTile(Tile tile)
        {
            tiles[tile.id] = tile;
            rules.AddRange(tile.rules);
        }

        List<Rule> rules;
        Dictionary<ushort, Tile> tiles;

    }

    public class Tile
    {
        public Tile(ushort id, UnityEngine.GameObject prefab, ushort priority, bool allowAllZeros)
        {
            _id = id;
            _priority = priority;
            _allowAllZeros = allowAllZeros;
        }

        public void AddRule(Rule rule)
        {
            _rules.Add(rule);
            rule.tile = this;
        }

        private List<Rule> _rules;
        public List<Rule> rules { get => _rules; }
        private ushort _id;
        public ushort id { get => _id; }
        private UnityEngine.GameObject _pref;
        public UnityEngine.GameObject pref { get => _pref; }
        private ushort _priority = 1;
        public ushort priority { get => _priority; }
        private bool _allowAllZeros;
        public bool allowAllZeros { get => _allowAllZeros; }
    }

    public class Rule
    {
        public Rule(List<RulePair> p)
        {
            pairs = p;
        }

        List<RulePair> pairs;
        private Tile _tile = null;
        public Tile tile { get => _tile; set => _tile = _tile is null ? value : throw new Exception("Second tile asigment in rule!"); }



        public void AddPair(RulePair pair)
        {
            pairs.Add(pair);
        }
    }

    public class RulePair
    {
        public RulePair(int x, int y, ushort[] t)
        {
            pos = (x, y);
            types = t;
        }

        (int, int) pos;
        ushort[] types;
    }
}