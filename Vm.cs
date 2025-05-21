using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8Emu
{
    internal class Vm
    {
        private static byte[] Fonts =
        {
              0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
              0x20, 0x60, 0x20, 0x20, 0x70, // 1
              0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
              0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
              0x90, 0x90, 0xF0, 0x10, 0x10, // 4
              0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
              0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
              0xF0, 0x10, 0x20, 0x40, 0x40, // 7
              0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
              0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
              0xF0, 0x90, 0xF0, 0x90, 0x90, // A
              0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
              0xF0, 0x80, 0x80, 0x80, 0xF0, // C
              0xE0, 0x90, 0x90, 0x90, 0xE0, // D
              0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
              0xF0, 0x80, 0xF0, 0x80, 0x80  // F
        };

        public byte[] Memory;

        public byte[] Registers;
        public ushort I; // Index register
        public byte DelayTimer;
        public byte SoundTimer;

        public ushort PC; // Program Counter
        public byte SP; // Stack Pointer

        public Stack<ushort> Stack;

        public bool[] Display; // 64x32 display


        public Vm()
        {
            Memory = new byte[4096];
            Registers = new byte[16];
            I = 0;
            DelayTimer = 0;
            SoundTimer = 0;
            PC = 0x200;
            SP = 0;
            Stack = new Stack<ushort>();
            Display = new bool[64 * 32];
            for (int i = 0; i < Fonts.Length; i++) // Load fontset into memory
            {
                Memory[i] = Fonts[i];
            }
        }

        public void LoadRom(byte[] rom)
        {
            for (int i = 0; i < rom.Length; i++)
            {
                Memory[0x200 + i] = rom[i];
            }
        }

        public void PrintDisplayToConsole()
        {
            Console.Clear();
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    Console.Write(Display[y * 64 + x] ? "■" : " ");
                }
                Console.WriteLine();
            }
        }

        public void Cycle()
        {
            // Fetch
            ushort opcode = (ushort)((Memory[PC] << 8) | Memory[PC + 1]);
            PC += 2;
            // Decode and Execute
            switch (opcode & 0xF000)
            {
                case 0x0000:
                    switch (opcode & 0x00FF)
                    {
                        case 0x00E0: // CLS
                            // Clear the display.
                            break;
                        case 0x00EE: // RET
                            // Return from a subroutine.
                            PC = Stack.Pop();
                            break;
                        default: // SYS addr
                            break;
                    }
                    break;

                case 0x1000: // JP addr
                    // Jump to location nnn
                    PC = (ushort)(opcode & 0x0FFF);
                    break;

                
                case 0x6000: // LD Vx, byte
                    // Set Vx = kk.
                    Registers[(opcode & 0x0F00) >> 8] = (byte)(opcode & 0x00FF);
                    break;

                case 0xA000: // LD I, addr
                    // Set I = nnn.
                    I = (ushort)(opcode & 0x0FFF);
                    break;

                case 0xD000: // DRW Vx, Vy, nibble
                    // Display n-byte sprite starting at memory location I at (Vx, Vy), set VF = collision.
                    byte x = Registers[(opcode & 0x0F00) >> 8];
                    byte y = Registers[(opcode & 0x00F0) >> 4];
                    byte height = (byte)(opcode & 0x000F);
                    for (int row = 0; row < height; row++)
                    {
                        byte sprite = Memory[I + row];
                        for (int col = 0; col < 8; col++)
                        {
                            if ((sprite & (0x80 >> col)) != 0)
                            {
                                int pixelIndex = ((y + row) % 32) * 64 + ((x + col) % 64);
                                Display[pixelIndex] ^= true;
                            }
                        }
                    }
                    break;

                default:
                    break;
            }

            // Update timers TODO


            PrintDisplayToConsole();
        }
    }
}
