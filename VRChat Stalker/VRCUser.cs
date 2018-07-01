﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRChat_Stalker
{
    public enum UserStatus
    {
        Online,
        Private,
        Offline,
    }

    public class VRCUser
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public int Star { get; set; } = 1;
        public bool IsTracked { get; set; } = true;
        public string ImageUrl { get; set; }
        public string StatusText { get; set; }

        public UserStatus Status
        {
            get
            {
                if (Location == "offline")
                {
                    return UserStatus.Offline;
                }
                else if (Location == "private")
                {
                    return UserStatus.Private;
                }
                else
                {
                    var loc = Location.Split(':');

                    if (loc.Length >= 2 && loc[1] == Id)
                    {
                        return UserStatus.Private;
                    }
                }

                return UserStatus.Online;
            }
        }

        public string WorldId
        {
            get
            {
                if (Location.Contains(':'))
                {
                    return Location.Split(':')[0];
                }

                return string.Empty;
            }
        }

        public string InstanceId
        {
            get
            {
                if (Location.Contains(':'))
                {
                    return Location.Split(':')[1];
                }

                return string.Empty;
            }
        }
    }
}