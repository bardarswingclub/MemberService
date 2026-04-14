# Inventory Management Module — Implementation Plan & Progress Tracker

> **Usage**: Check off items as they are completed. Each WP can be implemented in a separate conversation.
> Use this file to resume work: read the Context section and the first unchecked WP.

---

## Initial Data Load

To seed the inventory data on a new environment, use the CSV import REST endpoint:

```bash
curl -X POST https://<host>/api/inventory/assets/import \
  -H "Content-Type: application/json" \
  -H "RequestVerificationToken: <csrf>" \
  --cookie "<session-cookie>" \
  -d "{\"content\": $(jq -Rs . < 'Inventarliste for utstyr - Inventar.csv')}"
```

Or use the **Importer CSV** page in the UI at `/Inventory` (paste or upload the file). Requires `InventoryManager` or `Admin` role. The CSV file lives outside the repo at `data/Inventarliste for utstyr - Inventar.csv` — do not add auto-seeding to startup code, as the path is machine-specific.

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
| `PhotoUrl` | — | `string?` | Not in CSV; added via UI or asset edit form |
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
- **SPA shell**: Razor page at `/Inventory/Index.cshtml`; `data-can-manage` attribute embeds server-side permission for the React app
- **Auth**: existing session cookie (same-origin) + anti-forgery token in `<meta name="csrf-token">`
- **QR Scanning**: `html5-qrcode` npm package — on-device, iOS/Android, no server calls
- **Build**: SPA built manually via `npm run build` in `inventory-spa/`; output goes to `wwwroot/dist/inventory/`. No MSBuild target yet — add in future if CI needed.

---

## Data Model

**`InventoryAsset`** — all fields from attribute table above, plus:
- `Guid Id`
- `Guid? CurrentBorrowId` — null = available; set to borrow session Id on checkout; cleared on return/check-complete
- `InventoryBorrow? CurrentBorrow` — navigation property (loaded on demand)

**`InventoryBorrow`** — a session
- `Guid Id`, `string BorrowedByUserId` (FK → AspNetUsers), `string EventName`
- `DateTime StartedAt`, `DateTime? CompletedAt`
- `BorrowType Type` enum: `Borrow` | `Return` | `InventoryCheck` (stored as string)

**`InventoryBorrowItem`** — line items
- `Guid Id`, `Guid BorrowId` (FK), `Guid AssetId` (FK), `DateTime ScannedAt`
- Unique index on `(BorrowId, AssetId)`

---

## Work Packages

### WP0 — QR Scanning Proof of Concept ✅ DONE
- [x] Scaffold `inventory-spa/` with Vite + React + TypeScript
- [x] Install `html5-qrcode`
- [x] Build `QrScanner.tsx` component (Html5QrcodeScanner with camera selector UI)
- [x] Test on iOS Safari and Android — camera + QR decode + haptic feedback confirmed

---

### WP1 — Backend Models + EF Migration ✅ DONE
- [x] `InventoryAsset.cs`, `InventoryBorrow.cs` (+ `BorrowType` enum), `InventoryBorrowItem.cs`
- [x] `InventoryConfiguration.cs` with `IEntityTypeConfiguration<T>` for all three
- [x] 3 `DbSet<T>` added to `MemberContext.cs`
- [x] Migration `20260330182442_AddInventoryModule` applied — all tables created

---

### WP2 — Roles, Policies, Auth Wiring ✅ DONE
- [x] `INVENTORY_MANAGER`, `INVENTORY_USER` added to `Roles.cs`
- [x] `CanManageInventory`, `CanBorrowInventory` added to `Policy.cs`
- [x] Two switch cases added to `RoleRequirementsHandler.cs`
- [x] "Lager" nav link in `_Layout.cshtml` — visible to `CanBorrowInventory`
- [x] "Importer CSV" button hidden for non-managers (via `data-can-manage` on root element)

---

### WP3 — JSON API Controllers ✅ DONE
- [x] `InventoryModels.cs` — all DTOs including `BorrowedByEventName`, `BorrowedByUserName` on asset DTO
- [x] `CsvImportService.cs` — upsert by tag, Norwegian decimal comma, quoted fields, empty row skip
- [x] `InventoryAssetsController.cs`
  - [x] `GET /api/inventory/assets?search=&borrowedOnly=` — includes current borrow event + user name
  - [x] `GET /api/inventory/assets/{tag}`
  - [x] `POST /api/inventory/assets`
  - [x] `PUT /api/inventory/assets/{tag}`
  - [x] `DELETE /api/inventory/assets/{tag}`
  - [x] `POST /api/inventory/assets/import`
