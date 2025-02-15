using OpenTK.Mathematics;

namespace Ugly.src;

public class Camera
{
    private Vector3 _position;
    private Vector3 _target;
    private Vector3 _direction;
    private Vector3 _up;
    private Vector3 _right;
    private Matrix4 _view;

    public Camera(Vector3 position,
        Vector3 target,
        Vector3 up)
    {
        _position = position;
        _target = target;
        _direction = Vector3.Normalize(_position - _target);
        _right = Vector3.Normalize(Vector3.Cross(up, _direction));
        _up = Vector3.Cross(_direction, _right);

        _view = Matrix4.LookAt(
            new Vector3(0f, 0f, 3f),
            new Vector3(0f, 0f, 0f),
            new Vector3(0f, 1f, 0f));
    }

    public Vector3 GetDirection()
    {
        return Vector3.Normalize(_position - _target);
    }


}
