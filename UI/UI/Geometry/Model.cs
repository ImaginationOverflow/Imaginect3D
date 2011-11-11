using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace UI.Geometry
{

    internal class GeometricData
    {
        public Matrix Matrix { get; set; }
        public Color Color { get; set; }
    }
    public class Model : DrawableGameComponent
    {
        private readonly Imaginect3D _game;
    

        IDictionary<GeometricPrimitive, List<GeometricData>> _models = new Dictionary<GeometricPrimitive, List<GeometricData>>();

        public Model(Imaginect3D game)
            : base(game)
        {
            _game = game;
        }

        public void AddPrimitives(params GeometricPrimitive[] primitives)
        {
            foreach (var geometricPrimitive in primitives)
            {
                _models.Add(geometricPrimitive, new List<GeometricData>());
            }
        }

        public void PrimitiveFreeze(GeometricPrimitive primitive, Matrix location, Color color)
        {
            if (primitive == null)
                return;
            _models[primitive].Add(new GeometricData{Color = color, Matrix = location});
        }

        public override void Draw(GameTime gameTime)
        {
            foreach (var model in _models)
            {
                foreach (var data in model.Value)
                {
                    model.Key.Draw(data.Matrix,_game.View, _game.Projection,data.Color);
                }
            }
            base.Draw(gameTime);
        }

        public GeometricPrimitive GetSingletonPrimitiveInstance<T>() where T : GeometricPrimitive
        {
            foreach (var geometricPrimitive in _models.Keys)
            {
                if (geometricPrimitive is T)
                    return geometricPrimitive;
            }
            throw new InvalidOperationException();
        }

        public void ClearAll()
        {
            foreach (var model in _models)
            {
                model.Value.Clear();
            }
        }
    }
}
