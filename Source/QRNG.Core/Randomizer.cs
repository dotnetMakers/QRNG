using Meadow;
using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace dotnetMakers;

public class Randomizer
{
    private readonly IAnalogInputPort? _port;
    private readonly IAnalogInputArray? _analogInputArray;

    public Voltage CenterVoltage { get; set; }

    public Randomizer(IAnalogInputPort port)
        : this(port, Voltage.Zero)
    {
    }

    public Randomizer(IAnalogInputArray array)
        : this(array, Voltage.Zero)
    {
    }

    public Randomizer(IAnalogInputPort port, Voltage centerVoltage)
    {
        _port = port;
        CenterVoltage = centerVoltage;
    }

    public Randomizer(IAnalogInputArray array, Voltage centerVoltage)
    {
        _analogInputArray = array;
        CenterVoltage = centerVoltage;
    }

    internal double ReadAdcVolts()
    {
        if (_port != null)
        {
            return _port.Read().GetAwaiter().GetResult().Volts;
        }
        if (_analogInputArray != null)
        {
            _analogInputArray.Refresh();
            return _analogInputArray.CurrentValues[0];
        }

        throw new NotSupportedException();
    }

    public void AutoFindCenterVoltage()
    {
        // just read a bunch of ADC values and get the mean
        var sampleCount = 64;
        var list = new List<double>(sampleCount);

        for (var i = 0; i < sampleCount; i++)
        {
            list.Add(ReadAdcVolts());
        }

        CenterVoltage = list.Average().Volts();
    }

    public byte GetRandomByte()
    {
        var bits = GetRandomBits(8);
        var bytes = new byte[1];
        bits.CopyTo(bytes, 0);
        return bytes[0];
    }

    public ushort GetRandomUInt16()
    {
        var bits = GetRandomBits(16);
        var bytes = new byte[2];
        bits.CopyTo(bytes, 0);
        return BitConverter.ToUInt16(bytes, 0);
    }

    public uint GetRandomUInt32()
    {
        var bits = GetRandomBits(32);
        var bytes = new byte[4];
        bits.CopyTo(bytes, 0);
        return BitConverter.ToUInt32(bytes, 0);
    }

    public ulong GetRandomUInt64()
    {
        var bits = GetRandomBits(64);
        var bytes = new byte[8];
        bits.CopyTo(bytes, 0);
        return BitConverter.ToUInt64(bytes, 0);
    }

    public int GetRandomInt(int minValue, int maxValue)
    {
        if (minValue >= maxValue)
        {
            throw new ArgumentException("minValue must be less than maxValue");
        }

        uint range = (uint)(maxValue - minValue + 1);
        uint randomValue = GetRandomUInt32();
        return (int)(minValue + (randomValue % range));
    }

    public double GetRandomDouble()
    {
        return GetRandomUInt32() / (double)uint.MaxValue;
    }

    public void NextBytes(byte[] bufferToFill)
    {
        // Process in chunks to avoid memory pressure with large buffers
        const int chunkSize = 1024; // Process 1KB at a time

        for (int offset = 0; offset < bufferToFill.Length; offset += chunkSize)
        {
            // Calculate the size of this chunk (might be smaller for the last chunk)
            int currentChunkSize = Math.Min(chunkSize, bufferToFill.Length - offset);

            // Get random bits for the current chunk
            BitArray bits = GetRandomBits(currentChunkSize * 8);

            // Create a temporary buffer for the current chunk
            byte[] chunkBuffer = new byte[currentChunkSize];
            bits.CopyTo(chunkBuffer, 0);

            // Copy the chunk to the appropriate position in the target buffer
            Array.Copy(chunkBuffer, 0, bufferToFill, offset, currentChunkSize);

            Resolver.Log.Info($"To go: {bufferToFill.Length - offset}");
        }
    }

    public BitArray GetRandomBits(int bitCount)
    {
        var array = new BitArray(bitCount);
        for (int i = 0; i < array.Length; i++)
        {
            // Read two consecutive voltage samples
            var sample1 = ReadAdcVolts();
            var sample2 = ReadAdcVolts();

            // Apply Von Neumann debiasing (removes bias)
            // Only keep bit when consecutive samples differ
            if (sample1 > CenterVoltage.Volts && sample2 <= CenterVoltage.Volts)
            {
                // If first sample is above center and second is below, output 1
                array[i] = true;
            }
            else if (sample1 <= CenterVoltage.Volts && sample2 > CenterVoltage.Volts)
            {
                // If first sample is below center and second is above, output 0
                array[i] = false;
            }
            else
            {
                // If both samples are on same side, discard and try again
                i--;
            }
        }

        return WhitenBitArray(array);
    }

    /// <summary>
    /// Applies a whitening algorithm to distribute entropy more evenly across all bits
    /// </summary>
    private BitArray WhitenBitArray(BitArray input)
    {
        // Convert BitArray to array of bytes for easier manipulation
        byte[] bytes = new byte[(input.Length + 7) / 8];
        input.CopyTo(bytes, 0);

        // Apply whitening to each 32-bit segment
        for (int i = 0; i <= bytes.Length - 4; i += 4)
        {
            // Convert 4 bytes to uint32 for bitwise operations
            uint value = BitConverter.ToUInt32(bytes, i);

            // Avalanching function (similar to a simple hash function)
            // Each input bit affects multiple output bits
            value ^= (value << 13);
            value ^= (value >> 17);
            value ^= (value << 5);

            // Convert back to bytes
            BitConverter.GetBytes(value).CopyTo(bytes, i);
        }

        // Handle any remaining bytes (if input length isn't a multiple of 32 bits)
        if (bytes.Length % 4 != 0)
        {
            int remainder = bytes.Length % 4;
            int startIdx = bytes.Length - remainder;

            // Simple whitening for remaining bytes
            for (int i = startIdx; i < bytes.Length; i++)
            {
                bytes[i] = (byte)(bytes[i] ^ (bytes[i] << 4) ^ (bytes[i] >> 3));
            }
        }

        // Convert back to BitArray, trimming to original length
        BitArray result = new BitArray(bytes);
        if (result.Length > input.Length)
        {
            // Create new BitArray of correct length
            BitArray trimmed = new BitArray(input.Length);
            for (int i = 0; i < input.Length; i++)
            {
                trimmed[i] = result[i];
            }
            return trimmed;
        }

        return result;
    }
}
