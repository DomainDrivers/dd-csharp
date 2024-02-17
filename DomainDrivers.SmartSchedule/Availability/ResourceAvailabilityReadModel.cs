using System.Data;
using Dapper;
using DomainDrivers.SmartSchedule.Shared;

namespace DomainDrivers.SmartSchedule.Availability;

public class ResourceAvailabilityReadModel
{
    private const string CalendarQuery =
        $"""
         WITH AvailabilityWithLag AS (
            SELECT
                resource_id,
                taken_by,
                from_date,
                to_date,
                COALESCE(LAG(to_date) OVER (PARTITION BY resource_id, taken_by ORDER BY from_date), from_date) AS prev_to_date
            FROM
                availabilities
            WHERE
                from_date >= @{nameof(ResourceAvailabilityReadModelParam.FromDate)}
                AND to_date <= @{nameof(ResourceAvailabilityReadModelParam.ToDate)}
                AND resource_id = ANY (@{nameof(ResourceAvailabilityReadModelParam.ResourceIds)})
            
         ),
         GroupedAvailability AS (
            SELECT
                resource_id,
                taken_by,
                from_date,
                to_date,
                prev_to_date,
                CASE WHEN
                    from_date = prev_to_date
                    THEN 0 ELSE 1 END
                AS new_group_flag,
                SUM(CASE WHEN
                    from_date = prev_to_date
                    THEN 0 ELSE 1 END)
                OVER (PARTITION BY resource_id, taken_by ORDER BY from_date) AS grp
            FROM
                AvailabilityWithLag
         )
         SELECT
            resource_id,
            taken_by,
            MIN(from_date) AS start_date,
            MAX(to_date) AS end_date
         FROM
            GroupedAvailability
         GROUP BY
            resource_id, taken_by, grp
         ORDER BY
            start_date;
         """;
    private readonly IDbConnection _dbConnection;

    public ResourceAvailabilityReadModel(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<Calendar> Load(ResourceId resourceId, TimeSlot timeSlot)
    {
        var loaded = await LoadAll(new HashSet<ResourceId> { resourceId }, timeSlot);
        return loaded.Get(resourceId);
    }

    public async Task<Calendars> LoadAll(ISet<ResourceId> resourceIds, TimeSlot timeSlot)
    {
        var results = await _dbConnection.QueryAsync<ResourceAvailabilityReadModelRow>(
            CalendarQuery,
            new ResourceAvailabilityReadModelParam(
                timeSlot.From, timeSlot.To, resourceIds.Select(x => x.Id!.Value).ToArray()));
        var calendars = new Dictionary<ResourceId, Dictionary<Owner, IList<TimeSlot>>>();
        foreach (var result in results)
        {
            var key = new ResourceId(result.resource_id);
            var takenBy = result.taken_by == null ? Owner.None() : new Owner(result.taken_by);
            var loadedSlot = new TimeSlot(result.start_date, result.end_date);
            if (!calendars.ContainsKey(key))
            {
                calendars[key] = new Dictionary<Owner, IList<TimeSlot>>();
            }

            if (!calendars[key].ContainsKey(takenBy))
            {
                calendars[key][takenBy] = new List<TimeSlot>();
            }

            calendars[key][takenBy].Add(loadedSlot);
        }
        return new Calendars(calendars.ToDictionary(
            entry => entry.Key,
            entry => new Calendar(entry.Key, entry.Value)));
    }

    private record ResourceAvailabilityReadModelParam(
        DateTime FromDate,
        DateTime ToDate,
        IEnumerable<Guid> ResourceIds);

    private record ResourceAvailabilityReadModelRow(
        Guid resource_id,
        Guid? taken_by,
        DateTime start_date,
        DateTime end_date
      );
}