// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.EntityFrameworkCore;
using Samples.BlogIt.Models;

namespace Samples.BlogIt.Database
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// The data set for the BlogAuthor model.
        /// </summary>
        public DbSet<BlogAuthor> BlogAuthors => Set<BlogAuthor>();

        /// <summary>
        /// The data set for the BlogComment model.
        /// </summary>
        public DbSet<BlogComment> BlogComments => Set<BlogComment>();

        /// <summary>
        /// The data set for the BlogPost model.
        /// </summary>
        public DbSet<BlogPost> BlogPosts => Set<BlogPost>();

        /// <summary>
        /// Do any database initialization required.
        /// </summary>
        /// <returns>A task that completes when the database is initialized</returns>
        public async Task InitializeDatabaseAsync()
        {
            await Database.EnsureCreatedAsync().ConfigureAwait(false);
        }
    }
}
