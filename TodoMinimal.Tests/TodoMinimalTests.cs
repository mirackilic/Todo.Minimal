using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using TodoMinimal.Api;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace TodoMinimal.Tests;

public class TodoMinimalTests
{
    [Fact]
    public async Task GetTodos()
    {
        await using var application = new TodoApplication();

        var client = application.CreateClient();
        var todos = await client.GetFromJsonAsync<List<Todo>>("/todoitems");

        Assert.Empty(todos);
    }

    [Fact]
    public async Task PostTodos()
    {
        await using var application = new TodoApplication();

        var client = application.CreateClient();
        var response = await client.PostAsJsonAsync("/todoitems", new Todo { Title = "I want to do this thing tomorrow" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var todos = await client.GetFromJsonAsync<List<Todo>>("/todoitems");

        var todo = Assert.Single(todos);
        Assert.Equal("I want to do this thing tomorrow", todo.Title);
        Assert.False(todo.IsComplete);
    }
}

class TodoApplication : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        var root = new InMemoryDatabaseRoot();

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<TodoDbContext>));

            services.AddDbContext<TodoDbContext>(options =>
                options.UseInMemoryDatabase("TodoList", root));
        });

        return base.CreateHost(builder);
    }
}