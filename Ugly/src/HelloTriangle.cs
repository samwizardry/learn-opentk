using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Ugly;

internal class HelloTriangle : GameWindow
{
    private readonly float[] _triangle =
        [-0.5f, -0.5f, 0.0f,
          0.5f, -0.5f, 0.0f,
          0.0f,  0.5f, 0.0f];

    private readonly float[] _leftTriangle =
        [-0.9f, -0.5f, 0.0f, 1.0f, 0.0f, 0.0f,
         -0.1f, -0.5f, 0.0f, 0.0f, 1.0f, 0.0f,
         -0.5f,  0.5f, 0.0f, 0.0f, 0.0f, 1.0f];

    private readonly float[] _rightTriangle =
        [0.1f, -0.5f, 0.0f, 1.0f, 0.0f, 0.0f,
         0.9f, -0.5f, 0.0f, 0.0f, 1.0f, 0.0f,
         0.5f,  0.5f, 0.0f, 0.0f, 0.0f, 1.0f];

    private readonly float[] _rectangle =
        [-0.5f, 0.5f, 0.0f,
          0.5f, 0.5f, 0.0f,
          0.5f, -0.5f, 0.0f,
         -0.5f, -0.5f, 0.0f,];

    private readonly uint[] _indices =
        [0, 1, 2,
         0, 3, 2];

    private int _vertexArrayLeftTriangle;
    private int _vertexBufferLeftTriangle;
    private int _vertexArrayRightTriangle;
    private int _vertexBufferRightTriangle;

    Shader _shader = null!;

    //private double _totalElapsedTime = 0.0d;

    public HelloTriangle(int width, int height, string title)
        : base(
            GameWindowSettings.Default,
            new NativeWindowSettings()
            {
                Title = title,
                ClientSize = (width, height),
                Vsync = VSyncMode.Off
            })
    { }

    protected override void OnLoad()
    {
        base.OnLoad();

        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

        // Uncomment this call to draw in wireframe polygons.
        //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);


        // set up vertex data (and buffer(s)) and configure vertex attributes
        // ------------------------------------------------------------------
        _vertexArrayLeftTriangle = GL.GenVertexArray();
        // bind the Vertex Array Object first, then bind and set vertex buffer(s), and then configure vertex attributes(s).
        GL.BindVertexArray(_vertexArrayLeftTriangle);

        _vertexBufferLeftTriangle = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferLeftTriangle);
        GL.BufferData(BufferTarget.ArrayBuffer, _leftTriangle.Length * sizeof(float), _leftTriangle, BufferUsageHint.StaticDraw);

        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));

        // note that this is allowed, the call to glVertexAttribPointer registered VBO as the vertex attribute's bound vertex buffer object so afterwards we can safely unbind
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

        // remember: do NOT unbind the EBO while a VAO is active as the bound element buffer object IS stored in the VAO; keep the EBO bound.
        //glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);

        // You can unbind the VAO afterwards so other VAO calls won't accidentally modify this VAO, but this rarely happens. Modifying other
        // VAOs requires a call to glBindVertexArray anyways so we generally don't unbind VAOs (nor VBOs) when it's not directly necessary.
        GL.BindVertexArray(0);


        // set up right triangle
        // ------------------------------------
        _vertexArrayRightTriangle = GL.GenVertexArray();
        GL.BindVertexArray(_vertexArrayRightTriangle);

        _vertexBufferRightTriangle = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferRightTriangle);
        GL.BufferData(BufferTarget.ArrayBuffer, _rightTriangle.Length * sizeof(float), _rightTriangle, BufferUsageHint.StaticDraw);

        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));

        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);

        _shader = new Shader(
            Path.Combine(Environment.CurrentDirectory, "assets", "VertexShader.glsl"),
            Path.Combine(Environment.CurrentDirectory, "assets", "FragmentShader.glsl"));
    }

    protected override void OnUnload()
    {
        base.OnUnload();

        // optional: de-allocate all resources once they've outlived their purpose:
        // ------------------------------------------------------------------------
        GL.DeleteVertexArray(_vertexArrayLeftTriangle);
        GL.DeleteBuffer(_vertexBufferLeftTriangle);

        GL.DeleteVertexArray(_vertexArrayRightTriangle);
        GL.DeleteBuffer(_vertexBufferRightTriangle);

        _shader.Delete();
    }

    private readonly float _velocity = 5.0f;
    private float _xPos = 0.0f;

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        if (KeyboardState.IsKeyDown(Keys.Escape))
        {
            Close();
        }

        if (KeyboardState.IsKeyDown(Keys.A))
        {
            _xPos -= _velocity * (float)args.Time;
        }

        if (KeyboardState.IsKeyDown(Keys.D))
        {
            _xPos += _velocity * (float)args.Time;
        }

        Console.WriteLine(args.Time);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.Clear(ClearBufferMask.ColorBufferBit);

        //_totalElapsedTime += args.Time;
        //float greenValue = (MathF.Sin((float)_totalElapsedTime) / 2.0f) + 0.5f;
        //int vertexColorLocation = GL.GetUniformLocation(_shaderProgram, "myColor");

        _shader.Use();

        //GL.Uniform4(vertexColorLocation, 0.0f, greenValue, 0.0f, 1.0f);


        _shader.SetFloat1("xPos", _xPos);

        GL.BindVertexArray(_vertexArrayLeftTriangle);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

        GL.BindVertexArray(_vertexArrayRightTriangle);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

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
