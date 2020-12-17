using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MapGenerator : MonoBehaviour
{
    [SerializeField]
    private TileType[] _tiles;
    private Dictionary<ushort, TileType> tiles = new Dictionary<ushort, TileType>();
    private List<(Dictionary<(int, int), ushort[]>, ushort)> rules;

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
}

[System.Serializable]
public class TileType
{
    [SerializeField]
    public readonly ushort id;
    [SerializeField]
    private readonly string name = "None";
    [SerializeField]
    public readonly int priority = 1;
    public readonly bool allowZeros;
    [SerializeField]
    private NeighborList[] _neighbors;
    private List<(Dictionary<(int, int), ushort[]>, ushort)> rules = new List<(Dictionary<(int, int), ushort[]>, ushort)>();
    public List<(Dictionary<(int, int), ushort[]>, ushort)> GetRules() => rules;

    public void Awake()
    {
        foreach(NeighborList variation in _neighbors)
        {
            Dictionary<(int, int), ushort[]> var = new Dictionary<(int, int), ushort[]>();
            foreach(Neighbor neighbor in variation.variation)
            {
                var[(neighbor.posX, neighbor.posY)] = neighbor.types;
            }
            rules.Add((var, id));
        }
        _neighbors = null;
    }

    [System.Serializable]
    private class NeighborList
    {
        public Neighbor[] variation;
    }

    [System.Serializable]
    private class Neighbor
    {
        public int posX;
        public int posY;
        public ushort[] types;
    }
}
