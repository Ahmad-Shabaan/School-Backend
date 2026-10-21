# Task: Complete the Existing Learning Platform Backend

## Role and working rules

Act as a senior ASP.NET Core backend engineer. Complete the learning-platform organization described below **inside the existing repository**.

The project already uses:
- ASP.NET Core Web API
- Clean Architecture
- CQRS
- Unit of Work and Repository Pattern

### Mandatory rules before implementation
1. Inspect the entire existing solution structure, entities, DbContext, configurations, repositories, Unit of Work, CQRS handlers, DTOs, validators, controllers/endpoints, authentication/authorization, migrations, and tests.
2. Identify what already exists and what is incomplete. Extend and refactor the existing implementation; **do not recreate the solution or duplicate existing features**.
3. Preserve the existing architecture, naming conventions, ID types, error/result patterns, mediator library, validation approach, and dependency-injection style.
4. Do not bypass CQRS by injecting `DbContext` or repositories directly into controllers. Controllers/endpoints should delegate to the existing Application layer pattern.
5. Keep domain/business rules in the appropriate Domain/Application layer. Infrastructure owns EF Core persistence and external integrations.
6. Use async APIs and pass `CancellationToken` through the request flow wherever supported.
7. Follow the existing Unit of Work and Repository abstractions. Add methods/specifications only where needed; avoid unnecessary generic repository redesign.
8. Do not invent new architectural patterns or add packages without explaining why they are needed.
9. Do not implement screen-recording/screenshot prevention. This is explicitly out of scope.
10. Do not start by dumping large amounts of code. First provide a concise audit and implementation plan, then implement in small, coherent steps.
11. Build the solution and run relevant tests after changes. Fix regressions caused by the implementation.
12. Never claim a feature is complete unless the code and tests support that claim. Document any remaining limitation.

---

## Product requirements

The platform contains courses. A course contains ordered lessons. A lesson can include:
- A YouTube video URL
- PDF or other downloadable/reference file(s)
- Lesson goals/objectives
- An optional meeting URL and meeting date/time
- A quiz
- Comments and replies

Students are created/managed by an administrator/teacher through the existing user-management flow. Use the project's existing authentication and user identity model; do not assume student IDs are `int` or `Guid` until the repository is inspected.

The required learning rules:
- A student must be enrolled/authorized for a course before accessing its protected content.
- Lessons have a defined order.
- The next lesson is unlocked only when the student has watched at least **90%** of the current lesson's video according to the platform's progress-tracking rules **and** passed that lesson's quiz with at least **80%**.
- These thresholds should be configurable at the appropriate level if this fits the current design; do not scatter magic numbers throughout the code.
- A student's progress must be saved and restored when they return.
- Quiz attempts and scores must be stored per student and attempt.
- Students can comment on lessons and reply to comments. Apply authorization rules so a student cannot edit/delete another student's comment. Teacher/admin moderation should follow existing roles and authorization conventions.
- Real-time comment updates are desirable only if the project already has SignalR or it can be added consistently. Persist comments to the database first; real-time notifications must not replace persistence.

## YouTube requirements and limitations

Use YouTube to host videos. Store the YouTube video URL or, preferably, a validated/extracted YouTube video ID in the database, following the existing model style. The frontend will use the official YouTube IFrame Player API to play videos and observe playback events.

Do not upload or proxy YouTube video files through the backend. Do not store Google credentials or attempt to use YouTube Private videos as if the platform's student login automatically grants YouTube access.

### Important integrity limitation
The YouTube IFrame Player API can report playback state/current time/duration to the frontend, but the backend cannot independently prove that a student genuinely watched the video. Treat progress as **best-effort tracking**, not cryptographic proof. Never state that it is impossible to manipulate.

The frontend should report progress periodically and on relevant player events. The backend must:
- Derive the student identity from the authenticated request, never trust a client-supplied `StudentId`.
- Verify course enrollment and access to the lesson before accepting progress.
- Validate that the lesson exists and the reported duration/time values are reasonable.
- Reject negative values, impossible durations, malformed payloads, and implausible progress jumps according to a documented policy.
- Avoid marking a lesson watched merely because the client sends `IsWatched = true` or one final `CurrentTime` value.
- Calculate progress server-side from accepted progress data.
- Persist progress so it survives logout/re-entry.
- Make update operations idempotent where practical.
- Return the authoritative progress/completion status from the backend.

