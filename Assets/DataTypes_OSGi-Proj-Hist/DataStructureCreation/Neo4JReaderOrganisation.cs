using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OSGI_Datatypes.OrganisationElements;
using Neo4JDriver;
using Neo4j.Driver.V1;
using System.Linq;
using Assets;

namespace OSGI_Datatypes.DataStructureCreation
{

    public class Neo4JReaderOrganisation
    {
        public static Neo4J database;

        public static void SetDatabase(Neo4J neo)
        {
            database = neo;
        }

        public static  IEnumerator ReadIssues(bool printInfo, Dictionary<int, Issue> issueDict)
        {
            if (issueDict == null)
            {
                issueDict = new Dictionary<int, Issue>();
            }

            string statement = "MATCH(i: IssueImpl) RETURN id(i) AS iNeoId, i.message AS iMessage ORDER BY iNeoId";

            IStatementResult result = database.ReadTransaktion(statement);
            var resList = result.ToList();

            yield return null;

            for (int i = 0; i < resList.Count; i++)
            {
                var issueInfo = resList[i];
                int issueId = issueInfo["iNeoId"].As<int>();
                string desc = issueInfo["iMessage"].As<string>();
                issueDict.Add(issueId, new Issue(issueId, desc));
                yield return null;
            }
            if (printInfo)
            {
                Debug.Log(issueDict.Count + " Issues in Project");
            }
        }

        public static IEnumerator ReadBranches(Dictionary<string, Author> authorDict, Dictionary<int, Branch> branchDict, bool printInfo)
        {
            if (branchDict == null)
            {
                branchDict = new Dictionary<int, Branch>();
            }

            string statement = "MATCH(b: BranchImpl) OPTIONAL MATCH(b)-[:CREATOR]->(a: AuthorImpl) " +
                "OPTIONAL MATCH(b)-[:PARENT_BRANCH]->(p: BranchImpl) " +
                "OPTIONAL MATCH(c:BranchImpl)-[:PARENT_BRANCH]->(b) " +
                "RETURN id(b) AS bNeoId, b.name AS bName, b.time AS bCreationTime, a.token AS bCreator, id(p) AS bParent, collect(distinct id(c)) AS bChildren ORDER BY bParent DESC";

            IStatementResult result = database.ReadTransaktion(statement);
            var resList = result.ToList();

            yield return null;

            for (int i = 0; i < resList.Count; i++)
            {
                //Extrakt Infos from return array
                var issueInfo = resList[i];
                int id = issueInfo["bNeoId"].As<int>();
                string name = issueInfo["bName"].As<string>();
                long cTime = issueInfo["bCreationTime"].As<long>();
                string cToken = issueInfo["bCreator"].As<string>();
                

                //Branch Creator
                Author a;
                authorDict.TryGetValue(cToken, out a);

                //Create Branch
                Branch newBranch = new Branch(id, name, cTime, a);
                branchDict.Add(id, newBranch);
                if (a != null)
                {
                    a.AddBranch(newBranch);
                }

                //Set Parent if possible
                if (issueInfo["bParent"] != null)
                {
                    int parentId = issueInfo["bParent"].As<int>();
                                        Branch p;
                    branchDict.TryGetValue(parentId, out p);
                    if (p != null)
                    {
                        p.AddChildBranch(newBranch);
                        newBranch.SetParentBranch(p);
                    }
                }
                //SetChildBranches if possible
                if (issueInfo["bChildren"] != null)
                {
                    List<int> childIds = issueInfo["bChildren"].As<List<int>>();

                    foreach (int childId in childIds)
                    {
                        Branch c;
                        branchDict.TryGetValue(childId, out c);
                        if (c != null)
                        {
                            newBranch.AddChildBranch(c);
                            c.SetParentBranch(newBranch);
                        }
                    }
                }
                yield return null;
            }

            if (branchDict.Count > 0 && printInfo)
            {
                Debug.Log(branchDict.Count + " Branches in Project");
            }

            //Create Default MasterBranch if no branch exists
            if (branchDict.Count == 0)
            {
                Branch defMaster = new Branch(-1, "Default Master", -1, null);
                branchDict.Add(-1, defMaster);
                if (printInfo)
                {
                    Debug.Log("No Branch in Project, created default master");
                }
            }
            yield return null;
        }

