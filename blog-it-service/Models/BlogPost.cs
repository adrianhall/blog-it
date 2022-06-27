// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.EFCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Samples.BlogIt.Models
{
    public class BlogPost : EntityTableData
    {
        /// <summary>
        /// The ID of the author that created this post.
        /// </summary>
        [Column(TypeName = "varchar(64)")]
        public string? AuthorId { get; set; }

        /// <summary>
        /// The content for the post.
        /// </summary>
        [Column(TypeName = "varchar(1024)")]
        [Required, MinLength(3), MaxLength(1024)]
        public string Content { get; set; } = "";

        /// <summary>
        /// The date/time this record was created.
        /// </summary>
        public DateTimeOffset? CreatedAt { get; set; }

        /// <summary>
        /// The title for the post.
        /// </summary>
        [Column(TypeName = "varchar(120)")]
        [Required, MinLength(3), MaxLength(120)]
        public string Title { get; set; } = "";
    }
}
