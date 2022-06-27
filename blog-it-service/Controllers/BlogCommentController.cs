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
    [Route("tables/comment")]
    public class BlogCommentController : TableController<BlogComment>
    {
        public BlogCommentController(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            Repository = new EntityTableRepository<BlogComment>(context);
            AccessControlProvider = new BlogCommentAccessControlProvider(context, httpContextAccessor);
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
    public class BlogCommentAccessControlProvider : AuthControlProvider<BlogComment>
    {
        public BlogCommentAccessControlProvider(AppDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        /// <summary>
        /// Determines if the client is allowed to perform the <see cref="TableOperation"/> on
        /// the provided entity.
        /// </summary>
        /// <param name="operation">The <see cref="TableOperation"/> being requested.</param>
        /// <param name="entity">The entity being used.</param>
        /// <param name="token">A cancellation token</param>
        /// <returns>True if the operation is authorized.</returns>
        public override Task<bool> IsAuthorizedAsync(TableOperation operation, BlogComment entity, CancellationToken token = default)
            => Task.FromResult(operation switch
            {
                TableOperation.Query or TableOperation.Read =>true,
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
        public override async Task PreCommitHookAsync(TableOperation operation, BlogComment entity, CancellationToken token = default)
        {
            // PostId has to exist at this point.  Throw a BadRequestException if the post does not exist.
            var postEntity = await context.BlogPosts.SingleOrDefaultAsync(m => m.Id == entity.PostId, token).ConfigureAwait(false);
            if (postEntity == null)
            {
                throw new BadRequestException("PostId does not exist", null);
            }

            await EnsureAuthorCreatedAsync(token).ConfigureAwait(false);

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
