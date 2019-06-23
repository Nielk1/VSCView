using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSCView
{
    public enum EConnectionType
    {
        Unknown,
        USB,
        Bluetooth,
        Dongle,
    }
    public delegate void ControllerNameUpdateEvent();
    public interface IController
    {
        event ControllerNameUpdateEvent ControllerNameUpdated;

        EConnectionType ConnectionType { get; }
        void DeInitalize();
        ControllerState GetState();
        void Initalize();
        void Identify();
        string GetName();
        Image GetIcon();
    }

    public interface IControllerFactory
    {
        IController[] GetControllers();
    }
}
