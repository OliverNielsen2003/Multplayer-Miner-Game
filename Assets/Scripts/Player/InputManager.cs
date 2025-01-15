using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Unity.VisualScripting;

public class InputManager : NetworkBehaviour
{
    public static PlayerInput PlayerInput;

    public static Vector2 Movement;
    public static bool JumpWasPressed;
    public static bool JumpIsHeld;
    public static bool JumpWasReleased;
    public static bool RunIsHeld;
    public static bool HitWasPressed;
    public static bool AbilityWasPressed;

    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _runAction;
    private InputAction _hitAction;
    private InputAction _abilityAction;

    private void Awake()
    {
        PlayerInput = GetComponent<PlayerInput>();

        _moveAction = PlayerInput.actions["Move"];
        _jumpAction = PlayerInput.actions["Jump"];
        _runAction = PlayerInput.actions["Run"];
        _hitAction = PlayerInput.actions["Hit"];
        _abilityAction = PlayerInput.actions["Ability"];
    }

    private void Update()
    {
        if (IsOwner)
        {
            Movement = _moveAction.ReadValue<Vector2>();

            JumpWasPressed = _jumpAction.WasPerformedThisFrame();
            JumpIsHeld = _jumpAction.IsPressed();
            JumpWasReleased = _jumpAction.WasReleasedThisFrame();
            AbilityWasPressed = _abilityAction.WasReleasedThisFrame();

            RunIsHeld = _runAction.IsPressed();
            HitWasPressed = _hitAction.IsPressed();
        }
    }
}
