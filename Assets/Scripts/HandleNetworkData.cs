using UnityEngine;
using System.Collections.Generic;
//using UnityEditor;
using System.IO;

public class HandleNetworkData : MonoBehaviour
{
    string readPath;
    string writePath;

    public List<string> stringList = new List<string>();
    public List<string> writeList = new List<string>();

    void Start()
    {
        readPath = Application.dataPath + "Assets/Resources/testText.txt.txt";
        writePath = Application.dataPath + "Assets/Resources/testText.txt.txt";
    }

    // Read from a text file
    void ReadFile(string filePath)
    {
        StreamReader sReader = new StreamReader(filePath);
        
        // Make sure a file to read from exists
        if (File.Exists(filePath))
        {
            // Loop through lines and read each line
            while (!sReader.EndOfStream)
            {
                string line = sReader.ReadLine();
                //string text = sReader.Read();
                stringList.Add(line);
            }
        }
        // End file streaming
        sReader.Close();
    }

    // Write to a text file -- This will overwrite any existing files
    void WriteFile(string filePath)
    {
        StreamWriter sWriter;

        // Ensure the file does exist
        if(!File.Exists(filePath))
        {
            // If file doesnt exist, create it
            sWriter = File.CreateText(Application.dataPath + "Assets/Resources/testText.txt");
        }
        else
        {
            // If does exist, write to filepath
            sWriter = new StreamWriter(filePath);
        }

        for(int k = 0; k < writeList.Count; k++)
        {
            // Write one line at a time
            sWriter.WriteLine(writeList[k]);
        }
        // End file streaming
        sWriter.Close();
    }

    // Add text to an existing file
    void AppendFile(string filePath)
    {
        StreamWriter sWriter;

        if(!File.Exists(filePath))
        {
            sWriter = File.CreateText(Application.dataPath + "Assets/Resources/testText.txt");
        }
        else
        {
            // Will not clear the file of current text
            sWriter = new StreamWriter(filePath, append: true);
        }
        for(int k = 0; k < writeList.Count; k++)
        {
            // Write line at a time
            sWriter.WriteLine(writeList[k]);
        }
        // End stream
        sWriter.Close();
    }

    /*tk
    public void SaveNetwork(NeuralNetwork test)
    {
        string test123;
        // Clear dataset from network
        //test.dataSet.Clear();
        test123 = test.ParseData();

        WriteFile(test123);

        Debug.Log("TEST)");
    }*/
}