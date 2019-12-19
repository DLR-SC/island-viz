using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace OSGI_Datatypes.OrganisationElements
{
    public class Author
    {
        int authorId;
        string name;
        string abreviation;
        string email;
        List<Commit> commits;
        List<Branch> branches;

        public Author(int id, string n, string token, string mail)
        {
            authorId = id;
            name = n;
            abreviation = token;
            email = mail;
            commits = new List<Commit>();
            branches = new List<Branch>();
        }
        public void AddCommit(Commit c)
        {
            commits.Add(c);
        }
        public void AddBranch(Branch b)
        {
            branches.Add(b);
        }

        public string GetToken()
        {
            return abreviation;
        }
    }
}