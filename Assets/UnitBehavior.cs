using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitBehavior : MonoBehaviour
{
    public Color SelectedTintColor;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        References.PlayerUnits.Add(this.gameObject);
    }

    private void OnDisable()
    {
        References.PlayerUnits.Remove(this.gameObject);
    }

    public void OnSelect()
    {
        spriteRenderer.color = SelectedTintColor;
    }

    public void OnDeselect()
    {
        spriteRenderer.color = Color.white;
    }
}
