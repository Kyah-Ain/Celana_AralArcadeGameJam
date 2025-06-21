using UnityEngine;
using UnityEngine.UI;

public class BackgroundScroller : MonoBehaviour
{
    public RawImage image;
    public float scrollSpeed = 0.02f;

    private void Update()
    {
        Rect uvRect = image.uvRect;
        uvRect.x += scrollSpeed * Time.deltaTime;
        image.uvRect = uvRect;
    }
}

