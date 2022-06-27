// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Datasync.EFCore;
using Microsoft.AspNetCore.Datasync;
using Samples.BlogIt.Database;
using Samples.BlogIt.Models;
using Microsoft.AspNetCore.Mvc;

namespace Samples.BlogIt.Controllers
{
    [AllowAnonymous]
    [Route("tables/author")]
    public class BlogAuthorController : TableController<BlogAuthor>
    {
        public BlogAuthorController(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            Repository = new EntityTableRepository<BlogAuthor>(context);
            AccessControlProvider = new BlogAuthorAccessControlProvider(httpContextAccessor);
        }
    }

    /// <summary>
    /// An access control provider for the blog comments.
    /// 
    /// Blog Comments can be:
    ///     Read or queries by anyone (including anonymous)
    ///     Created by any authenticated user.
    ///     Deletes and Updates allowed by the author.
    /// </summary>
    public class BlogAuthorAccessControlProvider : AccessControlProvider<BlogAuthor>
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public BlogAuthorAccessControlProvider(IHttpContextAccessor httpContextAccessor)
        {
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
        public override Task<bool> IsAuthorizedAsync(TableOperation operation, BlogAuthor entity, CancellationToken token = default)
            => Task.FromResult(operation switch
            {
                TableOperation.Query or TableOperation.Read => true,
                TableOperation.Create => IsAuthenticated,
                TableOperation.Delete or TableOperation.Update => IsAuthenticated && entity.Id == UserId,
                _ => false,
            });
    }
}
