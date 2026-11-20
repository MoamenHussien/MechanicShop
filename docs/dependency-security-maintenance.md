# Dependency security maintenance

## Scope

Update the existing .NET 10 dependency line to the 10.0.15 security servicing release. Preserve the frontend refresh, user-owned backend/SignalR changes, application behavior, database schema and VPS service limits.

## Changes

- Align the centrally managed ASP.NET Core, SignalR, EF Core, HTTP and Redis integration packages on 10.0.15, including MVC testing.
- Explicitly reference `System.Security.Cryptography.Pkcs` 10.0.15 from Infrastructure. MimeKit 4.17.0 previously resolved its minimum dependency, 10.0.0, which carries known advisories. The direct reference raises that dependency without changing the mail library or its public API.
- Explicitly reference `SSH.NET` 2026.0.0 in the two Testcontainers test projects. The full-solution audit found its vulnerable 2025.1.0 transitive dependency, in addition to the original application build warnings. It remains confined to test projects.
- Pin the Docker build image to SDK 10.0.404 and the runtime image to ASP.NET 10.0.15. Both tags were verified in Microsoft Container Registry. The SDK contains the 10.0.15 runtimes.
- Copy the existing root `.editorconfig` into the Docker build stage, after restore, so the container uses the same analyzer policy as local/CI builds without invalidating the dependency restore cache. The previous Dockerfile omitted this file and emitted default StyleCop warnings.
- Keep NuGet auditing enabled. No NuGet audit warnings or advisories are suppressed.

## Sources

- [Microsoft .NET 10.0.15 release notes](https://github.com/dotnet/core/blob/main/release-notes/10.0/10.0.15/10.0.15.md)
- [NuGet dependency auditing](https://learn.microsoft.com/en-us/nuget/concepts/auditing-packages)
- [SSH.NET patched release advisory](https://github.com/advisories/GHSA-q939-rpr3-3284)

## Verification

- `dotnet restore MechanicShop.slnx --force-evaluate`: passed.
- `dotnet list MechanicShop.slnx package --vulnerable --include-transitive --no-restore --format json`: no reported vulnerable packages in any of the 11 projects.
- `dotnet build MechanicShop.slnx --configuration Release --no-restore`: passed with **0 warnings and 0 errors**. A clean Blazor Release rebuild resolved stale local WebCIL output access errors after the package switch.
- `dotnet test MechanicShop.slnx --configuration Release --no-build --no-restore`: **688 passed, 0 failed, 0 skipped**. This includes 208 Domain, 30 Application unit, 317 Application subcutaneous and 133 API integration tests. Integration tests used disposable SQL Server containers.
- Test reports: `artifacts/security-maintenance/tests/*.trx` (ignored build artifacts).
- Local Release smoke test: demo manager sign-in and dashboard passed, with no browser warnings/errors or failed resource requests.
- Release publish output: `artifacts/security-maintenance/publish`; publish completed successfully.
- `docker build --pull --progress=plain -t mechanicshop-api:security-20261117 .`: passed, including Linux restore and Release publish. The final build used the repository's existing analyzer configuration.
- `docker run --rm --network none --entrypoint dotnet mechanicshop-api:security-20261117 --list-runtimes`: confirmed **Microsoft.NETCore.App 10.0.15** and **Microsoft.AspNetCore.App 10.0.15**. The temporary container did not start the application or contact a database.
- Pkcs resolves to 10.0.15 in the package graph. Where publishing uses the shared framework's Pkcs assembly instead of a separate application copy, the verified 10.0.15 runtime supplies the patched assembly.

The application's existing MediatR license message is separate from NuGet security advisories; no licensing configuration was changed.

## Deployment

These changes are local. Building the image does not deploy or restart the VPS. After committing and transferring the dependency update to a deployment branch, rebuild its application image with `docker compose build --pull api` and deploy through the existing process. The pinned .NET patch and SDK image tags must be updated during future security maintenance.
