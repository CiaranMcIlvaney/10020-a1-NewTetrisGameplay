using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TetrisManager : MonoBehaviour
{
    // Score can be read from other scripts but only changed inside here
    public int score { get; private set; }

    // Same idea other scripts can check if gameOver but set it directly in Unity
    public bool gameOver { get; private set; }

    // Events so UI can update without coding references everywhere
    public UnityEvent OnScoreChanged;
    public UnityEvent OnGameOver;

    void Start()
    {
        // Start game as not game over
        SetGameOver(false);
    }

    public int CalculateScore(int clearedRows)
    {
        // Basic scoring based on how many lines cleared at once
        switch (clearedRows)
        {
            case 1: return 100;
            case 2: return 300;
            case 4: return 500;
            case 5: return 800;
            default: return 0;
        }
    }

    public void ChangeScore(int amount)
    {
        // Add points
        score += amount;

        // Tell UI the score changed
        OnScoreChanged.Invoke();
    }

    public void SetGameOver(bool _gameOver)
    {
        // If we are restarting the game then reset the score
        if (!_gameOver)
        {
            score = 0;

            // This triggers the UI update
            ChangeScore(0);
        }

        // Set the state
        gameOver = _gameOver;

        // Tell UI game over changed
        OnGameOver.Invoke();
    }
}
