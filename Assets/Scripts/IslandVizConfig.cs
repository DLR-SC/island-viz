using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class IslandVizConfig : MonoBehaviour
{
    public static IslandVizConfig Instance { get; private set; }

    public bool UseConfig = true; // You can disable this for faster testing.


    private void Awake()
    {
        Instance = this;
    }


    public IEnumerator LoadAndApplyConfig()
    {
        List<string> config;
        string line;

        StreamReader reader = new StreamReader(Application.dataPath + "/config.txt");
        config = new List<string>();
        try
        {
            while (reader.Peek() != -1)
            {
                line = reader.ReadLine();
                Debug.Log(line);
                string[] words = line.Split();
                if (words[0] != "#")
                {
                    string[] entries = line.Split('=');
                    config.Add(entries[1]);
                }
            }
        }
        catch
        {
            Debug.LogError("config file may be corrupt!");
        }
        finally
        {
            reader.Close();
        }

        // Data Loading        
        if (config[0] == "Json")
        {
            IslandVizData.Instance.DataLoading = IslandVizData.DataLoadingType.Json;
        }
        else if (config[0] == "Neo4J")
        {
            IslandVizData.Instance.DataLoading = IslandVizData.DataLoadingType.Neo4J;
        }
        else
        {
            Debug.LogError("IslandVizConfig: Could not resolve config entry for DataLoading!");
        }
        // Data Locations 
        if (IslandVizData.Instance.DataLoading == IslandVizData.DataLoadingType.Neo4J)
        {
            IslandVizData.Instance.Neo4J_URI = config[1];
        }
        else
        {
            IslandVizData.Instance.JsonDataPath = config[1];
        }
        // Neo4J user name
        IslandVizData.Instance.Neo4J_User = config[2];
        // Neo4J password
        IslandVizData.Instance.Neo4J_Password = config[3];
        // Procedural seed
        if (int.TryParse(config[4], out int seed))
        {
            IslandVizVisualization.Instance.RandomSeed = seed;
        }
        // Graph layout
        if (config[5] == "ForceDirected")
        {
            IslandVizVisualization.Instance.Graph_Layout = Graph_Layout.ForceDirected;
        }
        else if (config[5] == "Random")
        {
            IslandVizVisualization.Instance.Graph_Layout = Graph_Layout.Random;
        }
        else
        {
            Debug.LogError("IslandVizConfig: Could not resolve config entry for GraphLayout!");
        }

        yield return null;
    }

}


