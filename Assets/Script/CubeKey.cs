using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CubeKey : MonoBehaviour {

    private centralSystem systemScript;

    //何回も入力が走る対策
    private bool doneEnter = false;
    private bool inCol = false;

    //頂点座標
    Vector3[] StartVertex = new Vector3[4];
    Vector3[] EndVertex = new Vector3[4];
    Vector3[] SideVertex = new Vector3[16];
    //全頂点
    Vector3[] AllVertex;
    //合成後のメッシュから座標の重なる頂点をまとめたもの
    int?[,] AllVertexKey;
    //色の変更など
    MeshRenderer meshRenderer;
    //文字のレンダラー
    MeshRenderer textMeshRender;
    //メッシュフィルター
    MeshFilter mesh_filter;

    //面情報
    int[] EndFace = new int[6]   { 1,      0,      3,
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
    Vector3[] normalVector;
    //基準の法線群
    Vector3[] normalBasicVec;
    //侵入した指の番号
    int? fingerNum = null;

    //文字のゲームオブジェクト
    public TextMesh TmeshC;

    CombineInstance[] combineInstanceAry;

    //Textの位置
    private Vector3 textPosition;

    //初期化後に行く座標
    public Vector3 myPosition;

    //Text情報
    [SerializeField]
    public string MyText { get; set; } = "";

    void Start() {

        //法線初期化
        normalVector = new Vector3[6];
        normalBasicVec = new Vector3[6];

        systemScript = GameObject.Find("central").GetComponent<centralSystem>();

        // CombineMeshes()する時に使う配列   始端と終端も含めるので+3
        CombineInstance[] combineInstanceAry = new CombineInstance[3];

        //メッシュ作成
        //頂点計算
        CalcVertices();

        //最初の一枚
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

        //真ん中
        Mesh mesh = new Mesh();
        //メッシュリセット
        mesh.Clear();
        //メッシュへの頂点情報の追加
        mesh.vertices = SideVertex;
        //メッシュへの面情報の追加
        mesh.triangles = SideFace;
        //合成するMesh（同じMeshを円形に並べたMesh）
        combineInstanceAry[1].mesh = mesh;
        combineInstanceAry[1].transform = Matrix4x4.Translate(transform.position);

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
        combineInstanceAry[2].mesh = meshLast;
        combineInstanceAry[2].transform = Matrix4x4.Translate(transform.position);

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

        //クリエイト元を親にする
        transform.parent = systemScript.gameObject.transform;
        transform.position = myPosition + transform.parent.position;
    }

    void Update() {
        //RadiallyはcentralSystemのコルーチン内で初期化される
        if (MyText == "") {
            askMyText();
            if (MyText == "") {
                gameObject.SetActive(false);
            }
        }

        //テキストの更新
        TmeshC.text = MyText;

        if (meshRenderer.enabled)
            inCol = fireInnerProductCollider();

        if (!doneEnter && inCol) {
            doneEnter = true;
            OnTriggerEnterOwnMade(null);
        } else if (doneEnter && !inCol) {
            doneEnter = false;
            OnTriggerExitOwnMade(null);
        }
    }

    //インスペクターからの変更時に再計算
    private void OnValidate() {
    }

    //直方体の素の頂点計算
    private void CalcVertices() {

        //上側手前左の頂点座標
        Vector3 vertex1 = new Vector3(0, variables.cubeVertical, 0);
        //上側手前右の頂点座標
        Vector3 vertex2 = new Vector3(variables.cubeWidth, variables.cubeVertical, 0);
        //下側手前左の頂点座標
        Vector3 vertex3 = new Vector3(0, 0, 0);
        //下側手前右の頂点座標
        Vector3 vertex4 = new Vector3(variables.cubeWidth, 0, 0);
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

        //端面用
        //最初の端面
        StartVertex[0] = vertex1;
        StartVertex[1] = new Vector3(vertex1.x, vertex1.y, vertex1.z + variables.poleHeight);
        StartVertex[2] = vertex3;
        StartVertex[3] = new Vector3(vertex3.x, vertex3.y, vertex3.z + variables.poleHeight);
        //最後の端面
        EndVertex[0] = new Vector3(vertex2.x, vertex2.y, vertex2.z + variables.poleHeight);
        EndVertex[1] = vertex2;
        EndVertex[2] = new Vector3(vertex4.x, vertex4.y, vertex4.z + variables.poleHeight);
        EndVertex[3] = vertex4;

        //Textの座標を計算する
        textPosition = ( SideVertex[0] + SideVertex[2] + SideVertex[4] + SideVertex[6] ) / 4f;

    }

    private void OnMouseEnterOwnMade() {
        OnTriggerEnterOwnMade(null);
    }

    private void OnMouseExitOwnMade() {
        OnTriggerExitOwnMade(null);
    }

    public void OnTriggerEnterOwnMade(GameObject other) {
        if (meshRenderer.material.color != variables.material_TrapezoidPole_Touch.color) {
            if (( other == null ) || ( other != null && other.name.Substring(2) == "index_endPointer" )) {
                systemScript.UpdateChuringNum(int.Parse(gameObject.name));
                Debug.Log("i am " + ( int.Parse(gameObject.name) ).ToString());
                meshRenderer.material = variables.material_TrapezoidPole_Touch;
            }
        }
    }

    public void OnTriggerExitOwnMade(GameObject other) {
        if (meshRenderer.material.color != variables.material_TrapezoidPole_Normal.color) {
            if (( other == null ) || ( other != null && other.name.Substring(2) == "index_endPointer" )) {
                if (wasOutFromFront()) {
                    systemScript.UpdateChuringNum(int.Parse(gameObject.name) + 2000);
                    Debug.Log("i am " + ( int.Parse(gameObject.name) + 2000 ).ToString());
                    meshRenderer.material = variables.material_TrapezoidPole_Normal;
                } else {
                    systemScript.UpdateChuringNum(int.Parse(gameObject.name) + 1000);
                    Debug.Log("i am " + ( int.Parse(gameObject.name) + 1000 ).ToString());
                    meshRenderer.material = variables.material_TrapezoidPole_Normal;
                }
            }
        }
    }

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

    //自分の数字をシステムに問い合わせる
    private void askMyText() {
        int myNum;// = int.Parse(transform.gameObject.name);
        myNum = int.Parse(transform.gameObject.name);
        //Debug.Log(myNum);
        MyText = systemScript.tellKeyText(myNum);
    }

    //各台形の法線を計算する
    private void culcNormal(Vector3[] meshVec) {
        //参考
        //https://docs.unity3d.com/ja/2018.4/Manual/ComputingNormalPerpendicularVector.html
        Vector3 a, b, c;
        for (int j = 0; j < 6; j++) {
            a = meshVec[normalNumber[j * 3 + 0] + 4];
            b = meshVec[normalNumber[j * 3 + 1] + 4];
            c = meshVec[normalNumber[j * 3 + 2] + 4];
            Vector3 side1 = b - a;
            Vector3 side2 = c - a;
            normalVector[j] = Vector3.Cross(side1, side2);
            normalVector[j] /= normalVector[j].magnitude;
            normalBasicVec[j] = a;
        }
    }

    //内積を用いて当たり判定を行う
    private bool fireInnerProductCollider() {
        bool col = false;
        for (int i = 0; i < variables.fingers.Length; i++) {
            for (int k = 0; k < 6; k++) {
                if (variables.isLeftHandLastTouch) {
                    if (i == 1)
                        break;
                } else {
                    if (i == 0)
                        break;
                }
                if (Vector3.Dot(normalVector[k], variables.fingers[i].transform.position - ( normalBasicVec[k] + transform.position )) > 0) {
                    //内側を向いている
                    col = true;
                    fingerNum = i;
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
        return col;
    }

    //指が手前から出たかどうかを判定する
    private bool wasOutFromFront() {
        bool col = false;
        if (Vector3.Dot(normalVector[3], variables.fingers[(int)fingerNum].transform.position - ( normalBasicVec[3] + transform.position )) < 0) {
            //内向き法線との内積が負なので手前から出た
            col = true;
        }
        fingerNum = null;
        return col;
    }
}

