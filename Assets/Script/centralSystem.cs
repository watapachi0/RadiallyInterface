using System.Collections;
using System.Collections.Generic;
//using System.Windows.Forms;
using UnityEngine;
using UnityEngine.XR;
//try catch用 System.Exception
using System;

public class centralSystem : MonoBehaviour {

    private int churingNumber = -100;//各ボタンから得られるカーソル位置情報
    protected int stage = 0;//インターフェースの処理段階       -100=Error  -2=システムアイコン入力済み  -1=外縁  0=Nutral  1=第一段階選択  2=第二段階選択時  3=入力決定
    protected string InputText = "";
    protected string setText = "";
    private int baseNumber;
    private int consonant;  //子音

    /* システムの形決定 */
    //protected int poleSum;            //キーの数
    private int trapezoidDivisionNum = 5;     //キー当たりのメッシュ数　1以上
    protected float radiusOut = 1f;       //システムの外縁の半径
    protected float radiusIn = 0.7f;        //ニュートラルエリアの半径
    protected float poleHeight = 1f;      //システムの厚み
    private TextMesh textMesh;

    /* キーを取得済みフラグ */
    private bool isGetKeyObjects = false;
    /* キーオブジェクト */                          //中身はキーを再表示する度に再設定
    private GameObject[] keyObjects;
    //private GameObject[] keySubObjects;         //副輪の精製後の一時保管用
    private GameObject[][] subCircles;           //副輪をまとめて取得
    //副輪オブジェクト
    //private GameObject subCircle;
    //副輪の表示処理が終わった後、LateUpdateにてGameObjectを取得するためのフラグ
    private bool doLateUpdat = false;
    //主輪の中心キーのスクリプトのインスタンス
    private PolygonalPillar polygonalPillar;

    //主輪の中心に触れているか
    private bool isTouchMainPillar = false;
    //副輪の中心に触れているか
    private bool isTouchSubPillar = false;
    //副輪が存在するか
    //private bool hasSubCircle = false;

    //テスト中
    //public bool isCircleInterface;

    //中心の多角柱を使用可にしていいか
    private bool canEnterPolygonalPiller = true;
    //それぞれの意見
    private bool[] couldEnterPP;

    /*[親のpointNum,set]*/
    protected string[,] textSet;

    //実験用のタイピングスクリプト
    private TypingSystem typingSystem;

