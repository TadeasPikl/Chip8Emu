using Chip8Emu.Emulator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
        public MainWindow()
        {
            InitializeComponent();

            // Initialize the emulator
            vm = new Vm(pictureBox1);
        }

        private void loadROMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "./";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    string filePath = openFileDialog.FileName;

                    var fileStream = openFileDialog.OpenFile();

                    vm.LoadRom(fileStream);
                }
            }

            StartMainLoop();
        }

        private async void StartMainLoop()
        {
            isRunning = true;

            await Task.Run(() =>
            {
                while (isRunning)
                {
                    // Update must be done on UI thread
                    Invoke((MethodInvoker)(() =>
                    {
                        vm.Cycle();
                    }));

                    // Frame timing
                    Thread.Sleep(2); // 2ms delay for 500Hz
                }
            });
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            isRunning = false;
        }

    }
}
