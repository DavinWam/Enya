using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
public class ShearSprite : BaseMeshEffect
{
    public float shearAmountX = 0.0f; 
    public float shearAmountY = 0.0f; 

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive() || vh.currentVertCount == 0)
            return;

        List<UIVertex> verts = new List<UIVertex>();
        vh.GetUIVertexStream(verts);

        for (int i = 0; i < verts.Count; i++)
        {
            UIVertex vert = verts[i];

            vert.position.x += shearAmountX * vert.position.y;
            vert.position.y += shearAmountY * vert.position.x;

            verts[i] = vert;
        }

        vh.Clear();
        vh.AddUIVertexTriangleStream(verts);
    }


}
