using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8Emu.Emulator
{
    public enum ChipMode { CHIP8, SUPERCHIP }

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

        private byte[] Memory;

        private byte[] Registers;
        private ushort I; // Index register
        private byte DelayTimer;
        public byte SoundTimer;
        private float TimerAccumulator = 0.0f;

        private ushort PC; // Program Counter

        private Stack<ushort> Stack;

        public bool[] Keypad; // 16 keys (0x0 to 0xF)

        public bool[,] Display; // 64x32 display
        public PictureBox DisplayPictureBox; // PictureBox for graphics output
        public bool DrawFlag = true;

        private Random VmRand = new Random();

        // Settings
        public ChipMode ChipMode;
        public float FrameTiming = 700.0f;
        public Color ForegroundColor = Color.White;
        public Color BackgroundColor = Color.Black;

        public void ResetVm()
        {
            Memory = new byte[4096];
            Registers = new byte[16];
            I = 0;
            DelayTimer = 0;
            SoundTimer = 0;
            PC = 0x200;
            Stack = new Stack<ushort>();
            Keypad = new bool[16]; // 0x0 to 0xF
            Display = new bool[64,32];
            DrawFlag = true;
            for (int i = 0; i < Fonts.Length; i++) // Load fontset into memory
            {
                Memory[i] = Fonts[i];
            }
        }

        public Vm(ChipMode chipMode, PictureBox pictureBoxDisplay)
        {
            ResetVm();
            ChipMode = chipMode;
            DisplayPictureBox = pictureBoxDisplay;
        }

        public void LoadRom(Stream romStream)
        {
            ResetVm();

            using (BinaryReader reader = new BinaryReader(romStream))
            {
                // Read the ROM into memory starting at address 0x200
                for (int i = 0; i < romStream.Length; i++)
                {
                    Memory[0x200 + i] = reader.ReadByte();
                }
            }
        }

        public void KeyDown(byte key)
        {
            if (key < 16)
            {
                Keypad[key] = true;
            }
        }

        public void KeyUp(byte key)
        {
            if (key < 16)
            {
                Keypad[key] = false;
            }
        }

        public Bitmap BuildDisplayBitmap()
        {
            Bitmap bitmap = new Bitmap(64, 32);
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    bitmap.SetPixel(x, y, Display[x, y] ? ForegroundColor : BackgroundColor);
                }
            }
            return bitmap;
        }


        public void Cycle()
        {
            // Fetch
            ushort opcode = (ushort)(Memory[PC] << 8 | Memory[PC + 1]);
            PC += 2;
            // Decode and Execute
            switch (opcode & 0xF000)
            {
                case 0x0000:
                    switch (opcode & 0x00FF)
                    {
                        case 0x00E0: // CLS
                            // Clear the display.
                            Display = new bool[64, 32];
                            DrawFlag = true;
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
                    // Jump to location nnn.
                    PC = (ushort)(opcode & 0x0FFF);
                    break;

                case 0x2000: // CALL addr
                    // Call subroutine at nnn.
                    Stack.Push(PC);
                    PC = (ushort)(opcode & 0x0FFF);
                    break;

                case 0x3000: // SE Vx, byte
                    // Skip next instruction if Vx = kk.
                    if (Registers[(opcode & 0x0F00) >> 8] == (byte)(opcode & 0x00FF))
                    {
                        PC += 2;
                    }
                    break;

                case 0x4000: // SNE Vx, byte
                    // Skip next instruction if Vx != kk.
                    if (Registers[(opcode & 0x0F00) >> 8] != (byte)(opcode & 0x00FF))
                    {
                        PC += 2;
                    }
                    break;

                case 0x5000: // SE Vx, Vy
                    // Skip next instruction if Vx != Vy.
                    if (Registers[(opcode & 0x0F00) >> 8] == Registers[(opcode & 0x00F0) >> 4])
                    {
                        PC += 2;
                    }
                    break;

                case 0x6000: // LD Vx, byte
                    // Set Vx = kk.
                    Registers[(opcode & 0x0F00) >> 8] = (byte)(opcode & 0x00FF);
                    break;

                case 0x7000: // ADD Vx, byte
                    // Set Vx = Vx + kk (carry flag is not changed).
                    Registers[(opcode & 0x0F00) >> 8] += (byte)(opcode & 0x00FF);
                    break;

                case 0x8000:
                    switch (opcode & 0x000F)
                    {
                        case 0x0000: // LD Vx, Vy
                            // Set Vx = Vy.
                            Registers[(opcode & 0x0F00) >> 8] = Registers[(opcode & 0x00F0) >> 4];
                            break;
                        case 0x0001: // OR Vx, Vy
                            // Set Vx = Vx OR Vy.
                            Registers[(opcode & 0x0F00) >> 8] |= Registers[(opcode & 0x00F0) >> 4];
                            Registers[0xF] = 0;
                            break;
                        case 0x0002: // AND Vx, Vy
                            // Set Vx = Vx AND Vy.
                            Registers[(opcode & 0x0F00) >> 8] &= Registers[(opcode & 0x00F0) >> 4];
                            Registers[0xF] = 0;
                            break;
                        case 0x0003: // XOR Vx, Vy
                            // Set Vx = Vx XOR Vy.
                            Registers[(opcode & 0x0F00) >> 8] ^= Registers[(opcode & 0x00F0) >> 4];
                            Registers[0xF] = 0;
                            break;
                        case 0x0004: // ADD Vx, Vy
                            // Set Vx = Vx + Vy, set VF = carry.
                            ushort sum = (ushort)(Registers[(opcode & 0x0F00) >> 8] + Registers[(opcode & 0x00F0) >> 4]);
                            Registers[(opcode & 0x0F00) >> 8] = (byte)sum;
                            Registers[0xF] = sum > 255 ? (byte)1 : (byte)0; // Set carry flag
                            break;
                        case 0x0005: // SUB Vx, Vy
                            // Set Vx = Vx - Vy, set VF = NOT borrow.
                            byte xa = Registers[(opcode & 0x0F00) >> 8];
                            byte ya = Registers[(opcode & 0x00F0) >> 4];
                            Registers[(opcode & 0x0F00) >> 8] = (byte)(xa - ya);
                            Registers[0xF] = (byte)(xa >= ya ? 1 : 0);
                            break;
                        case 0x0006: // SHR Vx {, Vy}
                            // Set Vx = Vx SHR 1.
                            int x86 = (opcode & 0x0F00) >> 8;
                            int y86 = (opcode & 0x00F0) >> 4;

                            byte value86 = Registers[x86];
                            if (ChipMode == ChipMode.CHIP8)
                            {
                                value86 = Registers[y86];
                            }

                            byte changeF86 = (byte)(value86 & 0x1);
                            Registers[x86] = (byte)(value86 >> 1);
                            Registers[0xF] = changeF86;
                            break;
                        case 0x0007: // SUBN Vx, Vy
                            // Set Vx = Vy - Vx, set VF = NOT borrow.
                            Registers[(opcode & 0x0F00) >> 8] = (byte)(Registers[(opcode & 0x00F0) >> 4] - Registers[(opcode & 0x0F00) >> 8]);
                            Registers[0xF] = (byte)((Registers[(opcode & 0x00F0) >> 4] > Registers[(opcode & 0x0F00) >> 8]) ? 1 : 0);
                            break;
                        case 0x000E: // SHL Vx {, Vy}
                            // Set Vx = Vx SHL 1.
                            int x8E = (opcode & 0x0F00) >> 8;
                            int y8E = (opcode & 0x00F0) >> 4;

                            byte value8E = Registers[x8E];
                            if (ChipMode == ChipMode.CHIP8)
                            {
                                value8E = Registers[y8E];
                            }

                            byte changeF8E = (byte)((value8E & 0x80) >> 7);
                            Registers[x8E] = (byte)(value8E << 1);
                            Registers[0xF] = changeF8E;
                            break;
                    }
                    break;

                case 0x9000: // SNE Vx, Vy
                    // Skip next instruction if Vx != Vy.
                    if (Registers[(opcode & 0x0F00) >> 8] != Registers[(opcode & 0x00F0) >> 4])
                    {
                        PC += 2;
                    }
                    break;

                case 0xA000: // LD I, addr
                    // Set I = nnn.
                    I = (ushort)(opcode & 0x0FFF);
                    break;

                case 0xB000: // JP V0, addr
                    // Jump to location nnn + V0.
                    PC = (ushort)((opcode & 0x0FFF) + Registers[0]);
                    break;

                case 0xC000: // RND Vx, byte
                    // Set Vx = random byte AND kk.
                    Registers[(opcode & 0x0F00) >> 8] = (byte)(VmRand.Next(0, 256) & (opcode & 0x00FF));
                    break;

                case 0xD000: // DRW Vx, Vy, nibble
                    // Display n-byte sprite starting at memory location I at (Vx, Vy), set VF = collision.
                    byte x = (byte)(Registers[(opcode & 0x0F00) >> 8] & 63); // VX & 63
                    byte y = (byte)(Registers[(opcode & 0x00F0) >> 4] & 31); // VY & 31
                    byte height = (byte)(opcode & 0x000F); // N
                    Registers[0xF] = 0;

                    for (int row = 0; row < height; row++)
                    {
                        byte spriteByte = Memory[I + row];
                        int posY = y + row;
                        if (posY >= 32) break; // Stop if bottom edge reached

                        for (int bit = 0; bit < 8; bit++)
                        {
                            int posX = x + bit;
                            if (posX >= 64) break; // Stop if right edge reached

                            // Get the sprite pixel
                            bool spritePixel = (spriteByte & (0x80 >> bit)) != 0;

                            if (spritePixel)
                            {
                                // XOR the pixel on screen
                                if (Display[posX, posY])
                                {
                                    Display[posX, posY] = false;
                                    Registers[0xF] = 1;
                                }
                                else
                                {
                                    Display[posX, posY] = true;
                                }
                            }
                        }
                    }

                    DrawFlag = true; // Signal that the screen should be redrawn
                    break;

                case 0xE000: // EX
                    switch (opcode & 0x00FF)
                    {
                        case 0x009E: // SKP Vx
                            // Skip next instruction if key with the value of Vx is pressed.
                            if (Keypad[Registers[(opcode & 0x0F00) >> 8]])
                            {
                                PC += 2;
                            }
                            break;
                        case 0x00A1: // SKNP Vx
                            // Skip next instruction if key with the value of Vx is not pressed.
                            if (!Keypad[Registers[(opcode & 0x0F00) >> 8]])
                            {
                                PC += 2;
                            }
                            break;
                    }
                    break;

                case 0xF000:
                    switch (opcode & 0x00FF)
                    {
                        case 0x0007: // LD Vx, DT
                            // Set Vx = delay timer value.
                            Registers[(opcode & 0x0F00) >> 8] = DelayTimer;
                            break;
                        case 0x000A: // LD Vx, K
                            // Wait for a key press, store the value of the key in Vx.
                            bool keyPressed = false;
                            for (byte i = 0; i < Keypad.Length; i++)
                            {
                                if (Keypad[i])
                                {
                                    keyPressed = true;
                                    Registers[(opcode & 0x0F00) >> 8] = i;
                                    break;
                                }
                            }
                            if (!keyPressed)
                            {
                                PC -= 2; // If no key is pressed, decrement PC to wait for a key press.
                            }
                            break;
                        case 0x0015: // LD DT, Vx
                            // Set delay timer = Vx.
                            DelayTimer = Registers[(opcode & 0x0F00) >> 8];
                            break;
                        case 0x0018: // LD ST, Vx
                            // Set sound timer = Vx.
                            SoundTimer = Registers[(opcode & 0x0F00) >> 8];
                            break;
                        case 0x001E: // ADD I, Vx
                            // Set I = I + Vx.
                            I += Registers[(opcode & 0x0F00) >> 8];
                            break;
                        case 0x0029: // LD F, Vx
                            // Set I = location of sprite for digit Vx.
                            I = (ushort)(Registers[(opcode & 0x0F00) >> 8] * 5);
                            break;
                        case 0x0033: // LD B, Vx
                            // Store BCD representation of Vx in memory locations I, I+1, and I+2.
                            Memory[I] = (byte)(Registers[(opcode & 0x0F00) >> 8] / 100);
                            Memory[I + 1] = (byte)((Registers[(opcode & 0x0F00) >> 8] / 10) % 10);
                            Memory[I + 2] = (byte)(Registers[(opcode & 0x0F00) >> 8] % 10);
                            break;
                        case 0x0055: // LD [I], Vx
                            // Store registers V0 through Vx in memory starting at location I.
                            for (int j = 0; j <= (opcode & 0x0F00) >> 8; j++)
                            {
                                Memory[I + j] = Registers[j];
                            }
                            // I is set to I + x + 1 after operation
                            I += (ushort)(((opcode & 0x0F00) >> 8) + 1);
                            break;
                        case 0x0065: // LD Vx, [I]
                            // Read registers V0 through Vx from memory starting at location I.
                            for (int j = 0; j <= (opcode & 0x0F00) >> 8; j++)
                            {
                                Registers[j] = Memory[I + j];
                            }
                            // I is set to I + x + 1 after operation
                            I += (ushort)(((opcode & 0x0F00) >> 8) + 1);
                            break;
                    }
                    break;

                default:
                    // Unrecognized opcode
                    Console.WriteLine($"Unrecognized opcode: {opcode:X4}");
                    break;
            }

            // Update timers
            TimerAccumulator += 60.0f / FrameTiming;
            if (TimerAccumulator >= 1.0f)
            {
                TimerAccumulator -= 1.0f;
                if (DelayTimer > 0)
                {
                    DelayTimer--;
                }
                if (SoundTimer > 0)
                {
                    SoundTimer--;
                }
            }

            if (DrawFlag)
            {
                DrawFlag = false;
                BuildDisplayBitmap();
            }
        }
    }
}
