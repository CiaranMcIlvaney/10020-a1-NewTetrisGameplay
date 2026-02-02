using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Piece : MonoBehaviour
{
    // What shape/tile this piece is
    public TetronimoData data;

    // Board reference so we can ask for collision checks + set/clear tiles
    public Board board;

    // The blocks that make up this piece
    public Vector2Int[] cells;

    // Wgere the piece is on the board
    public Vector2Int position;

    // If true this piece is donne moving and is now "placed"
    public bool freeze = false;

    // How many blocks are still alive for this piece 
    int activeCellCount = -1;

    public void Intialize(Board board, Tetronimo tetronimo)
    {
        // Save board reference
        this.board = board;

        // Find the right data from the board list
        for (int i = 0; i < board.tetronimos.Length; i++)
        {
            if (board.tetronimos[i].tetronimo == tetronimo)
            {
                this.data = board.tetronimos[i];
                break;
            }
        }

        // Copy the cell positions into this piece
        cells = new Vector2Int[data.cells.Length];
        for (int i = 0;i < data.cells.Length; i++) cells[i] = data.cells[i];

        // Spawn position
        position = board.startPosition;

        // Start with full block count
        activeCellCount = cells.Length;
    }

    private void Update()
    {
        if (board == null || board.tetrisManager == null) return;

        // Stop inputs if game is over
        if (board.tetrisManager.gameOver) return;

        // Stop inputs if frozen
        if (freeze) return;

        // Clear before moving so we dont collide with ourselves
        board.Clear(this);

        // Space does hard drop
        if (Input.GetKeyDown(KeyCode.Space)) NextDrop();
        else
        {   
            // Left/Right movement
            if (Input.GetKeyDown(KeyCode.A)) Move(Vector2Int.left);
            else if (Input.GetKeyDown(KeyCode.D)) Move(Vector2Int.right);
            
            // Soft drop
            if (Input.GetKeyDown(KeyCode.S)) Move(Vector2Int.down);

            // Rotate
            if (Input.GetKeyDown(KeyCode.LeftArrow)) Rotate(1);
            else if (Input.GetKeyDown(KeyCode.RightArrow)) Rotate(-1);
        }

        // Draw piece again after movement 
        board.Set(this);
    }

    void Rotate(int direction)
    {
        // Save old cells so we can undo if rotation fails
        Vector2Int[] originalCells = new Vector2Int[cells.Length];
        for (int i = 0; i < cells.Length; i++) originalCells[i] = cells[i];

        // Actually rotate
        ApplyRotation(direction);

        // If rotation is invalid then try wall kicks
        if (!board.IsPositionValid(this, position))
        {
            if (!TryWallKicks())
            {
                // If no wall kicks worked revert rotation
                RevertRotation(originalCells);
            }   
        }
    }

    void RevertRotation(Vector2Int[] originalCells)
    {
        // Put everything back how it was
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i] = originalCells[i];
        }
    }

    bool TryWallKicks()
    {
        // A list of offsets to try so the piece can rotate near walls/blocks
        List<Vector2Int> wallKickOffsets = new List<Vector2Int>
        {
            Vector2Int.left,
            Vector2Int.right,
            Vector2Int.down,
            new Vector2Int(-1, -1), // diagonal down-left
            new Vector2Int(1, -1), // diagonal down-right
        };

        // I piece is extra long so it gets extra kick attempts
        if (data.tetronimo == Tetronimo.I)
        {
            wallKickOffsets.Add(2 * Vector2Int.left);
            wallKickOffsets.Add(2 * Vector2Int.right);
        }

        // Try each offset and see if it becomes valid
        foreach (Vector2Int offset in wallKickOffsets)
        {
            if (Move(offset)) return true;
        }

        return false;
    }

    void ApplyRotation(int direction)
    {
        // Rotate 90 degrees 
        Quaternion rotation = Quaternion.Euler(0, 0, 90 + direction);

        // I and O have weird rotation centers so we handle them speical
        bool isSpecial = data.tetronimo == Tetronimo.I || data.tetronimo == Tetronimo.O;
        for (int i = 0; i < cells.Length; i++)
        {
            Vector3 cellPosition = new Vector3(cells[i].x, cells[i].y);

            // Shift pivot so rotation works nicer for I and O
            if (isSpecial)
            {
                cellPosition.x -= 0.5f;
                cellPosition.y -= 0.5f;
            }

            // Apply rotation math
            Vector3 result = rotation * cellPosition;

            // Convert back to int grid positions
            if (isSpecial)
            {
                cells[i].x = Mathf.CeilToInt(result.x);
                cells[i].y = Mathf.CeilToInt(result.y);
            }
            else
            {
                cells[i].x = Mathf.RoundToInt(result.x);
                cells[i].y = Mathf.RoundToInt(result.y);
            }

        }
    }

    void NextDrop()
    {
        // Keep moving down until it falls
        while (Move(Vector2Int.down))
        {
            // Nothing needed in here just keep looping
        }
        
        // Once it fails freeze it 
        freeze = true;
        
    }

    public bool Move(Vector2Int translation)
    {
        // Where we want to go
        Vector2Int newPosition = position;

        newPosition += translation;

        // Check if that spot is allowed
        bool positionValid = board.IsPositionValid(this, newPosition);

        // Only move if its valid
        if (positionValid) position = newPosition;

        return positionValid;
    }

    public void ReduceActiveCount()
    {
        // Line clear removed one of our blocks
        activeCellCount -= 1;
       
        // If all blocks are gone delete this piece object
        if (activeCellCount <= 0)
        {
            Destroy(gameObject);
        }
    }
}
