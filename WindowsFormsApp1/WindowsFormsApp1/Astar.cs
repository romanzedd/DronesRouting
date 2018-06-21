using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public class Astar
    {
    }

    public interface WeightedGraph<L>
    {
        double Cost(Location a, Location b);
        IEnumerable<Location> Neighbors(Location id);
    }


    public struct Location
    {
        public readonly int x, y;
        public readonly int z;
        public Location(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }


    public class SquareGrid : WeightedGraph<Location>
    {

        public static readonly Location[] DIRS = new[]
            {
            new Location(1, 0, 0),
            new Location(0, -1, 0),
            new Location(-1, 0, 0),
            new Location(0, 1, 0),
            new Location(0, 0, 1),
            new Location(0, 0, -1)
        };

        public int length, width, height;
        public HashSet<Location> walls = new HashSet<Location>();
        public HashSet<Location> forests = new HashSet<Location>();

        public SquareGrid(int length, int width, int height)
        {
            this.length = length;
            this.width = width;
            this.height = height;
        }

        public bool InBounds(Location id)
        {
            return 0 <= id.x && id.x < length
                && 0 <= id.y && id.y < width
                && 0 <= id.z && id.z < width;
        }

        public bool Passable(Location id)
        {
            return !walls.Contains(id);
        }

        public double Cost(Location a, Location b)
        {
            return forests.Contains(b) ? 5 : 1;
        }

        public IEnumerable<Location> Neighbors(Location id)
        {
            foreach (var dir in DIRS)
            {
                Location next = new Location(id.x + dir.x, id.y + dir.y, id.z + dir.z);
                if (InBounds(next) && Passable(next))
                {
                    yield return next;
                }
            }
        }
    }


    public class PriorityQueue<T>
    {
        private List<Tuple<T, double>> elements = new List<Tuple<T, double>>();

        public int Count
        {
            get { return elements.Count; }
        }

        public void Enqueue(T item, double priority)
        {
            elements.Add(Tuple.Create(item, priority));
        }

        public T Dequeue()
        {
            int bestIndex = 0;

            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].Item2 < elements[bestIndex].Item2)
                {
                    bestIndex = i;
                }
            }

            T bestItem = elements[bestIndex].Item1;
            elements.RemoveAt(bestIndex);
            return bestItem;
        }
    }

    public class AStarSearch
    {
        public Dictionary<Location, Location> cameFrom
            = new Dictionary<Location, Location>();
        public Dictionary<Location, double> costSoFar
            = new Dictionary<Location, double>();

        static public double Heuristic(Location a, Location b)
        {
            return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y) - 1;
            //routing route = new routing();
            //return route.Haversine(a.x, b.x, a.y, b.y, a.z, b.z);
        }

        public AStarSearch(WeightedGraph<Location> graph, Location start, Location goal)
        {
            var frontier = new PriorityQueue<Location>();
            frontier.Enqueue(start, 0);

            cameFrom[start] = start;
            costSoFar[start] = 0;

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();

                if (current.Equals(goal))
                {
                    break;
                }

                foreach (var next in graph.Neighbors(current))
                {
                    double newCost = costSoFar[current]
                        + graph.Cost(current, next);
                    if (!costSoFar.ContainsKey(next)
                        || newCost < costSoFar[next])
                    {
                        costSoFar[next] = newCost;
                        double priority = newCost + Heuristic(next, goal);
                        frontier.Enqueue(next, priority);
                        cameFrom[next] = current;
                    }
                }
            }
        }

        public List<Location> ReconstructPath(Location start, Location goal, Dictionary<Location, Location> came_from)
        {
            List<Location> path = new List<Location>();
            Location current = goal;
            path.Add(current);
            while (current.x != start.x && current.y != start.y)
            {
                current = came_from[current];
                path.Add(current);
            }
            path.Add(start); 
            path.Reverse();
            return path;
        }
    }

    public class Test
    {
        public static string[] DrawGrid(SquareGrid grid, AStarSearch astar, Location start, Location goal)
        {
            string[] output = new string[4];
            // Печать массива cameFrom
           for (int z = 0; z < 4; z++)
            {
                for (var y = 0; y < 41; y++)
                {
                    for (var x = 0; x < 41; x++)
                    {
                        Location id = new Location(x, y, z);
                        Location ptr = id;
                        if (!astar.cameFrom.TryGetValue(id, out ptr))
                        {
                            ptr = id;
                        }
                        if (start.x == x && start.y == y && start.z == z) { /*Console.Write("\u2191 ");*/x++; output[z] += "S"; }
                        if (goal.x == x && goal.y == y && goal.z == z) { /*Console.Write("\u2191 ");*/x++; output[z] += "F"; }

                        if (grid.walls.Contains(id)) { /*Console.Write("##");*/ output[z] += "##"; }
                        else if (ptr.x == x + 1) { /*Console.Write("\u2192 ");*/ output[z] += "\u2192 "; }
                        else if (ptr.x == x - 1) { /*Console.Write("\u2190 ");*/ output[z] += "\u2190 "; }
                        else if (ptr.y == y + 1) { /*Console.Write("\u2193 ");*/ output[z] += "\u2193 "; }
                        else if (ptr.y == y - 1) { /*Console.Write("\u2191 ");*/ output[z] += "\u2191 "; }
                        else if (ptr.z == z + 1) { /*Console.Write("\u2193 ");*/ output[z] += "U"; }
                        else if (ptr.z == z - 1) { /*Console.Write("\u2191 ");*/ output[z] += "D"; }
                        else { /*Console.Write("* ");*/ output[z] += "* "; }
                    }
                    //Console.WriteLine();
                    output[z] += "\n";
                }
                //MessageBox.Show(output[z]);
            }
            return output;
        }
    }
}
