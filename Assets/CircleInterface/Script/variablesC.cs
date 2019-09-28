using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class variablesC : MonoBehaviour {

    //現在の文字種
    //public static int CharacterType { get; set; }
    /* int  type
     * -1   デバッグ用
     * 0    標準。表示なし
     * 1    ひらがな
     * 2    アルファベット
     * 3    数字記号
     */

    //多角形部分の半径・円環の内側の半径
    public static float radiusIn { get; set; }

    //円環の外側の半径
    public static float radiusOut { get; set; }

    //システムの厚さ
    public static float poleHeight { get; set; }

    //システムのキーの数（中心とシステムキー除く）
    public static int poleSum { get; set; }

    //多角形作成用頂点群
    public static Vector3[] polygonalPillarVertex { get; set; }

    //多角形のマテリアル
    public static Material material_PolygonalPillar { get; set; }

    //台形用通常マテリアル
    public static Material material_TrapezoidPole_Normal { get; set; }

    //台形用接触時マテリアル
    public static Material material_TrapezoidPole_Touch { get; set; }

    //台形の強調ライン用マテリアル
    public static Material material_LineRenderer { get; set; }

    //台形部分の分割数
    public static int trapezoidDivisionNum { get; set; }

    //システムアイコンの数
    public static int systemCommandNum { get; set; }

    //そのシステムアイコンを表示するか
    public static bool[] displaySystemCommand { get; set; }

    //システムアイコンの名前
    public static string[] systemCommandName { get; set; }

    //そのシステムアイコンを使えるようにするか
    public static bool[] useSystemCommand { get; set; }

    //システムアイコンの座標
    //public static Vector3[] systemCommandVector { get; set; }

    //システムアイコンの配置される円の半径
    public static float systemCommandRadius { get; set; }

    //文字の色
    public static Material material_SystemText { get; set; }

    //文字の大きさ
    public static int systemTextFontSize { get; set; }

    //VRで使用しているか
    public static bool isOnXR { get; set; }

    //centralSystemのgameObjectの座標
    public static Vector3 createSourcePosition { get; set; }

    //キーの縁取りの太さ
    public static float lineRendererWidth { get; set; }

    //キーの縁取りの本体からのずらし加減
    public static float lineShiftSlightly { get; set; }

    //システムの状態
    public static int stage { get; set; }
    /* ring1 → 主に真ん中にいるリング
     * ring2 → 途中で出現する2個目のリング
     *
     * stage: 0（そのままorポインタ帰還）
     * ring1 濃く　入力可能
     * ring2 存在せず
     * 
     * stage: 1（ring1に接触～ring2に接触中）
     * ring1 薄く　入力停止　ring2呼び出す
     * ring2 濃く　入力可能
     * 
     * stage: 2（ring2の外か中に移動～中心部に移動中）
     * ring1 （更に）薄く　入力停止
     * ring2 薄く　入力不可
     * 
     * stage: 0（ポインタ帰還）
     * ring1 濃く　入力可能
     * ring2 消す
     */

    /* Inspector用 */
    [SerializeField, Header("円環の内径(単位cm)")]
    private float RadiusIn;

    [SerializeField, Header("円環の外径(単位cm)")]
    private float RadiusOut;

    [SerializeField, Header("円環の厚さ(単位cm)")]
    private float PoleHeight;

    [SerializeField, Header("円環の縁取りの太さ(単位cm)")]
    private float LineRendererWidth;

    [SerializeField, Header("円環の縁取りのずらし量(単位cm)")]
    private float LineShiftSlightly;

    [SerializeField, Header("システムのキーの数（規定値 5 ）"), Tooltip("変更する際はスクリプトの見直すが必要")]
    private int PoleSum;

    [SerializeField, Header("円環内部の多角形のマテリアル")]
    private Material Material_PolygonalPillar;

    [SerializeField, Header("キーの常態のマテリアル")]
    private Material Material_TrapezoidPole_Normal;

    [SerializeField, Header("キーの接触時のマテリアル")]
    private Material Material_TrapezoidPole_Touch;

    [SerializeField, Header("キーの輪郭線のマテリアル")]
    private Material Material_LineRenderer;

    [SerializeField, Header("円環外側のシステムキーのマテリアル")]
    private Material Material_SystemText;

    [SerializeField, Header("円環外側のシステムキーの半径(単位cm)")]
    private float SystemCommandRadius;

    [SerializeField, Header("テキストのフォントサイズ( X cm角。小数点第二位まで有効)"), Tooltip("システム上に表示されるテキストのフォントサイズ")]
    private float SystemTextFontSize;

    [SerializeField, Header("キーの分割数"), Tooltip("キーを表示するための台形のポリゴンを任意の回数分割する"), Range(0, 29)]//MeshColliderのConvexが三角形ポリゴン255枚以下の必要があるため
    private int TrapezoidDivisionNum;


    private void Awake() {
        //文字種初期化
        //CharacterType = 0;
        //内径
        radiusIn = RadiusIn / 100;
        //外形
        radiusOut = RadiusOut / 100;
        //厚さ
        poleHeight = PoleHeight / 100;
        //キー数
        poleSum = PoleSum;
        //縁取りの太さ
        lineRendererWidth = LineRendererWidth / 100;
        //縁取りのずらし
        lineShiftSlightly = LineShiftSlightly / 100;

        //多角形のマテリアル初期化
        material_PolygonalPillar = Material_PolygonalPillar;
        //台形柱通常
        material_TrapezoidPole_Normal = Material_TrapezoidPole_Normal;
        //台形柱接触
        material_TrapezoidPole_Touch = Material_TrapezoidPole_Touch;
        //台形の強調ライン
        material_LineRenderer = Material_LineRenderer;

        //台形の分割回数
        trapezoidDivisionNum = TrapezoidDivisionNum + 1;

        //システムキーの半径
        systemCommandRadius = SystemCommandRadius / 100;
        //テキストのマテリアル
        material_SystemText = Material_SystemText;
        //テキストのフォントサイズ
        systemTextFontSize = (int)( SystemTextFontSize * 100 );

        //stage初期化
        stage = 0;
    }
}
