using Alex.Engine.Vertices;
using Veldrid;

namespace Alex.Engine.Graphics
{
    public class VertexBuffer : DeviceBuffer
    {
	    private DeviceBuffer _underlying;
	    private int _elementSize;
	    private GraphicsDevice _device;
		public VertexBuffer(GraphicsDevice device, VertexDeclaration declaration, int elements, BufferUsage usage)
		{
			_elementSize = declaration.VertexStride;
			_device = device;
		    _underlying =
			    device.ResourceFactory.CreateBuffer(new BufferDescription((uint) (elements * declaration.VertexStride), usage));

	    }

	    public VertexBuffer(GraphicsDevice device, int elementSize, int elements, BufferUsage usage)
	    {
		    _elementSize = elementSize;
		    _device = device;
		    _underlying =
			    device.ResourceFactory.CreateBuffer(new BufferDescription((uint)(elements * elementSize), usage));

	    }

		public override void Dispose()
	    {
			_underlying.Dispose();

		}

	    public override uint SizeInBytes => _underlying.SizeInBytes;

	    public override BufferUsage Usage => _underlying.Usage;

	    public override string Name {
		    get { return _underlying.Name; }
		    set { _underlying.Name = value; }
	    }

	    public int VertexCount => (int) (SizeInBytes / _elementSize);

	    public void SetData<T>(T[] vertices) where T : struct 
		{
		   _device.UpdateBuffer(_underlying, 0, vertices);
	    }
    }
}
