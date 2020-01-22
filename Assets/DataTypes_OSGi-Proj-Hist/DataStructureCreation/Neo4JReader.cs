using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OSGI_Datatypes.OrganisationElements;
using Neo4JDriver;
using Neo4j.Driver.V1;
using System.Linq;

namespace OSGI_Datatypes.DataStructureCreation
{

    public class Neo4JReader
    {
        public static Neo4J database;

        public static void SetDatabase(Neo4J neo)
        {
            database = neo;
        }

        public static Dictionary<int, Issue> ReadIssues(bool printInfo)
        {
            Dictionary<int, Issue> issueDict = new Dictionary<int, Issue>();

            string statement = "MATCH(i: IssueImpl) RETURN id(i) AS iNeoId, i.message AS iMessage ORDER BY iNeoId";

            IStatementResult result = database.ReadTransaktion(statement);
            var resList = result.ToList();

            for (int i = 0; i < resList.Count; i++)
            {
                var issueInfo = resList[i];
                int issueId = issueInfo["iNeoId"].As<int>();
                string desc = issueInfo["iMessage"].As<string>();
                issueDict.Add(issueId, new Issue(issueId, desc));
            }
            if (printInfo)
            {
                Debug.Log(issueDict.Count + " Issues in Project");
            }

            return issueDict;
        }

        public static Dictionary<int, Branch> ReadBranches(Dictionary<string, Author> authorDict, bool printInfo)
        {
            Dictionary<int, Branch> branchDict = new Dictionary<int, Branch>();

            string statement = "MATCH(b: BranchImpl) OPTIONAL MATCH(b)-[:CREATOR]->(a: AuthorImpl) " +
                "OPTIONAL MATCH(b)-[:PARENT_BRANCH]->(p: BranchImpl) " +
                "OPTIONAL MATCH(c:BranchImpl)-[:PARENT_BRANCH]->(b) " +
                "RETURN id(b) AS bNeoId, b.name AS bName, b.time AS bCreationTime, a.token AS bCreator, id(p) AS bParent, collect(distinct id(c)) AS bChildren ORDER BY bParent DESC";

            IStatementResult result = database.ReadTransaktion(statement);
            var resList = result.ToList();

            for (int i = 0; i < resList.Count; i++)
            {
                //Extrakt Infos from return array
                var issueInfo = resList[i];
                int id = issueInfo["bNeoId"].As<int>();
                string name = issueInfo["bName"].As<string>();
                long cTime = issueInfo["bCreationTime"].As<long>();
                string cToken = issueInfo["bCreator"].As<string>();
                int parentId = issueInfo["bParent"].As<int>();
                List<int> childIds = issueInfo["bChildren"].As<List<int>>();

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
                Branch p;
                branchDict.TryGetValue(parentId, out p);
                if (p != null)
                {
                    p.AddChildBranch(newBranch);
                    newBranch.SetParentBranch(p);
                }
                //SetChildBranches if possible
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

            return branchDict;
        }

        public static Dictionary<string, Author> ReadAuthor(bool printInfo)
        {
            Dictionary<string, Author> authorDict = new Dictionary<string, Author>();

            string statement = "MATCH(a: AuthorImpl) RETURN id(a) AS aNeoId, a.name AS aName, a.token AS aToken, a.email AS aEmail";

            IStatementResult result = database.ReadTransaktion(statement);
            var resList = result.ToList();

            for (int i = 0; i < resList.Count; i++)
            {
                var authorInfo = resList[i];
                int id = authorInfo["aNeoId"].As<int>();
                string name = authorInfo["aName"].As<string>();
                string token = authorInfo["aToken"].As<string>();
                string mail = authorInfo["aEmail"].As<string>();

                authorDict.Add(token, new Author(id, name, token, mail));
            }

            if (printInfo)
            {
                Debug.Log(authorDict.Count + " Authors in project");
            }

            return authorDict;
        }

        public static Dictionary<int, Commit> ReadCommits(Project project, Branch branch, bool printInfo)
        {
            if (branch.GetNeoId() == -1)
            {
                //branch id = -1 indicates default Masterbranch
                return ReadCommits(project, branch, false, printInfo);
            }
            else
            {
                return ReadCommits(project, branch, true, printInfo);
            }
        }
        
        public static Dictionary<int, Commit> ReadCommits(Project project, Branch branch, bool considerbranching, bool printInfo)
        {
            Dictionary<int, Commit> commitDict = new Dictionary<int, Commit>();

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
                "id(cOld) AS cPrev " +
                "order by cTime";
            }
            else
            {
                statement += "OPTIONAL MATCH(cBranch:CommitImpl)-[:BRANCH]->(c) " +
                "OPTIONAL MATCH(c)-[:MERGE]->(cMerge: CommitImpl) " +
                "RETURN id(c) AS cNeoId, c.commitId AS cIdString, c.commitMessage AS cMessage, c.time as cTime, " +
                "c.Author as cAuthor, a.token AS cAuthorNode, " +
                "collect(distinct id(i)) as cIssues, " +
                "id(cOld) as cPrev, id(cBranch) as cBranchOrigin, id(cMerge) as cMergeTarget " +
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
                Commit newCommit = new Commit(author, branch, cTime, cId, cIdString, cMess, issues, i);
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
                        if (commitInfos["cBranch"] != null)
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
                        if (commitInfos["cMerge"] != null)
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
            }

            if (printInfo)
            {
                Debug.Log(commitDict.Count() + " commits in branch " + branch.GetName());
            }

            return commitDict;

        }



 
    }

}