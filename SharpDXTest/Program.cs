using System;
using System.Windows.Forms;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

namespace SharpDXTest
{
	internal static class Program
	{
		[STAThread]
		private static void Main()
		{
			var form = new RenderForm("SharpDXTest");


			// SwapChain description
			var desc = new SwapChainDescription() {
				BufferCount = 1,
				ModeDescription = new ModeDescription(form.ClientSize.Width, form.ClientSize.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
				IsWindowed = true,
				OutputHandle = form.Handle,
				SampleDescription = new SampleDescription(1, 0),
				SwapEffect = SwapEffect.Discard,
				Usage = Usage.RenderTargetOutput
			};

			// Create Device and SwapChain
			Device device;
			SwapChain swapChain;
			Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, desc, out device, out swapChain);
			var context = device.ImmediateContext;

			// Ignore all windows events
			var factory = swapChain.GetParent<Factory>();
			factory.MakeWindowAssociation(form.Handle, WindowAssociationFlags.IgnoreAll);

			// New RenderTargetView from the backbuffer
			var backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);
			var renderView = new RenderTargetView(device, backBuffer);

			// Compile Vertex and Pixel shaders
			var shaderFileName = "Shader.fx";
			var vertexShaderByteCode = ShaderBytecode.CompileFromFile(shaderFileName, "VS", "vs_4_0", ShaderFlags.None, EffectFlags.None);
			var vertexShader = new VertexShader(device, vertexShaderByteCode);

			var pixelShaderByteCode = ShaderBytecode.CompileFromFile(shaderFileName, "PS", "ps_4_0", ShaderFlags.None, EffectFlags.None);
			var pixelShader = new PixelShader(device, pixelShaderByteCode);

			// Layout from VertexShader input signature
			var layout = new InputLayout(device, ShaderSignature.GetInputSignature(vertexShaderByteCode), new[] {
				new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
				new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0)
			});

			// Instantiate Vertex buiffer from vertex data
			//var vertices = Buffer.Create(device, BindFlags.VertexBuffer, new[] {
			//	new Vector4( 0.0f,  0.5f, 0.5f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
			//	new Vector4( 0.5f, -0.5f, 0.5f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
			//	new Vector4(-0.5f, -0.5f, 0.5f, 1.0f), new Vector4(1.0f, 1.0f, 1.0f, 1.0f),

			//});
			var vertices = Buffer.Create(device, BindFlags.VertexBuffer, new[] {
				new Vector4( 0.0f,  0.5f, 0.5f, 1.0f),
				new Vector4( 0.5f, -0.5f, 0.5f, 1.0f),
				new Vector4(-0.5f, -0.5f, 0.5f, 1.0f),
			});
			var contantBuffer = new Buffer(device, Utilities.SizeOf<Matrix>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
			//var contantBuffer2 = new Buffer(device, Utilities.SizeOf<Matrix>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 16 * 4);

			// Prepare All the stages
			context.InputAssembler.InputLayout = layout;
			context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
			//context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertices, 32, 0));
			context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertices, 16, 0));
			context.VertexShader.SetConstantBuffer(0, contantBuffer);
			//context.VertexShader.SetConstantBuffer(1, contantBuffer2);
			context.VertexShader.Set(vertexShader);
			context.Rasterizer.SetViewport(new Viewport(0, 0, form.ClientSize.Width, form.ClientSize.Height, 0.0f, 1.0f));
			context.PixelShader.Set(pixelShader);
			context.OutputMerger.SetTargets(renderView);


			form.KeyUp += (sender, args) => {
				switch (args.KeyCode) {
					case Keys.F5:
						swapChain.SetFullscreenState(true, null);
						break;
					case Keys.F4:
						swapChain.SetFullscreenState(false, null);
						break;
					case Keys.Escape:
						form.Close();
						break;
				}
			};

			var view = Matrix.LookAtLH(new Vector3(0, 0, -5), new Vector3(0, 0, 0), Vector3.UnitY);
			var proj = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, form.ClientSize.Width / (float)form.ClientSize.Height, 0.1f, 100.0f);
			var viewProj = Matrix.Multiply(view, proj);
			//viewProj.Transpose();

			// Main loop
			RenderLoop.Run(form, () => {
				context.ClearRenderTargetView(renderView, Color.Black);

				var world = Matrix.Identity;
				//world.Transpose();
				var wvp = world * viewProj;

				wvp.Transpose();
				context.UpdateSubresource(ref wvp, contantBuffer);
				//context.UpdateSubresource(ref world, contantBuffer2);
				// Update WorldViewProj Matrix
				//var worldViewProj = viewProj;
				//worldViewProj.Transpose();

				context.Draw(3, 0);



				world.TranslationVector = new Vector3(1, 0, 0);
				wvp = world * viewProj;
				wvp.Transpose();
				context.UpdateSubresource(ref wvp, contantBuffer);
				context.Draw(3, 0);

				swapChain.Present(0, PresentFlags.None);
			});

			// Release all resources
			vertexShaderByteCode.Dispose();
			vertexShader.Dispose();
			pixelShaderByteCode.Dispose();
			pixelShader.Dispose();
			vertices.Dispose();
			layout.Dispose();
			renderView.Dispose();
			backBuffer.Dispose();
			context.ClearState();
			context.Flush();
			device.Dispose();
			context.Dispose();
			swapChain.Dispose();
			factory.Dispose();
		}
	}
}