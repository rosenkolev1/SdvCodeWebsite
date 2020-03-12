﻿// Copyright (c) SDV Code Project. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace SdvCode.Areas.Administration.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Identity;
    using SdvCode.Constraints;
    using SdvCode.Data;

    public class UsersPenaltiesService : IUsersPenaltiesService
    {
        private readonly ApplicationDbContext db;
        private readonly RoleManager<IdentityRole> roleManager;

        public UsersPenaltiesService(
            ApplicationDbContext db,
            RoleManager<IdentityRole> roleManager)
        {
            this.db = db;
            this.roleManager = roleManager;
        }

        public async Task<bool> BlockUser(string username)
        {
            var user = this.db.Users.FirstOrDefault(x => x.UserName == username);
            var adminRoleId = this.roleManager.FindByNameAsync(GlobalConstants.AdministratorRole).Result.Id;

            if (this.db.UserRoles.Any(x => x.RoleId == adminRoleId && x.UserId == user.Id))
            {
                return false;
            }

            if (user != null && user.IsBlocked == false)
            {
                user.IsBlocked = true;
                this.db.Users.Update(user);
                await this.db.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<bool> UnblockUser(string username)
        {
            var user = this.db.Users.FirstOrDefault(x => x.UserName == username);

            if (user != null && user.IsBlocked == true)
            {
                user.IsBlocked = false;
                this.db.Users.Update(user);
                await this.db.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public ICollection<string> GetAllBlockedUsers()
        {
            return this.db.Users.Where(u => u.IsBlocked == true).Select(x => x.UserName).ToList();
        }

        public ICollection<string> GetAllNotBlockedUsers()
        {
            var adminRoleId = this.roleManager.FindByNameAsync(GlobalConstants.AdministratorRole).Result.Id;
            var usersAdminsIds = this.db.UserRoles.Where(x => x.RoleId == adminRoleId).Select(x => x.UserId).ToList();
            return this.db.Users.Where(u => u.IsBlocked == false && !usersAdminsIds.Contains(u.Id)).Select(x => x.UserName).ToList();
        }

        public async Task<int> BlockAllUsers()
        {
            var role = this.roleManager.FindByNameAsync(GlobalConstants.AdministratorRole).Result;
            int count = 0;

            if (role != null)
            {
                var noneAdminsIds = this.db.UserRoles.Where(x => x.RoleId != role.Id).Select(x => x.UserId).ToList();
                var users = this.db.Users.Where(x => noneAdminsIds.Contains(x.Id) && x.IsBlocked == false).ToList();
                count = users.Count();

                foreach (var user in users)
                {
                    user.IsBlocked = true;
                }

                this.db.Users.UpdateRange(users);
                await this.db.SaveChangesAsync();
                return count;
            }

            return count;
        }

        public async Task<int> UnblockAllUsers()
        {
            var users = this.db.Users.Where(x => x.IsBlocked == true).ToList();
            int count = users.Count();

            foreach (var user in users)
            {
                user.IsBlocked = false;
            }

            this.db.Users.UpdateRange(users);
            await this.db.SaveChangesAsync();
            return count;
        }
    }
}