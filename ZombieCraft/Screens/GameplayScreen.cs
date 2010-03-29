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
    int nextEntityToUpdate = 0;
    int entityBegin;
    int entityEnd;
    Entity[] entities;
    City city;
    int scrollValue;

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

      int entityCount = nEntities;
      float gridSize = 600;
      maxEntitiesPerFrame = entityCount / 4;

      // create the camera
      float aspect = ScreenManager.Game.GraphicsDevice.Viewport.AspectRatio;
      InstancedModelDrawer.Camera = new Camera( MathHelper.PiOver4, aspect, 1f, 5000f,
                                       new Vector3( 0, gridSize, gridSize ), Vector3.Zero );

      // create the zombies
      entities = new Entity[entityCount];
      Entity.Entities = entities;
      InstancedModel model = content.Load<InstancedModel>( "Models/zombie" );
      float scale = .95f * (float)Math.Sqrt( 2 ) / 2;
      for ( int i = 0; i < entityCount; ++i )
      {
        entities[i] = new Entity( i );
        entities[i].Transform = InstancedModelDrawer.GetInstanceRef( model );
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
      }

      AISuperBrain.InitializeGrid( gridSize, gridSize, (int)( gridSize / 25 ), (int)( gridSize / 25 ) );
      AISuperBrain.PopulateGrid( entities );

      // create the city
      city = new City();

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
        /*/
        int entityCount = entities.Length;
        for ( int i = 0; i < entityCount; ++i )
        {
          AISuperBrain.Update( ref entities[i] );
        }
        /*/
        int entityCount = entities.Length;
        int entitiesPerFrame = Math.Min( maxEntitiesPerFrame, entityCount );

        if ( entityBegin == 0 && entityEnd == 0 )
          entityEnd += entitiesPerFrame;

        if ( entityBegin > entityEnd )
        {
          for ( int i = 0; i < entityEnd; ++i )
            AISuperBrain.Update( ref entities[i] );
          for ( int i = entityEnd; i < entityBegin; ++i )
            ; // update position
          for ( int i = entityBegin; i < entityCount; ++i )
            AISuperBrain.Update( ref entities[i] );
        }
        else
        {
          for ( int i = 0; i < entityBegin; ++i )
            ;// update position
          for ( int i = entityBegin; i < entityEnd; ++i )
            AISuperBrain.Update( ref entities[i] );
          for ( int i = entityEnd; i < entityCount; ++i )
            ;// update position
        }

        entityBegin = ( entityBegin + entitiesPerFrame ) % entityCount;
        entityEnd = ( entityEnd + entitiesPerFrame ) % entityCount;
        /**/
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
        float zoom = input.CurrentGamePadStates[0].Triggers.Left - 
                     input.CurrentGamePadStates[0].Triggers.Right;

        float lastScrollValue = scrollValue;
        scrollValue = Mouse.GetState().ScrollWheelValue;
        if ( lastScrollValue != scrollValue )
          zoom = .25f * ( lastScrollValue - scrollValue );

        InstancedModelDrawer.Camera.Position += new Vector3( 0, 1, 1 ) * zoom;
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
      AISuperBrain.DrawGrid( camera.ViewMatrix, camera.ProjectionMatrix );

      //city.Draw( InstancedModelDrawer.Camera.ViewMatrix, InstancedModelDrawer.Camera.ProjectionMatrix );

      // If the game is transitioning on or off, fade it out to black.
      if ( TransitionPosition > 0 )
        ScreenManager.FadeBackBufferToBlack( 255 - TransitionAlpha );
    }

    #endregion
  }
}
