# Logstash Configuration for MuseTrip360

This directory contains the Logstash configuration for the MuseTrip360 ELK Stack implementation.

## Directory Structure

```
logstash/
├── config/
│   └── logstash.yml              # Main Logstash configuration
├── pipeline/
│   └── logstash.conf             # Pipeline configuration
├── serilog-logstash-config.json  # Sample .NET Core Serilog configuration
└── README.md                     # This file
```

## Configuration Overview

### Logstash Service
- **Image**: `docker.elastic.co/logstash/logstash:9.0.0`
- **Ports**:
  - `5044`: Beats input (for Filebeat, Metricbeat, etc.)
  - `5001`: TCP input for .NET Core structured logs
  - `8080`: HTTP input for direct log shipping
  - `9600`: Logstash API and monitoring

### Input Sources

1. **TCP Input (Port 5001)**: For .NET Core structured JSON logs
2. **Beats Input (Port 5044)**: For Filebeat and other Elastic Beats
3. **HTTP Input (Port 8080)**: For direct HTTP log shipping

### Processing Pipeline

The pipeline processes different types of logs:

- **API Logs**: Structured logs from .NET Core application
- **Access Logs**: HTTP access logs in Apache format
- **Beat Logs**: Logs from Elastic Beats (Filebeat, Metricbeat, etc.)

### Output

All processed logs are sent to Elasticsearch with different index patterns:
- `musetrip360-api-logs-YYYY.MM.dd`: .NET Core API logs
- `musetrip360-beats-logs-YYYY.MM.dd`: Beats logs
- `musetrip360-access-logs-YYYY.MM.dd`: HTTP access logs
- `musetrip360-general-logs-YYYY.MM.dd`: Other logs

## .NET Core Integration

### Required NuGet Packages

Add these packages to your .NET Core project:

```xml
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
<PackageReference Include="Serilog.Sinks.Http" Version="8.0.0" />
<PackageReference Include="Serilog.Formatting.Json" Version="1.0.0" />
<PackageReference Include="Serilog.Enrichers.Environment" Version="2.3.0" />
<PackageReference Include="Serilog.Enrichers.Thread" Version="3.1.0" />
```

### Program.cs Configuration

```csharp
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// ... other configurations

var app = builder.Build();

// Add Serilog request logging
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "Handled {RequestPath} in {Elapsed:0.0000} ms";
    options.GetLevel = (httpContext, elapsed, ex) => LogEventLevel.Information;
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].FirstOrDefault());
    };
});

app.Run();
```

### appsettings.json Configuration

Use the provided `serilog-logstash-config.json` as a reference for your `appsettings.json`:

```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "Http",
        "Args": {
          "requestUri": "http://logstash:8080",
          "batchPostingLimit": 50,
          "period": "00:00:02"
        }
      }
    ]
  }
}
```

## Starting the ELK Stack

1. **Start all services**:
   ```bash
   docker-compose up -d
   ```

2. **Check service health**:
   ```bash
   docker-compose ps
   ```

3. **View Logstash logs**:
   ```bash
   docker-compose logs -f logstash
   ```

## Accessing Services

- **Elasticsearch**: http://localhost:9200
  - Username: `elastic`
  - Password: `musetrip360_elastic`

- **Kibana**: http://localhost:5601
  - Username: `kibana_admin`
  - Password: `musetrip360_admin`

- **Logstash API**: http://localhost:9600

## Testing Log Ingestion

### Test HTTP Input

```bash
curl -X POST "http://localhost:8080" \
  -H "Content-Type: application/json" \
  -d '{
    "timestamp": "2024-01-01T12:00:00Z",
    "level": "INFO",
    "message": "Test log message",
    "application": "test-app"
  }'
```

### Test TCP Input (from .NET Core)

The .NET Core application will automatically send logs to Logstash when configured with Serilog HTTP sink.

## Kibana Dashboard Setup

1. Open Kibana at http://localhost:5601
2. Go to **Management** > **Stack Management** > **Index Patterns**
3. Create index patterns for:
   - `musetrip360-api-logs-*`
   - `musetrip360-beats-logs-*`
   - `musetrip360-access-logs-*`
4. Go to **Analytics** > **Discover** to view logs
5. Create dashboards in **Analytics** > **Dashboard**

## Monitoring and Troubleshooting

### Check Logstash Health

```bash
curl http://localhost:9600/_node/stats/pipeline
```

### View Pipeline Stats

```bash
curl http://localhost:9600/_node/stats/pipeline?pretty
```

### Common Issues

1. **Connection refused**: Ensure Elasticsearch is healthy before starting Logstash
2. **Index creation failed**: Check Elasticsearch authentication credentials
3. **No logs appearing**: Verify .NET Core Serilog configuration and network connectivity

## Production Considerations

1. **Remove debug output**: Comment out the `stdout` output in `logstash.conf`
2. **Increase resources**: Adjust `LS_JAVA_OPTS` for production workloads
3. **Security**: Enable SSL/TLS for production environments
4. **Monitoring**: Enable X-Pack monitoring for production monitoring
5. **Backup**: Implement Elasticsearch backup strategies

## Log Retention

Logs are stored with daily indices. Implement Index Lifecycle Management (ILM) policies in Elasticsearch to manage log retention and cleanup old indices automatically. 