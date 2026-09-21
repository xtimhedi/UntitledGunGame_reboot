using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UGGR.SceneEditor.Nodes;
using UGGR.SceneEditor.Nodes.Executory;

namespace UGGR.SceneEditor
{
    public static class NodeRegistry
    {
        public static List<Func<CustomGraphNode>> NodeFactories = new List<Func<CustomGraphNode>>
        {
            // Mathematics
            () => new AddNode(),
            () => new SubtractNode(),
            () => new MultiplyNode(),
            () => new DivideNode(),

            // Standard shit
            () => new Input(),
            () => new Display(),
            () => new Branch(),
            () => new ButtonNode(),
            () => new ExecTest(),
            () => new GreaterThan()
        };
    }
}
