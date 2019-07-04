﻿using System.Collections.Generic;
using Plato.Internal.Security.Abstractions;

namespace Plato.Discuss.Follow
{
    public class Permissions : IPermissionsProvider<Permission>
    {

        public static readonly Permission FollowTopics =
            new Permission("FollowTopics", "Can follow topics");

        public static readonly Permission FollowNewTopics =
            new Permission("FollowNewTopics", "Automatically follow new topics");

        public static readonly Permission FollowParticipatedTopics =
            new Permission("FollowParticipatedTopics", "Automatically follow topics when posting replies");


        public IEnumerable<Permission> GetPermissions()
        {
            return new[]
            {
                FollowTopics,
                FollowNewTopics,
                FollowParticipatedTopics
            };
        }

        public IEnumerable<DefaultPermissions<Permission>> GetDefaultPermissions()
        {
            return new[]
            {
                new DefaultPermissions<Permission>
                {
                    RoleName = DefaultRoles.Administrator,
                    Permissions = new[]
                    {
                        FollowTopics,
                        FollowNewTopics,
                        FollowParticipatedTopics
                    }
                },
                new DefaultPermissions<Permission>
                {
                    RoleName = DefaultRoles.Member,
                    Permissions = new[]
                    {
                        FollowTopics,
                        FollowNewTopics,
                        FollowParticipatedTopics
                    }
                },
                new DefaultPermissions<Permission>
                {
                    RoleName = DefaultRoles.Staff,
                    Permissions = new[]
                    {
                        FollowTopics,
                        FollowNewTopics,
                        FollowParticipatedTopics
                    }
                }
            };

        }

    }

}
