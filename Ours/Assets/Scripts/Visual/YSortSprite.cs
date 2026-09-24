using UnityEngine;

public class YSortSprite : MonoBehaviour
{
    [SerializeField] private SpriteRenderer targetRenderer;
    [SerializeField] private Transform sortPoint;
    [SerializeField] private int sortingOffset;
    [SerializeField] private float precision = 100f;

    private void Awake()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void LateUpdate()
    {
        if (targetRenderer == null)
        {
            return;
        }

        Transform referencePoint = sortPoint != null ? sortPoint : transform;
        float safePrecision = Mathf.Max(1f, precision);
        targetRenderer.sortingOrder = -Mathf.RoundToInt(referencePoint.position.y * safePrecision) + sortingOffset;
    }
}
