

using Microsoft.Xna.Framework;
namespace ZombieCraft
{
  struct InstanceTransformRef
  {
    internal int index;
    internal StaticInstanceVertex[] array;

    public Vector3 Position
    {
      get { return array[index].Position; }
      set { array[index].Position = value; }
    }
    public float Scale
    {
      get { return array[index].Scale; }
      set { array[index].Scale = value; }
    }
    public Vector3 Axis
    {
      get { return array[index].Axis; }
      set { array[index].Axis = value; }
    }
    public float Angle
    {
      get { return array[index].Angle; }
      set { array[index].Angle = value; }
    }

    public InstanceTransformRef( int index, StaticInstanceVertex[] array )
    {
      this.index = index;
      this.array = array;
    }
  }
}