Do not claim that periodic client reports fully prevent tampering. Document the limitations of YouTube-based tracking.

## Suggested data model — adapt to existing entities

Do not blindly create every entity below if equivalents already exist. Inspect first, then make the minimum necessary schema changes.

### Course and lessons
- `Course`: course metadata and its existing relationships.
- `Lesson`: course foreign key, title/description, order/index, YouTube video URL or video ID, optional meeting URL/date, and any existing publication fields.
- `LessonGoal`: one lesson can have many goals.
- `LessonResource`: optional normalized entity for PDF/files/other lesson resources if the existing schema does not already model resources. Store a file URL/storage key and metadata; do not store raw large file bytes in ordinary text columns.
- If each lesson has at most one quiz, enforce a unique relationship on `Quiz.LessonId`. If the current product model permits multiple quizzes, preserve that design instead.

### Enrollment
Use an enrollment/join entity if one does not already exist, such as:
- Student/user foreign key
- Course foreign key
- Enrollment status and timestamps as appropriate

Enforce uniqueness for the student-course pair unless the existing domain explicitly supports multiple enrollment records.

### Lesson progress
Create or adapt a progress entity, such as `LessonProgress`, with fields appropriate to the current ID types:
- Student/User ID
- Lesson ID
- Last playback position in seconds
- Video duration in seconds, when known
- Watched percentage
- Completion flag (e.g. `IsWatched` or a clearer existing equivalent)
- Completion timestamp
- Last updated timestamp
- Optional concurrency token if useful

Enforce one current progress record per student and lesson using a composite unique key/index. Do not rely on a frontend Boolean as proof of completion.

For a stronger best-effort 90% calculation, track watched intervals/segments (for example, `LessonWatchSegment` with student, lesson, start/end seconds, and timestamps) or implement an equivalent reliable interval-merging strategy. Avoid double-counting overlapping segments. Do not count arbitrary seeks as watched time. Keep the interval/report acceptance policy documented and tested.

Calculate watched coverage using the union of accepted watched intervals, not by adding overlapping durations. Mark the viewing condition complete only when the accepted coverage reaches the configured threshold of the video duration. Handle unknown/zero duration safely and do not unlock lessons when duration is invalid or unavailable.

### Quiz and attempts
Reuse existing quiz/question/option entities if available. The conceptual model is:
- `Quiz`: associated with lesson, passing threshold/settings as appropriate.
- `Question`: belongs to a quiz.
- `AnswerOption`: belongs to a question if the quiz is multiple-choice.
- `QuizAttempt`: student/user, quiz, attempt number if useful, start/submission timestamps, server-calculated score, pass/fail result, and status.
- `QuizAttemptAnswer`: attempt, question, selected answer(s) or submitted response, and awarded points as appropriate.

Requirements:
- Store every attempt, not only the latest score.
- Calculate objective-question scores on the backend from stored correct-answer data.
- Never accept a client-calculated score or `IsPassed` as authoritative.
- Do not expose correct answers in student-facing quiz payloads before submission.
- Validate that submitted questions belong to the relevant quiz and reject duplicate/invalid answers according to the existing domain rules.
- Use a transaction or existing Unit of Work boundary to persist an attempt and its answers consistently.
- Enforce the passing threshold (default 80%) on the backend.
- Follow the current policy for retries; if none exists, implement a reasonable unlimited-retry default and document it, rather than inventing a hidden limit.

### Lesson comments and replies
Reuse the current comment entity if present. Otherwise, a suitable shape includes:
- Comment ID
- Lesson ID
- Author/user ID
- Content
- Created/updated timestamps
- Optional parent comment ID for replies, or a separate reply entity if that better matches existing architecture
- Optional soft-delete/moderation fields consistent with the project

Requirements:
- Only authorized students/enrolled users and permitted staff may read/create comments according to course access policy.
- Authors may edit/delete their own comments, subject to existing business rules.
- Teachers/admins can moderate according to roles.
- Validate content length and reject empty/whitespace-only comments.
- Prevent cross-lesson/cross-course reply references.
- Paginate comment listing if the project already follows pagination conventions.
- If SignalR is implemented, authorize access to the relevant lesson/group and broadcast only after successful database persistence.

