using BlogSharp.Data;
using BlogSharp.Entities;
using BlogSharp.Services;
using Microsoft.EntityFrameworkCore;

namespace BlogSharp.Consumers;

public class BlogConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IQueueable _queue;

    public BlogConsumer(IServiceProvider serviceProvider, IQueueable queue)
    {
        _serviceProvider = serviceProvider;
        _queue = queue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var blog = await _queue.DequeueAsync<Blog>("BlogQueue");
            if (blog != null)
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<BlogDbContext>();

                dbContext.Blogs.Add(blog);
                await dbContext.SaveChangesAsync(stoppingToken);
            }
        }
    }
}
