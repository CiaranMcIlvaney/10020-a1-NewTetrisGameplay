using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    // Main manager for the game
    public TetrisManager tetrisManager;

    // Prefab spawned every time we need a new falling piece
    public Piece prefabPiece;

    // The tilemap we are drawing the blocks onto 
    public Tilemap tilemap;

    // All the shapes + what tiles they use
    public TetronimoData[] tetronimos;

    // Board size (10x20)
    public Vector2Int boardSize;

    // Where the piece starts when it spawns
    public Vector2Int startPosition;

    // How fast the piece drops automatically
    public float dropInterval = 0.5f;

    // Timer
    float dropTime = 0.0f;

    // Keeps track of what piece own what tile spot on the grid
    Dictionary<Vector3Int, Piece> pieces = new Dictionary<Vector3Int, Piece>();

    // Current falling piece
    Piece activePiece;

    // Dummy piece used ONLY to own the gray preplaced tiles on the board
    Piece filler;

    // This is the order the pieces will spawn in for the puzzle
    Tetronimo[] puzzleSequence = new Tetronimo[]
    {
        Tetronimo.J,
        Tetronimo.T,
        Tetronimo.I,
        Tetronimo.P7,
        Tetronimo.L,
        Tetronimo.O
    };

    // Keeps track of which piece we are on in the sequence
    int sequenceIndex = 0;

    int left { get { return -boardSize.x / 2;  }}
    int right { get { return boardSize.x / 2; }}
    int top {get { return boardSize.y / 2; }}
    int bottom { get { return -boardSize.y / 2; }}

    private void Update()
    {
        
        // If game is over, dont do anything
        if (tetrisManager.gameOver) return;

        // Keep counting up time
        dropTime += Time.deltaTime;

        // If enough time is passed, drop the piece 1 cell down
        if (dropTime >= dropInterval)
        {
            // Reset the timer
            dropTime = 0.0f;  

            // Clear the piece from the map first
            Clear(activePiece);

            // Try to move down
            bool moveResult = activePiece.Move(Vector2Int.down);

            // Draw it again at the new spot
            Set(activePiece);

            // If it cannot move down that means it hit the floor or another tetris piece
            if (!moveResult)
            {
                // Freeze it so it becomes placed
                activePiece.freeze = true;

                // Clear lines if any
                CheckBoard();

                // Spawn the next place
                SpawnPiece();
            }
        }
    }

    public void SpawnPiece()
    {
        // Spawn a new piece object
        activePiece = Instantiate(prefabPiece);

        Tetronimo t;

        if (sequenceIndex < puzzleSequence.Length)
        {
            t = puzzleSequence[sequenceIndex];
            sequenceIndex++;
        }
        else
        {
            // No more pieces in the puzzle sequence
            tetrisManager.SetGameOver(true);
            return;
        }

        // Tell the piece what board it belongs to + what shape it is
        activePiece.Intialize(this, t);

        // If the spawn point is blocked then game over
        CheckEndGame();

        // Draw the piece on the tilemap
        Set(activePiece);
    }

    void CheckEndGame()
    {
        // If the piece cannot even exist at the spawn position then you are done
        if (!IsPositionValid(activePiece, activePiece.position))
        {
            tetrisManager.SetGameOver(true);
        }
    }    

    public void UpdateGameOver()
    {
        // If gameover got pressed by button then reset everything
        if (!tetrisManager.gameOver)
        {
            ResetBoard();
        }
    }

    void ResetBoard()
    {
        // Find every Piece object and delete it so that theres not left over objects still in the scene
        Piece[] foundPieces = FindObjectsByType<Piece>(FindObjectsSortMode.None);
        foreach (Piece piece in foundPieces) Destroy(piece.gameObject);

        // No active piece currently
        activePiece = null;

        // Clear all the tiles off the tilemap
        tilemap.ClearAllTiles();

        // Clear our dictionary so it matches the tilemap again
        pieces.Clear();

        // Reset the puzzle
        sequenceIndex = 0;
        
        SetupSpecialBoard();

        // Start fresh with a new piece
        SpawnPiece();
    }

    void SetTile(Vector3Int cellPosition, Piece piece)
    {
        if (piece == null)
        {
            // Remove tile from map
            tilemap.SetTile(cellPosition, null);

            // Remove from dictionary too
            pieces.Remove(cellPosition);
        }
        else
        {
            // Put the correct tile for that piece
            tilemap.SetTile(cellPosition, piece.data.tile);

            // Store who owns this cell
            pieces[cellPosition] = piece;
        }
    }
    
    // Draw a whole piece by setting every cell it has
    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            // Convert from piece local cells to board cell position
            Vector3Int cellPosition = (Vector3Int)(piece.cells[i] + piece.position);

            // Set it on the tilemap and dictionary
            SetTile(cellPosition, piece);
        }
    }

    // Erase a whole piece by clearing every cell it has
    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int cellPosition = (Vector3Int)(piece.cells[i] + piece.position);
            SetTile(cellPosition, null);
        }
    }

    // CHecks if a piece can go at a position
    public bool IsPositionValid(Piece piece, Vector2Int position)
    {
        // Bounds of the board
        int left = -boardSize.x / 2;
        int right = boardSize.x / 2;
        int bottom = -boardSize.y / 2;
        int top = boardSize.y / 2;

        for (int i = 0; i < piece.cells.Length; i++)
        {
            // Where would this cell be on the board
            Vector3Int cellPosition = (Vector3Int)(piece.cells[i] + position);

            // If outside bounds then its not valid
            if (cellPosition.x < left || cellPosition.x >= right ||
                cellPosition.y < bottom || cellPosition.y >= top) return false;

            // If the tilemap already has a tile there its blocked
            if (tilemap.HasTile(cellPosition)) return false;
        }
        return true;
    }

    // Checks if a row is fully filled with tiles
    bool IsLineFull(int y)
    {
        for (int x = left; x < right; x++)
        {
            Vector3Int cellPosition = new Vector3Int(x, y);

            // If any tile is missing then the row is not full
            if (!tilemap.HasTile(cellPosition)) return false;
        }
        return true;
    }

    // CLear a full row and updates any pieces that had blocks in that row 
    void DestroyLine(int y)
    {     
        // Loop across the entire width of the board on row y
        for (int x = left; x < right; x++)
        {
            // Get the position of this cell on the board
            Vector3Int cellPosition = new Vector3Int(x, y);

            // Check to see if this cell is owned by a piece
            if (pieces.TryGetValue(cellPosition, out Piece piece))
            {
                // If this tile belongs to the gray filler piece then remove the tile and dont affect the piece itself
                // The filler pieces are only meant to act as a static obstacle
                if (piece == filler)
                {
                    SetTile(cellPosition, null);
                    continue;
                }

                // Reduce how many active blocks this piece still has
                piece.ReduceActiveCount();

                // Remove the tile from the tilemap and dictionary
                SetTile(cellPosition, null);
            }
        }
    }

    // After a line is cleared, move everything above it down by 1 row
    void ShiftRowsDown(int clearRow)
    {
        // Start 1 row above the cleared row and go upward
        for (int y = clearRow + 1; y < top; y++)
        {
            for (int x = left; x < right; x++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y);

                // Only shift tiles that actually exist
                if (pieces.ContainsKey(cellPosition))
                {
                    // Who owns this cell
                    Piece currentPiece = pieces[cellPosition];

                    // Clear the old spot
                    SetTile(cellPosition, null);

                    // Move 1 down
                    cellPosition.y -= 1;

                    // Set it in the new spot
                    SetTile(cellPosition, currentPiece);
                }
                
            }
        }
    }

    public void CheckBoard()
    {
        // Store which rows we cleared 
        List<int> destroyedLines = new List<int>();

        // Scan from bottom to top
        for (int y = bottom; y < top; y++)
        {
            if (IsLineFull(y))
            {
                DestroyLine(y);
                destroyedLines.Add(y);
            }
        }

        // When you clear a row everything above shifts down so the next clear row index needs to be adjusted
        int rowsShiftedDown = 0;
        foreach (int y in destroyedLines)
        {
            ShiftRowsDown(y - rowsShiftedDown);
            rowsShiftedDown++;
        }

        // Update the score depending how many lines were cleared at once
        int score = tetrisManager.CalculateScore(destroyedLines.Count);
        tetrisManager.ChangeScore(score);
    }

    void SetupSpecialBoard()
    {
        // If there is not already a filler piece then make one
        if (filler == null)
        {
            filler = Instantiate(prefabPiece);

            // Make sure it bever moves or updates
            filler.freeze = true;
            filler.enabled = false;

            // Loop through the tetronimo data
            for (int i = 0; i < tetronimos.Length; i++)
            {
                // Check if this tetronimo is the gray filler type (G)
                if (tetronimos[i].tetronimo == Tetronimo.G)
                {
                    // Give the gray tetronimo data to the filler piece, making all the prefilled board blocks use the gray tile
                    filler.data = tetronimos[i];

                    // Stop looking once we find it
                    break;
                }
            }
        }

        // These positions define the starting puzzle board
        List<Vector3Int> filledCells = new List<Vector3Int>()
        {
            // Row One
            new Vector3Int(left + 0, bottom + 0, 0),
            new Vector3Int(left + 1, bottom + 0, 0),
            new Vector3Int(left + 2, bottom + 0, 0),
            new Vector3Int(left + 3, bottom + 0, 0),
            new Vector3Int(left + 4, bottom + 0, 0),
            new Vector3Int(left + 7, bottom + 0, 0),
            new Vector3Int(left + 8, bottom + 0, 0),
            new Vector3Int(left + 9, bottom + 0, 0),

            // Row Two
            new Vector3Int(left + 0, bottom + 1, 0),
            new Vector3Int(left + 1, bottom + 1, 0),
            new Vector3Int(left + 2, bottom + 1, 0),
            new Vector3Int(left + 3, bottom + 1, 0),
            new Vector3Int(left + 7, bottom + 1, 0),
            new Vector3Int(left + 8, bottom + 1, 0),

            // Row Three
            new Vector3Int(left + 0, bottom + 2, 0),
            new Vector3Int(left + 2, bottom + 2, 0),
            new Vector3Int(left + 7, bottom + 2, 0),
            new Vector3Int(left + 8, bottom + 2, 0),

            // Row Four
            new Vector3Int(left + 5, bottom + 3, 0),
            new Vector3Int(left + 6, bottom + 3, 0),
            new Vector3Int(left + 7, bottom + 3, 0),
            new Vector3Int(left + 8, bottom + 3, 0),

            // Row Five
            new Vector3Int(left + 0, bottom + 4, 0),
            new Vector3Int(left + 1, bottom + 4, 0),
            new Vector3Int(left + 2, bottom + 4, 0),
            new Vector3Int(left + 3, bottom + 4, 0),
            new Vector3Int(left + 4, bottom + 4, 0),
            new Vector3Int(left + 5, bottom + 4, 0),
            new Vector3Int(left + 6, bottom + 4, 0),
            new Vector3Int(left + 8, bottom + 4, 0),

            // Row Six
            new Vector3Int(left + 0, bottom + 5, 0),
            new Vector3Int(left + 1, bottom + 5, 0),
            new Vector3Int(left + 2, bottom + 5, 0),
            new Vector3Int(left + 3, bottom + 5, 0),
            new Vector3Int(left + 4, bottom + 5, 0),
            new Vector3Int(left + 9, bottom + 5, 0),

            // Row Seven
            new Vector3Int(left + 0, bottom + 6, 0),
            new Vector3Int(left + 1, bottom + 6, 0),
            new Vector3Int(left + 2, bottom + 6, 0),
            new Vector3Int(left + 6, bottom + 6, 0),
            new Vector3Int(left + 7, bottom + 6, 0),
            new Vector3Int(left + 8, bottom + 6, 0),
            new Vector3Int(left + 9, bottom + 6, 0),
        };

        // Actually place each tile on the board
        foreach (var cell in filledCells)
        {
            SetTile(cell, filler);
        }
    }
}
