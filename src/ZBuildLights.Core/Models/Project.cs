﻿using System.Collections.Generic;
using System.Linq;
using BuildLightControl;

namespace ZBuildLights.Core.Models
{
    public class Project
    {
        private readonly List<Light> _lights = new List<Light>();
        public string Name { get; set; }
        public StatusMode StatusMode { get; set; }

        public Light[] Lights
        {
            get { return _lights.OrderBy(x => x.Color.DisplayOrder).ToArray(); }
        }

        public Project AddLight(Light light)
        {
            _lights.Add(light);
            return this;
        }
    }
}