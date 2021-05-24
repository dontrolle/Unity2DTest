using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class Controller : MonoBehaviour
{
    [SerializeField] private float movementSpeed;

    public Tilemap map;
    private readonly List<GameObject> SelectedUnits = new List<GameObject>();
    private MouseInput mouseInput;
    private Vector3 destination;
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
        //mouseInput.Mouse.LeftClick.performed += _ => OnLeftClickPerformed();
        mouseInput.Mouse.LeftClick.started += _ => OnLeftClickStarted();
        mouseInput.Mouse.LeftClick.canceled += _ => OnLeftClickCancelled();

        //mouseInput.Mouse.LeftClickHold.started += _ => Debug.Log("LeftClickHold started.");
        //mouseInput.Mouse.LeftClickHold.performed += _ => Debug.Log("LeftClickHold performed.");
        //mouseInput.Mouse.LeftClickHold.canceled += _ => Debug.Log("LeftClickHold canceled.");

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
        //Debug.Log($"Left Click cancelled at {mouseInput.Mouse.MousePosition.ReadValue<Vector2>()}");

        (_, Vector2 mouseWorldPosition, _) = GetMousePosition();

        DeSelectAllSelectedUnits();

        // single-click or rectangle-select?
        if (mouseWorldPosition == lastLeftClickStartedAt)
        {
            // single click
            //Debug.Log("Single select");

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
            //Debug.Log("Rectangle select");

            //Debug.Log($"No. of PlayerUnits: {References.PlayerUnits.Count}");

            // select units in PlayerUnits that are inside rectangle
            Collider2D[] colliderHits = Physics2D.OverlapAreaAll(lastLeftClickStartedAt, mouseWorldPosition, unitLayerMask);

            // add to selected
            SelectedUnits.AddRange(colliderHits.Select(c2d => c2d.gameObject));
        }

        foreach(var selectedUnit in SelectedUnits)
        {
            selectedUnit.gameObject.GetComponent<UnitBehavior>().OnSelect();

            // TODO we need several destinations
            destination = selectedUnit.transform.position;
        }
    }

    private void OnLeftClickStarted()
    {
        //Debug.Log($"Left Click started at {mouseInput.Mouse.MousePosition.ReadValue<Vector2>()}");
        
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
                // TODO Each unit should have it's own destination
                destination = mouseWorldPosition;
            }

            //Debug.Log($"Unit: {SelectedUnit.transform.position.x},{SelectedUnit.transform.position.y}");
            //Debug.Log($"Destination: {destination.x},{destination.y}");
            //Debug.Log($"Distance:{Vector3.Distance(SelectedUnit.transform.position, destination)}");
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

        //Debug.Log($"mouse: {mousePosition.x},{mousePosition.y}");
        //Debug.Log($"grid: {gridPosition.x},{gridPosition.y}");

        return (mousePosition, mouseWorldPosition, gridPosition);

    }

    // Update is called once per frame
    void Update()
    {
        if (SelectedUnits.Count > 0)
        {
            foreach(var unit in SelectedUnits)
            {
                if(Vector3.Distance(unit.transform.position, destination) > 0.1f)
                {
                    unit.transform.position = Vector3.MoveTowards(unit.transform.position, destination, movementSpeed * Time.deltaTime);
                }
            }
        }
    }
}
