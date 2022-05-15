using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public enum GameMode
    {
        GamePaused,
        PlayersTurn,
        MinotaursTurn
    }

    public struct SavedPositions
    {
        public Vector3 playerLastPosition;
        public Vector3 minotaurLastPosition;
    }

    [SerializeField] private GameObject _nextButton;
    [SerializeField] private GameObject _previousButton;
    [SerializeField] private GameObject _undoButton;
    [SerializeField] private GameObject _messagePanel;
    [SerializeField] private GameObject _infoPanel;

    private static GameMode _currentTurn = GameMode.PlayersTurn;

    public static GameMode CurrentTurn => _currentTurn;

    private List<SavedPositions> _savedPositions;

    private PlayerMovement _player;
    private MinotaurMovement _minotaur;
    private LevelEnd _levelEnd;

    private void Awake()
    {
        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        _minotaur = GameObject.FindGameObjectWithTag("Minotaur").GetComponent<MinotaurMovement>();
        _levelEnd = FindObjectOfType<LevelEnd>();
    }

    private void Start()
    {
        _nextButton.GetComponent<Button>().interactable = false;
        _undoButton.GetComponent<Button>().interactable = false;
        _messagePanel.SetActive(false);
        _infoPanel.SetActive(false);

        _savedPositions = new List<SavedPositions>();
        // adds starting positions to the list of saved positions
        _savedPositions.Add(new SavedPositions()
        {
            playerLastPosition = _player.transform.position,
            minotaurLastPosition = _minotaur.transform.position
        });

        // the player always has the first turn
        _currentTurn = GameMode.PlayersTurn;

        // if this is the first scene in the build
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            // deactivates the button that allows the player to go back one level
            _previousButton.GetComponent<Button>().interactable = false;
        }
    }

    private void OnEnable()
    {
        _player.OnEndOfTurn += ChangeTurn;
        _minotaur.OnEndOfTurn += ChangeTurn;
        _minotaur.OnGameOver += EndGame;
        _levelEnd.OnLevelFinished += FinishLevel;
    }

    private void OnDisable()
    {
        _player.OnEndOfTurn -= ChangeTurn;
        _minotaur.OnEndOfTurn -= ChangeTurn;
        _minotaur.OnGameOver -= EndGame;
        _levelEnd.OnLevelFinished -= FinishLevel;
    }

    public void LoadNextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void LoadPreviousLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    public void ReloadLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void WaitTurn()
    {
        // if this is the player's turn, makes it the Minotaur's turn instead
        if (_currentTurn == GameMode.PlayersTurn)
        {
            _currentTurn = GameMode.MinotaursTurn;
        }
    }

    public void Undo()
    {
        // finds last positions before current move
        SavedPositions lastPositions = GetLastPositions();

        // sets positions of the player and the Minotaur
        _player.transform.SetPositionAndRotation(lastPositions.playerLastPosition, _player.transform.rotation);
        _minotaur.transform.SetPositionAndRotation(lastPositions.minotaurLastPosition, _minotaur.transform.rotation);
    }

    public void ToggleInfoPanel()
    {
        _infoPanel.SetActive(!_infoPanel.activeSelf);
    }

    private void ChangeTurn()
    {
        // if the game is paused, turns won't be alternating anymore
        if (_currentTurn == GameMode.GamePaused) return;

        // alternates between the player's and the Minotaur's turn
        if (_currentTurn == GameMode.PlayersTurn)
        {
            _currentTurn = GameMode.MinotaursTurn;

            // deactivates button allowing to undo last move
            _undoButton.GetComponent<Button>().interactable = false;
        }
        else
        {
            _currentTurn = GameMode.PlayersTurn;

            // activates button allowing to undo last move
            _undoButton.GetComponent<Button>().interactable = true;

            // adds current positions to the list of saved positions
            _savedPositions.Add(new SavedPositions()
            {
                playerLastPosition = _player.transform.position,
                minotaurLastPosition = _minotaur.transform.position
            });
        }
    }

    private SavedPositions GetLastPositions()
    {
        // gets the last positions before current ones
        SavedPositions lastPositions = _savedPositions[_savedPositions.Count - 2];

        // if there is only one set of positions in the list, skip 
        if (_savedPositions.Count >= 2)
        {
            // removes last positions from the list
            _savedPositions.RemoveAt(_savedPositions.Count - 1);

            // if we are back to the first positions recorded
            if (_savedPositions.Count <= 1)
            {
                // deactivates button allowing to undo last move
                _undoButton.GetComponent<Button>().interactable = false;
            }
        }

        return lastPositions;
    }

    private void FinishLevel()
    {
        // pauses the game
        _currentTurn = GameMode.GamePaused;

        // deactivates button allowing to undo last move
        _undoButton.GetComponent<Button>().interactable = false;

        // if this is not the last scene in the build
        if (SceneManager.GetActiveScene().buildIndex < SceneManager.sceneCountInBuildSettings - 1)
        {
            // activates the button that allows the player to skip to the next level
            _nextButton.GetComponent<Button>().interactable = true;
        }

        // activates panel with a congratulating message
        _messagePanel.SetActive(true);
        _messagePanel.GetComponentInChildren<TextMeshProUGUI>().text = "Congratulations!";
    }

    private void EndGame()
    {
        // pauses the game
        _currentTurn = GameMode.GamePaused;
        // activates panel with "game over" message
        _messagePanel.SetActive(true);
        _messagePanel.GetComponentInChildren<TextMeshProUGUI>().text = "Too bad!";
        // deactivates button allowing to undo last move
        _undoButton.GetComponent<Button>().interactable = false;
    }
}