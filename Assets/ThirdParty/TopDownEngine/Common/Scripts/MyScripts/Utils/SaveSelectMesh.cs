using UnityEngine;
using UnityEditor;
using System.Collections;

public class SaveSelectMesh : EditorWindow
{

    [MenuItem("GameObject/Save Select Mesh")]
    static void Copy()
    {
        Transform t = Selection.activeTransform;
        GameObject obj = t ? t.gameObject : null;

        if (obj)
        {
            MeshFilter meshFilter = obj.GetComponent(typeof(MeshFilter)) as MeshFilter;
            Mesh mesh = meshFilter ? meshFilter.sharedMesh : null;

            Mesh newMesh = new Mesh();
            newMesh.vertices = mesh.vertices;
            newMesh.uv = mesh.uv;
            newMesh.normals = mesh.normals;
            newMesh.triangles = mesh.triangles;
            newMesh.RecalculateNormals();
            newMesh.RecalculateBounds();

#if false
            // chagne pivot :)
            Vector3 diff = Vector3.Scale(newMesh.bounds.extents, new Vector3(0, 0, -1));
            obj.transform.position -= Vector3.Scale(diff, obj.transform.localScale);
            Vector3[] verts = newMesh.vertices;
            for (int i = 0; i < verts.Length; i++)
            {
                verts[i] += diff;
            }
            newMesh.vertices = verts;
            newMesh.RecalculateBounds();
#endif

            string fileName = EditorUtility.SaveFilePanelInProject("Save Mesh", "mesh", "asset", "");
            AssetDatabase.CreateAsset(newMesh, fileName);
        }
    }
}