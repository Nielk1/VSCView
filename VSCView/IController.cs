using ExtendInput.Providers;
using System.Drawing;

namespace VSCView
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
        ExtendInput.ControllerState GetState();
        void Initalize();
        void Identify();
        string GetName();
        Image GetIcon();
    }
}
