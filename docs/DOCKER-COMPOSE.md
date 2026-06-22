# Docker Compose

Docker Compose setup for running the KB application with (optionally) a local llama.cpp LLM service.

## Quick Start

```bash
# Build and start all services
docker compose up --build

# Start in detached mode
docker compose up -d --build

# View logs
docker compose logs -f

# View logs for a specific service
docker compose logs -f kb
docker compose logs -f llama

# Stop and remove containers
docker compose down
```

## Configuration

### Environment Variables

Create a `.env` file in the project root to configure the services. The following variables are available:

| Variable                                | Description               | Default    |
| --------------------------------------- | ------------------------- | ---------- |
| `KB_IMAGE`                              | KB container image        | `sys27/kb` |
| `KB_PORT`                               | Host port for KB UI       | `8080`     |
| `LLM__Endpoint`                         | LLM API endpoint          | —          |
| `LLM__ApiKey`                           | LLM API key               | —          |
| `LLM__Model`                            | Chat model name           | —          |
| `LLM__EmbeddingModel`                   | Embedding model name      | —          |
| `LLM__RerankingModel`                   | Reranking model name      | —          |
| `Ingestion__IsIngestionEnabled`         | Enable document ingestion | `true`     |
| `Ingestion__IsDocumentDiscoveryEnabled` | Enable document discovery | `true`     |
| `Summarization__Enabled`                | Enable summarization      | `true`     |
| `WebSearch__BaseUrl`                    | Web search API base URL   | —          |

### KB with local llama.cpp

Use this for fully local inference with GPU acceleration.

```yaml
services:
  llama:
    image: llama:latest
    build:
      context: ./llama.cpp
      dockerfile: .devops/cuda.Dockerfile
    pull_policy: never
    container_name: llama
    command: --models-max 1 --models-preset /gguf/config-docker.ini --host 0.0.0.0 --port 11434
    restart: unless-stopped
    ports:
      - 11434:11434
    volumes:
      - ./gguf:/gguf:ro
    deploy:
      resources:
        reservations:
          devices:
            - driver: nvidia
              count: all
              capabilities: [gpu]

  kb:
    image: ${KB_IMAGE:-sys27/kb}
    container_name: kb
    restart: unless-stopped
    ports:
      - "${KB_PORT:-8080}:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:80
      - LLM__Endpoint=http://llama:11434/v1
      - LLM__ApiKey=None
      - LLM__Model=${LLM__Model}
      - LLM__EmbeddingModel=${LLM__EmbeddingModel}
      - LLM__RerankingModel=${LLM__RerankingModel}
      - Ingestion__IsIngestionEnabled=true
      - Ingestion__IsDocumentDiscoveryEnabled=true
      - Summarization__Enabled=true
    volumes:
      - ./kb:/data
    depends_on:
      - llama
```

## Volume Management

| Volume            | Purpose                             |
| ----------------- | ----------------------------------- |
| `./kb:/data`      | Persistent KB storage               |
| `./gguf:/gguf:ro` | Read-only GGUF models for llama.cpp |

The `./kb` directory stores all KB data including database and ingested documents.

## GPU Support

GPU acceleration requires:
- NVIDIA GPU
- NVIDIA Container Toolkit installed
- A CUDA-compatible llama.cpp Dockerfile (e.g. `.devops/cuda.Dockerfile`)
