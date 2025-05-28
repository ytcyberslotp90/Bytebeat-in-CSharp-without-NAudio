using System;
using System.Runtime.InteropServices;
using System.Threading;

class BytebeatPlayer
{
    // WinMM API for playing sound
    [DllImport("winmm.dll")]
    private static extern int waveOutWrite(IntPtr hWaveOut, IntPtr lpWaveHdr, int uSize);
    [DllImport("winmm.dll")]
    private static extern int waveOutOpen(out IntPtr hWaveOut, int uDeviceID, ref WAVEFORMATEX lpFormat, IntPtr dwCallback, IntPtr dwInstance, int dwFlags);
    [DllImport("winmm.dll")]
    private static extern int waveOutPrepareHeader(IntPtr hWaveOut, IntPtr lpWaveHdr, int uSize);
    [DllImport("winmm.dll")]
    private static extern int waveOutUnprepareHeader(IntPtr hWaveOut, IntPtr lpWaveHdr, int uSize);
    [DllImport("winmm.dll")]
    private static extern int waveOutClose(IntPtr hWaveOut);

    [StructLayout(LayoutKind.Sequential)]
    struct WAVEFORMATEX
    {
        public ushort wFormatTag;
        public ushort nChannels;
        public uint nSamplesPerSec;
        public uint nAvgBytesPerSec;
        public ushort nBlockAlign;
        public ushort wBitsPerSample;
        public ushort cbSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct WAVEHDR
    {
        public IntPtr lpData;
        public uint dwBufferLength;
        public uint dwBytesRecorded;
        public uint dwUser;
        public uint dwFlags;
        public uint dwLoops;
        public IntPtr lpNext;
        public uint reserved;
    }

    static void Main()
    {
        const int SAMPLE_RATE = 8000;
        const int DURATION_SECONDS = 10;
        const int BUFFER_SIZE = SAMPLE_RATE * DURATION_SECONDS;

        byte[] buffer = new byte[BUFFER_SIZE];

        // Your Bytebeat formula here
        for (int t = 0; t < BUFFER_SIZE; t++)
        {
            // Example: simple bytebeat: ((t & (t >> 5 | t >> 8 | t >> 2) ^ t) * t >> 1)
            buffer[t] = (byte)((t & (t >> 5 | t >> 8 | t >> 2) ^ t) * t >> 1);

        }

        PlayRawSound(buffer, SAMPLE_RATE);
    }

    static void PlayRawSound(byte[] data, int sampleRate)
    {
        IntPtr hWaveOut;
        WAVEFORMATEX format = new WAVEFORMATEX
        {
            wFormatTag = 1,
            nChannels = 1,
            nSamplesPerSec = (uint)sampleRate,
            wBitsPerSample = 8,
            nBlockAlign = 1,
            nAvgBytesPerSec = (uint)sampleRate,
            cbSize = 0
        };

        waveOutOpen(out hWaveOut, -1, ref format, IntPtr.Zero, IntPtr.Zero, 0);

        GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
        WAVEHDR header = new WAVEHDR
        {
            lpData = handle.AddrOfPinnedObject(),
            dwBufferLength = (uint)data.Length,
            dwFlags = 0
        };

        IntPtr headerPtr = Marshal.AllocHGlobal(Marshal.SizeOf(header));
        Marshal.StructureToPtr(header, headerPtr, false);

        waveOutPrepareHeader(hWaveOut, headerPtr, Marshal.SizeOf(header));
        waveOutWrite(hWaveOut, headerPtr, Marshal.SizeOf(header));

        // wait for sound to finish
        Thread.Sleep((int)(data.Length * 1000.0 / sampleRate));

        waveOutUnprepareHeader(hWaveOut, headerPtr, Marshal.SizeOf(header));
        waveOutClose(hWaveOut);

        Marshal.FreeHGlobal(headerPtr);
        handle.Free();
    }
}
