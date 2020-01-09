using OSGI_Datatypes.DataStructureCreation;
using OSGI_Datatypes.OrganisationElements;
using OSGI_Datatypes.ArchitectureElements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using Assets;
using Neo4JDriver;

public class DataLoading : MonoBehaviour
{
    public static DataLoading Instance { get { return instance; } }
    private static DataLoading instance; // The instance of this class.


    //Fields of canvas for user information
    /* [SerializeField]
     private Text taskTextfield;
     [SerializeField]
     private Text statusTextfield;
     [SerializeField]
     private Text loadingDotsTextfield;*/

    private Project project;
    private Neo4J database;

    void Start()
    {
        instance = this;
        database = GameObject.Find("DatabaseObject").GetComponent<DatabaseAccess>().GetDatabase();
        project = GameObject.Find("DataObject").GetComponent<OSGi_Project_Script>().GetProject();

        if (database == null)
        {
            Debug.LogError("No valid database");
            throw new MissingComponentException("No valid database");
        }
        if (project == null)
        {
            Debug.LogError("No data object available");
            throw new MissingComponentException("No valid project data object");
        }

        Neo4JReaderOrganisation.SetDatabase(database);
        Neo4JReaderContentDetails.SetDatabase(database);

        //Debug.Log("Successful framestart");
        //StartCoroutine(LoadingProject());
        //StartCoroutine(Test());
    }

    private IEnumerator Test()
    {
        yield return new WaitForSeconds(10);

        SceneManager.LoadScene(2);


    }

    private void Update()
    {
       // loadingDotsTextfield.color = new Color(loadingDotsTextfield.color.r, loadingDotsTextfield.color.g, loadingDotsTextfield.color.b, Mathf.PingPong(Time.time, 1));
    }

    public IEnumerator LoadingProject()
    {
        IslandVizUI.Instance.UpdateLoadingScreenUI("Loading Issues", ""); // Update UI.
        //taskTextfield.text = "Loading all Issue-Elements";
        //statusTextfield.text = "...";
        yield return Neo4JReaderOrganisation.ReadIssues(false, project.GetIssues());
        //statusTextfield.text = "Found " + project.GetIssues().Count + " Issues in Software Project";
        IslandVizUI.Instance.UpdateLoadingScreenUI("Issues Found", project.GetIssues().Count.ToString()); // Update UI.


        //taskTextfield.text = "Loading all Authors";
        //statusTextfield.text = "...\n" + "Found " + project.GetIssues().Count + " Issues in Software Project";
        //project.GetIssues().Count
        IslandVizUI.Instance.UpdateLoadingScreenUI("Loading Authors", ""); // Update UI.
        yield return Neo4JReaderOrganisation.ReadAuthor(false, project.GetAuthors());
        //statusTextfield.text = "Found " + project.GetAuthors().Count + " Authors in Software Project\n" +
        //    "Found " + project.GetIssues().Count + " Issues in Software Project";
        IslandVizUI.Instance.UpdateLoadingScreenUI("Authors Found", "0"); // Update UI.

        IslandVizUI.Instance.UpdateLoadingScreenUI("Loading Branches", ""); // Update UI.
        //taskTextfield.text = "Loading all Branches";
        //statusTextfield.text = "...\n" + "Found " + project.GetAuthors().Count + " Authors in Software Project\n";
        yield return Neo4JReaderOrganisation.ReadBranches(project.GetAuthors(), project.GetBranches(), false);
        //statusTextfield.text = "Found " + project.GetBranches().Count + " Branches in SoftwareProject\n" +
        //    "Found " + project.GetAuthors().Count + " Authors in Software Project";
        //taskTextfield.text = "Loading all Branches\n" + "Creating Branch Hierarchy";

        Branch master = Branch.getMasterBranch(project.GetBranches());
        master.SetHierarchy(0);
        IslandVizUI.Instance.UpdateLoadingScreenUI("Branches Found", project.GetBranches().Count.ToString()); // Update UI.


        IslandVizUI.Instance.UpdateLoadingScreenUI("Loading Commits", ""); // Update UI.
        //taskTextfield.text = "Loading Commits per Branch";
        //statusTextfield.text = "";
        yield return LoadCommitsOfBranch(master);
        //statusTextfield.text = "Totally " + project.GetCommits().Count + " commits in " + project.GetBranches().Count + " Branches in Project";
        IslandVizUI.Instance.UpdateLoadingScreenUI("Found Commits", project.GetCommits().Count.ToString()); // Update UI.


        //taskTextfield.text = "Loading detailed content of commits";
        //statusTextfield.text = "";

        //yield return LoadCommitsContents(master, true);
          yield return LoadCommitsContents_New(master, true, 1);


        Debug.Log("Total MasterBundles: " + project.GetMasterBundles().Count);
        Debug.Log("Total MasterPackages: " + project.GetMasterPackages().Count);
        Debug.Log("Total MasterCompUnits: " + project.GetMasterCompUnits().Count);

        Debug.Log("ferig");


        //Finisched Loading -> next Scene Creates HexLayout Of Islands
        //SceneManager.LoadScene(2);
    }

