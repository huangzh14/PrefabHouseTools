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
        public double Dist2Root { get; set; }
        public object Object { get; }
        public Vertex(object linkedObj)
        {
            this.Object = linkedObj;
            this.Index = -1;
            this.Parent = this;
            this.Rank = 0;
            this.Dist2Root = -1;
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
        private void ResetGraph()
        {
            Vertex[] verArray = this.Vertices.ToArray();
            for (int i = 0; i < VertexCount; i++)
            {
                verArray[i].Parent = verArray[i];
                verArray[i].Index = i;
                verArray[i].Rank = 0;
            }
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
            Vertex e = edge.End;
            if (!adjacentEdges.ContainsKey(b))
                adjacentEdges.Add(b, new List<Edge>());
            if (!adjacentEdges.ContainsKey(e))
                adjacentEdges.Add(e, new List<Edge>());
            adjacentEdges[b].Add(edge);
            adjacentEdges[e].Add(edge);
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

        /// <summary>
        /// Using the Kruskal algorithm to generate a 
        /// minium span tree.
        /// </summary>
        /// 
        /// <returns>All the edges of the tree.</returns>
        public Edge[] KruskalMinTree()
        {
            Edge[] mst = new Edge[VertexCount - 1];

            //Step1:Sort all the edges in non-decending of weight
            var sortedEdges = this.Edges.OrderBy(edge => edge.Weight);
            var edgeEnumerator = sortedEdges.GetEnumerator();
            //Initializa the subtrees.
            this.ResetGraph();
            Vertex[] subtrees = this.Vertices.ToArray();
            
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

        /// <summary>
        /// Using Breadth First Search method to 
        /// traverse the graph.
        /// </summary>
        /// 
        /// <param name="root">
        /// The root vertex to start the traverse.</param>
        /// <returns>
        /// The edges of the traverse tree.</returns>
        public Edge[] BFS(Vertex root)
        {
            Edge[] bfs = new Edge[VertexCount - 1];
            int ei = 0;
            bool[] visited = new bool[VertexCount];
            this.ResetGraph();
            for (int i = 0; i < VertexCount; i++)
            {
                visited[i] = false;
            }
            Queue<Vertex> verQ = new Queue<Vertex>();
            verQ.Enqueue(root);
            visited[root.Index] = true;
            while (verQ.Count > 0)
            {
                Vertex curV = verQ.Dequeue();
                List<Edge> adjE = adjacentEdges[curV];
                foreach(Edge e in adjE)
                {
                    Vertex adjV = curV == e.Begin ? 
                                  e.End : e.Begin;
                    if (visited[adjV.Index] == false)
                    {
                        verQ.Enqueue(adjV);
                        visited[adjV.Index] = true;
                        bfs[ei++] = e;
                    }
                }
            }
            return bfs;
        }

        /// <summary>
        /// Using Dijkstra algorithm to create a tree in which
        /// all vertex has a shortest route to root.
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public Edge[] DijkstraTree(Vertex root)
        {
            Edge[] dt = new Edge[VertexCount - 1];
            this.ResetGraph();
            root.Dist2Root = 0;
            List<Vertex> vList = Vertices.ToList();
            for (int i = 0; i < VertexCount -1 ; i++)
            {
                var curV = vList
                        .Where(v => v.Dist2Root >= 0)
                        .OrderBy(v => v.Dist2Root)
                        .First() as Vertex;
                List<Edge> es = adjacentEdges[curV];
                foreach (Edge e in es)
                {
                    Vertex adjV = e.Begin == curV ?
                                  e.End : e.Begin ;
                    if (adjV.Dist2Root < 0)
                    {
                        adjV.Dist2Root = e.Weight;
                        adjV.Parent = curV;
                    }
                    else if (adjV.Dist2Root > curV.Dist2Root + e.Weight)
                    {
                        adjV.Dist2Root = curV.Dist2Root + e.Weight;
                        adjV.Parent = curV;
                    }
                }
                vList.Remove(curV);
            }
            return dt;
        }
    }
}
