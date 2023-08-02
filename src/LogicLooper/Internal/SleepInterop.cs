using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32;

namespace Cysharp.Threading.Internal;

internal static class SleepInterop
{
    private static readonly bool _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Sleep(int millisecondsTimeout)
    {
        if (_isWindows)
        {
            Win32WaitableTimerSleep.Sleep(millisecondsTimeout);
        }
        else
        {
            Thread.Sleep(millisecondsTimeout);
        }
    }

    private static class Win32WaitableTimerSleep
    {
        private const uint CREATE_WAITABLE_TIMER_MANUAL_RESET = 0x00000001;
        private const uint CREATE_WAITABLE_TIMER_HIGH_RESOLUTION = 0x00000002;

        [ThreadStatic]
        private static SafeHandle? _timerHandle;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Sleep(int milliseconds)
        {
            _timerHandle ??= PInvoke.CreateWaitableTimerEx(null, default(string?), CREATE_WAITABLE_TIMER_HIGH_RESOLUTION, 0x1F0003 /* TIMER_ALL_ACCESS */);
            var result = PInvoke.SetWaitableTimer(_timerHandle, milliseconds * -10000, 0, null, null, false);
            var resultWait = PInvoke.WaitForSingleObject(_timerHandle, 0xffffffff /* Infinite */);
        }
    }
}
