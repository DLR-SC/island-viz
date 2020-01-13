using Neo4j.Driver.V1;
using Neo4JDriver;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace DatabasePreprocessing
{
    public class Neo4JDatabasePreprocessing
    {

        private static Neo4J database;

        public static void SetDatabase(Neo4J neo)
        {
            database = neo;
        }

        public static List<int> GetBranches()
        {
            List<int> branchIds = new List<int>();
            string statement = "MATCH (b:BranchImpl) RETURN id(b)";

            IStatementResult result = database.ReadTransaktion(statement);
            var resList = result.ToList();

            for (int i = 0; i < resList.Count; i++)
            {
                branchIds.Add(resList[i]["id(b)"].As<int>());
            }
            return branchIds;
        }

        private static IEnumerator ProcessCommitChainStatementResult(IStatementResult result, List<Dictionary<string, int>> newConnectionsList)
        {
            var resList = result.ToList();

            for (int i = 0; i < resList.Count; i++)
            {
                var step = resList[i];
                if (i == 0)
                {
                    //First Element of Branch needs a Predecessor if it is branched out from parent Branch
                    if (step["id(cBranch)"] != null && step["id(cPred)"] == null)
                    {
                        int sourceId = step["id(cBranch)"].As<int>();
                        int targetId = step["id(c)"].As<int>();
                        newConnectionsList.Add(new Dictionary<string, int> { { "id1", sourceId }, { "id2", targetId } });
                    }
                }
                if (i > 0)
                {
                    //Every Element in Element in Chain needs a predecessor
                    if (step["id(cPred)"] == null)
                    {
                        int sourceId = resList[i - 1]["id(c)"].As<int>();
                        int targetId = step["id(c)"].As<int>();
                        newConnectionsList.Add(new Dictionary<string, int> { { "id1", sourceId }, { "id2", targetId } });
                    }
                }
                if (i == resList.Count - 1)
                {
                    //Last Element of Branch needs a Follower if it is merged back to parent branch
                    if (step["id(cMerge)"] != null && step["id(cFollow)"] == null)
                    {
                        int sourceId = step["id(c)"].As<int>();
                        int targetId = step["id(cMerge)"].As<int>();
                        newConnectionsList.Add(new Dictionary<string, int> { { "id1", sourceId }, { "id2", targetId } });
                    }
                }
                if (i % 5 == 0)
                {
                    yield return null;
                }
            }
        }

        public static IEnumerator CheckCommitsForChain(NeedForPrepro nfpObject)
        {
            yield return CheckBranchForChain(-1, nfpObject);
        }

        public static IEnumerator CheckBranchForChain(int branchId, NeedForPrepro nfpObject)
        {
            List<Dictionary<string, int>> newConnectionsList = new List<Dictionary<string, int>>();

            string statement = "";
            if(branchId == -1)
            {
                statement = "MATCH (c:CommitImpl) ";
            }
            else
            {
                statement = "MATCH (c:CommitImpl)-[:BRANCH]->(b:BranchImpl) " +
                "WHERE id(b)=$bid ";
            }
            statement += "OPTIONAL MATCH (cPred:CommitImpl)-[:NEXT]->(c) OPTIONAL MATCH (c)-[:NEXT]->(cFollow:CommitImpl) " +
                "OPTIONAL MATCH (cBranch:CommitImpl)-[:BRANCH]->(c) OPTIONAL MATCH (c)-[:MERGE]->(cMerge:CommitImpl) " +
                "RETURN id(c), id(cPred), id(cFollow), id(cBranch), id(cMerge) order by c.time"; 

            Dictionary<string, object> parameters = new Dictionary<string, object> { { "bid", branchId } };
            IStatementResult result = database.ReadTransaktion(statement, parameters);

            yield return ProcessCommitChainStatementResult(result, newConnectionsList);
            
            if (newConnectionsList.Count != 0)
            {
                nfpObject.preproNeeded = true;
                yield return WriteNextConnectionCommitLevel(newConnectionsList);
            }
           
        }

        private static IEnumerator WriteNextConnectionCommitLevel(List<Dictionary<string, int>> nodeIds)
        {
            string statement = "UNWIND $param as row " +
                "MATCH(n),(m)" +
                "WHERE id(n) = row.id1 AND id(m)= row.id2 " +
                "CREATE(n) -[:NEXT]->(m)";
            Dictionary<string, object> parameters = new Dictionary<string, object> { { "param", nodeIds } };

            IStatementResult result = database.ReadTransaktion(statement, parameters);

            yield return null;
        }

        public static IEnumerator PreprocessingMainRoutine()
        {
            yield return ConnectSameNameBundles();
            //statusTextfield.text = "20%";
            IslandVizUI.Instance.UpdateLoadingScreenUI("Creating History Connections", "20%"); // Update UI.

            yield return ConnectRenamedBundles();
            //statusTextfield.text = "40%";
            IslandVizUI.Instance.UpdateLoadingScreenUI("Creating History Connections", "40%"); // Update UI.

            yield return ConnectSameNamePackages();
            //statusTextfield.text = "60%";
            IslandVizUI.Instance.UpdateLoadingScreenUI("Creating History Connections", "60%"); // Update UI.

            yield return ConnectRenamedPackages();
            //statusTextfield.text = "80%";
            IslandVizUI.Instance.UpdateLoadingScreenUI("Creating History Connections", "80%"); // Update UI.

            yield return ConnectSameNameCompUnits();
            //statusTextfield.text = "100%";
            IslandVizUI.Instance.UpdateLoadingScreenUI("Creating History Connections", "100%"); // Update UI.

            yield return null;

        }

        #region Step1_SameNameBundles
        public static IEnumerator ConnectSameNameBundles()
        {
            string statement = "MATCH (c1:CommitImpl)-[:NEXT]->(c2:CommitImpl), (c1)-[:HAS]->(b1:BundleImpl), (c2)-[:HAS]->(b2:BundleImpl) " +
                "WHERE b1.symbolicName = b2.symbolicName " +
                "MERGE(b1) -[:NEXT]->(b2)";

            IStatementResult result = database.ReadTransaktion(statement);

            yield return null;
        }
        #endregion

        #region Step2_RenamedBundles
        public static IEnumerator ConnectRenamedBundles()
        {
            yield return ConnectSameNamePackagesUnconnectedBundles();
            //statusTextfield.text = "24%";
            IslandVizUI.Instance.UpdateLoadingScreenUI("Creating History Connections", "24%"); // Update UI.
            yield return RemoveDoubleSourceAtPackage();
            //statusTextfield.text = "28%";
            IslandVizUI.Instance.UpdateLoadingScreenUI("Creating History Connections", "28%"); // Update UI.
            yield return RemoveDoubleTargetAtPackage();
            //statusTextfield.text = "32%";
            IslandVizUI.Instance.UpdateLoadingScreenUI("Creating History Connections", "32%"); // Update UI.
            yield return ConnectRenamedBundlesIfPossible();
            //statusTextfield.text = "36%";
            IslandVizUI.Instance.UpdateLoadingScreenUI("Creating History Connections", "36%"); // Update UI.
            yield return RemoveWrongConnectionsAtPackage();
        }
        public static IEnumerator ConnectSameNamePackagesUnconnectedBundles()
        {
            string statement = "MATCH (c1:CommitImpl)-[:NEXT]->(c2:CommitImpl), (c1)-[:HAS]->(b1:BundleImpl), (c2)-[:HAS]->(b2:BundleImpl) " +
                 "WHERE not exists(() -[:NEXT]->(b2)) AND not exists((b1) -[:NEXT]->()) " +
                 "MATCH (b1)-[:USE]->(pf1:PackageFragmentImpl)-[:BELONGS_TO]->(p1: PackageImpl), (b2)-[:USE]->(pf2:PackageFragmentImpl)-[:BELONGS_TO]->(p2:PackageImpl) " +
                 "WHERE p1.name = p2.name " +
                 "MERGE (pf1)-[:NEXT]->(pf2)";

            IStatementResult result = database.ReadTransaktion(statement);

            yield return null;
        }

        public static IEnumerator RemoveDoubleSourceAtPackage()
        {
            string statement = "MATCH (p1:PackageFragmentImpl)-[nextRel:NEXT]->(p2:PackageFragmentImpl), (b1:BundleImpl)-[:USE]->(p1), (b2:BundleImpl)-[USE]->(p2) " +
                "WHERE not exists((b1)-[:NEXT]->(b2)) " +
                "WITH p1, count(p2) as p2Count, collect(id(nextRel)) as nextRelIds " +
                "WHERE p2Count > 1 " +
                "MATCH ()-[searchedRels]->() " +
                "WHERE id(searchedRels) in nextRelIds " +
                "DELETE searchedRels";

            IStatementResult result = database.ReadTransaktion(statement);

            yield return null;
        }
        public static IEnumerator RemoveDoubleTargetAtPackage()
        {
            string statement = "MATCH (p1:PackageFragmentImpl)-[nextRel:NEXT]->(p2:PackageFragmentImpl), (b1:BundleImpl)-[:USE]->(p1), (b2:BundleImpl)-[USE]->(p2) " +
                "WHERE not exists((b1) -[:NEXT]->(b2)) " +
                "WITH p2, count(p1) as p1Count, collect(id(nextRel)) as nextRelIds " +
                "WHERE p1Count > 1 " +
                "MATCH() -[searchedRels]->() " +
                "WHERE id(searchedRels) in nextRelIds " +
                "DELETE searchedRels";

            IStatementResult result = database.ReadTransaktion(statement);

            yield return null;
        }

        public static IEnumerator ConnectRenamedBundlesIfPossible()
        {
            string statement = "MATCH (c1:CommitImpl)-[:NEXT]->(c2:CommitImpl), (c1)-[:HAS]->(b1:BundleImpl), (c2)-[:HAS]->(b2:BundleImpl) " +
                "WHERE not exists(() -[:NEXT]->(b2)) AND not exists((b1) -[:NEXT]->()) " +
                "MATCH(b1)-[:USE]->(p1All: PackageFragmentImpl), (b2)-[:USE]->(p2All: PackageFragmentImpl), " +
                "(b1)-[:USE]->(p1Next: PackageFragmentImpl), (b2)-[:USE]->(p2Next: PackageFragmentImpl) " +
                "WHERE exists((p1Next)-[:NEXT]->(p2Next)) " +
                "WITH b1 as b1, b2 as b2, " +
                "count(distinct p1All) as p1AllCount, count(distinct p1Next) as p1NextCount, " +
                "count(distinct p2All) as p2AllCount, count(distinct p2Next) as p2NextCount " +
                "WHERE p1AllCount*0.5 < p1NextCount AND p2AllCount*0.5 < p2NextCount " +
                "MERGE(b1) -[:NEXT]->(b2)";

            IStatementResult result = database.ReadTransaktion(statement);

            yield return null;
        }

        public static IEnumerator RemoveWrongConnectionsAtPackage()
        {
            string statement = "MATCH (p1:PackageFragmentImpl)-[wrongRel:NEXT]->(p2:PackageFragmentImpl), (b1:BundleImpl)-[:USE]->(p1), (b2:BundleImpl)-[:USE]->(p2) " +
                "WHERE not exists((b1) -[:NEXT]->(b2)) " +
                "DELETE wrongRel";

            IStatementResult result = database.ReadTransaktion(statement);

            yield return null;
        }
        #endregion

        #region Step3_SameNamePackages
        public static IEnumerator ConnectSameNamePackages()
        {
            string statement = "MATCH (b1:BundleImpl)-[:NEXT]->(b2:BundleImpl), (b1)-[:USE]->(pf1:PackageFragmentImpl)-[:BELONGS_TO]->(p1:PackageImpl), " +
                "(b2)-[:USE]->(pf2:PackageFragmentImpl)-[:BELONGS_TO]->(p2:PackageImpl) " +
                "WHERE p1.qualifiedName = p2.qualifiedName OR p1.name=p2.name " +
                "MERGE(pf1)-[:NEXT]->(pf2)";

            IStatementResult result = database.ReadTransaktion(statement);

            yield return null;
        }
        #endregion

        #region Step4_RenamedPackages

        public static IEnumerator ConnectRenamedPackages()
        {
            yield return ConnectSameNameCompUnitsUnconnectedPackages();
            //statusTextfield.text = "64%";
            IslandVizUI.Instance.UpdateLoadingScreenUI("Creating History Connections", "64%"); // Update UI.
            yield return RemoveDoubleSourceAtCompUnit();
            //statusTextfield.text = "68%";
            IslandVizUI.Instance.UpdateLoadingScreenUI("Creating History Connections", "68%"); // Update UI.
            yield return RemoveDoubleTargetAtCompUnit();
            //statusTextfield.text = "72%";
            IslandVizUI.Instance.UpdateLoadingScreenUI("Creating History Connections", "72%"); // Update UI.
            yield return ConnectRenamedPackagesIfPossible();
            //statusTextfield.text = "76%";
            IslandVizUI.Instance.UpdateLoadingScreenUI("Creating History Connections", "76%"); // Update UI.
            yield return RemoveWrongConnectionsAtCompUnit();
        }

        public static IEnumerator ConnectSameNameCompUnitsUnconnectedPackages()
        {
            string statement = "MATCH (b1:BundleImpl)-[:NEXT]->(b2:BundleImpl), (b1)-[:USE]->(p1:PackageFragmentImpl), (b2)-[:USE]->(p2:PackageFramgmentImpl) " +
                "WHERE not exists(() -[:NEXT]->(p2)) AND not exists((p1) -[:NEXT]->()) " +
                "MATCH (p1)-[:HAS]->(cu1:CompilationUnitImpl)-[:CLASS]->(c1), (p2)-[:HAS]->(cu2:CompilationUnitImpl)-[:CLASS]->(c2) " +
                "WHERE (c1:ClassImpl OR c1:InterfaceImpl) AND(c2:ClassImpl OR c2: InterfaceImpl) " +
                "AND NOT exists(()-[:NESTED_TYPES]->(c1)) AND NOT exists(()-[:NESTED_TYPES]->(c2)) AND c1.name = c2.name " +
                "MERGE (cu1)-[:NEXT]->(cu2)";

            IStatementResult result = database.ReadTransaktion(statement);

            yield return null;
        }

        public static IEnumerator RemoveDoubleSourceAtCompUnit()
        {
            string statement = "MATCH (cu1:CompilationUnitImpl)-[nextRel:NEXT]->(cu2:CompilationUnitImpl), (p1:PackageFragmentImpl)-[:HAS]->(cu1), (p2:PackageFragmentImpl)-[:USE]->(cu2) " +
                "WHERE not exists((p1)-[:NEXT]->(p2)) " +
                "WITH cu1, count(cu2) as cu2Count, collect(id(nextRel)) as nextRelIds " +
                "WHERE cu2Count > 1 " +
                "MATCH()-[searchedRels]->() " +
                "WHERE id(searchedRels) in nextRelIds " +
                "DELETE searchedRels";

            IStatementResult result = database.ReadTransaktion(statement);

            yield return null;
        }
        public static IEnumerator RemoveDoubleTargetAtCompUnit()
        {
            string statement = "MATCH (cu1:CompilationUnitImpl)-[nextRel:NEXT]->(cu2:CompilationUnitImpl), (p1:PackageFragmentImpl)-[:HAS]->(cu1), (p2:PackageFragmentImpl)-[:USE]->(cu2) " +
                "WHERE not exists((p1)-[:NEXT]->(p2)) " +
                "WITH cu2, count(cu1) as cu1Count, collect(id(nextRel)) as nextRelIds " +
                "WHERE cu1Count > 1 " +
                "MATCH ()-[searchedRels]->() " +
                "WHERE id(searchedRels) in nextRelIds " +
                "DELETE searchedRels";

            IStatementResult result = database.ReadTransaktion(statement);

            yield return null;
        }

        public static IEnumerator ConnectRenamedPackagesIfPossible()
        {
            string statement = "MATCH (b1:BundleImpl)-[:NEXT]->(b2:BundleImpl), (b1)-[:USE]->(p1:PackageFragmentImpl), (b2)-[:USE]->(p2:PackageFragmentImpl) " +
                "WHERE not exists(() -[:NEXT]->(p2)) AND not exists((p1) -[:NEXT]->()) " +
                "MATCH(p1)-[:HAS]->(p1All: CompilationUnitImpl), (p2)-[:HAS]->(p2All: CompilationUnitImpl), " +
                "(p1)-[:HAS]->(p1Next: CompilationUnitImpl), (p2)-[:HAS]->(p2Next: CompilationUnitImpl) " +
                "WHERE exists((p1Next)-[:NEXT]->(p2Next)) " +
                "WITH p1 as p1, p2 as p2, " +
                "count(distinct p1All) as p1AllCount, count(distinct p1Next) as p1NextCount, " +
                "count(distinct p2All) as p2AllCount, count(distinct p2Next) as p2NextCount " +
                "WHERE p1AllCount*0.5 < p1NextCount AND p2AllCount*0.5 < p2NextCount " +
                "MERGE(p1) -[:NEXT]->(p2)";

            IStatementResult result = database.ReadTransaktion(statement);

            yield return null;
        }

        public static IEnumerator RemoveWrongConnectionsAtCompUnit()
        {
            string statement = "MATCH (cu1:CompilationUnitImpl)-[wrongRel:NEXT]->(cu2:CompilationUnitImpl), " +
                "(p1:PackageFragmentImpl)-[:HAS]->(cu1), (p2:PackageFragmentImpl)-[:HAS]->(cu2) " +
                "WHERE not exists((p1) -[:NEXT]->(p2)) " +
                "DELETE wrongRel";

            IStatementResult result = database.ReadTransaktion(statement);

            yield return null;
        }
        #endregion

        #region Step5_SameNameCompUnits
        public static IEnumerator ConnectSameNameCompUnits()
        {
            string statement = "MATCH (p1:PackageFragmentImpl)-[:NEXT]->(p2:PackageFragmentImpl), (p1)-[:HAS]->(cu1:CompilationUnitImpl)-[:CLASS]->(c1), " +
                "(p2)-[:HAS]->(cu2:CompilationUnitImpl)-[:CLASS]->(c2) " +
                "WHERE(c1: ClassImpl OR c1: InterfaceImpl) AND(c2: ClassImpl OR c2: InterfaceImpl) " +
                "AND NOT exists(() -[:NESTED_TYPES]->(c1)) AND NOT exists(() -[:NESTED_TYPES]->(c2)) " +
                "AND c1.qualifiedName = c2.qualifiedName " +
                "MERGE(cu1) -[:NEXT]->(cu2)";

            IStatementResult result = database.ReadTransaktion(statement);

            yield return null;
        }
        #endregion

    }

}
