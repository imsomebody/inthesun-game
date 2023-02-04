using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundController : MonoBehaviour
{
    private Transform cameraTransform;
    private Vector3 lastPosition;
    private Sprite sprite;
    private float textureUnitSize;

    public float parallaxMultiplier = .5f;

    private void Start()
    {
        // Maybe grab by tag
        cameraTransform = Camera.main.transform;
        sprite = GetComponent<SpriteRenderer>().sprite;
        textureUnitSize = sprite.texture.width / sprite.pixelsPerUnit;

        this.UpdateLastPos();
    }

    void UpdateLastPos()
    {
        lastPosition = cameraTransform.position;
    }

    private void LateUpdate()
    {
        var deltaM = cameraTransform.position - lastPosition;

        transform.position += deltaM * parallaxMultiplier;
        UpdateLastPos();

        if(cameraTransform.position.x - transform.position.x >= textureUnitSize)
        {
            float offsetPosition = (cameraTransform.position.x - transform.position.x) % textureUnitSize;
            transform.position = new Vector3(cameraTransform.position.x + offsetPosition, transform.position.y);
        }
    }
}
