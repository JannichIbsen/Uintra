﻿namespace Uintra.Navigation
{
    public interface ILeftSideNavigationModelBuilder
    {
        MenuModel GetMenu();
        UserListLinkModel GetUserListLink();
    }
}
