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
        Wireless,
        USB,
        BT,
        Chell,
    }
    public interface IController
    {
        EConnectionType ConnectionType { get; }

        void DeInitalize();
        ControllerState GetState();
        void Initalize();
        void Identify();
        string GetDevicePath();
    }

    public interface IControllerFactory
    {
        IController[] GetControllers();
    }
}