        public static IEnumerator ReadAuthor(bool printInfo, Dictionary<string, Author> authorDict)
        {
            if(authorDict == null)
            {
                authorDict = new Dictionary<string, Author>();
            }
            string statement = "MATCH(a: AuthorImpl) RETURN id(a) AS aNeoId, a.name AS aName, a.token AS aToken, a.email AS aEmail";

            IStatementResult result = database.ReadTransaktion(statement);
            var resList = result.ToList();

            yield return null;

            for (int i = 0; i < resList.Count; i++)
            {
                var authorInfo = resList[i];
                int id = authorInfo["aNeoId"].As<int>();
                string name = authorInfo["aName"].As<string>();
                string token = authorInfo["aToken"].As<string>();
                string mail = authorInfo["aEmail"].As<string>();

                authorDict.Add(token, new Author(id, name, token, mail));
                yield return null;
            }

            if (printInfo)
            {
                Debug.Log(authorDict.Count + " Authors in project");
            }

        }

        public static IEnumerator ReadCommits(Project project, Branch branch, Dictionary<int, Commit> commitDict, bool printInfo)
        {
            if (branch.GetNeoId() == -1)
            {
                //branch id = -1 indicates default Masterbranch
                yield return ReadCommits(project, branch, commitDict, false, printInfo);
            }
            else
            {
                yield return ReadCommits(project, branch, commitDict, true, printInfo);
            }
        }
        
