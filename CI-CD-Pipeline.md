# CI/CD Workflow Documentation

## Overview
Automated pipeline for Unity 2021.3.4f1 project that runs tests, builds Windows executables, and creates releases.

## Workflow Triggers
- **Push** to `main` or `develop` branches
- **Pull Request** to `main`
- **Tags** matching `v*.*.*` (creates release)
- **Manual** via workflow_dispatch

## Required Secrets
Configure in Settings → Secrets → Actions:
- `UNITY_EMAIL` - Unity account email
- `UNITY_PASSWORD` - Unity account password  
- `UNITY_LICENSE` - Unity license file content (.ulf)

## Jobs

### 1. Test
- Runs EditMode and PlayMode tests
- Uploads test results as artifacts
- Uses memory leak detection disabled to avoid Docker issues

### 2. Build
- Builds Windows x64 executable
- Version from git tag (v1.2.3) or ProjectVersion.txt
- Creates zip archive with build number
- Runs even if tests fail

### 3. Release (tag only)
- Creates GitHub Release for version tags
- Uploads build zip to release assets

## Version Management
```bash
# Create release
git tag -a v1.0.0 -m "Release version 1.0.0"
git push origin v1.0.0
```

## Artifacts
- **Test Results**: `test-results/`
- **Build**: `ClockApp-{version}-build{number}.zip`

## Local Testing
Run tests locally before pushing:
```
Window → General → Test Runner → Run All
```

## Troubleshooting

### License Issues
- Ensure all three secrets are set correctly
- License must be Personal or Pro/Plus
- Re-activate if builds fail with license errors

### Memory Errors
- ProfilerEditor memory leaks are disabled via `-disableManagedDebugger`
- Docker cleanup runs before tests to free disk space