    private IEnumerator LoadCommitsOfBranch(Branch branch)
    {
        int commitsBefore = project.GetCommits().Count;
        //taskTextfield.text = "Loading Commits per Branch\n" + "Loading Commits of Branch " + branch.GetName();
        IslandVizUI.Instance.UpdateLoadingScreenUI("Loading Commits of Branch " + branch.GetName(), ""); // Update UI.
        yield return Neo4JReaderOrganisation.ReadCommits(project, branch, project.GetCommits(), false);
        //statusTextfield.text = "Found " + (project.GetCommits().Count - commitsBefore) + " Commits in Branch " + branch.GetName();
        IslandVizUI.Instance.UpdateLoadingScreenUI("Found Commits in Branch + branch.GetName()", (project.GetCommits().Count - commitsBefore).ToString()); // Update UI.


        foreach (Branch childBranch in branch.GetChildBranches())
        {
            yield return LoadCommitsOfBranch(childBranch);
        }
    }

    public IEnumerator LoadCommitsContents(Branch branch, bool isMaster)
    {
        //taskTextfield.text = "Loading detailed content of commits \n" +
        //    "Loading Commits of branch " + branch.GetName() + " (Total " + branch.GetCommits(false).Count + " Commits in Branch)";
        List<Commit> commits = branch.GetCommits(true);
        yield return null;

        //TODO hier anpassen um wieder gesamtes Project zu laden
        for (int i = 0; (i < commits.Count)&&i<2; i++)
        {
            //statusTextfield.text = "Loading " + (i + 1) + "th Commit\n";
            Commit c = commits[i];

            yield return LoadCommitsContent(c);
        }
        foreach (Branch b in branch.GetChildBranches())
        {
            yield return LoadCommitsContents(b, false);
        }
        yield return null;
    }

