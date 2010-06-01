using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Utility
{
  public static class Geometry
  {
    //public static bool SegmentVsAABB( Vector2 p0, Vector2 p1, Vector2 min, Vector2 max )
    //{
    //  Vector2 dir = p1 - p0;

    //  float enter = 0f, exit = 0f;

    //  if ( !SegmentVsAABB1D( p0.X, dir.X, min.X, max.X, ref enter, ref exit ) )
    //    return false;

    //  if ( !SegmentVsAABB1D( p0.Y, dir.Y, min.Y, max.Y, ref enter, ref exit ) )
    //    return false;

    //  return true;
    //}

    //private static bool SegmentVsAABB1D( float start, float dir, float min, float max, ref float enter, ref float exit )
    //{
    //  if ( dir == 0 )
    //    return ( start >= min && start <= max );

    //  float t0, t1;
    //  t0 = ( min - start ) / dir;
    //  t1 = ( max - start ) / dir;

    //  if ( t0 > t1 )
    //  {
    //    float temp = t0;
    //    t0 = t1;
    //    t1 = temp;
    //  }

    //  if ( t0 > exit || t1 < enter )
    //    return false;

    //  if ( t0 > enter )
    //    enter = t0;
    //  if ( t1 < exit )
    //    exit = t1;

    //  return true;
    //}
  }
}