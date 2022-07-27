using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

using Emgu.CV;
using Emgu.CV.Structure;

using IronOcr;
using OcrLiteLib;

using GTranslate.Translators;
using System.Drawing.Drawing2D;
using System.IO;
using Emgu.CV.CvEnum;

namespace EmguCV_TextDetection
{
    public partial class Form1 : Form
    {
        private readonly string[] SizeSuffixes =
                   { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        private string memorySize, calculationTime;


        private PictureBox leftPicture, rightPicture;
        private OpenFileDialog file;

        private IronTesseract Ocr;
        private OcrLite ocrEngin;
        
        private AggregateTranslator translator;

        public Form1()
        {
            InitializeComponent();
            // set true, otherwise key press is swallowed by the control that has focus
            this.KeyPreview = true;
            // add KeyEvent to the form
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(KeyEvent);

            // initialize the memory size and calculation time
            memorySize = string.Empty;
            calculationTime = string.Empty;
            // set the initial text in the form
            labelCalculation.Text = calculationTime;
            labelMemory.Text = memorySize;

            leftPicture = new PictureBox();
            rightPicture = new PictureBox();

            leftPicture.SizeMode = PictureBoxSizeMode.AutoSize;
            rightPicture.SizeMode = PictureBoxSizeMode.AutoSize;

            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel2.AutoScroll = true;

            flowLayoutPanel1.Controls.Add(leftPicture);
            flowLayoutPanel2.Controls.Add(rightPicture);
            
            // initialize the Translator
            translator = new AggregateTranslator();
            
            // dislay various options for detecting and translating text
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
            if (methodChoices.Items.Count == 3)
            {
                if (methodChoices.SelectedIndex == 0)
                {
                    // load Onxx model
                    loadOnnxModel();
                }
                else
                {
                    if (Ocr == null)
                    {
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
                    }
                    
                }
            }
            
        }

        #region "Delegate"

        /**
         * A delegate function which accepts a Bitmap
         * Use Func when you want to use a delegate function with a return value
         * Use Action when you want to use a delegate function without a return value (void)
         * Example: https://stackoverflow.com/questions/2082615/pass-method-as-parameter-using-c-sharp
         */
        private async Task Delegate_Bitmap(Func<Bitmap, Task> action)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            
            Bitmap bitmap = (Bitmap)(leftPicture.Image);
            
            await action(bitmap);
            
            stopwatch.Stop();
            // This obtains the current application process
            Process thisProcess = Process.GetCurrentProcess();
            // This obtains the memory used by the process
            long usedMemory = thisProcess.WorkingSet64;
            CalculateStats(stopwatch, usedMemory);
        }

        /**
         * A delegate function which accepts a Bitmap
         * Use Func when you want to use a delegate function with a return value
         * Use Action when you want to use a delegate function without a return value (void)
         * Example: https://stackoverflow.com/questions/2082615/pass-method-as-parameter-using-c-sharp
         */
        private async Task Delegate_Image(Func<Image<Bgr, byte>, Task> action)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Bitmap bitmap = (Bitmap)(leftPicture.Image);
            Image<Bgr, byte> image = bitmap.ToImage<Bgr, byte>();

            await action(image);

            stopwatch.Stop();
            // This obtains the current application process
            Process thisProcess = Process.GetCurrentProcess();
            // This obtains the memory used by the process
            long usedMemory = thisProcess.WorkingSet64;
            CalculateStats(stopwatch, usedMemory);
        }


        #endregion

        #region "Click events"
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
                extractTextToolStripMenuItem.Enabled = true;
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
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();



