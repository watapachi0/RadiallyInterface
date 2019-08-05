using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/*
 * 
 * オブジェクトの描画と各種コンポーネントのアタッチのみ行う
 * 台形
 * 
 * TrapezoidPole.csから拡張した内容
 * 任意の数に台形を分割し、一つのメッシュとして用いる
 * 台形
 * 台形分割は21分割以下にすること（MeshRendererの都合で256ポリゴン以下である必要がある。Cube12面*21＜256）
 * //　　　　台形分割は31分割以下にすること（MeshRendererの都合で256ポリゴン以下である必要がある。端4枚+側面8枚*31＜256）
 * 
 */

public class MultipleTrapezoidPole : MonoBehaviour {

    private int poleNum;

    private createTrapezoidPole createSorce;
    private centralSystem systemScript;

    //頂点座標
    Vector3[] EndVertex = new Vector3[4];
    Vector3[] SideVertex = new Vector3[24];

    //面情報
    int[] EndFace1 = new int[6]  { 1,      3,      0,
                                   3,      2,      0 };
    int[] SideFace = new int[24] { 1,      3,      0,
                                   3,      2,      0,

                                   7,      6,      4,
                                   5,      7,      4,

                                   2 + 8,  4 + 8,  0 + 8,
                                   4 + 8,  6 + 8,  0 + 8,

                                   7 + 8,  3 + 8,  1 + 8,
                                   5 + 8,  3 + 8,  7 + 8,
    };

    //文字のゲームオブジェクト
    TextMesh TmeshC;

    //分割体の何体目か
    private int DivisionNum;

    //Text情報
    public string MyText { get; set; } = "";

    private void Awake() {
        createSorce = GameObject.Find("central").GetComponent<createTrapezoidPole>();
        systemScript = GameObject.Find("central").GetComponent<centralSystem>();
    }