    //Radially用textSet
    /* か行見出し　か行あ段　か行い段  …
     * あ
     * さ行見出し　さ行あ段　さ行い段  …
     * た行見出し　た行あ段　た行い段  …
     * い
     * な行見出し　な行あ段　な行い段  …
     * ：
     * ：
     */
    //デバッグ用
    protected readonly string[,] textSetDebug = new string[15, 7] {    { "k", "ka"   , "ki", "ku"  , "ke"   , "ko"   , "Error"},
                                                                       { "a", "--"   , "--", "--"  , "--"   , "--"   , "Error"},
                                                                       { "s", "sa"   , "si", "su"  , "se"   , "so"   , "Error"},
                                                                       { "t", "ta"   , "ti", "tu"  , "te"   , "to"   , "Error"},
                                                                       { "i", "--"   , "--", "--"  , "--"   , "--"   , "Error"},
                                                                       { "n", "na"   , "ni", "nu"  , "ne"   , "no"   , "Error"},
                                                                       { "h", "ha"   , "hi", "hu"  , "he"   , "ho"   , "Error"},
                                                                       { "u", "--"   , "--", "--"  , "--"   , "--"   , "Error"},
                                                                       { "m", "ma"   , "mi", "mu"  , "me"   , "mo"   , "Error"},
                                                                       { "y", "ya"   , "゛", "yu"  , "゜"   , "yo"   , "Error"},
                                                                       { "e", "--"   , "--", "--"  , "--"   , "--"   , "Error"},
                                                                       { "r", "ra"   , "ri", "ru"  , "re"   , "ro"   , "Error"},
                                                                       { "w", "wa"   , "wo", "nn"  , "改/確", "空/変", "Error"},
                                                                       { "o", "--"   , "--", "--"  , "--"   , "--"   , "Error"},
                                                                       { "BS", "記/数", "BS", "かな", "カナ" , "小"   , "Error"} };
    //ひらがな
    protected readonly string[,] textSetHiragana = new string[15, 7] { { "か", "か"   , "き", "く", "け"   , "こ"   , "Error"},
                                                                       { "あ", "--"   , "--", "--", "--"   , "--"   , "Error"},
                                                                       { "さ", "さ"   , "し", "す", "せ"   , "そ"   , "Error"},
                                                                       { "た", "た"   , "ち", "つ", "て"   , "と"   , "Error"},
                                                                       { "い", "--"   , "--", "--", "--"   , "--"   , "Error"},
                                                                       { "な", "な"   , "に", "ぬ", "ね"   , "の"   , "Error"},
                                                                       { "は", "は"   , "ひ", "ふ", "へ"   , "ほ"   , "Error"},
                                                                       { "う", "--"   , "--", "--", "--"   , "--"   , "Error"},
                                                                       { "ま", "ま"   , "み", "む", "め"   , "も"   , "Error"},
                                                                       { "や", "や"   , "゛", "ゆ", "゜"   , "よ"   , "Error"},
                                                                       { "え", "--"   , "--", "--", "--"   , "--"   , "Error"},
                                                                       { "ら", "ら"   , "り", "る", "れ"   , "ろ"   , "Error"},
                                                                       { "わ", "わ"   , "を", "ん", "改/確", "空/変", "Error"},
                                                                       { "お", "--"   , "--", "--", "--"   , "--"   , "Error"},
                                                                       { "BS", "BS"   ,"記/数",  "英", "カナ" , "小"   , "Error"} };
    //カタカナ
    protected readonly string[,] textSetKatakana = new string[15, 7] { { "カ", "カ"   , "キ", "ク", "ケ"   , "コ"   , "Error"},
                                                                       { "ア", "--"   , "--", "--", "--"   , "--"   , "Error"},
                                                                       { "サ", "サ"   , "シ", "ス", "セ"   , "ソ"   , "Error"},
                                                                       { "タ", "タ"   , "チ", "ツ", "テ"   , "ト"   , "Error"},
                                                                       { "イ", "--"   , "--", "--", "--"   , "--"   , "Error"},
                                                                       { "ナ", "ナ"   , "ニ", "ヌ", "ネ"   , "ノ"   , "Error"},
                                                                       { "ハ", "ハ"   , "ヒ", "フ", "ヘ"   , "ホ"   , "Error"},
                                                                       { "ウ", "--"   , "--", "--", "--"   , "--"   , "Error"},
                                                                       { "マ", "マ"   , "ミ", "ム", "メ"   , "モ"   , "Error"},
                                                                       { "ヤ", "ヤ"   , "゛", "ユ", "゜"   , "ヨ"   , "Error"},
                                                                       { "エ", "--"   , "--", "--", "--"   , "--"   , "Error"},
                                                                       { "ラ", "ラ"   , "リ", "ル", "レ"   , "ロ"   , "Error"},
                                                                       { "ワ", "ワ"   , "ヲ", "ン", "改/確", "空/変", "Error"},
                                                                       { "オ", "--"   , "--", "--", "--"   , "--"   , "Error"},
                                                                       { "--", "記/数", "BS", "英", "かな" , "小"   , "Error"} };
    //小文字アルファベット
    protected readonly string[,] textSetAlphabet = new string[15, 7] { { "abc" , "a"    , "b" , "c"   , "--"   , "--"   , "Error"},
                                                                       { ""    , "--"   , "--", "--"  , "--"   , "--"   , "Error"},
                                                                       { "def" , "d"    , "e" , "f"   , "--"   , "--"   , "Error"},
                                                                       { "ghi" , "g"    , "h" , "i"   , "--"   , "--"   , "Error"},
                                                                       { ""    , "--"   , "--", "--"  , "--"   , "--"   , "Error"},
                                                                       { "jkl" , "j"    , "k" , "l"   , "--"   , "--"   , "Error"},
                                                                       { "mno" , "m"    , "n" , "o"   , "--"   , "--"   , "Error"},
                                                                       { ""    , "--"   , "--", "--"  , "--"   , "--"   , "Error"},
                                                                       { "pqrs", "p"    , "q" , "r"   , "s"    , "--"   , "Error"},
                                                                       { "tuv" , "t"    , "u" , "v"   , "--"   , "--"   , "Error"},
                                                                       { ""    , "--"   , "--", "--"  , "--"   , "--"   , "Error"},
                                                                       { "wxyz", "w"    , "x" , "y"   , "z"    , "--"   , "Error"},
                                                                       { "--"  , "--"   , "--", "--"  , "改/確", "空/変", "Error"},
                                                                       { "--"  , "--"   , "--", "--"  , "--"   , "--"   , "Error"},
                                                                       { "--"  , "記/数", "BS", "かな", "カナ" , "Ａ"   , "Error"} };
    //大文字アルファベット
    protected readonly string[,] textSetALPHABET = new string[15, 7] { { "ABC" , "A"    , "B" , "C"   , "--"   , "--"   , "Error"},
                                                                       { ""    , "--"   , "--", "--"  , "--"   , "--"   , "Error"},
                                                                       { "DEF" , "D"    , "E" , "F"   , "--"   , "--"   , "Error"},
                                                                       { "GHI" , "G"    , "H" , "I"   , "--"   , "--"   , "Error"},
                                                                       { ""    , "--"   , "--", "--"  , "--"   , "--"   , "Error"},
                                                                       { "JKL" , "J"    , "K" , "L"   , "--"   , "--"   , "Error"},
                                                                       { "MNO" , "M"    , "N" , "O"   , "--"   , "--"   , "Error"},
                                                                       { ""    , "--"   , "--", "--"  , "--"   , "--"   , "Error"},
                                                                       { "PQRS", "P"    , "Q" , "R"   , "S"    , "--"   , "Error"},
                                                                       { "TUV" , "T"    , "U" , "V"   , "--"   , "--"   , "Error"},
                                                                       { ""    , "--"   , "--", "--"  , "--"   , "--"   , "Error"},
                                                                       { "WXYZ", "W"    , "X" , "Y"   , "Z"    , "--"   , "Error"},
                                                                       { "--"  , "--"   , "--", "--"  , "改/確", "空/変", "Error"},
                                                                       { "--"  , "--"   , "--", "--"  , "--"   , "--"   , "Error"},
                                                                       { "--"  , "記/数", "BS", "かな", "カナ" , "ａ"   , "Error"} };
    //数字と記号
    protected readonly string[,] textSetSignNum = new string[15, 7] {  { "1-/:;" , "1" , "-"   , "/" , ":"    , ";"    , "Error"},
                                                                       { ""      , "--", "--"  , "--", "--"   , "--"   , "Error"},
                                                                       { "2()\\&", "2" , "("   , ")" , "\\"   , "&"    , "Error"},
                                                                       { "3@\".,", "3" , "@"   , "\"", "."    , ","    , "Error"},
                                                                       { ""      , "--", "--"  , "--", "--"   , "--"   , "Error"},
                                                                       { "4?!'"  , "4" , "?"   , "!" , "'"    , "--"   , "Error"},
                                                                       { "5[]{}" , "5" , "["   , "]" , "{"    , "}"    , "Error"},
                                                                       { ""      , "--", "--"  , "--", "--"   , "--"   , "Error"},
                                                                       { "6#%^*" , "6" , "#"   , "%" , "^"    , "*"    , "Error"},
                                                                       { "7+=_|" , "7" , "+"   , "=" , "_"    , "|"    , "Error"},
                                                                       { ""      , ""  , "--"  , "--", "--"   , "--"   , "Error"},
                                                                       { "8~<>$" , "8" , "~"   , "<" , ">"    , "$"    , "Error"},
                                                                       { "9"     , "9" , "--"  , "--", "改/確", "空/変", "Error"},
                                                                       { ""      , "--", "--"  , "--", "--"   , "--"   , "Error"},
                                                                       { "0"     , "0" , "かな", "BS", "英"   , "カナ" , "Error"} };

