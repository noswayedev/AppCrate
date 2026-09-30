# AppCrate

**Check your applications, click Install.** AppCrate is a small Windows installer in one 4.exe': it displays a catalog of popular applications, you check the cells you want, and it installs them all silently using [winget](https://learn.microsoft.com/windows/package-manager/).

Ideal for preparing a new or reformatted PC in just a few shots.

## Features

- Catalog of 75 applications classified by categories, each with **its description**
- **Logos** applications (downloaded on first launch, then cached)
- **10 languages**: French, English, Español, Italiano, Deutsch, Português, Русский, 中文, 日本語, 한국어 — choice at first launch, modifiable in **Settings**
- Detection of applications **already installed** (green dot on the logo)
- Instant search ('ctrl+F'), filter by category, "check all" by category
- **Export/import selection** to reuse your list on another PC
- Silent installation in series, with status by application, progress, undo and log deleted
- Modern dark interface, no dependencies: only one '.exe'

## Prerequisites

- Windows 10 (1809+) or Windows 11
- [winget](https://learn.microsoft.com/windows/package-manager/) (preinstalled on recent versions, otherwise "Application Installer" in the Microsoft Store)

## Compilation

No tools to install: the script uses the C# compiler built into Windows.

## Add an app

In 'AppCrate.cs', added a row to the 'DATA' list:

'''
"Name|Identifier.winget|website.com"
'''

Then added its description to each file ʼaxlang/xx.txtʼ:

'''
app.identifiant.winget=Short description
'''

The identifier is with 'winget search name'. The website is only used to preserve the logo.

## Logos and brands

Logos are **not** included in this repository: they are retrieved at runtime from public favicon services and stored in 'C%LocalAppData%\AppCrate\icons'. Names and logos belong to their respective owners. AppCrate is not affiliated with any of the logics listed.

## License

[MIT] (LICENSE)