### Lesson meeting and resources
- Store an optional meeting URL and meeting date/time; use a separate `LessonMeeting` entity only if multiple meetings per lesson are supported.
- Validate URLs and access rules. Do not expose private meeting links to unauthorized users.
- Reuse the project's file-storage approach for PDFs. If none exists, avoid introducing a large storage integration without approval; define a clean resource abstraction or store an existing externally hosted file URL/key as appropriate.

---

## Lesson unlocking / access policy

Implement one backend service/domain policy that determines whether a student can access a lesson. Do not duplicate the rule in multiple handlers.

A lesson should be considered completed for progression only when:
1. The student is authorized/enrolled in the course.
2. The accepted video coverage is at least the required percentage (default 90%).
3. At least one submitted quiz attempt meets the required passing score (default 80%), or the existing domain's intended attempt policy is applied.

The next lesson's access must be calculated per student. Never globally mark a lesson as unlocked for every student because one student completed it.

For the first lesson, use the existing course rules (normally accessible to enrolled students). For subsequent lessons, confirm completion of the immediately preceding lesson in the same course and respect the lesson order. Do not trust a client-provided `LessonId`/`IsUnlocked` flag alone; validate access on every protected request.

Return a clear response describing:
- Whether the requested lesson is accessible
- Whether video completion is satisfied
- Whether quiz passing is satisfied
- Which requirement remains incomplete, without leaking another student's data

Keep authorization checks in the backend for lesson details, resources, meeting links, video playback page data, comments, quiz access, and progress updates.

---

## CQRS/Application features to inspect and implement

Follow the project's established feature folder and command/query conventions. Reuse existing features and avoid duplicates. Likely operations include:

### Courses and lessons
- Get course details and ordered lessons.
- Get a lesson's details only if the student is authorized to access it.
- Create/update lesson metadata including YouTube URL/video ID, resources, goals, and optional meeting data (teacher/admin only).
- Reorder lessons safely, with course ownership/authorization validation.
- Return per-student lesson status/progression where appropriate.

### Progress
- Get current student's progress for a lesson/course.
- Submit a playback progress report.
- Get completion/access status for the next lesson.
- Server-side completion calculation and persistence.

### Quiz
- Get quiz questions/options without exposing correct answers.
- Submit quiz attempt.
- Get the current student's attempt history/results according to policy.

### Comments
- List comments/replies for an authorized lesson.
- Create comment/reply.
- Edit/delete own comment, or moderate with authorized staff role.
- Optionally broadcast persisted comments via existing SignalR infrastructure.

Use the existing mediator and result/error handling style. Add DTOs, validators, mappings, repository methods/specifications, and Unit of Work changes as required by the current codebase.

---

## Dynamic video watermark: backend preparation

Screen-recording prevention is explicitly excluded. Do not implement screen-recording detection, black screens, DRM, or screenshot blocking.

A dynamic watermark is still desired as a deterrent, and it requires both frontend rendering and backend-provided identity data.

Backend responsibilities:
- Provide a minimal watermark DTO for the authenticated current student, using only data appropriate to display (e.g. display name and/or a masked email or stable non-sensitive student identifier).
- Derive identity from the authenticated principal, not a client-supplied user ID.
- Return watermark data only when the student is authorized to view the lesson.
- Avoid returning unnecessary personal data. Prefer a display name plus a non-sensitive student identifier; do not expose raw email unless the product explicitly requires it and the user has approved that privacy choice.
- Apply existing authorization and privacy conventions.
- Add tests proving one student cannot request another student's watermark identity by changing an ID in the request.

Frontend responsibilities (document only if frontend is outside the current repository):
- Overlay the watermark on top of the YouTube player/container, with CSS positioning and periodic movement.
- Use the authenticated student's watermark DTO returned by the backend.
- Keep the watermark visible above the player where technically possible.
- Explain that a frontend overlay can be cropped/covered/removed by a determined user and is not DRM or guaranteed anti-recording protection.

If the current repository includes the frontend, implement the watermark UI in the existing style and architecture. Otherwise implement the backend DTO/endpoint or include the required identity fields in the authorized lesson response, and provide a clear frontend integration contract.

