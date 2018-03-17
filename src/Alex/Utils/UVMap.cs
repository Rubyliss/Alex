﻿using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using Alex.ResourcePackLib.Json;

namespace Alex.Utils
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    // ReSharper disable once InconsistentNaming
    public struct UVMap
    {
        public Color ColorLeft;
        public Color ColorRight;

        public Color ColorFront;
        public Color ColorBack;

        public Color ColorTop;
        public Color ColorBottom;

        public readonly Vector2 TopLeft;
        public readonly Vector2 TopRight;

        public readonly Vector2 BottomLeft;
        public readonly Vector2 BottomRight;

        public UVMap(Vector2 topLeft, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomRight, Color colorSide,
            Color colorTop, Color colorBottom)
        {
            TopLeft = topLeft;
            TopRight = topRight;
            BottomLeft = bottomLeft;
            BottomRight = bottomRight;

	        ColorFront = colorSide;
	        ColorBack = colorSide;

	        ColorLeft = colorSide;
	        ColorRight = colorSide;

	        ColorTop = colorTop;
	        ColorBottom = colorBottom;
        }
    }

    internal class UvMapHelp
    {
	    public static readonly Color LightColor = Color.FromArgb(245, 245, 225);

        /// <summary>
        /// The default lighting information for rendering a block;
        ///  i.e. when the lighting param to CreateUniformCube == null.
        /// </summary>
        public static readonly int[] DefaultLighting =
            new int[]
            {
                15, 15, 15,
                15, 15, 15
            };

        /// <summary>
        /// Maps a light level [0..15] to a brightness modifier for lighting.
        /// </summary>
        public static readonly float[] CubeBrightness =
            new float[]
            {
                0.050f, 0.067f, 0.085f, 0.106f, // [ 0..3 ]
                0.129f, 0.156f, 0.186f, 0.221f, // [ 4..7 ]
                0.261f, 0.309f, 0.367f, 0.437f, // [ 8..11]
                0.525f, 0.638f, 0.789f, 1.000f //  [12..15]
            };

        /// <summary>
        /// The per-face brightness modifier for lighting.
        /// </summary>
        public static readonly float[] FaceBrightness =
            new float[]
            {
                0.6f, 0.6f, // North / South
                0.8f, 0.8f, // East / West
                1.0f, 0.5f // Top / Bottom
            };

	    public static Color AdjustColor(Color color, BlockFace face, int lighting, bool shade = true)
	    {
		    float brightness = 1f;
		    if (shade)
		    {
			    switch (face)
			    {
				    case BlockFace.Down:
					    brightness = FaceBrightness[5];
					    break;
				    case BlockFace.Up:
					    brightness = FaceBrightness[4];
					    break;
				    case BlockFace.East:
					    brightness = FaceBrightness[2];
					    break;
				    case BlockFace.West:
					    brightness = FaceBrightness[3];
					    break;
				    case BlockFace.North:
					    brightness = FaceBrightness[0];
					    break;
				    case BlockFace.South:
					    brightness = FaceBrightness[1];
					    break;
				    case BlockFace.None:

					    break;
			    }
		    }

		    var light = new Vector3(LightColor.R, LightColor.G, LightColor.B) * CubeBrightness[lighting];
		    var c = brightness * new Vector3(color.R, color.G, color.B) * light;
		    return Color.FromArgb((int) c.X, (int) c.Y, (int) c.Z);
	    }
    }
}
