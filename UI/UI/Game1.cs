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
using UI.Kinect.Movement;
using UI.Kinect.Movement.EventsArgs;
using UI.Kinect.Movement.Gestures;
using Model = Microsoft.Xna.Framework.Graphics.Model;

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

        private MovementTracker _mTracker;

        GeometricPrimitive _primitive;
        bool _movedLeft, _movedRight, _movedUp, _movedDown, _movedBackward, _movedForward;

        Runtime nui;

        SpeechRecognition _speechRecognition = new SpeechRecognition();
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
            View = Matrix.CreateLookAt(new Vector3(0, 0, 5f), Vector3.Zero, Vector3.Up);
            Projection = Matrix.CreatePerspectiveFieldOfView(1, GraphicsDevice.Viewport.AspectRatio, 1, 6);

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
            //_primitive = new ConePrimitive(GraphicsDevice);
            Components.Add(new FrameRateCounter(this));
            _speechRecognition.AddCommand(
                new CommandSpeechRecognition("select cone", () =>
                                                         {
                                                             speech = "cone";
                                                             //model.PrimitiveFreeze(_primitive, this.world, Color.Blue);
                                                             _primitive = model.GetSingletonPrimitiveInstance<ConePrimitive>();

                                                         }),
                new CommandSpeechRecognition("select cube", () =>
                                                    {
                                                        //model.PrimitiveFreeze(_primitive, this.world, Color.Yellow);
                                                        speech = "cube";
                                                        _primitive = model.GetSingletonPrimitiveInstance<CubePrimitive>();
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
            _speechRecognition.InicializeSpeechRecognize();
            _speechRecognition.Start();

            nui = Runtime.Kinects[0];

            try
            {
                nui.Initialize(RuntimeOptions.UseSkeletalTracking);
            }
            catch (InvalidOperationException)
            {

            }

            this.Exiting += (s, arg) => { nui.Uninitialize(); _speechRecognition.Stop(); };

            _mTracker = new MovementTracker(nui);
            _mTracker.AddMovementHandler(MovementType.Any, 0.01f, OnSkeletonMovement, JointID.Head);

            //_mTracker.AddMovementHandler(MovementType.Any, 0.01f, OnHandsChanged, JointID.HandRight);
            //_mTracker.AddMovementHandler(MovementType.Any, 0.07f, OnRotateGesture, JointID.HandRight, JointID.HandLeft);
            _rotateGesture = new RotateGesture(this, _mTracker);
            _translationGesture = new TranslationGesture(this, _mTracker);
            _scaleGesture = new ScaleGesture(this, _mTracker);
            _scaleGesture.Register();
            Components.Add(_translationGesture);
            //Components.Add(_rotateGesture);
            // Components.Add(_scaleGesture);
            //_rotateGesture.Register();
            _translationGesture.Register();


            _mTracker.OnSkeletonOnViewChange += UpdateSkeletonState;
            base.Initialize();
        }

        private void OnSkeletonMovement(object state, MovementHandlerEventArgs args)
        {
            double angleRadians = 0d;
            double x = args.KinectCoordinates.X;
            double y = args.KinectCoordinates.Y;
            angleRadians = Math.Atan(x/y);  
            _speechRecognition.UpdateMicBeamAngle(angleRadians);
        }

        private bool _startYRightRotation, _startYLeftRotation;

        private void OnRotateGesture(object state, MovementHandlerEventArgs args)
        {
            if (args.Joint == JointID.HandRight)
                _startYRightRotation = args.KinectCoordinates.Z < args.Skeleton.Joints[JointID.ShoulderRight].Position.Z - 0.3 &&
                    args.KinectCoordinates.Z < args.Skeleton.Joints[JointID.HandLeft].Position.Z - 0.1;
            else
                _startYLeftRotation = args.KinectCoordinates.Z < args.Skeleton.Joints[JointID.ShoulderLeft].Position.Z - 0.3 &&
                    args.KinectCoordinates.Z < args.Skeleton.Joints[JointID.HandRight].Position.Z - 0.1;

            _args = args;
        }

        private void UpdateSkeletonState(object state, SkeletonOnViewEventArgs args)
        {
            _foundSkeleton = args.State == SkeletonOnViewType.Entered;
        }

        private MovementHandlerEventArgs _args = new MovementHandlerEventArgs() { Movements = new MovementType[0] };
        private Vector rightHandVector, leftHandVector;


        private void OnHandsChanged(object state, MovementHandlerEventArgs args)
        {
            if (args.Joint == JointID.HandRight)
                rightHandVector = args.KinectCoordinates;
        }

        private bool _foundSkeleton;

        TimeSpan _current;
        TimeSpan _old;

        private Vector3 ConvertRealWorldPoint(Vector position)
        {
            var returnVector = new Vector3 { X = position.X * 10, Y = position.Y * 10, Z = position.Z };
            return returnVector;
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
        private float yRotation = 0.1f;

        private RotateGesture _rotateGesture;
        private TranslationGesture _translationGesture;
        private ScaleGesture _scaleGesture;

        private string speech = "";

        public Matrix Projection { get; private set; }
        public Matrix View { get; private set; }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();

            Vector3 translation;

            if (_foundSkeleton)
            {
                spriteBatch.DrawString(spriteFont, "Found Skeleton.", new Vector2(48, 48), Color.White);
                //spriteBatch.DrawString(spriteFont, "LEngt : " + Extensions.ArraysToString(_args.Movements), new Vector2(48, 70), Color.White);
                spriteBatch.DrawString(spriteFont, "Speach: " + speech, new Vector2(48, 70), Color.White);
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

            spriteBatch.End();
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
