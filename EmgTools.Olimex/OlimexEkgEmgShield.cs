using System;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Threading;

namespace EmgTools.IO.OlimexShield
{
    public class OlimexEkgEmgShield
    {
        private readonly byte[] _buffer = new byte[17];
        private readonly object _lock = new object();
        private readonly SerialPort _serialPort;
        private long epoch;

        public OlimexEkgEmgShield(string portName)
        {
            _serialPort = new SerialPort(portName, 57600, Parity.None, 8, StopBits.One);
        }

        public bool IsOpen
        {
            get { return _serialPort.IsOpen; }
        }

        public event EventHandler<ShieldDataReceivedEventArgs> DataReceived;
        public event EventHandler ShieldSynchronized;

        protected void OnDataReceived(ShieldDataReceivedEventArgs args)
        {
            if (DataReceived != null)
            {
                DataReceived(this, args);
            }
        }

        public void Open()
        {
            if (!IsOpen)
            {
                _serialPort.Open();
                Sync();
            }
        }

        public void Close()
        {
            _serialPort.Close();
        }

        private void Sync()
        {
            _serialPort.DataReceived -= sp_DataReceived;
            epoch = 0;
            _serialPort.DiscardInBuffer();

            while (true)
            {
                while (_serialPort.ReadByte() != 0xa5)
                {
                    Console.WriteLine("Syncing");
                }
                Console.WriteLine("Sync1");
                if (_serialPort.ReadByte() != 0x5a)
                    continue;
                Console.WriteLine("Sync2");
                var buf = new byte[15];
                if (_serialPort.Read(buf, 0, buf.Length) == 15)
                {
                    Console.WriteLine("Synchronized");
                    break;
                }
            }

            if (ShieldSynchronized != null)
            {
                ShieldSynchronized(this, EventArgs.Empty);
            }

            _serialPort.ReceivedBytesThreshold = 17;
            _serialPort.DataReceived += sp_DataReceived;
        }

        private void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            lock (_lock)
            {
                var sp = (SerialPort) sender;
                while (sp.BytesToRead > 16)
                {
                    sp.Read(_buffer, 0, _buffer.Length);
                    var message = ByteArrayToStructure<EkgEmgShieldEvent>(_buffer);

                    if (message.Sync0 == 0xa5 && message.Sync1 == 0x5a)
                    {
                        OnDataReceived(new ShieldDataReceivedEventArgs(Interlocked.Increment(ref epoch), message));
                    }
                    else
                    {
                        Console.WriteLine("Invalid Packet");
                        Sync();
                    }
                }
            }
        }

        private static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            var ret = (T) Marshal.PtrToStructure(handle.AddrOfPinnedObject(),
                typeof (T));
            handle.Free();
            return ret;
        }
    }
}