                stopwatch.Stop();
                // This obtains the current application process
                Process thisProcess = Process.GetCurrentProcess();
                // This obtains the memory used by the process
                long usedMemory = thisProcess.WorkingSet64;
                CalculateStats(stopwatch, usedMemory);
            }
            else
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                Bitmap bm = (Bitmap)(leftPicture.Image);
                await DetectText_Async(bm.ToImage<Bgr, byte>());

                stopwatch.Stop();
                // This obtains the current application process
                Process thisProcess = Process.GetCurrentProcess();
                // This obtains the memory used by the process
                long usedMemory = thisProcess.WorkingSet64;
                CalculateStats(stopwatch, usedMemory);
            }
                
        }
        
        /**
         * 
         */
        private async void extractTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /**
             * EmguCV + Onnx = "0"
             * IronOCR Only = "1"
             * EmguCV + IronOCR = "2"
             */
            string selectedVal = (string)methodChoices.SelectedValue;
            if (selectedVal.Equals("0"))
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();



                stopwatch.Stop();
                // This obtains the current application process
                Process thisProcess = Process.GetCurrentProcess();
                // This obtains the memory used by the process
                long usedMemory = thisProcess.WorkingSet64;
                CalculateStats(stopwatch, usedMemory);
            }
            else
            {
                Bitmap bm = (Bitmap)(leftPicture.Image);
                if (selectedVal.Equals("1"))
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    await ExtractText_IronOCR(bm);

                    stopwatch.Stop();
                    // This obtains the current application process
                    Process thisProcess = Process.GetCurrentProcess();
                    // This obtains the memory used by the process
                    long usedMemory = thisProcess.WorkingSet64;
                    CalculateStats(stopwatch, usedMemory);
                }
                else if (selectedVal.Equals("2"))
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    await ExtractText(bm.ToImage<Bgr, byte>());

                    stopwatch.Stop();
                    // This obtains the current application process
                    Process thisProcess = Process.GetCurrentProcess();
                    // This obtains the memory used by the process
                    long usedMemory = thisProcess.WorkingSet64;
                    CalculateStats(stopwatch, usedMemory);
                    
                }
                    
            }
            
        }

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
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();



                stopwatch.Stop();
                // This obtains the current application process
                Process thisProcess = Process.GetCurrentProcess();
                // This obtains the memory used by the process
                long usedMemory = thisProcess.WorkingSet64;
                CalculateStats(stopwatch, usedMemory);
            }
            else
            {
                Bitmap bm = (Bitmap)(leftPicture.Image);
                if (selectedVal.Equals("1"))
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    await TranslateText_IronOCR(bm);

                    stopwatch.Stop();
                    // This obtains the current application process
                    Process thisProcess = Process.GetCurrentProcess();
                    // This obtains the memory used by the process
                    long usedMemory = thisProcess.WorkingSet64;
                    CalculateStats(stopwatch, usedMemory);
                }
                else if (selectedVal.Equals("2"))
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    await TranslateText(bm.ToImage<Bgr, byte>());

                    stopwatch.Stop();
                    // This obtains the current application process
                    Process thisProcess = Process.GetCurrentProcess();
                    // This obtains the memory used by the process
                    long usedMemory = thisProcess.WorkingSet64;
                    CalculateStats(stopwatch, usedMemory);

                }

            }
        }
        #endregion
        
        /**
         * Method to handle various methods from pressing down the key
         */
        private async void KeyEvent(object sender, KeyEventArgs e) //Keyup Event 
        {
            if (!(extractTextToolStripMenuItem.Enabled && detectTextToolStripMenuItem.Enabled))
            {
                if (e.KeyCode == Keys.F8 || e.KeyCode == Keys.F9 || e.KeyCode == Keys.F10)
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
            Bitmap bitmap = (Bitmap)(leftPicture.Image);
            if (selectedVal.Equals("0"))
            {
                // if we are currently using EmguCV + Onnx
                if (e.KeyCode == Keys.F8)
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    if (detectTextToolStripMenuItem.Enabled)
                    {
                        await ProcessText_Onnx(bitmap, bitmap.Width);
                    }
                    
                    stopwatch.Stop();
                    // This obtains the current application process
                    Process thisProcess = Process.GetCurrentProcess();
                    // This obtains the memory used by the process
                    long usedMemory = thisProcess.WorkingSet64;
                    CalculateStats(stopwatch, usedMemory);
                }
                else if (e.KeyCode == Keys.F9)
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    if (extractTextToolStripMenuItem.Enabled)
                    {
                        await ProcessText_Onnx(bitmap, bitmap.Width, extractText: true);
                    }
                    
                    stopwatch.Stop();
                    // This obtains the current application process
                    Process thisProcess = Process.GetCurrentProcess();
                    // This obtains the memory used by the process
                    long usedMemory = thisProcess.WorkingSet64;
                    CalculateStats(stopwatch, usedMemory);
                }
                else if (e.KeyCode == Keys.F10)
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    if (translateTextToolStripMenuItem.Enabled)
                    {
                        await ProcessText_Onnx(bitmap, bitmap.Width, translateText: true);
                    }

                    stopwatch.Stop();
                    // This obtains the current application process
                    Process thisProcess = Process.GetCurrentProcess();
                    // This obtains the memory used by the process
                    long usedMemory = thisProcess.WorkingSet64;
                    CalculateStats(stopwatch, usedMemory);
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
                        if (selectedVal.Equals("1"))
                        {
                            await Delegate_Bitmap(DetectTextIronOCR_Async);
                        }
                        else if (selectedVal.Equals("2"))
                        {
                            await Delegate_Image(DetectText_Async);
                        }
                    }
                }
                else if (e.KeyCode == Keys.F9)
                {
                    // extract the text when press F9
                    if (extractTextToolStripMenuItem.Enabled)
                    {
                        if (selectedVal.Equals("1"))
                        {
                            await Delegate_Bitmap(ExtractText_IronOCR);
                        }
                        else if (selectedVal.Equals("2"))
                        {
                            await Delegate_Image(ExtractText);
                        }
                    }
                }
                else if (e.KeyCode == Keys.F10)
                {
                    // translate the text when press F10
                    if (translateTextToolStripMenuItem.Enabled)
                    {
                        if (selectedVal.Equals("1"))
                        {
                            await Delegate_Bitmap(TranslateText_IronOCR);
                        }
                        else if (selectedVal.Equals("2"))
                        {
                            await Delegate_Image(TranslateText);
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
         * A converter method to convert bytes to the appropriate size (kb, mb, etc.)
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

        /**
         * Update both strings that contain the statistics of the current algorithm.
         * TimeSpan method taken from 
         * https://stackoverflow.com/questions/51826732/is-there-a-concise-way-to-achieve-conditional-pluralization-of-timespan-format
         */
        private void CalculateStats(Stopwatch stopwatch, long memory)
        {
            // convert the time from ms to anything conditionally
            TimeSpan ts = new TimeSpan(stopwatch.ElapsedTicks);
            string[] TFormat = new[] {
                (ts.Days > 1 ? ts.Days + " Days" : (ts.Days == 1 ? "1 Day" : "")),
                (ts.Hours > 1 ? " "+ ts.Hours + " Hours" : (ts.Hours == 1 ? " 1 Hour" : "")),
                (ts.Minutes > 1 ? " " + ts.Minutes + " Minutes" : (ts.Minutes == 1 ? " 1 Minute" : "")),
                (ts.Seconds > 1 ? " " + ts.Seconds + " Seconds" : (ts.Seconds == 1 ? " 1 Second" : "")),
                (ts.TotalMilliseconds < 1000 ? $"{ts.TotalMilliseconds }ms" : "")
            };
            calculationTime = String.Format($"{TFormat[0]}{TFormat[1]}{TFormat[2]}{TFormat[3]}{TFormat[4]}".TrimStart());
            memorySize = SizeSuffix(memory);
            UpdateStatsForm();
        }

        /**
         * Helper Method: Update the stats form with the current stats
         */
        private void UpdateStatsForm()
        {
            labelCalculation.Text = calculationTime;
            labelMemory.Text = memorySize;
        }

        /**
         * Helper Method: print out the calculation time and the memory usage
         */
        private void PrintStats()
        {
            Console.WriteLine("Calculation Time: " + calculationTime);
            Console.WriteLine("Memory Usage: " + memorySize);
        }
        
        /**
         * Helper method: Utilizes GTranslate library to detect language and translate to English
         */
        private async Task<string> Translate(string text)
        {
            var result = await translator.TranslateAsync(text, "en");

            return result.Translation;
        }

        /**
         * Change the text font size if it does not fit vertically
         */
        private Font changeTextFont(Graphics g, Font font, string translatedText, Rectangle rect)
        {
            var textSize = g.MeasureString(translatedText, font);
            bool fitVertically = textSize.Height <= rect.Height;
            while (!fitVertically)
            {
                font = new Font(font.FontFamily, font.Size - 1, font.Style);
                textSize = g.MeasureString(translatedText, font);
                fitVertically = textSize.Height <= rect.Height;
            }
            System.GC.Collect();
            return font;
        }

        /**
         * Helper method to draw the rectangles and translated text on the image
         * If the text is too long, it will be resized horizontally
         */
        private void drawTranslatedText(Graphics g, Brush brush, Font font, string translatedText, Rectangle rect)
        {
            // change the font size if it does not fit vertically
            font = changeTextFont(g, font, translatedText, rect);
            
            // improve the quality of the text rendering
            g.InterpolationMode = InterpolationMode.High;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // fill in the background of the rectangle
            g.FillRectangle(brush, rect);
            var textSize = g.MeasureString(translatedText, font);
            if (textSize.Width > rect.Width)
            {
                // if the text is too wide, scale it down horizontally
                var state = g.Save();
                g.TranslateTransform(rect.Left, rect.Top);
                // scale horizontally, keep the vertical size the same
                Console.WriteLine("The ratio is {0} and text: {1}", (float)(rect.Width / textSize.Width),  translatedText);
                g.ScaleTransform((float)(rect.Width / textSize.Width), 1);
                g.DrawString(translatedText, font, Brushes.Black, PointF.Empty);
                g.Restore(state);
            }
            else
            {
                // if the text is smaller than the rectangle, write it
                g.DrawString(translatedText, font, Brushes.Black, rect);
            }
        }

        /**
         * Helper Method to load ONNX model
         */
        private void loadOnnxModel(
            string detName = "dbnet.onnx",
            string clsName = "angle_net.onnx",
            string recName = "crnn_lite_lstm.onnx",
            string keysName = "keys.txt",
            int numThread = 4
            )
        {

            string appPath = AppDomain.CurrentDomain.BaseDirectory;
            string modelsDir = appPath + "models";
            // modelsTextBox.Text = modelsDir;
            string detPath = modelsDir + "\\" + detName;
            string clsPath = modelsDir + "\\" + clsName;
            string recPath = modelsDir + "\\" + recName;
            string keysPath = modelsDir + "\\" + keysName;
            bool isDetExists = File.Exists(detPath);
            if (!isDetExists)
            {
                MessageBox.Show("Model file not found at: " + detPath);
            }
            bool isClsExists = File.Exists(clsPath);
            if (!isClsExists)
            {
                MessageBox.Show("Model file not found at: " + clsPath);
            }
            bool isRecExists = File.Exists(recPath);
            if (!isRecExists)
            {
                MessageBox.Show("Model file not found at: " + recPath);
            }
            bool isKeysExists = File.Exists(keysPath);
            if (!isKeysExists)
            {
                MessageBox.Show("Keys file not found at: " + keysPath);
            }
            if (isDetExists && isClsExists && isRecExists && isKeysExists)
            {
                if (ocrEngin != null)
                {
                    ocrEngin = null;
                    System.GC.Collect();
                }
                ocrEngin = new OcrLite();
                ocrEngin.InitModels(detPath, clsPath, recPath, keysPath, (int)numThread);
            }
            else
            {
                MessageBox.Show("Initialization failed, please confirm the model folder and file, and then reinitialize!");
            }
            System.GC.Collect();
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

        #region "Extracting Text"
        /**
         * Detect text in the image and draw Bounding Rectangles around it.
         * Using IronOCR to get bounding rectangles + EmguCV for extracting text
         */
        private async Task ExtractText(Image<Bgr, byte> img)
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

        /**
         * Detect text in the image and draw Bounding Rectangles around it.
         * Using IronOCR to get both bounding rectangles and extracting text
         */
        private async Task ExtractText_IronOCR(Bitmap bitmap)
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
                        String LineText = Line.Text;
                        int LineX_location = Line.X;
                        int LineY_location = Line.Y;
                        int LineWidth = Line.Width;
                        int LineHeight = Line.Height;
                        double LineOcrAccuracy = Line.Confidence;

                        //Console.WriteLine("LineText: {0}\nX: {1}, Y: {2}\nWidth: {3}, Height: {4}, Confidence: {5}"
                        //   , LineText, LineX_location, LineY_location, LineWidth, LineHeight, LineOcrAccuracy);

                        // draw the background
                        Rectangle rect = new Rectangle(LineX_location, LineY_location, LineWidth, LineHeight);
                        CvInvoke.Rectangle(img, rect, new MCvScalar(220, 220, 220), -1);

                        // define the string format
                        StringFormat strFormat = new StringFormat();
                        strFormat.Alignment = StringAlignment.Center;
                        strFormat.LineAlignment = StringAlignment.Center;
                        
                        // draw the text
                        using (Graphics g = Graphics.FromImage(img.AsBitmap()))
                        {
                            g.DrawString(LineText, new Font("Times New Roman", 24), Brushes.Black, rect, strFormat);
                        }
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

        Font font = new Font("Tahoma", 18, GraphicsUnit.Point);

        foreach (var rect in currentRectList)
        {
            // check if the area contains text in the rectangle
            using (var Input = new OcrInput())
            {
                Input.AddImage(leftPicture.Image, rect);
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

                    Boolean containsText = !string.IsNullOrEmpty(LineText);
                    if (containsText)
                    {
                        // draw the text
                        using (Graphics g = Graphics.FromImage(img.AsBitmap()))
                        {
                            int alpha = 255; // from 0-255, 128 is 50% opacity
                            int red = 220, green = 220, blue = 220;
                            using (Brush brush = new SolidBrush(Color.FromArgb(alpha, red, green, blue)))
                            {
                                var translatedText = await Task.Run(() => Translate(LineText).Result);
                                drawTranslatedText(g, brush, font, translatedText, rect);
                            }
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
        using (var Input = new OcrInput(bitmap))
        {
            Input.TargetDPI = 300;

            var Result = await Task.Run(() => Ocr.Read(Input));
            Image<Bgr, byte> img = Result.Pages[0].ContentAreaToBitmap(Input).ToImage<Bgr, byte>();
            Font font = new Font("Tahoma", 24, GraphicsUnit.Point);

            foreach (var Line in Result.Lines)
            {
                // only draw if the confidence is higher than 0%
                if (Line.Confidence > 0 && !string.IsNullOrEmpty(Line.Text))
                {
                    String LineText = Line.Text;
                    int LineX_location = Line.X;
                    int LineY_location = Line.Y;
                    int LineWidth = Line.Width;
                    int LineHeight = Line.Height;

                    // draw the background
                    Rectangle rect = new Rectangle(LineX_location, LineY_location, LineWidth, LineHeight);

                    // draw the text
                    using (Graphics g = Graphics.FromImage(img.AsBitmap()))
                    {
                        int alpha = 255; // from 0-255, 128 is 50% opacity
                        int red = 220, green = 220, blue = 220;
                        using (Brush brush = new SolidBrush(Color.FromArgb(alpha, red, green, blue)))
                        {
                            var translatedText = await Task.Run(() => Translate(LineText).Result);
                            drawTranslatedText(g, brush, font, translatedText, rect);
                        }
                    }
                }

            }
                
            rightPicture.Image = null; // delete the old image
            System.GC.Collect();
            Bitmap resized = new Bitmap(img.ToBitmap(), leftPicture.Size);
            rightPicture.Image = resized;
        }
    }
        #endregion

        /**
         * Detect text in the image and draw Bounding Rectangles around it.
         * Using IronOCR to get both bounding rectangles and Onnx model for extracting text
         */
        private async Task ProcessText_Onnx(
            Bitmap bitmap,
            int imgResize,
            int padding = 50,
            float boxScoreThresh = 0.618f,
            float boxThresh = 0.300f,
            float unClipRatio = 2.0f,
            bool doAngle = true,
            bool mostAngle = true,
            bool extractText = false,
            bool translateText = false
            )
        {
            if (ocrEngin == null)
            {
                MessageBox.Show("OCR Engine is uninitialized, cannot execute!");
                return;
            }
            Image<Bgr, byte> imageCV = bitmap.ToImage<Bgr, byte>(); //Image Class from Emgu.CV
            Mat mat = imageCV.Mat;

            OcrLiteLib.OcrResult ocrResult = await Task.Run(() => ocrEngin.Detect(
                mat, padding, imgResize, boxScoreThresh, boxThresh, unClipRatio,
                doAngle, mostAngle, extractText, translateText, translator));
            rightPicture.Image = ocrResult.BoxImg.ToBitmap();
            
        }
    }
}
