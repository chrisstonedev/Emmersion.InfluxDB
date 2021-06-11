# EL.InfluxDB
A library for sending data to Influx.

## Usage

To configure DI, call:

 `Emmersion.InfluxDB.DependencyInjectionConfig.ConfigureServices(services);`

Then you can record data to Influx by injecting an `IInfluxRecorder`.
Here is a simple example:

```csharp
var fields = new [] {
    new Field("count", 1),
    new Field("duration-ms", 123)
};
influxRecorder.Record(new InfluxPoint("measurement-name", fields));
```

### Tags

Each `InfluxPoint` may optionally specify tags which are key-value pairs you can use to filter or group data in Grafana.
However, you need to be careful about the total number of unique tags in the Influx database.
A high cardinality of unique tags can impact performance; try to keep the number to 100 or less.

(This means you should not use things like `accountId` or `userId` as tags, as there are too many possible unique values.)

## Version History

### v3.0
* Changed namespace from `EL.` to `Emmersion.`
* Removed `IDataSanitizer` because it was overly specific.
