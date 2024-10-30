using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // static ref

    public GameObject backgroundPanel; // grey background
    public GameObject victoryPanel;
    public GameObject losePanel;
    public GameObject pausePanel;
    public GameObject pauseButton;

    public int goal; // amount of points to win.
    public int moves; // number of turns to win
    public int points; // the current points you have earned
    public float timeLeft; // seconds remaining to complete the level

    public int blueToMatch = 3;
    public int greenToMatch = 3;
    public int purpleToMatch = 3;
    public int redToMatch = 3;
    public int yellowToMatch = 3;

    public bool isGameEnded;
    public bool timerOn = true;

    public TMP_Text pointsTxt;
    public TMP_Text movesTxt;
    public TMP_Text timeTxt;

    public TMP_Text yellowTxt;
    public TMP_Text redTxt;
    public TMP_Text blueTxt;
    public TMP_Text purpleTxt;
    public TMP_Text greenTxt;

    private void Awake()
    {
        Instance = this;
    }

    public void Initialize(int _moves, int _goal)
    {
        moves = _moves;
        goal = _goal;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        pointsTxt.text = "Points: " + points.ToString();
        movesTxt.text = "Moves: " + moves.ToString();

        redTxt.text    = "Red Left: " + redToMatch.ToString();
        blueTxt.text   = "Blue Left: " + blueToMatch.ToString();
        purpleTxt.text = "Purple Left: " + purpleToMatch.ToString();
        greenTxt.text  = "Green Left: " + greenToMatch.ToString();
        yellowTxt.text = "Yellow Left: " + yellowToMatch.ToString();

        if (timerOn)
        {
            if(timerOn)
            {
                if (timeLeft > 0)
                {
                    timeLeft -= Time.deltaTime;
                    UpdateTimer(timeLeft);
                }
                else
                {
                    Debug.Log("Time is UP!");
                    timeLeft = 0;
                    isGameEnded = true;
                    backgroundPanel.SetActive(true);
                    losePanel.SetActive(true);
                    Board.Instance.candyParent.SetActive(false);
                    timerOn = false;
                }
            }
        }
    }

    /*Red,
    Blue,
    Purple,
    Green,
    Yellow */

    public void TypeMatch(CandyType type)
    {
        switch (type)
        {
            case CandyType.Red:
                if (redToMatch > 0)
                {
                    redToMatch--;
                }            
                break;

            case CandyType.Green:
                if (greenToMatch > 0)
                {
                    greenToMatch--;
                }
                break;

            case CandyType.Blue:
                if (blueToMatch > 0)
                {
                    blueToMatch--;
                }
                break;
            
            case CandyType.Yellow:
                if (yellowToMatch > 0)
                {
                    yellowToMatch--;
                }
                break;

            case CandyType.Purple:
                if (purpleToMatch > 0)
                {
                    purpleToMatch--;
                }
                break;
        }
    }

    public void ProcessTurn(int _pointsToGain, bool _subtractMoves)
    {
        points += _pointsToGain;
        if (_subtractMoves)
            moves--;

        if (blueToMatch == 0 && redToMatch == 0 && purpleToMatch == 0 && yellowToMatch == 0 && greenToMatch == 0)
        {
            //trigger win
            isGameEnded = true;
            //Display victory screen
            backgroundPanel.SetActive(true);
            victoryPanel.SetActive(true);
            timerOn = false;
            Board.Instance.candyParent.SetActive(false);
            return;
        }
        if (moves == 0)
        {
            //lose the game
            isGameEnded = true;
            backgroundPanel.SetActive(true);
            losePanel.SetActive(true);
            timerOn = false;
            Board.Instance.candyParent.SetActive(false);
            return;
        }
    }

    public void PauseGame()
    {
        Board.Instance.candyParent.SetActive(false);
        pausePanel.SetActive(true);
        pauseButton.SetActive(false);
        timerOn = false;
    }

    public void ResumeGame()
    {
        Board.Instance.candyParent.SetActive(true);
        pausePanel.SetActive(false);
        pauseButton.SetActive(true);
        timerOn = true;
    }

    //attached to a button to change scene when winning
    public void WinGame()
    {
        SceneManager.LoadScene(0);
    }

    public void LoseGame()
    {
        SceneManager.LoadScene(0);
    }

    void UpdateTimer(float currentTime)
    {
        currentTime += 1;

        float seconds = Mathf.FloorToInt(currentTime);

        timeTxt.text = string.Format("Time: {00}", seconds);
    }
}
