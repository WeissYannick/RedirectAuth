using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using HR;
using HR_Toolkit;
using UnityEngine;

public class LoggerRealHand :MonoBehaviour
{
    private RedirectionManager redirectionManager;

    private StreamWriter writer;
    private int frame = 0;
    private int entryNr = 0;

    public int logFrequencyHz = 10;

    void Start()
    {
        redirectionManager = RedirectionManager.instance;
        writer = new StreamWriter(GetPath());
        writer.WriteLine("EntryNr;" +
                        "Timestamp;" +
                        "Frame;" +
                        "Real Hand Position;");
        InvokeRepeating("Log", 0, 1.0f / logFrequencyHz);
    }

    private void Update() 
    {
        frame++;
    }

    private void Log()
    {
        entryNr++;
        writer.WriteLine(entryNr + ";" +
                        DateTime.Now + ";" +
                        frame + ";" +
                        redirectionManager.realHand.transform.position + ";");
    }

    private void OnApplicationQuit()
    {
        writer.Close();
        CancelInvoke("Log");
    }

    private static string GetPath() 
    {
        #if UNITY_EDITOR
        return Application.dataPath + "/RealHandLog.csv";
        #endif
    } 
}
