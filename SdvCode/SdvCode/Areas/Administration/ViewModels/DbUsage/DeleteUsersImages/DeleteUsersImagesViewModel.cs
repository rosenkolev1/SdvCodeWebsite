﻿// Copyright (c) SDV Code Project. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace SdvCode.Areas.Administration.ViewModels.DbUsage.DeleteUsersImages
{
    using System.Collections.Generic;

    public class DeleteUsersImagesViewModel
    {
        public DeleteImagesByUsernameInputModel DeleteUserImages { get; set; }

        public ICollection<string> Usernames { get; set; } = new HashSet<string>();
    }
}