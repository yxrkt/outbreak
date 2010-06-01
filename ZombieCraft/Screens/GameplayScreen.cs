#region File Description
//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ContentTypes;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using Utility;
#endregion

namespace ZombieCraft
{
  class GameplayScreen : GameScreen
  {
    #region Fields and Properties

    ContentManager content;
    SpriteFont gameFont;

    const int nEntities = 10000;
    int maxEntitiesPerFrame = nEntities / 4;
    int entityBegin;
    int entityEnd;
    Entity[] entities;
    City city;
    int scrollValue;
    Texture2D cursor;
    Texture2D[] availableBeacons;
    Quad beaconHaloGhost;
    float closeZoom = 100f;
    float farZoom = 1150f;
    float minBeaconRadius = 200;
    float maxBeaconRadius = 1200;
    float totalUnpausedTime = 0f;
    Camera camera;

    // eventually, these will be part of some sort of player class
    Beacon[] beacons;
    List<int> placedBeacons = new List<int>( 3 );

    Random random = new Random( 1 );

    #endregion

    #region Initialization

    public GameplayScreen()
    {
      TransitionOnTime = TimeSpan.FromSeconds( 1.5 );
      TransitionOffTime = TimeSpan.FromSeconds( 0.5 );
    }

    public override void LoadContent()
    {
      if ( content == null )
        content = new ContentManager( ScreenManager.Game.Services, "Content" );

      ZombieCraft.Instance.TimeRuler.StartFrame();
      ZombieCraft.Instance.TimeRuler.Visible = true;
      ZombieCraft.Instance.TimeRuler.ShowLog = true;
      ZombieCraft.Instance.FpsCounter.Visible = true;

      gameFont = content.Load<SpriteFont>( "Fonts/gamefont" );

      //TODO: Throw these in player class
      cursor = content.Load<Texture2D>( "Textures/cursor" );
      availableBeacons = new Texture2D[]
      {
        content.Load<Texture2D>( "Textures/beaconLabelA" ),
        content.Load<Texture2D>( "Textures/beaconLabelB" ),
        content.Load<Texture2D>( "Textures/beaconLabelX" ),
      };
      Texture2D halo = content.Load<Texture2D>( "Textures/halo" );
      beaconHaloGhost = new Quad( Quad.XZPlaneUnitQuad, Vector3.Zero, halo );
      beaconHaloGhost.Color = new Color( Color.Yellow, .5f );

      int entityCount = /**/3;/*/nEntities;/**/
      float gridSize  = /**/50;/*/1200;/**/
      maxEntitiesPerFrame = (int)MathHelper.Max( entityCount / 4, 1 );

      // create the camera
      float aspect = ScreenManager.Game.GraphicsDevice.Viewport.AspectRatio;
      InstancedModelDrawer.Camera = new Camera( MathHelper.PiOver4, aspect, 1f, 5000f,
                                                new Vector3( 0, gridSize, gridSize ), Vector3.Zero );
      camera = InstancedModelDrawer.Camera;

      // create the city
      city = new City();

      // create the beacon
      beacons = new Beacon[3]{ new Beacon( 0 ), new Beacon( 1 ), new Beacon( 2 ) };
      Beacon.Beacons = beacons;

      // create the civilians and zombies
      entities = new Entity[entityCount];
      Entity.Entities = entities;
      Entity.CivilianModel = content.Load<InstancedModel>( "Models/civilian" );
      Entity.ZombieModel = content.Load<InstancedModel>( "Models/zombie" );
      float scale = .95f;// *(float)Math.Sqrt( 2 ) / 2;
      /**/
      for ( int i = 0; i < entityCount; ++i )
      {
        entities[i] = new Entity( i );
        if ( i == 0 )
        {
          entities[i].Transform = InstancedModelDrawer.GetInstanceRef( Entity.ZombieModel );
          entities[i].Type = EntityType.Zombie;
        }
        else
        {
          entities[i].Transform = InstancedModelDrawer.GetInstanceRef( Entity.CivilianModel );
          entities[i].Type = EntityType.Civilian;
        }
        entities[i].Transform.Position = new Vector3( scale * gridSize * ( (float)random.NextDouble() - .5f ),
                                                      0,
                                                      scale * gridSize * ( (float)random.NextDouble() - .5f ) );
        entities[i].Transform.Axis  = InstancedModelDrawer.Camera.Up;
        entities[i].Transform.Angle = (float)random.NextDouble() * MathHelper.TwoPi;
        entities[i].Transform.Scale = 1f;

        entities[i].NextPosition = entities[i].Transform.Position;
        entities[i].NextAngle = entities[i].Transform.Angle;

        entities[i].HalfSize = 3;
        Vector2 boundsOffset = new Vector2( entities[i].HalfSize, entities[i].HalfSize );
        Vector2 gridPosition = new Vector2( entities[i].Transform.Position.X, 
                                            entities[i].Transform.Position.Z );
        entities[i].AABB = new AABB( gridPosition - boundsOffset,
                                     gridPosition + boundsOffset );
        //entities[i].Behavior += AISuperBrain.Wander;
        entities[i].Behavior += AISuperBrain.Wander;
        entities[i].MoveSpeed = 10;
      }
      /*/

      /**/



      AISuperBrain.InitializeGrid( gridSize, gridSize, (int)( gridSize / 25 ), (int)( gridSize / 25 ) );
      AISuperBrain.PopulateGrid( entities, Building.Buildings );

      ScreenManager.Game.ResetElapsedTime();
    }