    void Start() {
        //自分の番号を名前から取得し初期化
        poleNum = int.Parse(transform.name) - 1;

        // CombineMeshes()する時に使う配列   始端と終端も含めるので+2
        CombineInstance[] combineInstanceAry = new CombineInstance[variables.trapezoidDivisionNum + 2];

        for (DivisionNum = 0; DivisionNum < variables.trapezoidDivisionNum + 2; DivisionNum++) {
            //頂点計算
            CalcVertices();

            if (DivisionNum == 0) {
                //最初の一枚だけ別計算
                //メッシュ作成
                Mesh meshFirst = new Mesh();
                //メッシュリセット
                meshFirst.Clear();
                //メッシュへの頂点情報の追加
                meshFirst.vertices = EndVertex;
                //メッシュへの面情報の追加
                meshFirst.triangles = EndFace1;

                // 合成するMesh（同じMeshを円形に並べたMesh）
                combineInstanceAry[0].mesh = meshFirst;
            }

            //メッシュ作成
            Mesh mesh = new Mesh();
            //メッシュリセット
            mesh.Clear();
            //メッシュへの頂点情報の追加
            mesh.vertices = SideVertex;
            //メッシュへの面情報の追加
            mesh.triangles = SideFace;

            // 合成するMesh（同じMeshを円形に並べたMesh）
            combineInstanceAry[DivisionNum + 1].mesh = mesh;

            if (DivisionNum == variables.trapezoidDivisionNum - 1) {
                //最後の一枚だけ別計算
                //メッシュ作成
                Mesh meshFirst = new Mesh();
                //メッシュリセット
                meshFirst.Clear();
                //メッシュへの頂点情報の追加
                meshFirst.vertices = EndVertex;
                //メッシュへの面情報の追加
                meshFirst.triangles = EndFace1;

                // 合成するMesh（同じMeshを円形に並べたMesh）
                combineInstanceAry[DivisionNum + 1].mesh = meshFirst;
            }
        }
        // 合成した（する）メッシュ
        Mesh combinedMesh = new Mesh();
        combinedMesh.name = transform.name;
        combinedMesh.CombineMeshes(combineInstanceAry);
        // 上書きする
        this.GetComponent<MeshFilter>().mesh = combinedMesh;

        //メッシュフィルター追加
        MeshFilter mesh_filter = new MeshFilter();
        this.gameObject.AddComponent<MeshFilter>();
        mesh_filter = GetComponent<MeshFilter>();
        //メッシュアタッチ
        mesh_filter.mesh = combinedMesh;
        //レンダラー追加 + マテリアルアタッチ
        this.gameObject.AddComponent<MeshRenderer>();
        this.gameObject.GetComponent<MeshRenderer>().material = variables.material_TrapezoidPole_Normal;
        //コライダーアタッチ
        this.gameObject.AddComponent<MeshCollider>();
        this.gameObject.GetComponent<MeshCollider>().sharedMesh = mesh_filter.mesh;

        //NormalMapの再計算
        mesh_filter.mesh.RecalculateNormals();


        //暫定当たり判定用Event Trigger
        //イベントトリガーのアタッチと初期化
        EventTrigger currentTrigger = this.gameObject.AddComponent<EventTrigger>();
        currentTrigger.triggers = new List<EventTrigger.Entry>();
        //イベントトリガーのトリガーイベント作成
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener((x) => OnTouchPointer());  //ラムダ式の右側は追加するメソッド
                                                              //トリガーイベントのアタッチ
        currentTrigger.triggers.Add(entry);

        /* 侵入イベント用 */
        //侵入時に色を変える
        EventTrigger.Entry entry2 = new EventTrigger.Entry();
        entry2.eventID = EventTriggerType.PointerEnter;
        entry2.callback.AddListener((x) => OnMouseEnter());
        currentTrigger.triggers.Add(entry2);
        //侵入終了時に色を戻す
        EventTrigger.Entry entry3 = new EventTrigger.Entry();
        entry3.eventID = EventTriggerType.PointerExit;
        entry3.callback.AddListener((x) => OnMouseExit());
        currentTrigger.triggers.Add(entry3);

        //テキスト表示
        make3Dtext();

        /*        //縁取り
                LineRenderer lineRenderer = this.gameObject.AddComponent<LineRenderer>();
                //ローカルで描画
                lineRenderer.useWorldSpace = false;
                //頂点数
                lineRenderer.positionCount = 16;
                //幅
                lineRenderer.startWidth = 0.1f;
                //頂点　一筆書きのため、重複あり
                float ShiftSlightly = 0.01f;    //見づらかったからすこしずらした
                Vector3[] lineVertecies = new Vector3[16] { vertex[0] + Vector3.back    * ShiftSlightly, vertex[2] + Vector3.back    * ShiftSlightly, vertex[4] + Vector3.back    * ShiftSlightly, vertex[6] + Vector3.back    * ShiftSlightly,
                                                            vertex[7] + Vector3.forward * ShiftSlightly, vertex[5] + Vector3.forward * ShiftSlightly, vertex[3] + Vector3.forward * ShiftSlightly, vertex[1] + Vector3.forward * ShiftSlightly,
                                                            vertex[7] + Vector3.forward * ShiftSlightly, vertex[1] + Vector3.forward * ShiftSlightly, vertex[0] + Vector3.back    * ShiftSlightly, vertex[6] + Vector3.back    * ShiftSlightly,
                                                            vertex[4] + Vector3.back    * ShiftSlightly, vertex[5] + Vector3.forward * ShiftSlightly, vertex[3] + Vector3.forward * ShiftSlightly, vertex[2] + Vector3.back    * ShiftSlightly };
                lineRenderer.SetPositions(lineVertecies);
                //線の折れる部分の丸み具合
                lineRenderer.numCornerVertices = 20;
                //線の端の丸み具合
                lineRenderer.numCapVertices = 20;
                //線の色
                lineRenderer.material = variables.material_LineRenderer;
                //影を発生させない
                lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                //影の影響を受けない
                lineRenderer.receiveShadows = false;
                */
    }

    void Update() {
        //テキストの更新
        TmeshC.text = MyText;
    }

