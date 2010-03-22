using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ZombieCraft
{
  public class Camera
  {
    public float Fov;
    public float Aspect;
    public float Near;
    public float Far;
    public Vector3 Position;
    public Vector3 Target;
    public Vector3 Up;

    public Matrix ViewMatrix { get { return Matrix.CreateLookAt( Position, Target, Up ); } }
    public Matrix ProjectionMatrix { get { return Matrix.CreatePerspectiveFieldOfView( Fov, Aspect, Near, Far ); } }

    private Camera() { }

    public Camera( float fov, float aspect, float near, float far, Vector3 pos, Vector3 target )
    {
      Fov      = fov;
      Aspect   = aspect;
      Near     = near;
      Far      = far;
      Position = pos;
      Target   = target;
      Up       = Vector3.UnitY;
    }

    public void Translate( Vector3 offset )
    {
      Position += offset;
      Target += offset;
    }
  }
}