using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitBehavior : MonoBehaviour
{
    [SerializeField] private float movementSpeed;

    public Color SelectedTintColor;
    private SpriteRenderer spriteRenderer;

    public Vector3 Destination { get; internal set; }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        References.PlayerUnits.Add(gameObject);
        Destination = transform.position;
    }

    private void OnDisable()
    {
        References.PlayerUnits.Remove(gameObject);
    }

    public void OnSelect()
    {
        spriteRenderer.color = SelectedTintColor;
    }

    public void OnDeselect()
    {
        spriteRenderer.color = Color.white;
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, Destination) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, Destination, movementSpeed * Time.deltaTime);
        }
    }
}
