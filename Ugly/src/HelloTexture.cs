using System.Runtime.CompilerServices;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
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

    private int _cubeVAO;
    private int _cubeVBO;
    private int _cubeEBO;

    private int _containerTexture;
    private int _awesomefaceTexture;

    private readonly Vector3[] _cubePositions = [
        new Vector3( 0.0f,  0.0f,  0.0f),
        new Vector3( 2.0f,  5.0f, -15.0f),
        new Vector3(-1.5f, -2.2f, -2.5f),
        new Vector3(-3.8f, -2.0f, -12.3f),
        new Vector3( 2.4f, -0.4f, -3.5f),
        new Vector3(-1.7f,  3.0f, -7.5f),
        new Vector3( 1.3f, -2.0f, -2.5f),
        new Vector3( 1.5f,  2.0f, -2.5f),
        new Vector3( 1.5f,  0.2f, -1.5f),
        new Vector3(-1.3f,  1.0f, -1.5f)
    ];

    private Vector3 _cameraPosition = new Vector3(0.0f, 0.0f, 3.0f);
    private Vector3 _cameraFront = new Vector3(0.0f, 0.0f, -1.0f);
    private Vector3 _cameraUp = new Vector3(0.0f, 1.0f, 0.0f);

    private Matrix4 _projection = Matrix4.CreatePerspectiveFieldOfView(float.DegreesToRadians(85.0f), 1280.0f / 720.0f, 0.1f, 100.0f);

    private Vector2 _lastMousePosition = Vector2.Zero;
    private Vector2 _lookDirection = Vector2.Zero;
    private float _yaw = 0.0f;
    private float _pitch = 0.0f;
    private float _fov = 85.0f;

    public float Time { get; private set; } = 0.0f;

    private struct Cube
    {
        public readonly float[] Vertices = [
            // front
            -0.5f, -0.5f, 0.5f, 0.0f, 0.0f,
            -0.5f,  0.5f, 0.5f, 0.0f, 1.0f,
             0.5f,  0.5f, 0.5f, 1.0f, 1.0f,
             0.5f, -0.5f, 0.5f, 1.0f, 0.0f,
            // back
            -0.5f, -0.5f, -0.5f, 0.0f, 0.0f,
            -0.5f,  0.5f, -0.5f, 0.0f, 1.0f,
             0.5f,  0.5f, -0.5f, 1.0f, 1.0f,
             0.5f, -0.5f, -0.5f, 1.0f, 0.0f,
            // left
            -0.5f, -0.5f,  0.5f, 0.0f, 0.0f,
            -0.5f,  0.5f,  0.5f, 0.0f, 1.0f,
            -0.5f,  0.5f, -0.5f, 1.0f, 1.0f,
            -0.5f, -0.5f, -0.5f, 1.0f, 0.0f,
            //right
             0.5f, -0.5f,  0.5f, 0.0f, 0.0f,
             0.5f,  0.5f,  0.5f, 0.0f, 1.0f,
             0.5f,  0.5f, -0.5f, 1.0f, 1.0f,
             0.5f, -0.5f, -0.5f, 1.0f, 0.0f,
            // bottom
            -0.5f, -0.5f,  0.5f, 0.0f, 0.0f,
            -0.5f, -0.5f, -0.5f, 0.0f, 1.0f,
             0.5f, -0.5f, -0.5f, 1.0f, 1.0f,
             0.5f, -0.5f,  0.5f, 1.0f, 0.0f,
            // top
            -0.5f, 0.5f,  0.5f, 0.0f, 0.0f,
            -0.5f, 0.5f, -0.5f, 0.0f, 1.0f,
             0.5f, 0.5f, -0.5f, 1.0f, 1.0f,
             0.5f, 0.5f,  0.5f, 1.0f, 0.0f
        ];

        public readonly uint[] Indices = [
            0, 1, 2, 2, 3, 0,
            4, 5, 6, 6, 7, 4,
            8, 9, 10, 10, 11, 8,
            12, 13, 14, 14, 15, 12,
            16, 17, 18, 18, 19, 16,
            20, 21, 22, 22, 23, 20
        ];

        public Cube() { }
    }

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

        CursorState = CursorState.Grabbed;

        GL.Enable(EnableCap.DepthTest);

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
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, awesomeface.Width, awesomeface.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

        // set up vertex data (and buffer(s)) and configure vertex attributes
        // ------------------------------------------------------------------
        Cube cube = new Cube();
        _cubeVAO = GL.GenVertexArray();
        // bind the Vertex Array Object first, then bind and set vertex buffer(s), and then configure vertex attributes(s).
        GL.BindVertexArray(_cubeVAO);

        _cubeVBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _cubeVBO);
        GL.BufferData(BufferTarget.ArrayBuffer, cube.Vertices.Length * sizeof(float), cube.Vertices, BufferUsageHint.StaticDraw);

        _cubeEBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _cubeEBO);
        GL.BufferData(BufferTarget.ElementArrayBuffer, cube.Indices.Length * sizeof(uint), cube.Indices, BufferUsageHint.StaticDraw);

        // positions
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
        // tex coords
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
        // You can unbind the VAO afterwards so other VAO calls won't accidentally modify this VAO, but this rarely happens. Modifying other
        // VAOs requires a call to glBindVertexArray anyways so we generally don't unbind VAOs (nor VBOs) when it's not directly necessary.
        //GL.BindVertexArray(0);
        // note that this is allowed, the call to glVertexAttribPointer registered VBO as the vertex attribute's bound vertex buffer object so afterwards we can safely unbind
        //GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        // remember: do NOT unbind the EBO while a VAO is active as the bound element buffer object IS stored in the VAO; keep the EBO bound.
        //GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
    }

    protected override void OnUnload()
    {
        base.OnUnload();

        // optional: de-allocate all resources once they've outlived their purpose:
        // ------------------------------------------------------------------------
        GL.DeleteVertexArray(_cubeVAO);
        GL.DeleteBuffer(_cubeVBO);
        GL.DeleteBuffer(_cubeEBO);
        GL.DeleteTexture(_containerTexture);
        GL.DeleteTexture(_awesomefaceTexture);

        _shader.Delete();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        Time += (float)args.Time;

        if (KeyboardState.IsKeyDown(Keys.Escape))
        {
            Close();
        }

        // Mouse move

        float cameraSpeed = 10f;

        if (IsKeyDown(Keys.W))
            _cameraPosition += cameraSpeed * (float)args.Time * _cameraFront;

        if (IsKeyDown(Keys.S))
            _cameraPosition -= cameraSpeed * (float)args.Time * _cameraFront;

        if (IsKeyDown(Keys.A))
            _cameraPosition -= cameraSpeed * (float)args.Time * Vector3.Normalize(Vector3.Cross(_cameraFront, _cameraUp));

        if (IsKeyDown(Keys.D))
            _cameraPosition += cameraSpeed * (float)args.Time * Vector3.Normalize(Vector3.Cross(_cameraFront, _cameraUp));

        // Look at

        _lookDirection = MousePosition - _lastMousePosition;
        _lastMousePosition = MousePosition;

        _yaw += _lookDirection.X * (float)args.Time * 5.0f;
        _pitch -= _lookDirection.Y * (float)args.Time * 5.0f;

        _pitch = float.Clamp(_pitch, -89.0f, 89.0f);

        Vector3 direction = new Vector3(
            MathF.Cos(float.DegreesToRadians(_yaw)) * MathF.Cos(float.DegreesToRadians(_pitch)),
            MathF.Sin(float.DegreesToRadians(_pitch)),
            MathF.Sin(float.DegreesToRadians(_yaw)) * MathF.Cos(float.DegreesToRadians(_pitch)));

        _cameraFront = Vector3.Normalize(direction);

        // Zoom in

        if (IsKeyDown(Keys.Up))
            _fov -= (float)args.Time * 85.0f;
        if (IsKeyDown(Keys.Down))
            _fov += (float)args.Time * 85.0f;

        _fov = float.Clamp(_fov, 10.0f, 85.0f);

        _projection = Matrix4.CreatePerspectiveFieldOfView(float.DegreesToRadians(_fov), 1280.0f / 720.0f, 0.1f, 100.0f);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _shader.Use();
        _shader.SetInt1("texture2", 1);

        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, _containerTexture);
        GL.ActiveTexture(TextureUnit.Texture1);
        GL.BindTexture(TextureTarget.Texture2D, _awesomefaceTexture);

        Matrix4 view = Matrix4.LookAt(
            _cameraPosition,
            _cameraPosition + _cameraFront,
            _cameraUp);

        _shader.SetMat4("view", false, ref view);
        _shader.SetMat4("projection", false, ref _projection);

        GL.BindVertexArray(_cubeVAO);

        for (int i = 0; i < _cubePositions.Length; i++)
        {
            Matrix4 transform = Matrix4.Identity;
            if (i % 3 == 0)
            {
                transform *=
                    Matrix4.CreateFromAxisAngle(new Vector3(1.0f, 0.3f, 0.5f), float.DegreesToRadians(10f * int.Clamp(i, 1, int.MaxValue) * Time)) *
                    Matrix4.CreateTranslation(_cubePositions[i]);
            }
            else
            {
                transform *=
                    Matrix4.CreateFromAxisAngle(new Vector3(1.0f, 0.3f, 0.5f), float.DegreesToRadians(20.0f * i)) *
                    Matrix4.CreateTranslation(_cubePositions[i]);
            }

            _shader.SetMat4("model", false, ref transform);

            GL.DrawElements(BeginMode.Triangles, 36, DrawElementsType.UnsignedInt, 0);
        }

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
