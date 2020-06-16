﻿// Copyright (c) SDV Code Project. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace SdvCode.Areas.Editor.Services.Post
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.EntityFrameworkCore;
    using SdvCode.Areas.Administration.Models.Enums;
    using SdvCode.Areas.UserNotifications.Services;
    using SdvCode.Data;
    using SdvCode.Hubs;
    using SdvCode.Models.Enums;
    using SdvCode.Services;

    public class EditorPostService : UserValidationService, IEditorPostService
    {
        private readonly ApplicationDbContext db;
        private readonly INotificationService notificationService;
        private readonly IHubContext<NotificationHub> notificationHubContext;
        private readonly List<UserActionsType> actionsForBann = new List<UserActionsType>()
        {
            UserActionsType.LikedPost,
            UserActionsType.LikePost,
            UserActionsType.LikeOwnPost,
            UserActionsType.EditedPost,
            UserActionsType.EditPost,
            UserActionsType.EditOwnPost,
            UserActionsType.CreatePost,
            UserActionsType.UnlikedPost,
            UserActionsType.UnlikePost,
            UserActionsType.UnlikeOwnPost,
        };

        public EditorPostService(
            ApplicationDbContext db,
            INotificationService notificationService,
            IHubContext<NotificationHub> notificationHubContext)
            : base(db)
        {
            this.db = db;
            this.notificationService = notificationService;
            this.notificationHubContext = notificationHubContext;
        }

        public async Task<bool> ApprovePost(string id)
        {
            var post = this.db.Posts.FirstOrDefault(x => x.Id == id);
            if (post != null)
            {
                var targetApprovedEntity = this.db.PendingPosts
                    .FirstOrDefault(x => x.PostId == post.Id && x.IsPending == true);
                if (targetApprovedEntity != null)
                {
                    post.PostStatus = PostStatus.Approved;
                    targetApprovedEntity.IsPending = false;
                    this.db.PendingPosts.Update(targetApprovedEntity);
                    this.db.Posts.Update(post);
                    await this.db.SaveChangesAsync();

                    var specialRoleIds = this.db.Roles
                        .Where(x => x.Name == Roles.Administrator.ToString() ||
                        x.Name == Roles.Editor.ToString())
                        .Select(x => x.Id)
                        .ToList();
                    var specialIds = this.db.UserRoles
                        .Where(x => specialRoleIds.Contains(x.RoleId))
                        .Select(x => x.UserId)
                        .ToList();
                    var followerIds = this.db.FollowUnfollows
                    .Where(x => x.PersonId == post.ApplicationUserId && !specialIds.Contains(x.FollowerId))
                    .Select(x => x.FollowerId)
                    .ToList();

                    foreach (var followerId in followerIds)
                    {
                        var toUser = await this.db.Users.FirstOrDefaultAsync(x => x.Id == followerId);
                        var user = await this.db.Users.FirstOrDefaultAsync(x => x.Id == post.ApplicationUserId);

                        string notificationId =
                            await this.notificationService
                            .AddBlogPostNotification(toUser, user, post.ShortContent, post.Id);

                        var count = await this.notificationService.GetUserNotificationsCount(toUser.UserName);
                        await this.notificationHubContext
                            .Clients
                            .User(toUser.Id)
                            .SendAsync("ReceiveNotification", count, true);

                        var notification = await this.notificationService.GetNotificationById(notificationId);
                        await this.notificationHubContext.Clients.User(toUser.Id)
                            .SendAsync("VisualizeNotification", notification);
                    }

                    return true;
                }

                return false;
            }

            return false;
        }

        public async Task<bool> BannPost(string id)
        {
            var post = this.db.Posts.FirstOrDefault(x => x.Id == id);

            if (post != null)
            {
                var targetApprovedEntity = this.db.BlockedPosts
                    .FirstOrDefault(x => x.PostId == post.Id && x.IsBlocked == false);
                if (targetApprovedEntity != null)
                {
                    post.PostStatus = PostStatus.Banned;
                    targetApprovedEntity.IsBlocked = true;
                    var favorites = this.db.FavouritePosts.Where(x => x.PostId == id);

                    foreach (var favor in favorites)
                    {
                        favor.IsFavourite = false;
                    }

                    var likes = this.db.PostsLikes.Where(x => x.PostId == id && x.IsLiked == true);

                    foreach (var like in likes)
                    {
                        like.IsLiked = false;
                        post.Likes--;
                    }

                    var actions = this.db.UserActions
                        .Where(x => x.PostId == id && this.actionsForBann.Contains(x.Action));

                    this.db.FavouritePosts.UpdateRange(favorites);
                    this.db.PostsLikes.UpdateRange(likes);
                    this.db.UserActions.RemoveRange(actions);
                    this.db.BlockedPosts.Update(targetApprovedEntity);
                    this.db.Posts.Update(post);
                    await this.db.SaveChangesAsync();
                    return true;
                }

                return false;
            }

            return false;
        }

        public async Task<bool> UnbannPost(string id)
        {
            var post = this.db.Posts.FirstOrDefault(x => x.Id == id);

            if (post != null)
            {
                var targetApprovedEntity = this.db.BlockedPosts
                    .FirstOrDefault(x => x.PostId == post.Id && x.IsBlocked == true);
                if (targetApprovedEntity != null)
                {
                    post.PostStatus = PostStatus.Approved;
                    targetApprovedEntity.IsBlocked = false;
                    this.db.BlockedPosts.Update(targetApprovedEntity);
                    this.db.Posts.Update(post);
                    await this.db.SaveChangesAsync();
                    return true;
                }

                return false;
            }

            return false;
        }
    }
}