using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Utility
{
  public static class ArrayExtensions
  {
    public static int FindIndex<T>( this T[] array, Predicate<T> match )
    {
      int length = array.Length;
      for ( int i = 0; i < length; ++i )
      {
        if ( match( array[i] ) )
          return i;
      }
      return -1;
    }
  }

  public static class StringBuilderExtentions
  {
    public static StringBuilder AppendInt( this StringBuilder builder, int n )
    {
      if ( n < 0 )
        builder.Append( '-' );

      int index = builder.Length;
      do
      {
        builder.Insert( index, digits, n % 10 + 9, 1 );
        n /= 10;
      } while ( n != 0 );

      return builder;
    }

    public static StringBuilder Clear( this StringBuilder builder )
    {
      return builder.Remove( 0, builder.Length );
    }

    private static readonly char[] digits = new char[]
    {
      '9', '8', '7', '6', '5', '4', '3', '2', '1', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
    };
  }

  public static class RandomExtensions
  {
    public static Vector3 NextVector3( this Random random )
    {
      return new Vector3( (float)random.NextDouble() * 2f - 1f,
                          (float)random.NextDouble() * 2f - 1f,
                          (float)random.NextDouble() * 2f - 1f );
    }

    public static void NextVector3( this Random random, ref Vector3 min, ref Vector3 max, ref Vector3 next )
    {
      next.X = MathHelper.Lerp( min.X, max.X, (float)random.NextDouble() );
      next.Y = MathHelper.Lerp( min.Y, max.Y, (float)random.NextDouble() );
      next.Z = MathHelper.Lerp( min.Z, max.Z, (float)random.NextDouble() );
    }
  }
}