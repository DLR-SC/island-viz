using DatabasePreprocessing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Neo4JDriver;

public class DatabasePreprocessingScript : MonoBehaviour
{

    //Fields of canvas for user information
    [SerializeField]
    private Text taskTextfield;
    [SerializeField]
    private Text statusTextfield;
    [SerializeField]
    private Text loadingDotsTextfield;

    private Neo4J database;

    // Start is called before the first frame update
    void Start()
    {
        database = GameObject.Find("DatabaseObject").GetComponent<DatabaseAccess>().GetDatabase();
        Neo4JDatabasePreprocessing.SetDatabase(database);

        StartCoroutine(PreprocessingMain());
    }

    private void Update()
    {
        loadingDotsTextfield.color = new Color(loadingDotsTextfield.color.r, loadingDotsTextfield.color.g, loadingDotsTextfield.color.b, Mathf.PingPong(Time.time, 1));
    }


    private IEnumerator PreprocessingMain()
    {
        taskTextfield.text = "Check if database needs additional adjustments of is ready for visualization";
        statusTextfield.text = "";

        List<int> branchList = Neo4JDatabasePreprocessing.GetBranches();
        
        NeedForPrepro needForPrepro = new NeedForPrepro();
        needForPrepro.preproNeeded = false;

        if (branchList.Count == 0)
        {
            yield return Neo4JDatabasePreprocessing.CheckCommitsForChain(needForPrepro);
        }
        else
        {
            for(int i = 0; i<branchList.Count; i++)
            {
                yield return Neo4JDatabasePreprocessing.CheckBranchForChain(branchList[i], needForPrepro);
            }
        }

        if (!needForPrepro.preproNeeded)
        {
            taskTextfield.text = "No Adjustment needed, Continue with next Task";

        }
        else
        {
            taskTextfield.text = "Creating History Connections in Database";
            yield return Neo4JDatabasePreprocessing.PreprocessingMainRoutine(statusTextfield);
        }

        yield return null;
        statusTextfield.text = "Loading Next Step";
        SceneManager.LoadScene(1);
    }
   
   

}
