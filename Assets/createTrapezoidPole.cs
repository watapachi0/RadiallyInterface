using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class createTrapezoidPole : MonoBehaviour {

    public Material TrapezoidMaterial;  //キーのマテリアル
    public Material TrapezoidMaterialOnTouch;
    public Material PolygonalMaterial;  //中心の多角柱用のマテリアル
    [SerializeField]
    private int poleSum = 5;            //キーの数
    private float radiusOut = 4f;       //システムの外縁の半径
    private float radiusIn = 2f;        //ニュートラルエリアの半径
    private float poleHeight = 2f;      //システムの厚み
    private Vector3[] vertex;           //各台形柱が計算した頂点情報
    private bool[] isCalledBackVertex;  //頂点座標が返されているか確認

    void Start() {
        //一応初期化されなかった時用
        if (poleSum <= 0) {
            poleSum = 5;
        }

        //頂点情報の初期化
        vertex = new Vector3[poleSum * 10];
        //台形柱の生成
        GameObject obj;
        isCalledBackVertex = new bool[poleSum];
        for (int i = 0; i < poleSum; i++) {
            obj = new GameObject(( i + 1 ).ToString());
            TrapezoidPole trianglePole = obj.AddComponent<TrapezoidPole>();
            trianglePole.SetPoleNums(i, poleSum, radiusOut, radiusIn, poleHeight);
            trianglePole._material = TrapezoidMaterial;
            trianglePole._touchMaterial = TrapezoidMaterialOnTouch;
            isCalledBackVertex[i] = false;
        }
    }

    void Update() {
        for (int i = 0; ( i < poleSum ) && isCalledBackVertex[i]; i++) {
            if (i + 1 == poleSum) {
                //中心の多角柱を描画する
                GameObject obj = new GameObject(0.ToString());
                PolygonalPillar polygonalPillar = obj.AddComponent<PolygonalPillar>();
                
                polygonalPillar.SetPoleSums(poleSum, radiusIn, poleHeight, vertex);
                polygonalPillar._material = PolygonalMaterial;
                //コンポーネント削除
                Destroy(this);
            }
        }
    }

    public void callBackVertex(Vector3[] vertexies, int poleNum) {
        isCalledBackVertex[poleNum - 1] = true;
        for (int i = 0; i < 8; i++)
            vertex[( poleNum - 1 ) * 8 + i] = vertexies[i % 4];
        vertex[8 * poleSum + poleNum - 1] = transform.position;
        vertex[9 * poleSum + poleNum - 1] = new Vector3(transform.position.x, transform.position.y, transform.position.z + poleHeight);
    }
}
