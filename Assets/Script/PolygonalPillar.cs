using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/*
 * 
 * オブジェクトの描画と各種コンポーネントのアタッチのみ行う
 * 多角柱
 * 
 * 
 */

public class PolygonalPillar : MonoBehaviour {

    private createTrapezoidPole createSorce;
    private centralSystem systemScript;
    private GameObject myParent = null;
    private int poleSum;
    private int myNum;

    //何回も入力が走る対策
    private bool doneEnter = false;
    private bool flgEnter = false;
    private bool flgExit = false;
    private GameObject enterObject = null;
    private bool inCol = false;

    //副輪の中心である
    public bool isSubRingPillar { get; set; } = false;

    //面情報
    int[] face;

    //
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;
    //文字のレンダラー
    MeshRenderer textMeshRender;

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

    //Textの位置
    private Vector3 textPosition;

    //Text情報
    [SerializeField]
    public string MyText { get; set; } = "";

    private void Awake() {
        systemScript = GameObject.Find("central").GetComponent<centralSystem>();
    }

    void Start() {

        //法線初期化
        normalVector = new Vector3[variables.poleSum*(variables.trapezoidDivisionNum + 1), 1];
        normalBasicVec = new Vector3[variables.poleSum * ( variables.trapezoidDivisionNum + 1 ), 1];

        if (myParent == null || !variables.isCircleSystem) {
            createSorce = GameObject.Find("central").GetComponent<createTrapezoidPole>();
            //現在のTextSetの段数を取得
            poleSum = variables.poleSum;
        } else {
            createSorce = myParent.GetComponent<createTrapezoidPole>();
            //自分の親の名前から該当TextSetの行からアイテム数を取得
            //Debug.Log(myParent.name);
            myNum = int.Parse(myParent.name.Substring(9));
            poleSum = systemScript.GetTextSetItemNum(myNum);
        }

        //メッシュ作成
        Mesh mesh = new Mesh();
        //メッシュリセット
        mesh.Clear();
        //メッシュへの頂点情報の追加
        int PPVindex;
        if (myParent == null) {
            PPVindex = 0;
        } else {
            PPVindex = myNum;
        }
        mesh.vertices = variables.polygonalPillarVertex[PPVindex];
        //テキストの位置
        int sum;
        for (sum = 0, textPosition = Vector3.zero; sum < poleSum * ( variables.trapezoidDivisionNum + 1 ); sum++) {
            if (myParent == null || !variables.isCircleSystem) {
                textPosition += variables.polygonalPillarVertex[0][sum * 8];
            } else {
                textPosition += variables.polygonalPillarVertex[myNum][sum * 8];
            }
        }
        textPosition /= sum;
        //メッシュへの面情報の追加
        SetFace();
        //Debug.Log("mesh : "+mesh.vertices.Length+" face : "+face[face.Length-1]+" poly : "+ variables.polygonalPillarVertex.GetLength(0));
        mesh.triangles = face;

        //メッシュフィルター追加
        MeshFilter mesh_filter = new MeshFilter();
        mesh_filter = this.gameObject.AddComponent<MeshFilter>();
        //メッシュアタッチ
        mesh_filter.mesh = mesh;
        //レンダラー追加 + マテリアルアタッチ
        meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = variables.material_PolygonalPillar;
        //コライダーアタッチ
        meshCollider = this.gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
        meshCollider.convex = true;
        meshCollider.isTrigger = true;

        //NormalMapの再計算
        mesh_filter.mesh.RecalculateNormals();

        if (!variables.isOnXR) {
            //当たり判定用 Event Trigger
            //イベントトリガーのアタッチと初期化
            EventTrigger currentTrigger = this.gameObject.AddComponent<EventTrigger>();
            currentTrigger.triggers = new List<EventTrigger.Entry>();

            //イベントトリガーのトリガーイベント作成
            EventTrigger.Entry entry1 = new EventTrigger.Entry();
            entry1.eventID = EventTriggerType.PointerEnter;
            entry1.callback.AddListener((x) => OnMouseEnterOwnMade());  //ラムダ式の右側は追加するメソッド
            currentTrigger.triggers.Add(entry1);
            //イベントトリガーのトリガーイベント作成
            EventTrigger.Entry entry2 = new EventTrigger.Entry();
            entry2.eventID = EventTriggerType.PointerExit;
            entry2.callback.AddListener((x) => OnMouseExitOwnMade());
            currentTrigger.triggers.Add(entry2);
        }

        //テキスト表示
        make3Dtext();

        //クリエイト元を親にする
        transform.parent = createSorce.gameObject.transform;
        //親の余計なスクリプトの削除フラグを立てる
        createSorce.IsReadyToDestroy(true);
    }

