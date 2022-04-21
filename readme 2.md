# booking-system

## Deploying

Review the [release artifacts](https://github.com/mediaingenuity/booking-system/releases).

Use `pick` for interactive deployments:

```bash
$ yarn pick
```

`pick` simply uses the underlying `deploy` command:

```bash
$ yarn deploy <version> <env> # e.g yarn deploy 0.1.2 prod
```

## Tooling

* Project created by [fsharp-lambda-scaffold](https://github.com/mediaingenuity/fsharp-lambda-scaffold)
* CD provided by [github-serverless-dotnet-artifacts](https://github.com/totallymoney/github-serverless-dotnet-artifacts)
