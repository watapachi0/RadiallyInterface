using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//try catch用 System.Exception
using System;

public class createTrapezoidPoleC : MonoBehaviour {

    private Vector3[] vertex;           //各台形柱が計算した頂点情報
    private bool[] isCalledBackVertex;  //頂点座標が返されているか確認

    //スクリプト消去フラグ
    private bool isReadyToDestroy = false;
    //呼び出し元保存　nullなら主輪
    private GameObject createSorceObj = null;
    public GameObject PositionObj { get; set; }
    //中心の多角柱用のインスタンス
    private PolygonalPillarC polygonalPillar = null;
    private int poleSum = 0;

    void Start() {
        if (createSorceObj != null) {
            //自分の名前から該当TextSetの行からアイテム数を取得
            poleSum = createSorceObj.GetComponent<centralSystemC>().GetTextSetItemNum(int.Parse(transform.name.Substring(9)));
        } else {
            //現在のTextSetの段数を取得
            poleSum = variablesC.poleSum;
            //Debug.Log(transform.name);
        }

        //頂点情報の初期化
        vertex = new Vector3[poleSum * ( variablesC.trapezoidDivisionNum + 1 ) * 10];
        //台形柱の生成
        GameObject obj;
        isCalledBackVertex = new bool[poleSum * ( variablesC.trapezoidDivisionNum + 1 )];
        for (int i = 0; i < poleSum; i++) {
            obj = new GameObject(( i + 1 ).ToString());
            obj.transform.position = transform.position;
            //TrapezoidPole trianglePoleC = obj.AddComponent<TrapezoidPoleC>();
            MultipleTrapezoidPoleC trianglePole = obj.AddComponent<MultipleTrapezoidPoleC>();
            if (createSorceObj != null)
                trianglePole.setMyParent(this.gameObject);
            if (createSorceObj != null) {
                trianglePole.isSubRingPole = true;
            } else {
                trianglePole.isSubRingPole = false;
            }
            isCalledBackVertex[i] = false;
        }
        /*Debug.Log("主輪外径(" + variablesC.radiusOut * 100 + "cm) 主輪内径(" + variablesC.radiusIn * 100 + "cm) 副輪外径　　　(" + variablesC.radiusOut_subCircle * 100 + "cm)副輪内径(" + variablesC.radiusIn_subCircle * 100 + "cm)\n"
                 + "\t\t            副輪外径理論値(" + calcTheoreticalRadiusout() * 100 + "cm)");*/
        //システムキーの生成
        /*for (int i = 0; i < variablesC.systemCommandNum; i++) {
            GameObject systemKey = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            systemKey.name = "systemKey" + i.ToString();
            systemKey.AddComponent<SystemKeyPoleC>();
        }*/
    }

    void Update() {
        //削除準備が整ったら削除
        if (isReadyToDestroy) {
            //ずれていたら座標を合わせる(許容誤差1cm)
            try {
                if (createSorceObj != null || 0.01f <= Vector3.Distance(PositionObj.transform.position, transform.position))
                    transform.position = PositionObj.transform.position;
                //Debug.Log("副輪のcreateスクリプトを削除します");
            } catch (Exception e) {
                //Debug.Log("主輪のcreateスクリプトを削除します");
                //Debug.LogWarning("try failed : ///\n" + e.Message + "///\n" + e.TargetSite + "///\n" + e.StackTrace);
            } finally {
                //コンポーネント削除
                Destroy(this);
            }
        }

        ///親オブジェクトがいないとき（主輪である）ときのみ中心オブジェクトを作る
        if (/*createSorceObj == null*/true) {
            for (int i = 0; ( i < poleSum ) && isCalledBackVertex[i]; i++) {
                if (i + 1 == poleSum && polygonalPillar == null) {
                    //多角形用の頂点群の準備
                    variablesC.polygonalPillarVertex = new Vector3[poleSum * 10];
                    variablesC.polygonalPillarVertex = vertex;

                    //中心の多角柱を描画する
                    GameObject obj = new GameObject(0.ToString());
                    polygonalPillar = obj.AddComponent<PolygonalPillarC>();
                    if (createSorceObj != null)
                        polygonalPillar.setMyParent(this.gameObject);
                    if (createSorceObj != null) {
                        //親がいるときは伝える
                        polygonalPillar.isSubRingPillar = true;
                        //親がいるならそいつの子供になる
                        this.gameObject.transform.parent = createSorceObj.transform;
                    }
                }
            }
        } else {
            //親がいるならそいつの子供になる
            this.gameObject.transform.parent = createSorceObj.transform;
        }
        //削除準備OK
        IsReadyToDestroy(true);
    }

    public void callBackVertex(Vector3[] vertexies, int poleNum) {
        isCalledBackVertex[poleNum - 0] = true;
        for (int i = 0; i < 8; i++)
            vertex[( poleNum - 0 ) * 8 + i] = vertexies[i % 4];

        vertex[8 * poleSum * ( variablesC.trapezoidDivisionNum + 1 ) + poleNum - 0] = variablesC.createSourcePosition;
        vertex[9 * poleSum * ( variablesC.trapezoidDivisionNum + 1 ) + poleNum - 0] = new Vector3(variablesC.createSourcePosition.x, variablesC.createSourcePosition.y, variablesC.createSourcePosition.z + variablesC.poleHeight);
    }

    public void IsReadyToDestroy(bool ready) {
        isReadyToDestroy = ready;
    }

    public void SetCreateSorce(GameObject sorceObject) {
        createSorceObj = sorceObject;
    }

    private float calcTheoreticalRadiusout() {
        return ( variablesC.radiusOut - variablesC.radiusIn ) / 2f;
    }
}
