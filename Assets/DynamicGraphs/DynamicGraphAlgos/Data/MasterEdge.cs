using System;
using GraphBasics;

namespace DynamicGraphAlgoImplementation
{
    public class MasterEdge : DirectedEdge<MasterVertex>, IWeightedEdge
    {
        
        private float weight;
        private int appearanceCount;
        
        public MasterEdge(MasterVertex source, MasterVertex target) : base(source, target)
        {
            weight = 0f;
            appearanceCount = 0;
        }

        public MasterEdge(MasterVertex source, MasterVertex target, float inWeight) : base(source, target)
        {
            weight = inWeight;
        }
       
        public float getWeight()
        {
            return weight;
        }

        public void incrementAppearanceCount()
        {
            appearanceCount++;
        }

        public void incrementWeight(float i)
        {
            weight += i;
        }
        
        public string GetDotFormatLine(bool invis)
        {
            string line = this.GetSource().getName() + " -- " + this.GetTarget().getName();

            if (invis)
            {
                line += " [style=\"invis\"]";
            }

            line += ";";
            line += "// weight: " + String.Format("{0:0.00}", weight) + " count: " + appearanceCount; 
            return line;
        }
    }
}