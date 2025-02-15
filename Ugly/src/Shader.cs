using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Ugly;

// TODO: Можно закешировать uniform-ы, заранее пройдя по всем существующим
// TODO: опять забыл что такое in (in string str)

public class Shader : IDisposable
{
    private int _program;
    private bool _disposed;

    private readonly Dictionary<string, int> _uniformLocations = new Dictionary<string, int>();

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
        _program = GL.CreateProgram();
        GL.AttachShader(_program, vertexShader);
        GL.AttachShader(_program, fragmentShader);
        GL.LinkProgram(_program);

        // Check for linking errors
        GL.GetProgram(_program, GetProgramParameterName.LinkStatus, out success);
        if (success != (int)All.True)
        {
            string infoLog = GL.GetProgramInfoLog(_program);
            throw new Exception($"An error occurred whilst linking  program '{_program}'.\n{infoLog}");
        }

        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);
    }

    ~Shader()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            // Освобождение управляемых ресурсов
        }

        // Освобождение неуправляемых ресурсов
        if (_program != 0)
        {
            GL.DeleteProgram(_program);
            _program = 0;
        }

        _disposed = true;
    }

    protected void CheckDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(Shader));
        }
    }

    public void Use()
    {
        CheckDisposed();
        GL.UseProgram(_program);
    }

    public void Delete()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected int GetUniformLocation(in string name)
    {
        if (!_uniformLocations.TryGetValue(name, out int uniformLocation))
        {
            uniformLocation = GL.GetUniformLocation(_program, name);
            _uniformLocations.Add(name, uniformLocation);
        }

        return uniformLocation;
    }

    public void SetInt1(in string name, int value)
    {
        GL.Uniform1(GetUniformLocation(name), value);
    }

    public void SetInt2(in string name, int x, int y)
    {
        GL.Uniform2(GetUniformLocation(name), x, y);
    }

    public void SetInt3(in string name, int x, int y, int z)
    {
        GL.Uniform3(GetUniformLocation(name), x, y, z);
    }

    public void SetInt4(in string name, int x, int y, int z, int w)
    {
        GL.Uniform4(GetUniformLocation(name), x, y, z, w);
    }

    public void SetFloat1(in string name, float value)
    {
        GL.Uniform1(GetUniformLocation(name), value);
    }

    public void SetFloat2(in string name, float x, float y)
    {
        GL.Uniform2(GetUniformLocation(name), x, y);
    }

    public void SetFloat3(in string name, float x, float y, float z)
    {
        GL.Uniform3(GetUniformLocation(name), x, y, z);
    }

    public void SetFloat4(in string name, float x, float y, float z, float w)
    {
        GL.Uniform4(GetUniformLocation(name), x, y, z, w);
    }

    public void SetMat4(string name, int count, bool transpose, ref float value)
    {
        GL.UniformMatrix4(GetUniformLocation(name), count, transpose, ref value);
    }

    public void SetMat4(string name, bool transpose, ref Matrix4 mat)
    {
        GL.UniformMatrix4(GetUniformLocation(name), transpose, ref mat);
    }
}
