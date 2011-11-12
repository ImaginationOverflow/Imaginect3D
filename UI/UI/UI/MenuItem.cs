using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UI.Kinect.Movement;

namespace UI.UI
{


    public abstract class MenuItem : DrawableGameComponent
    {



        public event OnSelectedGeometryHandler OnItemSelected = new OnSelectedGeometryHandler((_) => {});


        public Rectangle Position { get; private set; }
        private SpriteBatch _spriteBatch;

        private static volatile Texture2D _boxTexture;
        

        public MenuItem(Game game,SpriteBatch spriteBatch, Rectangle position) : base(game)
        {
            Position = position;
            _spriteBatch = spriteBatch;
        }

        protected void InvokeEvent(Object state)
        {
            OnItemSelected.Invoke(state);
        }

        public override void Initialize()
        {
            base.Initialize();
            if (_boxTexture == null)
                _boxTexture = Game.Content.Load<Texture2D>("OptionTexture");

           
            DoInitialize();
        }

        protected abstract void DoInitialize();

        public sealed override void Update(GameTime gameTime)
        {
            
            if (Position.Contains((int)KinectMouse.MouseCoordinates.X, (int)KinectMouse.MouseCoordinates.Y))
            {
                UpdateMenuItemContent(gameTime);    
            }
            
        }

        protected abstract void UpdateMenuItemContent(GameTime gameTime);

        public sealed override void Draw(GameTime gameTime)
        {
            //_spriteBatch.Draw(_boxTexture,Position,Color.White);
            
            DrawMenuItemContent(gameTime, _spriteBatch);
           
        }

        protected abstract void DrawMenuItemContent(GameTime gameTime, SpriteBatch spriteBatch);
    }
}
