﻿using System;

namespace LuckParser.Models.ParseModels
{
    public abstract class Actor
    {
        protected enum PositionType { ID, Array };

        public bool Filled { get; }
        public Tuple<int, int> Lifespan { get; }
        public string Color { get; }
        public int Growing { get; }
        protected PositionType Type;
        protected Point3D Position;

        protected Actor(bool fill, int growing, Tuple<int, int> lifespan, string color)
        {
            Lifespan = lifespan;
            Color = color;
            Filled = fill;
            Growing = growing;
            Type = PositionType.ID;
        }
        protected Actor(bool fill, int growing, Tuple<int, int> lifespan, string color, Point3D position)
        {
            Lifespan = lifespan;
            Color = color;
            Filled = fill;
            Growing = growing;
            Position = position;
            Type = PositionType.Array;
        }
        protected Actor(bool fill, int growing, Tuple<int, int> lifespan, string color, Point3D prev, Point3D next, int time)
        {
            Lifespan = lifespan;
            Color = color;
            Filled = fill;
            Growing = growing;
            Type = PositionType.Array;
            if (prev != null && next != null)
            {
                long denom = next.Time - prev.Time;
                if (denom == 0)
                {
                    Position = prev;
                } else
                {
                    float ratio = (float)(time - prev.Time) / denom;
                    Position = new Point3D(prev, next, ratio, time);
                }
            } else
            {
                Position = prev ?? next;
            }
        }
        //
        protected class Serializable<T>
        {

            public bool Fill { get; set; }
            public int Growing { get; set; }
            public string Color { get; set; }
            public string Type { get; set; }
            public long Start { get; set; }
            public long End { get; set; }
            public T Position { get; set; }
        }

        public abstract string GetCombatReplayJSON(CombatReplayMap map, AbstractMasterPlayer master);

    }
}
