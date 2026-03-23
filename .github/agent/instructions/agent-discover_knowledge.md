# GitHub Copilot Agent Instructions: Code Discovery & Documentation Guide

**Prompt Version**: 1.0.0

Welcome, Copilot Agent! Use this guide to explore and document the code in this solution.

## 1. Scope
Scan the entire repository starting at the root directory. Ignore these paths and file types:

- **Directories**:
    in/, obj/, 
ode_modules/, dist/, .git/, .vs/, .idea/, coverage/, TestResults/, Migrations/
- **File extensions**:
    .dll, .exe, .zip, .png, .jpg

## 2. Outputs

### Knowledge Documents (.github/agent/knowledge/)
Generate these markdown files (or update to ensure accuracy if they already exist):

- **README.md**
    - How to leverage this codebase
    - Build, run, and test instructions

- **CODEBASE-OVERVIEW.md**
    - Highâ€‘level purpose and entry points
    - Folder structure overview
    - Key services/controllers and data models

- **DEPENDENCIES.md**
    - NuGet/package list (name, version, usage, purpose)
    - Notable transitive dependencies

- **ARCHITECTURE-MAP.md**
    - Layers (Domain, Application, Infrastructure, Web)
    - Main workflows, interfaces, and extension points

- **MIDDLEWARE-PIPELINE.md**
    - Ordered list of middleware
    - What each middleware does and where it's configured

## 3. Document Standards
Every generated markdown file must include:
    - **Updated:** timestamp in YYYY-MM-DD HH:mm Z format
    - A **TL;DR** section at the top summarizing the document
    - No secrets or sensitive data (redact or remove)

## 4. Workflow Steps
1. **Scan** the repo learning its structure and components to build context and understanding for the files that will be generated above.
2. **Generate** all docs listed above, do not skip any, highlight key areas and provide examples where relevant.
3. **Stop** after generating the files. Do NOT start enhancements.

## 5. Final Step
Once the above is complete, create a summary log:
.github/agent/summaries/AGENT-DISCOVER_KNOWLEDGE_SUMMARY_COMPLETED_<yyyy-mm-dd>.md
This summary should recap all the tasks you performed, the files you generated, and any important notes or observations about the codebase.

## 6. Validation Checklist (Agent must self-check before finishing)
â˜ **Scope respected.** Scan started at repo root, and **ignores applied** for directories: in/, obj/, 
ode_modules/, dist/, .git/, .vs/, .idea/, coverage/, TestResults/, Migrations/; and file types: .dll, .exe, .zip, .png, .jpg.
â˜ **No scanned content from ignored paths/types** was used in outputs.

### Outputs present in .github/agent/knowledge/
â˜ README.md exists and covers: how to leverage the codebase; build, run, and test instructions.
â˜ CODEBASE-OVERVIEW.md exists and covers: high-level purpose and entry points; folder structure overview; key services/controllers and data models.
â˜ DEPENDENCIES.md exists and lists: NuGet/packages (name, version, usage, purpose) and notable transitive dependencies.
â˜ ARCHITECTURE-MAP.md exists and explains: layers (Domain, Application, Infrastructure, Web); main workflows, interfaces, and extension points.
â˜ MIDDLEWARE-PIPELINE.md exists and includes: **ordered** middleware list; what each middleware does; where each is configured.

### Document Standards (apply to every generated markdown file)
â˜ Contains **Updated**: timestamp in **YYYY-MM-DD HH:mm Z** format (e.g., 2025-09-04 16:00 -0500).
â˜ Begins with a concise **TL;DR** (2â€“3 sentences) summarizing the document.
â˜ **No secrets/sensitive data** included; anything sensitive is redacted/removed.
â˜ Uses clear headings, consistent formatting, and repository-accurate terminology.
â˜ Links/paths, if present, are relative and valid.

### Workflow adherence
â˜ **Scan** phase completed: repository structure, components, and relationships learned for context.
â˜ **Generate** phase completed: **all five** knowledge docs produced; no required doc skipped; key areas highlighted with examples where relevant.
â˜ **Stop** phase honored: **no enhancements** or non-requested changes performed beyond generating the docs.

### Final Step (summary log)
â˜ Created summary log at: .github/agent/summaries/AGENT-DISCOVER_KNOWLEDGE_SUMMARY_COMPLETED_<yyyy-mm-dd>.md.
â˜ Summary log **recaps**: tasks performed, files generated, and important notes/observations about the codebase.
â˜ <yyyy-mm-dd> matches the current date used in the docs' **Updated** timestamps.

### Output hygiene
â˜ **Only** the specified files were created in the specified locations; no stray or temporary files committed.
â˜ Filenames and paths match **exactly** (including case and hyphenation).
â˜ Markdown renders cleanly (no broken tables/lists/code fences).
â˜ Any code snippets/examples compile or are clearly marked as pseudocode.

### Optional quality checks (nice to have)
â˜ Cross-references between docs are present where helpful (e.g., README â†’ CODEBASE-OVERVIEW, ARCHITECTURE-MAP).
â˜ Large lists (e.g., dependencies) are organized and deduplicated; versions sourced consistently.
â˜ Middleware order verified against actual configuration code.
