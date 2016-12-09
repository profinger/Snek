using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Snek1
{
    public partial class Form1 : Form
    {
        public enum BlockTypes { BG = 0, Snake = 1, Food = 2, SnakeHead = 3, Vis=4, None=5, Wall=6}

        public Snek yaboi { get; set; }
        public Graphics g { get; set; }
        public Timer t { get; set; }
        //public int w { get; set; }
        //public int h { get; set; }
        public int gw { get; set; }
        public int gh { get; set; }
        public int segSize = 10;
        public int initLen = 5;
        public int origInitLen;
        public List<bodySeg> food;
        public int foodCount = 10;
        public Random r;
        public int visLen = 10;
        public List<bodySeg> visArray;
        public int[,,] pixs;
        public int[,,] ovlpix;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            r = new Random();
            //w = pictureBox1.Width;
            //h = pictureBox1.Height;
            origInitLen = initLen;
            g = pictureBox1.CreateGraphics();
            gw = pictureBox1.Width / segSize;
            gh = pictureBox1.Height / segSize;
            pixs = new int[gw, gh, 1];
            ovlpix = new int[gw, gh, 1];
            initPixs();
            this.KeyPress += new KeyPressEventHandler(Form1_KeyPress);
            listView1.KeyPress += new KeyPressEventHandler(Form1_KeyPress);
            //this.KeyPress += Form1_KeyPress1;
            t = new Timer();
            t.Interval = 5;
            t.Tick += T_Tick;
            visArray = new List<bodySeg>();
            newGame();
        }

        public void initPixs()
        {
            for(int x = 0; x < gw; x++)
            {
                for(int y = 0; y < gh; y++)
                {
                    pixs[x, y, 0] = (int)BlockTypes.BG;
                    ovlpix[x, y, 0] = (int)BlockTypes.None;
                }
            }
        }

        private void newGame ()
        {
            //yaboi = new Snek(segSize, (w / 2)-((w/2)%segSize), h / 2 - ((h/2)%segSize), initLen, 1);
            yaboi = new Snek(segSize, (gw / 2), gh / 2, initLen, 1);
            food = new List<bodySeg>();
            initLen = origInitLen;
            initPixs();
            initFood();
            t.Enabled = true;
        }

        private void initFood ()
        {
            for(int x = 0; x < foodCount; x++ )
            {
                var newSeg = newFood();
                if (food.Where(b => b.x == newSeg.x && b.y == newSeg.y).Count() == 0)
                {
                    food.Add(newSeg);
                } else
                {
                    newSeg = newFood();
                    food.Add(newSeg);
                }
                pixs[newSeg.x, newSeg.y, 0] = (int)BlockTypes.Food;
            }
        }

        private bodySeg newFood ()
        {
            var nx = r.Next(0, gw);
            nx = nx - (nx % segSize);
            var ny = r.Next(0, gh);
            ny = ny - (ny % segSize);

            return new bodySeg(nx, ny);
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 'w' && yaboi.facing != 3)
            {
                yaboi.facing = 1; // up
            } else if (e.KeyChar == 'a' && yaboi.facing != 2)
            {
                yaboi.facing = 4; // left 
            } else if (e.KeyChar == 's' && yaboi.facing != 1)
            {
                yaboi.facing = 3; // down
            } else if (e.KeyChar == 'd' && yaboi.facing != 4)
            {
                yaboi.facing = 2; // right
            }
        }

        private void T_Tick(object sender, EventArgs e)
        {
            move();
            paint();
            visArray = see();
            if (detectColl())
            {
                t.Enabled = false;
                //MessageBox.Show("SHIT");
                //g.DrawString("SHIT", new Font(FontFamily.GenericMonospace, 50), Brushes.Orange, w / 2, h / 2);
                
                newGame();
            }
            detectFoodColl();
        }

        public void detectFoodColl()
        {
            var eaten = food.Where(f => f.x == yaboi.headx && f.y == yaboi.heady);
            var eatCount = eaten.Count();
            food.RemoveAll(f => f.x == yaboi.headx && f.y == yaboi.heady);
            if (eatCount > 0)
            {
                initLen += eatCount;
                var fd = newFood();
                food.Add(fd);
                pixs[fd.x, fd.y, 0] = (int)BlockTypes.Food;
            }

        }

        public void process ()
        {
            move();
            
        }

        public bool detectColl()
        {
            bool ret = false;

            if (yaboi.headx < 0)
            {
                return true;
            } else if (yaboi.headx > (gw))
            {
                return true;
            } else if (yaboi.heady < 0)
            {
                return true;
            } else if (yaboi.heady > (gh))
            {
                return true;
            }
            
            foreach(bodySeg s in yaboi.segs)
            {
                if (yaboi.facing == 1 && yaboi.headx==s.x && yaboi.heady-1 == s.y) //up
                {
                    return true;
                } else if (yaboi.facing == 2 && yaboi.headx + 2 == s.x && yaboi.heady == s.y) // right
                {
                    return true;
                } else if (yaboi.facing == 3 && yaboi.headx==s.x && yaboi.heady + 2 == s.y)
                {
                    return true; 
                } else if (yaboi.facing == 4 && yaboi.headx-2 == s.x && yaboi.heady==s.y)
                {
                    return true;
                }
            }

            return ret;
        }

        public void move ()
        {
            if (yaboi.facing == 1) //up
            {
                yaboi.heady -= 1;
            }
            else if (yaboi.facing == 2)//right
            {
                yaboi.headx += 1;
            }
            else if (yaboi.facing == 3)//down
            {

                yaboi.heady += 1;
            }
            else if (yaboi.facing == 4)//left
            {
                yaboi.headx -= 1;
            }

            yaboi.AddHead(new bodySeg(yaboi.headx, yaboi.heady));

            //yaboi.segs.Add(new bodySeg(yaboi.headx, yaboi.heady,true));
            if (yaboi.segs.Count > initLen)
            {
                pixs[yaboi.segs[0].x, yaboi.segs[0].y, 0] = (int)BlockTypes.BG;
                yaboi.segs.Remove(yaboi.segs[0]);
            }
        }

        public void paint ()
        {
            //foreach(bodySeg b in yaboi.segs)
            //{

            //}

            //for(int x = 0; x < w/segSize; x ++)
            //{
            //    for (int y =0; y < h/segSize; y++)
            //    {
            //        g.FillRectangle(pixs[x,y,0]==1 ? Brushes.Black : Brushes.Blue, x * segSize, y * segSize, segSize, segSize);
            //    }
            //}
            //g.FillRectangle(Brushes.Black, 0, 0, w, h);

            foreach (bodySeg b in yaboi.segs)
            {
                //g.FillRectangle(b.isHead ? Brushes.Red : Brushes.Green, b.x, b.y, yaboi.segSize, yaboi.segSize);
                if (b.x > -1 && b.y > -1 && b.x < gw && b.y < gh)
                {
                    pixs[b.x, b.y, 0] = b.isHead ? (int)BlockTypes.SnakeHead : (int)BlockTypes.Snake;
                }
            }

            //foreach (bodySeg f in food)
            //{
            //    g.FillRectangle(Brushes.Blue, f.x, f.y, segSize, segSize);
            //}

            for(int x = 0; x < gw; x++)
            {
                for(int y = 0; y < gh; y++)
                {
                    var col = pixs[x, y, 0];
                    var brsh = col == (int)BlockTypes.BG ? Brushes.Black : col == (int)BlockTypes.Food ? Brushes.Blue : col == (int)BlockTypes.SnakeHead ? Brushes.Red : col == (int)BlockTypes.Snake ? Brushes.Green : Brushes.White;
                    //var sz = col == (int)BlockTypes.Vis ? segSize / 2 : segSize;
                    g.FillRectangle(brsh, x * segSize, y * segSize, segSize, segSize);
                    if (ovlpix[x,y,0] != (int)BlockTypes.None)
                    {
                        g.FillRectangle(Brushes.Orange, x * segSize, y * segSize, segSize/2, segSize/2);
                    }
                }
            }

            listView1.Items.Clear();
            for (int o = 0; o < visArray.Count(); o++)
            {
                listView1.Items.Add(o + ": " + visArray[o].segId);
            }

            //for (int g1 = 0; g1 < w; g1 = g1 + segSize)
            //{
            //    g.DrawLine(Pens.White, g1, 0, g1, h);
            //}

            //for (int g2 = 0; g2 < h; g2 = g2 + segSize)
            //{
            //    g.DrawLine(Pens.White, 0, g2, w, g2);
            //}
        }

        public int getSight(int x, int y)
        {
            var s = 0;
            
            //var bmp = new Bitmap(pictureBox1.Image);
            //var pix = bmp.GetPixel(x, y);
            //s = pix.R + pix.G + pix.B;

            if (x < 0 || y < 0 || x >= gw || y >= gh)
            {
                return (int)BlockTypes.Wall;
            }
            return pixs[x,y,0];
        }

        public void removeOldVis()
        {
            foreach(bodySeg b in visArray)
            {
                if (b.x >= 0 && b.y >= 0 && b.x < gw && b.y < gh)
                {
                    ovlpix[b.x, b.y, 0] = (int)BlockTypes.None;
                }
            }
        }

        public List<bodySeg> see()
        {
            var d = new List<bodySeg>();
            removeOldVis();

            if (yaboi.facing == 1) //up
            {
                for(int z = 0; z < visLen; z++) // front
                {
                    if (yaboi.heady -(z) < 0)
                    {
                        for (int oo = 0; oo < visLen - z; oo++)
                        {
                            d.Add(new bodySeg(yaboi.headx, yaboi.heady - z, (int)BlockTypes.Wall));
                        }
                        break;
                    }

                    d.Add(new bodySeg(yaboi.headx,yaboi.heady - z,getSight(yaboi.headx, yaboi.heady - (z))));
                    ovlpix[yaboi.headx, yaboi.heady - z, 0] = (int)BlockTypes.Vis;
                }

                for(int z = 0; z < visLen; z++) // left
                {
                    if (yaboi.heady - z < 0 || yaboi.headx - z < 0)
                    {
                        for (int oo = 0; oo < visLen - z; oo++)
                        {
                            d.Add(new bodySeg(yaboi.headx - z < 0 ? yaboi.headx - z : yaboi.headx, yaboi.heady - z < 0 ? yaboi.heady - z : yaboi.heady, (int)BlockTypes.Wall));
                        }
                        break;
                    }

                    d.Add(new bodySeg(yaboi.headx - z, yaboi.heady - z, getSight(yaboi.headx - z, yaboi.heady - z)));
                    ovlpix[yaboi.headx -z , yaboi.heady - z, 0] = (int)BlockTypes.Vis;
                }

                for (int z = 0; z < visLen; z++) // right
                {
                    if (yaboi.heady - z < 0 || yaboi.headx + z >= gw)
                    {
                        for (int oo = 0; oo < visLen - z; oo++)
                        {
                            d.Add(new bodySeg(yaboi.headx + z >= gw ? yaboi.headx + z : yaboi.headx, yaboi.heady - z < 0 ? yaboi.heady - z : yaboi.heady, (int)BlockTypes.Wall));
                        }
                        break;
                    }

                    d.Add(new bodySeg(yaboi.headx + z, yaboi.heady - z, getSight(yaboi.headx + z, yaboi.heady - z)));
                    ovlpix[yaboi.headx + z, yaboi.heady - z, 0] = (int)BlockTypes.Vis;
                }
            }
            else if (yaboi.facing == 2) //right
            {
                for (int z = 0; z < visLen; z++) //front
                {
                    if (yaboi.headx + (z) >= gw)
                    {
                        for (int oo = 0; oo < visLen - z; oo++)
                        {
                            d.Add(new bodySeg(yaboi.headx + z, yaboi.heady, (int)BlockTypes.Wall));
                        }
                        break;
                    }
                    //d.Add(getSight(yaboi.headx + (z), yaboi.heady));
                    d.Add(new bodySeg(yaboi.headx + z, yaboi.heady, getSight(yaboi.headx + z, yaboi.heady)));
                    ovlpix[yaboi.headx + z, yaboi.heady, 0] = (int)BlockTypes.Vis;
                }
                for (int z = 0; z < visLen; z++) //left
                {
                    if (yaboi.headx + (z) >= gw || yaboi.heady - z < 0)
                    {
                        for (int oo = 0; oo < visLen - z; oo++)
                        {
                            d.Add(new bodySeg(yaboi.headx + z >= gw ? yaboi.headx + z : yaboi.headx, yaboi.heady - z < 0 ? yaboi.heady - z : yaboi.heady, (int)BlockTypes.Wall));
                        }
                        break;
                    }
                    //d.Add(getSight(yaboi.headx + (z), yaboi.heady));
                    d.Add(new bodySeg(yaboi.headx + z, yaboi.heady - z, getSight(yaboi.headx + z, yaboi.heady - z)));
                    ovlpix[yaboi.headx + z, yaboi.heady - z, 0] = (int)BlockTypes.Vis;
                }
                for (int z = 0; z < visLen; z++) //
                {
                    if (yaboi.headx + (z) >= gw || yaboi.heady + z >= gh)
                    {
                        for (int oo = 0; oo < visLen - z; oo++)
                        {
                            d.Add(new bodySeg(yaboi.headx + z >= gw ? yaboi.headx + z : yaboi.headx, yaboi.heady + z > gh ? yaboi.heady + z : yaboi.heady, (int)BlockTypes.Wall));
                        }
                        break;
                    }
                    //d.Add(getSight(yaboi.headx + (z), yaboi.heady));
                    d.Add(new bodySeg(yaboi.headx + z, yaboi.heady + z, getSight(yaboi.headx + z, yaboi.heady + z)));
                    ovlpix[yaboi.headx + z, yaboi.heady + z, 0] = (int)BlockTypes.Vis;
                }
            }
            else if (yaboi.facing == 3) //down
            {
                for (int z = 0; z < visLen; z++) //front
                {
                    if (yaboi.heady + (z) >= gh)
                    {
                        for (int oo = 0; oo < visLen - z; oo++)
                        {
                            d.Add(new bodySeg(yaboi.headx, yaboi.heady + z, (int)BlockTypes.Wall));
                        }
                        break;
                    }
                    //d.Add(getSight(yaboi.headx, yaboi.heady + (z)));
                    d.Add(new bodySeg(yaboi.headx, yaboi.heady + z, getSight(yaboi.headx, yaboi.heady + (z))));
                    ovlpix[yaboi.headx, yaboi.heady + z, 0] = (int)BlockTypes.Vis;
                }
                for (int z = 0; z < visLen; z++) //left
                {
                    if (yaboi.heady + (z) >= gh || yaboi.headx + z >= gw)
                    {
                        for (int oo = 0; oo < visLen - z; oo++)
                        {
                            d.Add(new bodySeg(yaboi.headx + z >= gw ? yaboi.headx + z : yaboi.headx, yaboi.heady + z > gh ? yaboi.heady + z : yaboi.heady, (int)BlockTypes.Wall));
                        }
                        break;
                    }
                    //d.Add(getSight(yaboi.headx, yaboi.heady + (z)));
                    d.Add(new bodySeg(yaboi.headx + z, yaboi.heady + z, getSight(yaboi.headx + z, yaboi.heady + (z))));
                    ovlpix[yaboi.headx + z, yaboi.heady + z, 0] = (int)BlockTypes.Vis;
                }
                for (int z = 0; z < visLen; z++) //right
                {
                    if (yaboi.heady + (z) >= gh || yaboi.headx - z < 0)
                    {
                        for (int oo = 0; oo < visLen - z; oo++)
                        {
                            d.Add(new bodySeg(yaboi.headx - z < 0 ? yaboi.headx - z : yaboi.headx, yaboi.heady + z >= gh ? yaboi.heady + z : yaboi.heady, (int)BlockTypes.Wall));
                        }
                        break;
                    }
                    //d.Add(getSight(yaboi.headx, yaboi.heady + (z)));
                    d.Add(new bodySeg(yaboi.headx-z, yaboi.heady + z, getSight(yaboi.headx-z, yaboi.heady + (z))));
                    ovlpix[yaboi.headx-z, yaboi.heady + z, 0] = (int)BlockTypes.Vis;
                }
            }
            else if (yaboi.facing == 4) //left
            {
                for (int z = 0; z < visLen; z++) //front
                {
                    if (yaboi.headx - (z) < 0)
                    {
                        for (int oo = 0; oo < visLen - z; oo++)
                        {
                            d.Add(new bodySeg(yaboi.headx - z, yaboi.heady, (int)BlockTypes.Wall));
                        }
                        break;
                    }
                    //d.Add(getSight(yaboi.headx - (z), yaboi.heady));
                    d.Add(new bodySeg(yaboi.headx - z, yaboi.heady, getSight(yaboi.headx - z, yaboi.heady)));
                    ovlpix[yaboi.headx - z, yaboi.heady, 0] = (int)BlockTypes.Vis;
                }
                for (int z = 0; z < visLen; z++) //left
                {
                    if (yaboi.headx - (z) < 0 || yaboi.heady + z > gh)
                    {
                        for (int oo = 0; oo < visLen - z; oo++)
                        {
                            d.Add(new bodySeg(yaboi.headx - z < 0 ? yaboi.headx - z : yaboi.headx, yaboi.heady + z > gh ? yaboi.heady + z : yaboi.heady, (int)BlockTypes.Wall));
                        }
                        break;
                    }
                    //d.Add(getSight(yaboi.headx - (z), yaboi.heady));
                    d.Add(new bodySeg(yaboi.headx - z, yaboi.heady + z, getSight(yaboi.headx - z, yaboi.heady + z)));
                    ovlpix[yaboi.headx - z, yaboi.heady + z, 0] = (int)BlockTypes.Vis;
                }
                for (int z = 0; z < visLen; z++) // right
                {
                    if (yaboi.headx - (z) < 0 || yaboi.heady - z < 0)
                    {
                        for (int oo = 0; oo < visLen - z; oo++)
                        {
                            d.Add(new bodySeg(yaboi.headx - z < 0 ? yaboi.headx - z : yaboi.headx, yaboi.heady - z < 0 ? yaboi.heady - z : yaboi.heady, (int)BlockTypes.Wall));
                        }
                        break;
                    }
                    //d.Add(getSight(yaboi.headx - (z), yaboi.heady));
                    d.Add(new bodySeg(yaboi.headx - z, yaboi.heady -z, getSight(yaboi.headx - z, yaboi.heady-z)));
                    ovlpix[yaboi.headx - z, yaboi.heady-z, 0] = (int)BlockTypes.Vis;
                }

            }

            return d;
        }  
    }

    public class Snek
    {
        public int headx { get; set; }
        public int heady { get; set; }
        public int facing { get; set; }
        public List<bodySeg> segs {get;set;}
        public int segSize { get; set; }

        public Snek (int segSize, int initx, int inity, int initLen, int dir)
        {
            this.segSize = segSize;
            headx = initx;
            heady = inity;
            facing = dir;
            segs = new List<bodySeg>();

            //for(int x = 0; x < initLen; x++)
            //{
            //    AddHead(new bodySeg(initx, (inity+((segSize*x)+(initLen*segSize*x)))));
            //}

        }

        public void AddHead(bodySeg hd)
        {
            foreach(bodySeg bs in segs)
            {
                bs.isHead = false;
            }
            hd.isHead = true;
            segs.Add(hd);

        }
    }

    public class bodySeg
    {
        public int x { get; set; }
        public int y { get; set; }
        public int segId { get; set; }
        public bool isHead { get; set; }

        public bodySeg(int sx, int sy)
        {
            x = sx;
            y = sy;
        }

        public bodySeg(int sx, int sy, int id)
        {
            x = sx;
            y = sy;
            segId = id;
        }
    }

}
