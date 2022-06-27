// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync;
using Microsoft.EntityFrameworkCore;
using Samples.BlogIt.Database;
using Samples.BlogIt.Models;
using System.Security.Claims;

namespace Samples.BlogIt.Controllers
{
    public abstract class AuthControlProvider<T> : AccessControlProvider<T> where T : ITableData
    {
        protected readonly AppDbContext context;
        protected readonly IHttpContextAccessor accessor;
        
        protected AuthControlProvider(AppDbContext context, IHttpContextAccessor accessor)
        {
            this.context = context;
            this.accessor = accessor;
        }

        /// <summary>
        /// The authenticated user, or null if not authenticated.
        /// </summary>
        protected ClaimsPrincipal? User 
        { 
            get => accessor.HttpContext?.User; 
        }

        /// <summary>
        /// The userId for this user, or null.
        /// </summary>
        protected string? UserId 
        { 
            get => User?.Identity?.Name; 
        }

        /// <summary>
        /// true if the client is authenticated, false otherwise.
        /// </summary>
        protected bool IsAuthenticated 
        { 
            get => User?.Identity?.IsAuthenticated ?? false; 
        }

        /// <summary>
        /// Ensures that an author record exists in the database.
        /// </summary>
        /// <param name="token">A cancellation token.</param>
        protected async Task EnsureAuthorCreatedAsync(CancellationToken token = default)
        {
            var authorEntity = await context.BlogAuthors.SingleOrDefaultAsync(m => m.Id == UserId, token).ConfigureAwait(false);
            if (authorEntity == null)
            {
                BlogAuthor newAuthor = new() { Id = UserId, UpdatedAt = DateTimeOffset.UtcNow, Version = Guid.NewGuid().ToByteArray() };
                context.BlogAuthors.Add(newAuthor);
                await context.SaveChangesAsync(token).ConfigureAwait(false);
            }
        }
    }
}
