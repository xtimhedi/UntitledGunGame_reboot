using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UGGR.SceneEditor
{
    public static class NodeMaterials
    {
        public static StyleBoxFlat Titlebar(Color color)
        {
            Color darkened = color.Darkened(0.08f);
            return new StyleBoxFlat
            {
                BgColor = darkened,
                BorderWidthLeft = 4,
                BorderWidthRight = 4,
                BorderWidthTop = 4,
                BorderWidthBottom = 4,
                BorderColor = color,
                CornerRadiusTopLeft = 6,
                CornerRadiusTopRight = 6,
                ShadowColor = new Color(color.R, color.G, color.B, color.A / 8f),
                ShadowSize = 1,
                ShadowOffset = new Vector2(1, -1)
            };
        }

        public static StyleBoxFlat TitlebarSelected(Color color)
        {
            Color darkened = color.Lightened(0.08f);
            return new StyleBoxFlat
            {
                BgColor = darkened,
                BorderWidthLeft = 4,
                BorderWidthRight = 4,
                BorderWidthTop = 4,
                BorderWidthBottom = 4,
                BorderColor = color,
                CornerRadiusTopLeft = 6,
                CornerRadiusTopRight = 6,
                ShadowColor = new Color(color.R, color.G, color.B, color.A / 8f),
                ShadowSize = 1,
                ShadowOffset = new Vector2(1, -1)
            };
        }
    }
}
