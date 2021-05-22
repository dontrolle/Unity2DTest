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
    public GameObject Player;

    MouseInput mouseInput;
    private Vector3 destination;

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
        destination = Player.transform.position;
        mouseInput.Mouse.MouseClick.performed += _ => OnMouseClick();
    }

    private void OnMouseClick()
    {
        Vector2 mousePosition = mouseInput.Mouse.MousePosition.ReadValue<Vector2>();
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        //Debug.Log($"mouse: {mousePosition.x},{mousePosition.y}");
        // make sure we are clicking the cell
        Vector3Int gridPosition = map.WorldToCell(mousePosition);
        //Debug.Log($"grid: {gridPosition.x},{gridPosition.y}");
        if (map.HasTile(gridPosition))
        {
            destination = mousePosition;
        }

        Debug.Log($"Player: {Player.transform.position.x},{Player.transform.position.y}");
        Debug.Log($"Destination: {destination.x},{destination.y}");
        Debug.Log($"Vector3.Distance(Player.transform.position, destination):{Vector3.Distance(Player.transform.position, destination)}");
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(Player.transform.position, destination) > 0.1f)
        {
            Player.transform.position = Vector3.MoveTowards(Player.transform.position, destination, movementSpeed * Time.deltaTime);
        }
    }
}
