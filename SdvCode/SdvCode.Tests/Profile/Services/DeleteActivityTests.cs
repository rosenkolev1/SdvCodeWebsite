﻿namespace SdvCode.Tests.Profile.Services
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using SdvCode.Data;
    using SdvCode.Models.User;
    using SdvCode.Services.Profile;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Xunit;

    public class DeleteActivityTests
    {
        [Fact]
        public async Task TestDeleteActivity()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), UserName = "pesho" };
            var activity = new UserAction { Id = 1, ApplicationUser = user };
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;

            //using (var db = new ApplicationDbContext(options))
            //{
            //    IProfileService profileService = new ProfileService(db);
            //    db.Users.Add(user);
            //    db.UserActions.Add(activity);
            //    await db.SaveChangesAsync();
            //    await profileService.DeleteActivity(user);

            //    Assert.Equal(0, db.UserActions.Count());
            //}
        }
    }
}