using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UI.Geometry;
using UI.Kinect.Movement;

namespace UI.UI
{
    public class GeometryMenuItem : MenuItem
    {
       
        private readonly GeometricPrimitive _icon;
        private readonly Matrix _view = Matrix.CreateLookAt(new Vector3(0, 0, 5f), Vector3.Zero, Vector3.Up);
        private Matrix _projection;
        private int _mouseSelectionToken = -1;

        public GeometryMenuItem( Game game, SpriteBatch spriteBatch, Rectangle position, GeometricPrimitive icon)
            : base(game,spriteBatch, position)
        {

            _icon = icon;
        }


        protected override void DoInitialize()
        {
            _projection = Matrix.CreatePerspectiveFieldOfView(1, GraphicsDevice.Viewport.AspectRatio, 1, 10);
            
        }

        protected override void UpdateMenuItemContent(GameTime gameTime)
        {
            if (Position.Contains((int)KinectMouse.MouseCoordinates.X, (int)KinectMouse.MouseCoordinates.Y))
            {
                if (_mouseSelectionToken == -1)
                    _mouseSelectionToken = KinectMouseSelection.Instance.GetMouseToken();

                KinectMouseSelection.Instance.FeedSelection(_mouseSelectionToken);
            }
            else
            {
                _mouseSelectionToken = -1;
                
            }

            if (KinectMouseSelection.Instance.IsSelected())
            {
                InvokeEvent(_icon);
            }
        }




        protected override void DrawMenuItemContent(GameTime gameTime, SpriteBatch spriteBatch)
        {
            var vect = GraphicsDevice.Viewport.Unproject(new Vector3(Position.X, Position.Y , 1), _projection, _view,
                                              Matrix.Identity);
            _icon.Draw(Matrix.Multiply(Matrix.CreateTranslation(vect),Matrix.CreateTranslation(0.6f,-0.9f,0)),_view,_projection,Color.Yellow);
        }
    }
}
