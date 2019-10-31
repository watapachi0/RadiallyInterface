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

public class PolygonalPillarC : MonoBehaviour {

    private createTrapezoidPoleC createSorce;
    private centralSystemC systemScript;
    private GameObject myParent = null;

    //副輪の中心である
    public bool isSubRingPillar { get; set; } = false;

    //面情報
    int[] face;

    //
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;

    private void Awake() {
        systemScript = GameObject.Find("central").GetComponent<centralSystemC>();
    }

    void Start() {

        if (myParent == null)
            createSorce = GameObject.Find("central").GetComponent<createTrapezoidPoleC>();
        else
            createSorce = myParent.GetComponent<createTrapezoidPoleC>();

        //メッシュ作成
        Mesh mesh = new Mesh();
        //メッシュリセット
        mesh.Clear();
        //メッシュへの頂点情報の追加
        mesh.vertices = variablesC.polygonalPillarVertex;
        //メッシュへの面情報の追加
        SetFace();
        mesh.triangles = face;

        //メッシュフィルター追加
        MeshFilter mesh_filter = new MeshFilter();
        mesh_filter = this.gameObject.AddComponent<MeshFilter>();
        //メッシュアタッチ
        mesh_filter.mesh = mesh;
        //レンダラー追加 + マテリアルアタッチ
        meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = variablesC.material_PolygonalPillar;
        //コライダーアタッチ
        meshCollider = this.gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
        meshCollider.convex = true;
        meshCollider.isTrigger = true;

        //NormalMapの再計算
        mesh_filter.mesh.RecalculateNormals();

        if (!variablesC.isOnXR) {
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

        //クリエイト元を親にする
        transform.parent = createSorce.gameObject.transform;
        //親の余計なスクリプトの削除フラグを立てる
        createSorce.IsReadyToDestroy(true);

    }

    //何個のオブジェクト中の何番目のオブジェクトか
    private void SetFace() {
        //底面それぞれ3頂点+(ポリゴン二枚なので)側面6頂点 * 角数 * 分割数
        face = new int[12 * variablesC.poleSum * ( variablesC.trapezoidDivisionNum + 1 )];
        for (int i = 0; i < variablesC.poleSum * ( variablesC.trapezoidDivisionNum + 1 ); i++) {
            //以下で、poleSum分割の三角柱ができる
            face[i * 12 + 0] = 8 * i + 2;  //側面1
            face[i * 12 + 1] = 8 * i + 1;  //
            face[i * 12 + 2] = 8 * i + 0;  //
            face[i * 12 + 3] = 8 * i + 1;  //側面2
            face[i * 12 + 4] = 8 * i + 2;  //
            face[i * 12 + 5] = 8 * i + 3;  //
            face[i * 12 + 6] = 8 * i + 4;  //底面1
            face[i * 12 + 7] = 8 * i + 5;  //
            face[i * 12 + 8] = 8 * variablesC.poleSum * ( variablesC.trapezoidDivisionNum + 1 ) + i + 1 - 1;   //底面1の角部分
            face[i * 12 + 9] = 8 * i + 7;   //底面2
            face[i * 12 + 10] = 8 * i + 6;  //
            face[i * 12 + 11] = 9 * variablesC.poleSum * ( variablesC.trapezoidDivisionNum + 1 ) + i + 1 - 1;  //底面2の角部分
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
                Debug.Log("i am " + ( int.Parse(gameObject.name) + 100 ).ToString());
            } else {
                systemScript.UpdateChuringNum(int.Parse(gameObject.name));
                Debug.Log("i am " + ( int.Parse(gameObject.name) ).ToString());
            }
        }
    }

    public void OnTriggerExitOwnMade(GameObject other) {
        if (( other == null ) || ( other != null && other.name.Substring(2) == "index_endPointer" )) {
            if (isSubRingPillar) {
                systemScript.UpdateChuringNum(int.Parse(gameObject.name) + 100 + 1000);
                Debug.Log("i am " + ( int.Parse(gameObject.name) + 100 + 1000 ).ToString());
            } else {
                systemScript.UpdateChuringNum(int.Parse(gameObject.name) + 1000);
                Debug.Log("i am " + ( int.Parse(gameObject.name) + 1000 ).ToString());
            }
        }
    }
    /* 独自メソッド　終 */

    public void setMyParent(GameObject parent) {
        myParent = parent;
    }

    public void Enable(bool enable) {
        meshRenderer.enabled = enable;
        meshCollider.enabled = enable;
    }
}
