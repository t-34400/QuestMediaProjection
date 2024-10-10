#nullable enable

using System.Collections;
using System.Linq;
using UnityEngine;
using MediaProjection.Models;

public class BarcodeHighlightController : MonoBehaviour
{
    [SerializeField] private Camera targetCamera = default!;
    [Header("Highlight properties")]
    [SerializeField] private Material lineMaterial = default!;
    [SerializeField] private Color lineColor = Color.red;
    [SerializeField] private float lineWidth = 0.005f;
    [SerializeField] private float highlightDepth = 1.0f;
    [SerializeField] private float displayDuration = 0.2f;
    [Header("Coordinate transformation properties")]
    [SerializeField] private Vector2 centerOffset = new Vector2(485, 291);
    [SerializeField] private float pixelLengthRatio = 570f;

    private LineRenderer? lineRenderer;

    public void HighlightBarcode(BarcodeReadingResult[] results)
    {
        if (lineRenderer == null || results.Length == 0)
        {
            return;
        }

        var result = results[0];
        
        var positions = 
            result.ResultPoints
                .Select(point => ConvertCapturedImageToLocal(point))
                .ToArray();
        targetCamera.transform.TransformPoints(positions);

        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);

        StopAllCoroutines();
        StartCoroutine(HideCoroutine());
    }

    private Vector3 ConvertCapturedImageToLocal(Vector2 point)
    {
        var centeredPoint = point - centerOffset;
        return new Vector3(centeredPoint.x / pixelLengthRatio, - centeredPoint.y / pixelLengthRatio, highlightDepth);
    }

    private IEnumerator HideCoroutine()
    {
        yield return new WaitForSeconds(displayDuration);
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
        }
    }

    private void OnEnable()
    {
        lineRenderer ??= gameObject.AddComponent<LineRenderer>();

        lineRenderer.enabled = true;
        lineRenderer.material = lineMaterial;

        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endColor = lineColor;
        
        lineRenderer.loop = true;
        lineRenderer.positionCount = 0;
    }

    private void OnDisable()
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }
}
