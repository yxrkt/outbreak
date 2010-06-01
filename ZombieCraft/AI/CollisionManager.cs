using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace ZombieCraft
{
  static class CollisionManager
  {
    static AIGrid grid;
    static int interleave;
    static int begin = 0;
    static Vector2[] tempAABBVerts = new Vector2[4];

    public static void Initialize( AIGrid grid, int interleave )
    {
      CollisionManager.grid = grid;
      CollisionManager.interleave = interleave;
    }

    public static void Update()
    {
      int cellCount = grid.Rows * grid.Cols;
      for ( int i = begin; i < cellCount; i += interleave )
      {
        EntityList entities = grid[i].Entities;
        List<int> buildings = grid[i].Buildings;

        if ( entities.Count == 0 ) continue;

        Entity.Entities[entities.First().EntityIndex].Collision = 0;
        for ( EntityListNode nodeA = entities.First(); nodeA != null; nodeA = entities.Next( nodeA ) )
        {
          foreach ( int index in buildings )
            CheckForAndManageCollision( ref Entity.Entities[nodeA.EntityIndex], ref Building.Buildings[index] );

          for ( EntityListNode nodeB = entities.Next( nodeA ); nodeB != null; nodeB = entities.Next( nodeB ) )
            CheckForAndManageCollision( ref Entity.Entities[nodeA.EntityIndex], ref Entity.Entities[nodeB.EntityIndex] );
        }
      }

      begin = ( begin + 1 ) % interleave;
    }

    private static void CheckForAndManageCollision( ref Entity entity, ref Building building )
    {
      if ( entity.AABB.Min.X > building.AABB.Max.X || entity.AABB.Max.X < building.AABB.Min.X ||
           entity.AABB.Min.X > building.AABB.Max.X || entity.AABB.Max.X < building.AABB.Min.X )
      {
        entity.Collision = 0;
        return;
      }

      // SAT part 2 here:
      tempAABBVerts[0] = entity.AABB.Min;
      tempAABBVerts[1] = new Vector2( entity.AABB.Min.X, entity.AABB.Max.Y );
      tempAABBVerts[2] = entity.AABB.Max;
      tempAABBVerts[3] = new Vector2( entity.AABB.Max.X, entity.AABB.Min.Y );

      int vertCount = building.Boundary.Length;
      for ( int i = 0; i < vertCount; ++i )
      {
        float lower = float.MaxValue;
        float upper = float.MinValue;
        for ( int j = 0; j < 4; ++j )
        {
          float proj = ( building.Boundary[i].X - tempAABBVerts[j].X ) * building.Normals[i].X +
                       ( building.Boundary[i].Y - tempAABBVerts[j].Y ) * building.Normals[i].Y;
          if ( proj < lower ) lower = proj;
          if ( proj > upper ) upper = proj;
        }

        if ( upper < 0 || lower > building.SATProjections[i] )
        {
          entity.Collision = 0;
          return;
        }
      }

      // resolve collision here:
      Vector2 bToA = Vector2.Zero;
      bToA.X = entity.NextPosition.X - building.Center.X;
      bToA.Y = entity.NextPosition.Z - building.Center.Y;

      // use difference to seperate entities
      Vector2 direction;
      Vector2.Normalize( ref bToA, out direction );

      float resolveMoveSpeed = 2f;

      if ( entity.Collision == 0 || building.Index < entity.Collision )
      {
        entity.Direction.X = resolveMoveSpeed * direction.X;
        entity.Direction.Z = resolveMoveSpeed * direction.Y;
        entity.Collision = 5;
      }
    }

    private static void CheckForAndManageCollision( ref Entity entityA, ref Entity entityB )
    {
      if ( entityA.AABB.Min.X > entityB.AABB.Max.X || entityB.AABB.Min.X > entityA.AABB.Max.X ||
           entityA.AABB.Min.Y > entityB.AABB.Max.Y || entityB.AABB.Min.Y > entityA.AABB.Max.Y )
      {
        if ( entityB.Collision != 5 ) // (this means it's colliding with a building)
          entityB.Collision = 0;
        return;
      }

      // resolve collision here:
      Vector2 bToA = Vector2.Zero;
      bToA.X = entityA.NextPosition.X - entityB.NextPosition.X;
      bToA.Y = entityA.NextPosition.Z - entityB.NextPosition.Z;

      // if pathing, check to see if the zombie is close enough to the beacon to stop
      if ( entityA.Behavior == AISuperBrain.TracePath )
      {
        Beacon beacon = entityA.Beacon;

        Vector2 aToTarget = Vector2.Zero;
        aToTarget.X = beacon.Position.X - entityA.NextPosition.X;
        aToTarget.Y = beacon.Position.Z - entityA.NextPosition.Z;

        if ( aToTarget.LengthSquared() < beacon.RestRadiusSquared )
        {
          float dot;
          Vector2.Dot( ref aToTarget, ref bToA, out dot );
          if ( dot < 0f )
            entityA.Behavior = AISuperBrain.Wander;
        }
      }
      if ( entityB.Behavior == AISuperBrain.TracePath )
      {
        Beacon beacon = entityB.Beacon;

        Vector2 bToTarget = Vector2.Zero;
        bToTarget.X = beacon.Position.X - entityB.NextPosition.X;
        bToTarget.Y = beacon.Position.Z - entityB.NextPosition.Z;

        if ( bToTarget.LengthSquared() < beacon.RestRadiusSquared )
        {
          float dot;
          Vector2.Dot( ref bToTarget, ref bToA, out dot );
          if ( dot > 0f )
            entityB.Behavior = AISuperBrain.Wander;
        }
      }

      // infection!
      if ( entityA.Type == EntityType.Zombie && entityB.Type != EntityType.Zombie )
      {
        //if ( entityA.Behavior != AISuperBrain.MorphToZombie )
          entityB.Behavior = AISuperBrain.MorphToZombie;
      }
      else if ( entityB.Type == EntityType.Zombie && entityA.Type != EntityType.Zombie )
      {
        //if ( entityB.Behavior != AISuperBrain.MorphToZombie )
          entityA.Behavior = AISuperBrain.MorphToZombie;
      }

      // use difference to seperate entities
      Vector2 direction;
      Vector2.Normalize( ref bToA, out direction );

      float resolveMoveSpeed = 2f;

      if ( entityA.Collision == 0 || entityB.Index < entityA.Collision )
      {
        entityA.Direction.X = resolveMoveSpeed * direction.X;
        entityA.Direction.Z = resolveMoveSpeed * direction.Y;
        entityA.Collision = 10;
      }

      if ( entityA.Collision == 0 || entityA.Index < entityB.Collision )
      {
        entityB.Direction.X = resolveMoveSpeed * -direction.X;
        entityB.Direction.Z = resolveMoveSpeed * -direction.Y;
        entityB.Collision = 10;
      }
    }
  }
}