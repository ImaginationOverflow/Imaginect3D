using System;
using System.Collections.Generic;
using System.Linq;
using SpeechRecognitionDemo;
using UI.Common;
using UI.Geometry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Research.Kinect.Nui;
using UI.Kinect.Debug;
using UI.Kinect.Movement;
using UI.Kinect.Movement.EventsArgs;
using UI.Kinect.Movement.Gestures;
using UI.UI;
using Model = Microsoft.Xna.Framework.Graphics.Model;
using Mouse = Microsoft.Xna.Framework.Input.Mouse;

namespace UI
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Imaginect3D : Microsoft.Xna.Framework.Game
    {
        
        SpriteFont spriteFont;

        private MovementTracker _mTracker;

        GeometricPrimitive _primitive;
       
        Runtime nui;


        private bool _foundSkeleton;
        Matrix world;
        private RotateGesture _rotateGesture;
        private TranslationGesture _translationGesture;
        private ScaleGesture _scaleGesture;

        private string speech = "";

        public Matrix Projection { get; private set; }
        public Matrix View { get; private set; }


        SpeechRecognition _speechRecognition = new SpeechRecognition();
        private GraphicsDeviceManager graphics;

        public Imaginect3D()
        {
            graphics = new GraphicsDeviceManager(this);
           
            //graphics.PreferredBackBufferHeight = 1024;
            //graphics.PreferredBackBufferWidth = 768;
            //graphics.ToggleFullScreen();

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
            SpriteBatchSingleton.Instance = new SpriteBatch(GraphicsDevice);
            View = Matrix.CreateLookAt(new Vector3(0, 0, 5f), Vector3.Zero, Vector3.Up);
            Projection = Matrix.CreatePerspectiveFieldOfView(1, GraphicsDevice.Viewport.AspectRatio, 1, 10);

            Geometry.Model model = new Geometry.Model(this);
            Components.Add(model);

            var conePrimitive = new ConePrimitive(GraphicsDevice);
            var cubePrimitive = new CubePrimitive(GraphicsDevice);
            var cylinderPrimitive = new CylinderPrimitive(GraphicsDevice);
            var spherePrimitive = new SpherePrimitive(GraphicsDevice);
            var torusPrimitive = new TorusPrimitive(GraphicsDevice);

            model.AddPrimitives
                (
                    conePrimitive,
                    cubePrimitive,
                    cylinderPrimitive,
                    spherePrimitive,
                    torusPrimitive
                );
            

            Components.Add(new FrameRateCounter(this));
            _speechRecognition.AddCommand(
                new CommandSpeechRecognition("select cone", () =>
                                                         {
                                                             speech = "cone";
                                                             _primitive = model.GetSingletonPrimitiveInstance<ConePrimitive>();

                                                         }),
                new CommandSpeechRecognition("select cube", () =>
                                                    {
                                                       speech = "cube";
                                                       _primitive = model.GetSingletonPrimitiveInstance<CubePrimitive>();
                                                    }),
                new CommandSpeechRecognition("select cylinder",() =>
                                                                   {
                                                                       speech = "cylinder";
                                                                       _primitive =model.GetSingletonPrimitiveInstance<CylinderPrimitive>();
                                                                   }),
                new CommandSpeechRecognition("select sphere", () =>
                {
                    speech = "sphere";
                    _primitive = model.GetSingletonPrimitiveInstance<SpherePrimitive>();
                }),
                new CommandSpeechRecognition("delete all", () =>
                                                    {
                                                        speech = "delete all"; model.ClearAll();
                                                    }),
                new CommandSpeechRecognition("add", () =>
                                                        {
                                                         speech = "Add";
                                                        model.PrimitiveFreeze(_primitive, world, Color.Yellow);
                                                        _primitive = null;
                                                    })
            );
            //
            _speechRecognition.InicializeSpeechRecognize();
            //_speechRecognition.Start();
            //*/

            if (Runtime.Kinects.Count != 0)
            {
                //use first Kinect
                nui = Runtime.Kinects[0];         //Initialize to do skeletal tracking
                try
                {
                    nui.Initialize(RuntimeOptions.UseSkeletalTracking);
                }
                catch (InvalidOperationException)
                {

                }
            }

            Exiting += (s, arg) =>
                           {
                               nui.Uninitialize(); 
                               _speechRecognition.Stop();
                           };

            _mTracker = new MovementTracker(nui);

           
            _rotateGesture = new RotateGesture(this, _mTracker);
            _translationGesture = new TranslationGesture(this, _mTracker);
            _scaleGesture = new ScaleGesture(this, _mTracker);
            _scaleGesture.Register();
            
            Components.Add(_translationGesture);
            //Components.Add(_rotateGesture);
            // Components.Add(_scaleGesture);
            //_rotateGesture.Register();
            _translationGesture.Register();




            int width = 120, height = 400;

            RightMenu rightMenu = new RightMenu(this, new Rectangle(graphics.PreferredBackBufferWidth - width, (graphics.PreferredBackBufferHeight - height) / 2, width, height));
            Components.Add(rightMenu);
            SkeletonTracker tracker = new SkeletonTracker(this, _mTracker);
            Components.Add(tracker);

            KinectMouse mouse = new KinectMouse(this, _mTracker, JointID.HandRight);
            KinectMouseSelection mouseSelection = new KinectMouseSelection(this);
            Components.Add(mouse);
            Components.Add(mouseSelection);

            _mTracker.OnSkeletonOnViewChange += (s, args) =>
            {
                _foundSkeleton = args.State == SkeletonOnViewType.Entered;
                mouse.Enable();
            };

          
            
            
            /*/
            var token = mouseSelection.GetMouseToken();
            _mTracker.AddMovementHandler(MovementType.Any,1f,(obj,args)=>
                                                                 {
                                                                     if (KinectMouse.MouseCoordinates.Y > 220)
                                                                     {
                                                                         mouseSelection.FeedSelection(token);
                                                                     }
                                                                     else
                                                                     {
                                                                         token = mouseSelection.GetMouseToken();
                                                                     }

                                                                     if (mouseSelection.IsSelected())
                                                                     {
                                                                         token = mouseSelection.GetMouseToken();
                                                                     }
                                                                 },
                                                                 JointID.HandRight);
            //*/
            rightMenu.OnSelectedGeometry += (state) =>
                                                {
                                                    if (state is CubePrimitive)
                                                        _primitive =
                                                            model.GetSingletonPrimitiveInstance<CubePrimitive>();
                                                    else if (state is CylinderPrimitive)
                                                        _primitive =
                                                            model.GetSingletonPrimitiveInstance<CylinderPrimitive>();
                                                    else if (state is ConePrimitive)
                                                        _primitive =
                                                            model.GetSingletonPrimitiveInstance<ConePrimitive>();
                                                    else if (state is SpherePrimitive)
                                                        _primitive =
                                                            model.GetSingletonPrimitiveInstance<SpherePrimitive>();
                                                    else
                                                    {
                                                        return;
                                                    }
                                                    mouse.Disable();
                                                };
            base.Initialize();
        }




       

        

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
           

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
            if (Keyboard.GetState(PlayerIndex.One).GetPressedKeys().Contains(Keys.Escape))
                this.Exit();
            if (Keyboard.GetState(PlayerIndex.One).GetPressedKeys().Contains(Keys.LeftAlt) && Keyboard.GetState(PlayerIndex.One).GetPressedKeys().Contains(Keys.Enter))
                this.graphics.ToggleFullScreen();


            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
           SpriteBatchSingleton.Instance.Begin(SpriteSortMode.Immediate,BlendState.AlphaBlend);

          
            if (_foundSkeleton)
            {
                SpriteBatchSingleton.Instance.DrawString(spriteFont, "Found Skeleton.", new Vector2(48, 48), Color.White);
                SpriteBatchSingleton.Instance.DrawString(spriteFont, "Speach: " + speech, new Vector2(48, 70), Color.White);
                SpriteBatchSingleton.Instance.DrawString(spriteFont, "Y:"+KinectMouse.MouseCoordinates.Y,new Vector2(48,92),Color.White );
            }


            
            //world = Matrix.CreateTranslation(ConvertRealWorldPoint(rightHandVector));
            //  if (_startYRightRotation)
            //     yRotation += (float )gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f;
            // else if(_startYLeftRotation)
            //     yRotation -= (float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f;


            //world = Matrix.Multiply(Matrix.CreateRotationY(yRotation), Matrix.CreateTranslation(ConvertRealWorldPoint(rightHandVector)));
            //world = _rotateGesture.GetRotationMatrix();
            //world = Matrix.Multiply(world, _translationGesture.GetTranslationMatrix());
            //world = _scaleGesture.GetScaleMatrix();    
            world = _translationGesture.GetTranslationMatrix();

            //Matrix view = Matrix.CreateLookAt(new Vector3(0, 0, 5f), Vector3.Zero, Vector3.Up);
            //Matrix projection = Matrix.CreatePerspectiveFieldOfView(1, GraphicsDevice.Viewport.AspectRatio, 1, 6);

            if (_primitive != null)
                _primitive.Draw(world, View, Projection, Color.Red);

            SpriteBatchSingleton.Instance.End();
            base.Draw(gameTime);
        }
    }

    public static class Extensions
    {
        public static String ArraysToString(this MovementType[] arr)
        {
            return arr.Aggregate("", (current, movementType) => current + (movementType + "; "));
        }
    }
}
