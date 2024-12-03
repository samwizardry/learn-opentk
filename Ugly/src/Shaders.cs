using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Ugly;

internal class Shaders : GameWindow
{
    public Shaders(int width, int height, string title)
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

        int nrAttributes = GL.GetInteger(GetPName.MaxVertexAttribs);
        Console.WriteLine("Maximum nr of vertex attributes supported: {0}", nrAttributes);
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
