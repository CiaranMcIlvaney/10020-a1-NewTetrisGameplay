using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public TetrisManager tetrisManager;
    public Piece prefabPiece;
    public Tilemap tilemap;

    public TetronimoData[] tetronimos;

    
    public Vector2Int boardSize;
    public Vector2Int startPosition;

    Piece activePiece;

    int left
    {
        get { return -boardSize.x / 2;  }
    }
    int right
    {
        get { return boardSize.x / 2; }
    }
    int top
    {
        get { return boardSize.y / 2; }
    }
    int bottom
    {
        get { return -boardSize.y / 2; }
    }

    public void Start()
    {
        SpawnPiece();
    }
    
    public void SpawnPiece()
    {
        activePiece = Instantiate(prefabPiece);

        Tetronimo t = (Tetronimo)Random.Range(0, tetronimos.Length);

        activePiece.Intialize(this, t);
        Set(activePiece);
    }

    // Set will color in the tiles for a piece
    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int cellPosition = (Vector3Int)(piece.cells[i] + piece.position);
            tilemap.SetTile(cellPosition, piece.data.tile);
        }
    }

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int cellPosition = (Vector3Int)(piece.cells[i] + piece.position);
            tilemap.SetTile(cellPosition, null);
        }
    }

    public bool IsPositionValid(Piece piece, Vector2Int position)
    {
        int left = -boardSize.x / 2;
        int right = boardSize.x / 2;
        int bottom = -boardSize.y / 2;
        int top = boardSize.y / 2;

        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int cellPosition = (Vector3Int)(piece.cells[i] + position);

            if (cellPosition.x < left || cellPosition.x >= right ||
                cellPosition.y < bottom || cellPosition.y >= top) return false;

            if (tilemap.HasTile(cellPosition)) return false;
        }
        return true;
    }

    bool IsLineFull(int y)
    {
        for (int x = left; x < right; x++)
        {
            Vector3Int cellPosition = new Vector3Int(x, y);
            if (!tilemap.HasTile(cellPosition)) return false;
        }
        return true;
    }

    void DestroyLine(int y)
    {
        Debug.Log("DestoryLine");
        for (int x = left; x < right; x++)
        {
            Vector3Int cellPosition = new Vector3Int(x, y);
            tilemap.SetTile(cellPosition, null);
        }
    }

    void ShiftRowsDown(int clearRow)
    {
        for (int y = clearRow + 1; y < top; y++)
        {
            Debug.Log("Shift Down {clearRow + 1}");
            for (int x = left; x < right; x++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y);

                // temporarily save the tile
                TileBase currentTile = tilemap.GetTile(cellPosition);

                // clear the tile
                tilemap.SetTile(cellPosition, null);

                cellPosition.y -= 1;
                tilemap.SetTile(cellPosition, currentTile);
            }
        }
    }

    public void CheckBoard()
    {
        List<int> destroyedLines = new List<int>();
        // scan from bottom to top
        for (int y = bottom; y < top; y++)
        {
            if (IsLineFull(y))
            {
                DestroyLine(y);
                destroyedLines.Add(y);
            }
        }

        Debug.Log(destroyedLines);

        int rowsShiftedDown = 0;
        foreach (int y in destroyedLines)
        {
            ShiftRowsDown(y - rowsShiftedDown);
            rowsShiftedDown++;
        }

        // Update the score
        int score = tetrisManager.CalculateScore(destroyedLines.Count);
        tetrisManager.ChangeScore(score);
    }
}
