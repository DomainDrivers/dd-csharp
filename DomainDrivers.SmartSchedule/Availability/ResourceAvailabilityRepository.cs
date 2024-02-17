using System.Data;
using Dapper;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Availability;

public class ResourceAvailabilityRepository
{
    private readonly IDbConnection _dbConnection;

    public ResourceAvailabilityRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task SaveNew(ResourceAvailability resourceAvailability)
    {
        await SaveNew(new List<ResourceAvailability>() { resourceAvailability });
    }

    public async Task SaveNew(ResourceGroupedAvailability groupedAvailability)
    {
        await SaveNew(groupedAvailability.Availabilities);
    }

    private async Task SaveNew(IList<ResourceAvailability> availabilities)
    {
        var rows = availabilities.Select(x => ResourceAvailabilityRow.Map(x));
        const string sql =
            $"""
             INSERT INTO availabilities
                (id, resource_id, resource_parent_id, from_date, to_date, taken_by, disabled, version)
             VALUES (
                @{nameof(ResourceAvailabilityRow.id)},
                @{nameof(ResourceAvailabilityRow.resource_id)},
                @{nameof(ResourceAvailabilityRow.resource_parent_id)},
                @{nameof(ResourceAvailabilityRow.from_date)},
                @{nameof(ResourceAvailabilityRow.to_date)},
                @{nameof(ResourceAvailabilityRow.taken_by)},
                @{nameof(ResourceAvailabilityRow.disabled)},
                @{nameof(ResourceAvailabilityRow.version)}
                )
             """;
        await _dbConnection.ExecuteAsync(sql, rows);
    }

    public async Task<IList<ResourceAvailability>> LoadAllWithinSlot(ResourceAvailabilityId resourceId,
        TimeSlot segment)
    {
        var param = new
        {
            ResourceId = resourceId.Id,
            FromDate = segment.From,
            ToDate = segment.To
        };
        const string sql =
            $"""
             select * from availabilities
                where resource_id = @{nameof(param.ResourceId)}
                    and from_date >= @{nameof(param.FromDate)}
                    and to_date <= @{nameof(param.ToDate)}
             """;
        var rows = await _dbConnection.QueryAsync<ResourceAvailabilityRow>(sql, param);
        return rows.Select(x => x.Map()).ToList();
    }

    public async Task<IList<ResourceAvailability>> LoadAllByParentIdWithinSlot(ResourceAvailabilityId parentId,
        TimeSlot segment)
    {
        var param = new
        {
            ResourceParentId = parentId.Id,
            FromDate = segment.From,
            ToDate = segment.To
        };
        const string sql =
            $"""
             select * from availabilities
             where resource_parent_id = @{nameof(param.ResourceParentId)}
                and from_date >= @{nameof(param.FromDate)}
                and to_date <= @{nameof(param.ToDate)}
             """;
        var rows = await _dbConnection.QueryAsync<ResourceAvailabilityRow>(sql, param);
        return rows.Select(x => x.Map()).ToList();
    }

    public async Task<bool> SaveCheckingVersion(ResourceAvailability resourceAvailability)
    {
        return await SaveCheckingVersion(new List<ResourceAvailability>() { resourceAvailability });
    }

    public async Task<bool> SaveCheckingVersion(ResourceGroupedAvailability groupedAvailability)
    {
        return await SaveCheckingVersion(groupedAvailability.Availabilities);
    }

    public async Task<bool> SaveCheckingVersion(IList<ResourceAvailability> resourceAvailabilities)
    {
        var rows = resourceAvailabilities.Select(x => ResourceAvailabilityRow.Map(x));
        const string sql =
            $"""
             UPDATE availabilities
             SET
                taken_by = @{nameof(ResourceAvailabilityRow.taken_by)},
                disabled = @{nameof(ResourceAvailabilityRow.disabled)},
                version = @{nameof(ResourceAvailabilityRow.version)} + 1
             WHERE
                id = @{nameof(ResourceAvailabilityRow.id)}
                AND version = @{nameof(ResourceAvailabilityRow.version)}
             """;
        var update = await _dbConnection.ExecuteAsync(sql, rows);
        return update == rows.Count();
    }

    public async Task<ResourceAvailability> LoadById(ResourceAvailabilityId availabilityId)
    {
        const string sql =
            $"""
             select * from availabilities
             where id = @{nameof(availabilityId.Id)}
             """;
        var row = await _dbConnection.QuerySingleAsync<ResourceAvailabilityRow>(sql, new { Id = availabilityId.Id });
        return row.Map();
    }

    public async Task<IList<ResourceAvailability>> LoadAvailabilitiesOfRandomResourceWithin(
        ISet<ResourceAvailabilityId> resourceIds, TimeSlot normalized)
    {
        var param = new
        {
            Ids = resourceIds.Select(x => x.Id),
            FromDate = normalized.From,
            ToDate = normalized.To
        };
        const string sql =
            $"""
             WITH AvailableResources AS (
                SELECT resource_id FROM availabilities
                WHERE
                    resource_id = ANY(@{nameof(param.Ids)})
                    AND taken_by IS NULL
                    AND from_date >= @{nameof(param.FromDate)}
                    AND to_date <= @{nameof(param.ToDate)}
                    GROUP BY resource_id
             ),
             RandomResource AS (
                SELECT resource_id FROM AvailableResources
                ORDER BY RANDOM()
                LIMIT 1
             )
             SELECT a.* FROM availabilities a
             JOIN RandomResource r ON a.resource_id = r.resource_id
             """;
        var rows = await _dbConnection.QueryAsync<ResourceAvailabilityRow>(sql, param);
        return rows.Select(x => x.Map()).ToList();
    }

    private record ResourceAvailabilityRow(
        Guid id,
        Guid resource_id,
        Guid? resource_parent_id,
        long version,
        DateTime from_date,
        DateTime to_date,
        Guid? taken_by,
        bool disabled)
    {
        public static ResourceAvailabilityRow Map(ResourceAvailability resourceAvailability)
        {
            return new ResourceAvailabilityRow(resourceAvailability.Id.Id!.Value,
                resourceAvailability.ResourceId.Id!.Value,
                resourceAvailability.ResourceParentId.Id,
                resourceAvailability.Version,
                resourceAvailability.Segment.From, resourceAvailability.Segment.To,
                resourceAvailability.Blockade.TakenBy.OwnerId,
                resourceAvailability.Blockade.Disabled);
        }

        public ResourceAvailability Map()
        {
            return new ResourceAvailability(new ResourceAvailabilityId(id),
                new ResourceAvailabilityId(resource_id),
                new ResourceAvailabilityId(resource_parent_id),
                new TimeSlot(from_date, to_date),
                new Blockade(new Owner(taken_by), disabled),
                (int)version);
        }
    }
}