# File Bundler CLI

A professional command-line tool built with C# and .NET to aggregate source code files into a single organized text file. Designed to streamline code reviews, backups, and project snapshots.

## Key Features

*   Recursive Bundling: Scans directories and subdirectories to collect files into one output.
*   Language Filters: Specify file extensions (e.g., cs, js) or use all.
*   Response File Support: Create .rsp files to save complex configurations and run them with a single command: @filename.rsp.
*   Advanced Formatting: Includes options to add source paths as comments, sort files, and remove empty lines.
*   Robust Error Handling: Automatically skips restricted system folders to prevent access-denied crashes.

## Quick Start

1.  Publish the project:
    dotnet publish -c Release -o ./publish

2.  Create a configuration file:
    p1 create-rsp

3.  Run the bundler:
    p1 @myconfig.rsp

## Tech Stack
*   Language: C#
*   Framework: .NET Core
*   Library: System.CommandLine
