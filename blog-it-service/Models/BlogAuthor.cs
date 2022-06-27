// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.EFCore;

namespace Samples.BlogIt.Models
{
    public class BlogAuthor : EntityTableData
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string PhotoUrl { get; set; } = "";
    }
}
