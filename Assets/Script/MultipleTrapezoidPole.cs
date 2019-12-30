//using System.Collections;
using System.Collections.Generic;
//using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

/*
 *
 * オブジェクトの描画と各種コンポーネントのアタッチのみ行う
 * 台形
 *
 * TrapezoidPoleC.csから拡張した内容
 * 任意の数に台形を分割し、一つのメッシュとして用いる
 * 台形
 * 台形分割は21分割以下にすること（MeshRendererの都合で256ポリゴン以下である必要がある。Cube12面*21＜256）
 * //　　　　台形分割は31分割以下にすること（MeshRendererの都合で256ポリゴン以下である必要がある。端4枚+側面8枚*31＜256）
 *
 */

public class MultipleTrapezoidPole : MonoBehaviour {
    float TimeCount = 150.0f;
    private int poleNum;

    private createTrapezoidPole createSorce;
    private centralSystem systemScript;
    private GameObject myParent = null;
    private int subCircleNum;
    private int poleSum;

    //何回も入力が走る対策
    private bool doneEnter = false;
    private bool flgEnter = false;
    private bool flgExit = false;
    private GameObject enterObject = null;
    private bool inCol = false;

    //頂点座標
    Vector3[] EndVertex = new Vector3[4];
    Vector3[] SideVertex = new Vector3[24];
    Vector3[] StartVertex = new Vector3[4];
    //LineRenderer用の頂点情報
    Vector3[] LineVertex;
    Vector3[] lineVertecies;   //lineVertexの配置を格納
    //色の変更など
    MeshRenderer meshRenderer;
    //当たり判定
    MeshCollider meshCollider;
    //縁取り用
    LineRenderer lineRenderer;
    //文字のレンダラー
    MeshRenderer textMeshRender;

    //stage情報保存
    int stage;
    //副輪であるか
    public bool isSubRingPole { get; set; } = false;

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

    //法線を計算するための頂点の組み合わせ
    int[] normalNumber = new int[] {1,2,3,
        3,4,5,
        5,6,7,
        6,4,2,
        0,1,6,
        1,3,5,};
    //各台形の内向き法線ベクトル
    Vector3[,] normalVector;
    //基準の法線群
    Vector3[,] normalBasicVec;

    //文字のゲームオブジェクト
    public TextMesh TmeshC;

    //分割体の何体目か
    private int DivisionNum;

    //Textの位置
    private Vector3 textPosition;

    //Text情報
    [SerializeField]
    public string MyText { get; set; } = "";

    //Circle用 自身がアクティブかどうか
    public bool isActiveObj { get; set; } = true;

    private void Awake() {
    }

