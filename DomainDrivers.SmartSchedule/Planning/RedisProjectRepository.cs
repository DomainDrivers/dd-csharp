using System.Text.Json;
using StackExchange.Redis;

namespace DomainDrivers.SmartSchedule.Planning;

public class RedisProjectRepository : IProjectRepository
{
    private readonly IDatabase _database;

    public RedisProjectRepository(IConnectionMultiplexer redisConnection)
    {
        _database = redisConnection.GetDatabase();
    }

    public async Task<Project> GetById(ProjectId projectId)
    {
        var redisValue = await _database.HashGetAsync("projects", projectId.Id.ToString());

        if (redisValue.IsNull)
        {
            return null!;
        }

        return JsonSerializer.Deserialize<Project>(redisValue.ToString())!;
    }
    
    public async Task<Project> Save(Project project)
    {
        await _database.HashSetAsync("projects", project.Id.Id.ToString(), JsonSerializer.Serialize(project));
        return project;
    }

    public async Task<IList<Project>> FindAllByIdIn(ISet<ProjectId> projectIds)
    {
        var ids = projectIds.Select(p => (RedisValue)p.Id.ToString()).ToArray();
        var redisValues = await _database.HashGetAsync("projects", ids);
        return redisValues.Select(redisValue => JsonSerializer.Deserialize<Project>(redisValue.ToString())!).ToList();
    }

    public async Task<IList<Project>> FindAll()
    {
        var redisValues = await _database.HashValuesAsync("projects");
        return redisValues.Select(redisValue => JsonSerializer.Deserialize<Project>(redisValue.ToString())!).ToList();
    }
}