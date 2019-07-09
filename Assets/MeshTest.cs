using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * 
 * 
 * 全頂点を共有する形でポリゴン生成した結果
 * 
 * 失敗
 * 
 * 一面ごとに頂点を変えないと、全体で一面として処理されるらしい
 * 適当な一面の色情報を全体で共有する
 * なんならカメラから見た裏側のポリゴンの色情報を共有する
 * そのため、光源の側からみるとすべて真っ黒
 * 光源の反対から見ると色が反映される
 * 
 * さらに、全体で一面扱いなので、エッジが消えたような表示になる
 * 具体的にはスムーズシェーディング100％の状態
 * 
 * 回避策は
 * 3面に接する頂点は、同一座標上に3頂点を持つように配置する
 * 4面なら4頂点
 * 面同士の頂点共有は行わない
 * 
 * 
 * 
 */


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
    int[] face = new int[] { 0, 1, 3,
                             1, 2, 3,
                             4, 6, 5,
                             4, 7, 6,
                             1, 6, 2,
                             1, 5, 6,
                             3, 2, 7,
                             2, 6, 7,
                             1, 0, 5,
                             0, 4, 5,
                             0, 7, 4,
                             0, 3, 7
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
        new Vector2(1f,0f)
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
        GameObject obj = this.gameObject;
        //メッシュフィルター追加
        //MeshFilter mesh_filter = obj.AddComponent<MeshFilter>();
        //メッシュフィルター取得
        MeshFilter mesh_filter = obj.GetComponent<MeshFilter>();
        //レンダラー追加
        //obj.AddComponent<MeshRenderer>();
        for (int i = 0; i < mesh_filter.mesh.vertices.Length; i++) {
            Debug.Log("頂点" + i + " " + mesh_filter.mesh.vertices[i]);
        }
        for (int i = 0; i < mesh_filter.mesh.triangles.Length; i++) {
            Debug.Log("面" + i + " " + mesh_filter.mesh.triangles[i]);
        }
        //メッシュアタッチ
        mesh_filter.mesh = mesh;
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
        //mesh_filter.mesh.colors = x;
        //色決定4
        obj.GetComponent<MeshRenderer>().material = _material;
        //UV情報追加
        mesh_filter.mesh.uv = uvs;
        //　Boundsの再計算
        mesh.RecalculateBounds();
        //　NormalMapの再計算
        mesh.RecalculateNormals();
    }


    void Update() {

    }
}