    //Circle用textset
    //デバッグ用
    protected readonly string[,] textSetDebugCircle = new string[11, 7] {    { "a", "a"    , "i",  "u"   , "e"    , "o"    , "Error"},
                                                                             { "ka", "ka"   , "ki", "ku"  , "ke"   , "ko"   , "Error"},
                                                                             { "sa", "sa"   , "si", "su"  , "se"   , "so"   , "Error"},
                                                                             { "ta", "ta"   , "ti", "tu"  , "te"   , "to"   , "Error"},
                                                                             { "na", "na"   , "ni", "nu"  , "ne"   , "no"   , "Error"},
                                                                             { "ha", "ha"   , "hi", "hu"  , "he"   , "ho"   , "Error"},
                                                                             { "ma", "ma"   , "mi", "mu"  , "me"   , "mo"   , "Error"},
                                                                             { "ya", "ya"   , "゛", "yu"  , "゜"   , "yo"   , "Error"},
                                                                             { "ra", "ra"   , "ri", "ru"  , "re"   , "ro"   , "Error"},
                                                                             { "wa", "wa"   , "wo", "nn"  , "改/確", "空/変", "Error"},
                                                                             { "!?", "記/数", "BS", "かな", "カナ" , "小"   , "Error"} };
    //ひらがな
    protected readonly string[,] textSetHiraganaCircle = new string[11, 9] { { "あ", "あ"   , "い", "う", "え"   , "お"   , "Error", "Error", "Error"},
                                                                             { "か", "か"   , "き", "く", "け"   , "こ"   , "゛", "Error", "Error"},
                                                                             { "さ", "さ"   , "し", "す", "せ"   , "そ"   , "゛", "Error", "Error"},
                                                                             { "た", "た"   , "ち", "つ", "て"   , "と"   , "゛", "Error", "Error"},
                                                                             { "な", "な"   , "に", "ぬ", "ね"   , "の"   , "Error", "Error", "Error"},
                                                                             { "は", "は"   , "ひ", "ふ", "へ"   , "ほ"   , "゛", "゜", "Error"},
                                                                             { "ま", "ま"   , "み", "む", "め"   , "も"   , "Error", "Error", "Error"},
                                                                             { "や", "や"   , "゛", "ゆ", "゜"   , "よ"   , "小"   , "Error", "Error"},
                                                                             { "ら", "ら"   , "り", "る", "れ"   , "ろ"   , "Error", "Error", "Error"},
                                                                             { "わ", "わ"   , "を", "ん", "改/確", "空/変", "Error", "Error", "Error"},
                                                                             { "BS", "記/数", "英", "BS", "カナ" , "小"   , "Error", "Error", "Error"} };
    //カタカナ
    protected readonly string[,] textSetKatakanaCircle = new string[11, 7] { { "ア", "ア"   , "イ", "ウ", "エ"   , "オ"   , "Error"},
                                                                             { "カ", "カ"   , "キ", "ク", "ケ"   , "コ"   , "Error"},
                                                                             { "サ", "サ"   , "シ", "ス", "セ"   , "ソ"   , "Error"},
                                                                             { "タ", "タ"   , "チ", "ツ", "テ"   , "ト"   , "Error"},
                                                                             { "ナ", "ナ"   , "ニ", "ヌ", "ネ"   , "ノ"   , "Error"},
                                                                             { "ハ", "ハ"   , "ヒ", "フ", "ヘ"   , "ホ"   , "Error"},
                                                                             { "マ", "マ"   , "ミ", "ム", "メ"   , "モ"   , "Error"},
                                                                             { "ヤ", "ヤ"   , "゛", "ユ", "゜"   , "ヨ"   , "Error"},
                                                                             { "ラ", "ラ"   , "リ", "ル", "レ"   , "ロ"   , "Error"},
                                                                             { "ワ", "ワ"   , "ヲ", "ン", "改/確", "空/変", "Error"},
                                                                             { "--", "記/数", "BS", "英", "かな" , "小"   , "Error"} };
    //小文字アルファベット
    protected readonly string[,] textSetAlphabetCircle = new string[11, 7] { { "abc" , "a"    , "b" , "c"   , "--"   , "--"   , "Error"},
                                                                             { "abc" , "a"    , "b" , "c"   , "--"   , "--"   , "Error"},
                                                                             { "def" , "d"    , "e" , "f"   , "--"   , "--"   , "Error"},
                                                                             { "ghi" , "g"    , "h" , "i"   , "--"   , "--"   , "Error"},
                                                                             { "jkl" , "j"    , "k" , "l"   , "--"   , "--"   , "Error"},
                                                                             { "mno" , "m"    , "n" , "o"   , "--"   , "--"   , "Error"},
                                                                             { "pqrs", "p"    , "q" , "r"   , "s"    , "--"   , "Error"},
                                                                             { "tuv" , "t"    , "u" , "v"   , "--"   , "--"   , "Error"},
                                                                             { "wxyz", "w"    , "x" , "y"   , "z"    , "--"   , "Error"},
                                                                             { "--"  , "--"   , "--", "--"  , "改/確", "空/変", "Error"},
                                                                             { "--"  , "記/数", "BS", "かな", "カナ" , "Ａ"   , "Error"} };
    //大文字アルファベット
    protected readonly string[,] textSetALPHABETCircle = new string[11, 7] { { "ABC" , "A"    , "B" , "C"   , "--"   , "--"   , "Error"},
                                                                             { "ABC" , "A"    , "B" , "C"   , "--"   , "--"   , "Error"},
                                                                             { "DEF" , "D"    , "E" , "F"   , "--"   , "--"   , "Error"},
                                                                             { "GHI" , "G"    , "H" , "I"   , "--"   , "--"   , "Error"},
                                                                             { "JKL" , "J"    , "K" , "L"   , "--"   , "--"   , "Error"},
                                                                             { "MNO" , "M"    , "N" , "O"   , "--"   , "--"   , "Error"},
                                                                             { "PQRS", "P"    , "Q" , "R"   , "S"    , "--"   , "Error"},
                                                                             { "TUV" , "T"    , "U" , "V"   , "--"   , "--"   , "Error"},
                                                                             { "WXYZ", "W"    , "X" , "Y"   , "Z"    , "--"   , "Error"},
                                                                             { "--"  , "--"   , "--", "--"  , "改/確", "空/変", "Error"},
                                                                             { "--"  , "記/数", "BS", "かな", "カナ" , "ａ"   , "Error"} };
    //数字と記号
    protected readonly string[,] textSetSignNumCircle = new string[11, 7] {  { "1-/:;" , "1" , "-"   , "/" , ":"    , ";"    , "Error"},
                                                                             { "1-/:;" , "1" , "-"   , "/" , ":"    , ";"    , "Error"},
                                                                             { "2()\\&", "2" , "("   , ")" , "\\"   , "&"    , "Error"},
                                                                             { "3@\".,", "3" , "@"   , "\"", "."    , ","    , "Error"},
                                                                             { "4?!'"  , "4" , "?"   , "!" , "'"    , "--"   , "Error"},
                                                                             { "5[]{}" , "5" , "["   , "]" , "{"    , "}"    , "Error"},
                                                                             { "6#%^*" , "6" , "#"   , "%" , "^"    , "*"    , "Error"},
                                                                             { "7+=_|" , "7" , "+"   , "=" , "_"    , "|"    , "Error"},
                                                                             { "8~<>$" , "8" , "~"   , "<" , ">"    , "$"    , "Error"},
                                                                             { "9"     , "9" , "--"  , "--", "改/確", "空/変", "Error"},
                                                                             { "0"     , "0" , "かな", "BS", "英"   , "カナ" , "Error"} };

