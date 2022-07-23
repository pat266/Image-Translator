using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Emgu.CV;
using Emgu.CV.Structure;

using IronOcr;

namespace EmguCV_TextDetection
{
    public partial class Form1 : Form
    {
        private readonly string[] SizeSuffixes =
                   { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        private string memorySize, calculationTime;
        
        private PictureBox leftPicture, rightPicture;
        private OpenFileDialog file;

        IronTesseract Ocr;
        public Form1()
        {
            memorySize = string.Empty;
            calculationTime = string.Empty;
            
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

            // initialize for IronOCR
            Ocr = new IronTesseract();

            // improve speed
            Ocr.Language = OcrLanguage.ChineseSimplifiedFast;
            // Latest Engine 
            Ocr.Configuration.TesseractVersion = TesseractVersion.Tesseract5;
            //AI OCR only without font analysis
            Ocr.Configuration.EngineMode = TesseractEngineMode.LstmOnly;
            //Turn off unneeded options
            Ocr.Configuration.ReadBarCodes = false;
            Ocr.Configuration.RenderSearchablePdfsAndHocr = false;
            // Assume text is laid out neatly in an orthagonal document
            Ocr.Configuration.PageSegmentationMode = TesseractPageSegmentationMode.SparseText;


            methodChoices.DisplayMember = "Text";
            methodChoices.ValueMember = "Value";

            var items = new[] {
                new { Text = "EmguCV + Onnx", Value = "0" },
                new { Text = "IronOCR Only", Value = "1" },
                new { Text = "EmguCV + IronOCR", Value = "2" }
            };

            methodChoices.DataSource = items;
        }

        private void methodChoices_SelectedValueChanged(object sender, EventArgs e)
        {
            rightPicture.Image = null; // delete the old image
            System.GC.Collect();
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

        private async void detectTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /**
             * EmguCV + Onnx = "0"
             * IronOCR Only = "1"
             * EmguCV + IronOCR = "2"
             */
            string selectedVal = (string)methodChoices.SelectedValue;
            if (selectedVal.Equals("0"))
            {

            }
            else
            {
                Bitmap bm = (Bitmap)(leftPicture.Image);
                await DetectText_Async(bm.ToImage<Bgr, byte>());
            }
                
        }
        
        /**
         * 
         */
        private async void translateTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /**
             * EmguCV + Onnx = "0"
             * IronOCR Only = "1"
             * EmguCV + IronOCR = "2"
             */
            string selectedVal = (string)methodChoices.SelectedValue;
            if (selectedVal.Equals("0"))
            {

            }
            else
            {
                Bitmap bm = (Bitmap)(leftPicture.Image);
                if (selectedVal.Equals("1"))
                {
                    await TranslateText_IronOCR(bm);
                }
                else if (selectedVal.Equals("2"))
                {
                    await TranslateText(bm.ToImage<Bgr, byte>());
                }
                    
            }
            
        }

