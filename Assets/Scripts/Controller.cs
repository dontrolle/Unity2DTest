using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class Controller : MonoBehaviour
{
    [SerializeField] private float movementSpeed;

    public Tilemap map;
    public GameObject SelectedUnit;

    MouseInput mouseInput;
    private Vector3 destination;

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
        destination = SelectedUnit.transform.position;
        mouseInput.Mouse.RightClick.performed += _ => OnRightClick();
        mouseInput.Mouse.LeftClick.performed += _ => OnLeftClick();
        unitLayerMask = LayerMask.GetMask("Units");
    }

    private void OnLeftClick()
    {
        // select unit
        (Vector2 mousePosition, _) = GetMousePosition();

        Collider2D colliderHit = Physics2D.OverlapPoint(mousePosition, unitLayerMask);

        // did we click on a unit?

        // if not, set SelectedUnit to null
        if (colliderHit == null) {
            SelectedUnit = null;
            return;
        }

        // else, select it, and set destination to current location of gameobject
        SelectedUnit = colliderHit.gameObject;
        destination = SelectedUnit.transform.position;
    }

    private void OnRightClick()
    {
        // move selected unit, if any
        if (SelectedUnit != null)
        {

            (Vector2 mousePosition, Vector3Int gridPosition) = GetMousePosition();

            // make sure we are clicking a cell
            if (map.HasTile(gridPosition))
            {
                destination = mousePosition;
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

    private (Vector2 mousePosition, Vector3Int gridPosition) GetMousePosition()
    {
        var mousePosition = Camera.main.ScreenToWorldPoint(mouseInput.Mouse.MousePosition.ReadValue<Vector2>());
        var gridPosition = map.WorldToCell(mousePosition);

        //Debug.Log($"mouse: {mousePosition.x},{mousePosition.y}");
        //Debug.Log($"grid: {gridPosition.x},{gridPosition.y}");

        return (mousePosition, gridPosition);

    }

    // Update is called once per frame
    void Update()
    {
        if (SelectedUnit != null && Vector3.Distance(SelectedUnit.transform.position, destination) > 0.1f)
        {
            SelectedUnit.transform.position = Vector3.MoveTowards(SelectedUnit.transform.position, destination, movementSpeed * Time.deltaTime);
        }
    }
}
