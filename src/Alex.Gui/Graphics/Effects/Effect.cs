using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using NLog;
using ShaderGen;
using ShaderGen.Glsl;
using ShaderGen.Hlsl;
using ShaderGen.Metal;
using Veldrid;

namespace Alex.Engine.Graphics.Effects
{
   /* public class Effect
    {
	    private Shader VertexShader { get; }
		private Shader FragmentShader { get; }

	    public Effect(Shader vertexShader, Shader fragmentShader)
	    {
		    VertexShader = vertexShader;
		    FragmentShader = fragmentShader;
	    }

	    public void Begin(CommandList encoder)
	    {

	    }

	    public void End()
	    {

	    }

	    internal Shader[] GetShaders()
	    {
		    return new[]
		    {
			    VertexShader,
			    FragmentShader
		    };
	    }


		public static Effect CreateEffect(GraphicsDevice device, string name)
	    {
		    string extension = null;
		    string folder = null;

		    switch (device.BackendType)
		    {
			    case GraphicsBackend.Direct3D11:
				    extension = ".hlsl.bytes";
				    folder = "hlsl";

					break;
			    case GraphicsBackend.Vulkan:
				    extension = ".spv";
				    folder = "spv";

					break;
			    case GraphicsBackend.OpenGL:
				    extension = ".glsl";
				    folder = "glsl";
				    break;
			    case GraphicsBackend.Metal:
				    extension = ".metallib";
				    folder = "metal";

				    break;
			    default: throw new System.InvalidOperationException();
		    }

		    string vertexPath = Path.Combine(System.AppContext.BaseDirectory, "Assets", "Shaders", folder, $"{name}.vertex{extension}");
		    string fragmentPath = Path.Combine(System.AppContext.BaseDirectory, "Assets", "Shaders", folder, $"{name}.fragment{extension}");

			if (File.Exists(vertexPath) && File.Exists(fragmentPath))
		    {
			    byte[] shaderBytes = File.ReadAllBytes(vertexPath);
			    byte[] fragmentBytes = File.ReadAllBytes(vertexPath);

				return new Effect(
					device.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Vertex, shaderBytes, "VS")),
					device.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Fragment, fragmentBytes, "FS")));
		    }
		    else
		    {
				throw new FileNotFoundException();
		    }
	    }
    }*/