    //濁点、半濁点、小文字対応
    /* あ行の検索index   あ   い   う   …
     * あ行の濁点文字    あ゛ い゛ う゛ …
     * あ行の半濁点文字　あ゜ い゜ う゜ …
     * あ行の小文字      ぁ   ぃ   ぅ   …
     * か行の検索index   か   き   く   …
     *      :
     *      :
     */
    protected readonly string[,] RetranslationSet = new string[48, 6] { { "あ", "い", "う", "え", "お", "Error" },
                                                                       { ""  , ""  , "ゔ", ""  , ""  , "Error" },
                                                                       { ""  , ""  , ""  , ""  , ""  , "Error" },
                                                                       { "ぁ", "ぃ", "ぅ", "ぇ", "ぉ", "Error" },

                                                                       { "か", "き", "く", "け", "こ", "Error" },
                                                                       { "が", "ぎ", "ぐ", "げ", "ご", "Error" },
                                                                       { ""  , ""  , ""  , ""  , ""  , "Error" },
                                                                       { ""  , ""  , ""  , ""  , ""  , "Error" },

                                                                       { "さ", "し", "す", "せ", "そ", "Error" },
                                                                       { "ざ", "じ", "ず", "ぜ", "ぞ", "Error" },
                                                                       { ""  , ""  , ""  , ""  , ""  , "Error" },
                                                                       { ""  , ""  , ""  , ""  , ""  , "Error" },

                                                                       { "た", "ち", "つ", "て", "と", "Error" },
                                                                       { "だ", "ぢ", "づ", "で", "ど", "Error" },
                                                                       { ""  , ""  , ""  , ""  , ""  , "Error" },
                                                                       { ""  , ""  , "っ", ""  , ""  , "Error" },

                                                                       { "は", "ひ", "ふ", "へ", "ほ", "Error" },
                                                                       { "ば", "び", "ぶ", "べ", "ぼ", "Error" },
                                                                       { "ぱ", "ぴ", "ぷ", "ぺ", "ぽ", "Error" },
                                                                       { ""  , ""  , ""  , ""  , ""  , "Error" },

                                                                       { "や", ""  , "ゆ", ""  , "よ", "Error" },
                                                                       { ""  , ""  , ""  , ""  , ""  , "Error" },
                                                                       { ""  , ""  , ""  , ""  , ""  , "Error" },
                                                                       { "ゃ", ""  , "ゅ", ""  , "ょ", "Error" },

                                                                       { "ア", "イ", "ウ", "エ", "オ", "Error" },
                                                                       { ""  , ""  , "ヴ", ""  , ""  , "Error" },
                                                                       { ""  , ""  , ""  , ""  , ""  , "Error" },
                                                                       { "ァ", "ィ", "ゥ", "ェ", "ォ", "Error" },

                                                                       { "カ", "キ", "ク", "ケ", "コ", "Error" },
                                                                       { "ガ", "ギ", "グ", "ゲ", "ゴ", "Error" },
                                                                       { ""  , ""  , ""  , ""  , ""  , "Error" },
                                                                       { "ヵ", ""  , ""  , "ヶ", ""  , "Error" },

                                                                       { "サ", "シ", "ス", "セ", "ソ", "Error" },
                                                                       { "ザ", "ジ", "ズ", "ゼ", "ゾ", "Error" },
                                                                       { ""  , ""  , ""  , ""  , ""  , "Error" },
                                                                       { ""  , ""  , ""  , ""  , ""  , "Error" },

                                                                       { "タ", "チ", "ツ", "テ", "ト", "Error" },
                                                                       { "ダ", "ヂ", "ヅ", "デ", "ド", "Error" },
                                                                       { ""  , ""  , ""  , ""  , ""  , "Error" },
                                                                       { ""  , ""  , "ッ", ""  , ""  , "Error" },

                                                                       { "ハ", "ヒ", "フ", "ヘ", "ホ", "Error" },
                                                                       { "バ", "ビ", "ブ", "ベ", "ボ", "Error" },
                                                                       { "パ", "ピ", "プ", "ペ", "ポ", "Error" },
                                                                       { ""  , ""  , ""  , ""  , ""  , "Error" },

                                                                       { "ヤ", ""  , "ユ", ""  , "ヨ", "Error" },
                                                                       { ""  , ""  , ""  , ""  , ""  , "Error" },
                                                                       { ""  , ""  , ""  , ""  , ""  , "Error" },
                                                                       { "ャ", ""  , "ュ", ""  , "ョ", "Error" } };

    private bool[] useSystemCommand = new bool[10] { true, true, true, true, true, true, true, true, true, true };
    protected readonly string[] SystemCommandName = new string[10] { "改/確", "空/変", "記/数", "BS", "カナ", "小", "英", "かな", "Ａ", "ａ" };
    private bool[] dispSystemCommand = new bool[10] { true, true, true, true, true, true, true, true, true, true };
    //protected readonly Vector3[] SystemCommandVector = new Vector3[10] { new Vector3(), Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };

    private void Awake() {
        //variablesの初期化
        //variables.poleSum = this.poleSum;
        //variables.trapezoidDivisionNum = this.trapezoidDivisionNum;
        //variables.radiusOut = this.radiusOut;
        //variables.radiusIn = this.radiusIn;
        //variables.poleHeight = this.poleHeight;
        variables.systemCommandNum = 10;
        //variables.systemCommandRadius = 5;
        variables.useSystemCommand = this.useSystemCommand;
        variables.systemCommandName = this.SystemCommandName;
        variables.displaySystemCommand = this.dispSystemCommand;
        //variables.SystemCommandVector = this.SystemCommandVector;
        //文字セット初期化
        if (variables.isCircleSystem) {
            textSet = textSetHiraganaCircle;
            variables.poleSum = textSet.GetLength(0);
        } else {
            textSet = textSetHiragana;
            variables.poleSum = textSet.GetLength(0) / 3;
        }

        //XRであるかどうか
        variables.isOnXR = XRSettings.enabled;
        variables.createSourcePosition = transform.position;

        subCircles = new GameObject[variables.poleSum + 1][];
    }

    void Start() {
        typingSystem = GetComponent<TypingSystem>();

        if (/*variables.isCircleSystem*/true) {
            //主輪の取得と副輪の一斉表示
            IEnumerator getkey = GetKeyObjects();
            StartCoroutine(getkey);
        }

        textMesh = GameObject.Find("InputText").GetComponent<TextMesh>();
        //XR用設定
        if (variables.isOnXR) {
            GameObject.Find("Main CameraNonVR").SetActive(false);
        }
    }

    void Update() {
        textMesh.text = InputText;
        if (!isGetKeyObjects && GameObject.Find(0.ToString())) {
            //GetKeyObjects();  //コルーチンじゃない方
            //SetKeytext();
        }
    }

    //外部から座標値を取得
    public void UpdateChuringNum(int nextNum) {
        churingNumber = nextNum;
        if (variables.isCircleSystem)
            ChuringSystemCircle();
        else
            ChuringSystemRadially();
    }

