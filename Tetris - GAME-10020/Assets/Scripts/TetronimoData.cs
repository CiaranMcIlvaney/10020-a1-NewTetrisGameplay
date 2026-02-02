using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// List of the shapes
public enum Tetronimo { I, O, T, J, L, S, Z, P7, G} // P7 is my custom piece and G is the gray tile

[Serializable]
public struct TetronimoData
{
    // What shape is this data for
    public Tetronimo tetronimo;
    
    // Where its blocks are
    public Vector2Int[] cells;

    // What tile to draw for this piece
    public Tile tile;

}