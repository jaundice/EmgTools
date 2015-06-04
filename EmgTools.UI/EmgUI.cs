using System;
using System.IO.Ports;
using System.Linq;
using System.Windows.Forms;
using EmgTools.IO.OlimexShield;
using ILNumerics;
using ILNumerics.Drawing;
using ILNumerics.Drawing.Plotting;

namespace EmgTools.UI
{
    public partial class EmgUI : Form
    {
        private const int GraphWindow = 1500;
        private OlimexEkgEmgShield _shield;

        public EmgUI()
        {
            InitializeComponent();
            Setup();
            bRefreshSerial.Click += (sender, args) => RefreshSerialPorts();
            bBrowse.Click += BBrowseOnClick;
            bCollect.Click += BCollectOnClick;
            Closing += (sender, args) =>
            {
                if (_shield != null)
                {
                    _shield.DataReceived -= ShieldOnDataReceived;
                    _shield.ShieldSynchronized -= ShieldOnShieldSynchronized;
                    _shield.Close();
                }
                ilPanel1.Dispose();
                args.Cancel = false;
            };
        }

        private void BCollectOnClick(object sender, EventArgs eventArgs)
        {
            if (_shield != null)
            {
                _shield.ShieldSynchronized -= ShieldOnShieldSynchronized;
                _shield.DataReceived -= ShieldOnDataReceived;
                _shield.Close();
                _shield = null;
            }

            if (comboBox1.SelectedItem != null && !string.IsNullOrEmpty(tbLogFile.Text))
            {
                _shield = new OlimexEkgEmgShield((string)comboBox1.SelectedItem);
                _shield.DataReceived += ShieldOnDataReceived;
                _shield.ShieldSynchronized += ShieldOnShieldSynchronized;
                _shield.Open();

                using (ILScope.Enter())
                {
                    ilPanel1.Scene.Children.Clear();

                    ilPanel1.Scene.Add(new ILPlotCube(twoDMode: true)
                    {
                        new ILLinePlot(ILMath.zeros<float>(3, 0)),
                        new ILLinePlot(ILMath.zeros<float>(3, 0)),
                        new ILLinePlot(ILMath.zeros<float>(3, 0)),
                        new ILLinePlot(ILMath.zeros<float>(3, 0)),
                        new ILLinePlot(ILMath.zeros<float>(3, 0)),
                        new ILLinePlot(ILMath.zeros<float>(3, 0))
                    });
                }
            }
        }

        private void ShieldOnDataReceived(object sender, ShieldDataReceivedEventArgs shieldDataReceivedEventArgs)
        {
            LogEvent(shieldDataReceivedEventArgs);

            UpdateGraph(shieldDataReceivedEventArgs);
        }

        private void UpdateGraph(ShieldDataReceivedEventArgs shieldDataReceivedEventArgs)
        {
            using (ILScope.Enter())
            {

                var linePlots = ilPanel1.Scene.OfType<ILLinePlot>().ToArray();
                for (var i = 0; i < 6; i++)
                {
                    var linePlot = linePlots[i];
                    ILArray<float> starPositions = ILMath.zeros<float>(3, 1);
                    starPositions["0;:"] = shieldDataReceivedEventArgs.Epoch;
                    starPositions["1;:"] = shieldDataReceivedEventArgs.Message[i];
                    //starPositions["2;:"] = 0;

                    var posBuffer = linePlot.Line.Positions;


                    posBuffer.Update(posBuffer.Storage.Length, 1, starPositions);
                    posBuffer = linePlot.Line.Positions;

                    //if (posBuffer.Storage.Length > GraphWindow)
                    //{
                    //    var start = posBuffer.Storage.Length - GraphWindow;
                    //    linePlot.Line.Indices.Update(linePlot.Line.Indices.Storage.C[string.Format("0;{0}:end",start)]);
                    //    linePlot.Line.Positions.Update(linePlot.Line.Positions.Storage.C[string.Format("0;{0}:end",start)]);
                    //}

                }


                Invoke(new Action(() =>
                {
                    ilPanel1.Scene.First<ILPlotCube>().Reset();
                    ilPanel1.Scene.Configure();
                    ilPanel1.Refresh();
                }));
            }
        }

        private void LogEvent(ShieldDataReceivedEventArgs shieldDataReceivedEventArgs)
        {
            var s = string.Format("Epoch: {0}\tS1:{1}\tS2:{2}\tS3:{3}\tS4:{4}\tS5:{5}\tS6:{6}",
                shieldDataReceivedEventArgs.Epoch, shieldDataReceivedEventArgs.Message[0],
                shieldDataReceivedEventArgs.Message[1], shieldDataReceivedEventArgs.Message[2],
                shieldDataReceivedEventArgs.Message[3], shieldDataReceivedEventArgs.Message[4],
                shieldDataReceivedEventArgs.Message[5]);

            UpdateMessage(s);
        }

        private void UpdateMessage(string m)
        {
            Invoke(new Action(() => textBox2.Text = m));
        }

        private void ShieldOnShieldSynchronized(object sender, EventArgs eventArgs)
        {
            UpdateMessage("Synchronized");
            if (ilPanel1.Scene.First<ILLinePlot>() != null)
            {
                foreach (var linePlot in ilPanel1.Scene.OfType<ILLinePlot>())
                    linePlot.Line.Buffers = new ILBufferSet();
            }
        }

        private void RefreshSerialPorts()
        {
            comboBox1.Items.Clear();
            foreach (var item in SerialPort.GetPortNames())
            {
                comboBox1.Items.Add(item);
            }
        }

        private void Setup()
        {
            RefreshSerialPorts();
        }

        private void BBrowseOnClick(object sender, EventArgs eventArgs)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                tbLogFile.Text = saveFileDialog1.FileName;
            }
        }
    }
}