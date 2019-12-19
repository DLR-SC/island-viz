using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using OSGI_Datatypes.OrganisationElements;

namespace OSGI_Datatypes.ComposedTypes
{
    public class Timeline<T>
    {
        Dictionary<Commit, T> timeline;

        public Timeline()
        {
            timeline = new Dictionary<Commit, T>();
        }

        public void Add(Commit c, T element)
        {
            if (timeline.ContainsKey(c)&& !timeline[c].Equals(element))
            {
                Debug.LogWarning("Trying to add another Element to occupied place in Timeline!");
            }
            else if (!timeline.ContainsKey(c))
            {
                timeline.Add(c, element);
            }
        }

        public T Get(Commit c)
        {
            T res;
            timeline.TryGetValue(c, out res);
            return res;
        }

        public Commit GetStart(SortTypes sortType)
        {
           return GetTimeline(sortType)[0];
        }

        public Commit GetEnd(SortTypes sortType)
        {
            List<Commit> list =  GetTimeline(sortType);
            Commit c = list[list.Count - 1];
            list = null;
            return c;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sortModus">0: not sorted, 1: sorted By time first, 2: sorted By Branch first</param>
        /// <returns></returns>
        public List<Commit> GetTimeline(SortTypes sortType)
        {
            List<Commit> commitList = timeline.Keys.ToList();
            if (sortType == SortTypes.byTime)
            {
                commitList.Sort();
            }
            else if(sortType == SortTypes.byBranch)
            {
                commitList.Sort((c1, c2) => c1.CompareToByBranchFirst(c2));
            }
            return commitList;
        }

        public Dictionary<Commit, T> GetDict()
        {
            return timeline;
        }

        public bool Contains(Commit c)
        {
            return timeline.ContainsKey(c);
        }

        public TimelineStatus RelationOfCommitToTimeline(Commit c)
        {
            if(Contains(c)){
                return TimelineStatus.present;
            }
            List<Commit> timelineTemp = GetTimeline(SortTypes.byTime);
            if (c.CompareTo(timelineTemp[0]) < 0)
            {
                return TimelineStatus.notYetPresent;
            }
            return TimelineStatus.notPresentAnymore;
            
        }
    }
}