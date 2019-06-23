using System;
using System.Collections.Generic;
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
        Chell,
    }
    public interface IController
    {
        EConnectionType ConnectionType { get; }
        void DeInitalize();
        ControllerState GetState();
        void Initalize();
        void Identify();
        string GetName();
    }

    public interface IControllerFactory
    {
        IController[] GetControllers();
    }
}
