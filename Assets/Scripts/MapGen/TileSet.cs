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
            _tiles[tile.Id] = tile;
            _rules.AddRange(tile.Rules);
        }

        public void FinalInit()
        {
            for (ushort num = 0; num < Rules.Count; num++)
            {
                foreach (var pos in Rules[num].GetPoss())
                {
                    if (!_posToRuleNums.ContainsKey(pos))
                        _posToRuleNums[pos] = new List<ushort>();
                    _posToRuleNums[pos].Add(num);


                    if (!_typeCountOnPos.ContainsKey(pos))
                        _typeCountOnPos[pos] = new Dictionary<ushort, ushort>();
                    
                    foreach (var type in Rules[num].Types[pos])
                    {
                        if (!_typeCountOnPos[pos].ContainsKey(type))
                            _typeCountOnPos[pos][type] = 0;
                        _typeCountOnPos[pos][type]++;
                    }


                    if (!_ruleCountOnPos.ContainsKey(pos))
                        _ruleCountOnPos[pos] = 0;
                    _ruleCountOnPos[pos]++;
                }
            }
        }

        private List<Rule> _rules = new List<Rule>();
        private Dictionary<(int, int), List<ushort>> _posToRuleNums = new Dictionary<(int, int), List<ushort>>();
        private Dictionary<ushort, Tile> _tiles = new Dictionary<ushort, Tile>();
        private Dictionary<(int, int), ushort> _ruleCountOnPos = new Dictionary<(int, int), ushort>();
        private Dictionary<(int, int), Dictionary<ushort, ushort>> _typeCountOnPos = new Dictionary<(int, int), Dictionary<ushort, ushort>>();

        public List<Rule> Rules { get => _rules; }
        public Dictionary<ushort, Tile> Tiles { get => _tiles; }
        public Dictionary<(int, int), List<ushort>> PosToRuleNums { get => _posToRuleNums; }
        public Dictionary<(int, int), Dictionary<ushort, ushort>> TypeCountOnPos { get => _typeCountOnPos; } // Dict<pos, Dict<type, count>>
        public Dictionary<(int, int), ushort> RuleCountOnPos { get => _ruleCountOnPos; }

        public IEnumerable<(int, int)> GetRulePoss(ushort ruleNum) => Rules[ruleNum].GetPoss();
    }

    public class Tile
    {
        public Tile(ushort id, UnityEngine.GameObject prefab, ushort priority, bool allowAllZeros)
        {
            _id = id;
            _pref = prefab;
            _priority = priority;
            _allowAllZeros = allowAllZeros;
        }

        public void AddRule(Rule rule)
        {
            _rules.Add(rule);
            rule.Tile = this;
        }

        private List<Rule> _rules = new List<Rule>();
        public List<Rule> Rules { get => _rules; }
        private ushort _id;
        public ushort Id { get => _id; }
        private UnityEngine.GameObject _pref;
        public UnityEngine.GameObject Pref { get => _pref; }
        private ushort _priority = 1;
        public ushort Priority { get => _priority; }
        private bool _allowAllZeros;
        public bool AllowAllZeros { get => _allowAllZeros; }
    }

    public class Rule
    {
        public Rule(List<RulePair> p)
        {
            pairs = p;
            foreach (var pair in pairs)
                _types[pair.Pos] = pair.Types;
        }

        private List<RulePair> pairs;
        private Dictionary<(int, int), ushort[]> _types = new Dictionary<(int, int), ushort[]>();
        
        private Tile _tile = null;
        public Tile Tile { get => _tile; set => _tile = _tile is null ? value : throw new Exception("Second tile asigment in rule!"); }
        public Dictionary<(int, int), ushort[]> Types { get => _types; }
        public List<RulePair> Pairs { get => pairs; }

        public void AddPair(RulePair pair)
        {
            pairs.Add(pair);
        }

        public IEnumerable<(int, int)> GetPoss()
        {
            return Pairs.Select(pair => pair.Pos);
        }
    }

    public class RulePair
    {
        public RulePair(int x, int y, ushort[] t)
        {
            _pos = (x, y);
            _types = t;
        }

        private (int, int) _pos;
        private ushort[] _types;

        public (int, int) Pos { get => _pos; }
        public ushort[] Types { get => _types; }
    }
}