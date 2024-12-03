using OpenTK.Graphics.OpenGL4;

namespace Ugly;

// Сделать IDisposable

public class Shader : IDisposable
{
    private readonly int _shaderProgram;

    public Shader(in string vertexShaderPath, in string fragmentShaderPath)
    {
        // Vertex shader
        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, File.ReadAllText(vertexShaderPath));
        GL.CompileShader(vertexShader);

        // Check for shader compile errors
        GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out int success);
        if (success != (int)All.True)
        {
            string infoLog = GL.GetShaderInfoLog(vertexShader);
            throw new Exception($"An error occurred whilst compiling vertex shader '{vertexShader}'.\n{infoLog}");
        }

        // Fragment shader
        int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, File.ReadAllText(fragmentShaderPath));
        GL.CompileShader(fragmentShader);

        // Check for shader compile errors
        GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out success);
        if (success != (int)All.True)
        {
            string infoLog = GL.GetShaderInfoLog(fragmentShader);
            throw new Exception($"An error occurred whilst compiling fragment shader '{fragmentShader}'.\n{infoLog}");
        }

        // Link shaders
        _shaderProgram = GL.CreateProgram();
        GL.AttachShader(_shaderProgram, vertexShader);
        GL.AttachShader(_shaderProgram, fragmentShader);
        GL.LinkProgram(_shaderProgram);

        // Check for linking errors
        GL.GetProgram(_shaderProgram, GetProgramParameterName.LinkStatus, out success);
        if (success != (int)All.True)
        {
            string infoLog = GL.GetProgramInfoLog(_shaderProgram);
            throw new Exception($"An error occurred whilst linking  program '{_shaderProgram}'.\n{infoLog}");
        }

        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);
    }

    public void Use()
    {
        GL.UseProgram(_shaderProgram);
    }

    public void Dispose()
    {
        GL.DeleteProgram(_shaderProgram);
    }
}
