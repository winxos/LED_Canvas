using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using winxos;

namespace LED_Canvas
{
    public partial class Form1 : Form
    {
        int block_sz = 10;
        int clear_msg = 0;
        int sw, sh;
        bool[,] map;
        byte bgcolor=50,bgd = 1;
        Bitmap img;
        CsImage ci;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            in_mode_method.SelectedIndex = 0;
            in_screenw.Text = "6";
            in_screenh.Text = "8";
            map = new bool[6, 8];
            ci = new CsImage();
            this.MinimumSize = this.Size;
            timer1.Enabled = true;
        }
        void mdraw(Graphics g)
        {
            g.Clear(Color.FromArgb(0,0,bgcolor));
            bgcolor+=bgd;
            if (bgcolor > 200 || bgcolor < 50) bgd = (byte)-bgd;
        }
        void draw_block(Graphics g)
        {
            try
            {
                if (sw != int.Parse(in_screenw.Text))
                {
                    sw = int.Parse(in_screenw.Text);
                    redim_array();
                }
                if (sh != int.Parse(in_screenh.Text))
                {
                    sh = int.Parse(in_screenh.Text);
                    redim_array();
                }
                block_sz = Math.Min(pictureBox1.Width / (2 + sw), pictureBox1.Height / (2 + sh));
            }
            catch (Exception) { };

            if (block_sz < 1) block_sz = 1;
            for (int j = 0; j < sh; j++)
            {
                for (int i = 0; i < sw; i++)
                {
                    if (!map[i, j])
                    {
                        g.FillRectangle(new SolidBrush(Color.White), block_sz + i * block_sz, block_sz + j * block_sz,
                             (int)(block_sz * 0.8), (int)(block_sz * 0.8));
                    }
                    else
                    {
                        g.FillRectangle(new SolidBrush(Color.Green), block_sz + i * block_sz, block_sz + j * block_sz,
                            (int)(block_sz * 0.8), (int)(block_sz * 0.8));
                    }
                }
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            BufferedGraphics bg = BufferedGraphicsManager.Current.Allocate(pictureBox1.CreateGraphics(), pictureBox1.ClientRectangle);
            Graphics g = bg.Graphics;
            g.Clear(Color.Black);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
            mdraw(g);
            draw_block(g);
            bg.Render();
            g.Dispose();
            if (clear_msg > 0) clear_msg--;
            if (clear_msg == 0)
            {
                out_msg.Text = DateTime.Now.ToLocalTime().ToString("MM-dd HH:mm:ss");
            }
            
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            int gridx=e.X/block_sz-1;
            int gridy=e.Y/block_sz-1;
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                try 
                {
                    map[gridx, gridy] = true;
                }
                catch (Exception) { };
            }
        }
        void redim_array()
        {
            map = new bool[sw, sh];
        }
        private void in_screenh_TextChanged(object sender, EventArgs e)
        {
            redim_array();
        }

        private void in_screenw_TextChanged(object sender, EventArgs e)
        {
            redim_array();
        }
        string createData()
        {
            string s = "";
            bool isPostive = in_positive.Checked;
            bool isClockWise = in_clockwise.Checked;
            byte tmp;
            for (int j = 0; j < sh / 8; j++)
            {
                for (int i = 0; i < sw; i++)
                {
                    tmp=0;
                    for (int k = 7; k >=0; k--)
                    {
                        if (map[i, j * 8 + k])
                        {
                            if (!isClockWise) //anti clockwise
                            {
                                tmp |= (byte)(1 << (7-k));
                            }
                            else
                            {
                                tmp |= (byte)(1 << k);
                            }
                        }   
                    }
                    if (!isPostive)
                    {
                        tmp = (byte)~tmp;
                    }
                    s += string.Format("0x{0:X2},",tmp);
                }
                s += Environment.NewLine;
            }
            s = s.TrimEnd(); //remove last newline
            s = s.TrimEnd(','); //remove last ,
            return s;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(createData());
            out_msg.Text = "数据已拷贝至剪贴板";
            clear_msg += 100;
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                redim_array();
            }
        }

        private void pictureBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int gridx = e.X / block_sz - 1;
            int gridy = e.Y / block_sz - 1;
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                try
                {
                    map[gridx, gridy] = !map[gridx, gridy];
                }
                catch (Exception) { };
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
        void resize_pic()
        {
            if (img!=null)
            {
                Bitmap b = new Bitmap(sw, sh);
                Graphics tmp = Graphics.FromImage(b);
                tmp.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
                tmp.DrawImage(img, new RectangleF(0, 0, sw, sh), 
                    new RectangleF(0, 0, img.Width, img.Height), GraphicsUnit.Pixel);
                img = b;
                tmp.Dispose();
            }
        }
        void reload_img()
        {
            byte[] bs=ci.GetBitmapRGB(img);
            byte bg=0;
            if (in_positive.Checked) bg = 255;
            for(int j=0;j<img.Height;j++)
            {
                for (int i = 0; i < img.Width; i ++)
                {
                    map[i, j] = (bs[j * img.Width * 4 + i * 4]==bg);
                }
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog of = new OpenFileDialog();
            of.Filter = "图片文件|*.jpg;*.bmp;*.png;*.gif";
            of.ShowDialog();
            try
            {

                img = (Bitmap)Bitmap.FromFile(of.FileName);
                resize_pic();
                img = ci.Binarization(ci.Rgb2Gray(img));
                reload_img();
            }
            catch (Exception) { };

        }
    }
}
