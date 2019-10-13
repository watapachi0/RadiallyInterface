using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class createTrapezoidPoleC : MonoBehaviour {

    private Vector3[] vertex;           //各台形柱が計算した頂点情報
    private bool[] isCalledBackVertex;  //頂点座標が返されているか確認

    void Start() {
        //頂点情報の初期化
        vertex = new Vector3[variablesC.poleSum * variablesC.trapezoidDivisionNum * 10];
        //台形柱の生成
        GameObject obj;
        isCalledBackVertex = new bool[variablesC.poleSum * variablesC.trapezoidDivisionNum];
        for (int i = 0; i < variablesC.poleSum; i++) {
            obj = new GameObject(( i + 1 ).ToString());
            //TrapezoidPole trianglePoleC = obj.AddComponent<TrapezoidPoleC>();
            MultipleTrapezoidPoleC trianglePole = obj.AddComponent<MultipleTrapezoidPoleC>();
            obj.transform.position = transform.position;
            trianglePole.setMyParent(this.gameObject);
            isCalledBackVertex[i] = false;
        }
        //システムキーの生成
        /*for (int i = 0; i < variablesC.systemCommandNum; i++) {
            GameObject systemKey = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            systemKey.name = "systemKey" + i.ToString();
            systemKey.AddComponent<SystemKeyPoleC>();
        }*/
    }

    void Update() {
        for (int i = 0; ( i < variablesC.poleSum ) && isCalledBackVertex[i]; i++) {
            if (i + 1 == variablesC.poleSum) {
                //多角形用の頂点群の準備
                variablesC.polygonalPillarVertex = new Vector3[variablesC.poleSum * 10];
                variablesC.polygonalPillarVertex = vertex;

                //中心の多角柱を描画する
                GameObject obj = new GameObject(0.ToString());
                PolygonalPillarC polygonalPillar = obj.AddComponent<PolygonalPillarC>();
                polygonalPillar.setMyParent(this.gameObject);

                //コンポーネント削除
                Destroy(this);
            }
        }
    }

    public void callBackVertex(Vector3[] vertexies, int poleNum) {
        isCalledBackVertex[poleNum - 0] = true;
        for (int i = 0; i < 8; i++)
            vertex[( poleNum - 0 ) * 8 + i] = vertexies[i % 4];

        vertex[8 * variablesC.poleSum * variablesC.trapezoidDivisionNum + poleNum - 0] = variablesC.createSourcePosition;
        vertex[9 * variablesC.poleSum * variablesC.trapezoidDivisionNum + poleNum - 0] = new Vector3(variablesC.createSourcePosition.x, variablesC.createSourcePosition.y, variablesC.createSourcePosition.z + variablesC.poleHeight);
    }
}