    void Start() {
        //stage情報初期化
        stage = variables.stage;

        //法線初期化
        normalVector = new Vector3[variables.trapezoidDivisionNum + 1, 6];
        normalBasicVec = new Vector3[variables.trapezoidDivisionNum + 1, 6];

        if (myParent == null) {
            createSorce = GameObject.Find("central").GetComponent<createTrapezoidPole>();
            //現在のTextSetの段数を取得
            poleSum = variables.poleSum;
        } else {
            createSorce = myParent.GetComponent<createTrapezoidPole>();
            //自分の親の名前から該当TextSetの行からアイテム数を取得
            //Debug.Log(myParent.name.Substring(9));
            poleSum = GameObject.Find("central").GetComponent<centralSystem>().GetTextSetItemNum(int.Parse(myParent.name.Substring(9)));
        }

        systemScript = GameObject.Find("central").GetComponent<centralSystem>();

        //自分の番号を名前から取得し初期化
        poleNum = int.Parse(transform.name) - 1;

        //LineVertex初期化  最初の面4頂点+中間面4頂点+最後の面4頂点
        LineVertex = new Vector3[4 + variables.trapezoidDivisionNum * 4 + 4];
        //格納先も初期化
        lineVertecies = new Vector3[8 + 4 * variables.trapezoidDivisionNum + 8];

        // CombineMeshes()する時に使う配列   始端と終端も含めるので+3
        CombineInstance[] combineInstanceAry = new CombineInstance[variables.trapezoidDivisionNum + 3];

        //メッシュ作成
        for (DivisionNum = 0; DivisionNum <= variables.trapezoidDivisionNum; DivisionNum++) {
            //頂点計算
            CalcVertices();

            if (DivisionNum == 0) {
                //最初の一枚だけ別計算
                //メッシュ作成
                Mesh meshFirst = new Mesh();
                //メッシュリセット
                meshFirst.Clear();
                //メッシュへの頂点情報の追加
                meshFirst.vertices = StartVertex;
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

            if (DivisionNum == variables.trapezoidDivisionNum) {
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
        //内向き法線の計算
        culcNormal(combinedMesh.vertices);

        //メッシュフィルター追加
        MeshFilter mesh_filter = new MeshFilter();
        mesh_filter = this.gameObject.AddComponent<MeshFilter>();
        //メッシュアタッチ
        mesh_filter.mesh = combinedMesh;
        //レンダラー追加 + マテリアルアタッチ
        meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = variables.material_TrapezoidPole_Normal;
        //コライダーアタッチ
        meshCollider = this.gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh_filter.mesh;
        meshCollider.convex = true;
        meshCollider.isTrigger = true;

        //NormalMapの再計算
        mesh_filter.mesh.RecalculateNormals();

        //XRじゃないなら
        if (!variables.isOnXR) {
            //暫定当たり判定用Event Trigger
            //イベントトリガーのアタッチと初期化
            EventTrigger currentTrigger = this.gameObject.AddComponent<EventTrigger>();
            currentTrigger.triggers = new List<EventTrigger.Entry>();

            //侵入イベント用
            //侵入時に色を変える
            //イベントトリガーのトリガーイベント作成
            EventTrigger.Entry entry1 = new EventTrigger.Entry();
            entry1.eventID = EventTriggerType.PointerEnter;
            entry1.callback.AddListener((x) => OnMouseEnterOwnMade());  //ラムダ式の右側は追加するメソッド
            //トリガーイベントのアタッチ
            currentTrigger.triggers.Add(entry1);
            //侵入終了時に色を戻す
            EventTrigger.Entry entry2 = new EventTrigger.Entry();
            entry2.eventID = EventTriggerType.PointerExit;
            entry2.callback.AddListener((x) => OnMouseExitOwnMade());
            currentTrigger.triggers.Add(entry2);
        }

        //テキスト表示
        make3Dtext();

        //縁取り
        lineRenderer = this.gameObject.AddComponent<LineRenderer>();
        //ローカルで描画
        lineRenderer.useWorldSpace = false;
        //頂点数
        lineRenderer.positionCount = lineVertecies.Length;
        //幅
        lineRenderer.startWidth = variables.lineRendererWidth;
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

        //クリエイト元を親にする
        transform.parent = createSorce.gameObject.transform;
    }

    void Update() {
      TimeCount -= Time.deltaTime;
        //RadiallyはcentralSystemのコルーチン内で初期化される
        if (variables.isCircleSystem && MyText == "") {
            askMyText();
            //stage情報取得
            stage = variables.stage;
        }

        //テキストの更新
        TmeshC.text = MyText;

      if(TimeCount <= 0){

        if (!variables.isCircleSystem)
            inCol = fireInnerProductCollider();
        else if (isActiveObj && meshRenderer.enabled)
            inCol = fireInnerProductCollider();

        if (!doneEnter && inCol) {
            doneEnter = true;
            OnTriggerEnterOwnMade(null);
        } else if (doneEnter && !inCol) {
            doneEnter = false;
            OnTriggerExitOwnMade(null);

            TimeCount = 150.0f;

        }
      }

    }

    private void CalcVertices() {
        //Circle用副輪の半径対応
        float radiusOut;
        float radiusIn;
        if (variables.isCircleSystem && isSubRingPole) {
            radiusOut = variables.radiusOut_subCircle;
            radiusIn = variables.radiusIn_subCircle;
        } else {
            radiusOut = variables.radiusOut;
            radiusIn = variables.radiusIn;
        }
        float nSum = (float)( poleSum * ( variables.trapezoidDivisionNum + 1 ) );
        float nNum = (float)poleNum * ( variables.trapezoidDivisionNum + 1 ) + DivisionNum;

        //台形の外側左の頂点座標その1
        Vector3 vertex1 = new Vector3(radiusOut * Mathf.Cos(( ( nNum + 0 ) / nSum - 1f / ( 2f * poleSum ) ) * Mathf.PI * 2f) * -1,
                                      radiusOut * Mathf.Sin(( ( nNum + 0 ) / nSum - 1f / ( 2f * poleSum ) ) * Mathf.PI * 2f),
                                      0)
                                      + variables.createSourcePosition;
        //台形の外側右の頂点座標その2
        Vector3 vertex2 = new Vector3(radiusOut * Mathf.Cos(( ( nNum + 1 ) / nSum - 1f / ( 2f * poleSum ) ) * Mathf.PI * 2f) * -1,
                                      radiusOut * Mathf.Sin(( ( nNum + 1 ) / nSum - 1f / ( 2f * poleSum ) ) * Mathf.PI * 2f),
                                      0)
                                      + variables.createSourcePosition;
        //台形の内側左の頂点座標その1
        Vector3 vertex3 = new Vector3(radiusIn * Mathf.Cos(( ( nNum + 0 ) / nSum - 1f / ( 2f * poleSum ) ) * Mathf.PI * 2f) * -1,
                                      radiusIn * Mathf.Sin(( ( nNum + 0 ) / nSum - 1f / ( 2f * poleSum ) ) * Mathf.PI * 2f),
                                      0)
                                      + variables.createSourcePosition;
        //台形の内側右の頂点座標その2
        Vector3 vertex4 = new Vector3(radiusIn * Mathf.Cos(( ( nNum + 1 ) / nSum - 1f / ( 2f * poleSum ) ) * Mathf.PI * 2f) * -1,
                                      radiusIn * Mathf.Sin(( ( nNum + 1 ) / nSum - 1f / ( 2f * poleSum ) ) * Mathf.PI * 2f),
                                      0)
                                      + variables.createSourcePosition;
        //全頂点数8にそれぞれ座標が2つずつある
        for (int i = 0; i < 8 * 2; i++) {
            if (i % 8 == 0) {
                SideVertex[i] = vertex3;
            } else if (i % 8 == 1) {
                SideVertex[i] = new Vector3(vertex3.x, vertex3.y, vertex3.z + variables.poleHeight);
            } else if (i % 8 == 2) {
                SideVertex[i] = vertex1;
            } else if (i % 8 == 3) {
                SideVertex[i] = new Vector3(vertex1.x, vertex1.y, vertex1.z + variables.poleHeight);
            } else if (i % 8 == 4) {
                SideVertex[i] = vertex2;
            } else if (i % 8 == 5) {
                SideVertex[i] = new Vector3(vertex2.x, vertex2.y, vertex2.z + variables.poleHeight);
            } else if (i % 8 == 6) {
                SideVertex[i] = vertex4;
            } else if (i % 8 == 7) {
                SideVertex[i] = new Vector3(vertex4.x, vertex4.y, vertex4.z + variables.poleHeight);
            } else {
                Debug.LogWarning("Calcration Error");
            }

        }

        //LineRenderer用
        for (int i = 0; i < 4; i++) {
            LineVertex[DivisionNum * 4 + i] = SideVertex[i];
        }
        if (DivisionNum == variables.trapezoidDivisionNum) {
            LineVertex[DivisionNum * 4 + 0 + 4] = SideVertex[6];
            LineVertex[DivisionNum * 4 + 1 + 4] = SideVertex[7];
            LineVertex[DivisionNum * 4 + 2 + 4] = SideVertex[4];
            LineVertex[DivisionNum * 4 + 3 + 4] = SideVertex[5];
        }

        //端面用
        if (DivisionNum == 0) {
            //最初の端面
            StartVertex[0] = vertex1;
            StartVertex[1] = new Vector3(vertex1.x, vertex1.y, vertex1.z + variables.poleHeight);
            StartVertex[2] = vertex3;
            StartVertex[3] = new Vector3(vertex3.x, vertex3.y, vertex3.z + variables.poleHeight);
        } else if (DivisionNum == variables.trapezoidDivisionNum) {
            //最後の端面
            EndVertex[0] = new Vector3(vertex2.x, vertex2.y, vertex2.z + variables.poleHeight);
            EndVertex[1] = vertex2;
            EndVertex[2] = new Vector3(vertex4.x, vertex4.y, vertex4.z + variables.poleHeight);
            EndVertex[3] = vertex4;
        }

        createSorce.callBackVertex(new Vector3[4] { SideVertex[0], SideVertex[6], SideVertex[1], SideVertex[7] }, ( int.Parse(gameObject.name) - 1 ) * ( variables.trapezoidDivisionNum + 1 ) + DivisionNum);

        //Textの座標を計算する
        if (DivisionNum == ( variables.trapezoidDivisionNum + 1 ) / 2) {
            if (variables.trapezoidDivisionNum % 2 != 0) {
                //分割数が奇数回の時は
                textPosition = ( SideVertex[0] + SideVertex[2] ) / 2f;
            } else {
                textPosition = ( SideVertex[0] + SideVertex[2] + SideVertex[4] + SideVertex[6] ) / 4f;
            }
        }
    }

    //LineVertexからlineVerteciesに整列しなおす（長いので別メソッド）
    private void SetLineVertecies() {
        //見づらかったからすこしずらした
        Vector3 zPlus = Vector3.forward * variables.lineShiftSlightly;
        Vector3 zMinus = Vector3.back * variables.lineShiftSlightly;

        lineVertecies[0] = LineVertex[0] + zMinus;
        lineVertecies[1] = LineVertex[1] + zPlus;
        lineVertecies[2 + variables.trapezoidDivisionNum] = LineVertex[4 * ( variables.trapezoidDivisionNum ) + 5] + zPlus;
        lineVertecies[3 + variables.trapezoidDivisionNum] = LineVertex[4 * ( variables.trapezoidDivisionNum ) + 4] + zMinus;
        lineVertecies[4 + 2 * ( variables.trapezoidDivisionNum )] = LineVertex[0] + zMinus;
        lineVertecies[5 + 2 * ( variables.trapezoidDivisionNum )] = LineVertex[2] + zMinus;
        lineVertecies[6 + 2 * ( variables.trapezoidDivisionNum )] = LineVertex[3] + zPlus;
        lineVertecies[7 + 2 * ( variables.trapezoidDivisionNum )] = LineVertex[1] + zPlus;
        lineVertecies[8 + 2 * ( variables.trapezoidDivisionNum )] = LineVertex[3] + zPlus;
        lineVertecies[9 + 3 * ( variables.trapezoidDivisionNum )] = LineVertex[4 * ( variables.trapezoidDivisionNum ) + 7] + zPlus;
        lineVertecies[10 + 3 * ( variables.trapezoidDivisionNum )] = LineVertex[4 * ( variables.trapezoidDivisionNum ) + 5] + zPlus;
        lineVertecies[11 + 3 * ( variables.trapezoidDivisionNum )] = LineVertex[4 * ( variables.trapezoidDivisionNum ) + 7] + zPlus;
        lineVertecies[12 + 3 * ( variables.trapezoidDivisionNum )] = LineVertex[4 * ( variables.trapezoidDivisionNum ) + 6] + zMinus;
        lineVertecies[13 + 3 * ( variables.trapezoidDivisionNum )] = LineVertex[4 * ( variables.trapezoidDivisionNum ) + 4] + zMinus;
        lineVertecies[14 + 3 * ( variables.trapezoidDivisionNum )] = LineVertex[4 * ( variables.trapezoidDivisionNum ) + 6] + zMinus;
        lineVertecies[15 + 4 * ( variables.trapezoidDivisionNum )] = LineVertex[2] + zMinus;
        for (int i = 1; i <= variables.trapezoidDivisionNum; i++) {
            lineVertecies[1 + i] = LineVertex[4 * i + 1] + zPlus;
            lineVertecies[3 + variables.trapezoidDivisionNum + i] = LineVertex[4 * ( variables.trapezoidDivisionNum - i ) + 4] + zMinus;
            lineVertecies[8 + 2 * ( variables.trapezoidDivisionNum ) + i] = LineVertex[4 * i + 3] + zPlus;
            lineVertecies[14 + 3 * ( variables.trapezoidDivisionNum ) + i] = LineVertex[4 * ( variables.trapezoidDivisionNum - i ) + 6] + zMinus;
        }

    }

    private void OnMouseEnterOwnMade() {
        OnTriggerEnterOwnMade(null);
        /*if (isActiveObj && meshRenderer.material != variables.material_TrapezoidPole_Touch) {
            if (isSubRingPole)
                systemScript.UpdateChuringNum(int.Parse(gameObject.name) + 10);
            else
                systemScript.UpdateChuringNum(int.Parse(gameObject.name));
            meshRenderer.material = variables.material_TrapezoidPole_Touch;
        }*/
    }

    private void OnMouseExitOwnMade() {
        OnTriggerExitOwnMade(null);
        /*if (isActiveObj) {
            meshRenderer.material = variables.material_TrapezoidPole_Normal;
        }*/
    }

    public void OnTriggerEnter(Collider other) {
        /* if (other.name.Substring(2) == "index_endPointer") {
             if (!doneEnter) {
                 doneEnter = true;
                 enterObject = other.gameObject;
                 OnTriggerEnterOwnMade(enterObject);
             } else {
                 flgEnter = true;
             }
         }*/
        //OnTriggerEnterOwnMade(other.gameObject);
    }

    public void OnTriggerExit(Collider other) {
        /*if (other.name.Substring(2) == "index_endPointer") {
            flgExit = true;
        }*/
        //OnTriggerExitOwnMade(other.gameObject);
    }

    /* 以下2つはもともとトリガーイベントだったが、
     * Convexを使えない問題が発生したため、
     * 独自メソッドとして再開発
     */
    public void OnTriggerEnterOwnMade(GameObject other) {
        //Debug.Log("runrun"+ other.name.Substring(2));
        if (isActiveObj && meshRenderer.material.color != variables.material_TrapezoidPole_Touch.color) {
            if (( other == null ) || ( other != null && other.name.Substring(2) == "index_endPointer" )) {
                if (variables.isCircleSystem && isSubRingPole) {
                    //副輪のときは+100した名前を送る
                    systemScript.UpdateChuringNum(int.Parse(gameObject.name) + 100);
                    //Debug.Log("i am " + ( int.Parse(gameObject.name) + 100 ).ToString());
                } else {
                    // if (Physics.OverlapSphere(other.transform.position, 0.01f).Any(col => col == GetComponent<Collider>()))
                    systemScript.UpdateChuringNum(int.Parse(gameObject.name));
                    //Debug.Log("i am " + ( int.Parse(gameObject.name) ).ToString());
                    //Debug.Log("ok " + other.transform.position);
                }
                meshRenderer.material = variables.material_TrapezoidPole_Touch;
            }
        }
    }

    public void OnTriggerExitOwnMade(GameObject other) {
        if (isActiveObj && meshRenderer.material.color != variables.material_TrapezoidPole_Normal.color) {
            if (( other == null ) || ( other != null && other.name.Substring(2) == "index_endPointer" )) {
                if (variables.isCircleSystem && isSubRingPole) {
                    //副輪のときは+100した名前を送る
                    systemScript.UpdateChuringNum(int.Parse(gameObject.name) + 100 + 1000);
                    //Debug.Log("i am " + ( int.Parse(gameObject.name) + 100 + 1000 ).ToString());
                } else {
                    //if (!( Physics.OverlapSphere(other.transform.position, 0.01f).Any(col => col == GetComponent<Collider>()) ))
                    systemScript.UpdateChuringNum(int.Parse(gameObject.name) + 1000);
                    //Debug.Log("i am " + ( int.Parse(gameObject.name) + 1000 ).ToString());
                    //Debug.Log("ng "+other.transform.position);
                }
                meshRenderer.material = variables.material_TrapezoidPole_Normal;
            }
        }
    }
    /* 独自メソッド　終 */

    private void make3Dtext() {
        //中心のUI表示
        GameObject textCentor = new GameObject("text");
        //接触判定のないレイヤーに変更
        textCentor.layer = 8;
        textMeshRender = textCentor.AddComponent<MeshRenderer>();
        TmeshC = textCentor.AddComponent<TextMesh>();
        //文字サイズ
        TmeshC.fontSize = variables.systemTextFontSize;
        //文字色
        TmeshC.color = variables.material_Text.color;
        //アンカー位置を中心に
        TmeshC.anchor = TextAnchor.MiddleCenter;
        //真ん中寄せ
        TmeshC.alignment = TextAlignment.Center;
        //表示文字(更新されればError表示は消えるはず)
        TmeshC.text = "Error";
        //位置調整（台形の中心に設定）
        textCentor.transform.position = textPosition;
        //大きさ
        float fontScale = 1 / 1000f;
        //textCentor.transform.localScale = new Vector3(fontScale, fontScale, fontScale);
        textCentor.transform.localScale = Vector3.one * fontScale;
        //子オブジェクトに設定
        textCentor.transform.parent = this.transform;
    }

    public void setMyParent(GameObject parent) {
        myParent = parent;
    }

    //非アクティブ化用
    public void Enable(bool enable) {
        // Debug.Log("run");
        meshCollider.enabled = enable;
        meshRenderer.enabled = enable;
        lineRenderer.enabled = enable;
        textMeshRender.enabled = enable;
    }

    //自分の数字をシステムに問い合わせる
    private void askMyText() {
        int myNum;// = int.Parse(transform.gameObject.name);
        if (isSubRingPole) {
            string myParentNum = myParent.name.Substring(9);
            myNum = int.Parse(transform.gameObject.name) + int.Parse(myParentNum) * 100;
        } else {
            myNum = int.Parse(transform.gameObject.name);
        }
        //Debug.Log(myNum);
        MyText = systemScript.tellKeyText(myNum);
    }

    //各台形の法線を計算する
    private void culcNormal(Vector3[] meshVec) {
        //参考
        //https://docs.unity3d.com/ja/2018.4/Manual/ComputingNormalPerpendicularVector.html
        Vector3 a, b, c;
        for (int i = 0; i < variables.trapezoidDivisionNum + 1; i++) {
            for (int j = 0; j < 6; j++) {
                a = meshVec[i * 16 + normalNumber[j * 3 + 0] + 4];
                b = meshVec[i * 16 + normalNumber[j * 3 + 1] + 4];
                c = meshVec[i * 16 + normalNumber[j * 3 + 2] + 4];
                Vector3 side1 = b - a;
                Vector3 side2 = c - a;
                normalVector[i, j] = Vector3.Cross(side1, side2);
                normalVector[i, j] /= normalVector[i, j].magnitude;
                normalBasicVec[i, j] = a;
            }
        }
    }

    //内積を用いて当たり判定を行う
    private bool fireInnerProductCollider() {
        bool col = false;
        for (int i = 0; i < variables.fingers.Length; i++) {
            for (int j = 0; j < variables.trapezoidDivisionNum + 1; j++) {
                for (int k = 0; k < 6; k++) {
                    if (variables.isLeftHandLastTouch) {
                        if (i == 1)
                            break;
                    } else {
                        if (i == 0)
                            break;
                    }
                    if (Vector3.Dot(normalVector[j, k], variables.fingers[i].transform.position - ( normalBasicVec[j, k] + transform.position )) > 0) {
                        //内側を向いている
                        col = true;
                    } else {
                        col = false;
                        k = 100;
                    }
                }
                if (col) {
                    return col;
                } else {
                }
            }
        }
        return col;
    }
}
