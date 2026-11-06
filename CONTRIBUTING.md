# Contributing

Thanks for helping out. Bug reports and ideas go in [issues](https://github.com/crazyrabbit0/C-Sharp/issues/new/choose); fixes and new programs go in pull requests.

## Pull requests

1. Keep each pull request to one script or one change.
2. Use Allman braces, `PascalCase` methods and `camelCase` fields, and wrap `Process.Start` and file access in `try`/`catch`.
3. Compile with `compile.bat` or `csc.exe` without errors or warnings (CI builds with `/warnaserror`), then run the program and check every menu item.
4. Update the [README](README.md) when you add a program or change what one does, and commit the rebuilt `.exe` with its source.
5. Describe how you tested it, including your Windows version.

These are personal single-file C# utilities that change real systems, so explain anything that edits the registry, services, files or permissions.

## License

By contributing you agree that your changes are licensed under [GPL-3.0](LICENSE).
