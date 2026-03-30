# Inventory Management Module — Implementation Plan & Progress Tracker

> **Usage**: Check off items as they are completed. Each WP can be implemented in a separate conversation.
> Use this file to resume work: read the Context section and the first unchecked WP.

---

## Context

Bårdar Swing Club needs an inventory management system integrated into the existing MemberService application (ASP.NET Core 8, C#, SQL Server, Razor Pages). The club has ~140 audio/AV items (speakers, cables, microphones, stands, etc.) tracked in a CSV file that needs to be imported and managed. Members need to borrow equipment for events, return it, and do storage inventory checks from mobile phones using QR code scanning.

The module is a React SPA (Vite + TypeScript) served from a Razor shell page at `/Inventory`, backed by new JSON API controllers and EF Core entities, reusing the existing cookie auth and role-based authorization infrastructure.

**Existing project patterns to follow:**
- Entities: `src/MemberService/Data/` with `IEntityTypeConfiguration<T>` in same file
- Roles: `const string` fields in `Roles.cs` (auto-seeded via reflection)
- Policies: enum values in `Policy.cs` (auto-registered via `Enum.GetValues<Policy>()`)
- Policy enforcement: switch expression in `RoleRequirementsHandler.IsAuthorized`; Admin granted everything unconditionally before the switch
- Controllers: co-located in feature folder under `Pages/`
- No existing JSON API controllers — this module introduces `[ApiController]`
- Tests: NUnit 4 + Shouldly, unit tests only (no integration test infra yet)

---

## Inventory Item Attributes

From the actual CSV at `/Users/knuterikborgen/code/data/Inventarliste for utstyr - Inventar.csv`:

| C# Property | CSV Column | Type | Notes |
|-------------|-----------|------|-------|
| `Tag` | `Tag` | `string` (unique) | Key field. Encoded in QR code. E.g. S-001, K-003 |
| `Kategori` | `Kategori` | `string?` | E.g. Stativ, Kabel, Høyttaler, Mikrofon |
| `SubKategori` | `Sub-kategori` | `string?` | E.g. Mikrofonstativ, Multikabel, Monitorer |
| `Beskrivelse` | `Beskrivelse` | `string` | Human-readable Norwegian description |
| `Merke` | `Merke` | `string?` | Brand. E.g. Shure, Behringer, K&M |
| `Modell` | `Modell` | `string?` | Model name. E.g. SM58, SRM 350, MR18 |
| `Detaljer` | `Detaljer` | `string?` | Extra notes |
| `LengdeM` | `Lengde [m]` | `decimal?` | Length in meters (cables). NB: Norwegian decimal comma |
| `Diameter` | `Diameter` | `int?` | Connector diameter in mm |
| `PhotoUrl` | — | `string?` | Not in CSV; added via UI |
| `InInventory` | `Inventory` | `bool` | 1 = active in inventory |
| `Lokasjon` | `Lokasjon` | `string?` | Location (empty for now, single storage) |
| `CreatedAt` | — | `DateTime` | Auto-set |
| `UpdatedAt` | — | `DateTime` | Updated on every change |

CSV import: `Tag` is the upsert key. Header row skipped. Quoted fields (e.g. `"2,5"`) and empty rows (140–200 in real file) handled by parser.

---

## New User Roles

- `InventoryManager` — create/edit/delete assets, CSV import, view all sessions
- `InventoryUser` — browse catalogue, start borrow/return sessions

---

## Architecture

- **Backend**: ASP.NET Core 8, EF Core + SQL Server
- **Frontend**: React 18 + TypeScript, Vite, output to `wwwroot/dist/inventory/`
- **SPA shell**: Razor page at `/Inventory/Index.cshtml`
- **Auth**: existing session cookie (same-origin) + anti-forgery token in `<meta name="csrf-token">`
- **QR Scanning**: `html5-qrcode` npm package — on-device, iOS/Android, no server calls

**QR/Camera feasibility notes:**
- Uses browser `getUserMedia` API — mature, iOS Safari 14.5+ / Android Chrome ✅
- Requires HTTPS — satisfied by Azure deployment ✅
- iOS requires user gesture to start camera — `html5-qrcode` handles this with a "Start" button ✅
- ZXing engine (underlying) is very reliable for printed QR codes at normal scanning distance ✅
- **Risk: Low.** WP0 (POC) confirms it works in actual deployment context before committing to full backend.

---

## Data Model

**`InventoryAsset`** — all fields from attribute table above, plus:
- `Guid Id`
- `Guid? CurrentBorrowId` — null = available; set to borrow session Id on checkout; cleared on return/check-complete

**`InventoryBorrow`** — a session
- `Guid Id`, `string BorrowedByUserId` (FK → AspNetUsers), `string EventName`
- `DateTime StartedAt`, `DateTime? CompletedAt`
- `BorrowType Type` enum: `Borrow` | `Return` | `InventoryCheck` (stored as string)

**`InventoryBorrowItem`** — line items
- `Guid Id`, `Guid BorrowId` (FK), `Guid AssetId` (FK), `DateTime ScannedAt`
- Unique index on `(BorrowId, AssetId)`

---

## Work Packages

### WP0 — QR Scanning Proof of Concept (Frontend Only) ✅ DONE
> **Purpose**: Validate camera/QR flow on real iOS and Android devices before investing in backend. Can be done as a standalone Vite+React app with mock data.

- [x] Scaffold `inventory-spa/` with Vite + React + TypeScript
- [x] Install `html5-qrcode`
- [x] Build `QrScanner.tsx` component
- [x] Build `BorrowScan.tsx` page (free-scan mode with mock asset lookup from hardcoded JSON)
- [x] Build `BorrowPickList.tsx` page (paste + QR scan tabs with mock data)
- [x] Deploy to staging (or test via `localhost` + ngrok for mobile testing)
- [x] Confirm: camera opens on iOS Safari, QR decodes correctly, haptic feedback works

**Status:** POC tested on Mac. QR scanning works with Html5QrcodeScanner providing camera input selection UI. Ready for backend integration.

**Files created in this WP:**
```
src/MemberService/inventory-spa/
  package.json
  vite.config.ts
  tsconfig.json
  src/
    main.tsx
    App.tsx                    (just the two scan routes + mock data)
    types/inventory.ts
    components/QrScanner.tsx
    components/AssetCard.tsx
    pages/BorrowScan.tsx
    pages/BorrowPickList.tsx
```

---

### WP1 — Backend Models + EF Migration ✅ DONE
- [x] Create `src/MemberService/Data/Inventory/InventoryAsset.cs`
- [x] Create `src/MemberService/Data/Inventory/InventoryBorrow.cs` (+ `BorrowType` enum)
- [x] Create `src/MemberService/Data/Inventory/InventoryBorrowItem.cs`
- [x] Create `src/MemberService/Data/Inventory/InventoryConfiguration.cs` (`IEntityTypeConfiguration<T>` for all three)
- [x] Add 3 `DbSet<T>` to `src/MemberService/Data/MemberContext.cs`
- [x] Generate migration: `dotnet ef migrations add AddInventoryModule` ✅ Created: `20260330182442_AddInventoryModule.cs`
- [ ] Verify `dotnet run` creates DB tables

**Status:** All entities created with proper EF Core configuration. Migration file generated with correct schema (tables, FKs, constraints, defaults). Next: verify migration applies successfully when DB is available.

---

### WP2 — Roles, Policies, Auth Wiring
- [ ] Add `INVENTORY_MANAGER` and `INVENTORY_USER` to `src/MemberService/Data/ValueTypes/Roles.cs`
- [ ] Add `CanManageInventory` and `CanBorrowInventory` to `src/MemberService/Auth/Policy.cs`
- [ ] Add 2 cases to switch in `src/MemberService/Auth/Requirements/RoleRequirementsHandler.cs`
- [ ] Verify roles appear in admin panel after `dotnet run`

---

### WP3 — JSON API Controllers
- [ ] Create `src/MemberService/Pages/Inventory/InventoryModels.cs` (all DTOs)
- [ ] Create `src/MemberService/Pages/Inventory/CsvImportService.cs`
- [ ] Create `src/MemberService/Pages/Inventory/InventoryAssetsController.cs`
  - [ ] `GET /api/inventory/assets` (list with search/filter)
  - [ ] `GET /api/inventory/assets/{tag}`
  - [ ] `POST /api/inventory/assets` (create)
  - [ ] `PUT /api/inventory/assets/{tag}` (update)
  - [ ] `DELETE /api/inventory/assets/{tag}` (delete)
  - [ ] `POST /api/inventory/assets/import` (CSV upsert)
- [ ] Create `src/MemberService/Pages/Inventory/InventoryBorrowsController.cs`
  - [ ] `GET /api/inventory/borrows`
  - [ ] `GET /api/inventory/borrows/{id}`
  - [ ] `POST /api/inventory/borrows` (start session)
  - [ ] `POST /api/inventory/borrows/{id}/scan`
  - [ ] `DELETE /api/inventory/borrows/{id}/items/{itemId}`
  - [ ] `POST /api/inventory/borrows/{id}/complete`
- [ ] Add `MapFallbackToPage` to `src/MemberService/Program.cs`
- [ ] Test all endpoints with curl/Postman
- [ ] Import real CSV: verify 139 rows created, empty rows skipped

---

### WP4 — Frontend Build Setup (connect to backend)
> WP0 creates the SPA scaffold; this WP wires it to the real backend and integrates with the .NET build.

- [ ] Update `vite.config.ts` base and outDir for production
- [ ] Create `src/MemberService/inventory-spa/src/api/client.ts` (fetch wrapper with CSRF)
- [ ] Create `src/MemberService/inventory-spa/src/api/assets.ts`
- [ ] Create `src/MemberService/inventory-spa/src/api/borrows.ts`
- [ ] Add `BuildSpa` MSBuild target to `src/MemberService/MemberService.csproj`
- [ ] Add entries to `.gitignore` (`wwwroot/dist/`, `node_modules/`)
- [ ] Create `src/MemberService/Pages/Inventory/Index.cshtml` (SPA shell)
- [ ] Create `src/MemberService/Pages/Inventory/Index.cshtml.cs`
- [ ] Create `src/MemberService/Pages/Inventory/_InventoryLayout.cshtml`
- [ ] Add Inventory nav link to `src/MemberService/Pages/_Layout.cshtml`
- [ ] Verify SPA loads at `/inventory` with auth cookie

---

### WP5 — Asset Management UI
- [ ] `AssetList.tsx` — searchable/filterable card grid, status badges
- [ ] `AssetForm.tsx` — create/edit form for all asset fields
- [ ] `CsvImport.tsx` — textarea paste + file upload + results display
- [ ] Wire `App.tsx` routes: `/inventory`, `/inventory/assets/new`, `/inventory/assets/:tag/edit`, `/inventory/assets/import`
- [ ] Test: CRUD an asset, import CSV, verify availability status

---

### WP6 — Borrow/Return/InventoryCheck Workflow UI
- [ ] `BorrowStart.tsx` — event name buttons + mode selector + session start
- [ ] `BorrowPickList.tsx` — paste tab + QR scan tab (wired to real API)
- [ ] `BorrowScan.tsx` — free-scan mode (wired to real API)
- [ ] `BorrowReview.tsx` — session review + complete
- [ ] Wire `App.tsx` routes for all workflow pages
- [ ] Test full borrow flow end-to-end (mobile + desktop)
- [ ] Test return flow
- [ ] Test inventory check flow

**Fixed event name buttons:**
`"Winter Jump"` `"Juni Jazz"` `"Shag up"` `"Sosialdanskomiteen"` `"O'Slow"` `"Oslo Balboa Weekend"` `"Just me"`

---

### WP7 — Tests
- [ ] Create `tests/MemberService.Tests/Inventory/CsvImportServiceTests.cs`
  - [ ] Valid 5-row import (3 new, 2 updates)
  - [ ] Missing tag field → row error, others proceed
  - [ ] Quoted field with comma decimal `"2,5"` → `decimal 2.5`
  - [ ] Windows line endings (CRLF)
  - [ ] Empty rows skipped
  - [ ] Header row skipped
- [ ] Create `tests/MemberService.Tests/Inventory/BorrowSessionLogicTests.cs`
  - [ ] Scan into completed session → 400
  - [ ] Double-scan same tag → no-op, no duplicate row
  - [ ] `CurrentBorrowId` null → set after borrow complete → null after return complete
- [ ] `dotnet test` passes

---

## Complete File Manifest

### New files

| Path | WP | Purpose |
|------|----|---------|
| `src/MemberService/Data/Inventory/InventoryAsset.cs` | WP1 | EF entity |
| `src/MemberService/Data/Inventory/InventoryBorrow.cs` | WP1 | EF entity + `BorrowType` enum |
| `src/MemberService/Data/Inventory/InventoryBorrowItem.cs` | WP1 | EF entity |
| `src/MemberService/Data/Inventory/InventoryConfiguration.cs` | WP1 | EF configurations |
| `src/MemberService/Migrations/YYYYMMDD_AddInventoryModule.cs` | WP1 | Generated migration |
| `src/MemberService/Pages/Inventory/InventoryModels.cs` | WP3 | DTOs |
| `src/MemberService/Pages/Inventory/CsvImportService.cs` | WP3 | CSV parser |
| `src/MemberService/Pages/Inventory/InventoryAssetsController.cs` | WP3 | Assets JSON API |
| `src/MemberService/Pages/Inventory/InventoryBorrowsController.cs` | WP3 | Borrows JSON API |
| `src/MemberService/Pages/Inventory/Index.cshtml` | WP4 | SPA shell page |
| `src/MemberService/Pages/Inventory/Index.cshtml.cs` | WP4 | Shell page model |
| `src/MemberService/Pages/Inventory/_InventoryLayout.cshtml` | WP4 | Minimal layout |
| `src/MemberService/inventory-spa/package.json` | WP0 | npm manifest |
| `src/MemberService/inventory-spa/vite.config.ts` | WP0 | Vite config |
| `src/MemberService/inventory-spa/tsconfig.json` | WP0 | TypeScript config |
| `src/MemberService/inventory-spa/src/main.tsx` | WP0 | React entry |
| `src/MemberService/inventory-spa/src/App.tsx` | WP0 | Router routes |
| `src/MemberService/inventory-spa/src/types/inventory.ts` | WP0 | TS interfaces |
| `src/MemberService/inventory-spa/src/components/QrScanner.tsx` | WP0 | html5-qrcode wrapper |
| `src/MemberService/inventory-spa/src/components/AssetCard.tsx` | WP0 | Asset display card |
| `src/MemberService/inventory-spa/src/pages/BorrowScan.tsx` | WP0 | Free-scan page |
| `src/MemberService/inventory-spa/src/pages/BorrowPickList.tsx` | WP0 | Pick list page |
| `src/MemberService/inventory-spa/src/api/client.ts` | WP4 | Fetch wrapper |
| `src/MemberService/inventory-spa/src/api/assets.ts` | WP4 | Asset API calls |
| `src/MemberService/inventory-spa/src/api/borrows.ts` | WP4 | Borrow API calls |
| `src/MemberService/inventory-spa/src/pages/AssetList.tsx` | WP5 | Asset list page |
| `src/MemberService/inventory-spa/src/pages/AssetForm.tsx` | WP5 | Asset form page |
| `src/MemberService/inventory-spa/src/pages/CsvImport.tsx` | WP5 | CSV import page |
| `src/MemberService/inventory-spa/src/pages/BorrowStart.tsx` | WP6 | Start session page |
| `src/MemberService/inventory-spa/src/pages/BorrowReview.tsx` | WP6 | Review + complete page |
| `tests/MemberService.Tests/Inventory/CsvImportServiceTests.cs` | WP7 | CSV tests |
| `tests/MemberService.Tests/Inventory/BorrowSessionLogicTests.cs` | WP7 | Session logic tests |

### Existing files to modify

| Path | WP | Change |
|------|----|--------|
| `src/MemberService/Data/ValueTypes/Roles.cs` | WP2 | Add `INVENTORY_MANAGER`, `INVENTORY_USER` |
| `src/MemberService/Auth/Policy.cs` | WP2 | Add `CanManageInventory`, `CanBorrowInventory` |
| `src/MemberService/Auth/Requirements/RoleRequirementsHandler.cs` | WP2 | Add 2 switch cases |
| `src/MemberService/Data/MemberContext.cs` | WP1 | Add 3 `DbSet<T>` |
| `src/MemberService/MemberService.csproj` | WP4 | Add `BuildSpa` target |
| `src/MemberService/Program.cs` | WP3 | Add `MapFallbackToPage` |
| `src/MemberService/Pages/_Layout.cshtml` | WP4 | Add Inventory nav link |
| `.gitignore` | WP4 | Exclude dist + node_modules |

---

## Verification Checklist

- [ ] `dotnet ef migrations add AddInventoryModule` succeeds
- [ ] `dotnet run` creates inventory tables in DB
- [ ] Assign `InventoryUser` role → `/inventory` accessible; assign `InventoryManager` → import returns 200
- [ ] POST real CSV → 139 assets created, empty rows skipped
- [ ] Duplicate tag → 409 Conflict
- [ ] Full borrow flow on mobile: camera opens → QR decoded → asset registered → complete → `CurrentBorrowId` set
- [ ] Full return flow: `CurrentBorrowId` cleared after complete
- [ ] Deep link to `/inventory/assets/new` → SPA loads (no 404)
- [ ] `dotnet test` passes