    public IEnumerator LoadCommitsContent(Commit c)
    {
        //Preparing Meta-Info
        Branch b = c.GetBranch();
        int branchLengt = b.GetCommits(false).Count;
        int commitIndex = b.GetIndexOfCommit(c);

        Dictionary<int, BundleElement> predBundleDict = null;
        Dictionary<int, BundleElement> nextBundleDict = null;

        if (c.GetPrevious(b) != null)
        {
            predBundleDict = c.GetPrevious(b).GetBundleDictionary();
        }
        if (!b.IsMaster() && commitIndex == branchLengt - 1)
        {
            if (c.GetNext(b) != null)
            {
                nextBundleDict = c.GetNext(b).GetBundleDictionary();
            }
        }

        //Read Bundles OfCommit;
        yield return Neo4JReaderContentDetails.ReadCommitsBundlesDetails(project, b, c, predBundleDict, nextBundleDict, null, true, false);
        //string text = statusTextfield.text.Split('\n')[0];
        //statusTextfield.text = text + "\n" + "Found " + c.GetBundles().Count + " Bundles in Commit";

        //ReadPackages of Bundle
        for (int i = 0; i < c.GetBundles().Count; i++)
        {
            BundleElement bundle = c.GetBundles()[i];
            Dictionary<int, PackageElement> prevPackDict = null;
            Dictionary<int, PackageElement> nextPackDict = null;
            if (bundle.GetPrevious(b) != null)
            {
                prevPackDict = bundle.GetPrevious(b).GetPackageDictionary();
            }
            if (!b.IsMaster() && commitIndex == branchLengt - 1)
            {
                if (c.GetNext(b) != null)
                {
                    nextPackDict = bundle.GetNext(b).GetPackageDictionary();
                }
            }

            yield return Neo4JReaderContentDetails.ReadBundlesPackageDetails(b, c, bundle, prevPackDict, nextPackDict, true, false);
            //string[] textblocks = statusTextfield.text.Split('\n');
            //statusTextfield.text = textblocks[0] + "\n" + textblocks[1] + "\n" + "Found " + bundle.GetPackages().Count + " Packages in " + (i + 1) + "th Bundle";

            //Read CompilationUnits of Package
            for (int j = 0; j < bundle.GetPackages().Count; j++)
            {
                PackageElement package = bundle.GetPackages()[j];
                Dictionary<int, CompUnitElement> prevCompUnitDict = null;
                Dictionary<int, CompUnitElement> nextCompUnitDict = null;
                if (package.GetPrevious(b) != null)
                {
                    prevCompUnitDict = package.GetPrevious(b).GetCompUnitDictionary();
                }
                if (!b.IsMaster() && commitIndex == branchLengt - 1)
                {
                    if (c.GetNext(b) != null)
                    {
                        nextCompUnitDict = package.GetNext(b).GetCompUnitDictionary();
                    }
                }

                yield return Neo4JReaderContentDetails.ReadPackagesCompUnitDetails(b, c, package, prevCompUnitDict, nextCompUnitDict, true, false);
                //textblocks = statusTextfield.text.Split('\n');
                //statusTextfield.text = textblocks[0] + "\n" + textblocks[1] + "\n" + textblocks[2] + "\n" + "Found " + package.GetCompUnits().Count + " CompUnits in " + (j + 1) + "th Package";
            }
        }

        yield return null;
    }


