using System;
using System.Threading.Tasks;

using Cosmos.Abstracts.Tests.Models;

using FluentAssertions;

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;

using Xunit;
using Xunit.Abstractions;

namespace Cosmos.Abstracts.Tests;

public class RoleRepositoryTest : DatabaseTestBase
{
    public RoleRepositoryTest(ITestOutputHelper output, DatabaseFixture databaseFixture)
        : base(output, databaseFixture)
    {
    }

    [Fact]
    public async Task CreateRole()
    {
        var role = new Role
        {
            Id = Guid.NewGuid().ToString(),
            Name = "CreateRole",
            NormalizedName = "createrole"
        };

        var roleRepo = Services.GetRequiredService<ICosmosRepository<Role>>();
        roleRepo.Should().NotBeNull();

        var result = await roleRepo.CreateAsync(role);
        result.Should().NotBeNull();
        result.Id.Should().Be(role.Id);
    }

    [Fact]
    public async Task SaveRole()
    {
        var role = new Role
        {
            Id = Guid.NewGuid().ToString(),
            Name = "SaveRole",
            NormalizedName = "saverole"
        };

        var roleRepo = Services.GetRequiredService<ICosmosRepository<Role>>();
        roleRepo.Should().NotBeNull();

        var result = await roleRepo.SaveAsync(role);
        result.Should().NotBeNull();
        result.Id.Should().Be(role.Id);
    }

    [Fact]
    public async Task CreateUpdateRole()
    {
        var role = new Role
        {
            Id = Guid.NewGuid().ToString(),
            Name = "CreateRole",
            NormalizedName = "createrole"
        };

        var roleRepo = Services.GetRequiredService<ICosmosRepository<Role>>();
        roleRepo.Should().NotBeNull();

        var createResult = await roleRepo.CreateAsync(role);
        createResult.Should().NotBeNull();
        createResult.Id.Should().Be(role.Id);

        createResult.Name = "CreateUpdateRole";
        createResult.NormalizedName = "createupdaterole";

        var updateResult = await roleRepo.UpdateAsync(createResult);
        updateResult.Should().NotBeNull();
        updateResult.Id.Should().Be(role.Id);
    }

    [Fact]
    public async Task CreateReadRole()
    {
        var role = new Role
        {
            Id = Guid.NewGuid().ToString(),
            Name = "CreateReadRole",
            NormalizedName = "createreadrole"
        };

        var roleRepo = Services.GetRequiredService<ICosmosRepository<Role>>();
        roleRepo.Should().NotBeNull();

        var createResult = await roleRepo.CreateAsync(role);
        createResult.Should().NotBeNull();
        createResult.Id.Should().Be(role.Id);

        var readResult = await roleRepo.FindAsync(role.Id);
        readResult.Should().NotBeNull();
        readResult.Id.Should().Be(role.Id);
    }

    [Fact]
    public async Task CreateDeleteRole()
    {
        var role = new Role
        {
            Id = Guid.NewGuid().ToString(),
            Name = "CreateDeleteRole",
            NormalizedName = "createdeleterole"
        };

        var roleRepo = Services.GetRequiredService<ICosmosRepository<Role>>();
        roleRepo.Should().NotBeNull();

        var createResult = await roleRepo.CreateAsync(role);
        createResult.Should().NotBeNull();
        createResult.Id.Should().Be(role.Id);

        await roleRepo.DeleteAsync(role.Id);

        var findResult = await roleRepo.FindAsync(role.Id);
        findResult.Should().BeNull();

        //Func<Task> findAsync = async () => await roleRepo.FindAsync(role.Id);
        //findAsync.Should().Throw<CosmosException>();
    }

    [Fact]
    public async Task FindAllStartsWith()
    {
        var role = new Role
        {
            Id = Guid.NewGuid().ToString(),
            Name = "CreateRole",
            NormalizedName = "createrole"
        };

        var roleRepo = Services.GetRequiredService<ICosmosRepository<Role>>();
        roleRepo.Should().NotBeNull();

        var createResult = await roleRepo.CreateAsync(role);
        createResult.Should().NotBeNull();
        createResult.Id.Should().Be(role.Id);

        var results = await roleRepo.FindAllAsync(r => r.Name.StartsWith("Create"));
        results.Should().NotBeNull();
        results.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task FindAllQueryDefinition()
    {
        var role = new Role
        {
            Id = Guid.NewGuid().ToString(),
            Name = "CreateRole",
            NormalizedName = "createrole"
        };

        var roleRepo = Services.GetRequiredService<ICosmosRepository<Role>>();
        roleRepo.Should().NotBeNull();

        var createResult = await roleRepo.CreateAsync(role);
        createResult.Should().NotBeNull();
        createResult.Id.Should().Be(role.Id);

        var query = new QueryDefinition("SELECT * FROM Role f WHERE f.name = @name")
            .WithParameter("@name", "CreateRole");

        var results = await roleRepo.FindAllAsync(query);
        results.Should().NotBeNull();
        results.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task FindOneStartsWith()
    {
        var role = new Role
        {
            Id = Guid.NewGuid().ToString(),
            Name = "CreateRole",
            NormalizedName = "createrole"
        };

        var roleRepo = Services.GetRequiredService<ICosmosRepository<Role>>();
        roleRepo.Should().NotBeNull();

        var createResult = await roleRepo.CreateAsync(role);
        createResult.Should().NotBeNull();
        createResult.Id.Should().Be(role.Id);

        var findResult = await roleRepo.FindOneAsync(r => r.Name.StartsWith("Create"));
        findResult.Should().NotBeNull();
    }

    [Fact]
    public async Task FindOneQueryDefinition()
    {
        var role = new Role
        {
            Id = Guid.NewGuid().ToString(),
            Name = "CreateRole",
            NormalizedName = "createrole"
        };

        var roleRepo = Services.GetRequiredService<ICosmosRepository<Role>>();
        roleRepo.Should().NotBeNull();

        var createResult = await roleRepo.CreateAsync(role);
        createResult.Should().NotBeNull();
        createResult.Id.Should().Be(role.Id);

        var query = new QueryDefinition("SELECT * FROM Role f WHERE f.name = @name")
            .WithParameter("@name", "CreateRole");

        var result = await roleRepo.FindOneAsync(query);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task FindAllEmpty()
    {
        var roleRepo = Services.GetRequiredService<ICosmosRepository<Role>>();
        roleRepo.Should().NotBeNull();

        var results = await roleRepo.FindAllAsync(r => r.Name == "blah" + DateTime.Now.Ticks);
        results.Should().NotBeNull();
        results.Count.Should().Be(0);
    }

}
