# KB - Personal Knowledge Base

A local-first, AI-powered knowledge base that understands your documents, conversations, and projects.

## Overview

**KB** (Knowledge Base) is a personal, single-user system that ingests your documents and chat history, extracts meaningful information, and builds a semantic understanding of your knowledge.

Instead of manually organizing notes, **KB** continuously learns from your data - so future interactions are smarter, more contextual, and personalized.

## Key Features

- **AI-Powered Knowledge Extraction.**  
  Automatically analyzes documents and conversations to extract relevant insights.
- **Document Ingestion Pipeline.**  
  Upload raw files - KB processes, chunks, and indexes them for semantic search.
- **Chat Memory Integration.**  
  Conversations are summarized and merged into your evolving knowledge base.
- **Semantic Search (Vector-Based).**  
  Uses embeddings to retrieve relevant context instead of keyword matching.
- **Local-First Architecture.**  
  Your data stays on your machine. No external vector DB required.
- **Context-Aware Prompting.**  
  Retrieved knowledge is injected into LLM prompts to improve response quality.

## Architecture

```
                ┌──────────────────────┐
                │      Frontend        │
                │   (In Development)   │
                └─────────┬────────────┘
                          │
                          ▼
                ┌──────────────────────┐
                │     .NET Backend     │
                └─────────┬────────────┘
                          │
        ┌─────────────────┼─────────────────┐
        ▼                 ▼                 ▼
┌──────────────┐  ┌──────────────┐  ┌──────────────┐
│  Ingestion   │  │   Chat Flow  │  │   Retrieval  │
│  Pipeline    │  │              │  │              │
└──────┬───────┘  └──────┬───────┘  └──────┬───────┘
       ▼                 ▼                 ▼
┌──────────────────────────────────────────────────┐
│          SQLite Vector Store                     │
│   - Embeddings                                   │
│   - Summaries                                    │
│   - Extracted user knowledge                     │
└──────────────────────────────────────────────────┘
```

## How It Works

1. Ingestion  
Documents are uploaded  
Content is chunked (typically 512 tokens)  
Embeddings are generated and stored  

2. Knowledge Extraction  
LLM analyzes content and extracts:
   - facts  
   - key points  
   - decisions  
   - user-specific insights  

3. Chat Processing  
Conversations are:
   - summarized
   - merged into long-term memory
   - indexed for retrieval

4. Retrieval + Prompting  
Relevant context is fetched via semantic search  
Injected into system prompt  
LLM generates context-aware responses

## Tech Stack

Backend: .NET 10  
AI Integration: Microsoft.Extensions.AI  
Vector Store: SQLite (local-first)  
Frontend: In progress

## Getting Started

> [!WARNING]
> Project is in active development

### Prerequisites

- .NET 10 SDK
- API key for your LLM provider (if applicable)

### Setup

```bash
git clone https://github.com/sys27/kb.git
cd kb/backend
dotnet build
```

### Run

```bash
dotnet run
```

## Design Principles

- Local-first — your data belongs to you
- Single-user focused — optimized for personal knowledge, not teams
- Composable architecture — easy to swap components (LLM, embeddings, storage)
- Incremental intelligence — the system improves as you use it

## Roadmap

- Frontend UI
- Advanced retrieval (hybrid search: vector + keyword)
- Better knowledge extraction pipelines
- Better ingestion system
- Migration path to external vector DB (e.g., Qdrant)

# License

GNU GPL v3. See the [LICENSE](https://github.com/sys27/kb/blob/master/LICENSE) file for details.