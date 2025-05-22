using Chip8Emu.Emulator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chip8Emu
{
    public partial class MainWindow : Form
    {
        private Vm vm;
        private bool isRunning = false;
        private CancellationTokenSource? mainLoopCts;

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
                case Keys.NumPad1:  return 0x1;
                case Keys.NumPad2:  return 0x2;
                case Keys.NumPad3:  return 0x3;
                case Keys.Add:      return 0xC;

                case Keys.NumPad4:  return 0x4;
                case Keys.NumPad5:  return 0x5;
                case Keys.NumPad6:  return 0x6;
                case Keys.Subtract: return 0xD;

                case Keys.NumPad7:  return 0x7;
                case Keys.NumPad8:  return 0x8;
                case Keys.NumPad9:  return 0x9;
                case Keys.Multiply: return 0xE;

                case Keys.NumPad0:  return 0xA;
                case Keys.Decimal:  return 0x0;
                case Keys.Divide:   return 0xB;
                case Keys.NumLock:  return 0xF;
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
                while (isRunning && !token.IsCancellationRequested)
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    long lastTicks = stopwatch.ElapsedTicks;
                    double tickFrequency = Stopwatch.Frequency;

                    if (!this.IsDisposed && this.IsHandleCreated)
                    {
                        try
                        {
                            Invoke((MethodInvoker)(() =>
                            {
                                vm.Cycle();
                            }));
                        }
                        catch (ObjectDisposedException)
                        {
                            isRunning = false;
                        }
                    }
                    else
                    {
                        isRunning = false;
                    }
                    long currentTicks = stopwatch.ElapsedTicks;

                    double timeToWait = 1 / FrameTiming * 1000 - ((currentTicks - lastTicks) / tickFrequency * 1000);

                    if (timeToWait > 0)
                    {
                        Thread.Sleep(Convert.ToInt32(timeToWait));
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
