using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ContentTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ZombieCraft
{
  class Beacon
  {
    StillModel model;
    Quad halo;
    Matrix rotationScale;
    Texture2D label;

    const float SettleRadiusFactor = .01f;

    public Vector3 Position;
    public bool Active;
    public float RadiusSquared = 1;
    public float RestRadiusSquared = 1;
    public int Version = 0;

    public static Beacon[] Beacons;

    public delegate void ActivatedEvent( Vector3 target );
    public event ActivatedEvent Activated;

    public delegate void DeactivatedEvent();
    public event DeactivatedEvent Deactivated;

    public Beacon( int index )
      : this( index, Vector3.Zero, false )
    {
    }

    public Beacon( int index, Vector3 position, bool active )
    {
      rotationScale = Matrix.CreateScale( .5f );
      Position = position;
      Active = active;

      ContentManager content = ZombieCraft.Instance.Content;
      model = content.Load<StillModel>( "Models/beacon" );

      Texture2D haloTexture = content.Load<Texture2D>( "Textures/halo" );
      Vector3[] verts = 
      {
        new Vector3(-1f, 0f,-1f ),
        new Vector3( 1f, 0f,-1f ),
        new Vector3( 1f, 0f, 1f ),
        new Vector3(-1f, 0f, 1f ),
      };
      halo = new Quad( verts, Position, haloTexture );

      switch ( index )
      {
        case 0:
          label = content.Load<Texture2D>( "Textures/beaconLabelA" );
          break;
        case 1:
          label = content.Load<Texture2D>( "Textures/beaconLabelB" );
          break;
        case 2:
          label = content.Load<Texture2D>( "Textures/beaconLabelX" );
          break;
      }
    }

    public void ActivateAt( Vector3 position, float radius )
    {
      RadiusSquared = radius * radius;
      RestRadiusSquared = radius * radius * SettleRadiusFactor * SettleRadiusFactor;

      Position = position;
      Active = true;
      if ( Activated != null )
        Activated( position );

      halo.Position = position;
      halo.Scale = (float)Math.Sqrt( RadiusSquared );

      Version++;
    }

    public void Deactivate()
    {
      Active = false;
      if ( Deactivated != null )
        Deactivated();
    }

    public void Draw( Matrix view, Matrix projection )
    {
      if ( Active )
      {
        Matrix translation = Matrix.CreateTranslation( Position );
        model.Draw( rotationScale * translation, view, projection );

        halo.Draw( view, projection );

        // get screen space coordinate for label
        Viewport viewport = ZombieCraft.Instance.GraphicsDevice.Viewport;
        Vector4 labelWorldPos = new Vector4( Position.X, Position.Y + 20, Position.Z, 1 );
        Vector4 labelNdcPos = Vector4.Transform( labelWorldPos, view * projection );
        labelNdcPos /= labelNdcPos.W;
        Vector2 labelScreenPos = new Vector2( viewport.Width * ( .5f * labelNdcPos.X + .5f ), 
                                              viewport.Height * ( -.5f * labelNdcPos.Y + .5f ) );
        Vector2 labelOrigin = new Vector2( label.Width, label.Height ) / 2;
        SpriteBatch spriteBatch = ZombieCraft.Instance.ScreenManager.SpriteBatch;
        spriteBatch.Begin( SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None );
        spriteBatch.Draw( label, labelScreenPos, null, Color.White, 0f, 
                          labelOrigin, .35f, SpriteEffects.None, 0f );
        spriteBatch.End();
      }
    }
  }
}