        public static IEnumerator ReadCommits(Project project, Branch branch, Dictionary<int, Commit> commitDict, bool considerbranching, bool printInfo)
        {
            int oldListLength = commitDict.Count;
            if(commitDict == null)
            {
                commitDict = new Dictionary<int, Commit>();

            }

            string statement = "";
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            if (!considerbranching)
            {
                statement += "MATCH (c:CommitImpl) ";
            }
            else
            {
                statement += "MATCH (c:CommitImpl)-[:BRANCH]->(b:BranchImpl) WHERE id(b) = $bId ";
                parameters.Add("bId", branch.GetNeoId());
            }

            statement += "OPTIONAL MATCH (c)-[:AUTHOR]->(a:AuthorImpl) " +
                "OPTIONAL MATCH (c)-[:ISSUE]->(i:IssueImpl) " +
                "OPTIONAL MATCH (cOld:CommitImpl)-[:NEXT]->(c) ";

            if (!considerbranching)
            {
                statement += "RETURN id(c) AS cNeoId, c.commitId AS cIdString, c.commitMessage AS cMessage, c.time AS cTime, " +
                "c.Author AS cAuthor, a.token AS cAuthorNode, " +
                "collect(distinct id(i)) AS cIssues, " +
                "id(cOld) AS cPrev, " +
                "c.graphLayouted, c.islandsLayouted " +
                "order by cTime";
            }
            else
            {
                statement += "OPTIONAL MATCH(cBranch:CommitImpl)-[:BRANCH]->(c) " +
                "OPTIONAL MATCH(c)-[:MERGE]->(cMerge: CommitImpl) " +
                "RETURN id(c) AS cNeoId, c.commitId AS cIdString, c.commitMessage AS cMessage, c.time as cTime, " +
                "c.Author as cAuthor, a.token AS cAuthorNode, " +
                "collect(distinct id(i)) as cIssues, " +
                "id(cOld) as cPrev, id(cBranch) as cBranchOrigin, id(cMerge) as cMergeTarget, " +
                "c.graphLayouted, c.islandsLayouted " +
                "order by cTime";
            }

            IStatementResult result;
            if (!considerbranching)
            {
                result = database.ReadTransaktion(statement);
            }
            else
            {
                result = database.ReadTransaktion(statement, parameters);
            }
            var resList = result.ToList();
            yield return null;

            for (int i = 0; i < resList.Count; i++)
            {
                var commitInfos = resList[i];
                int cId = commitInfos["cNeoId"].As<int>();
                string cIdString = commitInfos["cIdString"].As<string>();
                string cMess = commitInfos["cMessage"].As<string>();
                long cTime = commitInfos["cTime"].As<long>();
                string authorAsInfoInNode = commitInfos["cAuthor"].As<string>();
                string authorAsExtraNode = commitInfos["cAuthorNode"].As<string>();
                List<int> cIssues = commitInfos["cIssues"].As<List<int>>();

                //GetAuthor
                Author author = null;
                if (authorAsExtraNode != null)
                {
                    author = project.GetAuthor(authorAsExtraNode);
                }
                if (author == null && authorAsInfoInNode != null)
                {
                    author = project.GetAuthor(authorAsInfoInNode);
                    if(author == null)
                    {
                        //If author not yet present: create and add to project
                        author = new Author(-2, authorAsInfoInNode, authorAsInfoInNode, "unknown");
                        project.AddAuthor(author);
                    }
                }
                //Issue
                List<Issue> issues = new List<Issue>();
                foreach (int issId in cIssues)
                {
                    Issue iss = project.GetIssue(issId);
                    if (iss != null)
                    {
                        issues.Add(iss);
                    }
                }

                //New Commit
                Commit newCommit = new Commit(author, branch, cTime, cId, cIdString, cMess, issues);
                if (author != null)
                {
                    author.AddCommit(newCommit);
                }
                branch.AddCommit(newCommit);
                project.AddCommit(newCommit);
                foreach (Issue iss in issues)
                {
                    iss.AddImplementingCommit(newCommit);
                }

                //Connection to Previous
                if (commitInfos["cPrev"] != null)
                {
                    int cPrev = commitInfos["cPrev"].As<int>();
                    Commit prev = project.GetCommit(cPrev);
                    prev.AddNext(branch, newCommit);
                    newCommit.AddPrevious(branch, prev);
                }
                else if (i > 0)
                {
                    //With right preprocessing this should never occur
                    Debug.LogError("Error at OSGI_Datatypes.DataStructureCreateion.Neo4JReader - ReadCommits(): A commit that is not the first one has no previous commit");
                }
                if (considerbranching)
                {
                    if (i == 0 && branch.GetHierarchy() > 0)
                    {
                        if (commitInfos["cBranchOrigin"] != null)
                        {
                            //Branch replaces Previous for first commit in branch
                            int cBranch = commitInfos["cBranch"].As<int>();
                            Commit prev = project.GetCommit(cBranch);
                            prev.AddNext(branch, newCommit);
                            newCommit.AddPrevious(branch, prev);
                        }
                        else
                        {
                            Debug.LogError("Error at OSGI_Datatypes.DataStructureCreateion.Neo4JReader - ReadCommits(): First commit of a branch has no commit it is branched from");
                        }
                    }
                    if (i == resList.Count - 1)
                    {
                        //Last commit in branch has next by merging into parent branch
                        if (commitInfos["cMergeTarget"] != null)
                        {
                            int cMerge = commitInfos["cMerge"].As<int>();
                            Commit next = project.GetCommit(cMerge);
                            newCommit.AddNext(branch, next);
                            next.AddPrevious(branch, newCommit);
                        }
                        else if (branch.GetHierarchy() > 0)
                        {
                            Debug.LogWarning("Branch " + branch.GetName() + "is never merged to parent");
                        }
                    }
                }
                if (Constants.useValuesFromDBWherePossible)
                {
                    if (commitInfos["c.graphLayouted"] != null)
                    {
                        newCommit.SetIsGraphLayouted(commitInfos["c.graphLayouted"].As<bool>());
                    }
                    else
                    {
                        newCommit.SetIsGraphLayouted(false);
                    }
                    if(commitInfos["c.islandsLayouted"] != null)
                    {
                        newCommit.SetIsIslandsLayouted(commitInfos["c.islandsLayouted"].As<bool>());
                    }
                    else
                    {
                        newCommit.SetIsIslandsLayouted(false);
                    }
                }
                yield return null;
            }

            if (printInfo)
            {
                Debug.Log((commitDict.Count()-oldListLength) + " commits in branch " + branch.GetName());
            }
        }



 
    }

}