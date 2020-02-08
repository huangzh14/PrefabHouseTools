using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrefabHouseTools
{
    class Vertex
    {
        public int Index { get; set; }
        public Vertex Parent { get; set; }
        public int Rank { get; set; }
        public object Object { get; }
        public Vertex(object linkedObj)
        {
            this.Object = linkedObj;
            this.Parent = this;
            this.Rank = 0;
            this.Index = -1;
        }
    }
    class Edge
    {
        public Vertex Begin { get; }
        public Vertex End { get; }
        public double Weight { get; }
        public override string ToString()
        {
            return string.Format(
                "Begin:{0}, End:{1}, Weight{2}",
                Begin.ToString(), End.ToString(),
                Weight.ToString());
        }
        public object Object { get; }
        public Edge(Vertex begin,Vertex end, object linkedObj, double weight)
        {
            this.Begin = begin;
            this.End = end;
            this.Weight = weight;
            this.Object = linkedObj;
        }
    }
    class Graph
    {
        private Dictionary<Vertex, List<Edge>> adjacentEdges
            = new Dictionary<Vertex, List<Edge>>();
        public int VertexCount { get; }
        public Graph(IEnumerable<Vertex> vertices)
        {
            this.Vertices = vertices;
            this.VertexCount = vertices.Count();
        }
        public IEnumerable<Vertex> Vertices { get; }
        public IEnumerable<Edge> Edges {
            get { return adjacentEdges.Values
                    .SelectMany(e => e); }}
        public int EdgeCount { 
            get { return this.Edges.Count(); } }
        public void AddEdge(Edge edge)
        {
            Vertex b = edge.Begin;
            if (!adjacentEdges.ContainsKey(b))
                adjacentEdges
                    .Add(b, new List<Edge>());
            adjacentEdges[b].Add(edge);
        }
        private Vertex FindRoot(Vertex[] subtrees,Vertex v)
        {
            Vertex r = subtrees[v.Index].Parent;
            if (r != v)
                r = FindRoot(subtrees, r);
            return r;
        }
        private void Union(Vertex[] subtrees,Vertex vx,Vertex vy)
        {
            Vertex xroot = FindRoot(subtrees, vx);
            Vertex yroot = FindRoot(subtrees, vy);
            int xrank = subtrees[xroot.Index].Rank;
            int yrank = subtrees[yroot.Index].Rank;
            //Attach smaller rank tree under root of higher tree.
            if (xrank < yrank)
                xroot.Parent = yroot;
            else if (xrank > yrank)
                yroot.Parent = xroot;
            else
            {
                yroot.Parent = xroot;
                xroot.Rank++;
            }
        }

        public Edge[] KruskalMinTree()
        {
            Edge[] mst = new Edge[VertexCount - 1];

            //Step1:Sort all the edges in non-decending of weight
            var sortedEdges = this.Edges.OrderBy(edge => edge.Weight);
            var edgeEnumerator = sortedEdges.GetEnumerator();
            //Initializa the subtrees.
            int n = this.VertexCount;
            Vertex[] subtrees = this.Vertices.ToArray();
            for (int i = 0; i < n; i++)
            {
                subtrees[i].Parent = subtrees[i];
                subtrees[i].Rank = 0;
                subtrees[i].Index = i;
            }

            //Number of edges should be V-1
            int e = 0;
            while (e < VertexCount - 1)
            {
                Edge nextEdge;
                if (edgeEnumerator.MoveNext())
                {
                    nextEdge = edgeEnumerator.Current;
                    Vertex vx = FindRoot(subtrees, nextEdge.Begin);
                    Vertex vy = FindRoot(subtrees, nextEdge.End);
                    //If the two vertex of the edge doesnt come to one root,
                    //then include it.
                    if (vx != vy)
                    {
                        mst[e] = nextEdge;
                        e++;
                        Union(subtrees, vx, vy);
                    }
                }
            }
            return mst;
        }
    }
}
