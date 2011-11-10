using System;
using System.Collections.Generic;
using System.Linq;
using UI.Geometry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Research.Kinect.Nui;

namespace UI
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Imaginect3D : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont spriteFont;

        GeometricPrimitive _primitive;
        bool _movedLeft, _movedRight;

        Runtime nui;

        public Imaginect3D()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            _primitive = new CubePrimitive(GraphicsDevice);

            nui = Runtime.Kinects[0];

            try {
                nui.Initialize(RuntimeOptions.UseSkeletalTracking);
            }
            catch(InvalidOperationException) {

            }

            nui.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(nui_SkeletonFrameReady);

            base.Initialize();
        }

        private Vector _lastPosition;
        private bool hasData = false;
        private bool _foundSkeleton;

        TimeSpan _current;
        TimeSpan _old;

        void nui_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            SkeletonFrame frame = e.SkeletonFrame;
            SkeletonData data = frame.Skeletons.Where(sk => sk.TrackingState == SkeletonTrackingState.Tracked).FirstOrDefault();
            bool entered = false;
            
            if (data != null && data.TrackingState == SkeletonTrackingState.Tracked)
            {
                _foundSkeleton = true;
                entered = true;

                Joint rightHand = data.Joints[JointID.HandRight];
                Vector currentPosition = rightHand.Position;
                if(_current - _old > TimeSpan.FromMilliseconds(100)){
                if (hasData)
                {
                    float dif = currentPosition.X - _lastPosition.X;
   
                    if(dif > 0.1) 
                    {
                        _movedRight = true;
                        _movedLeft = false;
                    }
                    else if(dif < -0.1) 
                    {
                        _movedRight = false;
                        _movedLeft = true;
                    }
                    else 
                    {
                        _movedLeft = _movedRight = false;
                    }

                }

                _old = _current;
                _lastPosition = rightHand.Position;
                hasData = true; 
            }
            }

            if(!entered)
                _foundSkeleton = false;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            spriteFont = Content.Load<SpriteFont>("SpriteFont1");
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            
            _current = gameTime.TotalGameTime;
            

            if (_old == null)
                _old = _current; 
            
            base.Update(gameTime);
        }
        Matrix world;
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();

            Vector3 translation;

            if (_foundSkeleton) spriteBatch.DrawString(spriteFont, "Found Skeleton", new Vector2(48, 48), Color.White);
                        
            if (_movedLeft)
                world = Matrix.CreateTranslation(Vector3.Left);
            else if (_movedRight)
                world = Matrix.CreateTranslation(Vector3.Right);

            Matrix view = Matrix.CreateLookAt(new Vector3(0, 0, 5f), Vector3.Zero, Vector3.Up);
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(1, GraphicsDevice.Viewport.AspectRatio, 1, 10);

            _primitive.Draw(world, view, projection, Color.Red);
            
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
