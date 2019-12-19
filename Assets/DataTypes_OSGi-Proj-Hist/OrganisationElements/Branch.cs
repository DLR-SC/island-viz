using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace OSGI_Datatypes.OrganisationElements
{
    public class Branch : IComparable
    {
        private int branchId;
        private string name;
        private Branch parentBranch;
        private List<Branch> childBranches;
        private Author creator;
        private long creationDate;
        private List<Commit> commits;
        int hierarchy;

        public Branch(int id, string n, long time, Author a)
        {
            branchId = id;
            name = n;
            creator = a;
            creationDate = time;
            childBranches = new List<Branch>();
            commits = new List<Commit>();
            hierarchy = -1;
        }

        public void SetParentBranch(Branch p)
        {
            if (parentBranch == null)
            {
                parentBranch = p;
            }
        }

        public void AddChildBranch(Branch c)
        {
            if (!childBranches.Contains(c))
            {
                childBranches.Add(c);
            }
        }

        public void AddCommit(Commit c)
        {
            commits.Add(c);
        }

        public Branch GetParentBranch()
        {
            return parentBranch;
        }
        
        public List<Branch> GetChildBranches(){
            return childBranches;
        }

        public void SetHierarchy(int level)
        {
            hierarchy = level;
            foreach(Branch childB in childBranches)
            {
                childB.SetHierarchy(level + 1);
            }
        }

        public int GetHierarchy()
        {
            return hierarchy;
        }

        public bool IsMaster()
        {
            if (hierarchy == 0)
            {
                return true;
            }
            else return false;
        }

        public static Branch getMasterBranch(Dictionary<int, Branch> branchDict)
        {
            Branch masterBranch = null;

            foreach(KeyValuePair<int, Branch> kvp in branchDict)
            {
                if(masterBranch ==null && kvp.Value.GetParentBranch() == null)
                {
                    masterBranch = kvp.Value;
                }
                else if(masterBranch == null && kvp.Value.GetParentBranch() == null)
                {
                    Debug.LogError("more than one branch with no parent -> no unique master branch available");
                }
            }

            return masterBranch;
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }
            if (obj is Branch)
            {
                Branch other = (Branch)obj;

                if(this.hierarchy == -1 && other.hierarchy == -1)
                {
                    return 0;
                }
                if(this.hierarchy == -1)
                {
                    return 1;
                }
                if(other.hierarchy == -1)
                {
                    return -1;
                }
                return this.hierarchy.CompareTo(other.GetHierarchy());
            }
            else
            {
                throw new NotImplementedException();
            }
        }


        #region Classic_Getter_Setter
        public int GetNeoId()
        {
            return branchId;
        }
        public string GetName()
        {
            return name;
        }

        public List<Commit> GetCommits(bool sort)
        {
            if (sort)
            {
                commits.Sort();
            }
            return commits;
        }

        public int GetIndexOfCommit(Commit c)
        {
            commits.Sort();
            return commits.IndexOf(c);
        }

        #endregion
    }




}