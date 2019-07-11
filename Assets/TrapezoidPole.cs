using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 
 * オブジェクトの描画と各種コンポーネントのアタッチのみ行う
 * 台形
 * 
 * 
 */

public class TrapezoidPole : MonoBehaviour {

    private int poleNum = 1;
    private int poleSum = 6;
    private float radiusOut = 4f;
    private float radiusIn = 2f;
    private float poleHeight = 2f;

    //頂点座標
    Vector3[] vertex = new Vector3[24];/* = new Vector3[] {
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
    int[] face = new int[] { 1, 3, 0,
                             3, 2, 0,

                             5, 4, 2+8,
                             3+8, 5, 2+8,

                             7, 6, 4+8,
                             5+8, 7, 4+8,

                             1+8, 0+8, 6+8,
                             7+8, 1+8, 6+8,

                             2+16, 4+16, 0+16,
                             4+16, 6+16, 0+16,

                             7+16, 3+16, 1+16,
                             5+16, 3+16, 7+16, 
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
    public void SetPoleNums(int num, int sum, float radOut, float radIn, float height) {
        poleNum = num;
        poleSum = sum;
        radiusOut = radOut;
        radiusIn = radIn;
        poleHeight = height;
    }

    private void CalcVertices() {
        //台形の外側の頂点座標その1
        Vector3 vertex1 = new Vector3(radiusOut * Mathf.Sin(( poleNum + 0 ) / (float)poleSum * Mathf.PI * 2),
                                      radiusOut * Mathf.Cos(( poleNum + 0 ) / (float)poleSum * Mathf.PI * 2),
                                      0);
        //台形の外側の頂点座標その2
        Vector3 vertex2 = new Vector3(radiusOut * Mathf.Sin(( poleNum + 1 ) / (float)poleSum * Mathf.PI * 2),
                                      radiusOut * Mathf.Cos(( poleNum + 1 ) / (float)poleSum * Mathf.PI * 2),
                                      0);
        //台形の内側の頂点座標その1
        Vector3 vertex3 = new Vector3(radiusIn * Mathf.Sin(( poleNum + 0 ) / (float)poleSum * Mathf.PI * 2),
                                      radiusIn * Mathf.Cos(( poleNum + 0 ) / (float)poleSum * Mathf.PI * 2),
                                      0);
        //台形の内側の頂点座標その2
        Vector3 vertex4 = new Vector3(radiusIn * Mathf.Sin(( poleNum + 1 ) / (float)poleSum * Mathf.PI * 2),
                                      radiusIn * Mathf.Cos(( poleNum + 1 ) / (float)poleSum * Mathf.PI * 2),
                                      0);
        //全頂点数6にそれぞれ座標が3つずつある
        for (int i = 0; i < 8 * 3; i++) {
            if (i % 8 == 0) {
                vertex[i] = vertex3;
            } else if (i % 8 == 1) {
                vertex[i] = new Vector3(vertex3.x, vertex3.y, poleHeight);
            } else if (i % 8 == 2) {
                vertex[i] = vertex1;
            } else if (i % 8 == 3) {
                vertex[i] = new Vector3(vertex1.x, vertex1.y, poleHeight);
            } else if (i % 8 == 4) {
                vertex[i] = vertex2;
            } else if (i % 8 == 5) {
                vertex[i] = new Vector3(vertex2.x, vertex2.y, poleHeight);
            } else if (i % 8 == 6) {
                vertex[i] = vertex4;
            } else if (i % 8 == 7) {
                vertex[i] = new Vector3(vertex4.x, vertex4.y, poleHeight);
            } else {
                Debug.LogWarning("calcration error");
            }

        }
    }
}