        /**
         * Method to handle various methods from pressing down the key
         */
        private async void KeyEvent(object sender, KeyEventArgs e) //Keyup Event 
        {
            if (!(translateTextToolStripMenuItem.Enabled && detectTextToolStripMenuItem.Enabled))
            {
                if (e.KeyCode == Keys.F8 || e.KeyCode == Keys.F9)
                {
                    MessageBox.Show("Please insert an image", "Image not found!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            /**
             * EmguCV + Onnx = "0"
             * IronOCR Only = "1"
             * EmguCV + IronOCR = "2"
             */
            string selectedVal = (string)methodChoices.SelectedValue;
            if (selectedVal.Equals("0"))
            {
                // if we are currently using EmguCV + Onnx
                if (e.KeyCode == Keys.F8)
                {
                    
                }
                else if (e.KeyCode == Keys.F9)
                {
                    
                }
            }
            else
            {
                // if we choose IronOCR only or EmguCV + IronOCR
                if (e.KeyCode == Keys.F8)
                {
                    // detect the text when press F8
                    if (detectTextToolStripMenuItem.Enabled)
                    {
                        Bitmap bm = (Bitmap)(leftPicture.Image);
                        if (selectedVal.Equals("1"))
                        {
                            await DetectTextIronOCR_Async(bm);
                            Console.WriteLine(SizeSuffix(GC.GetTotalMemory(true)));
                        }
                        else if (selectedVal.Equals("2"))
                        {
                            await DetectText_Async(bm.ToImage<Bgr, byte>());
                            Console.WriteLine(SizeSuffix(GC.GetTotalMemory(true)));
                        }
                    }
                }
                if (e.KeyCode == Keys.F9)
                {
                    // translate the text when press F9
                    if (translateTextToolStripMenuItem.Enabled)
                    {
                        Bitmap bitmap = (Bitmap)(leftPicture.Image);
                        if (selectedVal.Equals("1"))
                        {
                            await TranslateText_IronOCR(bitmap);
                        }
                        else if (selectedVal.Equals("2"))
                        {
                            await TranslateText(bitmap.ToImage<Bgr, byte>());
                        }
                    }
                }
            }
            

        }

        #region "Helper Methods"
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
                var value = 0; // adjustable in the future?!
                brect.X -= value;
                brect.Y -= value;
                brect.Width += value;
                brect.Height += value;

                double ar = brect.Width / brect.Height;
                if (ar > 0.8 && brect.Width > 10 && brect.Height > 10 && brect.Height < 60)
                {
                    list.Add(brect);
                }

            }

            return list; // return the list of Rectangles
        }

        /**
         * Taken from https://stackoverflow.com/questions/14488796/does-net-provide-an-easy-way-convert-bytes-to-kb-mb-gb-etc
         */
        private string SizeSuffix(Int64 value, int decimalPlaces = 1)
        {
            if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
            if (value < 0) { return "-" + SizeSuffix(-value, decimalPlaces); }
            if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]);
        }

