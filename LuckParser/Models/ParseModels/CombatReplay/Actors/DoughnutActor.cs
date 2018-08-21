﻿using System;

namespace LuckParser.Models.ParseModels
{
    public class DoughnutActor : Actor
    {
        private readonly int _outerRadius;
        private readonly int _innerRadius;

        public DoughnutActor(int growing, int innerRadius, int outerRadius, Tuple<int, int> lifespan, string color) : base(true, growing, lifespan, color, new MobileActor())
        {
            _innerRadius = innerRadius;
            _outerRadius = outerRadius;
        }

        public DoughnutActor(int growing, int innerRadius, int outerRadius, Tuple<int, int> lifespan, string color, Point3D position) : base(true, growing, lifespan, color, new ImmobileActor(position))
        {
            _innerRadius = innerRadius;
            _outerRadius = outerRadius;
        }

        public int GetInnerRadius()
        {
            return _innerRadius;
        }

        public int GetOuterRadius()
        {
            return _outerRadius;
        }

    }
}
