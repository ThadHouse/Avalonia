namespace Avalonia.LinuxFramebuffer
{
    public enum SurfaceOrientation
    {
        Rotation0,
        Rotation90,
        Rotation180,
        Rotation270,
    }

    internal interface ISurfaceOrientation
    {
        SurfaceOrientation Orientation { get; }
    }
}
