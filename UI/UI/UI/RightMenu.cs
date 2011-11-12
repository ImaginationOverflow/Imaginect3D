using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using UI.Common;
using UI.Geometry;
using UI.Kinect.Movement;

namespace UI.UI
{
    public delegate void OnSelectedGeometryHandler(Object state);

    public class RightMenu : DrawableGameComponent
    {
        private bool _menuExtended = false;
        private SpriteBatch _spriteBatch;
        private Texture2D _menuTexture;
        private Rectangle _dest;
        private List<MenuItem> _items;
        private int _maxItems;
        private IEnumerable<GeometryMenuItem> _current;
        public event OnSelectedGeometryHandler OnSelectedGeometry;

        public RightMenu(Game game, Rectangle dest)
            : base(game)
        {
            _dest = dest;
            _maxItems = (_dest.Height - 30) / 85;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _menuTexture = Game.Content.Load<Texture2D>("MenuTexture2");
            _menuSmallTexture = Game.Content.Load<Texture2D>("rightsidesmall");
        }

        public override void Initialize()
        {
            base.Initialize();

            Rectangle r;
            _items = new List<MenuItem>() 
            {
                new GeometryMenuItem(Game,_spriteBatch, r = new Rectangle(_dest.X + 30, _dest.Y + 30, 75, 75), new CubePrimitive(GraphicsDevice)),
                new GeometryMenuItem(Game,_spriteBatch, r = Next(r), new ConePrimitive(GraphicsDevice)),
                new GeometryMenuItem(Game,_spriteBatch, r = Next(r), new CylinderPrimitive(GraphicsDevice)),
                new GeometryMenuItem(Game,_spriteBatch, r = Next(r), new SpherePrimitive(GraphicsDevice)),
            };
            
            foreach (MenuItem item in _items)
            {
                item.Initialize();
                item.OnItemSelected += (s) => OnSelectedGeometry(s);
            }
        }


        private Rectangle Next(Rectangle r)
        {
            return new Rectangle(r.X, r.Y + r.Height + 10, r.Width, r.Height);
        }



        private double _nextChange = 0;
        private int _nrOfSkipped = 0;
        private Texture2D _menuSmallTexture;

        public override void Update(GameTime gameTime)
        {
            _menuExtended = IsMouseOverMenu(KinectMouse.MouseCoordinates);
            if (_menuExtended)
            {
                foreach (MenuItem item in _items)
                    item.Update(gameTime);
            }
            base.Update(gameTime);
        }

        private bool IsMouseOverTheLowerMenuPart(Vector2 mouseCoordinates)
        {
            if (!_menuExtended)
                return false;
            return mouseCoordinates.Y >= (_dest.Y + _dest.Height - 80);
        }

        private bool IsMouseOverMenu(Vector2 mouseCoordenates)
        {
            if (_menuExtended)
                return (mouseCoordenates.X >= _dest.X && mouseCoordenates.Y >= _dest.Y && mouseCoordenates.Y <= _dest.Y + _dest.Height);
            else
            {
                return mouseCoordenates.X >= _dest.X + _dest.Width - (_dest.Width * 0.10) &&
                       mouseCoordenates.Y <= _dest.Y + _dest.Height;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);



            //_spriteBatch.DrawString(Game.Content.Load<SpriteFont>("SpriteFont1"), "Mouse X = " + KinectMouse.MouseCoordinates.X + " Y = " + KinectMouse.MouseCoordinates.Y, new Vector2(25, 25), Color.Red);


            //_spriteBatch.DrawString(Game.Content.Load<SpriteFont>("SpriteFont1"), "Menu X = " + _dest.X + " Y = " + _dest.Y + " - " + (KinectMouse.MouseCoordinates.X >= _dest.X && KinectMouse.MouseCoordinates.Y <= _dest.Y), new Vector2(50, 50), Color.Red);

            if (_menuExtended)
            {

                _spriteBatch.Draw(_menuTexture, _dest, Color.White);

                foreach (MenuItem item in _items)
                {
                    item.Draw(gameTime);
                }

            }
            else
            {
                _spriteBatch.DrawString(Game.Content.Load<SpriteFont>("SpriteFont1"), "Shortened", Vector2.UnitX, Color.Red);
                int width = (int)(_dest.Width * 0.10);
                Rectangle shortenedVersion = new Rectangle(_dest.X + _dest.Width - width, _dest.Y, _dest.Width, _dest.Height);
                _spriteBatch.Draw(_menuSmallTexture, new Vector2(_dest.X + _dest.Width - _menuSmallTexture.Width, _dest.Y), Color.White);

            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}