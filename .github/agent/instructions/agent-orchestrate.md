# GitHub Copilot Agent Instructions: Orchestrate Source Backlog into Manageable Tasks

**Prompt Version**: 1.0.0

Welcome, Copilot Agent! Use this guide to break down a source backlog into manageable, prioritized tasks for implementation.

## 1 Inputs
1. Source Backlog: .github/agent/tasks/0-backlog/TODO-AGENT.md
2. Priority Sections: ## High Priority Tasks, ## Medium Priority Tasks, etc.
3. Coding Rules: .github/agent/inputs/coding-rules.md (if exists)
4. Additional Context: .github/agent/knowledge/*.md

## 2 Rules
1. Slice any item that likely needs more than two hours or touches more than two modules
2. Prefer tasks that change up to 400 lines of code per pull request
3. Create subtasks with Title; Source; Unique ID; Priority; Deliverable; Paths/Artifacts; Dependencies; Definition of Done checklist
4. Mark items already created with [o] and completed with [x]
5. Append unclear questions to .github/agent/tasks/feedback.md and notify the user

## 3 Steps
1. Read the file at Source Backlog
2. Ignore items marked [o] or [x]
3. For each new slice perform the following:
    - Generate an eightâ€‘character unique ID as <uniqueId>
    - Set priority as <priority> (high, medium, low)
    - Set dependsOn value to y or 
 as <dependsOn>
    - Create or refresh .github/agent/tasks/1-open/<uniqueId>_<priority>_<dependsOn>_<short-slug>_<yyyy-mm-dd>.md
    - Update Source Backlog by marking the item [o] and tagging it with <uniqueId>, e.g. - [o] - <uniqueId> - Task description
4. If unclear, append questions to .github/agent/tasks/feedback.md
5. Commit all files you created or updated.

## 4 Output Template
Title
---
**Source**: .github/agent/tasks/0-backlog/TODO-AGENT.md
**Unique ID**: 01abc12z
**Priority**: High
**Epic**: Code Structure & Architecture
**Deliverable**: PR + tests
**Paths**: src/Domain/*, src/Application/*
**Depends On**: none
**Updated**: 2023-10-23 12:00 UTC

DoD
---
- [ ] ...
- [ ] ...

## 5 Finally
1. After completing all tasks create .github/agent/summaries/AGENT-ORCHESTRATE_SUMMARY_COMPLETED_<yyyy-mm-dd>.md summarizing the tasks you created

## 6. Validation Checklist (Agent must self-check before finishing)

â˜ **Inputs confirmed**. Verified access to:
    - .github/agent/tasks/0-backlog/TODO-AGENT.md (source backlog)
    - Priority sections ## High Priority Tasks, ## Medium Priority Tasks, etc. exist and were used.
    - .github/agent/inputs/coding-rules.md checked if present.
    - .github/agent/knowledge/*.md referenced for additional context.
â˜ **Ignored correctly**: Items already marked [o] (open) or [x] (completed) were not reprocessed.

### Rules application
â˜ Large items (>2 hours work or spanning >2 modules) sliced into smaller tasks.
â˜ Each task sized to ~400 LOC max per PR.
â˜ Subtasks include **all required metadata**: Title, Source, Unique ID, Priority, Deliverable, Paths/Artifacts, Dependencies, DoD.
â˜ Unique IDs are exactly **8 characters**.
â˜ [o] marks applied correctly in TODO-AGENT.md with the <uniqueId> tag.
â˜ Questions/unclear points appended to .github/agent/tasks/feedback.md and user notified.

### Task files
â˜ Each new task file created under: .github/agent/tasks/1-open/.
â˜ File names follow exact convention: <uniqueId>_<priority>_<dependsOn>_<short-slug>_<yyyy-mm-dd>.md
â˜ File content matches Output Template structure:
    - Title line
    - **Source, Unique ID, Priority, Epic, Deliverable, Paths, Depends On, Updated**
    - **DoD checklist**
â˜ **Updated timestamp** in YYYY-MM-DD HH:mm Z format.
â˜ Tasks grouped under correct **priority** (**high**, **medium**, **low**).
â˜ DependsOn field correctly set (y or 
).

### Workflow adherence
â˜ Step 1â€“5 followed exactly: backlog read, items sliced, tasks generated, backlog updated, questions appended, all changes committed.
â˜ No skipped required steps; no enhancements outside instructions.

### Final step
â˜ Summary log created at: .github/agent/summaries/AGENT-ORCHESTRATE_SUMMARY_COMPLETED_<yyyy-mm-dd>.md
â˜ Summary log **recaps**: number of tasks created, their IDs, priorities, dependencies, and any feedback items raised.
â˜ <yyyy-mm-dd> matches the date used in all task file timestamps.

### Output hygiene
â˜ **Only** specified files created/modified.
â˜ Filenames, paths, and casing match instructions exactly.
â˜ Markdown renders cleanly (headings, checklists, code fences intact).
â˜ Source backlog updated without corrupting structure.

### Optional quality checks (nice to have)
â˜ Tasks are evenly sized for predictable throughput.
â˜ Task slugs are short, descriptive, and consistent.
â˜ DoD checklists use concrete, testable criteria.
â˜ Cross-links between tasks (dependencies) are consistent and accurate.
