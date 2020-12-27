using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileSetNS;

namespace GeneratorNS
{
    public class MapGenerator : MonoBehaviour
    {
        [SerializeField]
        private TileRaw[] _tiles;
        private Dictionary<ushort, TileRaw> tiles = new Dictionary<ushort, TileRaw>();
        private TileSet _tileSet = new TileSet();
        private List<(Dictionary<(int, int), ushort[]>, ushort)> rules = new List<(Dictionary<(int, int), ushort[]>, ushort)>();

        private void Awake()
        {
            foreach (TileRaw tile in _tiles)
            {
                _tileSet.AddTile(tile.ToTile());
            }
            _tiles = null;
        }

        private void Start()
        {
            var a = new Generator(rules, tiles).Generate(123);
            foreach (var pair in a)
            {
                var inst = Instantiate(tiles[pair.Value].pref, new Vector3((float)(pair.Key.Item1 + pair.Key.Item2) / 2, 1, pair.Key.Item2 * 0.866025404f) * 4, Quaternion.Euler(90, 0, 0));
                if (Mathf.Abs(pair.Key.Item1) % 2 == 1)
                {
                    var sr = inst.GetComponent<SpriteRenderer>();
                    sr.flipY = true;
                }
            }
        }
    }

    [System.Serializable]
    public class TileRaw
    {
        [SerializeField]
        private ushort _id;
        public ushort id { get => _id; }
        [SerializeField]
        private GameObject _pref;
        public GameObject pref { get => _pref; }
        [SerializeField]
        private string name = "None";
        [SerializeField]
        private ushort _priority = 1;
        public ushort priority { get => _priority; }
        [SerializeField]
        private bool _allowZeros;
        public bool allowZeros { get => _allowZeros; }
        [SerializeField]
        private bool _allowAllZeros;
        public bool allowAllZeros { get => _allowAllZeros; }
        [SerializeField]
        private RuleRaw[] _rules;
        private List<(Dictionary<(int, int), ushort[]>, ushort)> rules = new List<(Dictionary<(int, int), ushort[]>, ushort)>();
        public List<(Dictionary<(int, int), ushort[]>, ushort)> GetRules() => rules;

        public Tile ToTile()
        {
            Tile tile = new Tile(id, pref, priority, allowAllZeros);
            foreach (var _rule in _rules)
            {
                tile.AddRule(_rule.ToRule());
            }
            return tile;
        }

        [System.Serializable]
        private class RuleRaw
        {
            public RulePairRaw[] rule;
            public Rule ToRule() => new Rule(rule.Select(x => x.ToRulePair()).ToList());
        }

        [System.Serializable]
        private class RulePairRaw
        {
            public int posX;
            public int posY;
            public ushort[] types;
            public RulePair ToRulePair() => new RulePair(posX, posY, types);
        }
    }
}