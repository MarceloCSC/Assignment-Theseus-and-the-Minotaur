using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public event Action OnEndOfTurn = delegate { };

    [SerializeField] private float _speed = 2.5f;
    [SerializeField] private LayerMask _obstacleLayer;

    private bool _isMoving;
    private Vector3 _destination;

    private PlayerInputActions _inputActions;

    private void Awake()
    {
        _inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        _inputActions.Enable();
    }

    private void OnDisable()
    {
        _inputActions.Disable();
    }

    private void Update()
    {
        // if the player isn't already moving and if it's the player's turn
        if (!_isMoving && GameManager.CurrentTurn == GameManager.GameMode.PlayersTurn)
        {
            GetInput();
        }
    }

    private void FixedUpdate()
    {
        if (_isMoving)
        {
            Move();
        }
    }

    private void GetInput()
    {
        // listens to player input
        Vector2 inputValues = _inputActions.Player.Movement.ReadValue<Vector2>();

        // if player is pressing either left or right
        // this will ignore diagonal movement
        if (Mathf.Abs(inputValues.x) == 1)
        {
            SetHorizontalMovement(inputValues);
        }
        // if player is pressing either up or down
        // this will ignore diagonal movement
        else if (Mathf.Abs(inputValues.y) == 1)
        {
            SetVerticalMovement(inputValues);
        }
    }

    private void SetHorizontalMovement(Vector2 inputValues)
    {
        // sets the destination to one unit in the direction of the pressed input
        _destination = new Vector3(transform.position.x + inputValues.x, transform.position.y, transform.position.z);

        // if there is no obstacle between the player and the destination
        if (!Physics.Linecast(transform.position, _destination, _obstacleLayer))
        {
            _isMoving = true;
        }
    }

    private void SetVerticalMovement(Vector2 inputValues)
    {
        // sets the destination to one unit in the direction of the pressed input
        _destination = new Vector3(transform.position.x, transform.position.y, transform.position.z + inputValues.y);

        // if there is no obstacle between the player and the destination
        if (!Physics.Linecast(transform.position, _destination, _obstacleLayer))
        {
            _isMoving = true;
        }
    }

    private void Move()
    {
        // moves the player towards the destination
        transform.position = Vector3.MoveTowards(transform.position, _destination, _speed * Time.fixedDeltaTime);

        // if the player reaches the destination
        if (Vector3.Distance(transform.position, _destination) == 0)
        {
            _isMoving = false;

            // fires event informing that the player's turn has ended
            OnEndOfTurn();
        }
    }
}