	public sealed class Effect : DisposableBase
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(Effect));

		private static byte _nextID = 0;

		private readonly GraphicsDevice _graphicsDevice;

		private VertexLayoutDescription[] _vertexDescriptors;
		private readonly Shader _vertexShader;
		private readonly Shader _pixelShader;

		private readonly ResourceLayout[] _resourceLayouts;

		private readonly Dictionary<EffectPipelineStateHandle, Pipeline> _cachedPipelineStates;

		private readonly Dictionary<string, EffectParameter> _parameters;

		private EffectPipelineStateHandle _pipelineStateHandle;
		public Pipeline PipelineState;

		private EffectDirtyFlags _dirtyFlags;

		public GraphicsDevice GraphicsDevice => _graphicsDevice;

		public byte ID { get; }

		[Flags]
		private enum EffectDirtyFlags
		{
			None = 0,

			PipelineState = 0x1
		}

		public Effect(
			GraphicsDevice graphicsDevice,
			string shaderName,
			VertexLayoutDescription vertexDescriptor,
			bool useNewShaders = false)
			: this(graphicsDevice, shaderName, new[] { vertexDescriptor }, useNewShaders)
		{

		}

		public Effect(
			GraphicsDevice graphicsDevice,
			string shaderName,
			VertexLayoutDescription[] vertexDescriptors,
			bool useNewShaders = false)
		{
			_graphicsDevice = graphicsDevice;

			ID = _nextID++;

			string extension = null;
			string folder = null;

			switch (graphicsDevice.BackendType)
			{
				case GraphicsBackend.Direct3D11:
					extension = ".hlsl.bytes";
					folder = "hlsl";

					break;
				case GraphicsBackend.Vulkan:
					extension = ".spv";
					folder = "spv";

					break;
				case GraphicsBackend.OpenGL:
					extension = "";
					folder = "glsl";
					break;
				case GraphicsBackend.Metal:
					extension = ".metallib";
					folder = "metal";

					break;
				default: throw new System.InvalidOperationException();
			}

			string vertexPath = Path.Combine(System.AppContext.BaseDirectory, "Assets", "Shaders", folder, $"{shaderName}.vertex{extension}");
			string fragmentPath = Path.Combine(System.AppContext.BaseDirectory, "Assets", "Shaders", folder, $"{shaderName}.fragment{extension}");

			if ( File.Exists(vertexPath) && File.Exists(fragmentPath))
			{
				byte[] shaderBytes = File.ReadAllBytes(vertexPath);
				byte[] fragmentBytes = File.ReadAllBytes(fragmentPath);

				//	return new Effect(
				_vertexShader = graphicsDevice.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Vertex, shaderBytes, "VS"));
				_pixelShader = _graphicsDevice.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Fragment, fragmentBytes, "FS")); //);

				Log.Info($"Loaded effect: " + vertexPath);
			}
			

		/*	if (useNewShaders)
			{
				using (var shaderStream = typeof(Effect).Assembly.GetManifestResourceStream($"Alex.Engine.Graphics.Effects.Compiled.{shaderName}-vertex.hlsl.bytes"))
				{
					shaderStream.Position = 0;
					byte[] data = new byte[shaderStream.Length];

					int l = shaderStream.Read(data, 0, data.Length);

					var vertexShaderBytecode = data.Take(l).ToArray();
					_vertexShader = AddDisposable(graphicsDevice.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Vertex, vertexShaderBytecode, "VS")));
				}

				using (var shaderStream = typeof(Effect).Assembly.GetManifestResourceStream($"Alex.Engine.Graphics.Effects.Compiled.{shaderName}-fragment.hlsl.bytes"))
				{
					shaderStream.Position = 0;
					byte[] data = new byte[shaderStream.Length];

					int l = shaderStream.Read(data, 0, data.Length);

					var pixelShaderBytecode = data.Take(l).ToArray();
					_pixelShader = AddDisposable(graphicsDevice.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Fragment, pixelShaderBytecode, "PS")));
				}
			}
			else
			{
				using (var shaderStream = typeof(Effect).Assembly.GetManifestResourceStream($"Alex.Engine.Graphics.Effects.{shaderName}.fxo"))
				using (var shaderReader = new BinaryReader(shaderStream))
				{
					var vertexShaderBytecodeLength = shaderReader.ReadInt32();
					var vertexShaderBytecode = shaderReader.ReadBytes(vertexShaderBytecodeLength);
					_vertexShader = AddDisposable(graphicsDevice.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Vertex, vertexShaderBytecode, "VS")));

					var pixelShaderBytecodeLength = shaderReader.ReadInt32();
					var pixelShaderBytecode = shaderReader.ReadBytes(pixelShaderBytecodeLength);
					_pixelShader = AddDisposable(graphicsDevice.ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Fragment, pixelShaderBytecode, "PS")));
				}
			}*/

			_cachedPipelineStates = new Dictionary<EffectPipelineStateHandle, Pipeline>();

			_vertexDescriptors = vertexDescriptors;

			var shaderDefinition = ShaderDefinitions.GetShaderDefinition(Path.Combine(System.AppContext.BaseDirectory, "Assets", "Shaders", "material", shaderName + ".material"));

			_parameters = new Dictionary<string, EffectParameter>();
			_resourceLayouts = new ResourceLayout[shaderDefinition.ResourceBindings.Length];

			for (var i = 0u; i < shaderDefinition.ResourceBindings.Length; i++)
			{
				var resourceBinding = shaderDefinition.ResourceBindings[i];
				var resourceLayoutDescription = new ResourceLayoutElementDescription(
					resourceBinding.Name,
					resourceBinding.Type,
					resourceBinding.Stages);

				var parameter = AddDisposable(new EffectParameter(
					graphicsDevice,
					resourceBinding,
					in resourceLayoutDescription,
					i));

				_parameters[parameter.Name] = parameter;
				_resourceLayouts[i] = parameter.ResourceLayout;
			}


        }
        private static bool CompileCode(LanguageBackend lang, string shaderPath, string entryPoint, ShaderFunctionType type, out string path)
        {
            Type langType = lang.GetType();
            if (langType == typeof(HlslBackend) && IsFxcAvailable())
            {
                return CompileHlsl(shaderPath, entryPoint, type, out path);
            }
            else if (langType == typeof(Glsl450Backend) && IsGlslangValidatorAvailable())
            {
                return CompileSpirv(shaderPath, entryPoint, type, out path);
            }
            else if (langType == typeof(MetalBackend) && AreMetalToolsAvailable())
            {
                return CompileMetal(shaderPath, out path);
            }
            else
            {
                path = null;
                return false;
            }
        }

        private static bool CompileHlsl(string shaderPath, string entryPoint, ShaderFunctionType type, out string path)
        {
            try
            {
                string profile = type == ShaderFunctionType.VertexEntryPoint ? "vs_5_0"
                    : type == ShaderFunctionType.FragmentEntryPoint ? "ps_5_0"
                    : "cs_5_0";
                string outputPath = shaderPath + ".bytes";
                string args = $"/T {profile} /E {entryPoint} {shaderPath} /Fo {outputPath}";
                string fxcPath = FindFxcExe();
                ProcessStartInfo psi = new ProcessStartInfo(fxcPath, args);
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                Process p = new Process() { StartInfo = psi };
                p.Start();
                var stdOut = p.StandardOutput.ReadToEndAsync();
                var stdErr = p.StandardError.ReadToEndAsync();
                bool exited = p.WaitForExit(2000);

                if (exited && p.ExitCode == 0)
                {
                    path = outputPath;
                    return true;
                }
                else
                {
                    string message = $"StdOut: {stdOut.Result}, StdErr: {stdErr.Result}";
                    Console.WriteLine($"Failed to compile HLSL: {message}.");
                }
            }
            catch (Win32Exception)
            {
                Console.WriteLine("Unable to launch fxc tool.");
            }

            path = null;
            return false;
        }

        private static bool CompileSpirv(string shaderPath, string entryPoint, ShaderFunctionType type, out string path)
        {
            string stage = type == ShaderFunctionType.VertexEntryPoint ? "vert"
                : type == ShaderFunctionType.FragmentEntryPoint ? "frag"
                : "comp";
            string outputPath = shaderPath + ".spv";
            string args = $"-V -S {stage} {shaderPath} -o {outputPath}";
            try
            {

                ProcessStartInfo psi = new ProcessStartInfo("glslangValidator", args);
                psi.RedirectStandardError = true;
                psi.RedirectStandardOutput = true;
                Process p = Process.Start(psi);
                p.WaitForExit();

                if (p.ExitCode == 0)
                {
                    path = outputPath;
                    return true;
                }
                else
                {
                    throw new ShaderGenerationException(p.StandardOutput.ReadToEnd());
                }
            }
            catch (Win32Exception)
            {
                Console.WriteLine("Unable to launch glslangValidator tool.");
            }

            path = null;
            return false;
        }

		private static string s_fxcPath;
		private static bool? s_fxcAvailable;
		private static bool? s_glslangValidatorAvailable;
		private static bool? s_metalToolsAvailable;

		const string metalPath = @"/Applications/Xcode.app/Contents/Developer/Platforms/MacOSX.platform/usr/bin/metal";
		const string metallibPath = @"/Applications/Xcode.app/Contents/Developer/Platforms/MacOSX.platform/usr/bin/metallib";

        private static bool CompileMetal(string shaderPath, out string path)
        {
            string shaderPathWithoutExtension = Path.ChangeExtension(shaderPath, null);
            string outputPath = shaderPathWithoutExtension + ".metallib";
            string bitcodePath = Path.GetTempFileName();
            string metalArgs = $"-x metal -o {bitcodePath} {shaderPath}";
            try
            {
                ProcessStartInfo metalPSI = new ProcessStartInfo(metalPath, metalArgs);
                metalPSI.RedirectStandardError = true;
                metalPSI.RedirectStandardOutput = true;
                Process metalProcess = Process.Start(metalPSI);
                metalProcess.WaitForExit();

                if (metalProcess.ExitCode != 0)
                {
                    throw new ShaderGenerationException(metalProcess.StandardError.ReadToEnd());
                }

                string metallibArgs = $"-o {outputPath} {bitcodePath}";
                ProcessStartInfo metallibPSI = new ProcessStartInfo(metallibPath, metallibArgs);
                metallibPSI.RedirectStandardError = true;
                metallibPSI.RedirectStandardOutput = true;
                Process metallibProcess = Process.Start(metallibPSI);
                metallibProcess.WaitForExit();

                if (metallibProcess.ExitCode != 0)
                {
                    throw new ShaderGenerationException(metallibProcess.StandardError.ReadToEnd());
                }

                path = outputPath;
                return true;
            }
            finally
            {
                File.Delete(bitcodePath);
            }
        }

        public static bool IsFxcAvailable()
        {
            if (!s_fxcAvailable.HasValue)
            {
                s_fxcPath = FindFxcExe();
                s_fxcAvailable = s_fxcPath != null;
            }

            return s_fxcAvailable.Value;
        }

        public static bool IsGlslangValidatorAvailable()
        {
            if (!s_glslangValidatorAvailable.HasValue)
            {
                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo("glslangValidator");
                    psi.RedirectStandardOutput = true;
                    psi.RedirectStandardError = true;
                    Process.Start(psi);
                    s_glslangValidatorAvailable = true;
                }
                catch { s_glslangValidatorAvailable = false; }
            }

            return s_glslangValidatorAvailable.Value;
        }

        public static bool AreMetalToolsAvailable()
        {
            if (!s_metalToolsAvailable.HasValue)
            {
                s_metalToolsAvailable = File.Exists(metalPath) && File.Exists(metallibPath);
            }

            return s_metalToolsAvailable.Value;
        }

        private static string BackendExtension(LanguageBackend lang)
        {
            if (lang.GetType() == typeof(HlslBackend))
            {
                return "hlsl";
            }
            else if (lang.GetType() == typeof(Glsl330Backend))
            {
                return "330.glsl";
            }
            else if (lang.GetType() == typeof(GlslEs300Backend))
            {
                return "300.glsles";
            }
            else if (lang.GetType() == typeof(Glsl450Backend))
            {
                return "450.glsl";
            }
            else if (lang.GetType() == typeof(MetalBackend))
            {
                return "metal";
            }

            throw new InvalidOperationException("Invalid backend type: " + lang.GetType().Name);
        }

        private static string FindFxcExe()
        {
            const string WindowsKitsFolder = @"C:\Program Files (x86)\Windows Kits";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && Directory.Exists(WindowsKitsFolder))
            {
                IEnumerable<string> paths = Directory.EnumerateFiles(
                    WindowsKitsFolder,
                    "fxc.exe",
                    SearchOption.AllDirectories);
                string path = paths.FirstOrDefault(s => !s.Contains("arm"));
                return path;
            }

            return null;
        }

        internal EffectParameter GetParameter(string name, bool throwIfMissing = true)
		{
			if (!_parameters.TryGetValue(name, out var result))
			{
				if (throwIfMissing)
				{
					throw new InvalidOperationException($"Could not find parameter with name '{name}'.");
				}
				else
				{
					return null;
				}
			}
			return result;
		}

		public void Begin(CommandList commandEncoder)
		{
			_dirtyFlags |= EffectDirtyFlags.PipelineState;
			
			foreach (var parameter in _parameters.Values)
			{
				parameter.SetDirty();
			}
		}

		public void ApplyPipelineState(CommandList commandEncoder)
		{
			if (_dirtyFlags.HasFlag(EffectDirtyFlags.PipelineState))
			{
				commandEncoder.SetPipeline(PipelineState);

				_dirtyFlags &= ~EffectDirtyFlags.PipelineState;
			}
		}

		public void ApplyParameters(CommandList commandEncoder)
		{
			foreach (var parameter in _parameters.Values)
			{
				parameter.ApplyChanges(commandEncoder);
			}
		}

		public void SetPipelineState(EffectPipelineStateHandle pipelineStateHandle)
		{
			if (_pipelineStateHandle == pipelineStateHandle)
			{
				return;
			}

			_pipelineStateHandle = pipelineStateHandle;
			PipelineState = GetPipelineState(pipelineStateHandle);

			if (!_dirtyFlags.HasFlag(EffectDirtyFlags.PipelineState))
			{
				_dirtyFlags |= EffectDirtyFlags.PipelineState;
			}
		}

		private Pipeline GetPipelineState(EffectPipelineStateHandle pipelineStateHandle)
		{
			if (!_cachedPipelineStates.TryGetValue(pipelineStateHandle, out var result))
			{
				var description = new GraphicsPipelineDescription(
					pipelineStateHandle.EffectPipelineState.BlendState,
					pipelineStateHandle.EffectPipelineState.DepthStencilState,
					pipelineStateHandle.EffectPipelineState.RasterizerState,
					PrimitiveTopology.TriangleList,
					new ShaderSetDescription(
						_vertexDescriptors,
						new[] { _vertexShader, _pixelShader }),
					_resourceLayouts,
					pipelineStateHandle.EffectPipelineState.OutputDescription);

				_cachedPipelineStates[pipelineStateHandle] = result = AddDisposable(_graphicsDevice.ResourceFactory.CreateGraphicsPipeline(ref description));
			}

			return result;
		}
	}

    internal class TestUtil
    {
        private static readonly string ProjectBasePath = Path.Combine(AppContext.BaseDirectory, "TestAssets");

        public static Compilation GetTestProjectCompilation()
        {
            CSharpCompilation compilation = CSharpCompilation.Create(
                "TestAssembly",
                syntaxTrees: GetSyntaxTrees(),
                references: GetProjectReferences(),
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            return compilation;
        }

        public static SyntaxTree GetSyntaxTree(Compilation compilation, string name)
        {
            foreach (SyntaxTree tree in compilation.SyntaxTrees)
            {
                if (Path.GetFileName(tree.FilePath) == name)
                {
                    return tree;
                }
            }

            throw new InvalidOperationException("Couldn't find a syntax tree with name " + name);
        }

        private static IEnumerable<SyntaxTree> GetSyntaxTrees()
        {
            foreach (string sourceItem in GetCompileItems())
            {
                using (FileStream fs = File.OpenRead(sourceItem))
                {
                    SourceText st = SourceText.From(fs);
                    yield return CSharpSyntaxTree.ParseText(st, path: sourceItem);
                }
            }
        }

        public static IEnumerable<MetadataReference> GetProjectReferences()
        {
            string[] referenceItems = GetReferenceItems();
            string[] packageDirs = GetPackageDirs();
            foreach (string refItem in referenceItems)
            {
                MetadataReference reference = GetFirstReference(refItem, packageDirs);
                if (reference == null)
                {
                    throw new InvalidOperationException("Unable to find reference: " + refItem);
                }

                yield return reference;
            }
        }

        private static MetadataReference GetFirstReference(string path, string[] packageDirs)
        {
            foreach (string packageDir in packageDirs)
            {
                string transformed = path.Replace("{nupkgdir}", packageDir);
                transformed = transformed.Replace("{appcontextbasedirectory}", AppContext.BaseDirectory);
                if (File.Exists(transformed))
                {
                    using (FileStream fs = File.OpenRead(transformed))
                    {
                        var result = MetadataReference.CreateFromStream(fs, filePath: transformed);
                        return result;
                    }
                }
            }

            return null;
        }

        private static string[] GetCompileItems()
        {
            return Directory.EnumerateFiles(ProjectBasePath, "*.cs", SearchOption.AllDirectories).ToArray();
        }

        private static string[] GetReferenceItems()
        {
            string[] lines = File.ReadAllLines(Path.Combine(ProjectBasePath, "References.txt"));
            return lines.Select(l => l.Trim()).ToArray(); ;
        }

        public static string[] GetPackageDirs()
        {
            List<string> dirs = new List<string>();
            dirs.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages"));
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                dirs.Add(@"C:\Program Files\dotnet\sdk\NuGetFallbackFolder");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                dirs.Add("/usr/local/share/dotnet/sdk/NuGetFallbackFolder");
            }
            else
            {
                dirs.Add("/usr/share/dotnet/sdk/NuGetFallbackFolder");
            }

            return dirs.ToArray();
        }

        public static LanguageBackend[] GetAllBackends(Compilation compilation)
        {
            return new LanguageBackend[]
            {
                new HlslBackend(compilation),
                new Glsl330Backend(compilation),
                new Glsl450Backend(compilation)
            };
        }
    }

    public class TempFile : IDisposable
    {
        public readonly string FilePath;

        public TempFile() : this(Path.GetTempFileName()) { }
        public TempFile(string path)
        {
            FilePath = path;
        }

        public static implicit operator string(TempFile tf) => tf.FilePath;

        public void Dispose()
        {
            File.Delete(FilePath);
        }
    }

    public class TempFile2 : IDisposable
    {
        public readonly string FilePath0;
        public readonly string FilePath1;

        public TempFile2() : this(Path.GetTempFileName(), Path.GetTempFileName()) { }
        public TempFile2(string path0, string path1)
        {
            FilePath0 = path0;
            FilePath1 = path1;
        }

        public void Dispose()
        {
            File.Delete(FilePath0);
            File.Delete(FilePath1);
        }
    }
}