    public IEnumerator LoadCommitsContents_New(Branch branch, bool isMaster, int currentCommitNr)
    {
       // taskTextfield.text = "Loading detailed content of commits \n" +
       //     "Loading Commits of branch " + branch.GetName() + " (Total " + branch.GetCommits(false).Count + " Commits in Branch)";

        //Preparing Meta-Info
        List<Commit> commits = branch.GetCommits(true);
        int branchLengt = commits.Count;

        //Preparing Time-Connections & Result Dicts
        Dictionary<int, BundleElement> predBundleDict = null;
        Dictionary<int, BundleElement> thisBundleDict = new Dictionary<int, BundleElement>();
        Dictionary<int, BundleElement> nextBundleDict = null;
        Dictionary<int, PackageElement> predPackageDict = null;
        Dictionary<int, PackageElement> thisPackageDict = new Dictionary<int, PackageElement>();
        Dictionary<int, PackageElement> nextPackageDict = null;
        Dictionary<int, CompUnitElement> predCompUnitDict = null;
        Dictionary<int, CompUnitElement> thisCompUnitDict = new Dictionary<int, CompUnitElement>();
        Dictionary<int, CompUnitElement> nextCompUnitDict = null;

        int bCount;
        int pCount;
        int cCount;

        //TODO hier zweite Beschränkung entfernen
        for (int i = 0; (i < branchLengt)&&(i<8); i++)
        {
            IslandVizUI.Instance.UpdateLoadingScreenUI("Loading Commit Details", (currentCommitNr*100/(float)project.GetCommits().Count).ToString("0.00")+"%"); // Update UI.

            //statusTextfield.text = "Loading Commit " + (i + 1) + " of Branch";
            Commit commit = commits[i];
            if (i == 0 && !isMaster)
            {
                Commit prevCommit = commit.GetPrevious(branch);
                if (prevCommit != null)
                {
                    predBundleDict = prevCommit.GetBundleDictionary();
                    predPackageDict = prevCommit.GetPackageDictionary();
                    predCompUnitDict = prevCommit.GetCompUnitDictionary();
                }
            }
            if (i == branchLengt - 1 && !isMaster)
            {
                Commit nextCommit = commit.GetNext(branch);
                if (nextCommit != null)
                {
                    nextBundleDict = nextCommit.GetBundleDictionary();
                    nextPackageDict = nextCommit.GetPackageDictionary();
                    nextCompUnitDict = nextCommit.GetCompUnitDictionary();
                }
            }
            yield return null;

            //statusTextfield.text = "Loading Commit " + (i + 1) + "of Branch\n" +
            //    "Loading Bundles of Commit";
            yield return Neo4JReaderContentDetails.ReadCommitsBundlesDetails(project, branch, commit, predBundleDict, nextBundleDict, thisBundleDict, Constants.useValuesFromDBWherePossible, false);
            bCount = commit.GetBundleCount();


            //statusTextfield.text = "Loading Commit " + (i + 1) + "of Branch\n" +
            //    "Loading Packages of Commit\n" +
            //    "Found " + bCount + " Bundles in Commit";
            yield return Neo4JReaderContentDetails.ReadCommitsPackageDetails_New(branch, commit, predPackageDict, nextPackageDict, thisPackageDict, thisBundleDict, Constants.useValuesFromDBWherePossible, false);
            pCount = commit.GetPackageCount();

            //statusTextfield.text = "Loading Commit " + (i + 1) + "of Branch\n" +
            //    "Loading CompilationUnits of Commit\n" +
            //    "Found " + pCount + " Packages in Commit\n" + "Found " + bCount + " Bundles in Commit";
            yield return Neo4JReaderContentDetails.ReadCommitsCompUnitDetails_New(branch, commit, predCompUnitDict, nextCompUnitDict, thisCompUnitDict, thisPackageDict, Constants.useValuesFromDBWherePossible, false);
            cCount = commit.GetPackageCount();

            /*statusTextfield.text = "Loading Commit " + (i + 1) + "of Branch\n" +
               "Loading Import & Export Relations in Commit \n" +
               "Found " + cCount + " Compilation in Commit\n" +
               "Found " + pCount + " Packages in Commit\n";*/
            //TODO einkommentieren wenn richtiger Datensatz vorhanden
             yield return Neo4JReaderContentDetails.ReadCommitsImportRelations(commit, thisBundleDict, thisPackageDict, false);

             /*statusTextfield.text = "Loading Commit " + (i + 1) + "of Branch\n" +
                 "Loading Service Layer of Commit \n" +
                 "Finished Loading Import Relations";
            //TODO einkommentieren für Servicelayer 
            yield return Neo4JReaderContentDetails.ReadCommitsServiceLayer(commit, thisCompUnitDict, false);*/

            //statusTextfield.text = "Finished Loading Commit " + (i + 1) + "of Branch";

            predBundleDict = thisBundleDict;
            predPackageDict = thisPackageDict;
            predCompUnitDict = thisCompUnitDict;
            thisBundleDict = new Dictionary<int, BundleElement>();
            thisPackageDict = new Dictionary<int, PackageElement>();
            thisCompUnitDict = new Dictionary<int, CompUnitElement>();
            nextBundleDict = null;
            nextPackageDict = null;
            nextCompUnitDict = null;

            currentCommitNr++;
            yield return null;


        }
        foreach (Branch b in branch.GetChildBranches())
        {
            yield return LoadCommitsContents_New(b, false, currentCommitNr);

        }





    }

}