    private void ChuringSystemCircle() {
        /*Debug.Log("stage = " + stage + " . " +
                         "churingNumber = " + churingNumber + " . " +
                         "baseNumber = " + baseNumber + " .");
                */
        /* Exitイベント、副輪イベント処理 */
        /* churingNumber    意味
         * ***0             主輪の中心
         * ***1~**99        主輪のキー
         * *100             副輪の中心
         * *101~*199        副輪のキー
         * 1000~1199        各キーのExitイベント(解釈せずにreturnする)
         * -1               システムコマンド
         */
        if (churingNumber == 0) {
            //主輪の中心に触れた
            isTouchMainPillar = true;
        } else if (churingNumber == 1000) {
            //主輪の中心から離れた
            isTouchMainPillar = false;
        } else if (churingNumber == 100) {
            //副輪の中心に触れた
            isTouchSubPillar = true;
        } else if (churingNumber == 1100) {
            //副輪の中心から離れた
            isTouchSubPillar = false;
        }

        //主輪の中心を触れながら主輪のキー入力はできない
        if (isTouchMainPillar) {
            if (1 <= churingNumber && churingNumber <= 99) {
                //return;
            }
        }
        //副輪の中心を触れながら副輪のキー入力はできない
        if (isTouchSubPillar) {
            if (101 <= churingNumber && churingNumber <= 199) {
                //return;
            }
        }

        //副輪の中心は解釈に使われない
        if (churingNumber == 100) {
            //ただし、主輪の0キーを無効にだけする
            //システムの大きさや円環の大きさ次第で主輪0キーと副輪0キーが重なるため
            //文字選択後に副輪の０キーに手が戻ってもいいように。
            if (stage <= 1)
                polygonalPillar.Enable(false);
            return;
        }

        //Exitイベントは解釈せずに終了
        if (1000 <= churingNumber) {
            //ただし、副輪の文字キーのExitだけは、主輪の0キー有効化のために使う
            if (1101 <= churingNumber && churingNumber <= 1199) {
                //副輪の0キーに触れているか
                if (isTouchSubPillar) {
                    //なにもしない
                } else {
                    //触れていないなら（中心へ戻ったわけではない）主輪0キーを有効化
                    polygonalPillar.Enable(true);
                }
            }
            return;
        }

        //副輪は戻す
        if (100 <= churingNumber) {
            churingNumber %= 100;
        }
        /* Exitイベント、副輪イベント処理  終 */

        if (churingNumber < 0) {
            //churingNumberがマイナス＝システムキーに触れたとき
            //数値を反転し、システムキーの名前を参照する
            setText = SystemCommandName[-churingNumber];
            stage = 4;
        } else {
            if (stage == 0) {
                //Debug.Log("run churing:" + churingNumber + " pole:" + variables.poleSum);
                //ニュートラル状態で、入力キー値が1～キー数の間の場合実行
                if (0 < churingNumber && churingNumber <= variables.poleSum + 1) {
                    //最初のキー値を決定
                    baseNumber = churingNumber;
                    consonant = baseNumber - 1;
                    //とりあえず母音を保存
                    //setText = textSet[consonant, 0];
                    //次の状態へ
                    stage = 1;

                    /* ここで副輪を呼ぶ */
                    //subCircleGenerete(true);
                    DisplaySubCircle(baseNumber, true);

                    //主輪を接触不可にし、色を変える
                    enableMainCircle(false);
                    //主輪の0キーを無効化
                    polygonalPillar.Enable(false);
                }
                //0の時は解釈終了判定にいきたくない
                return;
            } else if (stage == 1) {
                //主輪選択後で、中心に戻ったわけではない場合
                if (0 < churingNumber && churingNumber <= variables.poleSum + 1) {
                    //子音が決定するので計算
                    consonant = baseNumber - 1;
                    //子音と母音から再計算
                    setText = textSet[consonant, churingNumber - 1 + 1];
                    //Debug.Log(setText);
                    //次の状態へ
                    stage = 2;
                    //if (isGetKeyObjects)
                    //SetKeytext();

                    //副輪の再入力を認るときは以下をコメントアウト
                    stage = 3;
                    SystemCommandChuring();
                    InputText += setText;
                    SendText(InputText);
                    setText = "";
                }
            } else if (stage == 2) {
                //子音決定済み母音選択状態で、入力キー値が1～キー数の間の場合実行
                if (0 < churingNumber && churingNumber <= variables.poleSum + 1) {
                    setText = textSet[consonant, churingNumber - 1 + 1];
                    //if (isGetKeyObjects)
                    //SetKeytext();
                }
            } else if (stage == 3) {
                //副輪の再入力を認めない場合ここに来る
                //なにもしない
            } else if (stage == 4) {
                //stage4で、システムキー以外の接触のとき
                //なにもしない
            } else {
                Debug.LogWarning("Error. stage = " + stage + " ." +
                                 " churingNumber = " + churingNumber + " ." +
                                 " baseNumber = " + baseNumber);
            }
        }
        //もともと中心にいたわけではなく、中心へ戻った場合
        if (churingNumber == 0) {
            //まず、特殊なコマンドは実行する
            SystemCommandChuring();
            //表示用にわかりやすい名前に書き換える
            //ConvertToSystemCommand();
            //表示
            InputText += setText;
            SendText(InputText);
            //準備用の変数を初期化
            setText = "";
            //中心へ戻った
            stage = 0;
            //各テキストの初期化
            //if (isGetKeyObjects)
            //SetKeytext();

            //副輪を消す
            //subCircleGenerete(false);
            DisplaySubCircle(baseNumber, false);

            //主輪を戻す
            enableMainCircle(true);
        }
    }

