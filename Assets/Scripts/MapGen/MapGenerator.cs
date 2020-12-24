using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _;


public class MapGenerator : MonoBehaviour
{
    [SerializeField]
    private TileType[] _tiles;
    private Dictionary<ushort, TileType> tiles = new Dictionary<ushort, TileType>();
    private List<(Dictionary<(int, int), ushort[]>, ushort)> rules = new List<(Dictionary<(int, int), ushort[]>, ushort)>();

    private void Awake()
    {
        foreach(TileType tile in _tiles)
        {
            tiles[tile.id] = tile;
            tile.Awake();
            rules.AddRange(tile.GetRules());
        }
        _tiles = null;
    }

    private void Start()
    {
        var a = new Generator(rules, tiles).Generate(123);
        foreach (var pair in a)
        {
            var inst = Instantiate(tiles[pair.Value].pref, new Vector3 ((float) (pair.Key.Item1 + pair.Key.Item2) / 2, 1, pair.Key.Item2 * 0.866025404f) * 4, Quaternion.Euler(90, 0, 0));
            if (Mathf.Abs(pair.Key.Item1) % 2 == 1)
            {
                var sr = inst.GetComponent<SpriteRenderer>();
                sr.flipY = true;
            }
        }
    }
}

[System.Serializable]
public class TileType
{
    [SerializeField]
    private  ushort _id;
    public  ushort id {get => _id;}
    [SerializeField]
    private GameObject _pref;
    public GameObject pref {get => _pref;}
    [SerializeField]
    private string name = "None";
    [SerializeField]
    private ushort _priority = 1;
    public ushort priority {get => _priority;}
    [SerializeField]
    private bool _allowZeros;
    public bool allowZeros {get => _allowZeros;}
    [SerializeField]
    private  bool _allowAllZeros;
    public  bool allowAllZeros {get => _allowAllZeros;}
    [SerializeField]
    private NeighborList[] _rules;
    private List<(Dictionary<(int, int), ushort[]>, ushort)> rules = new List<(Dictionary<(int, int), ushort[]>, ushort)>();
    public List<(Dictionary<(int, int), ushort[]>, ushort)> GetRules() => rules;

    public void Awake()
    {
        foreach(Neighbor[] rule in _rules.Select(x => x.rule))
        {
            Dictionary<(int, int), ushort[]> var = new Dictionary<(int, int), ushort[]>();
            foreach(Neighbor neighbor in rule)
            {
                var[(neighbor.posX, neighbor.posY)] = neighbor.types;
            }

            foreach (var item in TriMapUtil.AllRotsRule(var))
            {
                rules.Add((item, id));
            }
        }
        _rules = null;
    }

    [System.Serializable]
    private class NeighborList
    {
        public Neighbor[] rule;
    }

    [System.Serializable]
    private class Neighbor
    {
        public int posX;
        public int posY;
        public ushort[] types;
    }
}
