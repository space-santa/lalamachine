# Changelog fragments

Anything someone using the app would notice gets a file here, as part of the change itself. `prepare-release.ps1` turns
them into the next entry in `CHANGELOG` and deletes them.

Name the file `<name>.<type>.md`, like `jellyfin-push.feature.md`. The name only has to be different from the others.
The type is one of:

- `feature`: something new, or something that works differently now.
- `bugfix`: something that was broken and works now. These show up after the features, with `Bugfix:` in front.

Each file is one item in the changelog. Write a sentence or two for someone who uses the app, not for someone who reads
the code.

To see what the next entry would look like, run `.\prepare-release.ps1` without a version.
