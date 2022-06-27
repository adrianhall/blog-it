// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.EFCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Samples.BlogIt.Models
{
    public class BlogComment : EntityTableData
    {
        /// <summary>
        /// The ID of the author that created this comment.
        /// </summary>
        [Column(TypeName = "varchar(64)")]
        public string? AuthorId { get; set; }

        /// <summary>
        /// The content for the comment.
        /// </summary>
        [Column(TypeName = "varchar(255)")]
        [Required, MaxLength(250)]
        public string? Content { get; set; }

        /// <summary>
        /// The date/time this record was created.
        /// </summary>
        public DateTimeOffset? CreatedAt { get; set; }

        [Column(TypeName = "varchar(64)")]
        [Required, MaxLength(64)]
        public string? PostId { get; set; }
    }
}
