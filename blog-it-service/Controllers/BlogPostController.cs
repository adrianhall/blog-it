// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Datasync;
using Microsoft.AspNetCore.Datasync.EFCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Samples.BlogIt.Database;
using Samples.BlogIt.Models;

namespace Samples.BlogIt.Controllers
{
    [AllowAnonymous]
    [Route("tables/post")]
    public class BlogPostController : TableController<BlogPost>
    {
        public BlogPostController(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            Repository = new EntityTableRepository<BlogPost>(context);
            AccessControlProvider = new BlogPostAccessControlProvider(context, httpContextAccessor);
        }
    }

    /// <summary>
    /// An access control provider for the blog comments.
    /// 
    /// Blog Comments can be:
    ///     Read or queries by anyone (including anonymous)
    ///     Created by any authenticated user.
    ///     Deleted by the author.
    ///     Updates are explicitly denied.
    /// </summary>
    public class BlogPostAccessControlProvider : AccessControlProvider<BlogPost>
    {
        private readonly AppDbContext context;
        private readonly IHttpContextAccessor httpContextAccessor;

        public BlogPostAccessControlProvider(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            this.context = context;
            this.httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// True if the user is authenticated; false otherwise.
        /// </summary>
        public bool IsAuthenticated { get => string.IsNullOrEmpty(UserId); }

        /// <summary>
        /// The userId of the connecting user, or null if anonymous.
        /// </summary>
        public string? UserId { get => httpContextAccessor.HttpContext?.User?.Identity?.Name; }

        /// <summary>
        /// Determines if the client is allowed to perform the <see cref="TableOperation"/> on
        /// the provided entity.
        /// </summary>
        /// <param name="operation">The <see cref="TableOperation"/> being requested.</param>
        /// <param name="entity">The entity being used.</param>
        /// <param name="token">A cancellation token</param>
        /// <returns>True if the operation is authorized.</returns>
        public override Task<bool> IsAuthorizedAsync(TableOperation operation, BlogPost entity, CancellationToken token = default)
            => Task.FromResult(operation switch
            {
                TableOperation.Query or TableOperation.Read => true,
                TableOperation.Create => IsAuthenticated,
                TableOperation.Delete => IsAuthenticated && entity.AuthorId == UserId,
                _ => false,
            });

        /// <summary>
        /// Updates the entity immediately prior to write operations with the data store to
        /// support the chosen access control rules.
        /// </summary>
        /// <param name="operation">The <see cref="TableOperation"/> being requested.</param>
        /// <param name="entity">The entity being used.</param>
        /// <param name="token">A cancellation token</param>
        public override async Task PreCommitHookAsync(TableOperation operation, BlogPost entity, CancellationToken token = default)
        {
            // Ensure that the current author is in the table.
            var authorEntity = await context.BlogPosts.SingleOrDefaultAsync(m => m.Id == UserId, token).ConfigureAwait(false);
            if (authorEntity == null)
            {
                BlogAuthor newAuthor = new() { Id = UserId, UpdatedAt = DateTimeOffset.UtcNow, Version = Guid.NewGuid().ToByteArray() };
                await context.BlogAuthors.AddAsync(newAuthor, token).ConfigureAwait(false);
            }

            // Store the current userId in the entity as the author.
            if (operation == TableOperation.Create)
            {
                entity.AuthorId = UserId;
                entity.CreatedAt = DateTimeOffset.UtcNow;
            }

            await base.PreCommitHookAsync(operation, entity, token);
        }
    }
}
