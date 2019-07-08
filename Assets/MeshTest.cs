using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTest : MonoBehaviour {

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
    };
    //面情報
    int[] face = new int[] { 0, 1, 2,1,2,3,4,5,7,5,6,7,1,2,6,1,5,6,2,3,7,2,7,6,1,5,0,0,4,5,0,4,7,0,3,7 };
    

    void Start() {
        //メッシュ作成
        Mesh mesh = new Mesh();
        //メッシュへの頂点情報の追加
        mesh.vertices = vertex;
        //メッシュへの面情報の追加
        mesh.triangles = face;
        //オブジェクト生成
        GameObject obj = new GameObject("Object");
        //メッシュフィルター追加
        MeshFilter mesh_filter = obj.AddComponent<MeshFilter>();
        //レンダラー追加
        obj.AddComponent<MeshRenderer>();
        //メッシュアタッチ
        mesh_filter.mesh = mesh;
        //色決定
        obj.GetComponent<MeshRenderer>().material.color = new Color(1f, 0.5f, 0.5f, 1f);
        //        mesh.colors = new Color(0.5f, 0.5f, 0.5f, 1f);
        int tmp = mesh_filter.mesh.vertices.Length;
        Debug.Log(tmp);
        Color[] x = new Color[tmp];
        x[0] = Color.red;
        x[1] = Color.blue;
        x[2] = Color.yellow;
        mesh_filter.mesh.colors = x;
    }


    void Update() {

    }
}
