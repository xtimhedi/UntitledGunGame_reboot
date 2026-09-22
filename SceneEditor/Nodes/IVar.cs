using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace UGGR.SceneEditor.Nodes
{
    public class IVar
    {
        public Variant value { get; set; }
        public string Name { get; set; }
        public int id { get; set; }
        public Variant.Type type { get; set; }
    }
}
