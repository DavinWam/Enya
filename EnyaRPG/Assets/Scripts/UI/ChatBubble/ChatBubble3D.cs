using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;


public class ChatBubble3D : MonoBehaviour
{
    // Start is called before the first frame update
    private SpriteRenderer backgroundSpriteRenderer;
    private TextMeshPro textMeshPro;

    private void Awake()
    {
        backgroundSpriteRenderer = transform.Find("Sprite").GetComponent<SpriteRenderer>();
        textMeshPro = transform.Find("text").GetComponent<TextMeshPro>();
        Setup(textMeshPro.text);
    }

    private void Setup(string text)
    {
        textMeshPro.SetText(text);
        textMeshPro.ForceMeshUpdate();
        Vector2 textSize = textMeshPro.GetRenderedValues(false);

        Vector2 padding = new Vector2(3f, 3f);
        backgroundSpriteRenderer.size = textSize + padding;

        Vector3 offset = new Vector3(-3f, 0f);
        backgroundSpriteRenderer.transform.localPosition =
            new Vector3(backgroundSpriteRenderer.size.x / 2f, 0f) + offset;


    }

}
