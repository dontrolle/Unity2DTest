using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class Controller : MonoBehaviour
{
    public Tilemap map;
    private List<GameObject> SelectedUnits = new List<GameObject>();
    private MouseInput mouseInput;
    private Vector2 lastLeftClickStartedAt;

    LayerMask unitLayerMask;

    private void Awake()
    {
        mouseInput = new MouseInput();
    }

    private void OnEnable()
    {
        mouseInput.Enable();
    }

    private void OnDisable()
    {
        mouseInput.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        mouseInput.Mouse.RightClick.performed += _ => OnRightClickPerformed();
        mouseInput.Mouse.LeftClick.started += _ => OnLeftClickStarted();
        mouseInput.Mouse.LeftClick.canceled += _ => OnLeftClickCancelled();

        unitLayerMask = LayerMask.GetMask("Units");
    }

    private void DeSelectAllSelectedUnits()
    {
        foreach (var unit in SelectedUnits)
        {
            unit.GetComponent<UnitBehavior>().OnDeselect();
        }

        SelectedUnits.Clear();
    }

    private void OnLeftClickCancelled()
    {
        (_, Vector2 mouseWorldPosition, _) = GetMousePosition();

        DeSelectAllSelectedUnits();

        // single-click or rectangle-select?
        if (mouseWorldPosition == lastLeftClickStartedAt)
        {
            // single click
            Collider2D colliderHit = Physics2D.OverlapPoint(mouseWorldPosition, unitLayerMask);

            // did we click on a unit?
            // if not, we don't need to do more
            if (colliderHit == null)
            {
                return;
            }

            // else, select it, signal it, and set destination to current location of gameobject
            SelectedUnits.Add(colliderHit.gameObject);
        }
        else
        {
            // rectangle-select

            // select units in PlayerUnits that are inside rectangle
            Collider2D[] colliderHits = Physics2D.OverlapAreaAll(lastLeftClickStartedAt, mouseWorldPosition, unitLayerMask);

            // add to selected
            SelectedUnits.AddRange(colliderHits.Select(c2d => c2d.gameObject));
        }

        foreach(var selectedUnit in SelectedUnits)
        {
            selectedUnit.gameObject.GetComponent<UnitBehavior>().OnSelect();
        }
    }

    private void OnLeftClickStarted()
    {
        (_, lastLeftClickStartedAt, _) = GetMousePosition();
    }

    private void OnRightClickPerformed()
    {
        // move selected unit, if any
        if (SelectedUnits.Count > 0)
        {
            (Vector2 mousePosition, Vector2 mouseWorldPosition, Vector3Int gridPosition) = GetMousePosition();

            // make sure we are clicking (i) inside the camera viewport and (ii) within a cell
            if (Camera.main.pixelRect.Contains(mousePosition) && map.HasTile(gridPosition))
            {
                // TODO: Each unit should have it's own destination
                foreach (var unit in SelectedUnits)
                {
                    unit.GetComponent<UnitBehavior>().Destination = mouseWorldPosition;
                }
            }
        }
        else
        {
            Debug.Log("No unit selected.");
        }
    }

    private (Vector2 mousePosition, Vector2 mouseWorldPosition, Vector3Int gridPosition) GetMousePosition()
    {
        var mousePosition = mouseInput.Mouse.MousePosition.ReadValue<Vector2>();
        var mouseWorldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        var gridPosition = map.WorldToCell(mouseWorldPosition);

        return (mousePosition, mouseWorldPosition, gridPosition);
    }
}