    private void ChuringSystemRadially() {
        /* Exitイベント、副輪イベント処理 */
        /* churingNumber    意味
         * ***0             主輪の中心
         * ***1~**99        主輪のキー
         * *100             副輪の中心
         * *101~*199        副輪のキー
         * 1000~1199        各キーのExitイベント(解釈せずにreturnする)
         * -1               システムコマンド
         */
        if (churingNumber == 0) {
            //中心に触れた
            isTouchMainPillar = true;
        } else if (churingNumber == 1000) {
            //中心から離れた
            isTouchMainPillar = false;
        }

        //主輪の中心を触れながらキー入力はできない
        if (isTouchMainPillar) {
            if (1 <= churingNumber && churingNumber <= 99) {
                //return;
            }
        }

        //文字キーのExitは、0キー有効化のために使う
        if (1001 <= churingNumber && churingNumber <= 1099) {
            //Debug.Log("run");
            //0キーに触れているか
            //if (!isTouchMainPillar) {
            //    Debug.Log("run22");
            //触れていないなら（中心へ戻ったわけではない）0キーを有効化
            polygonalPillar.Enable(true);
            //}
        }
        if (100 <= churingNumber)
            return;

        /*Debug.Log("stage = " + stage + " . " +
                         "churingNumber = " + churingNumber + " . " +
                         "baseNumber = " + baseNumber + " .");
                         */
        if (churingNumber < 0) {
            //churingNumberがマイナス＝システムキーに触れたとき
            Debug.Log("run");
            //数値を反転し、システムキーの名前を参照する
            setText = SystemCommandName[-churingNumber];
            stage = 3;
        } else if (stage == 1 && baseNumber != churingNumber && churingNumber != 0) {
            //子音および母音選択状態で、最初のキー値と入力キー値が違い、中心に戻ったわけではない場合
            //子音が決定するので計算
            if (baseNumber == variables.poleSum && churingNumber == 1) {
                //最後のキーから1キーへの入力の際
                consonant = textSet.GetLength(0) - 1;
            } else if (baseNumber == variables.poleSum && ( 2 <= churingNumber && churingNumber <= baseNumber - 2 )) {
                //最後のキーから2~最後-2キーへの入力の際

            } else if (baseNumber == 1 && churingNumber == variables.poleSum) {
                //１キーから最後のキーへの入力の際
                consonant = 0;
            } else if (baseNumber == 1 && ( baseNumber + 2 <= churingNumber && churingNumber <= variables.poleSum - 1 )) {
                //１キーから3最後のキーへの入力の際

            } else if (baseNumber + 2 <= churingNumber || churingNumber <= baseNumber - 2) {
                //それ以外の隣り合わないキー

            } else {
                //それ以外の隣り合うキー値が同じ場合の計算
                consonant = ( ( baseNumber - 1 ) * 3 + 1 ) + ( churingNumber - baseNumber );
            }
            //子音と母音から再計算
            setText = textSet[consonant, churingNumber - 1 + 1];
            //次の状態へ
            stage = 2;
            if (isGetKeyObjects)
                SetKeyRadially();
        } else if (stage == 2 && 1 <= churingNumber && churingNumber <= variables.poleSum + 1) {
            //子音決定済み母音選択状態で、入力キー値が1～キー数の間の場合実行
            setText = textSet[consonant, churingNumber - 1 + 1];
            if (isGetKeyObjects)
                SetKeyRadially();
        } else if (stage == 0 && 0 < churingNumber && churingNumber <= variables.poleSum + 1) {
            //ニュートラル状態で、入力キー値が1～キー数の間の場合実行
            //最初のキー値を決定
            baseNumber = churingNumber;
            //とりあえず母音を保存
            setText = textSet[( baseNumber - 1 ) * 3 + 1, 0];
            //次の状態へ
            stage = 1;
            //主輪の0キーを無効化
            polygonalPillar.Enable(false);
            if (isGetKeyObjects)
                SetKeyRadially();
        } else if (( stage == 1 || stage == 2 || stage == 3 ) && churingNumber == 0) {
            //入力状態で、中心へ戻った場合
            //まず、特殊なコマンドは実行する
            SystemCommandChuring();
            //表示用にわかりやすい名前に書き換える
            //ConvertToSystemCommand();
            //表示
            InputText += setText;
            SendText(InputText);
            //準備用の変数を初期化
            setText = "";
            //中心へ戻った
            stage = 0;
            //各テキストの初期化
            if (isGetKeyObjects)
                SetKeyRadially();
        } else if (stage == 3) {
            //stage3で、システムキー以外の接触のとき
            //なにもしない
        } else {
            Debug.LogWarning("Error. stage = " + stage + " ." +
                             " churingNumber = " + churingNumber + " ." +
                             " baseNumber = " + baseNumber);
        }
    }

    //キーオブジェクトの取得
    IEnumerator GetKeyObjects() {
        if (variables.isCircleSystem) {
            keyObjects = new GameObject[/*poleSum*/textSet.GetLength(0) + 1];
        } else {
            keyObjects = new GameObject[textSet.GetLength(0) / 3 + 1];
        }
        for (int i = 0; i < keyObjects.Length; i++) {
            if (keyObjects[i] == null) {
                if (GameObject.Find(i.ToString())) {
                    keyObjects[i] = GameObject.Find(i.ToString());
                } else {
                    i--;
                    yield return null;
                }
            }
            if (i == variables.poleSum) {
                //Radiallyならここで文字の初期化
                if (!variables.isCircleSystem) {
                    SetKeyRadially();
                }
                isGetKeyObjects = true;
            }
        }
        //インスタンスの取得
        polygonalPillar = keyObjects[0].GetComponent<PolygonalPillar>();
        //副輪の一斉表示
        if (variables.isCircleSystem)
            subCircleGenerete();

    }

    /*
    private void GetKeyObjects() {
        keyObjects = new GameObject[variables.poleSum + 1];
        for (int i = 0; i <= variables.poleSum; i++) {
            if (keyObjects[i] == null) {
                if (GameObject.Find(i.ToString()))
                    keyObjects[i] = GameObject.Find(i.ToString());
                else
                    break;
            }
            if (i == variables.poleSum) {
                isGetKeyObjects = true;
            }
        }
    }*/

    //副輪の呼び出しと削除
    private void subCircleGenerete() {
        //主輪の各キーに属する副輪を生成する
        for (int i = 1; i <= /*variables.poleSum*/textSet.GetLength(0); i++) {
            //副輪のジェネレート
            GameObject SubCircle = new GameObject("subCircle" + i.ToString());
            //variables.createSourcePosition = keyObjects[i].transform.Find("text").transform.position;
            createTrapezoidPole subTrapezoid = SubCircle.AddComponent<createTrapezoidPole>();
            subTrapezoid.PositionObj = keyObjects[i].transform.Find("text").gameObject;
            subTrapezoid.SetCreateSorce(this.gameObject);
            subCircles[i] = new GameObject[textSet.GetLength(1)];
            subCircles[i][0] = SubCircle;
            //Debug.LogError("");
            /* 以下で副輪の取得などしたいが、createTrapezoidPoleやMultipleTrapezoidPoleらの処理が追いつかずエラーが出る
             * そのため、コルーチンで処理を行う
             */
            IEnumerator waitGenereteSubKeys = WaitGenereteSubKeys(i);
            StartCoroutine(waitGenereteSubKeys);
        }
    }

