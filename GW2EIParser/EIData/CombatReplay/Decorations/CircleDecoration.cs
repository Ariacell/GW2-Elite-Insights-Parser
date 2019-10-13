﻿using GW2EIParser.Parser.ParsedData;

namespace GW2EIParser.EIData
{
    public class CircleDecoration : FormDecoration
    {
        public int Radius { get; }
        public int MinRadius { get; }

        public CircleDecoration(bool fill, int growing, int radius, (int start, int end) lifespan, string color, Connector connector) : base(fill, growing, lifespan, color, connector)
        {
            Radius = radius;
        }

        public CircleDecoration(bool fill, int growing, int radius, (int start, int end) lifespan, string color, Connector connector, int minRadius) : base(fill, growing, lifespan, color, connector)
        {
            Radius = radius;
            MinRadius = minRadius;
        }

        //
        protected class CircleSerializable : FormSerializable
        {
            public int Radius { get; set; }
            public int MinRadius { get; set; }
        }

        public override GenericDecorationSerializable GetCombatReplayJSON(CombatReplayMap map, ParsedLog log)
        {
            var aux = new CircleSerializable
            {
                Type = "Circle",
                Radius = Radius,
                MinRadius = MinRadius,
                Fill = Filled,
                Color = Color,
                Growing = Growing,
                Start = Lifespan.start,
                End = Lifespan.end,
                ConnectedTo = ConnectedTo.GetConnectedTo(map, log)
            };
            return aux;
        }
    }
}