    private void CalcVertices() {
        //台形の外側の頂点座標その1
        Vector3 vertex1 = new Vector3(variables.radiusOut * Mathf.Sin(( poleNum + 0 ) / (float)variables.poleSum * Mathf.PI * 2),
                                      variables.radiusOut * Mathf.Cos(( poleNum + 0 ) / (float)variables.poleSum * Mathf.PI * 2),
                                      0);
        //台形の外側の頂点座標その2 
        Vector3 vertex2 = new Vector3(variables.radiusOut * Mathf.Sin(( poleNum + 1 ) / (float)variables.poleSum * Mathf.PI * 2),
                                      variables.radiusOut * Mathf.Cos(( poleNum + 1 ) / (float)variables.poleSum * Mathf.PI * 2),
                                      0);
        //台形の内側の頂点座標その1
        Vector3 vertex3 = new Vector3(variables.radiusIn * Mathf.Sin(( poleNum + 0 ) / (float)variables.poleSum * Mathf.PI * 2),
                                      variables.radiusIn * Mathf.Cos(( poleNum + 0 ) / (float)variables.poleSum * Mathf.PI * 2),
                                      0);
        //台形の内側の頂点座標その2
        Vector3 vertex4 = new Vector3(variables.radiusIn * Mathf.Sin(( poleNum + 1 ) / (float)variables.poleSum * Mathf.PI * 2),
                                      variables.radiusIn * Mathf.Cos(( poleNum + 1 ) / (float)variables.poleSum * Mathf.PI * 2),
                                      0);
        //全頂点数8にそれぞれ座標が2つずつある
        for (int i = 0; i < 8 * 2; i++) {
            if (i % 8 == 0) {
                SideVertex[i] = vertex3;
            } else if (i % 8 == 1) {
                SideVertex[i] = new Vector3(vertex3.x, vertex3.y, variables.poleHeight);
            } else if (i % 8 == 2) {
                SideVertex[i] = vertex1;
            } else if (i % 8 == 3) {
                SideVertex[i] = new Vector3(vertex1.x, vertex1.y, variables.poleHeight);
            } else if (i % 8 == 4) {
                SideVertex[i] = vertex2;
            } else if (i % 8 == 5) {
                SideVertex[i] = new Vector3(vertex2.x, vertex2.y, variables.poleHeight);
            } else if (i % 8 == 6) {
                SideVertex[i] = vertex4;
            } else if (i % 8 == 7) {
                SideVertex[i] = new Vector3(vertex4.x, vertex4.y, variables.poleHeight);
            } else {
                Debug.LogWarning("Calcration Error");
            }

        }
        createSorce.callBackVertex(new Vector3[4] { SideVertex[0], SideVertex[6], SideVertex[1], SideVertex[7] }, ( int.Parse(gameObject.name) - 1 ) * variables.trapezoidDivisionNum + DivisionNum + 1);
    }

    private void OnTouchPointer() {
        systemScript.UpdateChuringNum(int.Parse(gameObject.name));
    }

    private void OnMouseEnter() {
        this.gameObject.GetComponent<MeshRenderer>().material = variables.material_TrapezoidPole_Touch;
    }

    private void OnMouseExit() {
        this.gameObject.GetComponent<MeshRenderer>().material = variables.material_TrapezoidPole_Normal;
    }

    private void make3Dtext() {
        //中心のUI表示
        GameObject textCentor = new GameObject("text");
        MeshRenderer MRC = textCentor.AddComponent<MeshRenderer>();
        TmeshC = textCentor.AddComponent<TextMesh>();
        //文字サイズ
        TmeshC.fontSize = 100;
        //アンカー位置を中心に
        TmeshC.anchor = TextAnchor.MiddleCenter;
        //真ん中寄せ
        TmeshC.alignment = TextAlignment.Center;
        //表示文字(更新されればError表示は消えるはず)
        TmeshC.text = "Error";
        //位置調整（台形の中心に設定）
        textCentor.transform.position = ( SideVertex[0] + SideVertex[2] + SideVertex[4] + SideVertex[6] ) / 4f;
        //大きさ
        textCentor.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        //子オブジェクトに設定
        textCentor.transform.parent = this.transform;
    }
}

/*
 using UnityEngine;

public class CircleCubes : MonoBehaviour
{
    static readonly float R = 10;
    static readonly float CubeNum = 20;

    void Start ()
    {
        // ひとつだけCubeを生成して使いまわす。ただし、Mesh自体は差し替える
        var o = GameObject.CreatePrimitive(PrimitiveType.Cube);

        o.transform.parent = GetComponent<Transform>();
        // BoxColliderは使わないので消す
        GameObject.Destroy(o.GetComponent<BoxCollider>());

        // meshではなく、sharedMeshの方が良いそう
        Mesh cubeMesh = o.GetComponent<MeshFilter>().sharedMesh;

        // CombineMeshes()する時に使う配列
        CombineInstance[] combineInstanceAry = new CombineInstance[(int)CubeNum];

        for(float rad = 0,int index = 0; rad < 2*Mathf.PI; rad += 2 * Mathf.PI / CubeNum)
        {
            // 合成するMesh（同じMeshを円形に並べたMesh）
            combineInstanceAry[index].mesh = cubeMesh;
            // 配置場所。ここのtransformはMatrix4x4。名前が紛らわしい。
            combineInstanceAry[index].transform = Matrix4x4.Translate(new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * R);
            index++;
        }

        // 合成した（する）メッシュ
        var combinedMesh = new Mesh();
        combinedMesh.name = "Cubes";
        combinedMesh.CombineMeshes(combineInstanceAry);
        // 上書きする
        o.GetComponent<MeshFilter>().mesh = combinedMesh;
    }   
}
     */

