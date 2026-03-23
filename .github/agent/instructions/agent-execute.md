# GitHub Copilot Agent Instructions: Execute Task Procedures

**Prompt Version**: 1.0.0

Welcome, Copilot Agent! Use this guide to identify and implement a single task from your open tasks.

## 1 Goal
Implement a single task defined in a subtask file under .github/agent/tasks/1-open/â€¦ and deliver it.
Prefer the file specified in the prompt, however if none is specified select one with no dependencies and high priority.

## 2 Inputs
1. Subtask spec file: path to the subtask you're executing, .github/agent/tasks/1-open/abc123yz_high_n_remove-placeholder-files_2025-07-23.md
2. Coding rules: .github/agent/inputs/coding-rules.md (if exists)
3. Domain context: .github/agent/knowledge/*.md

## 3 Steps
1. **Move the Task**: Move the subtask file to .github/agent/tasks/2-work/
2. **Load & Understand**:
    - **Read** the subtask file
    - **Extract**:
        - Title
        - Source
        - Unique ID
        - Deliverable
        - Paths to touch
        - Dependencies
        - DoD checklist
3. **Plan Work**:
    - **Sketch** the smallest change set satisfying the DoD
    - **Ensure** changes stay â‰¤400 LOC and span â‰¤2 modules
    - **Document** the plan by appending to the task file
4. **Execute Work**:
    - **Apply** the plan
    - **Update** each DoD item to [x] when completed
5. **Validate**:
    - **Run** build and tests locally
    - **Confirm** DoD items pass (tests green, docs build, linters)
6. **Update Task Status**:
    - **Add** at the top of the subtask file:
        - Status: Done
        - PR: #<PR number>
        - Updated: YYYYâ€‘MMâ€‘DD HH:mm TZ
7. **Update the Source File**: Change the [o] to [x] on the matching Unique ID in the **Source** file.
8. **Update Knowledge**: Update relevant files in .github/agent/knowledge/ with what was learned
9. **Move the Task**: Move the file to .github/agent/tasks/3-completed/

## 4 Finally
1. After finishing, create .github/agent/summaries/AGENT-EXECUTE_SUMMARY_COMPLETED_<yyyy-mm-dd>.md summarizing the tasks executed

## 5. Validation Checklist (Agent must self-check before finishing)

### Goal & Inputs
â˜ Executed **only one subtask** from .github/agent/tasks/1-open/.
â˜ If no subtask specified, selected one with **High priority** and **no dependencies**.
â˜ Verified access to:
    - Subtask spec file (path under .github/agent/tasks/1-open/â€¦).
    - .github/agent/inputs/coding-rules.md if present.
    - .github/agent/knowledge/*.md for domain context.

### Steps

#### 1. Move the Task
â˜ Subtask file moved from 1-open â†’ .github/agent/tasks/2-work/.
â˜ Original 1-open directory contains no duplicate copies.

#### 2. Load & Understand
â˜ Subtask file was **read in full**.
â˜ Extracted metadata correctly: **Title, Source, Unique ID, Deliverable, Paths, Dependencies, DoD checklist**.
â˜ Metadata consistent with file name and backlog entry.

#### 3. Plan Work
â˜ Work plan appended to the task file.
â˜ Change set designed â‰¤400 LOC and â‰¤2 modules.
â˜ Plan directly maps to each DoD checklist item.

#### 4. Execute Work
â˜ Changes applied exactly as described in plan.
â˜ DoD checklist updated to [x] per item when completed.
â˜ Code aligns with .github/agent/inputs/coding-rules.md (if exists).

#### 5. Validate
â˜ Build runs successfully.
â˜ All tests pass.
â˜ Linters and docs build succeed.
â˜ DoD checklist confirmed fully satisfied.

#### 6. Update Task Status
â˜ Added status lines at top of subtask file:
    - Status: Done
    - PR: #<PR number>
    - Updated: YYYY-MM-DD HH:mm TZ (exact format).

#### 7. Update Source File
â˜ Source backlog updated: [o] changed to [x] for the matching Unique ID.
â˜ Backlog formatting preserved.

#### 8. Update Knowledge
â˜ Relevant .github/agent/knowledge/ files updated with new learnings.
â˜ No secrets or sensitive data introduced.

#### 9. Move the Task
â˜ Task file moved to: .github/agent/tasks/3-completed/.
â˜ File name and metadata consistent with completion state.

### Final Step
â˜ Summary log created at: .github/agent/summaries/AGENT-EXECUTE_SUMMARY_COMPLETED_<yyyy-mm-dd>.md.
â˜ Summary log **recaps**: task executed, PR reference, status, and any knowledge updates.
â˜ <yyyy-mm-dd> matches Updated timestamps.

### Output Hygiene
â˜ Only specified files moved/modified.
â˜ Filenames and casing match exactly.
â˜ Markdown renders cleanly (no broken lists or metadata).
â˜ Source backlog and knowledge base remain intact and valid.

### Optional Quality Checks (nice to have)
â˜ PR contains small, reviewable changes (â‰¤400 LOC).
â˜ Knowledge updates link back to the completed task ID.
â˜ Completed task includes reference to test coverage or verification steps.
â˜ Summary log highlights any follow-ups or remaining open questions.
