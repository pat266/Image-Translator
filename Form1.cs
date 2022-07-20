using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Emgu.CV;
using Emgu.CV.Structure;

namespace EmguCV_TextDetection
{
    public partial class Form1 : Form
    {
        private PictureBox leftPicture, rightPicture;
        private OpenFileDialog file;
        public Form1()
        {
            InitializeComponent();
            // set true, otherwise key press is swallowed by the control that has focus
            this.KeyPreview = true;
            // add KeyEvent to the form
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(KeyEvent);

            leftPicture = new PictureBox();
            rightPicture = new PictureBox();

            leftPicture.SizeMode = PictureBoxSizeMode.AutoSize;
            rightPicture.SizeMode = PictureBoxSizeMode.AutoSize;
            
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel2.AutoScroll = true;
            
            flowLayoutPanel1.Controls.Add(leftPicture);
            flowLayoutPanel2.Controls.Add(rightPicture);
        }

        private void openImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            file = new OpenFileDialog
            {
                Title = "Open Image"
            };
            if (file.ShowDialog() == DialogResult.OK)
            {
                leftPicture.Image = null; // delete the old image
                rightPicture.Image = null; // delete the old image
                System.GC.Collect();
                leftPicture.Image = new Bitmap(file.FileName); // set to the new image
                
                // enable the options in the MenuStrip
                detectTextToolStripMenuItem.Enabled = true;
                translateTextToolStripMenuItem.Enabled = true;
            }
        }

        private void detectTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Bitmap bm = (Bitmap) (leftPicture.Image);
            DrawBoundingRectangles(bm.ToImage<Bgr, byte>());
        }

        /**
         * Method to handle various methods from pressing down the key
         */
        private void KeyEvent(object sender, KeyEventArgs e) //Keyup Event 
        {
            if (e.KeyCode == Keys.F8)
            {
                // detect the text when press F8
                if (detectTextToolStripMenuItem.Enabled)
                {
                    Bitmap bm = (Bitmap)(leftPicture.Image);
                    DrawBoundingRectangles(bm.ToImage<Bgr, byte>());
                }
            }
            if (e.KeyCode == Keys.F9)
            {
                // translate the text when press F9
                if (translateTextToolStripMenuItem.Enabled)
                {
                    Bitmap bitmap = (Bitmap)(leftPicture.Image);
                    TranslateText(bitmap.ToImage<Bgr, byte>());
                }
            }

        }

        
        /**
         * Algorithm taken from https://www.youtube.com/watch?v=KHes5M7zpGg
         * Detect text in the image and get Bounding Rectangles around it.
         * Return: a list of rectangles (can get (X, Y), width, and height)
         */
        private List<Rectangle> GetBoudingRectangles(Image<Bgr, byte> img)
        {
            /*
             1. Edge detection (sobel)
             2. Dilation (10,1)
             3. FindContours
             4. Geometrical Constrints
             */
            //sobel
            Image<Gray, byte> sobel = img.Convert<Gray, byte>().Sobel(1, 0, 3).AbsDiff(new Gray(0.0)).Convert<Gray, byte>().ThresholdBinary(new Gray(50), new Gray(255));
            Mat SE = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Rectangle, new Size(10, 2), new Point(-1, -1));
            sobel = sobel.MorphologyEx(Emgu.CV.CvEnum.MorphOp.Dilate, SE, new Point(-1, -1), 1, Emgu.CV.CvEnum.BorderType.Reflect, new MCvScalar(255));
            Emgu.CV.Util.VectorOfVectorOfPoint contours = new Emgu.CV.Util.VectorOfVectorOfPoint();
            Mat m = new Mat();

            CvInvoke.FindContours(sobel, contours, m, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

            List<Rectangle> list = new List<Rectangle>();

            for (int i = 0; i < contours.Size; i++)
            {
                Rectangle brect = CvInvoke.BoundingRectangle(contours[i]);

                // add more height and width to the rectangle
                var value = 2; // adjustable in the future?!
                brect.X -= value;
                brect.Y -= value;
                brect.Width += value;
                brect.Height += value;

                double ar = brect.Width / brect.Height;
                if (ar > 2 && brect.Width > 10 && brect.Height > 10 && brect.Height < 30)
                {
                    list.Add(brect);
                }
            }

            return list; // return the list of Rectangles
        }
        /**
         * Algorithm taken from https://www.youtube.com/watch?v=KHes5M7zpGg
         * Detect text in the image and draw Bounding Rectangles around it.
         */
        private void DrawBoundingRectangles(Image<Bgr, byte> img)
        {
            List<Rectangle> list = GetBoudingRectangles(img);
            // draw the rectangles
            foreach (var r in list)
            {
                CvInvoke.Rectangle(img, r, new MCvScalar(0, 0, 255), 2);
            }

            rightPicture.Image = null; // delete the old image
            System.GC.Collect();
            rightPicture.Image = img.ToBitmap();
        }

        /**
         * 
         */
        private void translateTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Bitmap bitmap = (Bitmap)(leftPicture.Image);
            TranslateText(bitmap.ToImage<Bgr, byte>());
        }

        /**
         * Detect text in the image and draw Bounding Rectangles around it.
         */
        private void TranslateText(Image<Bgr, byte> img)
        {
            List<Rectangle> list = GetBoudingRectangles(img);
            // draw the rectangles
            foreach (var r in list)
            {
                CvInvoke.Rectangle(img, r, new MCvScalar(0, 255, 255), -1);
            }

            rightPicture.Image = null; // delete the old image
            System.GC.Collect();
            rightPicture.Image = img.ToBitmap();
        }
    }
}
