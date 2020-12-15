using System.Collections;
using System.Collections.Generic;

public class Generator
{
    private Dictionary<(int, int), Tile> map;
    private List<Tile> tileQuery;


    private class Tile
    {
        public float priority;
        public ushort type;
    }
}

