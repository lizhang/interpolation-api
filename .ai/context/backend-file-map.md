# Repository Structure

**Read this file before writing or refactoring any code.**

```
src/
├── Program.cs
├── appsettings.json
│
├── Configuration/
│   └── AppSettings.cs
│
├── Models/
│   ├── PresignedPostData.cs
│   └── SqsJobMessage.cs
│
├── Extensions/
│   └── ServiceCollectionExtensions.cs
│
├── Controllers/
│   ├── JobsController.cs
│   ├── StatsController.cs
│   └── Dtos/
│       ├── GeneratePresignedUrlsRequest.cs
│       ├── GeneratePresignedUrlsResponse.cs
│       ├── SubmitJobRequest.cs
│       ├── SubmitJobResponse.cs
│       └── GetStatsResponse.cs
│
├── Operations/
│   ├── GeneratePresignedUrls/
│   │   ├── .instruction/
│   │   │   ├── business.md
│   │   │   ├── uploads.request.json
│   │   │   └── uploads.response.json
│   │   ├── IGeneratePresignedUrlsOperation.cs
│   │   ├── GeneratePresignedUrlsOperation.cs
│   │   ├── GeneratePresignedUrlsInput.cs
│   │   └── GeneratePresignedUrlsResult.cs
│   ├── SubmitJob/
│   │   ├── .instruction/
│   │   │   ├── business.md
│   │   │   ├── jobs.request.json
│   │   │   └── jobs.response.json
│   │   ├── ISubmitJobOperation.cs
│   │   ├── SubmitJobOperation.cs
│   │   ├── SubmitJobInput.cs
│   │   └── SubmitJobResult.cs
│   └── GetStats/
│       ├── .instruction/
│       │   ├── business.md
│       │   └── stats.response.json
│       ├── IGetStatsOperation.cs
│       ├── GetStatsOperation.cs
│       └── AppStats.cs
│
└── Services/
    ├── IS3Service.cs / S3Service.cs
    ├── ISqsService.cs / SqsService.cs
    └── IDynamoDbService.cs / DynamoDbService.cs
```

**Keep this file up to date whenever files are added or removed.**
