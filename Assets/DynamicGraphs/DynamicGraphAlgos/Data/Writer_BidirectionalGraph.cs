using System.Collections.Generic;
using GraphBasics;

namespace DynamicGraphAlgoImplementation
{
    public class Writer_BidirectionalGraph
    {
        public static string write(DirectedGraph<HistoryGraphVertex, HistoryGraphEdge> graph, string name, bool fixedPosition, bool invisLines)
        {
            string result = "graph " + name + " {\n";
            foreach (HistoryGraphVertex v in graph.GetVertices())
            {
                result += v.GetDotFormatLine(fixedPosition) + "\n";
            }

            foreach (HistoryGraphEdge e in graph.GetEdges())
            {
                result += e.GetDotFormatLine(invisLines) + "\n";
            }

            result += "}";

            
            return result;
        }
        
        public static string write(DirectedGraph<MasterVertex, MasterEdge> graph, string name, bool fixedPosition, bool invisLines)
        {
            string result = "graph " + name + " {\n";
            foreach (MasterVertex v in graph.GetVertices())
            {
                result += v.GetDotFormatLine(fixedPosition, 1.0, "", "") + "\n";
            }

            foreach (MasterEdge e in graph.GetEdges())
            {
                result += e.GetDotFormatLine(invisLines) + "\n";
            }

            result += "}";

            
            return result;
        }

        private static string GetBoundingBoxString(float minX, float maxX, float minZ, float maxZ)
        {
            string result = "";
            /*Stauchung auf 1/10
            result += "A [pos=\"";
            result += ((maxX+1)/10.0).ToString("0.####").Replace(",", ".") + ",";
            result += ((maxZ+1)/10.0).ToString("0.####").Replace(",", ".") + "!\"]\n";
            result += "B [pos=\"";
            result += ((maxX+1)/10.0).ToString("0.####").Replace(",", ".") + ",";
            result += ((minZ-1)/10.0).ToString("0.####").Replace(",", ".") + "!\"]\n";
            result += "C [pos=\"";
            result += ((minX-1)/10.0).ToString("0.####").Replace(",", ".") + ",";
            result += ((minZ-1)/10.0).ToString("0.####").Replace(",", ".") + "!\"]\n";
            result += "D [pos=\"";
            result += ((minX-1)/10.0).ToString("0.####").Replace(",", ".") + ",";
            result += ((maxZ+1)/10.0).ToString("0.####").Replace(",", ".") + "!\"]\n";*/
            result += "A [pos=\"";
            result += (maxX+1).ToString("0.####").Replace(",", ".") + ",";
            result += (maxZ+1).ToString("0.####").Replace(",", ".") + "!\"]\n";
            result += "B [pos=\"";
            result += (maxX+1).ToString("0.####").Replace(",", ".") + ",";
            result += (minZ-1).ToString("0.####").Replace(",", ".") + "!\"]\n";
            result += "C [pos=\"";
            result += (minX-1).ToString("0.####").Replace(",", ".") + ",";
            result += (minZ-1).ToString("0.####").Replace(",", ".") + "!\"]\n";
            result += "D [pos=\"";
            result += (minX-1).ToString("0.####").Replace(",", ".") + ",";
            result += (maxZ+1).ToString("0.####").Replace(",", ".") + "!\"]\n";

            return result;
        }
        
        public static string writeWithBox(DirectedGraph<HistoryGraphVertex, HistoryGraphEdge> graph, string name,
            bool fixedPosition, float minX, float maxX, float minZ, float maxZ, bool invisLines)
        {
            string result = "graph " + name + " {\n";

            result += "node [shape=circle, width = 1.0]\n";

            result += GetBoundingBoxString(minX, maxX, minZ, maxZ);

            foreach (HistoryGraphVertex v in graph.GetVertices())
            {
                result += v.GetDotFormatLine(fixedPosition) + "\n";
            }

            foreach (HistoryGraphEdge e in graph.GetEdges())
            {
                result += e.GetDotFormatLine(invisLines) + "\n";
            }

            result += "}";

            
            return result;
        }
        public static string writeWithBox(DirectedGraph<MasterVertex, MasterEdge> graph, string name,
            bool fixedPosition, float minX, float maxX, float minZ, float maxZ, bool invisLines)
        {
            string result = "graph " + name + " {\n";

            result += GetBoundingBoxString(minX, maxX, minZ, maxZ);

            foreach (MasterVertex v in graph.GetVertices())
            {
                result += v.GetDotFormatLine(fixedPosition, 1.0, "", "") + "\n";
            }

            foreach (MasterEdge e in graph.GetEdges())
            {
                result += e.GetDotFormatLine(invisLines) + "\n";
            }

            result += "}";

            
            return result;
        }

      /*  public static string writeStandard(BidirectionalGraph<HistoryGraphVertex, HistoryGraphEdge> graph)
        {
            return graph.ToGraphviz();
        }*/

        public static string WriteGraphsTransition(DirectedGraph<HistoryGraphVertex, HistoryGraphEdge> graph1,
            DirectedGraph<HistoryGraphVertex, HistoryGraphEdge> graph2, float minX, float maxX, float minZ,
            float maxZ, string name)
        {
            List<HistoryGraphVertex> oldAndNew = new List<HistoryGraphVertex>();
            List<HistoryGraphVertex> onlyOld = new List<HistoryGraphVertex>();
            List<HistoryGraphVertex> onlyNew = new List<HistoryGraphVertex>();
            
            foreach (HistoryGraphVertex v in graph1.GetVertices())
            {
                if (v.getNext() != null)
                {
                    oldAndNew.Add(v);
                }
                else
                {
                    onlyOld.Add(v);
                }
            }
            foreach (HistoryGraphVertex v in graph2.GetVertices())
            {
                if (v.getPrevious() == null)
                {
                    onlyNew.Add(v);
                }
            }
            
            string result = "graph " + name + " {\n";

            result += GetBoundingBoxString(minX, maxX, minZ, maxZ);

            foreach (HistoryGraphVertex v in onlyOld)
            {
                result += v.GetDotFormatLine(true, 0.7, "red", v.getName()+"0") + "\n";
            } 

            foreach (HistoryGraphVertex v in onlyNew)
            {
                result += v.GetDotFormatLine(true, 1.0, "green", v.getName()+"1") + "\n";
            }

            foreach (HistoryGraphVertex v in oldAndNew)
            {
                result += v.GetDotFormatLine(true, 0.7, "", v.getName() + "0") + "\n";
                if (v.getPosition() != v.getNext().getPosition())
                {
                    result += v.getNext().GetDotFormatLine(true, 1.0, "", v.getNext().getName() + "1") + "\n";
                }
            }

            foreach (HistoryGraphVertex v in oldAndNew)
            {
                if (v.getPosition() != v.getNext().getPosition())
                {
                    result += v.getName() + "0" + " -- " + v.getNext().getName() + "1;\n";
                }
            }

           

            result += "}";

            
            return result;
            
            
            
        }
    }
}