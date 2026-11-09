Architecture Discovery: Secure Read-Only Data Access Using Microsoft Data API Builder

Background

We have an existing enterprise application built on .NET Framework 4.8 with a Microsoft SQL Server database.

The application contains established business logic, authentication, authorization, user entitlements, and potentially multiple layers of data-level security.

We want to introduce Microsoft Data API Builder (DAB) as a way to expose authorized application data through:

- REST APIs
- GraphQL APIs
- Model Context Protocol (MCP) for AI agents

The goal is to enable users, integrations, and AI agents to query their authorized business data without bypassing existing application security.

This initiative is strictly read-only. No exposed interface should permit modification of business data.

Your initial assignment is to investigate the existing codebase and produce a detailed architecture assessment and recommended implementation strategy.

Do not begin implementation until the architecture and security model have been reviewed.

1. Discover the Existing Application Architecture

Examine the source code, configuration, database access patterns, and available database schema definitions.

Determine:

- How authentication works.
- How users are identified across application components.
- How the application connects to SQL Server.
- Whether database connections use service accounts, individual user identities, or another mechanism.
- Whether the application uses Entity Framework, ADO.NET, stored procedures, or custom data-access frameworks.
- Where business authorization is enforced.
- Whether security logic is centralized or distributed throughout the application.
- Whether existing services or libraries can be reused independently of the legacy application.

Document relevant classes, interfaces, methods, database objects, and their relationships.

2. Investigate Application Entitlements

Find and document the complete application entitlement model.

Determine:

1. How entitlements are defined and stored.
2. How they are assigned to users or groups.
3. How entitlement checks are performed.
4. Whether entitlements are hierarchical or context-dependent.
5. Whether entitlements govern modules, features, operations, data entities, or combinations of these.
6. Whether entitlements are cached and how changes are propagated.
7. Whether entitlements are enforced through application code, SQL queries, stored procedures, or other mechanisms.
8. Whether there are administrative overrides or special access rules.

Trace representative authorization decisions from the application entry point to the final database query.

Important: Do not assume that application entitlements can be represented accurately using static DAB roles.

Determine whether authorization requires dynamic evaluation using the current user's identity, business context, and existing entitlement rules.

Identify the actual source of truth for authorization decisions.

3. Investigate Data-Level Security

Application entitlements may not fully describe which records a user can access.

Identify all additional data-security mechanisms, including:

- SQL Server Row-Level Security (RLS).
- Security views and predicates.
- Stored procedures that enforce access restrictions.
- Application-generated WHERE clauses.
- Account-, portfolio-, fund-, or organization-specific permissions.
- Tenant or business-unit isolation.
- User-specific access mappings.
- Column-level restrictions or sensitive data masking.
- Security enforced through SQL Server execution context or session context.

Determine whether these controls operate independently of application entitlements or require both to be evaluated together.

Investigate whether security is enforced at the database layer or only when queries originate from the legacy application.

Pay special attention to security that could be bypassed by querying tables directly.

For every identified mechanism, provide source-code or database-object references and explain how it would behave when accessed through DAB.

4. Evaluate DAB Integration Strategies

Research the capabilities and limitations of the currently supported Microsoft Data API Builder version.

Evaluate the following approaches:

A. Native DAB authorization

Use DAB entity permissions, roles, claims-based policies, field restrictions, and database policies.

Determine which existing security rules can be represented without changing their semantics.

B. SQL Server-enforced authorization

Use database views, SQL Server RLS, security predicates, stored procedures, and trusted session context to enforce existing access rules.

Investigate whether existing security logic can be reused or adapted.

C. Dedicated authorization service

Introduce a modern .NET service or authorization adapter that reuses existing entitlement logic and produces trusted authorization decisions or claims.

Evaluate how DAB would consume those decisions and how entitlement changes would be reflected.

D. Hybrid architecture

Combine DAB entity-level authorization, a dedicated entitlement service, and SQL Server-enforced data security.

Evaluate whether this approach provides better security fidelity, maintainability, and performance.

For each approach, document:

- Feasibility
- Required changes
- Security guarantees
- Performance implications
- Operational complexity
- Compatibility with the existing application
- Long-term maintainability
- Risks and limitations

Do not assume DAB has a native extension point for custom entitlement evaluation. Verify supported capabilities and identify any necessary gateway or service boundary.

5. Identity and Security Context Propagation

Determine how a request from a user or AI agent would be authenticated and authorized.

Evaluate:

- Existing authentication integration.
- JWT or token-based authentication.
- User identity mapping.
- Trusted claims generation.
- Dynamic entitlement resolution.
- Delegated authentication where supported.
- SQL Server SESSION_CONTEXT integration.
- Service-to-service authentication.
- Identity propagation through REST, GraphQL, and MCP.

Distinguish between the identity of the AI agent, the authenticated human user, and the database connection identity.

The system must never allow an AI agent or client to impersonate another user by supplying arbitrary user IDs, roles, entitlements, or SQL session-context values.

