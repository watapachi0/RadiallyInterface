using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrianglePole : MonoBehaviour {

    //頂点座標
    Vector3[] vertex = new Vector3[] {
        new Vector3(0f,0f,0f),
        new Vector3(0f,1f,0f),
        new Vector3(1f,1f,0f),
        new Vector3(1f,0f,0f),
        new Vector3(0f,0f,-1f),
        new Vector3(0f,1f,-1f),
        new Vector3(1f,1f,-1f),
        new Vector3(1f,0f,-1f),

        new Vector3(0f,0f,0f),
        new Vector3(0f,1f,0f),
        new Vector3(1f,1f,0f),
        new Vector3(1f,0f,0f),
        new Vector3(0f,0f,-1f),
        new Vector3(0f,1f,-1f),
        new Vector3(1f,1f,-1f),
        new Vector3(1f,0f,-1f),

        new Vector3(0f,0f,0f),
        new Vector3(0f,1f,0f),
        new Vector3(1f,1f,0f),
        new Vector3(1f,0f,0f),
        new Vector3(0f,0f,-1f),
        new Vector3(0f,1f,-1f),
        new Vector3(1f,1f,-1f),
        new Vector3(1f,0f,-1f),
    };
    //面情報
    int[] face = new int[] { 0, 3, 1,
                             1, 3, 2,

                             4, 5, 6,
                             4, 6, 7,

                             1+8, 2+8, 6+8,
                             1+8, 6+8, 5+8,

                             3+8, 7+8, 2+16,
                             2+16, 7+8, 6+16,

                             1+16, 5+16, 0+8,
                             0+8, 5+16, 4+8,

                             0+16, 4+16, 7+16,
                             0+16, 7+16, 3+16,
    };
    //UV情報
    Vector2[] uvs = new Vector2[] {
        new Vector2(0f,0f),
        new Vector2(0f,1f),
        new Vector2(1f,1f),
        new Vector2(1f,0f),
        new Vector2(0f,0f),
        new Vector2(0f,1f),
        new Vector2(1f,1f),
        new Vector2(1f,0f),

        new Vector2(0f,0f),
        new Vector2(0f,1f),
        new Vector2(1f,1f),
        new Vector2(1f,0f),
        new Vector2(0f,0f),
        new Vector2(0f,1f),
        new Vector2(1f,1f),
        new Vector2(1f,0f),

        new Vector2(0f,0f),
        new Vector2(0f,1f),
        new Vector2(1f,1f),
        new Vector2(1f,0f),
        new Vector2(0f,0f),
        new Vector2(0f,1f),
        new Vector2(1f,1f),
        new Vector2(1f,0f),
    };

    public Material _material;
    public bool isEmptyObject;
    public bool isCreateOtherCube;
    public int mode;

    void Start() {
        if (isCreateOtherCube)
            isEmptyObject = true;

        //メッシュ作成
        Mesh mesh = new Mesh();
        //メッシュリセット
        mesh.Clear();
        //メッシュへの頂点情報の追加
        mesh.vertices = vertex;
        //メッシュへの面情報の追加
        mesh.triangles = face;

        GameObject obj;
        if (isCreateOtherCube) {
            //オブジェクト生成
            obj = new GameObject("Object");
        } else {
            //オブジェクト取得
            obj = this.gameObject;
        }

        MeshFilter mesh_filter;
        if (isEmptyObject) {
            //メッシュフィルター追加
            mesh_filter = obj.AddComponent<MeshFilter>();
            //レンダラー追加
            obj.AddComponent<MeshRenderer>();
        } else {
            //メッシュフィルター取得
            mesh_filter = obj.GetComponent<MeshFilter>();
        }
        //メッシュアタッチ
        mesh_filter.mesh = mesh;

        if (mode < 1) {                 //うまくいった
            //色決定
            obj.GetComponent<MeshRenderer>().material.color = new Color(1f, 0.5f, 0.5f, 1f);
        } else if (mode < 2) {          //うまくいかない？
            int tmp = mesh_filter.mesh.vertices.Length;
            Debug.Log(tmp);
            Color[] x = new Color[tmp];
            x[0] = Color.red;
            x[1] = Color.blue;
            x[2] = Color.yellow;
            for (int i = 0; i < tmp; i++) {
                if (/* test */false)
                    break;
                x[i] = Color.blue;
            }
            mesh.colors = x;
            //色決定2
            //mesh.colors[0] = new Color(0.5f, 0.5f, 0.5f, 1f);
        } else if (mode < 3) {          //うまくいかない？
            //色決定3
            int tmp = mesh_filter.mesh.vertices.Length;
            Debug.Log(tmp);
            Color[] x = new Color[tmp];
            x[0] = Color.red;
            x[1] = Color.blue;
            x[2] = Color.yellow;
            mesh_filter.mesh.colors = x;
        } else {                        //うまくいった
            //色決定4
            obj.GetComponent<MeshRenderer>().material = _material;
        }

        //UV情報追加
        //mesh_filter.mesh.uv = uvs;
        //Boundsの再計算
        mesh_filter.mesh.RecalculateBounds();
        //NormalMapの再計算
        mesh_filter.mesh.RecalculateNormals();
    }


    void Update() {

    }
}
