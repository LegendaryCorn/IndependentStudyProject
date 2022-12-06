using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class FPSLogger : MonoBehaviour
{
    public int avgFrameRate;
    public int framesUntilLog = 1000;
    private string logToFilepath;

    public GameObject targetObject;

    private int _counter = 0;

    private void LogToFile(string line)
    {
        string path = logToFilepath;
        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine(line);
        writer.Close();
    }

    private void Awake()
    {
        logToFilepath = targetObject.name.ToLower() + "_fps_log.txt";
        LogToFile("row_type id frame_count time_since_level_load avg_frame_rate");
    }

    public void Update()
    {
        int current = (int)(1f / Time.unscaledDeltaTime);
        avgFrameRate = current;
        //displayText.text = avgFrameRate.ToString() + " FPS";

        if (Time.frameCount % framesUntilLog == 0 && _counter < 30)
        {
            if (logToFilepath != "")
            {
                LogToFile("fps " + _counter.ToString() + " " + Time.frameCount.ToString() + " " + Time.timeSinceLevelLoad.ToString() + " " + avgFrameRate.ToString());
                _counter += 1;
            }
        }
    }
}
