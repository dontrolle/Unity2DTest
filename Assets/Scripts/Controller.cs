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
    public RectTransform SelectionBox;
    private bool mouseHeld = false;

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
        mouseHeld = false;

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

        mouseHeld = true;
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
                // TODO: Each unit should be assigned it's own offset destination
                //       Or is it naturally fixed, when units stop when blocked?
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

    void UpdateSelectionBox(Vector2 mouseWorldPosition)
    {
        SetRectangle(SelectionBox, mouseWorldPosition, lastLeftClickStartedAt);
    }

    public static void SetRectangle(RectTransform rectangle, Vector2 point0, Vector2 point1)
    {
        var left = Mathf.Min(point0.x, point1.x);
        var bottom = Mathf.Min(point0.y, point1.y);
        var right = Mathf.Max(point0.x, point1.x);
        var top = Mathf.Max(point0.y, point1.y);

        rectangle.offsetMin = new Vector2(left, bottom);
        rectangle.offsetMax = new Vector2(right, top);
    }

    private void Update()
    {
        // If left mouse is down draw rectangle between lastLeftClickStartedAt and current mouseWorldPosition
        if (mouseHeld)
        {
            (_, Vector2 mouseWorldPosition, _) = GetMousePosition();
            UpdateSelectionBox(mouseWorldPosition);

            // show selection box
            if (!SelectionBox.gameObject.activeInHierarchy)
            {
                SelectionBox.gameObject.SetActive(true);
            }
        }
        else
        {
            // hide selection box
            if (SelectionBox.gameObject.activeInHierarchy)
            {
                SelectionBox.gameObject.SetActive(false);
            }
        }
    }
}
