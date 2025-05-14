using Meadow;
using Meadow.Peripherals.Displays;
using System;

namespace dotnetMakers;

public sealed class DisplayService
{
    private readonly IPixelDisplay _display;

    private ColorMode TestMode { get; }

    public DisplayService(IPixelDisplay display)
    {
        _display = display;

        Resolver.Log.Info($"supported modes: {_display.SupportedColorModes}");

        TestMode = _display.ColorMode;

        _display.SetColorMode(TestMode);
    }

    private int ByteToGrayscale12Bit(byte byteValue)
    {
        // Scale the 8-bit value (0-255) to 12-bit (0-4095)
        return (int)byteValue * 16 + (byteValue >> 4);
    }

    public void SetNoisePattern(byte[] pixels)
    {
        Resolver.Log.Info($"mode: {_display.ColorMode}");
        Resolver.Log.Info($"buffer: {_display.PixelBuffer.Buffer.Length} bytes");

        var index = 0;
        var togo = _display.PixelBuffer.Buffer.Length;

        while (togo > 0)
        {
            var count = Math.Min(togo, pixels.Length);

            Resolver.Log.Info($"copy: {count} to offset {index}");

            Array.Copy(pixels, 0, _display.PixelBuffer.Buffer, index, count);
            index += pixels.Length;
            togo -= pixels.Length;
        }

        _display.Show();
    }

    public void SetNoisePattern2(byte[] pixels)
    {
        //        _rootLayout.IsVisible = false;
        //        Screen.IsVisible = false;

        Resolver.Log.Info($"setting noise...");

        int bufferSize = (320 * 240 * 12) / 8; // Convert bits to bytes
        byte[] buffer = new byte[bufferSize]; // This will be 115,200 bytes

        // To fill buffer with grayscale values (assuming you want to use all 12 bits)
        for (var p = 0; p < pixels.Length; p++)
        {
            // Calculate position in buffer
            int startBit = p * 12;
            int startByte = startBit / 8;
            int bitOffset = startBit % 8;

            // Get your 12-bit grayscale value (0-4095)
            int grayValue = ByteToGrayscale12Bit(pixels[p]);

            // Write the value to the buffer (handling the bit packing)
            if (bitOffset <= 4)
            {
                // Value fits mostly in first byte with some spillover
                buffer[startByte] = (byte)((buffer[startByte] & (0xFF >> (8 - bitOffset))) | ((grayValue & 0xFF) << bitOffset));

                if (bitOffset < 4)
                {
                    // Need to use part of next byte for high bits
                    buffer[startByte + 1] = (byte)((buffer[startByte + 1] & (0xFF << (4 - bitOffset))) | (grayValue >> (8 - bitOffset)));
                }
                else
                {
                    // Exactly 4 bits in next byte
                    buffer[startByte + 1] = (byte)((buffer[startByte + 1] & 0xF0) | (grayValue >> 8));
                }
            }
            else
            {
                // Value spans across 3 bytes
                buffer[startByte] = (byte)((buffer[startByte] & (0xFF >> (8 - bitOffset))) | ((grayValue & ((1 << (8 - bitOffset)) - 1)) << bitOffset));
                buffer[startByte + 1] = (byte)((grayValue >> (8 - bitOffset)) & 0xFF);
                buffer[startByte + 2] = (byte)((buffer[startByte + 2] & (0xFF << (16 - bitOffset))) | (grayValue >> (16 - bitOffset)));
            }

        }

        _display.Clear();

        Array.Copy(buffer, _display.PixelBuffer.Buffer, buffer.Length);

        Resolver.Log.Info($"done");

        _display.Show();
    }
}
