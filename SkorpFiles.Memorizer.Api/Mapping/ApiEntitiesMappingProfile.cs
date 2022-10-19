﻿using AutoMapper;
using SkorpFiles.Memorizer.Api.ApiModels.ApiEntities;

namespace SkorpFiles.Memorizer.Api.Mapping
{
    public class ApiEntitiesMappingProfile:Profile
    {
        public ApiEntitiesMappingProfile()
        {
            CreateMap<SkorpFiles.Memorizer.Api.Models.Questionnaire, Questionnaire>()
                .ForMember(dest => dest.Availability, opts => opts.MapFrom(src => src.Availability.ToString()));
        }
    }
}
