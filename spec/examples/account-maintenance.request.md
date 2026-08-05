# Maintenance Service Request

## Service

Service name: AccountMaintenance

Operation: Bulk upsert

Route: `/maintenance/accounts`

Batch behavior: Reject the entire batch if any entity fails validation or a required side effect cannot be produced.

## Entity

| Field | Type | Required | Notes |
|---|---|---:|---|
| AccountNumber | string | Yes | Maximum 20 characters. This is the key. |
| EffectiveDate | date | Yes | Cannot be after the current business date. |
| Status | AccountStatus | No | Defaults to Inactive for a new account. |
| ActivationDate | date | No | Set when an account becomes Active. |
| Description | string | No | Maximum 200 characters. Required when Status is Closed. |

Allowed statuses:

- Inactive
- Active
- Closed

## Validations

- V1: Account number is required.
- V2: Account number must be at most 20 characters.
- V3: Effective date cannot be after the current business date.
- V4: A Closed account cannot be changed back to Active.
- V5: Description is required when Status is Closed.
- V6: Account numbers must be unique within the submitted batch.

## Business Rules

- BR1: A new account defaults to Inactive when Status is not supplied.
- BR2: When an existing account changes from Inactive to Active, set ActivationDate to the current business date.

## Side Effects

### SE1: Opening ledger entry

When an existing account changes from Inactive to Active, create one `AccountLedgerEntry` in the same transaction.

Mapping:

- `AccountLedgerEntry.AccountNumber` = resulting `Account.AccountNumber`
- `AccountLedgerEntry.EntryType` = `Opening`
- `AccountLedgerEntry.EntryDate` = current business date

Idempotency: A retry of the same request for the same account must not create another opening ledger entry.

## Examples

### S1: Valid new account

Given no existing account with AccountNumber `A100`, when an account with AccountNumber `A100`, EffectiveDate `2026-08-01`, and no Status is submitted, the account is inserted with Status `Inactive`.

### S2: Valid activation

Given account `A200` is `Inactive`, when Status is changed to `Active`, ActivationDate is set to the business date and one opening ledger entry is produced.

### S3: Invalid reactivation

Given account `A300` is `Closed`, when Status is changed to `Active`, validation V4 fails and no records are written.

### S4: Invalid batch duplicate

When two submitted records use AccountNumber `A400`, validation V6 fails and the entire batch is rejected.
