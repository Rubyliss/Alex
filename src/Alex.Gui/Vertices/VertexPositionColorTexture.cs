using System.Drawing;
using System.Numerics;
using Veldrid;

namespace Alex.Engine.Vertices
{
	public struct VertexPositionColorTexture : IVertexType
	{
		/// <summary>
		///     Stores the position of this vertex
		/// </summary>
		public Vector3 Position;

		/// <summary>
		///     The UV co-ords for this vertex (the co-ords in the texture)
		/// </summary>
		public Vector2 TextureCoordinate;

		/// <summary>
		///     The color of this vertex
		/// </summary>
		public Color Color;

		/// <summary>
		///     Creates a new VertexPositionNormalTextureColor
		/// </summary>
		/// <param name="position">The position in space for this vertex</param>
		/// <param name="texCoords">The UV co-ords for this vertex</param>
		public VertexPositionColorTexture(Vector3 position, Vector2 texCoords)
		{
			Position = position;
			TextureCoordinate = texCoords;
			Color = Color.White;
		}

		/// <summary>
		///     Creates a new VertexPositionNormalTextureColor
		/// </summary>
		/// <param name="position">The position in space for this vertex</param>
		/// <param name="texCoords">The UV co-ords for this vertex</param>
		/// <param name="color">The color of this vertex</param>
		public VertexPositionColorTexture(Vector3 position, Vector2 texCoords, Color color)
		{
			Position = position;
			TextureCoordinate = texCoords;
			Color = color;
		}

		/// <summary>
		///     The vertex declaration for this vertex type
		/// </summary>
		public static VertexDeclaration VertexDeclaration { get; } = new VertexDeclaration
		(
			new VertexElement(0, VertexElementFormat.Float3,
				VertexElementSemantic.Position, 0),
			new VertexElement(3 * sizeof(float), VertexElementFormat.Float2,
				VertexElementSemantic.TextureCoordinate, 0),
			new VertexElement(5 * sizeof(float), VertexElementFormat.Float3,
				VertexElementSemantic.Color, 0)
		);

		VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
	}
}
