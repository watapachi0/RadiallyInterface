using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class variables : MonoBehaviour {

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



    /* Inspector用 */
    [SerializeField]
    private float RadiusIn;

    [SerializeField]
    private float RadiusOut;

    [SerializeField]
    private float PoleHeight;

    [SerializeField]
    private int PoleSum;

    [SerializeField]
    private Material Material_PolygonalPillar;

    [SerializeField]
    private Material Material_TrapezoidPole_Normal;

    [SerializeField]
    private Material Material_TrapezoidPole_Touch;

    [SerializeField]
    private Material Material_LineRenderer;

    private void Awake() {
        //文字種初期化
        //CharacterType = 0;
        //内径
        radiusIn = RadiusIn;
        //外形
        radiusOut = RadiusOut;
        //厚さ
        poleHeight = PoleHeight;
        //キー数
        poleSum = PoleSum;

        //多角形のマテリアル初期化
        material_PolygonalPillar = Material_PolygonalPillar;
        //台形柱通常
        material_TrapezoidPole_Normal = Material_TrapezoidPole_Normal;
        //台形柱接触
        material_TrapezoidPole_Touch = Material_TrapezoidPole_Touch;
        //台形の強調ライン
        material_LineRenderer = Material_LineRenderer;
    }
}
