using HidLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VSCView
{
    public partial class RawForm : Form
    {
        private const int VendorId = 10462;
        private static int ProductIdWireless = 4418;
        private static int ProductIdWired = 1102;
        private static bool _attached;

        //private Int16 MaxVal = 0;

        public RawForm()
        {
            InitializeComponent();
            SetDoubleBuffered(txtUpdate);
        }

        public static void SetDoubleBuffered(System.Windows.Forms.Control c)
        {
            //Taxes: Remote Desktop Connection and painting
            //http://blogs.msdn.com/oldnewthing/archive/2006/01/03/508694.aspx
            if (System.Windows.Forms.SystemInformation.TerminalServerSession)
                return;

            System.Reflection.PropertyInfo aProp =
                  typeof(System.Windows.Forms.Control).GetProperty(
                        "DoubleBuffered",
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Instance);

            aProp.SetValue(c, true, null);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            GetHidDevice();
        }

        private void GetHidDevice()
        {
            List<HidDevice> _devices = HidDevices.Enumerate(VendorId, ProductIdWireless, ProductIdWired).ToList();

            foreach(HidDevice _device in _devices)
            {
                if (_device != null)
                {
                    // found

                    // \\?\hid#vid_28de&pid_1142&mi_00&col01#8&d70e632&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}	HID Keyboard Device
                    // \\?\hid#vid_28de&pid_1142&mi_00&col02#8&d70e632&0&0001#{4d1e55b2-f16f-11cf-88cb-001111000030}	HID-compliant mouse
                    // \\?\hid#vid_28de&pid_1142&mi_01#8&314823f4&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}	HID-compliant device
                    // \\?\hid#vid_28de&pid_1142&mi_02#8&198497af&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}	HID-compliant device
                    // \\?\hid#vid_28de&pid_1142&mi_03#8&1c10b6a&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}	HID-compliant device
                    // \\?\hid#vid_28de&pid_1142&mi_04#8&279758bf&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}	HID-compliant device

                    int index = -1;
                    Match m = Regex.Match(_device.DevicePath, "&mi_([0-9]{2})");
                    if (!m.Success) continue;
                    index = int.Parse(m.Groups[1].Value) - 1;
                    if (index < 0) continue;

                    _device.OpenDevice();
                    //_device.OpenDevice(DeviceMode.NonOverlapped, DeviceMode.NonOverlapped, ShareMode.ShareRead);

                    Console.WriteLine(_device.DevicePath + "\t" + _device.Description);

                    byte[] featureData;
                    _device.ReadFeatureData(out featureData);
                    byte[] manufacturer;
                    _device.ReadManufacturer(out manufacturer);
                    byte[] productData;
                    _device.ReadProduct(out productData);
                    byte[] serialNumber;
                    _device.ReadSerialNumber(out serialNumber);

                    _device.Inserted += () => DeviceAttachedHandler(_device);
                    _device.Removed += () => DeviceRemovedHandler(_device);

                    _device.MonitorDeviceEvents = true;

                    _device.ReadReport((_data) => OnReport(_device, _data));
                    //_device.Read((_data) => OnRead(_device, _data));
                }
                else
                {
                    // not found
                }
            }
        }

        /*private static void OnRead(HidDevice _device, HidDeviceData data)
        {
            if (_attached == false) { return; }

            //if (data.Data.Length >= 4)
            {
                var message = MessageFactory.CreateMessage(ProductId, data.Data);
            }

            _device.Read((_data) => OnRead(_device, _data));
        }*/

        private void OnReport(HidDevice _device, HidReport report)
        {
            if (_attached == false) { return; }

            //if (report.Data.Length == 64)
            {
                txtDevicePath.Invoke((MethodInvoker)delegate
                {
                    // Running on the UI thread
                    txtDevicePath.Text = _device.DevicePath;
                });

                txtDescription.Invoke((MethodInvoker)delegate
                {
                    // Running on the UI thread
                    txtDescription.Text = _device.Description;
                });

                byte EventType = report.Data[2];

                string[] NiceOutputText = BitConverter.ToString(report.Data).Split('-');

                switch (EventType)
                {
                    case 0x00:
                        {

                        }
                        //_device.ReadReport((data) => OnReport(_device, data));
                        break;

                    case 0x01:
                        {
                            NiceOutputText[0] += " -------- Always 01?";
                            NiceOutputText[1] += " -------- Always 00?";
                            NiceOutputText[2] += " -------- Event Type";
                            NiceOutputText[3] += " -------- 3C?";

                            NiceOutputText[4] += " -------- Packet Index: " + BitConverter.ToUInt32(report.Data, 4);
                            NiceOutputText[5] += " ^^^^^^^^";
                            NiceOutputText[6] += " ^^^^^^^^";
                            NiceOutputText[7] += " ^^^^^^^^";

                            NiceOutputText[8] += " " + Convert.ToString(report.Data[8], 2).PadLeft(8, '0');
                            NiceOutputText[8] += (report.Data[8] & 128) == 128 ? " ↓A↓" : " ↑A↑";
                            NiceOutputText[8] += (report.Data[8] & 64) == 64 ? " ↓X↓" : " ↑X↑";
                            NiceOutputText[8] += (report.Data[8] & 32) == 32 ? " ↓B↓" : " ↑B↑";
                            NiceOutputText[8] += (report.Data[8] & 16) == 16 ? " ↓Y↓" : " ↑Y↑";
                            NiceOutputText[8] += (report.Data[8] & 8) == 8 ? " ↓LB↓" : " ↑LB↑";
                            NiceOutputText[8] += (report.Data[8] & 4) == 4 ? " ↓RB↓" : " ↑RB↑";
                            NiceOutputText[8] += (report.Data[8] & 2) == 2 ? " ↓LT↓" : " ↑LT↑";
                            NiceOutputText[8] += (report.Data[8] & 1) == 1 ? " ↓RT↓" : " ↑RT↑";

                            NiceOutputText[9] += " " + Convert.ToString(report.Data[9], 2).PadLeft(8, '0');
                            NiceOutputText[9] += (report.Data[9] & 128) == 128 ? " ↓L-Grip↓" : " ↑L-Grip↑";
                            NiceOutputText[9] += (report.Data[9] & 64) == 64 ? " ↓SteamR↓" : " ↑SteamR↑";
                            NiceOutputText[9] += (report.Data[9] & 32) == 32 ? " ↓Steam↓" : " ↑Steam↑";
                            NiceOutputText[9] += (report.Data[9] & 16) == 16 ? " ↓SteamL↓" : " ↑SteamL↑";
                            NiceOutputText[9] += (report.Data[9] & 8) == 8 ? " ↓Down↓" : " ↑Down↑";
                            NiceOutputText[9] += (report.Data[9] & 4) == 4 ? " ↓Left↓" : " ↑Left↑";
                            NiceOutputText[9] += (report.Data[9] & 2) == 2 ? " ↓Right↓" : " ↑Right↑";
                            NiceOutputText[9] += (report.Data[9] & 1) == 1 ? " ↓Up↓" : " ↑Up↑";

                            bool LeftAnalogMultiplexMode = (report.Data[10] & 128) == 128;
                            bool LeftPadTouch = (report.Data[10] & 8) == 8;

                            NiceOutputText[10] += " " + Convert.ToString(report.Data[10], 2).PadLeft(8, '0');
                            NiceOutputText[10] += LeftAnalogMultiplexMode ? " !MULTIPLEX!" : "            ";
                            NiceOutputText[10] += (report.Data[10] & 64) == 64 ? " ↓ThumbStick↓" : " ↑ThumbStick↑";
                            NiceOutputText[10] += (report.Data[10] & 32) == 32 ? " ↓-↓" : " ↑-↑";
                            NiceOutputText[10] += (report.Data[10] & 16) == 16 ? " ↓R-Pad Touch↓" : " ↑R-Pad Touch↑";
                            NiceOutputText[10] += LeftPadTouch ? " ↓L-Pad Touch↓" : " ↑L-Pad Touch↑";
                            NiceOutputText[10] += (report.Data[10] & 4) == 4 ? " ↓R-Pad Press↓" : " ↑R-Pad Press↑";
                            NiceOutputText[10] += (report.Data[10] & 2) == 2 ? " ↓?ThumbStick OR L-Pad Press?↓" : " ↑?ThumbStick OR L-Pad Press?↑";
                            NiceOutputText[10] += (report.Data[10] & 1) == 1 ? " ↓R-Grip↓" : " ↑R-Grip↑";

                            NiceOutputText[11] += " -------- LT: " + report.Data[11].ToString().PadLeft(3);
                            NiceOutputText[12] += " -------- RT: " + report.Data[12].ToString().PadLeft(3);

                            string LeftPadX = "      ";
                            string LeftPadY = "      ";
                            string LeftStickX = string.Empty;
                            string LeftStickY = string.Empty;

                            if (LeftAnalogMultiplexMode)
                            {
                                if (LeftPadTouch)
                                {
                                    LeftPadX = BitConverter.ToInt16(report.Data, 16).ToString().PadLeft(6);
                                    LeftPadY = BitConverter.ToInt16(report.Data, 18).ToString().PadLeft(6);
                                }
                                else
                                {
                                    LeftStickX = BitConverter.ToInt16(report.Data, 16).ToString().PadLeft(6);
                                    LeftStickY = BitConverter.ToInt16(report.Data, 18).ToString().PadLeft(6);
                                }
                            }
                            else if (LeftPadTouch)
                            {
                                LeftPadX = BitConverter.ToInt16(report.Data, 16).ToString().PadLeft(6);
                                LeftPadY = BitConverter.ToInt16(report.Data, 18).ToString().PadLeft(6);
                            }
                            else
                            {
                                LeftStickX = BitConverter.ToInt16(report.Data, 16).ToString().PadLeft(6);
                                LeftStickY = BitConverter.ToInt16(report.Data, 18).ToString().PadLeft(6);
                            }

                            NiceOutputText[16] += " -------- Left Pad X: " + LeftPadX + " ThumbStick X: " + LeftStickX;
                            NiceOutputText[17] += " ^^^^^^^^";
                            NiceOutputText[18] += " -------- Left Pad Y: " + LeftPadY + " ThumbStick Y: " + LeftStickY;
                            NiceOutputText[19] += " ^^^^^^^^";

                            NiceOutputText[20] += " -------- Right Pad X: " + BitConverter.ToInt16(report.Data, 20);
                            NiceOutputText[21] += " ^^^^^^^^";
                            NiceOutputText[22] += " -------- Right Pad Y: " + BitConverter.ToInt16(report.Data, 22);
                            NiceOutputText[23] += " ^^^^^^^^";

                            //NiceOutputText[28] += " -------- Acceleration X: " + BitConverter.ToInt16(report.Data, 28);
                            //NiceOutputText[29] += " ^^^^^^^^";
                            //NiceOutputText[30] += " -------- Acceleration Y: " + BitConverter.ToInt16(report.Data, 30);
                            //NiceOutputText[31] += " ^^^^^^^^";
                            //NiceOutputText[32] += " -------- Acceleration Z: " + BitConverter.ToInt16(report.Data, 32);
                            //NiceOutputText[33] += " ^^^^^^^^";
                            NiceOutputText[34] += " -------- AngularVelocity X: " + BitConverter.ToInt16(report.Data, 34);
                            NiceOutputText[35] += " ^^^^^^^^";
                            NiceOutputText[36] += " -------- AngularVelocity Y: " + BitConverter.ToInt16(report.Data, 36);
                            NiceOutputText[37] += " ^^^^^^^^";
                            NiceOutputText[38] += " -------- AngularVelocity Z: " + BitConverter.ToInt16(report.Data, 38);
                            NiceOutputText[39] += " ^^^^^^^^";

                            float qw = (BitConverter.ToInt16(report.Data, 40) * 1.0f / 32736);
                            float qx = (BitConverter.ToInt16(report.Data, 42) * 1.0f / 32736);
                            float qy = (BitConverter.ToInt16(report.Data, 44) * 1.0f / 32736);
                            float qz = (BitConverter.ToInt16(report.Data, 46) * 1.0f / 32736);

                            double roll;
                            double pitch;
                            double yaw;

                            // roll (x-axis rotation)
                            double sinr = +2.0 * (qw * qx + qy * qz);
                            double cosr = +1.0 - 2.0 * (qx * qx + qy * qy);
                            roll = Math.Atan2(sinr, cosr);

                            // pitch (y-axis rotation)
                            double sinp = +2.0 * (qw * qy - qz * qx);
                            if (Math.Abs(sinp) >= 1)
                                pitch = Math.Abs(Math.PI / 2) * Math.Sign(sinp); // use 90 degrees if out of range
                            else
                                pitch = Math.Asin(sinp);

                            // yaw (z-axis rotation)
                            double siny = +2.0 * (qw * qz + qx * qy);
                            double cosy = +1.0 - 2.0 * (qy * qy + qz * qz);
                            yaw = Math.Atan2(siny, cosy);

                            Console.WriteLine($"{(roll / 2 / Math.PI).ToString("0.###")},{(pitch / 2 / Math.PI).ToString("0.###")},{(yaw / 2 / Math.PI).ToString("0.###")}");

                            NiceOutputText[40] += " -------- Orientation W: " + BitConverter.ToInt16(report.Data, 40) + " " + (BitConverter.ToInt16(report.Data, 40) * 1.0f / 32736).ToString("0.###");
                            NiceOutputText[41] += " ^^^^^^^^";
                            NiceOutputText[42] += " -------- Orientation X: " + BitConverter.ToInt16(report.Data, 42) + " " + (BitConverter.ToInt16(report.Data, 42) * 1.0f / 32736).ToString("0.###");
                            NiceOutputText[43] += " ^^^^^^^^";
                            NiceOutputText[44] += " -------- Orientation Y: " + BitConverter.ToInt16(report.Data, 44) + " " + (BitConverter.ToInt16(report.Data, 44) * 1.0f / 32736).ToString("0.###");
                            NiceOutputText[45] += " ^^^^^^^^";
                            NiceOutputText[46] += " -------- Orientation Z: " + BitConverter.ToInt16(report.Data, 46) + " " + (BitConverter.ToInt16(report.Data, 46) * 1.0f / 32736).ToString("0.###");
                            NiceOutputText[47] += " ^^^^^^^^";

                            /*MaxVal = Math.Max(
                                Math.Max(
                                    Math.Abs(BitConverter.ToInt16(report.Data, 40)),
                                    Math.Max(
                                        Math.Abs(BitConverter.ToInt16(report.Data, 42)),
                                        Math.Max(
                                            Math.Abs(BitConverter.ToInt16(report.Data, 44)),
                                            Math.Abs(BitConverter.ToInt16(report.Data, 46))
                                        )
                                    )
                                ), MaxVal);

                            Console.WriteLine(MaxVal);*/

                            txtUpdate.Invoke((MethodInvoker)delegate
                            {
                                // Running on the UI thread
                                txtUpdate.Text = string.Join("\r\n", NiceOutputText);
                            });
                        }
                        //_device.ReadReport((data) => OnReport(_device, data));
                        _device.ReadReport((data) => OnReport(_device, data));
                        break;

                    case 0x03:
                        {
                            NiceOutputText[0] += " -------- Always 01?";
                            NiceOutputText[1] += " -------- Always 00?";
                            NiceOutputText[2] += " -------- Event Type";
                            NiceOutputText[3] += " -------- 01?";

                            // Connection detail. 0x01 for disconnect, 0x02 for connect, 0x03 for pairing request.
                            NiceOutputText[4] += " -------- Connection Detail: " + report.Data[4];

                            if (report.Data[4] == 0x01)
                            {
                                byte[] tmpBytes = new byte[4];
                                tmpBytes[1] = report.Data[5];
                                tmpBytes[2] = report.Data[6];
                                tmpBytes[3] = report.Data[7];

                                NiceOutputText[5] += " -------- Packet Index: " + BitConverter.ToUInt32(tmpBytes, 0);
                                NiceOutputText[6] += " ^^^^^^^^";
                                NiceOutputText[7] += " ^^^^^^^^";
                            }

                            // only works if controller is configured to send this data

                            // millivolts
                            NiceOutputText[8] += " -------- Battery Voltage: " + BitConverter.ToUInt16(report.Data, 8);
                            NiceOutputText[9] += " ^^^^^^^^";

                            NiceOutputText[10] += " -------- Unknown, stuck at 100%: " + BitConverter.ToUInt16(report.Data, 10);
                            NiceOutputText[11] += " ^^^^^^^^";

                            txtConnection.Invoke((MethodInvoker)delegate
                            {
                                // Running on the UI thread
                                txtConnection.Text = string.Join("\r\n", NiceOutputText);
                            });
                        }
                        _device.ReadReport((data) => OnReport(_device, data));
                        break;

                    case 0x04:
                        {
                            NiceOutputText[0] += " -------- Always 01?";
                            NiceOutputText[1] += " -------- Always 00?";
                            NiceOutputText[2] += " -------- Event Type";
                            NiceOutputText[3] += " -------- 0B?";

                            NiceOutputText[4] += " -------- Packet Index: " + BitConverter.ToUInt32(report.Data, 4);
                            NiceOutputText[5] += " ^^^^^^^^";
                            NiceOutputText[6] += " ^^^^^^^^";
                            NiceOutputText[7] += " ^^^^^^^^";

                            // only works if controller is configured to send this data

                            // millivolts
                            NiceOutputText[8] += " -------- Battery Voltage: " + BitConverter.ToUInt16(report.Data, 8);
                            NiceOutputText[9] += " ^^^^^^^^";

                            NiceOutputText[10] += " -------- Unknown, stuck at 100%: " + BitConverter.ToUInt16(report.Data, 10);
                            NiceOutputText[11] += " ^^^^^^^^";

                            txtBattery.Invoke((MethodInvoker)delegate
                            {
                                // Running on the UI thread
                                txtBattery.Text = string.Join("\r\n", NiceOutputText);
                            });
                        }
                        _device.ReadReport((data) => OnReport(_device, data));
                        break;

                    default:
                        {
                            Console.WriteLine("Unknown Packet Type " + EventType);
                        }
                        _device.ReadReport((data) => OnReport(_device, data));
                        break;
                }

                //var message = MessageFactory.CreateMessage(ProductId, report.Data);

                //_device.ReadReport((data) => OnReport(_device, data));
            }
        }

        private void DeviceAttachedHandler(HidDevice _device)
        {
            _attached = true;
            Console.WriteLine("Gamepad attached.");
            _device.ReadReport((_data) => OnReport(_device, _data));
            //_device.Read((_data) => OnRead(_device, _data));
        }

        private void DeviceRemovedHandler(HidDevice _device)
        {
            _attached = false;
            Console.WriteLine("Gamepad removed.");
        }
    }
}
