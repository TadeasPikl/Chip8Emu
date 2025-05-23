using Chip8Emu.Emulator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Threading.Timer;

namespace Chip8Emu
{
    public partial class MainWindow : Form
    {
        private Vm vm;
        private bool isRunning = false;
        private CancellationTokenSource? mainLoopCts;
        
        private int cycleCounter = 0;
        private Timer updateTimer;
        private Stopwatch frequencyStopwatch = new Stopwatch();

        private System.Windows.Forms.Timer displayRefreshTimer;
        private Bitmap latestFrame;

        public ChipMode CurrentChipMode = ChipMode.CHIP8; // Default to CHIP-8 mode
        public float FrameTiming = 700.0f; // 700Hz timing

        public MainWindow()
        {
            InitializeComponent();

            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(MainWindow_KeyDown);
            this.KeyUp += new KeyEventHandler(MainWindow_KeyUp);

            this.ResizeEnd += new EventHandler(MainWindow_ResizeEnd);

            // Initialize the emulator
            vm = new Vm(CurrentChipMode, pictureBox1);
            vm.FrameTiming = FrameTiming;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            frequencyStopwatch.Start();
            updateTimer = new System.Threading.Timer(UpdateFrequency, null, 0, 1000);

            displayRefreshTimer = new System.Windows.Forms.Timer();
            displayRefreshTimer.Interval = 16;
            displayRefreshTimer.Tick += (s, e) => RenderDisplay();
            displayRefreshTimer.Start();
        }

        private void RenderDisplay()
        {
            if (vm == null || vm.DisplayPictureBox == null) return;

            var rawBitmap = vm.BuildDisplayBitmap();

            // PictureBox dimensions
            int boxWidth = vm.DisplayPictureBox.Width;
            int boxHeight = vm.DisplayPictureBox.Height;

            float sourceAspect = 64f / 32f;
            float boxAspect = (float)boxWidth / boxHeight;

            int targetWidth, targetHeight;
            if (boxAspect > sourceAspect)
            {
                targetHeight = boxHeight;
                targetWidth = (int)(targetHeight * sourceAspect);
            }
            else
            {
                targetWidth = boxWidth;
                targetHeight = (int)(targetWidth / sourceAspect);
            }

            Bitmap finalImage = new Bitmap(boxWidth, boxHeight);
            using (Graphics g = Graphics.FromImage(finalImage))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                g.Clear(Color.Black);

                int offsetX = (boxWidth - targetWidth) / 2;
                int offsetY = (boxHeight - targetHeight) / 2;
                g.DrawImage(rawBitmap, new Rectangle(offsetX, offsetY, targetWidth, targetHeight));
            }

            vm.DisplayPictureBox.Image = finalImage;
        }


        private void UpdateFrequency(object state)
        {
            double seconds = frequencyStopwatch.Elapsed.TotalSeconds;
            int count = Interlocked.Exchange(ref cycleCounter, 0);
            frequencyStopwatch.Restart();

            double frequencyHz = seconds > 0 ? count / seconds : 0;

            this.BeginInvoke((Action)(() =>
            {
                frequencyLabel.Text = $"{frequencyHz:F2} Hz";
            }));
        }

        private void MainWindow_KeyDown(object? sender, KeyEventArgs e)
        {
            byte? keyByte = MapKey(e.KeyCode);
            if (keyByte.HasValue)
            {
                vm.KeyDown((byte)keyByte);
            }

        }
        private void MainWindow_KeyUp(object? sender, KeyEventArgs e)
        {
            byte? keyByte = MapKey(e.KeyCode);
            if (keyByte.HasValue)
            {
                vm.KeyUp((byte)keyByte);
            }
        }

        private byte? MapKey(Keys key)
        {
            switch (key)
            {
                case Keys.D1: return 0x1;
                case Keys.D2: return 0x2;
                case Keys.D3: return 0x3;
                case Keys.D4: return 0xC;

                case Keys.Q: return 0x4;
                case Keys.W: return 0x5;
                case Keys.E: return 0x6;
                case Keys.R: return 0xD;

                case Keys.A: return 0x7;
                case Keys.S: return 0x8;
                case Keys.D: return 0x9;
                case Keys.F: return 0xE;

                case Keys.Y: return 0xA;
                case Keys.X: return 0x0;
                case Keys.C: return 0xB;
                case Keys.V: return 0xF;

                default: return null;
            }
        }

        private void MainWindow_ResizeEnd(object? sender, EventArgs e)
        {
            if (vm != null)
            {
                vm.DrawFlag = true;
            }
        }

        private void loadROMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "./";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Cancel the previous main loop if running
                    StopMainLoop();

                    string filePath = openFileDialog.FileName;
                    var fileStream = openFileDialog.OpenFile();

                    vm.LoadRom(fileStream);

                    StartMainLoop();
                }
            }
        }

        private void StopMainLoop()
        {
            if (mainLoopCts != null)
            {
                isRunning = false;
                mainLoopCts.Cancel();
                mainLoopCts.Dispose();
                mainLoopCts = null;
            }
        }

        private async void StartMainLoop()
        {
            isRunning = true;
            mainLoopCts = new CancellationTokenSource();
            var token = mainLoopCts.Token;

            await Task.Run(() =>
            {
                double tickFrequency = Stopwatch.Frequency;

                while (isRunning && !token.IsCancellationRequested)
                {
                    long startTicks = Stopwatch.GetTimestamp();

                    vm.Cycle();
                    Interlocked.Increment(ref cycleCounter);

                    long endTicks = Stopwatch.GetTimestamp();
                    double elapsedMs = (endTicks - startTicks) * 1000.0 / tickFrequency;
                    double targetFrameTimeMs = 1000.0 / FrameTiming;

                    double sleepTime = targetFrameTimeMs - elapsedMs;
                    if (sleepTime > 2.0)
                    {
                        Thread.Sleep((int)sleepTime); // Coarse sleep
                    }
                    else
                    {
                        Stopwatch sw = Stopwatch.StartNew();
                        while (sw.Elapsed.TotalMilliseconds < sleepTime)
                        {
                            Thread.SpinWait(10); // Small CPU burn loop
                        }
                    }
                }
            }, token);
        }


        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            isRunning = false;
            base.OnFormClosing(e);
        }

    }
}
