using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Ugly;

internal class Game : GameWindow
{
    private readonly float[] _triangle =
        [-0.5f, -0.5f, 0.0f,
          0.5f, -0.5f, 0.0f,
          0.0f,  0.5f, 0.0f];

    private readonly float[] _leftTriangle =
        [-0.9f, -0.5f, 0.0f,
         -0.1f, -0.5f, 0.0f,
         -0.5f,  0.5f, 0.0f];

    private readonly float[] _rightTriangle =
        [0.1f, -0.5f, 0.0f,
         0.9f, -0.5f, 0.0f,
         0.5f,  0.5f, 0.0f];

    private readonly float[] _rectangle =
        [-0.5f, 0.5f, 0.0f,
          0.5f, 0.5f, 0.0f,
          0.5f, -0.5f, 0.0f,
         -0.5f, -0.5f, 0.0f,];

    private readonly uint[] _indices =
        [0, 1, 2,
         0, 3, 2];

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

    private readonly string _fragmentShaderSourceRightTriangle = """
        #version 330 core
        out vec4 FragColor;

        void main()
        {
            FragColor = vec4(0.2f, 0.5f, 1.0f, 1.0f);
        }
        """;

    private int _vertexArrayLeftTriangle;
    private int _vertexBufferLeftTriangle;
    private int _vertexArrayRightTriangle;
    private int _vertexBufferRightTriangle;
    private int _shaderProgram;
    private int _shaderProgramRightTriangle;

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

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

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

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);


        // build and compile our shader program
        // ------------------------------------
        // vertex shader
        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, _vertexShaderSource);
        GL.CompileShader(vertexShader);

        // check for shader compile errors
        GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out int success);
        if (success != (int)All.True)
        {
            string infoLog = GL.GetShaderInfoLog(vertexShader);
            throw new Exception($"An error occurred whilst compiling vertex shader '{vertexShader}'.\n{infoLog}");
        }

        // fragment shader
        int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, _fragmentShaderSource);
        GL.CompileShader(fragmentShader);

        // check for shader compile errors
        GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out success);
        if (success != (int)All.True)
        {
            string infoLog = GL.GetShaderInfoLog(fragmentShader);
            throw new Exception($"An error occurred whilst compiling fragment shader '{fragmentShader}'.\n{infoLog}");
        }

        // fragment shader right triangle
        int fragmentShaderRightTriangle = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShaderRightTriangle, _fragmentShaderSourceRightTriangle);
        GL.CompileShader(fragmentShaderRightTriangle);

        // check for shader compile errors
        GL.GetShader(fragmentShaderRightTriangle, ShaderParameter.CompileStatus, out success);
        if (success != (int)All.True)
        {
            string infoLog = GL.GetShaderInfoLog(fragmentShaderRightTriangle);
            throw new Exception($"An error occurred whilst compiling fragment shader '{fragmentShaderRightTriangle}'.\n{infoLog}");
        }

        // link shaders
        _shaderProgram = GL.CreateProgram();
        GL.AttachShader(_shaderProgram, vertexShader);
        GL.AttachShader(_shaderProgram, fragmentShader);
        GL.LinkProgram(_shaderProgram);

        // check for linking errors
        GL.GetProgram(_shaderProgram, GetProgramParameterName.LinkStatus, out success);
        if (success != (int)All.True)
        {
            string infoLog = GL.GetProgramInfoLog(_shaderProgram);
            throw new Exception($"An error occurred whilst linking  program '{_shaderProgram}'.\n{infoLog}");
        }

        _shaderProgramRightTriangle = GL.CreateProgram();
        GL.AttachShader(_shaderProgramRightTriangle, vertexShader);
        GL.AttachShader(_shaderProgramRightTriangle, fragmentShaderRightTriangle);
        GL.LinkProgram(_shaderProgramRightTriangle);

        // check for linking errors
        GL.GetProgram(_shaderProgramRightTriangle, GetProgramParameterName.LinkStatus, out success);
        if (success != (int)All.True)
        {
            string infoLog = GL.GetProgramInfoLog(_shaderProgramRightTriangle);
            throw new Exception($"An error occurred whilst linking  program '{_shaderProgramRightTriangle}'.\n{infoLog}");
        }

        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);
        GL.DeleteShader(fragmentShaderRightTriangle);
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

        GL.DeleteProgram(_shaderProgram);
        GL.DeleteProgram(_shaderProgramRightTriangle);
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        if (KeyboardState.IsKeyDown(Keys.Escape))
        {
            Close();
        }
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.Clear(ClearBufferMask.ColorBufferBit);

        GL.UseProgram(_shaderProgram);

        GL.BindVertexArray(_vertexArrayLeftTriangle);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

        GL.UseProgram(_shaderProgramRightTriangle);

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
