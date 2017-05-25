﻿using System;
using uCommunity.Navigation.Core.Models;

namespace uCommunity.Navigation.Core.Exceptions
{
    public class MyLinksDuplicatedException : ApplicationException
    {
        public MyLinksDuplicatedException(MyLinkDTO model)
            : base($"Can not add myLink with content {model.ContentId} for {model.UserId} and querString {model.QueryString}, becase it's already existed")
        {
            
        }
    }
}