    public override void UnloadContent()
    {
      content.Unload();
    }

    #endregion

    #region Update and Draw

    public override void Update( GameTime gameTime, bool otherScreenHasFocus,
                                                    bool coveredByOtherScreen )
    {
      base.Update( gameTime, otherScreenHasFocus, coveredByOtherScreen );

      if ( IsActive )
      {
        int entityCount = entities.Length;
        int entitiesPerFrame = Math.Min( maxEntitiesPerFrame, entityCount );

        if ( entityBegin == 0 && entityEnd == 0 )
          entityEnd += entitiesPerFrame;

        AISuperBrain.Elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
        AISuperBrain.PathsFound = 0;

        CollisionManager.Update();

        if ( entityBegin > entityEnd )
        {
          for ( int i = 0; i < entityEnd; ++i )
            AISuperBrain.Update( ref entities[i] );
          for ( int i = entityEnd; i < entityBegin; ++i )
            AISuperBrain.UpdateFast( ref entities[i] );
          for ( int i = entityBegin; i < entityCount; ++i )
            AISuperBrain.Update( ref entities[i] );
        }
        else
        {
          for ( int i = 0; i < entityBegin; ++i )
            AISuperBrain.UpdateFast( ref entities[i] );
          for ( int i = entityBegin; i < entityEnd; ++i )
            AISuperBrain.Update( ref entities[i] );
          for ( int i = entityEnd; i < entityCount; ++i )
            AISuperBrain.UpdateFast( ref entities[i] );
        }

        entityBegin = ( entityBegin + entitiesPerFrame ) % entityCount;
        entityEnd = ( entityEnd + entitiesPerFrame ) % entityCount;
        /**/

        totalUnpausedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
      }
    }

