using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrefabHouseTools
{
    public class Vertex
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
    public class Edge
    {
        public Vertex Begin { get; private set; }
        public Vertex End { get; private set; }
        public double Weight { get; }
        public override string ToString()
        {
            return string.Format(
                "Begin:{0}, End:{1}, Weight{2}",
                Begin.ToString(), End.ToString(),
                Weight.ToString());
        }
        public object Object { get; }
        public Edge
            (Vertex begin,Vertex end, 
            object linkedObj, double weight)
        {
            this.Begin = begin;
            this.End = end;
            this.Weight = weight;
            this.Object = linkedObj;
        }
        public Edge(Vertex begin,Vertex end,double weight)
        {
            this.Begin = begin;
            this.End = end;
            this.Weight = weight;
            this.Object = null;
        }

        public void Reverse()
        {
            Vertex temp = Begin;
            Begin = End;
            End = temp;
        }
    }
    public class Graph
    {
        private Dictionary<Vertex, List<Edge>> adjacentEdges;
        public int VertexCount 
        {
            get { return this.Vertices.Count(); } 
        }
        public Graph(List<Vertex> vertices)
        {
            this.Vertices = vertices;
            adjacentEdges = new Dictionary<Vertex, List<Edge>>();
        }
        public Graph()
        {
            Vertices = new List<Vertex>();
            adjacentEdges = new Dictionary<Vertex, List<Edge>>();
        }
        public List<Vertex> Vertices { get; private set; }
        public IEnumerable<Edge> Edges {
            get { return adjacentEdges.Values
                    .SelectMany(e => e).Distinct(); }}
        public int EdgeCount { 
            get { return this.Edges.Count(); }
        }
        private void ResetGraph()
        {
            Vertex[] verArray = this.Vertices.ToArray();
            for (int i = 0; i < VertexCount; i++)
            {
                verArray[i].Parent = verArray[i];
                verArray[i].Index = i;
                verArray[i].Rank = 0;
                verArray[i].Dist2Root = -1;
            }
        }
        public Graph Clone()
        {
            Graph copy = new Graph(this.Vertices);
            foreach (Edge e in this.Edges)
            {
                copy.AddEdge(e);
            }
            return copy;
        }
        public void AddVertex(Vertex vertex)
        {
            Vertices.Add(vertex);
        }
        public void AddVertices(List<Vertex> vertices)
        {
            this.Vertices.AddRange(vertices);
        }
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
        /// Up trace all the input vertex to the root and return 
        /// the collection of all vertex on the way.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public List<Vertex> UpTrace(IEnumerable<Vertex> inputV)
        {
            List<Vertex> upV = new List<Vertex>();
            Stack<Vertex> uncheck = new Stack<Vertex>();
            Vertex cur;
            foreach (Vertex v in inputV)
            {
                uncheck.Push(v);
            }
            while (uncheck.Count > 0)
            {
                cur = uncheck.Pop();
                if ((cur.Parent != cur)&&
                    (!uncheck.Contains(cur.Parent)))
                    uncheck.Push(cur.Parent);
                upV.Add(cur);
            }
            return upV;
        }
        public List<Vertex> UpTrace(Vertex inputV)
        {
            List<Vertex> vs = new List<Vertex> { inputV };
            return this.UpTrace(vs);
        }
        public Edge FindEdge(Vertex v1,Vertex v2)
        {
            foreach (Edge e in adjacentEdges[v1])
            {
                Vertex otherV = e.Begin == v1 ?
                    e.End : e.Begin;
                if (otherV == v2)
                    return e;
            }
            return null;
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
            ///Initialize the graph and root.
            Edge[] dt = new Edge[VertexCount - 1];
            this.ResetGraph();
            root.Dist2Root = 0;
            List<Vertex> vList = Vertices;
            ///Set current vertex to root.
            var curV = root;
            for (int i = 0; i < VertexCount -1 ; i++)
            {
                ///Find the vertex with shortest distance
                ///in the ramaining vertices.
                curV = vList
                        .Where(v => v.Dist2Root >= 0)
                        .OrderBy(v => v.Dist2Root)
                        .First() as Vertex;
                ///Update shortest distance of the adjacent vertex.
                List<Edge> es = adjacentEdges[curV];
                foreach (Edge e in es)
                {
                    Vertex adjV = e.Begin == curV ?
                                  e.End : e.Begin ;
                    if ((adjV.Dist2Root < 0) || 
                        (adjV.Dist2Root > curV.Dist2Root + e.Weight))
                    {
                        adjV.Dist2Root = curV.Dist2Root + e.Weight;
                        adjV.Parent = curV;
                    }
                }
                ///Remove the current vertex from remaining list.
                vList.Remove(curV);
            }
            return dt;
        }

        /// <summary>
        /// Using Dijkstra algorithm to generate the shortest route
        /// in the graph from start to end.The result is given in 
        /// vertex list form. Return true if a route is found.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="route"></param>
        /// <returns></returns>
        public bool DijkstraRoute(Vertex start,Vertex end,
            out List<Vertex>route)
        {
            ///Initialize the parameters and graph.
            bool result = false;
            route = new List<Vertex>();
            this.ResetGraph();
            ///Set start vertex as root.
            start.Dist2Root = 0;
            List<Vertex> vList = Vertices;
            var curV =start;
            for (int i = 0; i < VertexCount - 1; i++)
            {
                ///Find the vertex with shortest distance
                ///in the ramaining vertices.
                curV = vList
                        .Where(v => v.Dist2Root >= 0)
                        .OrderBy(v => v.Dist2Root)
                        .First() as Vertex;
                ///If the end vertex have shortest distance
                ///stop searching and generate result.
                if (curV == end)
                {
                    result = true;
                    route = this.UpTrace(curV);
                    route.Reverse();
                    break;
                }
                ///Update shortest distance of the adjacent vertex.
                List<Edge> es = adjacentEdges[curV];
                foreach (Edge e in es)
                {
                    Vertex adjV = e.Begin == curV ?
                                  e.End : e.Begin;
                    if ((adjV.Dist2Root < 0) ||
                        (adjV.Dist2Root > curV.Dist2Root + e.Weight))
                    {
                        adjV.Dist2Root = curV.Dist2Root + e.Weight;
                        adjV.Parent = curV;
                    }
                }
                ///Remove the current vertex from remaining list.
                vList.Remove(curV);
            }
            return result;
        }
    }
}
