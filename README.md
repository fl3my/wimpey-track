# WimpeyTrack
## Overview
> A responsive web application to simplify the input of the users expense information into a single system. 
> The grouped information can then be sent to recipients for approval through Gmail or downloaded.

### Features
- Convenient trip logging with automatic distance calculation with routing engine.
- Receipt Analysis to extract purchase information.
- Simple receipt detection to crop the image to the content only.
- Automatic claim rate calculate for the tax year.

## Architecture
```mermaid
flowchart LR
    React[React SPA] -->|HTTP REST| API
    API[NET Core API] -->|HTTP POST| Vision[Document Vision Service]
    API -->|Image Analysis Request| Azure[Azure Document Intelligence]
    API -->|Route Request| Route[OSRM Routing Service]
    API -->|EF Core| SQLite[(SQLite)]
    
```

## Demonstration
### Dashboard
![Dashboard](docs/dashboard.png)
### Journey entry
![Trip Entry](docs/tripentry.png)
### Receipt OCR entry
![Receipt OCR](docs/receiptocr.png)
### Report generation
![Report Generation](docs/report.png)
