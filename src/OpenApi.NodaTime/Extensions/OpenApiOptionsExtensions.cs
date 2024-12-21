using System;
using System.Text.Json;
using Microsoft.AspNetCore.OpenApi;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using OpenApi.Extensions.Extensions;
using OpenApi.NodaTime.Transformers.Schemas;

namespace OpenApi.NodaTime.Extensions;

public static class OpenApiOptionsExtensions
{
    public static OpenApiOptions ConfigureNodaTime(this OpenApiOptions options,
                                                   IDateTimeZoneProvider? dateTimeZoneProvider = null,
                                                   JsonSerializerOptions? jsonSerializerOptions = null)
    {
        dateTimeZoneProvider ??= DateTimeZoneProviders.Tzdb;
        jsonSerializerOptions ??= new JsonSerializerOptions();
        jsonSerializerOptions = jsonSerializerOptions.ConfigureForNodaTime(dateTimeZoneProvider);

        var instant = Instant.FromDateTimeOffset(DateTimeOffset.Now);
        var zoned = instant.InZone(dateTimeZoneProvider.GetSystemDefault());
        var offsetTime = zoned.ToOffsetDateTime();

        var interval = new Interval(instant,
                                    instant.PlusTicks(TimeSpan.TicksPerDay)
                                           .PlusTicks(TimeSpan.TicksPerHour)
                                           .PlusTicks(TimeSpan.TicksPerMinute)
                                           .PlusTicks(TimeSpan.TicksPerSecond)
                                           .PlusTicks(TimeSpan.TicksPerMillisecond));

        var period = Period.Between(zoned.LocalDateTime,
                                    interval.End.InZone(dateTimeZoneProvider.GetSystemDefault())
                                            .LocalDateTime,
                                    PeriodUnits.AllUnits);

        options.AddType<Instant, string>("date-time", instant, jsonSerializerOptions);
        options.AddType<LocalDate, string>("date", zoned.Date, jsonSerializerOptions);
        options.AddType<LocalTime, string>("time", zoned.TimeOfDay, jsonSerializerOptions);
        options.AddType<LocalDateTime, string>("date-time", zoned.LocalDateTime, jsonSerializerOptions);
        options.AddType<OffsetTime, string>("time", offsetTime.ToOffsetTime(), jsonSerializerOptions);
        options.AddType<OffsetDate, string>("date", offsetTime.ToOffsetDate(), jsonSerializerOptions);
        options.AddType<OffsetDateTime, string>("date-time", offsetTime, jsonSerializerOptions);
        options.AddType<ZonedDateTime, string>("date-time", zoned, jsonSerializerOptions);
        options.AddSchemaTransformer(new IntervalSchemaTransformer(instant, jsonSerializerOptions));
        options.AddSchemaTransformer(new DateIntervalSchemaTransformer(instant, dateTimeZoneProvider.GetSystemDefault(), jsonSerializerOptions));
        options.AddType<Offset, string>(zoned.Offset, jsonSerializerOptions);
        options.AddType<Period, string>(period, jsonSerializerOptions);
        options.AddType<Duration, string>("duration", interval.Duration, jsonSerializerOptions);
        options.AddType<DateTimeZone, string>(dateTimeZoneProvider.GetSystemDefault(), jsonSerializerOptions);

        return options;
    }
}
