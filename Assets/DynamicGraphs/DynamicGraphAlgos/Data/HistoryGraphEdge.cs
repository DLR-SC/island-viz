using System;
using GraphBasics;

namespace DynamicGraphAlgoImplementation
{
    public class HistoryGraphEdge : DirectedEdge<HistoryGraphVertex>, IWeightedEdge
    {
        private float weight;
        private HistoryGraphEdge next;
        private HistoryGraphEdge previous;
        private MasterEdge master;
        
        public HistoryGraphEdge(HistoryGraphVertex source, HistoryGraphVertex target, bool isNew) : base(source, target)
        {
            if (isNew)
            {
                weight = 1f;
                next = null;
                previous = null;
                master = new MasterEdge(source.getMaster(), target.getMaster());
            }
        }

        public HistoryGraphEdge CloneAsFollowingVersion()
        {
            return this.CloneAsFollowingVersion(this.getWeight());
        }

        public HistoryGraphEdge CloneAsFollowingVersion(float initWeight)
        {
            HistoryGraphEdge follower = new HistoryGraphEdge(this.GetSource().getNext(), this.GetTarget().getNext(), false);
            this.weight = initWeight;
            this.next = follower;
            follower.previous = this;
            follower.master = this.master;
            return follower;
        }
       
        public float getWeight()
        {
            return weight;
        }

        public MasterEdge getMaster()
        {
            return master;
        }

        public HistoryGraphEdge getPrevious()
        {
            return previous;
        }

        public HistoryGraphEdge getNext()
        {
            return next;
        }
        
        
        public void incrementWeight(float i)
        {
            if (weight + i < 1.0)
            {
                weight = 1.0f;
            }
            else
            {
                weight += i;
            }
        }

        public void setWeight(float w)
        {
            if (w < 0.1)
            {
                Console.WriteLine("Set EdgeWeight 0");
            }
            if (w >= 1)
            {
                this.weight = w;
            }
        }
        
        public bool ContainsVertex(HistoryGraphVertex v)
        {
            if (this.GetTarget() == v || this.GetSource() == v)
            {
                return true;
            }

            return false;
        }

        public string GetDotFormatLine(bool invis)
        {
            string line = this.GetSource().getName() + " -- " + this.GetTarget().getName();

            if (invis)
            {
                line += " [style=\"invis\"]";
            }

            line += ";";
            line += "// weight: " + String.Format("{0:0.00}", weight); 
            
            return line;
        }

        internal void setMaster(MasterEdge edgeMaster)
        {
            master = edgeMaster;
        }
    }
}