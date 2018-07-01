﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRChatApi.Classes
{
    public enum UserOptions
    {
        Me,
        Friends,
    }

    public enum SortOptions
    {
        Popularity,
        Created,
        Updated,
        Order,
    }

    public enum ReleaseStatus
    {
        Public,
        Private,
        All,
        Hidden,
    }

    public enum WorldGroups
    {
        Any,
        Active,
        Recent,
        Favorite,
    }
}
