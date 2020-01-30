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
        ArrayList lines = new ArrayList();
        string line;
        TextAsset textFile = (TextAsset)Resources.Load("Config", typeof(TextAsset));
        System.IO.StringReader textStream = new System.IO.StringReader(textFile.text);

        while ((line = textStream.ReadLine()) != null)
        {
            if (line.StartsWith("#"))
            {
                continue;
            }
            string[] entries = line.Split('=');
            if (entries[0] == "DataLoading")
            {
                if (entries[1] == "Json")
                {
                    IslandVizData.Instance.DataLoading = IslandVizData.DataLoadingType.Json;
                }
                else if (entries[1] == "Neo4J")
                {
                    IslandVizData.Instance.DataLoading = IslandVizData.DataLoadingType.Neo4J;
                }
                else
                {
                    Debug.LogError("IslandVizConfig: Could not resolve config entry for DataLoading!");
                }
            }
            else if (entries[0] == "DataLocation")
            {
                if (IslandVizData.Instance.DataLoading == IslandVizData.DataLoadingType.Neo4J)
                {
                    IslandVizData.Instance.Neo4J_URI = entries[1];
                }
                else
                {
                    IslandVizData.Instance.JsonDataPath = entries[1];
                }
            }
            else if (entries[0] == "UserName")
            {
                IslandVizData.Instance.Neo4J_User = entries[1];
            }
            else if (entries[0] == "Password")
            {
                IslandVizData.Instance.Neo4J_Password = entries[1];
            }
            else if (entries[0] == "Seed")
            {
                int seed = 0;

                if (int.TryParse(entries[1], out seed))
                {
                    IslandVizVisualization.Instance.RandomSeed = seed;
                }
                else
                {
                    Debug.LogError("IslandVizConfig: Could not resolve config entry for Seed!");
                }
            }
            else if (entries[0] == "GraphLayout")
            {
                if (entries[1] == "ForceDirected")
                {
                    IslandVizVisualization.Instance.Graph_Layout = Graph_Layout.ForceDirected;
                }
                else if (entries[1] == "Random")
                {
                    IslandVizVisualization.Instance.Graph_Layout = Graph_Layout.Random;
                }
                else
                {
                    Debug.LogError("IslandVizConfig: Could not resolve config entry for GraphLayout!");
                }
            }
        }
        textStream.Close();

        yield return null;
    }

}


