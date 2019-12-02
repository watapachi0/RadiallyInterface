//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
//using System;
using System.IO;

public class logSave : MonoBehaviour {

    private StreamWriter sw;
    private string filePath;
    private float deltaTime;
    private float deltaTimeF;

    void Start() {
        string nowdate = Nowdate();

        if (variables.logDirectory == "") {
            filePath = "C:/logSave";
        } else {
            filePath = variables.logDirectory;
        }

        Directory.CreateDirectory(filePath);
        string systemName;
        if (variables.isCircleSystem) {
            systemName = "Circle";
        } else {
            systemName = "Radially";
        }

        /* プラットホーム依存コンパイル */
# if UNITY_EDITOR
        filePath += "/log" + nowdate + " " + systemName + ".csv";
# elif UNITY_STANDALONE_WIN
        filePath += "/log" + nowdate + " " + systemName + ".csv";
# elif UNITY_ANDROID
        filePath = Application.persistentDataPath + "/log" + " " + systemName + ".csv";
# endif
        /* プラットホーム依存コンパイル ここまで*/

        sw = new StreamWriter(filePath, true, System.Text.Encoding.GetEncoding("utf-8"));
        //nowdate = nowdate.Replace("／", "/");
        //nowdate = nowdate.Replace("：", ":");
        sw.WriteLine(nowdate);
        string swStr = "\tRealtime\tGameTime\t\t\t\tEvent\n";

        sw.Write(swStr.Replace("\t", ","));
        sw.Flush();
        sw.Close();
        Debug.Log(swStr.Replace("\t", " "));
        deltaTime = NowTimeNum();
        deltaTimeF = Time.fixedTime;
    }

    public void LogSaving(string log, bool needDeltaCulc) {
        float ftime = Time.fixedTime;
        sw = new StreamWriter(filePath, true);
        string swStr = "";
        swStr += "\t" + NowTime()/*.Replace("：", ":")*/ + "\t";
        //swStr += Time.fixedTime.ToString() + "\t";
        if (needDeltaCulc) {
            swStr += "DeltaTime\t" + ( NowTimeNum() - deltaTime ).ToString("N2") + "\t"+1.234f.ToString()+"\t";
            swStr += "FixedTime\t" + ( ftime - deltaTimeF ).ToString("N2") + "\t";
        } else {
            swStr += "DeltaTime\t" + "none" + "\t";
            swStr += "FixedTime\t" + "none" + "\t";
        }
        swStr += log + "\n";
        if (needDeltaCulc) {
            deltaTime = NowTimeNum();
            deltaTimeF = ftime;
        }

        sw.Write(swStr.Replace("\t", ","));
        sw.Flush();
        sw.Close();
        Debug.Log(swStr.Replace("\t", " "));
    }

    private string NowTime() {
        string dateTimeStr = System.DateTime.Now.Hour.ToString("D2") + "-"
                           + System.DateTime.Now.Minute.ToString("D2") + "-"
                           + System.DateTime.Now.Second.ToString("D2") + "."
                           + System.DateTime.Now.Millisecond.ToString("D3");
        return dateTimeStr;
    }

    private string Nowdate() {
        string dateTimeStr = System.DateTime.Now.Year.ToString("D4") + "／"
                           + System.DateTime.Now.Month.ToString("D2") + "／"
                           + System.DateTime.Now.Day.ToString("D2") + " "
                           + System.DateTime.Now.Hour.ToString("D2") + "："
                           + System.DateTime.Now.Minute.ToString("D2") + "："
                           + System.DateTime.Now.Second.ToString("D2");
        return dateTimeStr;
    }

    private float NowTimeNum() {
        float dateTimeFlo = System.DateTime.Now.Hour * 3600
                          + System.DateTime.Now.Minute * 60
                          + System.DateTime.Now.Second
                          + System.DateTime.Now.Millisecond * 0.001f;
        return dateTimeFlo;
    }
}
