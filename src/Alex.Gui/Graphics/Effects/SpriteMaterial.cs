using System.Numerics;
using System.Runtime.InteropServices;
using Alex.Engine.Content;
using Veldrid;

namespace Alex.Engine.Graphics.Effects
{
	public sealed class SpriteMaterial : EffectMaterial
	{
		public SpriteMaterial(ContentManager contentManager, Effect effect, in OutputDescription outputDescription)
			: base(contentManager, effect)
		{
			SetSampler(contentManager.LinearClampSampler);

			PipelineState = new EffectPipelineState(
				RasterizerStateDescription.CullNone, 
				DepthStencilStateDescription.Disabled,
				BlendStateDescription.SingleAlphaBlend, outputDescription);
		}

		public void SetMaterialConstantsVS(DeviceBuffer value)
		{
			SetProperty("ProjectionBuffer", value);
		}

		public void SetSampler(Sampler samplerState)
		{
			SetProperty("Sampler", samplerState);
		}

		public void SetTexture(Texture texture)
		{
			SetProperty("Texture", texture);
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct MaterialConstantsVS
		{
			public Matrix4x4 Projection;
		}
	}
}
