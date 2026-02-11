- [x] You are running on a Windows 11 machine
- [x] Create all src code and folders in an organized way
- [x] Create a .md file for each source code file (in `docs/`); update md files when changes occur
- [x] Build test-driven development docs/tests
- [x] Create conventional commit GitHub .md file for agent (docs/CONVENTIONAL_COMMITS.md + .cursor/rules/conventional-commits.mdc)
- [x] Build the GitHub pipeline (build, tests, Docker build; deploy job ready in .github/workflows/ci.yml)
- [x] build healthcheck
- [x] Build a C# application that makes it stupid easy to install claude code on a Windows 11 computer.
- [x] each time there is a check in create and RC version tag it build it and upload to github ACR
- [x] build a winforms applicaton to download and isntall claude code.  the installer will check to see if there is a newer version out there.  if there is uninstall curent and install newer version.
- [x] the installer is also a runner it will run claude code for the user not need to do stupid stuff to get it running

cd C:\files\repos15\Projects\ideas && powershell -ExecutionPolicy Bypass -File .\build.ps1 -Publish