using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication2
{
    class Node
    {
        public int pictureY { get; set; }
        public int pictureX { get; set; }
        public List<Node> neighbours = new List<Node>();
        public String name { get; set; }
        public bool visited { get; set; } = false;
        public Node parent { get; set; } = null;
        public double distanceToBegin = double.MaxValue; 
        public Node(int pictureX, int pictureY)
        {
            this.pictureX = pictureX;
            this.pictureY = pictureY;
        }
        public double distanceTo(Node n)
        {
            return Math.Sqrt((n.pictureX - pictureX) * (n.pictureX - pictureX) + (n.pictureY - pictureY) * (n.pictureY - pictureY));
        }
        public string toString()
        {
            string res = "" + pictureX
                + "," + pictureY
                + "," + name;
            foreach(Node nei in neighbours)
            {
                res += "," + nei.name;
            }
            return res;
        }
    }
}
