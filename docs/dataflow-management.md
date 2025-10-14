# Dataflow Management Guide

This guide covers how to use the Microsoft Data Factory MCP Server for managing Microsoft Fabric dataflows.

## Overview

The dataflow management tools allow you to:
- List all dataflows within a specific workspace
- Access detailed dataflow information including properties and metadata
- Navigate paginated results for large dataflow collections
- Handle different dataflow types and configurations

## Available Operations

### List Dataflows

Retrieve a list of all dataflows within a specified workspace with full details.

#### Usage
```
list_dataflows(workspaceId: "12345678-1234-1234-1234-123456789012")
```

#### With Pagination
```
list_dataflows(workspaceId: "12345678-1234-1234-1234-123456789012", continuationToken: "next-page-token")
```

#### Response Format
```json
{
  "workspaceId": "12345678-1234-1234-1234-123456789012",
  "dataflowCount": 5,
  "continuationToken": "eyJza2lwIjoyMCwidGFrZSI6MjB9",
  "continuationUri": "https://api.fabric.microsoft.com/v1/workspaces/12345/dataflows?continuationToken=abc123",
  "hasMoreResults": true,
  "dataflows": [
    {
      "id": "87654321-4321-4321-4321-210987654321",
      "displayName": "Sales Data ETL",
      "description": "Extracts, transforms and loads sales data from multiple sources",
      "type": "Dataflow",
      "workspaceId": "12345678-1234-1234-1234-123456789012",
      "folderId": "11111111-1111-1111-1111-111111111111",
      "tags": [
        {
          "id": "22222222-2222-2222-2222-222222222222",
          "displayName": "Sales"
        }
      ],
      "properties": {
        "isParametric": false
      }
    }
  ]
}
```

## Dataflow Properties

Dataflows in Microsoft Fabric include several key properties:

### Basic Properties
- **id**: Unique identifier for the dataflow
- **displayName**: Human-readable name of the dataflow
- **description**: Optional description of the dataflow's purpose
- **type**: Always "Dataflow" for dataflow items
- **workspaceId**: ID of the containing workspace

### Optional Properties
- **folderId**: ID of the folder containing the dataflow (if organized in folders)
- **tags**: Array of tags applied to the dataflow for categorization
- **properties**: Additional metadata about the dataflow

### Dataflow-Specific Properties
- **isParametric**: Boolean indicating if the dataflow uses parameters

## Usage Examples

### Basic Dataflow Operations
```
# List all dataflows in a workspace
> list dataflows in workspace 12345678-1234-1234-1234-123456789012

# List dataflows with specific workspace
> show me all dataflows in my analytics workspace

# Get dataflows from a workspace with pagination
> list dataflows in workspace abc123 with continuation token xyz789
```

### Practical Scenarios
```
# Discovery - find all dataflows in a workspace
> what dataflows are available in workspace 12345678-1234-1234-1234-123456789012?

# Analysis - understand dataflow distribution
> show me how many dataflows are in workspace 12345678-1234-1234-1234-123456789012

# Navigation - browse through large collections
> get the next page of dataflows using token eyJza2lwIjoyMCwidGFrZSI6MjB9
```