---

## Database, migrations, and constraints

- Inspect the existing EF Core provider and migration setup first.
- Add EF Core entity configurations consistent with existing conventions.
- Define foreign keys, delete behaviors, indexes, and unique constraints intentionally.
- Avoid cascade-delete cycles or accidental deletion of quiz attempts/progress where history should be preserved.
- Add and review migrations; do not run destructive migrations against a production database.
- Keep the database provider independent of the Web/API layer.
- Ensure timestamps use the project's established UTC/`DateTimeOffset` conventions.
- Validate all relevant decimal/percentage ranges (0–100).
- Add indexes for common queries such as student+lesson progress, lesson ordering within course, quiz attempts by student/quiz, and comments by lesson/date.

## Security and validation checklist

- No client-supplied student ID is trusted for self-service operations.
- All student-specific queries are scoped to the authenticated user.
- Enforce enrollment and lesson unlock policy server-side.
- Never return correct answers before quiz submission.
- Never trust client-supplied score, pass/fail, watched percentage, or completion flags without server-side calculation.
- Validate and normalize YouTube URLs/video IDs.
- Validate resource and meeting URLs according to the project's requirements.
- Prevent IDOR (insecure direct object reference) across courses, lessons, comments, quiz attempts, progress, and watermark identity.
- Use validation and consistent error responses.
- Avoid logging tokens, credentials, quiz answer keys, or sensitive personal data.
- Do not claim YouTube URLs are private merely because they are stored in the database. YouTube visibility/access remains controlled by YouTube settings.

## Tests required

Add or extend tests using the project's existing test framework. At minimum, test:

1. First lesson access for an authorized enrolled student.
2. Later lesson remains locked if video progress is below 90%.
3. Later lesson remains locked if quiz score is below 80%.
4. Later lesson unlocks only when both conditions are met.
5. Another student's progress cannot unlock the current student's lesson.
6. Progress survives reloading/re-authentication.
7. Invalid or implausible progress reports are rejected.
8. Seeking to the end does not by itself mark 90% watched.
9. Overlapping watch intervals do not double-count coverage.
10. Quiz score/pass status is calculated by the backend.
11. Correct answers are not exposed in the quiz retrieval response.
12. Multiple attempts are stored separately.
13. A student cannot modify another student's comment.
14. Replies cannot reference comments from another lesson.
15. Unauthorized users cannot access lesson resources or meeting links.
16. Watermark data belongs only to the authenticated student.
17. YouTube URL/video ID validation handles supported URL forms and invalid input.
18. Existing tests continue to pass.

Use unit tests for domain/application rules and integration tests for database, authorization, and endpoint behavior as supported by the current test infrastructure. Do not add unnecessary new testing frameworks if the project already has an established setup.

---

## Implementation workflow and final report

### Phase 1 — Audit
- Inspect the repository and summarize the current architecture and existing relevant entities/features.
- Identify missing requirements and conflicts with the current design.
- Propose the minimum implementation plan and any migration impact.

### Phase 2 — Implement
- Implement entities/configurations and migration(s).
- Implement repositories/UoW extensions where needed.
- Implement CQRS commands/queries/handlers, DTOs, validation, authorization, and endpoints using current conventions.
- Implement progress tracking and server-side lesson unlock policy.
- Implement quiz attempts and scoring.
- Implement comments/replies and optional SignalR integration if appropriate.
- Implement backend watermark identity support and frontend watermark only if the frontend exists in this repository.

### Phase 3 — Verify
- Build the full solution.
- Run relevant tests and report exact results.
- Review migrations and dependency injection.
- Check for accidental architecture violations and duplicate features.
- Identify any remaining manual configuration, secrets, provider setup, or frontend work.

### Final response
Provide:
1. Summary of existing code reused.
2. Files created/modified and purpose.
3. Database changes and migration name.
4. CQRS operations/endpoints added or updated.
5. Exact commands to apply migrations and run tests, adapted to the actual solution.
6. A clear explanation of how 90% watched coverage is calculated and its YouTube tracking limitations.
7. The backend/frontend contract for the dynamic watermark.
8. Build/test results, known limitations, and any remaining manual steps.

Do not implement out-of-scope screen-recording prevention.
