using System;
using UnityEngine;

public class MinotaurMovement : MonoBehaviour
{
    public event Action OnEndOfTurn = delegate { };
    public event Action OnGameOver = delegate { };

    [SerializeField] private float _speed = 2.0f;
    [SerializeField] private LayerMask _obstacleLayer;

    private bool _isMoving;
    private int _movesLeft = 2;
    private Vector3 _destination;

    private GameObject _player;

    private void Awake()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        // if the Minotaur isn't already moving and if it's the Minotaur's turn
        if (!_isMoving && GameManager.CurrentTurn == GameManager.GameMode.MinotaursTurn)
        {
            GetDirection();
        }
    }

    private void FixedUpdate()
    {
        if (_isMoving)
        {
            Move();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // if the Minotaur reaches the position of the player
        if (other.CompareTag("Player"))
        {
            // fires an event informing that the game is over
            OnGameOver();
        }
    }

    private void GetDirection()
    {
        // direction of the player in relation to the Minotaur
        Vector3 direction = transform.position - _player.transform.position;

        // the Minotaur always prioritizes horizontal movement
        // if the player is either to the left or right of the Minotaur
        if (Mathf.Abs(direction.x) > 0)
        {
            // if there is no obstacle, the Minotaur moves and exits this method
            if (CanMoveHorizontal(direction)) return;
        }

        // if the player is either above or below the Minotaur
        if (Mathf.Abs(direction.z) > 0)
        {
            // if there is no obstacle, the Minotaur moves and exits this method
            if (CanMoveVertical(direction)) return;
        }

        // the Minotaur couldn't move in either direction, so he fires an event informing his turn has ended
        OnEndOfTurn();
        // sets the Minotaur's total moves back to 2
        _movesLeft = 2;
    }

    private bool CanMoveHorizontal(Vector3 direction)
    {
        // sets the destination to one unit either to the left or to the right of current position
        _destination = new Vector3(transform.position.x - Mathf.Sign(direction.x), transform.position.y, transform.position.z);

        // if there is no obstacle between the Minotaur and the destination
        if (!Physics.Linecast(transform.position, _destination, _obstacleLayer))
        {
            _isMoving = true;
            // removes one move from the total of moves allowed in this turn
            _movesLeft--;
            return true;
        }

        // the Minotaur couldn't move, so he's gonna try to move vertically next
        return false;
    }

    private bool CanMoveVertical(Vector3 direction)
    {
        // sets the destination to one unit either to the top or to the bottom of current position
        _destination = new Vector3(transform.position.x, transform.position.y, transform.position.z - Mathf.Sign(direction.z));

        // if there is no obstacle between the Minotaur and the destination
        if (!Physics.Linecast(transform.position, _destination, _obstacleLayer))
        {
            _isMoving = true;
            // removes one move from the total of moves allowed in this turn
            _movesLeft--;
            return true;
        }

        // the Minotaur couldn't move, so he's gonna end his turn next
        return false;
    }

    private void Move()
    {
        // moves the Minotaur towards the destination
        transform.position = Vector3.MoveTowards(transform.position, _destination, _speed * Time.fixedDeltaTime);

        // if the Minotaur reaches the destination
        if (Vector3.Distance(transform.position, _destination) == 0)
        {
            _isMoving = false;

            // if the Minotaur is out of moves
            if (_movesLeft == 0)
            {
                // fires event informing that the Minotaur's turn has ended
                OnEndOfTurn();
                // sets the Minotaur's total moves back to 2
                _movesLeft = 2;
            }
        }
    }
}