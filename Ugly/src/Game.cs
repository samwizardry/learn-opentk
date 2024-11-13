using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Ugly;

internal class Game : GameWindow
{
    private readonly float[] _vertices =
        [-0.5f, -0.5f, 0.0f,
         0.5f, -0.5f, 0.0f,
         0.0f,  0.5f, 0.0f];

    private readonly uint[] indices = [0, 1, 3];

    private readonly string _vertexShaderSource = """
        #version 330 core
        layout (location = 0) in vec3 aPos;

        void main()
        {
            gl_Position = vec4(aPos.x, aPos.y, aPos.z, 1.0);
        }
        """;

    private readonly string _fragmentShaderSource = """
        #version 330 core
        out vec4 FragColor;

        void main()
        {
            FragColor = vec4(1.0f, 0.5f, 0.2f, 1.0f);
        }
        """;

    private int _vbo;
    private int _vertexShader;
    private int _fragmentShader;
    private int _shaderProgram;
    private int _vao;

    public Game(int width, int height, string title)
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

        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

        _vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

        _vao = GL.GenVertexArray();
        GL.BindVertexArray(_vao);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        _vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(_vertexShader, _vertexShaderSource);
        GL.CompileShader(_vertexShader);

        _fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(_fragmentShader, _fragmentShaderSource);
        GL.CompileShader(_fragmentShader);

        _shaderProgram = GL.CreateProgram();
        GL.AttachShader(_shaderProgram, _vertexShader);
        GL.AttachShader(_shaderProgram, _fragmentShader);
        GL.LinkProgram(_shaderProgram);

        GL.DeleteShader(_vertexShader);
        GL.DeleteShader(_fragmentShader);
    }

    protected override void OnUnload()
    {
        base.OnUnload();

        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);
        GL.UseProgram(0);

        GL.DeleteBuffer(_vbo);
        GL.DeleteVertexArray(_vao);
        GL.DeleteProgram(_shaderProgram);
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        Console.WriteLine(args.Time);

        if (KeyboardState.IsKeyDown(Keys.Escape))
        {
            Close();
        }
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.Clear(ClearBufferMask.ColorBufferBit);

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BindVertexArray(_vao);
        GL.UseProgram(_shaderProgram);

        GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);
        GL.UseProgram(0);

        SwapBuffers();
    }

    protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
    {
        base.OnFramebufferResize(e);

        GL.Viewport(0, 0, e.Width, e.Height);

        Console.WriteLine($"Resized: {e.Width}, {e.Height}");
    }
}