        private string CalculateTime(Int64 value, int decimalPlaces = 1)
        {
            if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
            if (value < 0) { return "-" + SizeSuffix(-value, decimalPlaces); }
            if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]);
        }
        #endregion

        #region "Detecting Text"
        /**
         * Detect text in the image and draw Bounding Rectangles around it.
         */
        private async Task DetectText_Async(Image<Bgr, byte> img)
        {
            List<Rectangle> currentRectlist = await Task.Run(() => GetBoudingRectangles(img));
            // draw the rectangles
            foreach (var r in currentRectlist)
            {
                CvInvoke.Rectangle(img, r, new MCvScalar(0, 0, 255), 2);
            }

            rightPicture.Image = null; // delete the old image
            System.GC.Collect();
            rightPicture.Image = img.ToBitmap();
        }

        private async Task DetectTextIronOCR_Async(Bitmap bitmap)
        {
            // Image<Bgr, byte> img = bitmap.ToImage<Bgr, byte>();

            using (var Input = new OcrInput(bitmap))
            {
                Input.TargetDPI = 300;

                var Result = await Task.Run(() => Ocr.Read(Input));
                Image<Bgr, byte> img = Result.Pages[0].ContentAreaToBitmap(Input).ToImage<Bgr, byte>();

                foreach (var Line in Result.Lines)
                {
                    // only draw if the confidence is higher than 0%
                    if (Line.Confidence > 0 && !string.IsNullOrEmpty(Line.Text))
                    {
                        String LineText = Encoding.UTF8.GetString(Encoding.Default.GetBytes(Line.Text));
                        int LineX_location = Line.X;
                        int LineY_location = Line.Y;
                        int LineWidth = Line.Width;
                        int LineHeight = Line.Height;
                        Rectangle rect = new Rectangle(LineX_location, LineY_location, LineWidth, LineHeight);

                        CvInvoke.Rectangle(img, rect, new MCvScalar(0, 0, 255), 2);
                    }

                }

                rightPicture.Image = null; // delete the old image
                System.GC.Collect();
                Bitmap resized = new Bitmap(img.ToBitmap(), leftPicture.Size);
                rightPicture.Image = resized;
                // CvInvoke.Rectangle(img, brect, new MCvScalar(50, 50, 50), -1);
            }
        }
        #endregion

        #region "Translating Text"
        /**
         * Detect text in the image and draw Bounding Rectangles around it.
         */
        private async Task TranslateText(Image<Bgr, byte> img)
        {
            List<Rectangle> currentRectList = await Task.Run(() => GetBoudingRectangles(img));
            
            foreach (var brect in currentRectList)
            {
                // check if the area contains text in the rectangle
                using (var Input = new OcrInput())
                {
                    Input.AddImage(leftPicture.Image, brect);
                    // use the default value (https://ironsoftware.com/csharp/ocr/troubleshooting/x-and-y-coordinates-change/)
                    // Input.MinimumDPI = null;
                    Input.ToGrayScale();
                    
                    var Result = await Task.Run(() => Ocr.Read(Input));

                    // process the line of text
                    if (Result.Lines.Length > 0)
                    {
                        var Line = Result.Lines[0];
                        string LineText = Line.Text;
                        ///**
                        int LineX_location = Line.X;
                        int LineY_location = Line.Y;
                        int LineWidth = Line.Width;
                        int LineHeight = Line.Height;
                        double LineOcrAccuracy = Line.Confidence;

                        
                        string createText = String.Format("LineText: {0}, X: {1}, Y: {2}, Width: {3}, Height: {4}, Confidence: {5}\n",
                            LineText, LineX_location, LineY_location, LineWidth, LineHeight, LineOcrAccuracy);
                        // File.AppendAllText(@".\result.txt", createText, Encoding.UTF8);
                        //**/

                        Boolean containsText = !string.IsNullOrEmpty(LineText);

                        StringFormat strFormat = new StringFormat();
                        strFormat.Alignment = StringAlignment.Center;
                        strFormat.LineAlignment = StringAlignment.Center;

                        if (containsText)
                        {
                            // draw the rectangles
                            CvInvoke.Rectangle(img, brect, new MCvScalar(220, 220, 220), -1);

                            // draw the text
                            using (Graphics g = Graphics.FromImage(img.AsBitmap()))
                            {

                                g.DrawString(LineText, new Font("Times New Roman", 11), Brushes.Black, new RectangleF(brect.X, brect.Y, brect.Width, brect.Height), strFormat);
                            }
                        }
                    }
                    
                }
            }
            rightPicture.Image = null; // delete the old image
            System.GC.Collect();
            rightPicture.Image = img.ToBitmap();
        }

        

        private async Task TranslateText_IronOCR(Bitmap bitmap)
        {
            // Image<Bgr, byte> img = bitmap.ToImage<Bgr, byte>();
            
            using (var Input = new OcrInput(bitmap))
            {
                Input.TargetDPI = 300;

                var Result = await Task.Run(() => Ocr.Read(Input));
                Image<Bgr, byte> img = Result.Pages[0].ContentAreaToBitmap(Input).ToImage<Bgr, byte>();

                foreach (var Line in Result.Lines)
                {
                    // only draw if the confidence is higher than 0%
                    if (Line.Confidence > 0 && !string.IsNullOrEmpty(Line.Text))
                    {
                        String LineText = Encoding.UTF8.GetString(Encoding.Default.GetBytes(Line.Text));
                        int LineX_location = Line.X;
                        int LineY_location = Line.Y;
                        int LineWidth = Line.Width;
                        int LineHeight = Line.Height;
                        double LineOcrAccuracy = Line.Confidence;

                        //Console.WriteLine("LineText: {0}\nX: {1}, Y: {2}\nWidth: {3}, Height: {4}, Confidence: {5}"
                        //   , LineText, LineX_location, LineY_location, LineWidth, LineHeight, LineOcrAccuracy);

                        Rectangle rect = new Rectangle(LineX_location, LineY_location, LineWidth, LineHeight);

                        CvInvoke.Rectangle(img, rect, new MCvScalar(220, 220, 220), -1);
                    }
                    
                }

                rightPicture.Image = null; // delete the old image
                System.GC.Collect();
                Bitmap resized = new Bitmap(img.ToBitmap(), leftPicture.Size);
                rightPicture.Image = resized;
                // CvInvoke.Rectangle(img, brect, new MCvScalar(50, 50, 50), -1);
            }
        }
        #endregion
    }
}
