using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/*
 * 
 * オブジェクトの描画と各種コンポーネントのアタッチのみ行う
 * 台形
 * 
 */

public class TrapezoidPole : MonoBehaviour {

    private int poleNum;

    private createTrapezoidPole createSorce;
    private centralSystem systemScript;

    //頂点座標
    Vector3[] vertex = new Vector3[24];

    //面情報
    int[] face = new int[] { 1,      3,      0,
                             3,      2,      0,

                             5,      4,      2 + 8,
                             3 + 8,  5,      2 + 8,

                             7,      6,      4 + 8,
                             5 + 8,  7,      4 + 8,

                             1 + 8,  0 + 8,  6 + 8,
                             7 + 8,  1 + 8,  6 + 8,

                             2 + 16, 4 + 16, 0 + 16,
                             4 + 16, 6 + 16, 0 + 16,

                             7 + 16, 3 + 16, 1 + 16,
                             5 + 16, 3 + 16, 7 + 16,
    };

    MeshRenderer meshRenderer;

    //文字のゲームオブジェクト
    TextMesh TmeshC;

    //Text情報
    public string MyText { get; set; } = "";

    private void Awake() {
        createSorce = GameObject.Find("central").GetComponent<createTrapezoidPole>();
        systemScript = GameObject.Find("central").GetComponent<centralSystem>();
    }

    void Start() {
        //自分の番号を名前から取得し初期化
        poleNum = int.Parse(transform.name) - 1;

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
        meshRenderer= this.gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = variables.material_TrapezoidPole_Normal;
        //コライダーアタッチ
        this.gameObject.AddComponent<MeshCollider>();
        this.gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;

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
        //全頂点数8にそれぞれ座標が3つずつある
        for (int i = 0; i < 8 * 3; i++) {
            if (i % 8 == 0) {
                vertex[i] = vertex3;
            } else if (i % 8 == 1) {
                vertex[i] = new Vector3(vertex3.x, vertex3.y, variables.poleHeight);
            } else if (i % 8 == 2) {
                vertex[i] = vertex1;
            } else if (i % 8 == 3) {
                vertex[i] = new Vector3(vertex1.x, vertex1.y, variables.poleHeight);
            } else if (i % 8 == 4) {
                vertex[i] = vertex2;
            } else if (i % 8 == 5) {
                vertex[i] = new Vector3(vertex2.x, vertex2.y, variables.poleHeight);
            } else if (i % 8 == 6) {
                vertex[i] = vertex4;
            } else if (i % 8 == 7) {
                vertex[i] = new Vector3(vertex4.x, vertex4.y, variables.poleHeight);
            } else {
                Debug.LogWarning("Calcration Error");
            }

        }
        createSorce.callBackVertex(new Vector3[4] { vertex[0], vertex[6], vertex[1], vertex[7] }, int.Parse(gameObject.name));
    }

    private void OnTouchPointer() {
        systemScript.UpdateChuringNum(int.Parse(gameObject.name));
    }

    private void OnMouseEnter() {
        meshRenderer.material = variables.material_TrapezoidPole_Touch;
    }

    private void OnMouseExit() {
        meshRenderer.material = variables.material_TrapezoidPole_Normal;
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
        textCentor.transform.position = ( vertex[0] + vertex[2] + vertex[4] + vertex[6] ) / 4f;
        //大きさ
        textCentor.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        //子オブジェクトに設定
        textCentor.transform.parent = this.transform;
    }
}
