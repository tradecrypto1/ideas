# Code Signing (Authenticode)

Signing the executable with a **code-signing certificate** (Authenticode) reduces antivirus false positives and shows "Verified publisher" in Windows. Unsigned builds are common for open-source apps; signing is optional but recommended for distribution.

## Options

| Option | Cost | Use case |
|--------|------|----------|
| **Commercial cert** | ~$100–400/year | Public releases; best AV trust. Buy from DigiCert, Sectigo, SSL.com, etc. |
| **Self-signed cert** | Free | Testing or internal use. Windows will show "Unknown publisher" unless users install your cert as trusted. |
| **No signing** | Free | Builds work; some AVs may flag. See [VIRUSTOTAL_FALSE_POSITIVES.md](VIRUSTOTAL_FALSE_POSITIVES.md). |

## Prerequisites for signing

- **Windows** (signtool is part of Windows SDK, or use Visual Studio Installer → Windows SDK).
- **Certificate**: either a `.pfx` file (e.g. from your CA) or a cert in the machine/user store.

Locate `signtool.exe` (e.g. `C:\Program Files (x86)\Windows Kits\10\bin\<arch>\signtool.exe`).

## Signing with build.ps1

When publishing, you can sign the output exe by setting environment variables before running the build:

```powershell
# Optional: path to .pfx and password (avoid committing these)
$env:CLAUDE_INSTALLER_SIGN_PFX = "C:\path\to\your-cert.pfx"
$env:CLAUDE_INSTALLER_SIGN_PASSWORD = "your-pfx-password"

.\build.ps1 -Publish
```

If both are set, the script will run `signtool sign` on `artifacts\winforms\ClaudeCodeInstaller.WinForms.exe` after publish.

**CI (e.g. GitHub Actions):** Store the .pfx (base64) and password as secrets, decode the pfx in the workflow, set the env vars, then run `.\build.ps1 -Publish` and sign the exe in a later step (or use a separate signing step with signtool).

## Signing manually (signtool)

After building/publishing, sign the exe yourself:

```powershell
# Using a .pfx file
signtool sign /f "C:\path\to\cert.pfx" /p "password" /tr http://timestamp.digicert.com /td sha256 /fd sha256 "artifacts\winforms\ClaudeCodeInstaller.WinForms.exe"

# Using cert in store (by thumbprint)
signtool sign /sha1 <thumbprint> /tr http://timestamp.digicert.com /td sha256 /fd sha256 "artifacts\winforms\ClaudeCodeInstaller.WinForms.exe"
```

**Timestamping** (`/tr` / `/td`) is recommended so the signature remains valid after the cert expires.

## Verify signature

```powershell
signtool verify /pa "artifacts\winforms\ClaudeCodeInstaller.WinForms.exe"
```

Or right-click the exe → Properties → Digital Signatures.

## Security notes

- Never commit `.pfx` files or passwords to the repo.
- In CI, use secrets (e.g. `SIGN_PFX_BASE64`, `SIGN_PASSWORD`); decode the pfx to a temp file, sign, then delete.
- Use a dedicated code-signing cert (not an SSL cert) from a public CA for best recognition.
