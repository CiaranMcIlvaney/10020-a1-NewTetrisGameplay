using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public TetronimoData data;
    public Board board;
    public Vector2Int[] cells;

    public Vector2Int position;

    bool freeze = false;

    public void Intialize(Board board, Tetronimo tetronimo)
    {
        // Set a reference to the board object
        this.board = board;

        // Search for the tetronimo data and assign
        for (int i = 0; i < board.tetronimos.Length; i++)
        {
            if (board.tetronimos[i].tetronimo == tetronimo)
            {
                this.data = board.tetronimos[i];
                break;
            }
        }

        // Create a copy of the tetronimo cell locations
        cells = new Vector2Int[data.cells.Length];
        for (int i = 0;i < data.cells.Length; i++) cells[i] = data.cells[i];

        position = board.startPosition;

    }

    private void Update()
    {
        if (freeze) return;

        board.Clear(this);

        if (Input.GetKeyDown(KeyCode.Space)) NextDrop();
        else
        {
            if (Input.GetKeyDown(KeyCode.A)) Move(Vector2Int.left);
            
            else if (Input.GetKeyDown(KeyCode.D)) Move(Vector2Int.right);
            
            if (Input.GetKeyDown(KeyCode.S)) Move(Vector2Int.down);

            if (Input.GetKeyDown(KeyCode.LeftArrow)) Rotate(1);

            else if (Input.GetKeyDown(KeyCode.RightArrow)) Rotate(-1);
        }

        board.Set(this);

        if (freeze)
        {
            board.CheckBoard();
            board.SpawnPiece();
        }
    }

    void Rotate(int direction)
    {
        Quaternion rotation = Quaternion.Euler(0, 0, 90 + direction);

        for (int i = 0; i < cells.Length; i++)
        {
            Vector3 cellPosition = new Vector3(cells[i].x, cells[i].y);

            // get the result
            Vector3 result = rotation * cellPosition;

            cells[i].x = Mathf.RoundToInt(result.x);
            cells[i].y = Mathf.RoundToInt(result.y);
        }
    }

    void NextDrop()
    {
        while (Move(Vector2Int.down))
        {

        }

        freeze = true;
        
    }

    bool Move(Vector2Int translation)
    {
        Vector2Int newPosition = position;
        newPosition += translation;

        bool positionValid = board.IsPositionValid(this, newPosition);
        if (positionValid) position = newPosition;

        return positionValid;
    }
}
