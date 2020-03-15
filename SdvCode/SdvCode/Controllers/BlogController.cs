﻿// Copyright (c) SDV Code Project. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace SdvCode.Controllers
{
    using System.Threading.Tasks;
    using CloudinaryDotNet.Actions;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using SdvCode.Constraints;
    using SdvCode.Models.User;
    using SdvCode.Services.Blog;
    using SdvCode.ViewModels.Blog.InputModels;
    using SdvCode.ViewModels.Blog.ViewModels;
    using Twilio.Rest.Api.V2010.Account.Usage;

    public class BlogController : Controller
    {
        private readonly IBlogService blogService;
        private readonly UserManager<ApplicationUser> userManager;

        public BlogController(IBlogService blogService, UserManager<ApplicationUser> userManager)
        {
            this.blogService = blogService;
            this.userManager = userManager;
        }

        public IActionResult Index()
        {
            var model = new BlogViewModel
            {
                Posts = this.blogService.ExtraxtAllPosts(),
            };

            return this.View(model);
        }

        [Authorize]
        public IActionResult CreatePost()
        {
            var model = new CreatePostIndexModel
            {
                Categories = this.blogService.ExtractAllCategoryNames(),
                Tags = this.blogService.ExtractAllTagNames(),
                PostInputModel = new CreatePostInputModel(),
            };

            return this.View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreatePost(CreatePostIndexModel model)
        {
            if (this.ModelState.IsValid)
            {
                var user = await this.userManager.GetUserAsync(this.HttpContext.User);
                bool isAdded = await this.blogService.CreatePost(model, user);

                if (isAdded)
                {
                    this.TempData["Success"] = SuccessMessages.SuccessfullyCreatedPost;
                }
            }
            else
            {
                this.TempData["Error"] = ErrorMessages.InvalidInputModel;
            }

            return this.RedirectToAction("Index", "Blog");
        }
    }
}