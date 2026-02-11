# VirusTotal / Antivirus False Positives

Builds of **ClaudeCodeInstaller.WinForms.exe** may be flagged by some antivirus or VirusTotal vendors (e.g. "Win64:Malware-gen", "Trojan.Win64.Agent", "AIDetectMalware"). **These are false positives.**

## Why this happens

- **New or low-prevalence files** – Heuristic and ML-based engines often flag executables they haven’t seen before.
- **Single-file, self-contained .NET** – The published exe bundles the .NET runtime and looks “packed,” which can trigger generic detections.
- **Behavior** – The app starts other processes (PowerShell, `claude`, npm, winget, etc.), which can match “launcher” or “agent” heuristics.

Detections like `Win64:Malware-gen` and `Trojan.Win64.Agent` are **generic/heuristic**, not signatures of known malware.

## What you can do

- **Build from source** – Clone the repo and run `.\build.ps1 -Publish`. You can review the code; it’s MIT-licensed.
- **Add an exclusion** – If you trust the source, add the exe or folder to your AV’s exclusion list.
- **Report false positives** – On VirusTotal, use “Submit for reanalysis” or contact the flagging vendor; linking to this repo helps.

## What we do

- **Open source** – Full source is in this repo for audit.
- **CI** – GitHub Actions builds and runs tests; optional Windows Defender scan in CI.
- **Code signing** – You can sign the exe with an Authenticode certificate to reduce false positives. See [CODE_SIGNING.md](CODE_SIGNING.md) for how to sign (build script supports it via env vars).

## Contact

If you’re an AV vendor and need a sample or more context for whitelisting, open an issue in this repository.
