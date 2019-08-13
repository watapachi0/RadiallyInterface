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
    //LineRenderer用の頂点情報
    Vector3[] LineVertex;
    Vector3[] lineVertecies;   //lineVertexの配置を格納

    //面情報
    int[] EndFace = new int[6]  { 1,      0,      3,
                                  0,      2,      3 };
    int[] SideFace = new int[24] { 2,      5,      4,
                                   2,      3,      5,

                                   1,      6,      7,
                                   1,      0,      6,

                                   2 + 8,  4 + 8,  0 + 8,
                                   4 + 8,  6 + 8,  0 + 8,

                                   7 + 8,  3 + 8,  1 + 8,
                                   5 + 8,  3 + 8,  7 + 8,
    };

    //文字のゲームオブジェクト
    TextMesh TmeshC;

    //分割体の何体目か
    private int DivisionNum;

    //Textの位置
    private Vector3 textPosition;

    //Text情報
    public string MyText { get; set; } = "";

    private void Awake() {
        createSorce = GameObject.Find("central").GetComponent<createTrapezoidPole>();
        systemScript = GameObject.Find("central").GetComponent<centralSystem>();
    }

    void Start() {
        //自分の番号を名前から取得し初期化
        poleNum = int.Parse(transform.name) - 1;

        //LineVertex初期化  最初の面4頂点+中間面4頂点+最後の面4頂点
        LineVertex = new Vector3[4 + ( variables.trapezoidDivisionNum - 1 ) * 4 + 4];
        //格納先も初期化
        lineVertecies = new Vector3[8 + 4 * ( variables.trapezoidDivisionNum - 1 ) + 8];

        // CombineMeshes()する時に使う配列   始端と終端も含めるので+2
        CombineInstance[] combineInstanceAry = new CombineInstance[variables.trapezoidDivisionNum + 2];

        //メッシュ作成
        for (DivisionNum = 0; DivisionNum < variables.trapezoidDivisionNum; DivisionNum++) {
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
                meshFirst.triangles = EndFace;

                // 合成するMesh（同じMeshを円形に並べたMesh）
                combineInstanceAry[0].mesh = meshFirst;
                combineInstanceAry[0].transform = Matrix4x4.Translate(transform.position);
            }
            Mesh mesh = new Mesh();
            //メッシュリセット
            mesh.Clear();
            //メッシュへの頂点情報の追加
            mesh.vertices = SideVertex;
            //メッシュへの面情報の追加
            mesh.triangles = SideFace;

            //合成するMesh（同じMeshを円形に並べたMesh）
            combineInstanceAry[DivisionNum + 1].mesh = mesh;
            combineInstanceAry[DivisionNum + 1].transform = Matrix4x4.Translate(transform.position);

            if (DivisionNum + 1 == variables.trapezoidDivisionNum) {
                //最後の一枚だけ別計算
                //メッシュ作成
                Mesh meshLast = new Mesh();
                //メッシュリセット
                meshLast.Clear();
                //メッシュへの頂点情報の追加
                meshLast.vertices = EndVertex;
                //メッシュへの面情報の追加
                meshLast.triangles = EndFace;

                // 合成するMesh（同じMeshを円形に並べたMesh）
                combineInstanceAry[DivisionNum + 2].mesh = meshLast;
                combineInstanceAry[DivisionNum + 2].transform = Matrix4x4.Translate(transform.position);
            }
        }
        //合成した（する）メッシュ
        Mesh combinedMesh = new Mesh();
        combinedMesh.name = transform.name;
        combinedMesh.CombineMeshes(combineInstanceAry);

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




        //縁取り
        LineRenderer lineRenderer = this.gameObject.AddComponent<LineRenderer>();
        //ローカルで描画
        lineRenderer.useWorldSpace = false;
        //頂点数
        lineRenderer.positionCount = lineVertecies.Length;
        //幅
        lineRenderer.startWidth = 0.1f;
        //順番の格納
        SetLineVertecies();
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



    }

    void Update() {
        //テキストの更新
        TmeshC.text = MyText;
    }

    private void CalcVertices() {
        //台形の外側の頂点座標その1
        Vector3 vertex1 = new Vector3(variables.radiusOut * Mathf.Sin(( (float)poleNum * variables.trapezoidDivisionNum + DivisionNum + 0 ) / (float)( variables.poleSum * variables.trapezoidDivisionNum ) * Mathf.PI * 2),
                                      variables.radiusOut * Mathf.Cos(( (float)poleNum * variables.trapezoidDivisionNum + DivisionNum + 0 ) / (float)( variables.poleSum * variables.trapezoidDivisionNum ) * Mathf.PI * 2),
                                      0);
        //台形の外側の頂点座標その2 
        Vector3 vertex2 = new Vector3(variables.radiusOut * Mathf.Sin(( (float)poleNum * variables.trapezoidDivisionNum + DivisionNum + 1 ) / (float)( variables.poleSum * variables.trapezoidDivisionNum ) * Mathf.PI * 2),
                                      variables.radiusOut * Mathf.Cos(( (float)poleNum * variables.trapezoidDivisionNum + DivisionNum + 1 ) / (float)( variables.poleSum * variables.trapezoidDivisionNum ) * Mathf.PI * 2),
                                      0);
        //台形の内側の頂点座標その1
        Vector3 vertex3 = new Vector3(variables.radiusIn * Mathf.Sin(( (float)poleNum * variables.trapezoidDivisionNum + DivisionNum + 0 ) / (float)( variables.poleSum * variables.trapezoidDivisionNum ) * Mathf.PI * 2),
                                      variables.radiusIn * Mathf.Cos(( (float)poleNum * variables.trapezoidDivisionNum + DivisionNum + 0 ) / (float)( variables.poleSum * variables.trapezoidDivisionNum ) * Mathf.PI * 2),
                                      0);
        //台形の内側の頂点座標その2
        Vector3 vertex4 = new Vector3(variables.radiusIn * Mathf.Sin(( (float)poleNum * variables.trapezoidDivisionNum + DivisionNum + 1 ) / (float)( variables.poleSum * variables.trapezoidDivisionNum ) * Mathf.PI * 2),
                                      variables.radiusIn * Mathf.Cos(( (float)poleNum * variables.trapezoidDivisionNum + DivisionNum + 1 ) / (float)( variables.poleSum * variables.trapezoidDivisionNum ) * Mathf.PI * 2),
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

        //LineRenderer用
        for (int i = 0; i < 4; i++) {
            LineVertex[DivisionNum * 4 + i] = SideVertex[i];
        }
        if (DivisionNum + 1 == variables.trapezoidDivisionNum) {
            LineVertex[DivisionNum * 4 + 0 + 4] = SideVertex[6];
            LineVertex[DivisionNum * 4 + 1 + 4] = SideVertex[7];
            LineVertex[DivisionNum * 4 + 2 + 4] = SideVertex[4];
            LineVertex[DivisionNum * 4 + 3 + 4] = SideVertex[5];
        }

        //端面用
        if (DivisionNum == 0) {
            //最初の端面
            EndVertex[0] = vertex1;
            EndVertex[1] = new Vector3(vertex1.x, vertex1.y, variables.poleHeight);
            EndVertex[2] = vertex3;
            EndVertex[3] = new Vector3(vertex3.x, vertex3.y, variables.poleHeight);
        } else if (DivisionNum + 1 == variables.trapezoidDivisionNum) {
            //最後の端面
            EndVertex[0] = new Vector3(vertex2.x, vertex2.y, variables.poleHeight);
            EndVertex[1] = vertex2;
            EndVertex[2] = new Vector3(vertex4.x, vertex4.y, variables.poleHeight);
            EndVertex[3] = vertex4;
        }



        createSorce.callBackVertex(new Vector3[4] { SideVertex[0], SideVertex[6], SideVertex[1], SideVertex[7] }, ( int.Parse(gameObject.name) - 1 ) * variables.trapezoidDivisionNum + DivisionNum + 0);

        //Textの座標を計算する
        if (DivisionNum == variables.trapezoidDivisionNum / 2) {
            if (variables.trapezoidDivisionNum % 2 == 0) {
                textPosition = ( SideVertex[0] + SideVertex[2] ) / 2f;
            } else {
                textPosition = ( SideVertex[0] + SideVertex[2] + SideVertex[4] + SideVertex[6] ) / 4f;
            }
        }
    }

    //LineVertexからlineVerteciesに整列しなおす（長いので別メソッド）
    private void SetLineVertecies() {
        //見づらかったからすこしずらした
        float ShiftSlightly = 0.01f;
        Vector3 zPlus = Vector3.forward * ShiftSlightly;
        Vector3 zMinus = Vector3.back * ShiftSlightly;

        lineVertecies[0] = LineVertex[0] + zMinus;
        lineVertecies[1] = LineVertex[1] + zPlus;
        lineVertecies[1 + variables.trapezoidDivisionNum] = LineVertex[4 * variables.trapezoidDivisionNum + 1] + zPlus;
        lineVertecies[2 + variables.trapezoidDivisionNum] = LineVertex[4 * variables.trapezoidDivisionNum + 0] + zMinus;
        lineVertecies[2 + 2 * variables.trapezoidDivisionNum] = LineVertex[0] + zMinus;
        lineVertecies[3 + 2 * variables.trapezoidDivisionNum] = LineVertex[2] + zMinus;
        lineVertecies[4 + 2 * variables.trapezoidDivisionNum] = LineVertex[3] + zPlus;
        lineVertecies[5 + 2 * variables.trapezoidDivisionNum] = LineVertex[1] + zPlus;
        lineVertecies[6 + 2 * variables.trapezoidDivisionNum] = LineVertex[3] + zPlus;
        lineVertecies[6 + 3 * variables.trapezoidDivisionNum] = LineVertex[4 * variables.trapezoidDivisionNum + 3] + zPlus;
        lineVertecies[7 + 3 * variables.trapezoidDivisionNum] = LineVertex[4 * variables.trapezoidDivisionNum + 1] + zPlus;
        lineVertecies[8 + 3 * variables.trapezoidDivisionNum] = LineVertex[4 * variables.trapezoidDivisionNum + 3] + zPlus;
        lineVertecies[9 + 3 * variables.trapezoidDivisionNum] = LineVertex[4 * variables.trapezoidDivisionNum + 2] + zMinus;
        lineVertecies[10 + 3 * variables.trapezoidDivisionNum] = LineVertex[4 * variables.trapezoidDivisionNum + 0] + zMinus;
        lineVertecies[11 + 3 * variables.trapezoidDivisionNum] = LineVertex[4 * variables.trapezoidDivisionNum + 2] + zMinus;
        lineVertecies[11 + 4 * variables.trapezoidDivisionNum] = LineVertex[2] + zMinus;
        for (int i = 1; i < variables.trapezoidDivisionNum; i++) {
            lineVertecies[1 + i] = LineVertex[4 * i + 1] + zPlus;
            lineVertecies[2 + variables.trapezoidDivisionNum + i] = LineVertex[4 * ( variables.trapezoidDivisionNum - i ) + 0] + zMinus;
            lineVertecies[6 + 2 * variables.trapezoidDivisionNum + i] = LineVertex[4 * i + 3] + zPlus;
            lineVertecies[11 + 3 * variables.trapezoidDivisionNum + i] = LineVertex[4 * ( variables.trapezoidDivisionNum - i ) + 2] + zMinus;
        }

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
        textCentor.transform.position = textPosition;
        //大きさ
        textCentor.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        //子オブジェクトに設定
        textCentor.transform.parent = this.transform;
    }
}
