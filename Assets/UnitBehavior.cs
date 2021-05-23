using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitBehavior : MonoBehaviour
{
    public Color SelectedTintColor;
    private SpriteRenderer spriteRenderer;
    private Color defaultColor;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        defaultColor = spriteRenderer.color;
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
