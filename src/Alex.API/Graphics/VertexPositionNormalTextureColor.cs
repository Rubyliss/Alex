using System.Drawing;
using System.Numerics;
using Alex.Engine.Vertices;
using Veldrid;

namespace Alex.API.Graphics
{
    public struct VertexPositionNormalTextureColor : IVertexType
    {
        /// <summary>
        ///     Stores the position of this vertex
        /// </summary>
        public Vector3 Position;

        /// <summary>
        ///     The normal for this vertex
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        ///     The UV co-ords for this vertex (the co-ords in the texture)
        /// </summary>
        public Vector2 TexCoords;

        /// <summary>
        ///     The color of this vertex
        /// </summary>
        public Color Color;

        /// <summary>
        ///     Creates a new VertexPositionNormalTextureColor
        /// </summary>
        /// <param name="position">The position in space for this vertex</param>
        /// <param name="normal">The nomal for this vector</param>
        /// <param name="texCoords">The UV co-ords for this vertex</param>
        public VertexPositionNormalTextureColor(Vector3 position, Vector3 normal, Vector2 texCoords)
        {
            Position = position;
            Normal = normal;
            TexCoords = texCoords;
            Color = Color.White;
        }

        /// <summary>
        ///     Creates a new VertexPositionNormalTextureColor
        /// </summary>
        /// <param name="position">The position in space for this vertex</param>
        /// <param name="normal">The nomal for this vector</param>
        /// <param name="texCoords">The UV co-ords for this vertex</param>
        /// <param name="color">The color of this vertex</param>
        public VertexPositionNormalTextureColor(Vector3 position, Vector3 normal, Vector2 texCoords, Color color)
        {
            Position = position;
            Normal = normal;
            TexCoords = texCoords;
            Color = color;
        }

        /// <summary>
        ///     The vertex declaration for this vertex type
        /// </summary>
        public static VertexDeclaration VertexDeclaration { get; } = new VertexDeclaration
        (
	        new VertexElement(0, VertexElementFormat.Float3,
		        VertexElementSemantic.Position, 0),
	        new VertexElement(3 * sizeof(float), VertexElementFormat.Float3,
		        VertexElementSemantic.Normal, 0),
	        new VertexElement(6 * sizeof(float), VertexElementFormat.Float2,
		        VertexElementSemantic.TextureCoordinate, 0),
	        new VertexElement(8 * sizeof(float), VertexElementFormat.Float3,
		        VertexElementSemantic.Color, 0)
        );

	    VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
    }
}
