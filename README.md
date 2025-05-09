# FebroFlow

A .NET Core implementation of a workflow automation system for AI bots, similar to N8N but implemented in code.

## Overview

FebroFlow is a backend-only service that allows you to create and manage conversation flows for AI bots. It provides a simple API for defining nodes, connections, and executing workflows.

## Features

- Create and manage conversation flows
- Integration with Telegram
- OpenAI integration for processing messages
- Vector database (Pinecone) for context retrieval
- Database persistence for flow state

## Project Structure

- **FebroFlow.API**: Web API endpoints
- **FebroFlow.Business**: Business logic and services
- **FebroFlow.Core**: Core utilities and interfaces
- **FebroFlow.Data**: Data models and DTOs
- **FebroFlow.DataAccess**: Database access and repositories
