using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using UnityEngine;
using UnityEngine.SceneManagement;

public class centralSystem : MonoBehaviour {

    private int keyNums = 6;
    private int churingNumber = -100;//各ボタンから得られるカーソル位置情報
    [SerializeField]
    private int stage = 0;//インターフェースの処理段階       -100=Error  -2=システムアイコン入力済み  -1=外縁  0=Nutral  1=子音および母音選択  2=子音決定済み母音選択  3=母音決定
    private string InputText = "";
    private string setText = "";
    private int baseNumber;
    private int consonant;  //子音

    /* システムの形決定 */
    protected int poleSum = 5;            //キーの数
    protected float radiusOut = 4f;       //システムの外縁の半径
    protected float radiusIn = 2f;        //ニュートラルエリアの半径
    protected float poleHeight = 2f;      //システムの厚み
    private TextMesh textMesh;

    /* キーを取得済みフラグ */
    private bool isGetKeyObjects = false;
    /* キーオブジェクト */                          //中身はキーを再表示する度に再設定
    [SerializeField]
    private GameObject[] keyObjects;

    /*[親のpointNum,set]*/
    /* set = "見出し","要素数+1",要素1"a",要素2"b", ... 要素n+1,番外"Error" */
    protected readonly string[,] textSet = new string[15, 7] { { "k", "ka", "ki", "ku", "ke", "ko", "Error"},
                                                               { "a", "--", "--", "--", "--", "--", "Error"},
                                                               { "s", "sa", "si", "su", "se", "so", "Error"},
                                                               { "t", "ta", "ti", "tu", "te", "to", "Error"},
                                                               { "i", "--", "--", "--", "--", "--", "Error"},
                                                               { "n", "na", "ni", "nu", "ne", "no", "Error"},
                                                               { "h", "ha", "hi", "hu", "he", "ho", "Error"},
                                                               { "u", "--", "--", "--", "--", "--", "Error"},
                                                               { "m", "ma", "mi", "mu", "me", "mo", "Error"},
                                                               { "y", "ya", "゛", "yu", "゜", "yo", "Error"},
                                                               { "e", "--", "--", "--", "--", "--", "Error"},
                                                               { "r", "ra", "ri", "ru", "re", "ro", "Error"},
                                                               { "w", "wa", "wo", "nn", "改/確", "空/変", "Error"},
                                                               { "o", "--", "--", "--", "--", "--", "Error"},
                                                               { "-", "記号", "BS", "英", "数", "小", "Error"} };

    protected readonly string[,] textSetHiragana = new string[15, 7] { { "か", "か", "き", "く", "け", "こ", "Error"},
                                                                       { "あ", "--", "--", "--", "--", "--", "Error"},
                                                                       { "さ", "さ", "し", "す", "せ", "そ", "Error"},
                                                                       { "た", "た", "ち", "つ", "て", "と", "Error"},
                                                                       { "い", "--", "--", "--", "--", "--", "Error"},
                                                                       { "な", "な", "に", "ぬ", "ね", "の", "Error"},
                                                                       { "は", "は", "ひ", "ふ", "へ", "ほ", "Error"},
                                                                       { "う", "--", "--", "--", "--", "--", "Error"},
                                                                       { "ま", "ま", "み", "む", "め", "も", "Error"},
                                                                       { "や", "や", "゛", "ゆ", "゜", "よ", "Error"},
                                                                       { "え", "--", "--", "--", "--", "--", "Error"},
                                                                       { "ら", "ら", "り", "る", "れ", "ろ", "Error"},
                                                                       { "わ", "わ", "を", "ん", "改/確", "空/変", "Error"},
                                                                       { "お", "--", "--", "--", "--", "--", "Error"},
                                                                       { "--", "記号", "BS", "英", "数", "小", "Error"} };
    void Start() {
        textMesh = GameObject.Find("InputText").GetComponent<TextMesh>();
        Debug.Log(textSet.GetLength(0));
        SceneManager.LoadScene()
    }

    void Update() {
        textMesh.text = InputText;
        if (!isGetKeyObjects) {
            GetKeyObjects();
            SetKeytext();
        }
    }

    //外部から座標値を取得
    public void UpdateChuringNum(int nextNum) {
        churingNumber = nextNum;
        ChuringSystem();
    }

    private void ChuringSystem() {
        if (stage == 0 && 0 < churingNumber && churingNumber <= keyNums) {
            //ニュートラル状態で、入力キー値が1～キー数の間の場合実行
            //最初のキー値を決定
            baseNumber = churingNumber;
            //とりあえず母音を保存
            setText = textSet[( baseNumber - 1 ) * 3 + 1, 0];
            //次の状態へ
            stage = 1;
            if (isGetKeyObjects)
                SetKeytext();
            return;
        } else if (stage == 1 && baseNumber != churingNumber && churingNumber != 0) {
            //子音および母音選択状態で、最初のキー値と入力キー値が違い、中心に戻ったわけではない場合
            //子音が決定するので計算
            if (baseNumber == poleSum && churingNumber == 1) {
                //最後のキーから1キーへの入力の際
                consonant = textSet.GetLength(0) - 1;
            } else if (baseNumber == 1 && churingNumber == poleSum) {
                //１キーから最後のキーへの入力の際
                consonant = 0;
            } else {
                //それ以外の隣り合うキー値が同じ場合の計算
                consonant = ( ( baseNumber - 1 ) * 3 + 1 ) + ( churingNumber - baseNumber );
            }
            //子音と母音から再計算
            setText = textSet[consonant, churingNumber - 1 + 1];
            //次の状態へ
            stage = 2;
            if (isGetKeyObjects)
                SetKeytext();
            return;
        } else if (stage == 2 && 1 <= churingNumber && churingNumber <= keyNums) {
            //子音決定済み母音選択状態で、入力キー値が1～キー数の間の場合実行
            setText = textSet[consonant, churingNumber - 1 + 1];
            if (isGetKeyObjects)
                SetKeytext();
            return;
        } else if (( stage == 1 || stage == 2 ) && churingNumber == 0) {
            //入力状態で、中心へ戻った場合
            ConvertToSystemCommand();
            InputText = setText;
            setText = "";
            stage = 0;
            if (isGetKeyObjects)
                SetKeytext();
            return;
        }
        Debug.LogWarning("Error. stage = " + stage + " . churingNumber = " + churingNumber + " . baseNumber = " + baseNumber);
    }

