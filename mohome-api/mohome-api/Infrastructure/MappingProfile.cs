using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Models;
using mohome_api.ViewModels;

namespace mohome_api.Infrastructure
{
    public class MappingProfile : AutoMapper.Profile
    {
        public MappingProfile()
        { 
            CreateMap<List<PhotoAlbum>, List<PhotoAlbumModel>>();
        }
    }
}
