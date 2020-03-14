﻿// Copyright (c) SDV Code Project. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace SdvCode.ViewComponents
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using SdvCode.Data;
    using SdvCode.Models;
    using SdvCode.Services.ProfileServices;
    using SdvCode.ViewModels.Profile;
    using X.PagedList;

    public class FollowingViewComponent : ViewComponent
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IProfileFollowingService followingService;

        public FollowingViewComponent(UserManager<ApplicationUser> userManager, IProfileFollowingService followingService)
        {
            this.userManager = userManager;
            this.followingService = followingService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string username, int page)
        {
            var user = await this.userManager.FindByNameAsync(username);
            var currentUserId = this.userManager.GetUserId(this.HttpContext.User);
            List<FollowingViewModel> allFollowing = await this.followingService.ExtractFollowing(user, currentUserId);
            this.ViewBag.Following = allFollowing.ToPagedList(page, 1);
            this.ViewBag.Username = username;
            return this.View(allFollowing);
        }
    }
}