    //入力された内容をシステムコマンドに変換する(setText内で完結させる)
    private void ConvertToSystemCommand() {
        if (setText == "--") {
            setText = "入力なし";
        } else if (setText == "改/確") {
            setText = "改行/確定";
        } else if (setText == "空/変") {
            setText = "空白/変換";
        } else if (setText == "記号") {
            setText = "記号など";
        } else if (setText == "BS") {
            setText = "BackSpace";
        } else if (setText == "英") {
            setText = "英語";
        } else if (setText == "数") {
            setText = "数字";
        } else if (setText == "小") {
            setText = "小文字";
        } else if (setText == "゛") {
            setText = "濁点";
        } else if (setText == "゜") {
            setText = "半濁点";
        }
    }

    //キーオブジェクトの取得
    private void GetKeyObjects() {
        keyObjects = new GameObject[poleSum + 1];
        for (int i = 0; i <= poleSum; i++) {
            if (keyObjects[i] == null) {
                if (GameObject.Find(i.ToString()))
                    keyObjects[i] = GameObject.Find(i.ToString());
                else
                    break;
            }
            if (i == poleSum) {
                isGetKeyObjects = true;
            }
        }
    }

    //キーに文字を割り当てる
    private void SetKeytext() {
        for (int i = 1; i <= poleSum; i++) {
            if (stage == 0) {
                keyObjects[i].GetComponent<TrapezoidPole>().MyText = textSet[i * 3 - 3, 0] + textSet[i * 3 - 2, 0] + textSet[i * 3 - 1, 0];
            } else if (stage == 1) {
                /*********************デバッグ用**************************************************/
                if (i == 5) {
                    Debug.Log("1mytext is " + keyObjects[i].GetComponent<TrapezoidPole>().MyText);
                }
                /*********************デバッグ用終わり********************************************/
                if (churingNumber - 1 == i) {
                    //左隣の値
                    keyObjects[i].GetComponent<TrapezoidPole>().MyText = textSet[( churingNumber - 1 ) * 3, 0];
                } else if (churingNumber == i) {
                    //現在座標の値
                    keyObjects[i].GetComponent<TrapezoidPole>().MyText = textSet[( churingNumber - 1 ) * 3 + 1, 0];
                } else if (churingNumber + 1 == i) {
                    //右隣の値
                    keyObjects[i].GetComponent<TrapezoidPole>().MyText = textSet[( churingNumber - 1 ) * 3 + 2, 0];
                } else if (churingNumber - ( poleSum - 1 ) == i) {
                    //現在地(churingNumber)が端(最大値)の場合の右隣の値
                    keyObjects[i].GetComponent<TrapezoidPole>().MyText = textSet[( churingNumber - 1 ) * 3 + 2, 0];
                } else if (churingNumber + ( poleSum - 1 ) == i) {
                    //現在地(churingNumber)が端(1)の場合の左隣の値
                    keyObjects[i].GetComponent<TrapezoidPole>().MyText = textSet[0, 0];
                } else {
                    keyObjects[i].GetComponent<TrapezoidPole>().MyText = "--";
                }
                /*********************デバッグ用**************************************************/
                if (i == 5) {
                    Debug.Log("2mytext is " + keyObjects[i].GetComponent<TrapezoidPole>().MyText);
                }
                /*********************デバッグ用終わり********************************************/
            } else if (stage == 2) {

                /*********************デバッグ用**************************************************/
                if (i == 5) {
                    Debug.Log("1mytext is " + keyObjects[i].GetComponent<TrapezoidPole>().MyText);
                }
                /*********************デバッグ用終わり********************************************/
                keyObjects[i].GetComponent<TrapezoidPole>().MyText = textSet[consonant, i];
                /*
                                if (0 < i && i < poleSum) {
                                    //現在座標の値
                                    keyObjects[i].GetComponent<TrapezoidPole>().MyText = textSet[consonant, i];
                                } else if (churingNumber - ( poleSum - 1 ) == i) {
                                    //現在地(churingNumber)が端(最大値)の場合の右隣の値
                                    keyObjects[i].GetComponent<TrapezoidPole>().MyText = textSet[( churingNumber - 1 ) * 3 + 2, 0];
                                } else if (churingNumber + ( poleSum - 1 ) == i) {
                                    //現在地(churingNumber)が端(1)の場合の左隣の値
                                    keyObjects[i].GetComponent<TrapezoidPole>().MyText = textSet[consonant, i];
                                } else {
                                    Debug.LogWarning("Error. Unknown input.");
                                }
                                */
                /*********************デバッグ用**************************************************/
                if (i == 5) {
                    Debug.Log("2mytext is " + keyObjects[i].GetComponent<TrapezoidPole>().MyText);
                }
                /*********************デバッグ用終わり********************************************/
            }
        }
    }

    //システムの形などのコールバック用ゲッター
    public int PoleSum {
        get { return poleSum; }
    }

    public float RadiusOut {
        get { return radiusOut; }
    }

    public float RadiusIn {
        get { return radiusIn; }
    }

    public float PoleHeight {
        get { return poleHeight; }
    }
}