    IEnumerator WaitGenereteSubKeys(int poleNum) {
        //副輪のキーのオブジェクトを取得し、
        //母音からそれぞれのキー値を決定し、反映
        int subObjectsNum = GetTextSetItemNum(poleNum);//textSet.GetLength(1);
        //keySubObjects = new GameObject[subObjectsNum + 1];

        for (int i = 0; i <= subObjectsNum; i++) {
            try {
                //取得したいオブジェクトやその親がジェネレートされるまで処理しない
                if (subCircles[poleNum][0].transform.Find(( i ).ToString()).gameObject != null) {
                    //問題なければ取得する
                    //Debug.Log("取得中 " + i + " 番目");
                    subCircles[poleNum][i + 1] = subCircles[poleNum][0].transform.Find(( i ).ToString()).gameObject;
                    //Debug.Log("run");
                    //文字セット用
                    //keySubObjects[i] = subCircles[poleSum, i];
                }
                goto go;
            } catch (Exception e) {
                //Debug.LogWarning("例外処理発生：コルーチンを続行します");
                //for文が進まないようにロールバックする
                i--;
                //Debug.LogWarning("try failed : ///\n" + e.Message + "///\n" + e.TargetSite + "///\n" + e.StackTrace);
            }
            yield return null;

go:
            ;
            //Debug.Log("i = " + i);
        }

        //副輪のキーをまとめて非アクティブ化
        //Debug.Log("run = " + subCircles.GetLength(1));
        //for (int i = 1; i < subCircles.GetLength(0); i++) {
        /*back:
                    ;
                    try {
                        SetKeyCircle(i);
                        goto ok;
                    } catch {
                        i--;
                    }
                    yield return null;
                    goto back;
        ok:
                    ;*/
        //Debug.Log(GetTextSetItemNum(poleNum));
        for (int j = 1; j <= GetTextSetItemNum(poleNum) + 1; j++) {
            try {
                if (subCircles[poleNum][j].name == "0") {
                    //Debug.Log("check  0");
                    subCircles[poleNum][j].GetComponent<PolygonalPillar>().Enable(false);
                    //Debug.Log("run  0");
                } else {
                    //Debug.Log("check  "+  subCircles[poleNum][j].name );
                    //SetKeyCircle(i);
                    subCircles[poleNum][j].GetComponent<MultipleTrapezoidPole>().Enable(false);

                    //Debug.Log("run  "+  subCircles[poleNum][j].name );
                }

                goto next;
            } catch (Exception e) {
                j--;
                Debug.LogWarning("try failed : ///\n" + e.Message + "///\n" + e.TargetSite + "///\n" + e.StackTrace);
            }
            yield return null;
next:
            ;
            //                Debug.Log(j);
        }

        //}

        //hasSubCircle = true;
        //Debug.Log("run in the method");
        //SetKeytext();
        //SetKeyCircle(poleNum);

        //Debug.Log("召喚コルーチン終了");

    }


    //主輪のキー有効化と無効化
    private void enableMainCircle(bool doIt) {
        //有効にするか
        if (doIt) {
            //主輪を接触可能にし、色を通常に戻す
            for (int i = 1; i < keyObjects.Length; i++) {
                //当たり判定を戻す
                keyObjects[i].GetComponent<MultipleTrapezoidPole>().isActiveObj = true;
                //色を濃くする
                MeshRenderer meshrenderer = keyObjects[i].GetComponent<MeshRenderer>();
                meshrenderer.material = variables.material_TrapezoidPole_Normal;

                //LineRendererの表示を濃くする
                LineRenderer lineRenderer = keyObjects[i].GetComponent<LineRenderer>();
                lineRenderer.material = variables.material_LineRenderer;

                //文字を薄くする
                TextMesh textMeshRenderer = keyObjects[i].transform.Find("text").GetComponent<TextMesh>();
                textMeshRenderer.color = variables.material_Text.color;
            }
        } else {
            //主輪を接触不可にし、色を変える
            for (int i = 1; i < keyObjects.Length; i++) {
                //当たり判定を消す
                keyObjects[i].GetComponent<MultipleTrapezoidPole>().isActiveObj = false;
                //色を薄くする
                MeshRenderer meshrenderer = keyObjects[i].GetComponent<MeshRenderer>();
                meshrenderer.material = variables.material_TrapezoidPole_Normal_Nonactive;

                //LineRendererの表示を薄くする
                LineRenderer lineRenderer = keyObjects[i].GetComponent<LineRenderer>();
                lineRenderer.material = variables.material_LineRenderer_Nonactive;

                //文字を薄くする
                TextMesh textMeshRenderer = keyObjects[i].transform.Find("text").GetComponent<TextMesh>();
                textMeshRenderer.color = variables.material_Text_Nonactive.color;
            }
        }
    }

    //他文字セットなどのキーの解釈
    private void SystemCommandChuring() {
        if (setText == "英" || setText == "ａ") {
            if (variables.isCircleSystem) {
                //textSet = textSetAlphabetCircle;
                //variables.poleSum = textSet.GetLength(0);
                //システムの再描画
            } else {
                //textSet = textSetAlphabet;
                //variables.poleSum = textSet.GetLength(0);
            }
            setText = "";
        } else if (setText == "Ａ") {
            if (variables.isCircleSystem) {
                //textSet = textSetALPHABETCircle;
                //variables.poleSum = textSet.GetLength(0);
                //システムの再描画
            } else {
                //textSet = textSetALPHABET;
                //variables.poleSum = textSet.GetLength(0);
            }
            setText = "";
        } else if (setText == "かな") {
            if (variables.isCircleSystem) {
                //textSet = textSetHiraganaCircle;
                //variables.poleSum = textSet.GetLength(0);
                //システムの再描画
            } else {
                //textSet = textSetHiragana;
                //variables.poleSum = textSet.GetLength(0);
            }
            setText = "";
        } else if (setText == "カナ") {
            if (variables.isCircleSystem) {
                //textSet = textSetKatakanaCircle;
                //variables.poleSum = textSet.GetLength(0);
                //システムの再描画
            } else {
                //textSet = textSetKatakana;
                //variables.poleSum = textSet.GetLength(0);
            }
            setText = "";
        } else if (setText == "記/数") {
            if (variables.isCircleSystem) {
                //textSet = textSetSignNumCircle;
                //variables.poleSum = textSet.GetLength(0);
                //システムの再描画
            } else {
                //textSet = textSetSignNum;
                //variables.poleSum = textSet.GetLength(0);
            }
            setText = "";
        } else if (setText == "改/確") {
            //
            setText = "";
        } else if (setText == "空/変") {
            //
            setText = "";
        } else if (setText == "゛") {
            //本来の仕様は以下であるが、実験のためにコメントアウト
            /*int i = 0, j = 0;
            if (HaveRetranslationText(ref i, ref j)) {
                DeleteInputText(1);
                setText = RetranslationSet[i * 4 + 1, j];
            }*/
        } else if (setText == "゜") {
            //本来の仕様は以下であるが、実験のためにコメントアウト
            /*int i = 0, j = 0;
            if (HaveRetranslationText(ref i, ref j)) {
                DeleteInputText(1);
                setText = RetranslationSet[i * 4 + 2, j];
            }*/
        } else if (setText == "小") {
            int i = 0, j = 0;
            if (HaveRetranslationText(ref i, ref j)) {
                DeleteInputText(1);
                setText = RetranslationSet[i * 4 + 3, j];
            }
        } else if (setText == "BS") {
            DeleteInputText(1);
            setText = "";
        } else if (setText == "--") {
            setText = "";
        }
    }

    //直前の入力と再解釈用二次配列を照らし合わせていき、対象か否か判定。対象ならその配列番号を保存
    private bool HaveRetranslationText(ref int i, ref int j) {
        for (i = 0; i < RetranslationSet.GetLength(0) / 4; i++) {
            for (j = 0; j < RetranslationSet.GetLength(1) - 1; j++) {
                if (InputText.Substring(InputText.Length - 1) == RetranslationSet[i * 4, j]) {
                    return true;
                }

            }
        }
        return false;
    }

