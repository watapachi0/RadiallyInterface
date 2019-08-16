using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class createTrapezoidPole : MonoBehaviour {

    private Vector3[] vertex;           //各台形柱が計算した頂点情報
    private bool[] isCalledBackVertex;  //頂点座標が返されているか確認

    void Start() {
        //頂点情報の初期化
        vertex = new Vector3[variables.poleSum * variables.trapezoidDivisionNum * 10];
        //台形柱の生成
        GameObject obj;
        isCalledBackVertex = new bool[variables.poleSum * variables.trapezoidDivisionNum];
        for (int i = 0; i < variables.poleSum; i++) {
            obj = new GameObject(( i + 1 ).ToString());
            //TrapezoidPole trianglePole = obj.AddComponent<TrapezoidPole>();
            MultipleTrapezoidPole trianglePole = obj.AddComponent<MultipleTrapezoidPole>();
            isCalledBackVertex[i] = false;
        }
        //システムキーの生成
        for (int i = 0; i < variables.systemCommandNum && false; i++) {
            GameObject systemKey = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            systemKey.name = "systemKey" + i.ToString();
            systemKey.AddComponent<SystemKeyPole>();
        }
    }

    void Update() {
        for (int i = 0; ( i < variables.poleSum ) && isCalledBackVertex[i]; i++) {
            if (i + 1 == variables.poleSum) {
                //多角形用の頂点群の準備
                variables.polygonalPillarVertex = new Vector3[variables.poleSum * 10];
                variables.polygonalPillarVertex = vertex;

                //中心の多角柱を描画する
                GameObject obj = new GameObject(0.ToString());
                PolygonalPillar polygonalPillar = obj.AddComponent<PolygonalPillar>();

                //コンポーネント削除
                Destroy(this);
            }
        }
    }

    public void callBackVertex(Vector3[] vertexies, int poleNum) {
        isCalledBackVertex[poleNum - 0] = true;
        for (int i = 0; i < 8; i++)
            vertex[( poleNum - 0 ) * 8 + i] = vertexies[i % 4];
        vertex[8 * variables.poleSum * variables.trapezoidDivisionNum + poleNum - 0] = transform.position;
        vertex[9 * variables.poleSum * variables.trapezoidDivisionNum + poleNum - 0] = new Vector3(transform.position.x, transform.position.y, transform.position.z + variables.poleHeight);
    }
}