    public override void HandleInput( InputState input )
    {
      if ( input == null )
        throw new ArgumentNullException( "input" );

      // Look up inputs for the active player profile.
      int playerIndex = (int)ControllingPlayer.Value;

      KeyboardState keyboardState = input.CurrentKeyboardStates[playerIndex];
      GamePadState gamePadState = input.CurrentGamePadStates[playerIndex];

      bool gamePadDisconnected = !gamePadState.IsConnected &&
                                 input.GamePadWasConnected[playerIndex];

      if ( input.IsPauseGame( ControllingPlayer ) || gamePadDisconnected )
      {
        //ScreenManager.AddScreen( new PauseMenuScreen(), ControllingPlayer );
        ZombieCraft.Instance.Exit();
      }
      else
      {
        float mouseZoomSensitivity = 1f;
        float triggerZoomSensitivity = 9f;

        Vector3 zoomVector = Vector3.Normalize( new Vector3( 0, 1, 1 ) );

        // camera zoom (controller)
        float zoom = triggerZoomSensitivity * ( input.CurrentGamePadStates[0].Triggers.Left - 
                                                input.CurrentGamePadStates[0].Triggers.Right );

        // camera zoom (mouse wheel)
        float lastScrollValue = scrollValue;
        scrollValue = Mouse.GetState().ScrollWheelValue;
        if ( lastScrollValue != scrollValue )
          zoom = mouseZoomSensitivity * ( lastScrollValue - scrollValue );

        Camera camera = InstancedModelDrawer.Camera;
        camera.Position += zoomVector * zoom;

        // clamp zoom
        float lookLength = ( camera.Position - camera.Target ).Length();
        if ( lookLength < closeZoom || camera.Position.Y < 0 )
          camera.Position = camera.Target + closeZoom * zoomVector;
        else if ( lookLength > farZoom )
          camera.Position = camera.Target + farZoom * zoomVector;

        // camera pan (controller)
        float panSensitivity = 5f;
        if ( gamePadState.ThumbSticks.Left.X != 0 )
        {
          float movement = panSensitivity * gamePadState.ThumbSticks.Left.X;
          camera.Target.X += movement;
          camera.Position.X += movement;
        }
        if ( gamePadState.ThumbSticks.Left.Y != 0 )
        {
          float movement = panSensitivity * -gamePadState.ThumbSticks.Left.Y;
          camera.Target.Z += movement;
          camera.Position.Z += movement;
        }

        // place beacon with controller
        if ( input.IsNewButtonPress( Buttons.A, PlayerIndex.One ) )
          PlaceBeacon( 0 );
        if ( input.IsNewButtonPress( Buttons.B, PlayerIndex.One ) )
          PlaceBeacon( 1 );
        if ( input.IsNewButtonPress( Buttons.X, PlayerIndex.One ) )
          PlaceBeacon( 2 );
        if ( input.IsNewButtonPress( Buttons.Y, PlayerIndex.One ) )
          RemoveLastBeacon();

        // click to set target position
        MouseState mouseState = Mouse.GetState();
        if ( mouseState.LeftButton == ButtonState.Pressed )
          PlaceBeacon( 0 );
        if ( mouseState.RightButton == ButtonState.Pressed )
          RemoveLastBeacon();
      }
    }

    public void PlaceBeacon( int beacon )
    {
      Viewport viewport = ZombieCraft.Instance.GraphicsDevice.Viewport;
#if XBOX
      Vector2 target = PickGround( viewport.Width / 2, viewport.Height / 2 );
#else
      Vector2 target = PickGround( Mouse.GetState().X, Mouse.GetState().Y );
#endif
      if ( beacons[beacon].Active )
        placedBeacons.Remove( beacon );
      beacons[beacon].ActivateAt( new Vector3( target.X, 0, target.Y ), GetRadiusForBeacon() );
      placedBeacons.Add( beacon );
    }

    public void RemoveLastBeacon()
    {
      if ( placedBeacons.Count != 0 )
      {
        beacons[placedBeacons[placedBeacons.Count - 1]].Deactivate();
        placedBeacons.RemoveAt( placedBeacons.Count - 1 );
      }
    }

    public override void Draw( GameTime gameTime )
    {
      ScreenManager.GraphicsDevice.Clear( ClearOptions.Target,
                                          Color.CornflowerBlue, 0, 0 );

      ZombieCraft.Instance.TimeRuler.BeginMark( 1, "Draw Instanced Models", Color.White );
      InstancedModelDrawer.Draw();
      ZombieCraft.Instance.TimeRuler.EndMark( 1, "Draw Instanced Models" );

      Camera camera = InstancedModelDrawer.Camera;
      Matrix view = camera.ViewMatrix;
      Matrix projection = camera.ProjectionMatrix;

      AISuperBrain.DrawGrid( view, projection );
      city.Draw( view, projection );
      foreach ( Beacon beacon in beacons )
        beacon.Draw( view, projection );

      DrawHUD( view, projection );

      // If the game is transitioning on or off, fade it out to black.
      if ( TransitionPosition > 0 )
        ScreenManager.FadeBackBufferToBlack( 255 - TransitionAlpha );
    }

