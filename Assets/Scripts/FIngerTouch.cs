using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FIngerTouch : MonoBehaviour
{
    public SpriteRenderer renderer;
    public Sprite pressed;
    public Sprite unpressed;
    public Camera camera;
    public float distance;

    // Start is called before the first frame update
    void Start()
    {
        renderer.sprite = unpressed;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0)) { renderer.sprite = pressed; }
        else { renderer.sprite = unpressed; }
        var position = camera.ScreenPointToRay(Input.mousePosition).GetPoint(distance);
        transform.position = position;
    }
}
