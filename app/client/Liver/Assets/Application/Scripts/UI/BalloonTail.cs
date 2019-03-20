using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BalloonTail : UIBehaviour, IMeshModifier
{
    public Vector2? focusPosition = null;
    public Camera uiCamera;
    public void ModifyMesh(Mesh mesh) {}
    RectTransform rectTransform;

    protected override void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        base.Awake();
    }

    public void Update()
    {
        var graphics = base.GetComponent<Graphic>();
        graphics.SetVerticesDirty();
    }
    public void ModifyMesh(VertexHelper vh)
    {
        if (!focusPosition.HasValue) { return; }

        List<UIVertex> vertices = new List<UIVertex>();
        vh.GetUIVertexStream(vertices);
        UIVertex v;
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, focusPosition.Value, uiCamera, out pos);
        int[] index = new[] { 0, 4, 5 };

        foreach (var i in index)
        {
            v = vertices[i];
            v.position = pos;
            vertices[i] = v;
        }

        vh.Clear();
        vh.AddUIVertexTriangleStream(vertices);
    }
}
