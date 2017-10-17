﻿using AutoMapper;

namespace uIntra.Core.Extensions
{
    public static class AutoMapperExtentions
    {
        public static T Map<T>(this object obj) where T: class
        {
            return Mapper.Map<T>(obj);
        }
    }
}