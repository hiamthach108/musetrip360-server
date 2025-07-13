# Environment Configuration Guide

## Overview

The MuseTrip360 application now uses environment variables for all service configurations, making it easy to manage different environments (development, staging, production) and keep sensitive credentials secure.

## Setup Instructions

### 1. Create Environment File

Copy the template file to create your environment configuration:

```bash
cp env.template .env
```

### 2. Configure Service Credentials

Edit the `.env` file and update the following sections:

#### Database Configuration
```env
POSTGRES_USER=postgres
POSTGRES_PASSWORD=your_secure_postgres_password
POSTGRES_DB=musetrip360db
POSTGRES_PORT=5432
```

#### Redis Configuration
```env
REDIS_PASSWORD=your_secure_redis_password
REDIS_PORT=6379
```

#### RabbitMQ Configuration
```env
RABBITMQ_USER=your_rabbitmq_username
RABBITMQ_PASSWORD=your_secure_rabbitmq_password
RABBITMQ_PORT=5672
RABBITMQ_MANAGEMENT_PORT=15672
```

#### Elasticsearch Configuration
```env
ELASTIC_PASSWORD=your_secure_elastic_password
ELASTIC_PORT=9200
ELASTIC_TRANSPORT_PORT=9300
ELASTIC_CLUSTER_NAME=musetrip360-cluster
ELASTIC_NODE_NAME=musetrip360-elastic
```

#### Kibana Configuration
```env
KIBANA_PASSWORD=your_secure_kibana_password
KIBANA_ADMIN_PASSWORD=your_secure_kibana_admin_password
KIBANA_PORT=5601
```

### 3. Configure Application Services

Update the following required services in your `.env` file:

#### Google OAuth (Required for Authentication)
```env
GOOGLE_CLIENT_ID=your-google-client-id-here
GOOGLE_CLIENT_SECRET=your-google-client-secret-here
```

#### SMTP Email (Required for Email Notifications)
```env
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_ENABLE_SSL=true
SMTP_EMAIL=your-email@gmail.com
SMTP_PASSWORD=your-email-password-or-app-password
```

#### Cloudinary (Required for File Uploads)
```env
CLOUDINARY_CLOUD_NAME=your-cloudinary-cloud-name
CLOUDINARY_API_KEY=your-cloudinary-api-key
CLOUDINARY_API_SECRET=your-cloudinary-api-secret
```

#### PayOS (Required for Payment Processing)
```env
PAYOS_CLIENT_ID=your-payos-client-id
PAYOS_API_KEY=your-payos-api-key
PAYOS_CHECKSUM_KEY=your-payos-checksum-key
```

## Running the Application

### 1. Start All Services

```bash
# Start all services with docker-compose
docker-compose up -d

# Or start with logs visible
docker-compose up
```

### 2. Check Service Status

```bash
# Check all services status
docker-compose ps

# Check specific service logs
docker-compose logs postgres
docker-compose logs redis
docker-compose logs rabbitmq
docker-compose logs elastic
docker-compose logs kibana
docker-compose logs musetrip360-api
```

### 3. Access Services

Once all services are running, you can access:

- **API Application**: http://localhost:5000 (or your configured API_PORT)
- **API Documentation**: http://localhost:5000/swagger
- **RabbitMQ Management**: http://localhost:15672 (user: your RABBITMQ_USER, password: your RABBITMQ_PASSWORD)
- **Kibana Dashboard**: http://localhost:5601 (user: kibana_admin, password: your KIBANA_ADMIN_PASSWORD)
- **Elasticsearch**: http://localhost:9200 (user: elastic, password: your ELASTIC_PASSWORD)

## Environment Variables Reference

### Service Ports
All service ports can be customized via environment variables:

| Service | Environment Variable | Default Port |
|---------|---------------------|--------------|
| API | API_PORT | 5000 |
| PostgreSQL | POSTGRES_PORT | 5432 |
| Redis | REDIS_PORT | 6379 |
| RabbitMQ | RABBITMQ_PORT | 5672 |
| RabbitMQ Management | RABBITMQ_MANAGEMENT_PORT | 15672 |
| Elasticsearch | ELASTIC_PORT | 9200 |
| Elasticsearch Transport | ELASTIC_TRANSPORT_PORT | 9300 |
| Kibana | KIBANA_PORT | 5601 |
| Logstash Beats | LOGSTASH_BEATS_PORT | 5044 |
| Logstash TCP | LOGSTASH_TCP_PORT | 5001 |
| Logstash HTTP | LOGSTASH_HTTP_PORT | 8080 |
| Logstash API | LOGSTASH_API_PORT | 9600 |

### Default Values
All environment variables have sensible defaults. If you don't specify a value, the default will be used. For example:
- `${POSTGRES_PORT:-5432}` means use POSTGRES_PORT if set, otherwise use 5432

## Security Best Practices

### 1. Production Environment
For production deployment:

```env
ASPNETCORE_ENVIRONMENT=Production
```

### 2. Strong Passwords
Use strong, unique passwords for all services:
- At least 16 characters
- Mix of letters, numbers, and symbols
- Different passwords for each service

### 3. Secret Management
Consider using external secret management systems:
- Docker Secrets
- Kubernetes Secrets
- HashiCorp Vault
- Cloud provider secret managers (AWS Secrets Manager, Azure Key Vault, etc.)

### 4. Network Security
- Use custom Docker networks
- Implement proper firewall rules
- Enable SSL/TLS for all external communications

## Troubleshooting

### 1. Service Connection Issues
If services can't connect to each other:

```bash
# Check if all services are running
docker-compose ps

# Check service logs
docker-compose logs [service-name]

# Restart specific service
docker-compose restart [service-name]
```

### 2. Database Connection Issues
```bash
# Check PostgreSQL logs
docker-compose logs postgres

# Test database connection
docker-compose exec postgres psql -U postgres -d musetrip360db -c "SELECT 1;"
```

### 3. Redis Connection Issues
```bash
# Check Redis logs
docker-compose logs redis

# Test Redis connection
docker-compose exec redis redis-cli ping
```

### 4. Environment Variable Issues
```bash
# Check if .env file exists
ls -la .env

# Verify environment variables are loaded
docker-compose config
```

## Development vs Production

### Development Setup
```env
ASPNETCORE_ENVIRONMENT=Development
LOG_LEVEL_DEFAULT=Debug
```

### Production Setup
```env
ASPNETCORE_ENVIRONMENT=Production
LOG_LEVEL_DEFAULT=Information
# Use stronger passwords
# Enable SSL/TLS
# Use external secret management
```

## Backup and Recovery

### Database Backup
```bash
# Backup PostgreSQL database
docker-compose exec postgres pg_dump -U postgres musetrip360db > backup.sql

# Restore database
docker-compose exec -T postgres psql -U postgres musetrip360db < backup.sql
```

### Configuration Backup
- Keep your `.env` file secure and backed up
- Use version control for `env.template`
- Document any custom configurations

## Monitoring and Logging

### Application Logs
- Logs are sent to Elasticsearch via Logstash
- View logs in Kibana dashboard
- Configure log levels via environment variables

### Service Monitoring
- Use Docker health checks
- Monitor service ports and connectivity
- Set up alerts for service failures

## Next Steps

1. **Copy and configure your `.env` file**
2. **Update all service credentials**
3. **Configure external service integrations**
4. **Test the application startup**
5. **Set up monitoring and alerting**
6. **Plan for production deployment** 