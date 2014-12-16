﻿using AutoMapper;
using ZBuildLights.Core.Models;
using ZBuildLights.Web.Models.Admin;

namespace ZBuildLights.Web.Mapping
{
    public class ViewModelProfile : Profile
    {
        protected override void Configure()
        {
            CreateMap<Project, AdminProjectViewModel>();
            CreateMap<LightGroup, AdminLightGroupViewModel>();
            CreateMap<Light, AdminLightViewModel>();
        }
    }
}