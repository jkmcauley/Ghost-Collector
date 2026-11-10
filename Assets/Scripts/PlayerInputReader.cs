using System;

using UnityEngine;

using UnityEngine.InputSystem;

public class PlayerInputReader : MonoBehaviour

{

    [SerializeField] private InputActionReference _moveAction;

    [SerializeField] private InputActionReference _attackAction;

    public Vector2 MoveInput { get; private set; }

    public event Action AttackPressed;

    private void OnEnable()

    {

        _moveAction.action.performed += OnMove;

        _moveAction.action.canceled += OnMoveCanceled;

        _attackAction.action.performed += OnAttack;

        _moveAction.action.Enable();

        _attackAction.action.Enable();

    }

    private void OnDisable()

    {

        _moveAction.action.performed -= OnMove;

        _moveAction.action.canceled -= OnMoveCanceled;

        _attackAction.action.performed -= OnAttack;

        _moveAction.action.Disable();

        _attackAction.action.Disable();

        MoveInput = Vector2.zero;

    }

    private void OnMove(InputAction.CallbackContext ctx)

    {

        MoveInput = ctx.ReadValue<Vector2>();

    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)

    {

        MoveInput = Vector2.zero;

    }

    private void OnAttack(InputAction.CallbackContext ctx)

    {

        AttackPressed?.Invoke();

    }

}