    //InputTextの先頭n文字を削除
    private void DeleteInputText(int n) {
        if (n <= InputText.Length) {
            InputText = InputText.Substring(0, InputText.Length - n);
        }
    }

    //入力された内容をシステムコマンドに変換する(setText内で完結させる)
    private void ConvertToSystemCommand() {
        if (setText == "--") {
            setText = "入力なし";
        } else if (setText == "改/確") {
            setText = "改行/確定";
        } else if (setText == "空/変") {
            setText = "空白/変換";
        } else if (setText == "記/数") {
            setText = "記号など";
        } else if (setText == "BS") {
            setText = "BackSpace";
        } else if (setText == "英") {
            setText = "英語";
        } else if (setText == "かな") {
            setText = "ひらがな";
        } else if (setText == "カナ") {
            setText = "カタカナ";
        } else if (setText == "小") {
            setText = "小文字";
        } else if (setText == "゛") {
            setText = "濁点";
        } else if (setText == "゜") {
            setText = "半濁点";
        } else if (setText == "ａ") {
            setText = "ａｂｃ";
        } else if (setText == "Ａ") {
            setText = "ＡＢＣ";
        }
    }
    /*
    //キーに文字を割り当てる
    private void SetKeytext() {
        if (!isCircleInterface) {
            SetKeyRadially();
        } else {
            SetKeyCircle();
        }
    }*/

            //RadiallyUI用文字割り当て
            private void SetKeyRadially() {
        MultipleTrapezoidPole keyObjectITrapezoid;
        for (int i = 1; i <= variables.poleSum; i++) {
            keyObjectITrapezoid = keyObjects[i].GetComponent<MultipleTrapezoidPole>();
            if (stage == 0) {
                keyObjectITrapezoid.MyText = textSet[i * 3 - 3, 0] + textSet[i * 3 - 2, 0] + textSet[i * 3 - 1, 0];
            } else if (stage == 1) {
                if (churingNumber - 1 == i) {
                    //左隣の値
                    keyObjectITrapezoid.MyText = textSet[( churingNumber - 1 ) * 3, 0];
                } else if (churingNumber == i) {
                    //現在座標の値
                    keyObjectITrapezoid.MyText = textSet[( churingNumber - 1 ) * 3 + 1, 0];
                } else if (churingNumber + 1 == i) {
                    //右隣の値
                    keyObjectITrapezoid.MyText = textSet[( churingNumber - 1 ) * 3 + 2, 0];
                } else if (churingNumber - ( variables.poleSum - 1 ) == i) {
                    //現在地(churingNumber)が端(最大値)の場合の右隣の値
                    keyObjectITrapezoid.MyText = textSet[( churingNumber - 1 ) * 3 + 2, 0];
                } else if (churingNumber + ( variables.poleSum - 1 ) == i) {
                    //現在地(churingNumber)が端(1)の場合の左隣の値
                    keyObjectITrapezoid.MyText = textSet[0, 0];
                } else {
                    keyObjectITrapezoid.MyText = "--";
                }
            } else if (stage == 2) {
                keyObjectITrapezoid.MyText = textSet[consonant, i];
            }
        }
    }

    //CircleUI用文字割り当て
    //20191102メソッド破棄
    /*private void SetKeyCircle(int poleNum) {

        MultipleTrapezoidPole keyObjectITrapezoid;
        int keySum;
        //主輪の話かどうか
        if (poleNum == 0) {
            keySum = keyObjects.Length;

            for (int i = 1; i < keySum; i++) {

                keyObjectITrapezoid = keyObjects[i].GetComponent<MultipleTrapezoidPole>();

                keyObjectITrapezoid.MyText = textSet[i - 1, 0];
            }
        } else {
            keySum = subCircles.GetLength(1);
            try {

                for (int i = 1; i < keySum; i++) {
                    keyObjectITrapezoid = subCircles[poleNum, i].GetComponent<MultipleTrapezoidPole>();
                    Debug.Log("run instance : " + keyObjectITrapezoid.transform.position);
                    //keyObjectITrapezoid.MyText = textSet[poleNum, i];
                    Debug.Log("run ok");
                }
            }catch(Exception e) {
                Debug.LogWarning("try failed : ///\n" + e.Message + "///\n" + e.TargetSite + "///\n" + e.StackTrace);

            }
        }

    }*/

    //副輪を表示する
    private void DisplaySubCircle(int CircleNum, bool disp) {
        for (int i = 1; i <= GetTextSetItemNum(CircleNum) + 1; i++) {
            try {
                //Debug.Log(subCircles[CircleNum, i].name);
                if (subCircles[CircleNum][i].name == "0") {
                    subCircles[CircleNum][i].GetComponent<PolygonalPillar>().Enable(disp);
                } else {
                    subCircles[CircleNum][i].GetComponent<MultipleTrapezoidPole>().Enable(disp);
                    //Debug.Log("Disp " + CircleNum);
                }
            } catch (Exception e) {
                i--;
                Debug.LogWarning("try failed : ///\n" + e.Message + "///\n" + e.TargetSite + "///\n" + e.StackTrace);
            }
        }

    }

    //CircleUI用
    public string tellKeyText(int keyNumber) {
        if (keyNumber < 100) {
            //主輪用
            return textSet[keyNumber - 1, 0];
        } else {
            //副輪用
            return textSet[(int)( keyNumber / 100 ) - 1, keyNumber % 100];
        }
    }

    //各キーから多角柱のEnterイベントを起こしていいか意見をもらう
    public void PolygonalPillerEnterEvent(bool opinion) {
        if (opinion == false) {
            //意見「ダメ」が出た時点で、ダメな状態にする
            canEnterPolygonalPiller = false;
        } else {
            if (canEnterPolygonalPiller == false) {
                //意見「良い」で、もともと「ダメ」な状態なら全員の意見を待つ
            } else {
                //意見「良い」で、もともと「良い」な状態なら何もしない
            }
        }

    }

    //現在のtextSetの任意の行の"Error"が出るまでのアイテム数を数える
    public int GetTextSetItemNum(int dan) {
        int length = 0;
        for (; length < textSet.GetLength(1); length++) {
            if (textSet[dan - 1, length] == "Error") {
                return length - 1;
            }
        }
        Debug.LogWarning("textSetにErrorアイテムが見つかりません");
        return length;
    }

    //文字出力メソッド
    private void SendText(string text) {
        if (typingSystem == null) {
            //
        } else {
            //実験用
            typingSystem.listenKeyEvent(text);
        }
    }

    public void EditInputText(string newText) {
        //テキストの更新
        InputText = newText;
        //表示
        SendText(InputText);
    }
}