- [x] `InventoryBorrowsController.cs`
  - [x] `GET /api/inventory/borrows`
  - [x] `GET /api/inventory/borrows/{id}`
  - [x] `POST /api/inventory/borrows` — start session
  - [x] `POST /api/inventory/borrows/{id}/scan` — no-op on duplicate, reloads with ThenInclude
  - [x] `DELETE /api/inventory/borrows/{id}/items/{itemId}`
  - [x] `POST /api/inventory/borrows/{id}/complete` — sets/clears `CurrentBorrowId` on assets
- [x] `MapControllers()` added to `Program.cs`
- [x] Real CSV imported — 139 assets in DB

---

### WP4 — Frontend Build + SPA Shell ✅ DONE
- [x] `api/client.ts` — fetch wrapper with CSRF token from meta tag
- [x] `api/assets.ts`, `api/borrows.ts`
- [x] `Index.cshtml` with `data-can-manage` attribute read from `IndexModel`
- [x] `Index.cshtml.cs` — `[Authorize(Policy = "CanBorrowInventory")]`, exposes `CanManageInventory`
- [x] `_InventoryLayout.cshtml` — minimal layout with CSRF meta tag, loads `main.js` + `main.css`
- [x] `PermissionsContext` in `App.tsx` — passes `canManage` to all pages
- [x] `TopBar` with "← BSC" link back to main site
- [x] SPA basename: `/Inventory` in prod, `/` in dev
- [ ] `BuildSpa` MSBuild target — not yet; build manually with `npm run build` in `inventory-spa/`
- [ ] `.gitignore` entries for `wwwroot/dist/` and `node_modules/` — verify these are excluded

---

### WP5 — Asset Management UI ✅ DONE (partial)
- [x] `AssetList.tsx` — search, "Vis kun utlånt" filter, borrow info (event + user name), availability badge, photo thumbnails with lightbox
- [x] `CsvImport.tsx` — file upload + textarea paste + result display
- [ ] `AssetForm.tsx` — create/edit form with all fields including PhotoUrl + live preview (not yet built)

---

### WP6 — Borrow/Return/InventoryCheck Workflow UI ✅ DONE (partial)
- [x] `BorrowStart.tsx` — fixed event name buttons + "Annet..." free-text + type selector
- [x] `BorrowScan.tsx` — QR camera toggle + manual tag input + scanned item list with remove
- [x] `BorrowReview.tsx` — item list, remove items, complete session with success state
- [x] `Home.tsx` — "Lån / Retur / Tell", "Utstyrsliste", "Importer CSV" (managers only), "Tilbake til BSC"
- [ ] `PickList.tsx` — paste-to-pick workflow (see WP8)

---

### WP7 — Tests ✅ DONE
- [x] `tests/MemberService.Tests/Inventory/CsvImportServiceTests.cs`
  - [x] Valid 5-row import (3 new, 2 updates)
  - [x] Missing tag field → row error, others proceed
  - [x] Quoted field with comma decimal `"2,5"` → `decimal 2.5`
  - [x] Windows line endings (CRLF)
  - [x] Empty rows skipped
  - [x] Header row skipped
- [x] `tests/MemberService.Tests/Inventory/BorrowSessionLogicTests.cs`
  - [x] Scan into completed session → 400
  - [x] Double-scan same tag → no-op, no duplicate row
  - [x] `CurrentBorrowId` null → set after borrow complete → null after return complete
- [x] `dotnet test` passes (123/123)

---

### WP8 — Pick List + Asset Photos ✅ DONE

**Purpose:** Allow users to paste free-form text containing tag IDs to generate a pick list before borrowing. Also adds photo thumbnails throughout the UI.

#### Pick List feature

**UX flow:**
1. User opens pick list from Home (new button) or from within a borrow session
2. User pastes any text (email, spreadsheet, chat message) containing tag-like patterns
3. Smart parser extracts all tag patterns using regex `/\b[A-ZÆØÅ]+-\d+\b/gi` — ignores surrounding text
4. Deduplicates found tags; shows preview of what was found before looking up
5. Batch API call fetches all matched assets in one request
6. Shows checklist of assets:
   - Tag + beskrivelse + kategori
   - Photo thumbnail if `photoUrl` is set (click to enlarge)
   - Availability badge (tilgjengelig / utlånt)
   - Checkbox — pre-checked, user can deselect items they don't want
7. "Start lånesession med valgte" button — creates a new borrow session and adds all checked items, then navigates to BorrowReview

**Backend changes needed:**
- Add `POST /api/inventory/assets/lookup` endpoint — accepts `{ tags: string[] }`, returns matching assets (same DTO as list). Does not 404 on unknown tags; returns found subset with a `notFound` list for unknowns.

