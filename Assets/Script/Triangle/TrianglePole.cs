using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 
 * オブジェクトの描画と各種コンポーネントのアタッチのみ行う
 * 
 * 
 */

public class TrianglePole : MonoBehaviour {

    private int poleNum = 1;
    private int poleSum = 6;
    private float radius = 2f;
    private float poleHeight = 2f;

    //頂点座標
    Vector3[] vertex = new Vector3[18];/* = new Vector3[] {
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
    };*/
    //面情報
    int[] face = new int[] { 0, 3, 2,
                             0, 1, 3,

                             2+6, 3+6, 4,
                             3+6, 5, 4,

                             4+6, 5+6, 1+6,
                             4+6, 1+6, 0+6,

                             0+12, 2+12, 4+12,

                             1+12, 5+12, 3+12,
    };

    //マテリアル
    public Material _material;

    void Start() {

    }

    void Update() {

        //初期値を与えられたら処理する
        if (poleNum != -1) {

            //頂点計算
            CalcVertices();

            //メッシュ作成
            Mesh mesh = new Mesh();
            //メッシュリセット
            mesh.Clear();
            //メッシュへの頂点情報の追加
            mesh.vertices = vertex;
            //メッシュへの面情報の追加
            mesh.triangles = face;

            //メッシュフィルター追加
            MeshFilter mesh_filter = new MeshFilter();
            this.gameObject.AddComponent<MeshFilter>();
            mesh_filter = GetComponent<MeshFilter>();
            //メッシュアタッチ
            mesh_filter.mesh = mesh;
            //レンダラー追加 + マテリアルアタッチ
            this.gameObject.AddComponent<MeshRenderer>();
            this.gameObject.GetComponent<MeshRenderer>().material = _material;
            //コライダーアタッチ
            this.gameObject.AddComponent<MeshCollider>();
            this.gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;

            //NormalMapの再計算
            mesh_filter.mesh.RecalculateNormals();

            poleNum = -1;
        }
    }

    //何個のオブジェクト中の何番目のオブジェクトか
    public void SetPoleNums(int num, int sum, float rad, float height) {
        poleNum = num;
        poleSum = sum;
        radius = rad;
        poleHeight = height;
    }

    private void CalcVertices() {
        //三角柱の外側の頂点座標その1
        Vector3 vertex1 = new Vector3(radius * Mathf.Sin(( poleNum + 0 ) / (float)poleSum * Mathf.PI * 2),
                                      radius * Mathf.Cos(( poleNum + 0 ) / (float)poleSum * Mathf.PI * 2),
                                      0);
        //三角柱の外側の頂点座標その2
        Vector3 vertex2 = new Vector3(radius * Mathf.Sin(( poleNum + 1 ) / (float)poleSum * Mathf.PI * 2),
                                      radius * Mathf.Cos(( poleNum + 1 ) / (float)poleSum * Mathf.PI * 2),
                                      0);
        //全頂点数6にそれぞれ座標が3つずつある
        for (int i = 0; i < 6 * 3; i++) {
            if (i % 6 == 0) {
                vertex[i] = new Vector3(0, 0, 0);
            } else if (i % 6 == 1) {
                vertex[i] = new Vector3(0, 0, poleHeight);
            } else if (i % 6 == 2) {
                vertex[i] = vertex1;
            } else if (i % 6 == 3) {
                vertex[i] = new Vector3(vertex1.x, vertex1.y, poleHeight);
            } else if (i % 6 == 4) {
                vertex[i] = vertex2;
            } else if (i % 6 == 5) {
                vertex[i] = new Vector3(vertex2.x, vertex2.y, poleHeight);
            } else {
                Debug.LogWarning("calcration error");
            }

        }
    }
}
