using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ContentTypes;
using Microsoft.Xna.Framework;

namespace ZombieCraft
{
  class City
  {
    StillModel model;

    public Matrix Transform;

    public City()
    {
      model = ZombieCraft.Instance.Content.Load<StillModel>( "Models/City" );
      Transform = Matrix.CreateScale( .01f );
    }

    public void Draw( Matrix view, Matrix projection )
    {
      model.Draw( Transform, view, projection );
    }
  }
}
