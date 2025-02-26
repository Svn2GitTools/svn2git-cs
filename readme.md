# svn2git-cs: A C# SVN to Git Migration Tool

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![GitHub release](https://img.shields.io/github/v/release/Svn2GitTools/svn2git-cs)](https://github.com/Svn2GitTools/svn2git-cs/releases/latest)

**svn2git-cs** is a command-line tool written in C# to migrate Subversion (SVN) repositories to Git. It's inspired by the original `svn2git` Ruby project and aims to provide a robust and user-friendly solution for SVN to Git conversion using the power of `git svn` under the hood.

## Features

* **SVN to Git Conversion:** Migrates your SVN repository, including history, branches, and tags, to a Git repository.
* **Inspired by `svn2git`:**  Adopts the proven approach and options of the well-regarded `svn2git` Ruby project.
* **Written in C#:** Offers a native Windows experience and potentially better performance in certain environments compared to Ruby-based tools.
* **Command-Line Interface:** Easy to use from your terminal or command prompt.
* **Handles Standard SVN Layouts:** Supports common SVN repository layouts with trunk, branches, and tags directories.
* **Customizable SVN Paths:** Allows specifying custom paths for trunk, branches, and tags.
* **Authors Mapping:** Supports mapping SVN authors to Git authors using an authors file.
* **Revision Range:**  Allows migrating a specific revision range from SVN.
* **Metadata Inclusion:** Optionally includes `git-svn-id` metadata in Git commit logs.
* **Branch and Tag Handling:** Correctly converts SVN branches and tags to Git branches and tags.
* **Rebase Functionality:** Supports rebasing an existing Git repository against an SVN repository for incremental migration.
* **Branch Rebase:** Allows rebasing a specific branch from SVN into an existing Git repository.
* **Repository Optimization:** Optimizes the Git repository after migration (using `git gc`).
* **Verbose Output:** Provides detailed logging for debugging and monitoring the migration process.

## Disadvantages
- **Performance:** The migration process can be slow, especially for large SVN repositories with extensive histories. On my computer for 466 commits I need about 30mins
- **Git History Accuracy:** While the tool aims to preserve history, there may be discrepancies or issues with the accuracy of the migrated Git history.
- **Dependency on `git svn`:** Relies on `git svn`, which may have its own limitations and bugs that can affect the migration process.
- **Platform Limitations:** Although written in C#, the tool may have limitations or require additional setup on non-Windows platforms.
- **Manual Intervention:** Some aspects of the migration, such as authors mapping, may require manual intervention and adjustments.

## Getting Started

### Prerequisites

* **Git:** Ensure Git is installed and accessible in your system's PATH.
* **`git svn`:**  `svn2git-cs` relies on `git svn`, which is usually included with Git for Windows or can be installed separately on other systems.
* **.NET Runtime:** You need the .NET 9.0 and above runtime to execute the compiled `svn2git-cs` tool.

### Installation

1. **Download the latest release:** Go to the [Releases](link-to-your-releases-page) page of this GitHub repository and download the latest compiled binary (e.g., `svn2git-cs.exe`).
2. **Extract the archive:** Extract the downloaded archive to a directory of your choice.
3. **Add to PATH (Optional):** For easier access, add the directory containing `svn2git-cs.exe` to your system's PATH environment variable.

### Usage

Open your command prompt or terminal and navigate to the directory where you want to create your Git repository (or where your existing Git repository is if using `--rebase`).

Run `svn2git-cs` with the SVN repository URL as the first argument.

```bash
svn2git-cs SVN_URL [OUTPUT_DIR] [options]
```

* **`SVN_URL`**:  The URL of your Subversion repository.
* **`OUTPUT_DIR` (Optional)**: The directory where the new Git repository will be created. If omitted, the Git repository will be created in the current directory with the name derived from the SVN repository URL.

**Example:**

To convert an SVN repository at `http://svn.example.com/repos/myproject` to a Git repository named `myproject` in the current directory:

```bash
svn2git-cs http://svn.example.com/repos/myproject
```

To convert to a specific output directory:

```bash
svn2git-cs http://svn.example.com/repos/myproject output-git-repo
```

## Options

`svn2git-cs` supports various options to customize the migration process. You can see the full list of options by running:

```bash
svn2git-cs --help
```

Here's a summary of available options:

```
Usage: svn2git-cs SVN_URL [OUTPUT_DIR] [options]

Specific options:
  --rebase                        Instead of cloning a new project, rebase an existing one against SVN
  --createauthors EMAIL_DOMAIN    Create authors file
  --username NAME                 Username for transports that needs it (http(s), svn)
                                  Password for transports that need it (http(s), svn), must be set over client credential
  --trunk TRUNK_PATH              Subpath to trunk from repository URL (default: trunk)
  --branches BRANCHES_PATH        Subpath to branches from repository URL (default: branches); can be used multiple times
  --tags TAGS_PATH                Subpath to tags from repository URL (default: tags); can be used multiple times
  --rootistrunk                   Use this if the root level of the repo is equivalent to the trunk and there are no tags or branches
  --notrunk                       Do not import anything from trunk
  --nobranches                    Do not try to import any branches
  --notags                        Do not try to import any tags
  --no-minimize-url               Accept URLs as-is without attempting to connect to a higher level directory
  --revision START_REV[:END_REV]  Start importing from SVN revision START_REV; optionally end at END_REV
  -m, --metadata                  Include metadata in git logs (git-svn-id)
  --authors AUTHORS_FILE          Path to file containing svn-to-git authors mapping (default: authors.txt)
  --exclude REGEX                 Specify a Perl regular expression to filter paths when fetching; can be used multiple times
 -v, --verbose                   Be verbose in logging -- useful for debugging issues
  --rebasebranch REBASEBRANCH    Rebase specified branch.

  -h, --help                      Show this message
```

**Common Options:**

* `--rebase`: Rebase an existing Git repository against the SVN repository. Useful for incremental migrations.
* `--createauthors EMAIL_DOMAIN`: Creates an `authors.txt` file to map SVN usernames to Git author names and emails. You need to provide the email domain to construct email addresses.
* `--authors AUTHORS_FILE`: Specifies the path to your authors mapping file. Defaults to `authors.txt`.
* `--trunk TRUNK_PATH`, `--branches BRANCHES_PATH`, `--tags TAGS_PATH`:  Customize the paths to trunk, branches, and tags within your SVN repository if they deviate from the standard layout. You can use `--branches` and `--tags` multiple times to specify multiple branch and tag paths.
* `--revision START_REV[:END_REV]`: Migrate only a specific revision range.
* `--verbose` or `-v`: Enable verbose output for more detailed logging.
* `--rebasebranch REBASEBRANCH`: Rebase a specific branch from SVN into your Git repository.

## Authors Mapping (`authors.txt`)

To ensure correct author attribution in Git, you can create an `authors.txt` file to map SVN usernames to Git author names and email addresses.

**Example `authors.txt` format:**

```
svn_username1 = Git Name 1 <git_email1@example.com>
svn_username2 = Git Name 2 <git_email2@example.com>
```

You can generate a template `authors.txt` file using the `--createauthors` option:

```bash
svn2git-cs --createauthors example.com SVN_URL
```

This will create an `authors.txt` file in your current directory. Edit this file to fill in the correct Git author names and email addresses for each SVN username.

## Contributing

Contributions are welcome! Please feel free to submit bug reports, feature requests, or pull requests.

1. Fork the repository.
2. Create a new branch for your feature or bug fix.
3. Make your changes and commit them.
4. Submit a pull request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgements

This project is inspired by and conceptually based on the original [svn2git](https://github.com/svn2git/svn2git) Ruby project. We thank the authors of `svn2git` for providing the foundation and inspiration for this tool.
