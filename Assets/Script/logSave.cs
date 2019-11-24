//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
//using System;
using System.IO;

public class logSave : MonoBehaviour {

    private StreamWriter sw;
    private string filePath;
    private float deltaTime;

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
        filePath += "/log" + nowdate + " " + systemName + ".txt";
# elif UNITY_STANDALONE_WIN
        filePath += "/log" + nowdate + " " + systemName + ".txt";
# elif UNITY_ANDROID
        filePath = Application.persistentDataPath + "/log" + " " + systemName + ".txt";
# endif
        /* プラットホーム依存コンパイル ここまで*/

        sw = new StreamWriter(filePath, true);
        sw.WriteLine(nowdate);
        sw.Write("\tRealtime\t\tGameTime\t\t\t\tEvent");
        sw.Flush();
        sw.Close();
        deltaTime = NowTimeNum();
    }

    public void LogSaving(string log) {
        sw = new StreamWriter(filePath, true);
        sw.Write("\t" + NowTime() + "\t\t");
        sw.Write(Time.fixedTime.ToString() + "\t");
        sw.Write("DeltaTime\t" + ( NowTimeNum() - deltaTime ).ToString("N2") + "\t");
        deltaTime = NowTimeNum();
        sw.WriteLine(log);
        sw.Flush();
        sw.Close();
    }

    private string NowTime() {
        string dateTimeStr = System.DateTime.Now.Hour  .ToString()  + "-"
                           + System.DateTime.Now.Minute.ToString()  + "-"
                           + System.DateTime.Now.Second.ToString();
        return dateTimeStr;
    }

    private string Nowdate() {
        string dateTimeStr = System.DateTime.Now.Year  .ToString()  + "-"
                           + System.DateTime.Now.Month .ToString()  + "-"
                           + System.DateTime.Now.Day   .ToString()  + " "
                           + System.DateTime.Now.Hour  .ToString()  + "-"
                           + System.DateTime.Now.Minute.ToString()  + "-"
                           + System.DateTime.Now.Second.ToString();
        return dateTimeStr;
    }

    private float NowTimeNum() {
        float dateTimeFlo = System.DateTime.Now.Hour        * 3600
                          + System.DateTime.Now.Minute      * 60
                          + System.DateTime.Now.Second
                          + System.DateTime.Now.Millisecond * 0.001f;
        return dateTimeFlo;
    }
}