    private void Update() {

        //テキストの更新
        TmeshC.text = MyText;

        if (!variables.isCircleSystem)
            inCol = fireInnerProductCollider();
        else if (meshRenderer.enabled)
            inCol = fireInnerProductCollider();

        if (!doneEnter && inCol) {
            doneEnter = true;
            OnTriggerEnterOwnMade(null);
        } else if (doneEnter && !inCol) {
            doneEnter = false;
            OnTriggerExitOwnMade(null);
        }
    }

    //何個のオブジェクト中の何番目のオブジェクトか
    private void SetFace() {
        //底面それぞれ3頂点+(ポリゴン二枚なので)側面6頂点 * 角数 * 分割数
        face = new int[12 * poleSum * ( variables.trapezoidDivisionNum + 1 )];
        for (int i = 0; i < poleSum * ( variables.trapezoidDivisionNum + 1 ); i++) {
            //以下で、poleSum分割の三角柱ができる
            face[i * 12 + 0] = 8 * i + 2;  //側面1
            face[i * 12 + 1] = 8 * i + 1;  //
            face[i * 12 + 2] = 8 * i + 0;  //
            face[i * 12 + 3] = 8 * i + 1;  //側面2
            face[i * 12 + 4] = 8 * i + 2;  //
            face[i * 12 + 5] = 8 * i + 3;  //
            face[i * 12 + 6] = 8 * i + 4;  //底面1
            face[i * 12 + 7] = 8 * i + 5;  //
            face[i * 12 + 8] = 8 * poleSum * ( variables.trapezoidDivisionNum + 1 ) + i + 1 - 1;   //底面1の角部分
            face[i * 12 + 9] = 8 * i + 7;   //底面2
            face[i * 12 + 10] = 8 * i + 6;  //
            face[i * 12 + 11] = 9 * poleSum * ( variables.trapezoidDivisionNum + 1 ) + i + 1 - 1;  //底面2の角部分
        }
    }

    private void OnMouseEnterOwnMade() {
        OnTriggerEnterOwnMade(null);
    }

    private void OnMouseExitOwnMade() {
        OnTriggerExitOwnMade(null);
    }

    public void OnTriggerEnter(Collider other) {
        OnTriggerEnterOwnMade(other.gameObject);
    }

    public void OnTriggerExit(Collider other) {
        OnTriggerExitOwnMade(other.gameObject);
    }

    /* 以下はもともとトリガーイベントだったが、
     * Convexを使えない問題が発生したため、
     * 独自メソッドとして再開発
     */
    public void OnTriggerEnterOwnMade(GameObject other) {
        if (( other == null ) || ( other != null && other.name.Substring(2) == "index_endPointer" )) {
            if (isSubRingPillar) {
                systemScript.UpdateChuringNum(int.Parse(gameObject.name) + 100);
                //Debug.Log("i am " + ( int.Parse(gameObject.name) + 100 ).ToString());
            } else {
                systemScript.UpdateChuringNum(int.Parse(gameObject.name));
                //Debug.Log("i am " + ( int.Parse(gameObject.name) ).ToString());
            }
        }
    }

    public void OnTriggerExitOwnMade(GameObject other) {
        if (( other == null ) || ( other != null && other.name.Substring(2) == "index_endPointer" )) {
            if (isSubRingPillar) {
                systemScript.UpdateChuringNum(int.Parse(gameObject.name) + 100 + 1000);
                //Debug.Log("i am " + ( int.Parse(gameObject.name) + 100 + 1000 ).ToString());
            } else {
                systemScript.UpdateChuringNum(int.Parse(gameObject.name) + 1000);
                //Debug.Log("i am " + ( int.Parse(gameObject.name) + 1000 ).ToString());
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

    public void Enable(bool enable) {
        meshRenderer.enabled = enable;
        meshCollider.enabled = enable;
        textMeshRender.enabled = enable;
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
            for (int j = 0; j < (variables.trapezoidDivisionNum + 1)*variables.poleSum; j++) {
                for (int k = 0; k < 1; k++) {
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