**Frontend files:**
- `src/inventory-spa/src/pages/PickList.tsx` — paste input, tag extraction preview, asset checklist, start session button
- Route: `/pick-list` added to `App.tsx`
- Button "Plukkliste" added to `Home.tsx`

**Tag extraction logic (client-side only):**
```ts
const extractTags = (text: string): string[] => {
  const matches = text.match(/\b[A-ZÆØÅ]+-\d+\b/gi) ?? [];
  return [...new Set(matches.map(t => t.toUpperCase()))];
};
```

---

#### Asset Photo support

**UX:**
- In `AssetList.tsx`: if asset has `photoUrl`, show a small thumbnail (max 60×60px) to the right of the tag. Clicking opens the full image in a lightbox overlay (simple: fixed-position dark overlay + centered `<img>`).
- In `PickList.tsx`: show photo thumbnail on each pick list card (same lightbox).
- In `AssetForm.tsx` (future): `PhotoUrl` text input with live `<img>` preview below.

**No backend changes needed** — `PhotoUrl` is already in the data model and returned by the API.

#### Files to create/modify (WP8)

| File | Change |
|------|--------|
| `src/inventory-spa/src/pages/PickList.tsx` | New page |
| `src/inventory-spa/src/components/AssetPhoto.tsx` | Thumbnail + lightbox component |
| `src/inventory-spa/src/pages/AssetList.tsx` | Add photo thumbnail |
| `src/inventory-spa/src/pages/Home.tsx` | Add "Plukkliste" button |
| `src/inventory-spa/src/App.tsx` | Add `/pick-list` route |
| `src/inventory-spa/src/api/assets.ts` | Add `lookup(tags)` function |
| `src/MemberService/Pages/Inventory/InventoryAssetsController.cs` | Add `POST /api/inventory/assets/lookup` |
| `src/MemberService/Pages/Inventory/InventoryModels.cs` | Add `AssetLookupRequest`, `AssetLookupResult` DTOs |

---

### WP9 — Asset Maintenance UI (InventoryManager only) ✅ DONE

**Purpose:** Allow managers to view and edit individual assets — primarily to set `PhotoUrl`, fix descriptions, mark items as out-of-inventory, etc.

**Access:** Only shown to users with `CanManageInventory` (i.e. `InventoryManager` or `Admin`). "Rediger utstyr" button on `Home.tsx` hidden for regular users (same `canManage` flag already in context).

**UX flow:**
1. "Rediger utstyr" button on Home → navigates to `/assets/manage`
2. Shows same searchable list as `AssetList.tsx` but each row has an "Rediger" button
3. Clicking navigates to `/assets/:tag/edit`
4. Edit form shows all fields: Beskrivelse, Kategori, SubKategori, Merke, Modell, Detaljer, LengdeM, Diameter, PhotoUrl (with live `<img>` preview below the input), InInventory toggle, Lokasjon
5. Save → `PUT /api/inventory/assets/{tag}` → back to list
6. Tag field is read-only (it's the key)

**Notes:**
- Create/delete are lower priority; edit is the primary need (especially for adding `PhotoUrl`)
- The existing `PUT /api/inventory/assets/{tag}` endpoint already handles all fields
- Route guard: if `canManage` is false, redirect to `/` from this page

#### Files to create/modify (WP9)

| File | Change |
|------|--------|
| `src/inventory-spa/src/pages/AssetManageList.tsx` | New — manager asset list with edit buttons |
| `src/inventory-spa/src/pages/AssetEditForm.tsx` | New — edit form with all fields + photo preview |
| `src/inventory-spa/src/pages/Home.tsx` | Add "Rediger utstyr" button (managers only) |
| `src/inventory-spa/src/App.tsx` | Add `/assets/manage` and `/assets/:tag/edit` routes |

---

## Verification Checklist

- [x] Migration applied, DB tables created
- [x] Real CSV imported — 139 assets in DB
- [x] Borrow flow: start → scan QR/type tag → review → complete → `CurrentBorrowId` set on assets
- [x] Return flow: `CurrentBorrowId` cleared after complete
- [x] Asset list shows event name + user name for borrowed items
- [x] "Vis kun utlånt" filter works
- [x] "Importer CSV" hidden for non-managers
- [x] Pick list: paste text → extract tags → show checklist with photos → start session (WP8)
- [x] Photo thumbnails in asset list and pick list with lightbox (WP8)
- [x] Asset edit form: click asset → edit all fields incl. PhotoUrl with preview (WP9)
- [x] "Rediger utstyr" only visible to managers (WP9)
- [ ] `dotnet test` passes (WP7)
