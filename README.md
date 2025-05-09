# MuseTrip360: Museum Management System

## Project Context

MuseTrip360 is a modern, scalable museum management system designed to streamline operations for museums and enhance the visitor experience. The platform supports museum staff in managing artifacts, events, tickets, virtual tours, and user interactions, while providing visitors with seamless access to museum content, ticketing, and personalized recommendations. Built with a focus on flexibility and performance, MuseTrip360 leverages advanced technologies like vector search for content discovery and asynchronous messaging for efficient task processing.

The system addresses the needs of:
- **Museum Staff**: To manage collections, schedule events, handle ticketing, and communicate with visitors.
- **Visitors**: To explore artifacts, book tickets, join virtual tours, and receive personalized recommendations.
- **Administrators**: To oversee user roles, museum policies, and system analytics.

MuseTrip360 aims to modernize museum operations by combining robust backend infrastructure with a user-friendly interface, enabling museums to preserve cultural heritage while engaging a global audience.

## Project Goals

The primary goals of MuseTrip360 are:

1. **Centralized Management**:
   - Provide a unified platform for managing museum assets (artifacts, events, tickets, tours).
   - Support role-based access control (RBAC) for secure and flexible user management.

2. **Enhanced Visitor Experience**:
   - Enable seamless ticket purchasing and event booking.
   - Offer virtual 3D tours and interactive content for remote visitors.
   - Deliver personalized recommendations using vector search (e.g., similar artifacts or events).

3. **Scalability and Performance**:
   - Build a scalable backend using .NET Core and PostgreSQL with `pgvector` for vector searches.
   - Integrate Elasticsearch for advanced full-text and vector search capabilities.
   - Use RabbitMQ for asynchronous task processing (e.g., vector embedding generation).

4. **Extensibility**:
   - Design a modular database schema to support future features (e.g., multi-language support, advanced analytics).
   - Use Docker for consistent deployment across development, testing, and production environments.

5. **Security and Reliability**:
   - Implement JWT-based authentication and RBAC for secure access.
   - Ensure data integrity with transactional updates and audit logging (optional).
   - Provide robust error handling and monitoring for system reliability.

## System Architecture

MuseTrip360 is built as a microservices-inspired, containerized application with the following components:

### Backend
- **Framework**: .NET Core 8
- **API**: RESTful API using ASP.NET Core Web API
- **Authentication**: JWT-based with RBAC (roles and permissions stored in `roles` and `permissions` tables)
- **Key Features**:
  - CRUD operations for entities (artifacts, events, tickets, etc.).
  - Vector search for personalized recommendations (e.g., similar artifacts).
  - Asynchronous task processing (e.g., embedding generation).

### Database
- **Database**: PostgreSQL 15
- **Extensions**:
  - `pgvector`: For vector searches (e.g., `embedded_vectors` table with 768D vectors).
- **ORM**: Entity Framework Core and JSONB support for flexible metadata.

### Search and Recommendations
- **Elasticsearch**: Provides full-text search (e.g., artifact descriptions) and vector search for advanced recommendations.
- **pgvector**: Enables efficient vector similarity searches within PostgreSQL (e.g., cosine similarity for artifact embeddings).
- **Embedding Generation**: External Python service (e.g., SentenceTransformers) generates 768D vectors, integrated via RabbitMQ.

### Messaging
- **RabbitMQ**: Handles asynchronous tasks, such as:
  - Generating vector embeddings when artifacts or events are created/updated.
  - Sending notifications to users.
- **Library**: MassTransit for .NET Core integration with RabbitMQ.

### Deployment
- **Docker**: Containerized services for consistent deployment.
- **Services** (defined in `docker-compose.yml`):
  - `postgres`: PostgreSQL with `pgvector` extension.
  - `redis`: Caching for session management and performance.
  - `rabbitmq`: Message broker for asynchronous tasks.
  - `elasticsearch`: Advanced search engine.
  - `api`: .NET Core API backend.
- **Volumes**: Persist data for PostgreSQL, Redis, RabbitMQ, and Elasticsearch.

## Key Features

1. **User Management**:
   - Role-based access control (e.g., Admin, Staff, Visitor).
   - User profiles with metadata (JSONB) for flexibility.
   - Authentication via email or external providers.

2. **Museum Operations**:
   - Create and manage museums with policies (e.g., visitor rules, refunds).
   - Handle museum creation requests (`museum_requests`).
   - Publish articles related to museums or events.

