using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OSGI_Datatypes.ArchitectureElements;
using System.Linq;

namespace OSGI_Datatypes.OrganisationElements
{
    public class Project
    {
        //Meta-Info collection
        private Dictionary<int, Commit> commits;
        private Dictionary<string, Author> authors;
        private Dictionary<int, Branch> branches;
        private Dictionary<int, Issue> issues;

        //Architecture
        private List<BundleMaster> masterBundles;

        public Project()
        {
            commits = new Dictionary<int, Commit>();
            authors = new Dictionary<string, Author>();
            branches = new Dictionary<int, Branch>();
            issues = new Dictionary<int, Issue>();
            masterBundles = new List<BundleMaster>();
        }

        public void AddCommit(Commit c)
        {
            commits.Add(c.GetNeoId(), c);
        }
        public void AddAuthor(Author a)
        {
            authors.Add(a.GetToken(), a);
        }
        public void AddMasterBundles(BundleMaster b)
        {
            masterBundles.Add(b);
        }

        public Author GetAuthor(string token)
        {
            Author a;
            authors.TryGetValue(token, out a);
            return a;
        }

        public Commit GetCommit(int commitNeoId)
        {
            Commit c;
            commits.TryGetValue(commitNeoId, out c);
            return c;
        }

        public Branch GetBranch(int branchNeoId)
        {
            Branch b;
            branches.TryGetValue(branchNeoId, out b);
            return b; 
        }

        public Issue GetIssue(int issueNeoId)
        {
            Issue i;
            issues.TryGetValue(issueNeoId, out i);
            return i;
        }

        public Dictionary<int, Commit> GetCommits()
        {
            return commits;
        }

        public List<Commit> GetOrderedCommitList()
        {
            List<Commit> cList = commits.Values.ToList<Commit>();
            cList.Sort((c1, c2) => c1.CompareToByBranchFirst(c2));
            return cList;
        }

        public Dictionary<string, Author> GetAuthors()
        {
            return authors;
        }
        public Dictionary<int, Branch> GetBranches()
        {
            return branches;
        }
        public Dictionary<int, Issue> GetIssues()
        {
            return issues;
        }

        public List<BundleMaster> GetMasterBundles()
        {
            return masterBundles;
        }

        public List<PackageMaster> GetMasterPackages()
        {
            List<PackageMaster> mpList = new List<PackageMaster>();

            foreach(BundleMaster bm in masterBundles)
            {
                mpList.AddRange(bm.GetContainedMasterPackages());
            }

            return mpList;
        }

        public List<CompUnitMaster> GetMasterCompUnits()
        {
            List<CompUnitMaster> mcList = new List<CompUnitMaster>();

            foreach(PackageMaster pm in GetMasterPackages())
            {
                mcList.AddRange(pm.GetContainedMasterCompUnits());
            }

            return mcList;
        }

        public int GetMaxLocInProject()
        {
            int maxLoc = 0;
            foreach(BundleMaster bm in masterBundles)
            {
                int l = bm.GetMaxLoc();
                if(maxLoc < l)
                {
                    maxLoc = l;
                }
            }
            return maxLoc;
        }

    }
}