using System;
using System.Collections.Generic;
using UnityEngine;
using DynamicGraphAlgoImplementation.PositionInitializer;

namespace DynamicGraphAlgoImplementation
{
    public class MasterVertex : GraphVertex, IWeightedVertex
    {
        private static readonly System.Random R = new System.Random();

        private int ColorR { get; set; }
        private int ColorG { get; set; }
        private int ColorB { get; set; }

        private float weight;
        private float radius;

        private Dictionary<int, HistoryGraphVertex> historyVerticesMap;

        public MasterVertex(string n) : base(n)
        {

            //radius = 0.5f;
            radius = 1.5f * ((float)R.NextDouble()) + 0.5f;
            this.SetRandomColor();
            weight = 1;
            historyVerticesMap = new Dictionary<int, HistoryGraphVertex>();

        }
        public void SetRandomColor()
        {
            this.ColorR = R.Next(0, 255);
            this.ColorG = R.Next(0, 255);
            this.ColorB = R.Next(0, 255);
        }

        public void IncrementWeight()
        {
            weight++;
        }

        public void SetHistoryGraphVertexWithIndex(HistoryGraphVertex v, int index)
        {
            historyVerticesMap[index] = v;
        }
        public HistoryGraphVertex GetHistoryGraphVertexAtIndex(int index)
        {
            HistoryGraphVertex result;
            if (historyVerticesMap.TryGetValue(index, out result))
            {
                return result;
            }
            else {
                return null;
            }
        }

        public bool ExistsHistoryGraphVertexAtIndex(int index)
        {
            return historyVerticesMap.ContainsKey(index);
        }

        public float GetWeight()
        {
            return weight;
        }

        public override float getRadius()
        {
            return radius;
        }

        public string GetColorHexString(double intensity)
        {
            if (intensity > 1.0)
            {
                intensity = 1.0;
            }
            int red = (int)(ColorR * intensity);
            int green = (int)(ColorG * intensity);
            int blue = (int)(ColorB * intensity);

            string line = "#";
            line += red.ToString("X2");
            line += green.ToString("X2");
            line += blue.ToString("X2");

            return line;
        }

        public string GetColorHexString()
        {
            return GetColorHexString(1.0);
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

            line += " [fillcolor=\"" + GetColorHexString(intensity) + "\", ";

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

        public float GetR()
        {
            return ColorR / 255.0f;
        }
        public float GetG()
        {
            return ColorG / 255.0f;
        }
        public float GetB()
        {
            return ColorB / 255.0f;
        }
    }
}