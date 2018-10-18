﻿using Newtonsoft.Json;
using System;

namespace LuckParser.Models.ParseModels
{
    public class RotatedRectangleActor : RectangleActor
    {
        public int Rotation { get; } // initial rotation angle
        public int RadialTranslation { get; } // translation of the triangle center in the direction of the current rotation
        public int SpinAngle { get; } // rotation the rectangle is supposed to go through over the course of its lifespan, 0 for no rotation

        // Rectangles with fixed rotation and no translation
        public RotatedRectangleActor(bool fill, int growing, int width, int height, int rotation, Tuple<int, int> lifespan, string color, Connector connector)
            : this(fill, growing, width, height, rotation, 0, 0, lifespan, color, connector) { }
        

        // Rectangles with a fixed rotation and translation

        public RotatedRectangleActor(bool fill, int growing, int width, int height, int rotation, int translation, Tuple<int, int> lifespan, string color, Connector connector) 
            : this(fill, growing, width, height, rotation, translation, 0, lifespan, color, connector) { }

        // Rectangles rotating over time

        public RotatedRectangleActor(bool fill, int growing, int width, int height, int rotation, int translation, int spinAngle, Tuple<int, int> lifespan, string color, Connector connector) : base(fill, growing, width, height, lifespan, color, connector)
        {
            Rotation = rotation;
            RadialTranslation = translation;
            SpinAngle = spinAngle;
        }

        private class RotatedRectangleSerializable : RectangleSerializable
        {
            public int Rotation { get; set; }
            public int RadialTranslation { get; set; }
            public int SpinAngle { get; set; }
        }

        public override string GetCombatReplayJSON(CombatReplayMap map)
        {
            RotatedRectangleSerializable aux = new RotatedRectangleSerializable
            {
                Type = "RotatedRectangle",
                Width = Width,
                Height = Height,
                Rotation = Rotation,
                RadialTranslation = RadialTranslation,
                SpinAngle = SpinAngle,
                Fill = Filled,
                Color = Color,
                Growing = Growing,
                Start = Lifespan.Item1,
                End = Lifespan.Item2,
                ConnectedTo = ConnectedTo.GetConnectedTo(map)
            };
            return JsonConvert.SerializeObject(aux);
        }
    }
}
