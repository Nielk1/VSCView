using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSCView
{
    public interface IController
    {
        void DeInitalize();
        ControllerState GetState();
        void Initalize();
        void Identify();
    }
}
