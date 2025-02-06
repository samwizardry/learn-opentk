using System.Runtime.CompilerServices;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;

namespace Ugly;

public class HelloTexture : GameWindow
{
    private Shader _shader = null!;

    private readonly float _velocity = 5.0f;
    private float _xPos = 0.0f;
    private float _yPos = 0.0f;
    private float _mixFactor = 0.2f;

    private int _rectangleVAO;
    private int _rectangleVBO;
    private int _rectangleEBO;

    private int _containerTexture;
    private int _awesomefaceTexture;

    private readonly float[] _vertices = [
         // positions         // colors           // texture coords
         0.5f,  0.5f, 0.0f,   1.0f, 0.0f, 0.0f,   1.0f, 1.0f,   // top right
         0.5f, -0.5f, 0.0f,   0.0f, 1.0f, 0.0f,   1.0f, 0.0f,   // bottom right
        -0.5f, -0.5f, 0.0f,   0.0f, 0.0f, 1.0f,   0.0f, 0.0f,   // bottom left
        -0.5f,  0.5f, 0.0f,   1.0f, 1.0f, 0.0f,   0.0f, 1.0f    // top left
    ];

    private readonly uint[] _indices = [
        0, 1, 3,   // first triangle
        1, 2, 3    // second triangle
    ];

    public HelloTexture(int width, int height, string title)
        : base(
            GameWindowSettings.Default,
            new NativeWindowSettings()
            {
                Title = title,
                ClientSize = (width, height),
                Vsync = VSyncMode.On
            })
    { }

    protected override void OnLoad()
    {
        base.OnLoad();

        _shader = new Shader(
            Path.Combine(Environment.CurrentDirectory, "assets", "TextureVertexShader.glsl"),
            Path.Combine(Environment.CurrentDirectory, "assets", "TextureFragmentShader.glsl"));

        _containerTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _containerTexture);
        // set the texture wrapping/filtering options (on the currently bound texture object)
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        // load and generate the texture
        using var container = SixLabors.ImageSharp.Image.Load<Rgb24>(Path.Combine(Environment.CurrentDirectory, "assets", "container.jpg"));
        byte[] pixels = new byte[container.Width * container.Height * Unsafe.SizeOf<Rgb24>()];
        container.CopyPixelDataTo(pixels);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, container.Width, container.Height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, pixels);
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        GL.BindTexture(TextureTarget.Texture2D, 0);

        _awesomefaceTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _awesomefaceTexture);
        // set the texture wrapping/filtering options (on the currently bound texture object)
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        // load and generate the texture
        using var awesomeface = SixLabors.ImageSharp.Image.Load<Rgba32>(Path.Combine(Environment.CurrentDirectory, "assets", "awesomeface.png"));
        awesomeface.Mutate(new FlipProcessor(FlipMode.Vertical));
        pixels = new byte[awesomeface.Width * awesomeface.Height * Unsafe.SizeOf<Rgba32>()];
        awesomeface.CopyPixelDataTo(pixels);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, awesomeface.Width, awesomeface.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        GL.BindTexture(TextureTarget.Texture2D, 0);

        // set up vertex data (and buffer(s)) and configure vertex attributes
        // ------------------------------------------------------------------
        _rectangleVAO = GL.GenVertexArray();
        // bind the Vertex Array Object first, then bind and set vertex buffer(s), and then configure vertex attributes(s).
        GL.BindVertexArray(_rectangleVAO);

        _rectangleVBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _rectangleVBO);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

        _rectangleEBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _rectangleEBO);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

        // positions
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
        // colors
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
        // textures
        GL.EnableVertexAttribArray(2);
        GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));

        // note that this is allowed, the call to glVertexAttribPointer registered VBO as the vertex attribute's bound vertex buffer object so afterwards we can safely unbind
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

        // You can unbind the VAO afterwards so other VAO calls won't accidentally modify this VAO, but this rarely happens. Modifying other
        // VAOs requires a call to glBindVertexArray anyways so we generally don't unbind VAOs (nor VBOs) when it's not directly necessary.
        GL.BindVertexArray(0);

        // remember: do NOT unbind the EBO while a VAO is active as the bound element buffer object IS stored in the VAO; keep the EBO bound.
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
    }

    protected override void OnUnload()
    {
        base.OnUnload();

        // optional: de-allocate all resources once they've outlived their purpose:
        // ------------------------------------------------------------------------
        GL.DeleteVertexArray(_rectangleVAO);
        GL.DeleteBuffer(_rectangleVBO);
        GL.DeleteBuffer(_rectangleEBO);
        GL.DeleteTexture(_containerTexture);

        _shader.Delete();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        if (KeyboardState.IsKeyDown(Keys.Escape))
        {
            Close();
        }

        if (KeyboardState.IsKeyDown(Keys.W))
        {
            _yPos += _velocity * (float)args.Time;
        }
        if (KeyboardState.IsKeyDown(Keys.A))
        {
            _xPos -= _velocity * (float)args.Time;
        }
        if (KeyboardState.IsKeyDown(Keys.S))
        {
            _yPos -= _velocity * (float)args.Time;
        }
        if (KeyboardState.IsKeyDown(Keys.D))
        {
            _xPos += _velocity * (float)args.Time;
        }

        if (KeyboardState.IsKeyDown(Keys.Up))
        {
            _mixFactor += 2.0f * (float)args.Time;
        }
        if (KeyboardState.IsKeyDown(Keys.Down))
        {
            _mixFactor -= 2.0f * (float)args.Time;
        }

        _mixFactor = float.Clamp(_mixFactor, 0.0f, 1.0f);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        //GL.Enable(EnableCap.Blend);
        //GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        _shader.Use();

        _shader.SetFloat1("xPos", _xPos);
        _shader.SetFloat1("yPos", _yPos);
        _shader.SetFloat1("mixFactor", _mixFactor);

        _shader.SetInt1("mainTexture", 0);
        _shader.SetInt1("texture2", 1);

        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, _containerTexture);
        GL.ActiveTexture(TextureUnit.Texture1);
        GL.BindTexture(TextureTarget.Texture2D, _awesomefaceTexture);

        GL.BindVertexArray(_rectangleVAO);
        GL.DrawElements(BeginMode.Triangles, 6, DrawElementsType.UnsignedInt, 0);

        // glfw: swap buffers and poll IO events (keys pressed/released, mouse moved etc.)
        // -------------------------------------------------------------------------------
        SwapBuffers();
    }

    // glfw: whenever the window size changed (by OS or user resize) this callback function executes
    // ---------------------------------------------------------------------------------------------
    protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
    {
        base.OnFramebufferResize(e);
        // make sure the viewport matches the new window dimensions; note that width and 
        // height will be significantly larger than specified on retina displays.
        GL.Viewport(0, 0, e.Width, e.Height);
    }
}