    private void DrawHUD( Matrix view, Matrix projection )
    {
      Viewport viewport = ZombieCraft.Instance.GraphicsDevice.Viewport;
      float globalScale = viewport.Height / 1080f;

      // draw halo ghost
      if ( beacons.Contains( beaconNotActivePredicate ) )
      {
        beaconHaloGhost.Scale = 2 * GetRadiusForBeacon();
        beaconHaloGhost.Position = camera.Target;
        beaconHaloGhost.Draw( view, projection );
      }

      SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
      spriteBatch.Begin( SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None );

      // draw cursor
      Vector2 position = new Vector2( viewport.Width / 2, viewport.Height / 2 );
      Vector2 origin = new Vector2( cursor.Width / 2, cursor.Height / 2 );
      float scale = .35f * globalScale;
      spriteBatch.Draw( cursor, position, null, Color.White, 0f, origin, scale, SpriteEffects.None, 0f );

      // draw available beacons' labels
      float labelRadius = 80;
      float angleYCoord = labelRadius / 2;
      float angleXCoord = (float)Math.Sqrt( 3 ) * labelRadius / 2;
      Vector2 labelOrigin = new Vector2( availableBeacons[0].Width, availableBeacons[0].Height ) / 2;
      Vector2 labelPosition = Vector2.Zero;

      float alpha = MathHelper.Lerp( .1f, .45f, .5f + .5f * (float)Math.Sin( totalUnpausedTime * 3 ) );
      Color translucent = new Color( Color.White, alpha );

      if ( !beacons[0].Active )
      {
        labelPosition = position + new Vector2( 0, -labelRadius ) * globalScale;
        spriteBatch.Draw( availableBeacons[0], labelPosition, null, translucent,
                          0f, labelOrigin, scale, SpriteEffects.None, 0f );
      }
      if ( !beacons[1].Active )
      {
        labelPosition = position + new Vector2( angleXCoord, angleYCoord ) * globalScale;
        spriteBatch.Draw( availableBeacons[1], labelPosition, null, translucent,
                          0f, labelOrigin, scale, SpriteEffects.None, 0f );
      }
      if ( !beacons[2].Active )
      {
        labelPosition = position + new Vector2( -angleXCoord, angleYCoord ) * globalScale;
        spriteBatch.Draw( availableBeacons[2], labelPosition, null, translucent,
                          0f, labelOrigin, scale, SpriteEffects.None, 0f );
      }

      spriteBatch.End();
    }

    private Vector2 PickGround( int sx, int sy )
    {
      Camera camera = InstancedModelDrawer.Camera;
      Viewport viewport = ZombieCraft.Instance.GraphicsDevice.Viewport;
      Matrix view = camera.ViewMatrix;
      Matrix proj = camera.ProjectionMatrix;
      Vector3 p0 = camera.Position;
      Vector3 p1 = viewport.Unproject( new Vector3( sx, sy, 0 ), proj, view, Matrix.Identity );
      float t = -p0.Y / ( p1.Y - p0.Y );

      Vector2 dest = Vector2.Zero;
      dest.X = p0.X + t * ( p1.X - p0.X );
      dest.Y = p0.Z + t * ( p1.Z - p0.Z );

      dest.X = MathHelper.Clamp( dest.X, AISuperBrain.GridMin.X, AISuperBrain.GridMax.X - .0001f );
      dest.Y = MathHelper.Clamp( dest.Y, AISuperBrain.GridMin.Y, AISuperBrain.GridMax.Y - .0001f );
      return dest;
    }

    private float GetRadiusForBeacon()
    {
      float lookLength = ( camera.Position - camera.Target ).Length();
      return MathHelper.Lerp( minBeaconRadius, maxBeaconRadius,
                              lookLength / ( farZoom - closeZoom ) );
    }

    Predicate<Beacon> beaconActivePredicate = b => b.Active;
    Predicate<Beacon> beaconNotActivePredicate = b => !b.Active;

    #endregion
  }
}
