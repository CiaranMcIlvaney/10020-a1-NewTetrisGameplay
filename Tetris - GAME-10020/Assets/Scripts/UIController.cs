using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    // UI text that shows the score
    public TextMeshProUGUI scoreText;

    // Reference to tetris manager so we can read the score and gameOver
    public TetrisManager tetrisManager;

    // Panel that pops up when you lose 
    public GameObject endGamePanel;
    public void UpdateScore()
    {
        // Show score 
        scoreText.text = $"Score: {tetrisManager.score:n0}";
    }

    public void UpdateGameOver()
    {
        // Show/Hide the end game panel depending on gameOver
        endGamePanel.SetActive(tetrisManager.gameOver);
    }

    public void PlayAgain()
    {
        // Setting the game over to false resets the game
        tetrisManager.SetGameOver(false);
    }

}
