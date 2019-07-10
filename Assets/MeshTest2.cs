using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * 
 * 
 * MeshTest.csの反省を生かして、頂点座標を3つに分けてみた
 * 
 * 
 * 
 */


public class MeshTest2 : MonoBehaviour {

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
    int[] face = new int[] { 0, 1, 3,
                             1, 2, 3,

                             4, 6, 5,
                             4, 7, 6,

                             1+8, 6+8, 2+8,
                             1+8, 5+8, 6+8,

                             3+8, 2+16, 7+8,
                             2+16, 6+16, 7+8,

                             1+16, 0+8, 5+16,
                             0+8, 4+8, 5+16,

                             0+16, 7+16, 4+16,
                             0+16, 3+16, 7+16
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


    void Start() {
        //メッシュ作成
        Mesh mesh = new Mesh();
        //メッシュリセット
        mesh.Clear();
        //メッシュへの頂点情報の追加
        mesh.vertices = vertex;
        //メッシュへの面情報の追加
        mesh.triangles = face;

        //オブジェクト生成
        //GameObject obj = new GameObject("Object");
        //オブジェクト取得
        GameObject obj = this.gameObject;

        //メッシュフィルター追加
        //MeshFilter mesh_filter = obj.AddComponent<MeshFilter>();
        //メッシュフィルター取得
        MeshFilter mesh_filter = obj.GetComponent<MeshFilter>();

        //レンダラー追加
        //obj.AddComponent<MeshRenderer>();
        //メッシュアタッチ　　→Mesh.Colorの後の処理との意見
        //mesh_filter.mesh = mesh;

        //色決定
        //obj.GetComponent<MeshRenderer>().material.color = new Color(1f, 0.5f, 0.5f, 1f);
        //色決定2
        //mesh.colors = new Color(0.5f, 0.5f, 0.5f, 1f);
        //色決定3
        int tmp = mesh_filter.mesh.vertices.Length;
        Debug.Log(tmp);
        Color[] x = new Color[tmp];
        x[0] = Color.red;
        x[1] = Color.blue;
        x[2] = Color.yellow;
        mesh_filter.mesh.colors = x;
        //色決定4
        //obj.GetComponent<MeshRenderer>().material = _material;


        //メッシュアタッチ　　→Mesh.Colorの後に書いてみた
        mesh_filter.mesh = mesh;

        //UV情報追加
        mesh_filter.mesh.uv = uvs;
        //Boundsの再計算　　　→いらない疑惑
        //mesh.RecalculateBounds();
        //NormalMapの再計算　 →いらない疑惑
        //mesh.RecalculateNormals();
    }


    void Update() {

    }
}
