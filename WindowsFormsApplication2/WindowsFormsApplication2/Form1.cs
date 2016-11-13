using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace WindowsFormsApplication2
{
    public partial class Form1 : Form
    {
        Graphics g;
        private string path = ".\\file.txt";
        int nodeRadius = 10;
        int equivalenceThreshold = 4;// how many pixels of difference does not matter for our equivalence function.
        List<double> distances;
        List<double> angels;
        public Form1()
        {
            InitializeComponent();
            //System.Windows.Forms.Cursor.Current = new Cursor("C:\\Users\\Neu\\Documents\\visual studio 2015\\Projects\\WindowsFormsApplication2\\WindowsFormsApplication2\\resources\\circlespin\\Circle.cur");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                setAccess();
            } catch(Exception)
            {
            }
            //Image img = Image.FromFile("C:\\Users\\Neu\\Desktop\\Lenna.png");
            //pictureBox.Image = Image.FromFile("C:\\Users\\Neu\\Desktop\\Lenna.png");
        }

        
        List<Node> nodes = new List<Node>();
        Pen myPen;
        Node selectedNode = null;
        
        private void pictureBox_Paint(object sender, PaintEventArgs e)
        {
            g = e.Graphics;
            myPen = new System.Drawing.Pen(System.Drawing.Color.DarkCyan,3);
            //Console.WriteLine(" paint event:");
            foreach(Node n in nodes)
            {
                //Console.WriteLine(n.pictureX + " : " + n.pictureY);
                myPen.Color = System.Drawing.Color.DarkCyan;
                myPen.Width = 3;
                Point adjustedNcoor = transferFromImageCoordination(n.pictureX, n.pictureY);
                g.DrawEllipse(myPen, adjustedNcoor.X - 10, adjustedNcoor.Y - 10, 20, 20);
                g.DrawString(n.name, new System.Drawing.Font("Arial", 16), new System.Drawing.SolidBrush(System.Drawing.Color.Black), adjustedNcoor);
                myPen.Color = System.Drawing.Color.Black;
                myPen.Width = 2;
                foreach (Node neigh in n.neighbours)
                {
                    g.DrawLine(myPen, adjustedNcoor, transferFromImageCoordination(neigh.pictureX, neigh.pictureY));
                }
            }
            if(selectedNode != null)
            {
                Point selectedAdjustedCoor = transferFromImageCoordination(selectedNode.pictureX, selectedNode.pictureY);
                myPen = new System.Drawing.Pen(System.Drawing.Color.DarkRed, 2);
                g.DrawEllipse(myPen, selectedAdjustedCoor.X - 13, selectedAdjustedCoor.Y - 13, 26, 26);
            }
            this.Refresh();
        }
        Point mouseDown;
        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            //Console.WriteLine("size:" + getDisplayedImageSize(pictureBox).Height + " , " + getDisplayedImageSize(pictureBox).Width);
            mouseDown = transferToImageCoordination(e.X, e.Y);
            //Console.WriteLine("mouse down on image:" + transferToImageCoordination(e.X, e.Y));
        }
        Boolean equivalentPoints(int x1, int y1, int x2, int y2)
        {
            if(Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2)) < equivalenceThreshold)
            {
                return true;
            }
            return false;
        }
        private void pictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            Point adjustedE = transferToImageCoordination(e.X, e.Y);
            if(equivalentPoints(mouseDown.X, mouseDown.Y, adjustedE.X, adjustedE.Y))
            {//click
                Node myTempNode = getNode(adjustedE.X, adjustedE.Y);
                if (myTempNode == null)
                {//empty space
                    nodes.Add(new Node(adjustedE.X, adjustedE.Y)); //create new node
                    selectedNode = getNode(adjustedE.X, adjustedE.Y);//select it
                }
                else
                {//in node
                    selectedNode = myTempNode;// select node
                }
            }
            else
            {//drag
                selectedNode = null;
                Node n1 = getNode(mouseDown.X, mouseDown.Y);
                Node n2 = getNode(adjustedE.X, adjustedE.Y);
                if(n1 != null && n2 != null)
                {
                    n1.neighbours.Add(n2);
                    n2.neighbours.Add(n1);
                }
            }
            if (selectedNode == null)
            {
                setEdit(false);
            }
            else
            {
                setEdit(true);
                nameTB.Text = selectedNode.name;
            }
        }
        /**
         * returns a node that is located in that location. if no neighbour is found in that coordination, returns null.
         */
        Node getNode(int x, int y)
        {
            foreach(Node n in nodes)
            {
                if(nodeRadius > Math.Sqrt((x - n.pictureX )*(x - n.pictureX) + (y - n.pictureY)* (y - n.pictureY)))
                {
                    return n;
                }
            }
            return null;
        }
        void removeNode()
        {
            foreach(Node neigh in selectedNode.neighbours)
            { // remove the selected node from the lists of all neighbours.
                neigh.neighbours.Remove(selectedNode);
            }
            nodes.Remove(selectedNode);
            selectedNode = null;
            setEdit(false);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(selectedNode == null)
            {
                return;
            }
            removeNode();
        }
        private void setEdit(Boolean b)
        {
            deleteButton.Enabled = b;
            nameTB.Enabled = b;
        }
        private void nameTB_TextChanged(object sender, EventArgs e)
        {
            if(selectedNode != null)
            {
                selectedNode.name = nameTB.Text;
            }
        }
        OpenFileDialog openFileDialog1;
        private void loadImageBtn_Click(object sender, EventArgs e)
        {
            openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Image Files|*.png";
            openFileDialog1.Title = "Select a Map";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                pictureBox.Image = Image.FromFile(openFileDialog1.FileName);
                loadState();
                label9.Text = "Picture Size: " + getDisplayedImageSize(pictureBox).Width + "Px in " + getDisplayedImageSize(pictureBox).Height + "Px";
            }
        }
        private Size getDisplayedImageSize(PictureBox pictureBox)
        {
            if (pictureBox.Image == null)
                return new Size(new Point(0, 0));
            Size containerSize = pictureBox.ClientSize;
            float containerAspectRatio = (float)containerSize.Height / (float)containerSize.Width;
            Size originalImageSize = pictureBox.Image.Size;
            float imageAspectRatio = (float)originalImageSize.Height / (float)originalImageSize.Width;

            Size result = new Size();
            if (containerAspectRatio > imageAspectRatio)
            {
                result.Width = containerSize.Width;
                result.Height = (int)(imageAspectRatio * (float)containerSize.Width);
            }
            else
            {
                result.Height = containerSize.Height;
                result.Width = (int)((1.0f / imageAspectRatio) * (float)containerSize.Height);
            }
            return result;
        }
        /**
 * this function takes the coordination of a point in the picture box, and turns it into the coordination of the picturebox.
 */
        private Point transferFromImageCoordination(int x, int y)
        {
            if (pictureBox.ClientSize.Height == 0 || pictureBox.ClientSize.Width == 0)
            {
                return new Point(0, 0);
            }
            Point imageTopLeftCoordination;
            Size imageSize = getDisplayedImageSize(pictureBox);
            double heightRatio = (double)imageSize.Height / (double)pictureBox.ClientSize.Height;
            double widthRatio = (double)imageSize.Width / (double)pictureBox.ClientSize.Width;
            if (heightRatio > widthRatio)
            {// height defines how much the picture can be expanded or must be shrinked.
                imageTopLeftCoordination = new Point((int)(pictureBox.ClientSize.Width / 2.0 - imageSize.Width / 2.0), 0);
            }
            else
            {// width defines how much the picture can be expanded or must be shrinked.
                imageTopLeftCoordination = new Point(0, (int)(pictureBox.ClientSize.Height / 2.0 - imageSize.Height / 2.0));
            }
            return new Point(x + imageTopLeftCoordination.X, y + imageTopLeftCoordination.Y);
        }
        /**
         * this function takes the coordination of your click in the picture box, and turns it into the coordination of the actual image in its current size.
         */
        private Point transferToImageCoordination(int x, int y)
        {
            if(pictureBox.ClientSize.Height == 0 || pictureBox.ClientSize.Width == 0)
            {
                return new Point(0, 0);
            }
            Point imageTopLeftCoordination;
            Size imageSize = getDisplayedImageSize(pictureBox);
            double heightRatio = (double)imageSize.Height / (double)pictureBox.ClientSize.Height;
            double widthRatio = (double) imageSize.Width / (double)pictureBox.ClientSize.Width ;
            if(heightRatio > widthRatio)
            {// height defines how much the picture can be expanded or must be shrinked.
                imageTopLeftCoordination = new Point((int)(pictureBox.ClientSize.Width / 2.0 - imageSize.Width / 2.0), 0);
            } else
            {// width defines how much the picture can be expanded or must be shrinked.
                imageTopLeftCoordination = new Point(0, (int) (pictureBox.ClientSize.Height / 2.0 - imageSize.Height / 2.0));
            }
            //Console.WriteLine("picture box size: " + pictureBox.ClientSize);
            //Console.WriteLine("image size: " + imageSize);
            //Console.WriteLine("image location: " + imageTopLeftCoordination.X + " , " + imageTopLeftCoordination.Y);
            return new Point(x - imageTopLeftCoordination.X, y - imageTopLeftCoordination.Y);
        }
        /**
         * begin and destination should not be the same.
         */
        private void runDijkstra(Node begin, Node destination)
        {
            List<Node> unvisited = new List<Node>();
            foreach (Node n in nodes)
            {// add all nodes to unvisited.
                if(!n.Equals(begin))
                   unvisited.Add(n);
            }
            Node current = begin;
            begin.distanceToBegin = 0;
            do
            {
                current.visited = true;
                foreach(Node neigh in current.neighbours)
                {
                    if(neigh.visited == false) { 
                        double distThroughCurr = current.distanceToBegin + current.distanceTo(neigh);
                        if (neigh.distanceToBegin > distThroughCurr)
                        {
                            neigh.parent = current;
                            neigh.distanceToBegin = distThroughCurr;
                        }
                    }
                }
                Node closestNode = null;
                double closestDistance = Double.MaxValue;
                foreach(Node unvisN in unvisited)
                {
                    if(unvisN.distanceToBegin < closestDistance)
                    {
                        closestDistance = unvisN.distanceToBegin;
                        closestNode = unvisN;
                    }
                }
                if (destination.Equals(closestNode))
                {
                    break;
                }
                current = closestNode;
                unvisited.Remove(current);
            } while (true);
            Console.WriteLine("Distance To begin: " + destination.distanceToBegin);
            double scale = Double.Parse(scaleTB.Text);
            distances = new List<double>();
            angels = new List<double>();
            Node index = destination;
            //Console.WriteLine("begin is: " + begin.name);
            while (!index.Equals(begin))
            {
                Console.WriteLine("Node: " + index.name + ", Parent: " + index.parent.name + ", Distance: " + index.distanceTo(index.parent));
                distances.Insert(0, index.distanceTo(index.parent));
               // if (!index.parent.Equals(begin))
                {// if we are not on the last route. In other words, If there is still a turn to make.
                    double angel2 = (1 / Math.PI) * 180 * Math.Atan2((index.parent.pictureY - index.pictureY) , (index.parent.pictureX - index.pictureX));
                    //double angel1 = (1 / Math.PI) * 180 * Math.Atan2((index.parent.parent.pictureY - index.parent.pictureY) , (index.parent.parent.pictureX - index.parent.pictureX));
                    angels.Insert(0, (angel2 + 90)% 360);
                    Console.WriteLine("turn: " + (angel2 + 90) % 360);
                }
                index = index.parent;
            }
            writeOutput();
            // clean up
            foreach(Node n in nodes)
            {
                n.distanceToBegin = double.MaxValue;
                n.parent = null;
                n.visited = false;
            }
        }
        private void saveState()
        {
            if(nodes.Count > 0)
            {
                string nodS = nodes[0].toString();
                for(int i = 1; i < nodes.Count; i++)
                {
                    nodS += "*" + nodes[i].toString();
                }
                //Console.WriteLine(nodS);
                Properties.Settings.Default.nodeStates = nodS;
            }
            Properties.Settings.Default.scaleValue = scaleTB.Text;
            Properties.Settings.Default.speedValue = speedTB.Text;
            Properties.Settings.Default.Rval = RTB.Text;
            Properties.Settings.Default.Gval = GTB.Text;
            Properties.Settings.Default.Bval = BTB.Text;
            Properties.Settings.Default.Save();
        }
        private void loadState()
        {
            if(Properties.Settings.Default.speedValue.Length > 0)
            {
                speedTB.Text = Properties.Settings.Default.speedValue;
            }
            if (Properties.Settings.Default.scaleValue.Length > 0)
            {
                scaleTB.Text = Properties.Settings.Default.scaleValue;
            }
            if (Properties.Settings.Default.Rval.Length > 0)
            {
                RTB.Text = Properties.Settings.Default.Rval;
            }
            if (Properties.Settings.Default.Gval.Length > 0)
            {
                GTB.Text = Properties.Settings.Default.Gval;
            }
            if (Properties.Settings.Default.Bval.Length > 0)
            {
                BTB.Text = Properties.Settings.Default.Bval;
            }
            string nodS = Properties.Settings.Default.nodeStates;
            string[] nodSS = nodS.Split('*');
            //Console.WriteLine("loading:");
            foreach(string s in nodSS)
            {
                string[] props = s.Split(',');
                Node n = new Node(int.Parse(props[0]), int.Parse(props[1]));
                //Console.WriteLine(props[0] + ", " + props[1] + " , " + props[2]);
                n.name = props[2];
                nodes.Add(n);
            }
            // revive neighbourhood.
            int j = 0;
            foreach(Node mainNode in nodes)
            {
                
                string[] props = nodSS[j].Split(',');
                for(int i = 3; i < props.Length; i++)
                {
                    foreach(Node n in nodes)
                    {
                        if (n.name.Equals(props[i]))
                        {
                            mainNode.neighbours.Add(n);
                        }
                    }
                }
                j++;
            }
        }
        private void writeOutput()
        {
            using (StreamWriter sw = File.CreateText(path))
            {
                ;
                sw.WriteLine((int)(Double.Parse(speedTB.Text) * 128));
                sw.WriteLine(RTB.Text);// red
                sw.WriteLine(GTB.Text);// green
                sw.WriteLine(BTB.Text);// blue
                double scale = Double.Parse(scaleTB.Text);

                for (int i = 0; i < angels.Count; i++)
                {
                    sw.WriteLine(distances[i] * 0.01 / scale + "," + angels[i]);
                }
            }
            try
            {
                System.Diagnostics.Process.Start("demo.py", path);
            }
            catch (Exception) { }
        }
        private void searchButton_Click(object sender, EventArgs e)
        {
            Console.WriteLine("search function called.");
            Node begin = null, dest = null;
            foreach(Node n in nodes)
            {
                Console.WriteLine("Name: " + n.name);
                if(n.name == "BEGIN")
                {
                    begin = n;
                }
                if(n.name.Trim().ToLower() == queryTB.Text.Trim().ToLower())
                {
                    dest = n;
                }
            }
            if(begin == null)
            {
                MessageBox.Show("can't find the beginning position.");
                return;
            }
            if(dest == null)
            {
                MessageBox.Show("No " + queryTB.Text + " in this Shop -_-");
                return;
            } else
            {
                MessageBox.Show("follow around to the " + dest.name + " section");
            }
            runDijkstra(begin, dest);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            saveState();
        }
        Boolean access = true; // true=admin, false=user.
        private void toggleButton_Click(object sender, EventArgs e)
        {
            access = !access;
            setAccess();
            if (access)
            {
                toggleButton.Text = "User Mode";
            }
            else
            {
                toggleButton.Text = "Admin Mode";
            }
        }
        private void setAccess()
        {
            nameLabel.Visible = access;
            label1.Visible = access;
            label2.Visible = access;
            pictureBox.Visible = access;
            deleteButton.Visible = access;
            loadImageBtn.Visible = access;
            scaleTB.Visible = access;
            nameTB.Visible = access;
            label9.Visible = access;
        }
    }
}
