# sa-compliance-patterns.md

## Intent
Encode South African regulatory requirements (FICA, POPIA, NCA) as concrete implementation
patterns. This skill primes an LLM to generate compliant-by-default code rather than requiring
compliance to be bolted on after the fact. Every pattern here has a regulatory anchor — no
vague "best practices".

---

## Regulatory Anchors

| Regulation | Core Concern | Key Requirement |
|---|---|---|
| FICA | Anti-money laundering / KYC | Customer identity verification, risk-based approach, record keeping |
| POPIA | Personal data protection | Lawful processing, data minimisation, retention limits, breach notification |
| NCA | Consumer credit | Affordability assessment, disclosure, reckless credit prohibition |

---

## FICA Patterns

### Identity Verification
- SA ID number validation is non-negotiable before any onboarding completes
- Validation must cover: Luhn algorithm, date component parse, gender digit (positions 6–9), citizenship digit (position 10), cross-validation against applicant-supplied DOB
- Never trust the ID number alone — cross-validate against a supplied date of birth
- Invalid ID = application rejected at domain layer, not UI layer

```csharp
// ID validation lives in the domain, not a utility class
public class SouthAfricanIdNumber
{
    public static ValidationResult Validate(string idNumber, DateOnly applicantDob) { ... }
    // Luhn check, date parse, gender digit, citizenship digit, DOB cross-validation
}
```

### Risk-Based Approach (RBA)
- Every onboarding must produce a risk score (0–100) before a decision is made
- Risk factors are pluggable — each implements `IRiskFactor` and is independently audited
- Standard factors: PEP screening, sanctions screening, nationality, address verification, document quality
- Score thresholds drive due diligence tier:
  - 0–30: Standard Due Diligence (SDD)
  - 31–70: Customer Due Diligence (CDD)
  - 71–100: Enhanced Due Diligence (EDD) — requires compliance team routing + senior sign-off

```csharp
public interface IRiskFactor
{
    string Name { get; }
    Task<RiskFactorResult> EvaluateAsync(ApplicantContext context);
}
```

### Workflow State Machine
- Onboarding workflow must be an explicit state machine — no free-form status strings
- Valid transitions only: `ApplicationSubmitted → DocumentsCollected → RiskAssessed → UnderReview → Approved/Rejected`
- Invalid transitions rejected at domain layer before touching the database
- Use Stateless library in .NET — do not hand-roll state transition logic

### Record Keeping (FICA Section 22)
- All KYC records must be retained for **5 years** after the business relationship ends
- Retention policy must be enforced by infrastructure (scheduled job), not application logic alone
- Documents stored in private Azure Blob Storage — never publicly accessible
- SAS tokens for document access: max 15-minute expiry, generated on-demand, never stored

### Audit Trail
- Every state change must be recorded with: timestamp, actor, previous state, new state, reason
- Audit log must be append-only — enforce at DB constraint level (no UPDATE/DELETE grants on audit table)
- Audit records must be immutable — no application-layer "edit" path should exist

---

## POPIA Patterns

### Data Minimisation
- No PII in transaction records — reference numbers only, resolve to identity via secure lookup
- SA ID numbers: encrypt at rest using AES-256 via Azure Key Vault — never stored plaintext
- No PII in logs — scrub before any log statement touches personal data
- No PII in error messages returned to clients

### Retention Policy
- Default retention: **5 years** for financial records (FICA alignment)
- Scheduled job enforces archival/deletion — not manual process
- Archive policy must be documented in infrastructure, not just application code
- When a data subject requests deletion, assess FICA retention obligation first — they may conflict

### Lawful Processing Basis
- Every category of personal data must have a documented lawful basis before processing
- Consent is not always required — legitimate interest or legal obligation often applies in fintech
- Document the basis in the data register, not just in comments

### Breach Response
- Breach notification to Information Regulator required within **72 hours** of becoming aware
- Maintain an incident log — date discovered, data affected, affected individuals, remediation steps
- Build breach detection hooks into your audit pipeline

---

## Document Storage Pattern

```
Private Azure Blob Container (no public access)
└── {tenantId}/
    └── {applicantId}/
        └── {documentType}-{timestamp}.pdf

Access pattern:
- Application requests SAS token from Key Vault-backed service
- SAS token: 15-minute expiry, read-only, single blob
- SAS token never stored — generated per request
- Access attempt logged to audit trail
```

---

## Anti-Patterns

| Anti-Pattern | Why It's Wrong |
|---|---|
| Storing SA ID numbers plaintext | POPIA violation — PII must be encrypted at rest |
| Status as a string field | Allows invalid states — use explicit state machine |
| Manual audit trail inserts | Can be bypassed — enforce at DB constraint level |
| Storing SAS tokens | Tokens are ephemeral — generate on demand, never persist |
| Deleting audit records | FICA requires immutable records — append-only only |
| PII in logs | Log aggregators are not secure PII stores |
| Single due diligence tier for all customers | FICA requires risk-based approach — one size is non-compliant |

---

## Pre-Ship Compliance Checklist

- [ ] SA ID validation covers all 5 checks (Luhn, date, gender digit, citizenship digit, DOB cross-validation)
- [ ] Risk score produced and persisted before onboarding decision
- [ ] Risk score audit log records each contributing factor independently
- [ ] State machine transitions are the only path to status change
- [ ] Audit table has no UPDATE/DELETE grants at DB level
- [ ] SA ID numbers encrypted via Key Vault, not application-managed keys
- [ ] Documents in private Blob — SAS tokens generated on demand, max 15 min
- [ ] Retention policy enforced by scheduled job, not manual process
- [ ] No PII in logs, error messages, or transaction records
- [ ] EDD path requires compliance team routing and senior role sign-off