3. **Artifact Management**:
   - Store artifact details, images, and 3D models.
   - Rate and categorize artifacts by historical period.
   - Recommend similar artifacts using vector search.

4. **Event Management**:
   - Schedule events (exhibitions, workshops, lectures).
   - Manage event capacity, pricing, and booking deadlines.
   - Link events to artifacts or virtual tours.

5. **Ticketing System**:
   - Offer ticket types (`ticket_masters`) and add-ons (e.g., guided tours).
   - Generate QR codes and confirmation codes for tickets.
   - Track ticket status (e.g., Active, Used, Cancelled).

6. **Virtual Tours**:
   - Provide 3D virtual tours (`tour_onlines`) with interactive content.
   - Link tours to events or artifacts for immersive experiences.

7. **Payments**:
   - Process orders and payments with status tracking.
   - Support group bookings and discounts.

8. **Messaging and Notifications**:
   - Enable user-to-user messaging via conversations.
   - Send system-wide or user-specific notifications.

9. **Feedback and Reporting**:
   - Collect user feedback on museums, events, or exhibitions.
   - Generate system reports for analytics.

10. **Search and Recommendations**:
    - Full-text search for artifacts, events, and articles (via Elasticsearch).
    - Vector-based similarity search for personalized recommendations (via `pgvector` or Elasticsearch).

## Technical Implementation

### Backend Implementation
- **Controllers**: RESTful endpoints for CRUD operations and vector searches (e.g., `/api/vectors/search`).
- **Services**: Business logic for entity management, vector generation, and search.

### Vector Search Workflow
1. **Embedding Generation**:
   - Artifact or event content (e.g., `description`) is sent to a Python service via RabbitMQ.
   - The service generates 768D vectors using SentenceTransformers.
   - Vectors are stored in `embedded_vectors` (PostgreSQL) and `museum_vectors` (Elasticsearch).

2. **Search**:
   - Users query for similar entities (e.g., artifacts) using a vector.
   - The backend performs cosine similarity searches using `pgvector` or Elasticsearch.
   - Results are joined with entity data (e.g., artifact names) for display.

### Deployment
- **Docker Compose**: Defines services for development and testing.
- **CI/CD**: Planned for production (e.g., GitHub Actions, Kubernetes).
- **Monitoring**: Logging via Serilog, monitoring via Prometheus/Grafana (future).

## Challenges and Solutions

1. **Challenge**: Efficient vector search for large datasets.
   - **Solution**: Use `pgvector` with HNSW indexes for PostgreSQL and Elasticsearch for scalability.

2. **Challenge**: Asynchronous task processing for embedding generation.
   - **Solution**: Implement RabbitMQ with MassTransit for reliable message queuing.

3. **Challenge**: Consistent database naming conventions.
   - **Solution**: Apply snake_case naming globally in EF Core using a custom converter.

4. **Challenge**: Secure and scalable deployment.
   - **Solution**: Use Docker for containerization, secure credentials with environment variables, and plan for Kubernetes in production.

## Future Enhancements

1. **Multi-Language Support**:
   - Add translations for artifact descriptions and event titles.
   - Store translations in `metadata` (JSONB) or a dedicated table.

2. **Advanced Analytics**:
   - Implement dashboards for ticket sales, event attendance, and user engagement.
   - Use Elasticsearch aggregations for real-time analytics.

3. **Mobile App**:
   - Develop a mobile app for visitors to access tours, tickets, and notifications.
   - Integrate with the existing API.

4. **AI Enhancements**:
   - Use machine learning for dynamic pricing or visitor behavior analysis.
   - Improve vector search with fine-tuned embedding models.

## Getting Started

### Prerequisites
- **Docker**: For containerized deployment.
- **.NET Core 8 SDK**: For building the backend.
- **PostgreSQL 15**: With `pgvector` extension.
- **Node.js** (optional): For frontend development.

### Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/hiamthach108/musetrip360-server.git
   cd musetrip360-server
   ```

2. Start the services using Docker Compose:
   ```bash
   docker-compose up -d
   ```

3. Build and run the backend:
   ```bash
   dotnet build
   dotnet run
   ```

4. Access the API at `http://localhost:5000/api/v1`.

### Documentation
- API documentation is available at `http://localhost:5000/swagger`.
- For detailed coding conventions and project structure, refer to `memory-bank/api_conventions.md`.

## Contributing
- Fork the repository and create a feature branch.
- Submit a pull request with a clear description of the changes.

## License
This project is licensed under the MIT License - see the LICENSE file for details.
