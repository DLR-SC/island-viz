using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OSGI_Datatypes.OrganisationElements
{
    public class Issue
    {
        private int issueId;
        private string description;
        private List<Commit> implementingCommits;

        public Issue (int id, string descr)
        {
            issueId = id;
            description = descr;
            implementingCommits = new List<Commit>();
        }

        public int GetId()
        {
            return issueId;
        }
        public string GetDescription()
        {
            return description;
        }
        public void AddImplementingCommit(Commit c)
        {
            implementingCommits.Add(c);
        }
        public List<Commit> GetImplementingCommits()
        {
            return implementingCommits;
        }
    }
}