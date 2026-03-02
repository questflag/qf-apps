# Project Cleanup and Maintenance

This document summarizes recent architectural refactors and standard maintenance tasks to keep the codebase healthy.

## Architectural Notes (Recent Refactors)

- **Database Partitioning**: The Infrastructure and Passport databases are strictly separate. Do not introduce cross-database foreign keys.
- **ApiCore Consolidation**: Shared API services, middleware, and common DTOs are centralized in `QuestFlag.Infrastructure.ApiCore`.
- **Repository Location**: Domain repositories have been moved from `Core` projects to `Application` projects to better align with Clean Architecture (Domain defines interface, Application implements).

## Maintenance Tasks

### 1. Removing Log and Temporary Files
Clean up build artifacts, logs, and temp files to free up space and avoid file locking issues.

```powershell
# Remove all bin and obj folders
Get-ChildItem -Path . -Filter bin -Recurse | Remove-Item -Force -Recurse
Get-ChildItem -Path . -Filter obj -Recurse | Remove-Item -Force -Recurse

# Remove log files and temp files
Get-ChildItem -Path . -Filter *.log -Recurse | Remove-Item -Force
Get-ChildItem -Path . -Filter *.tmp -Recurse | Remove-Item -Force
```

### 2. Pruning Docker Resources
If you encounter disk space issues or networking conflicts:

```bash
docker system prune -f
docker volume prune -f
```

### 3. Solution File Synchronization
When adding or removing projects, ensure `QuestFlag.slnx` is updated. Modern Visual Studio handles this, but manual verificaton of the XML structure in `QuestFlag.slnx` is recommended after major refactors.
