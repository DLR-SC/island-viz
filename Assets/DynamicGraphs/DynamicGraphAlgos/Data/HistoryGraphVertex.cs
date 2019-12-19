using System;
using System.Globalization;
using UnityEngine;
using Random = System.Random;

namespace DynamicGraphAlgoImplementation
{
    public class HistoryGraphVertex : GraphVertex
    {
        private MasterVertex master;
        private HistoryGraphVertex next;
        private HistoryGraphVertex previous;
        private float radius;
        
        
        public HistoryGraphVertex(string n, bool isNewVertex) : base(n)
        {
            radius = -1;
            if (isNewVertex)
            {
                master = new MasterVertex(n);
                previous = null;
                next = null;
            }
        }

        public HistoryGraphVertex CloneAsFollowingVersion(bool initializeWithPosition)
        {
            HistoryGraphVertex follower = new HistoryGraphVertex(this.getName(), false);
            follower.setMaster(this.getMaster());
            
            if (initializeWithPosition)
            {
                follower.setPosition(new Vector3(this.getPosition().x, this.getPosition().y, this.getPosition().z));
            }
            follower.previous = this;
            this.next = follower;

            return follower;
        }

        
        public HistoryGraphVertex getNstepPrevious(int steps)
        {
            if (steps == 0)
            {
                return this;
            }else if (steps == 1)
            {
                return this.previous;
            }
            else
            {
                return this.getNstepPrevious(steps - 1);
            }
        }

        public HistoryGraphVertex getPrevious()
        {
            return previous;
        }

        public HistoryGraphVertex getNext()
        {
            return next;
        }

        public void SetPrevious(HistoryGraphVertex hgv)
        {
            previous = hgv;
        }
        public void SetNext(HistoryGraphVertex hgv)
        {
            next = hgv;
        }

        public MasterVertex getMaster()
        {
            return master;
        }

        public void setMaster(MasterVertex m)
        {
            master = m;
        }
        
        public override float getRadius()
        {
            if (radius == -1)
            {
                return master.getRadius();
            }
            return radius;
        }

        public void SetRadius(float r)
        {
            radius = r;
        }
        
        public string GetDotFormatLine(bool fixedPosition, double intensity, string border, string modifiedName)
        {
            string line;
            if (modifiedName.Equals(""))
            {
                line = this.getName();
            }
            else
            {
                line = modifiedName;
            }
                
            line += " [fillcolor=\"" + master.GetColorHexString(intensity) + "\", ";
           
            line += "style=\"filled\", ";

            switch (border)
            {
                case "red":
                    line += "color = red, ";
                    break;
                case "green":
                    line += "color = green, ";
                    break;
                default:
                    //Default = no special border color
                    break;
            }
            
            line += "pos=\"";
            /*Stauchung auf 1/10
            line += (this.getPosition().x/10.0f).ToString("0.####").Replace(",", ".") + ",";
            line += (this.getPosition().z/10.0f).ToString("0.####").Replace(",", ".");*/
            line += this.getPosition().x.ToString("0.####").Replace(",", ".") + ",";
            line += this.getPosition().z.ToString("0.####").Replace(",", ".");

            if (fixedPosition)
            {
                line += "!";
            }
            
            line += "\"];";
            
            return line;
        }

        public string GetDotFormatLine(bool fixedPosition)
        {
            return GetDotFormatLine(fixedPosition, 1.0, "", "");
        }
        
        
        
    }
}