 - ### Step 1:
 -    - Authenticate (if this is the first time) Note you must also pass --store-password-in-clear-text on non-Windows systems.
```bash
$ dotnet nuget add source https://nuget.pkg.github.com/deadmann/index.json -n github -u deadmann -p GH_TOKEN [--store-password-in-clear-text]
```
 -   - if already exists....

```bash
$ dotnet nuget remove source github
```
 -    - // if key update is required: https://github.com/settings/tokens

 - ### Step 2: Build
```bash
$ dotnet build --configuration Release
```

 - ### Step 3: Pack
```bash
$ dotnet pack --configuration Release
```

 - ### Step 4: Publish
```bash
$ dotnet nuget push "bin/Release/AdvancedRestHandler.1.0.0.nupkg" --source "github"
```
// OR (Based on nuget.config Configuration)
```bash
$ dotnet nuget push "bin/Release/AdvancedRestHandler.1.6.0.nupkg" --source "github" --api-key=[GH_AccessToken]
```
// OR (With %Key%)
```bash
set GITHUB_USERNAME=your_username
set GITHUB_TOKEN=your_token
$ dotnet nuget push "bin/Release/AdvancedRestHandler.1.6.0.nupkg" --source "github"
```