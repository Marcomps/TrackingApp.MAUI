# GitHub Copilot Agent Instructions: Tech Debt Identification Guide

**Prompt Version**: 1.0.0

Welcome, Copilot Agent! Use this guide to identify and document technical debt in this solution.

## 1. Scope
Scan the entire repository starting at the root directory. Ignore these paths and file types:

- **Directories**:
    in/, obj/, 
ode_modules/, dist/, .git/, .vs/, .idea/, coverage/, TestResults/, Migrations/
- **File extensions**:
    .dll, .exe, .zip, .png, .jpg

## 2. Outputs

### Task Backlog (.github/agent/tasks/0-backlog/)
Create **TODO-AGENT.md** to capture:

- Obvious cleanup gaps and tech debt
- Missing or failing tests
- In-code TODO comments
- Refactoring, optimization, or cleanup needs

Use this format for each task:
- [ ] - - Task description

Group tasks by priority: High, Medium, Low.

## 3. Document Standards
Every generated markdown file must include:
    - **Updated:** timestamp in YYYY-MM-DD HH:mm Z format
    - A **TL;DR** section at the top summarizing the document
    - No secrets or sensitive data (redact or remove)

## 4. Special Notes for TODO-AGENT.md
When documenting tasks, ensure to:
- Write clear, actionable tasks
- Organize tasks by priority (high, medium, low) and order them if applicable.
- Note any known issues or limitations
- Include all TODO comments found in code
- Include any known issues or limitations that need to be addressed.
- If you find any code that needs to be refactored, include it in this file.
- If you find any code that needs to be tested, include it in this file.
- If you find any code that needs to be optimized, include it in this file.
- If you find any code that needs to be cleaned up, include it in this file.


## 5. Workflow Steps
1. **Scan** the repo using using the criteria above to identify tech debt and tasks.
2. **Generate** all docs listed above, do not skip any, pay special attention to the 4. Special Notes for TODO-AGENT.md section.
3. **Stop** after generating the files. Do NOT start enhancements.

## 6. Final Step
Once the above is complete, create a summary log:
.github/agent/summaries/AGENT-DISCOVER_TECH-DEBT_SUMMARY_COMPLETED_<yyyy-mm-dd>.md
This summary should recap all the tasks you identified.

## 7. Validation Checklist (Agent must self-check before finishing)

â˜ **Scope respected**. Scan started at repo root, and **ignores applied** for directories: in/, obj/, 
ode_modules/, dist/, .git/, .vs/, .idea/, coverage/, TestResults/, Migrations/; and file types: .dll, .exe, .zip, .png, .jpg.
â˜ **No scanned content from ignored paths/types** was used in outputs.

### Outputs present in .github/agent/tasks/0-backlog/
â˜ TODO-AGENT.md exists in the correct path.
â˜ File includes sections grouped by **priority**: High, Medium, Low.
â˜ Each task follows required format: - [ ] - - Task description.
â˜ Captures all **required categories**:
    - Obvious cleanup gaps and tech debt
    - Missing or failing tests
    - In-code TODO comments
    - Refactoring needs
    - Optimization needs
    - Cleanup needs

### Document Standards (apply to TODO-AGENT.md)
â˜ Contains **Updated**: timestamp in **YYYY-MM-DD HH:mm Z** format.
â˜ Begins with a concise **TL;DR** (2â€“3 sentences) summarizing the document.
â˜ **No secrets/sensitive data** included; redact/remove if discovered.
â˜ Tasks are clear, actionable, and repository-specific.

### Special Notes for TODO-AGENT.md
â˜ Tasks are explicitly written, actionable, and organized under High, Medium, Low priority.
â˜ All **TODO** comments in the codebase are included.
â˜ Known issues or limitations are listed.
â˜ Any **refactoring**, **testing**, **optimization**, or **cleanup** opportunities are documented.
â˜ Redundant tasks consolidated; no duplicates.

### Workflow adherence
â˜ **Scan** phase completed: repo reviewed for cleanup gaps, TODOs, failing/missing tests, refactor/optimize/cleanup opportunities.
â˜ **Generate** phase completed: TODO-AGENT.md created with all required elements, respecting **Special Notes**.
â˜ **Stop** phase honored: **no enhancements** or non-requested content created.

### Final Step (summary log)
â˜ Created summary log at: .github/agent/summaries/AGENT-DISCOVER_TECH-DEBT_SUMMARY_COMPLETED_<yyyy-mm-dd>.md.
â˜ Summary log **recaps**: tech debt categories identified, tasks generated, and any key observations.
â˜ <yyyy-mm-dd> matches the current date used in the **Updated** timestamp.

### Output hygiene
â˜ **Only** the specified files were created in the specified locations.
â˜ Filenames and paths **match** exactly (including case, underscores, and hyphens).
â˜ Markdown renders cleanly (no broken lists, tables, or code fences).
â˜ Task backlog is concise, scannable, and free of formatting errors.

### Optional quality checks (nice to have)
â˜ Priority levels balanced realistically (not everything High).
â˜ Tasks are ordered logically within each priority group.
â˜ Cross-references to code files (e.g., path/to/file.cs) provided where useful.
â˜ Summary log highlights the most critical areas for human review.
