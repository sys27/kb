# Docker

## Build locally

```bash
docker build -t kb .
```

## Run the container

```bash
docker run -p 8080:80 kb
```

The application will be available at `http://localhost:8080`.

---

### Environment variables

Configure the following via `.env` file or directly in `docker run` command:

| Variable                                | Description               | Default |
| --------------------------------------- | ------------------------- | ------- |
| `LLM__Endpoint`                         | LLM API endpoint          | —       |
| `LLM__ApiKey`                           | LLM API key               | —       |
| `LLM__Model`                            | Chat model name           | —       |
| `LLM__EmbeddingModel`                   | Embedding model name      | —       |
| `LLM__RerankingModel`                   | Reranking model name      | —       |
| `Ingestion__IsIngestionEnabled`         | Enable document ingestion | `true`  |
| `Ingestion__IsDocumentDiscoveryEnabled` | Enable document discovery | `true`  |
| `Summarization__Enabled`                | Enable summarization      | `true`  |
| `WebSearch__BaseUrl`                    | Web search API base URL   | —       |

---

## Docker Compose with local LLM (llama.cpp)

See [`docs/DOCKER-COMPOSE.md`](./DOCKER-COMPOSE.md) for a complete docker-compose setup that includes a llama.cpp service for local model inference.
