#region File Description
//-----------------------------------------------------------------------------
// Game.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DebugSample;
using System;
using Utility;
using Microsoft.Xna.Framework.Input;
#endregion

namespace ZombieCraft
{
  public class ZombieCraft : Game
  {
    #region Fields

    GraphicsDeviceManager graphics;
    ScreenManager screenManager;
    public ScreenManager ScreenManager { get { return screenManager; } }

    static ZombieCraft _instance;
    public static ZombieCraft Instance { get { return _instance; } }

    public DebugManager DebugManager { get; private set; }
    public DebugCommandUI CommandLine { get; private set; }
    public FpsCounter FpsCounter { get; private set; }
    public TimeRuler TimeRuler { get; private set; }

    const string markUpdate = "Update Total";
    const string markDraw = "Draw Total";

    #endregion

    #region Initialization


    public ZombieCraft()
    {
      _instance = this;

      Content.RootDirectory = "Content";

      graphics = new GraphicsDeviceManager( this );

      IsFixedTimeStep = false;
      //TargetElapsedTime = TimeSpan.FromSeconds( 1d / 30d );
      graphics.SynchronizeWithVerticalRetrace = true;

#if WINDOWS
      IsMouseVisible = true;
      graphics.PreferredBackBufferWidth = 853;
      graphics.PreferredBackBufferHeight = 480;
#else
      graphics.PreferredBackBufferWidth = 1920;
      graphics.PreferredBackBufferHeight = 1080;
#endif

      // Create the screen manager component.
      screenManager = new ScreenManager( this );

      Components.Add( screenManager );

      //// Activate the first screens.
      screenManager.AddScreen( new BackgroundScreen(), null );
      screenManager.AddScreen( new MainMenuScreen(), null );
      LoadingScreen.Load( screenManager, true, PlayerIndex.One, new GameplayScreen() );

      // Debugging components
      DebugManager = new DebugManager( this );
      Components.Add( DebugManager );

      CommandLine = new DebugCommandUI( this );
      CommandLine.DrawOrder = 100;
      Components.Add( CommandLine );

      FpsCounter = new FpsCounter( this );
      Components.Add( FpsCounter );

      TimeRuler = new TimeRuler( this );
      Components.Add( TimeRuler );
    }

    protected override void LoadContent()
    {
    }

    #endregion

    #region Update and Draw


    protected override void Update( GameTime gameTime )
    {
      TimeRuler.StartFrame();
      TimeRuler.BeginMark( 0, markUpdate, Color.Yellow );

      base.Update( gameTime );
      Pool.CleanupAll();

      TimeRuler.EndMark( 0, markUpdate );
    }

    protected override void Draw( GameTime gameTime )
    {
      graphics.GraphicsDevice.Clear( Color.Black );

      TimeRuler.BeginMark( 1, markDraw, Color.Crimson );
      base.Draw( gameTime );
      TimeRuler.EndMark( 1, markDraw );
    }


    #endregion
  }


  #region Entry Point

  static class Program
  {
    static void Main()
    {
      using ( ZombieCraft game = new ZombieCraft() )
      {
        game.Run();
      }
    }
  }

  #endregion
}
