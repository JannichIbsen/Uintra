﻿namespace Uintra.Core.User
{
    public interface IRole
    {
        string Name { get; set; }
        int Priority { get; set; }
    }
}