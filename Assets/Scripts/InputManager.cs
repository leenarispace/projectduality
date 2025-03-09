using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private Camera _mainCamera;

    private static InputManager instance;

    private void Awake()
    {
        _mainCamera = Camera.main;

        if (instance != null)
        {
            Debug.LogError("씬에 한 개 이상의 Input Manager가 있습니다.");
        }
        instance = this;
    }

    public static InputManager GetInstance()
    {
        return instance;
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        var rayHit = Physics2D.GetRayIntersection(_mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue()));

        if (!rayHit.collider) return;

        //Debug.Log(rayHit.collider.gameObject.name);
    }
}