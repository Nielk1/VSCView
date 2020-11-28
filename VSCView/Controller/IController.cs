using ExtendInput.Providers;
using System.Drawing;

namespace VSCView.Controller
{
    /*public enum EConnectionType
    {
        Unknown,
        USB,
        Bluetooth,
        Dongle,
    }*/
    public delegate void ControllerNameUpdateEvent();
    public interface IController
    {
        event ControllerNameUpdateEvent ControllerNameUpdated;

        //EConnectionType ConnectionType { get; }
        IDevice DeviceHackRef { get; }
        bool HasMotion { get; }

        void DeInitalize();
        ExtendInput.Controls.ControllerState GetState();
        void Initalize();
        void Identify();
        string GetName();
        Image GetIcon();
    }
}