Determine how authorization remains correct when entitlements are modified or revoked.

6. Design a Strictly Read-Only Exposure Model

Investigate how to guarantee read-only behavior across all interfaces.

Requirements:

- Only explicitly approved entities may be exposed.
- REST write operations must be unavailable.
- GraphQL mutations must be unavailable.
- MCP write tools must be unavailable.
- SQL credentials must have least-privilege read access.
- Stored procedures and functions must be reviewed for side effects.
- Database objects that expose sensitive information must not be automatically published.
- Read-only access must be enforced at multiple layers.

Consider SQL Server permissions, views, stored procedures, DAB configuration, and deployment isolation.

7. MCP-Specific Security and AI Agent Considerations

Evaluate how DAB's MCP capabilities can safely expose business data to AI agents.

Investigate:

- How MCP tools are generated and exposed.
- Whether MCP operations enforce the same authorization rules as REST and GraphQL.
- How the authenticated user's identity reaches DAB.
- Whether tools can be restricted by user entitlements.
- How sensitive fields and records are excluded.
- How to prevent unrestricted data enumeration.
- Pagination, query complexity, and result-size limits.
- Auditing of agent-generated data requests.
- Risks of agents executing repeated or unexpectedly broad queries.
- How to prevent agents from accessing data using privileged service credentials.

MCP must not become an alternate path around application security.

The same user querying the same business data through the legacy application, REST, GraphQL, or MCP should receive equivalent authorization outcomes.

8. Performance and Scalability

Evaluate the operational impact of exposing the existing SQL Server database to additional query consumers.

Consider:

- Query execution plans and indexes.
- GraphQL relationship traversal.
- Expensive joins.
- Database connection pooling.
- Authorization lookup overhead.
- Entitlement caching and invalidation.
- SQL Server session-context behavior.
- Pagination and query limits.
- Query timeouts.
- Workload isolation.
- Read replicas or reporting databases where appropriate.

Determine whether DAB should access the operational database directly or use a separate read-optimized data surface.

9. Security Validation Strategy

Develop a test strategy proving that the new interfaces cannot bypass existing security.

Include test cases for:

- Unauthenticated users.
- Users without application entitlements.
- Users with partial entitlements.
- Users with entitlement access but restricted data access.
- Users with data access but missing required application entitlements.
- Users belonging to multiple organizations or security scopes.
- Revoked or modified entitlements.
- Cross-tenant data access attempts.
- Restricted columns and sensitive data.
- GraphQL nested relationships and filtering.
- MCP tool discovery and execution.
- Unauthorized attempts to supply or modify identity claims.
- Attempts to execute write operations.
- Stored procedures with unexpected side effects.

Test REST, GraphQL, and MCP independently.

Authorization should fail closed whenever identity, entitlement information, or required security context is unavailable.

10. Deliverables

Produce the following documentation.

A. Current-State Security Architecture

Include:

- Authentication flow.
- Entitlement model.
- Data-level security model.
- Important source-code locations.
- Relevant database objects.
- Known security assumptions.
- Areas requiring further investigation.

B. Authorization Matrix

Create a matrix mapping:

- Business entities
- Application entitlements
- Data-level restrictions
- Sensitive fields
- Existing enforcement points
- Proposed enforcement points
- REST/GraphQL/MCP exposure eligibility

C. Proposed Architecture

Provide a recommended target architecture and sequence diagrams for:

1. REST request authorization.
2. GraphQL request authorization.
3. MCP request authorization.
4. Entitlement resolution.
5. SQL Server data-security enforcement.

Explain the trust boundaries and identity propagation between components.

D. Architecture Decision Matrix

Compare the native DAB, SQL Server, authorization-service, and hybrid approaches.

Recommend an approach based on actual findings in the repository, rather than generic best practices.

E. Proof-of-Concept Plan

Identify a small set of representative business entities suitable for a secure proof of concept.

Include at least:

- One entity protected by application entitlements.
- One entity protected by data-level security.
- One entity requiring both mechanisms.

Provide an implementation plan, proposed project structure, DAB configuration approach, database changes, and security tests.

F. Risks and Open Questions

Document anything that cannot be conclusively determined from the available source code.

Separate confirmed findings from assumptions and recommendations.

Investigation Rules

1. Do not modify production application code or database objects.
2. Do not generate a full DAB configuration yet.
3. Do not assume all authorization occurs in one place.
4. Do not equate authentication with authorization.
5. Do not assume SQL Server security is sufficient to enforce application entitlements.
6. Do not duplicate existing security logic unless there is a compelling architectural reason.
7. Prefer reusing established authorization sources of truth.
8. Cite exact file paths, classes, methods, SQL objects, and relevant code locations for findings.
9. Clearly distinguish supported DAB capabilities from proposed custom components.
10. Prioritize security correctness and maintainability over minimizing implementation effort.

Start by mapping the existing authentication, entitlement, and data-security architecture. Then use those findings to recommend how DAB should be integrated safely.
