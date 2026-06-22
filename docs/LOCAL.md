# Local Development

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 20+](https://nodejs.org/)
- [llama.cpp](https://github.com/ggml-org/llama.cpp)

---

## Backend (.NET 10)

```bash
cd backend

# Restore dependencies and build
dotnet build

# Run the development server
dotnet run
```

---

## Frontend (React Router 7 + TypeScript)

```bash
cd frontend

# Install dependencies
npm install

# Start development server
npm run dev

# Build for production
npm run build
```

---

## Full-stack local development

The frontend dev server proxies `/api` requests to the backend. Run both:

```bash
# Terminal 1 - Backend
cd backend && dotnet run

# Terminal 2 - Frontend (proxy configured to http://localhost:5164)
cd frontend && npm run dev
```

> **Note:** Ensure the backend is running on port `5164` (configured in `frontend/vite.config.ts`). Adjust the proxy target if using a different port.

---

## llama.cpp

llama.cpp provides the local LLM backend. The application requires an OpenAI-compatible endpoint.

### Installation

Clone and build from source:

```bash
git clone https://github.com/ggml-org/llama.cpp.git
cd llama.cpp
cmake -B build -DGGML_CUDA=ON -DBUILD_SHARED_LIBS=OFF -DLLAMA_BUILD_TESTS=OFF
cmake --build build --config Release -j $(nproc)
```

or install it via your system's package manager.

### Running

Start the server with a model:

```bash
./build/bin/llama-server -m <path-to-model.gguf> --host 0.0.0.0 --port 11434
```

or

```bash
./build/bin/llama-server --models-preset <path-to-config> --host 0.0.0.0 --port 11434
```

The server exposes an OpenAI-compatible API at `http://localhost:11434/v1`.

## Application Configuration

All available configuration options are listed in `appsettings.json`. To run the application, you need to provide values for:

- LLM
  - Endpoint - a url to OpenAI compatible llama.cpp endpoint
  - ApiKey - any value
  - Model - the main LLM model, tested on Qwen 3.6
  - EmbeddingModel - the embedding model, tested on Qwen3
  - RerankingModel - the reranking model, tested on Qwen3
- WebSearch
  - BaseUrl - a url to SearXNG endpoint

Example `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=kb.db"
  },
  "LLM": {
    "Endpoint": "http://localhost:11434/v1/",
    "ApiKey": "none",
    "Model": "qwen3.6:35b-general",
    "EmbeddingModel": "qwen3-embedding:0.6b",
    "RerankingModel": "qwen3-reranking:0.6b"
  },
  "Ingestion": {
    "IsIngestionEnabled": true,
    "IsDocumentDiscoveryEnabled": true,
    "Path": "/path/documents",
    "IngestionDelay": "00:01:00",
    "DocumentDiscoveryDelay": "00:01:00"
  },
  "Summarization": {
    "Enabled": true,
    "Delay": "00:10:00",
    "SummaryInactivityWindow": "00:00:01"
  },
  "RemoveDanglingEmbeddings": {
    "Enabled": true,
    "BatchSize": 100,
    "Delay": "00:01:00"
  },
  "WebSearch": {
    "BaseUrl": "https://search.com/",
    "MaxResults": 10,
    "RerankTopK": 3
  }
}
```