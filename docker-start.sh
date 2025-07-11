#!/bin/bash

echo "🚀 Starting MuseTrip360 Application Stack..."

# Check if .env file exists
if [ ! -f .env ]; then
    echo "⚠️  Warning: .env file not found!"
    echo "📝 Please copy env.example to .env and configure your environment variables:"
    echo "   cp env.example .env"
    echo "   # Then edit .env with your actual values"
    echo ""
    echo "🔄 Continuing with default values for now..."
fi

# Build and start all services
echo "🔨 Building and starting services..."
docker-compose up --build -d

# Wait for services to be healthy
echo "⏳ Waiting for services to be healthy..."
echo "📊 You can monitor the logs with: docker-compose logs -f"

# Show service status
echo ""
echo "📋 Service Status:"
docker-compose ps

echo ""
echo "✅ Application stack started!"
echo ""
echo "🌐 Access points:"
echo "   - API Server: http://localhost:5000"
echo "   - Elasticsearch: http://localhost:9200"
echo "   - Kibana: http://localhost:5601"
echo "   - RabbitMQ Management: http://localhost:15672"
echo "   - PostgreSQL: localhost:5432"
echo "   - Redis: localhost:6379"
echo ""
echo "🔐 Default Credentials:"
echo "   - Kibana: kibana_admin / musetrip360_admin"
echo "   - RabbitMQ: user / password"
echo "   - PostgreSQL: postgres / 123456"
echo "   - Elasticsearch: elastic / musetrip360_elastic"
echo ""
echo "📊 Monitor logs: docker-compose logs -f musetrip360-api"
echo "🛑 Stop services: docker-compose down" 