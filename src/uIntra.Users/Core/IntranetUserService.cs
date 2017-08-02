﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Hosting;
using uIntra.Core;
using uIntra.Core.Caching;
using uIntra.Core.Extentions;
using uIntra.Core.TypeProviders;
using uIntra.Core.User;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace uIntra.Users
{
    public class IntranetUserService : IIntranetUserService<IntranetUser>, ISetupOnStartup
    {
        protected virtual string MemberTypeAlias => "Member";
        protected virtual string UmbracoUserIdPropertyAlias => "relatedUser";

        protected virtual string IntranetUsersCacheKey => "IntranetUsersCache";
        protected virtual string CurrentUserCacheKey => "CurrentUserCache";

        private readonly IMemberService _memberService;
        private readonly UmbracoContext _umbracoContext;
        private readonly UmbracoHelper _umbracoHelper;
        private readonly IRoleService _roleService;
        private readonly IIntranetRoleTypeProvider _intranetRoleTypeProvider;
        private readonly ICacheService _cacheService;

        public IntranetUserService(
            IMemberService memberService,
            UmbracoContext umbracoContext,
            UmbracoHelper umbracoHelper,
            IRoleService roleService,
            IIntranetRoleTypeProvider intranetRoleTypeProvider,
            ICacheService cacheService)
        {
            _memberService = memberService;
            _umbracoContext = umbracoContext;
            _umbracoHelper = umbracoHelper;
            _roleService = roleService;
            _intranetRoleTypeProvider = intranetRoleTypeProvider;
            _cacheService = cacheService;
        }

        public virtual IntranetUser Get(int umbracoId)
        {
            var member = GetAll().SingleOrDefault(el => el.UmbracoId == umbracoId);
            return member;
        }

        public virtual IntranetUser Get(Guid id)
        {
            var member = GetAll().SingleOrDefault(el => el.Id == id);
            return member;
        }

        public virtual IntranetUser Get(IHaveCreator model)
        {
            IIntranetUser member;

            if (model.UmbracoCreatorId.HasValue)
            {
                member = Get(model.UmbracoCreatorId.Value);
            }
            else
            {
                member = Get(model.CreatorId);
            }
            return (IntranetUser)member;
        }

        public virtual IEnumerable<IntranetUser> GetMany(IEnumerable<Guid> ids)
        {
            return ids.Distinct().Join(GetAll(),
               id => id,
               user => user.Id,
               (id, user) => user);
        }

        public virtual IEnumerable<IntranetUser> GetMany(IEnumerable<int> ids)
        {
            return ids.Distinct().Join(GetAll(),
                 id => id,
                 user => user.UmbracoId.GetValueOrDefault(),
                 (id, user) => user);
        }

        public virtual IEnumerable<IntranetUser> GetAll()
        {
            var users = _cacheService.GetOrSet(IntranetUsersCacheKey, GetAllFromSql, CacheHelper.GetMidnightUtcDateTimeOffset()).ToList();
            return users;
        }

        public virtual IntranetUser GetCurrentUser()
        {
            var currentUser = _cacheService.GetOrSet(CurrentUserCacheKey, GetCurrentUserFromSql, CacheHelper.GetMidnightUtcDateTimeOffset());
            return currentUser;
        }

        public virtual IEnumerable<IntranetUser> GetByRole(int role)
        {
            var users = GetAll().Where(el => el.Role.Priority == role);
            return users;
        }

        public virtual void Save(IntranetUserDTO user)
        {
            var member = _memberService.GetByKey(user.Id);
            member.SetValue("firstName", user.FirstName);
            member.SetValue("lastName", user.LastName);

            if (user.NewMedia.HasValue)
            {
                member.SetValue("photo", user.NewMedia.Value);
            }

            if (user.DeleteMedia)
            {
                member.SetValue("photo", null);
            }

            _memberService.Save(member);

            UpdateCache(user.Id);
        }

        protected virtual IntranetUser GetFromSql(Guid id)
        {
            var member = _memberService.GetByKey(id);
            return Map(member);
        }

        protected virtual IEnumerable<IntranetUser> GetAllFromSql()
        {
            var members = _memberService.GetAllMembers().Select(Map).ToList();
            return members;
        }

        protected virtual IntranetUser GetCurrentUserFromSql()
        {
            var userName = "";
            if (HostingEnvironment.IsHosted) //TODO: WTF IS THIS
            {
                var httpContext = _umbracoContext.HttpContext;
                if (httpContext.User?.Identity != null && httpContext.User.Identity.IsAuthenticated)
                {
                    userName = httpContext.User.Identity.Name;
                }
            }
            if (string.IsNullOrEmpty(userName))
            {
                var currentPrincipal = Thread.CurrentPrincipal;
                if (currentPrincipal?.Identity != null)
                {
                    userName = currentPrincipal.Identity.Name;
                }
            }
            var user = GetByName(userName);
            return user;
        }

        protected virtual IntranetUser Map(IMember member)
        {
            var user = new IntranetUser
            {
                Id = member.Key,
                UmbracoId = member.GetValueOrDefault<int?>(UmbracoUserIdPropertyAlias),
                Email = member.Email,
                FirstName = member.GetValueOrDefault<string>("firstName"),
                LastName = member.GetValueOrDefault<string>("lastName"),
                Role = GetMemberRole(member)
            };

            string userPhoto = null;
            var userPhotoId = member.GetValueOrDefault<int?>("photo");

            if (userPhotoId.HasValue)
            {
                userPhoto = _umbracoHelper.TypedMedia(userPhotoId.Value)?.Url;
            }

            user.Photo = GetUserPhotoOrDefaultAvatar(userPhoto);

            return user;
        }

        protected virtual IRole GetMemberRole(IMember member)
        {
            var roles = _memberService.GetAllRoles(member.Id).ToList();
            return _roleService.GetActualRole(roles);
        }

        protected virtual string GetGroupNameFromRole(int role)
        {
            var roleMode = _intranetRoleTypeProvider.Get(role);
            return roleMode.Name;
        }

        protected virtual string GetUserPhotoOrDefaultAvatar(string userImage)
        {
            return !string.IsNullOrEmpty(userImage) ? userImage : string.Empty;
        }

        protected virtual IntranetUser GetByName(string name)
        {
            var user = _memberService.GetByUsername(name);

            if (user == null)
            {
                return null;
            }

            return Map(user);
        }

        protected virtual void UpdateCache(Guid userId)
        {
            var updatedUser = GetFromSql(userId);

            var currentUser = GetCurrentUser();
            if (currentUser.Id == userId)
            {
                _cacheService.Set(CurrentUserCacheKey, updatedUser, CacheHelper.GetMidnightUtcDateTimeOffset());
            }

            var allCachedUsers = GetAll().ToList();
            var oldCachedUser = allCachedUsers.Find(el => el.Id == userId);
            allCachedUsers.Remove(oldCachedUser);
            allCachedUsers.Add(updatedUser);

            _cacheService.Set(IntranetUsersCacheKey, allCachedUsers, CacheHelper.GetMidnightUtcDateTimeOffset());
        }

        public void Setup()
        {
            MemberService.Created += MemberService_Created;
            MemberService.Saved += MemberService_Saved;
            MemberService.Deleted += MemberService_Deleted;
        }

        private void MemberService_Created(IMemberService sender, NewEventArgs<IMember> e)
        {
            var user = GetFromSql(e.Entity.Key);
            var allCachedUsers = GetAll().ToList();
            allCachedUsers.Add(user);

            _cacheService.Set(IntranetUsersCacheKey, allCachedUsers, CacheHelper.GetMidnightUtcDateTimeOffset());
        }

        private void MemberService_Saved(IMemberService sender, SaveEventArgs<IMember> e)
        {
            foreach (var member in e.SavedEntities)
            {
                UpdateCache(member.Key);
            }
        }

        private void MemberService_Deleted(IMemberService sender, DeleteEventArgs<IMember> e)
        {
            var currentUser = GetCurrentUser();
            var deletingUserIds = e.DeletedEntities.Select(el => el.Key).ToList();
            if (deletingUserIds.Contains(currentUser.Id))
            {
                _cacheService.Clear(CurrentUserCacheKey);
            }

            var allCachedUsers = GetAll().ToList();
            allCachedUsers.RemoveAll(el => deletingUserIds.Contains(el.Id));

            _cacheService.Set(IntranetUsersCacheKey, allCachedUsers, CacheHelper.GetMidnightUtcDateTimeOffset());
        }
    }
}
