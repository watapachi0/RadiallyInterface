using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

public class MultipleTrapezoidPole1 : MonoBehaviour {

    //何回も入力が走る対策
    private bool doneEnter = false;
    private bool flgEnter = false;
    private bool flgExit = false;
    private GameObject enterObject = null;
    private bool inCol = false;

    //頂点座標
    Vector3[] EndVertex = new Vector3[4];
    Vector3[] SideVertex = new Vector3[24];
    /// <summary>
    /// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>
    Vector3[] StartVertex = new Vector3[4];
    Vector3[] normalBasicVec;

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

    //文字のゲームオブジェクト
    public TextMesh TmeshC;

    //Textの位置
    private Vector3 textPosition;

    public GameObject sp;

    //Text情報
    [SerializeField]
    public string MyText { get; set; } = "";

    void Start() {

        //LineVertex初期化  最初の面4頂点+中間面4頂点+最後の面4頂点
        LineVertex = new Vector3[4 + 0 + 4];
        //格納先も初期化
        lineVertecies = new Vector3[8 + 0 + 8];

        // CombineMeshes()する時に使う配列   始端と終端も含めるので+3
        CombineInstance[] combineInstanceAry = new CombineInstance[3];

        //メッシュ作成
        //頂点計算
        CalcVertices();

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
        /// <summary>
        /// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// </summary>
        normalVector = new Vector3[1, 6];
        normalBasicVec = new Vector3[6];

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

    }

    void Update() {

        /// <summary>
        /// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// </summary>
        inCol = fireInnerProductCollider();
        if (!doneEnter && inCol) {
            doneEnter = true;
            OnTriggerEnterOwnMade(null);
        } else if (doneEnter && !inCol) {
            doneEnter = false;
            OnTriggerExitOwnMade(null);
        }
    }

    private void CalcVertices() {
        //台形の外側左の頂点座標その1
        Vector3 vertex1 = new Vector3(0, 1, 0);
        //台形の外側右の頂点座標その2 
        Vector3 vertex2 = new Vector3(1, 1, 0);
        //台形の内側左の頂点座標その1
        Vector3 vertex3 = new Vector3(0, 0, 0);
        //台形の内側右の頂点座標その2
        Vector3 vertex4 = new Vector3(1, 0, 0);
        //全頂点数8にそれぞれ座標が2つずつある
        for (int i = 0; i < 8 * 2; i++) {
            if (i % 8 == 0) {
                SideVertex[i] = vertex3;
            } else if (i % 8 == 1) {
                SideVertex[i] = new Vector3(vertex3.x, vertex3.y, vertex3.z + 1);
            } else if (i % 8 == 2) {
                SideVertex[i] = vertex1;
            } else if (i % 8 == 3) {
                SideVertex[i] = new Vector3(vertex1.x, vertex1.y, vertex1.z + 1);
            } else if (i % 8 == 4) {
                SideVertex[i] = vertex2;
            } else if (i % 8 == 5) {
                SideVertex[i] = new Vector3(vertex2.x, vertex2.y, vertex2.z + 1);
            } else if (i % 8 == 6) {
                SideVertex[i] = vertex4;
            } else if (i % 8 == 7) {
                SideVertex[i] = new Vector3(vertex4.x, vertex4.y, vertex4.z + 1);
            } else {
                Debug.LogWarning("Calcration Error");
            }

        }

        //端面用
        //最初の端面
        StartVertex[0] = vertex1;
        StartVertex[1] = new Vector3(vertex1.x, vertex1.y, vertex1.z + 1);
        StartVertex[2] = vertex3;
        StartVertex[3] = new Vector3(vertex3.x, vertex3.y, vertex3.z + 1);
        //最後の端面
        EndVertex[0] = new Vector3(vertex2.x, vertex2.y, vertex2.z + 1);
        EndVertex[1] = vertex2;
        EndVertex[2] = new Vector3(vertex4.x, vertex4.y, vertex4.z + 1);
        EndVertex[3] = vertex4;
    }

    public void OnTriggerEnterOwnMade(GameObject other) {
        Debug.Log("enter");
    }

    public void OnTriggerExitOwnMade(GameObject other) {
        Debug.Log("exit");
    }


    /// <summary>
    /// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>
    //各台形の法線を計算する
    private void culcNormal(Vector3[] meshVec) {
        //参考
        //https://docs.unity3d.com/ja/2018.4/Manual/ComputingNormalPerpendicularVector.html
        Vector3 a, b, c;
        for (int i = 0; i < 0 + 1; i++) {
            for (int j = 0; j < 6; j++) {
                a = meshVec[normalNumber[j * 3 + 0] + 4] + transform.position;
                b = meshVec[normalNumber[j * 3 + 1] + 4] + transform.position;
                c = meshVec[normalNumber[j * 3 + 2] + 4] + transform.position;
                Vector3 side1 = b - a;
                Vector3 side2 = c - a;
                normalVector[i, j] = Vector3.Cross(side1, side2);
                normalVector[i, j] = normalVector[i, j] / normalVector[i, j].magnitude;
                normalBasicVec[j] = a;
            }
        }
    }


    /// <summary>
    /// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>
    //内積を用いて当たり判定を行う
    private bool fireInnerProductCollider() {
        bool col = false;
        for (int j = 0; j < 0 + 1; j++) {
            for (int k = 0; k < 6; k++) {
                if (Vector3.Dot(normalVector[j, k], sp.transform.position - normalBasicVec[k]) > 0) {
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
        return col;
    }
}
