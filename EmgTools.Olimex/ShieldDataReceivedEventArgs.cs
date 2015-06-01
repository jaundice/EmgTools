using System;

namespace EmgTools.IO.OlimexShield
{
    public class ShieldDataReceivedEventArgs : EventArgs
    {
        public ShieldDataReceivedEventArgs(long epoch, EkgEmgShieldEvent message)
        {
            Epoch = epoch;
            Message = message;
        }

        public long Epoch { get; protected set; }

        public EkgEmgShieldEvent Message { get; protected set; }
    }
}