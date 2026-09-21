using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UGGR.SceneEditor
{
    public enum NodeTypeEnum
    {
        Event,
        Impure,
        Pure,
        FlowControl,
        Function
    }
    public enum PinTypeEnum
    {
        Execution,
        Boolean,
        Integer,
        Vector3,
        Vector2,
        String,
        Reference,
        Transform
    }
}
