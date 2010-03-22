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
    Entity[] entities;

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

      float aspect = ScreenManager.Game.GraphicsDevice.Viewport.AspectRatio;
      ModelDrawer.Camera = new Camera( MathHelper.PiOver4, aspect, 1f, 5000f, 
                                       new Vector3( 0, 600, 600 ), Vector3.Zero );

      // create the zombie dinosaurs
      entities = new Entity[nEntities];
      Entity.Entities = entities;
      InstancedModel model = content.Load<InstancedModel>( "Models/zombie" );
      for ( int i = 0; i < nEntities; ++i )
      {
        entities[i] = new Entity( i );
        entities[i].Transform = ModelDrawer.GetInstanceRef( model );
        entities[i].Transform.Position = new Vector3( 600 * ( (float)random.NextDouble() - .5f ),
                                                      0,
                                                      600 * ( (float)random.NextDouble() - .5f ) );
        entities[i].Transform.Axis  = ModelDrawer.Camera.Up;
        entities[i].Transform.Angle = (float)random.NextDouble() * MathHelper.TwoPi;
        entities[i].Transform.Scale = 1f;
      }

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
        for ( int i = 0; i < nEntities; ++i )
        {
          /**/
          AISuperBrain.Update( ref entities[i] );
          /*/
          entities[i].Transform.Position = entities[i].Transform.Position;
          entities[i].Transform.Angle += .01f;
          /**/
        }
      }
    }

    private void DoNothing()
    {
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
        ScreenManager.AddScreen( new PauseMenuScreen(), ControllingPlayer );
      }
      else
      {
        float zoom = input.CurrentGamePadStates[0].Triggers.Left - 
                     input.CurrentGamePadStates[0].Triggers.Right;
        ModelDrawer.Camera.Position += new Vector3( 0, 1, 1 ) * zoom;
      }
    }


    public override void Draw( GameTime gameTime )
    {
      ScreenManager.GraphicsDevice.Clear( ClearOptions.Target,
                                          Color.CornflowerBlue, 0, 0 );

      ZombieCraft.Instance.TimeRuler.BeginMark( 1, "Draw Models", Color.White );
      ModelDrawer.Draw();
      ZombieCraft.Instance.TimeRuler.EndMark( 1, "Draw Models" );

      // If the game is transitioning on or off, fade it out to black.
      if ( TransitionPosition > 0 )
        ScreenManager.FadeBackBufferToBlack( 255 - TransitionAlpha );
    }


    #endregion
  }
}
