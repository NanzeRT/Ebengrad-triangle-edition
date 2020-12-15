using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MapGenerator : MonoBehaviour
{
    [SerializeField]
    private TileType[] _tiles;
    private Dictionary<ushort, TileType> tiles = new Dictionary<ushort, TileType>();

    private void Awake()
    {
        foreach(TileType tile in _tiles)
        {
            tiles[tile.GetId()] = tile;
            tile.Awake();
        }
        _tiles = null;
    }
}

[System.Serializable]
public class TileType
{
    [SerializeField]
    private ushort id;
    public ushort GetId() => id;
    [SerializeField]
    private string name = "None";
    [SerializeField]
    private float priority = 1;
    public float GetPryority() => priority;
    [SerializeField]
    private NeighborList[] _neighbors;
    private List<Dictionary<(int, int), ushort[]>> neighbors = new List<Dictionary<(int, int), ushort[]>>();
    public List<Dictionary<(int, int), ushort[]>> GetCloseNeighbors() => neighbors;

    public void Awake()
    {
        foreach(NeighborList variation in _neighbors)
        {
            Dictionary<(int, int), ushort[]> var = new Dictionary<(int, int), ushort[]>();
            foreach(Neighbor neighbor in variation.variation)
            {
                var[(neighbor.posX, neighbor.posY)] = neighbor.types;
            }
            neighbors.Add(var);
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
