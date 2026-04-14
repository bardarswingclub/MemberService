# Inventory Management SPA — QR Scanning POC

This is a proof of concept for the inventory management system's QR scanning functionality. It's a standalone Vite+React+TypeScript app with mock data to validate the camera flow on real iOS and Android devices.

## Quick Start

### Development

```bash
cd src/MemberService/inventory-spa

# Install dependencies
npm install

# Start dev server
npm run dev
```

The app will be available at `http://localhost:5173` (or the port shown in the terminal).

### Testing on Mobile

To test on a real iOS or Android device:

1. **Using ngrok (easiest):**
   ```bash
   ngrok http 5173
   ```
   Then open the ngrok URL on your mobile device.

2. **Local network (if on same WiFi):**
   - Get your computer's local IP: `ipconfig getifaddr en0` (macOS) or `ipconfig` (Windows)
   - Open `http://<your-ip>:5173` on mobile device

3. **iOS Safari notes:**
   - Camera access requires HTTPS (use ngrok which provides HTTPS)
   - First time will ask permission to access camera
   - May ask to allow vibration

### Building for Production

```bash
npm run build
```

Output goes to `../wwwroot/dist/inventory/` (ready to be served by the .NET app).

## POC Scenarios

### Scenario 1: Free Scan Mode
- Click "Free Scan Mode"
- Camera opens automatically (tap "Start Scanning" button)
- Point at a QR code containing one of the test tags (see list below)
- Asset should be added to scanned list
- Remove buttons let you remove items
- "Review & Complete" button to finish

### Scenario 2: Pick List (Paste)
- Click "Pick List"
- Select "Paste Tags" tab
- Enter tags comma or newline separated (e.g., `S-001, K-001, H-001`)
- Click "Add to Scan List"
- Tags are validated and added (invalid tags are reported)

### Scenario 3: Pick List (QR Scan)
- Click "Pick List"
- Select "Scan QR" tab
- Shows preview of next item
- Point at QR codes
- Items added one by one

## Test Tags

Use these tags to generate QR codes for testing:

- `S-001` — Mikrofonstativ med galge
- `K-001` — 8 par kabel
- `H-001` — Hovedhøyttaler (HK Polar 10)
- `M-005` — Sang/vokal mikrofon (Shure SM58)
- `D-001` — Miksepult (Midas MR18)
- `H-005` — Monitor høyttaler (Behringer Eurolive B210D)
- `K-030` — XLR mikrofonkabel (veldig kort)

### Generate QR Codes

Use any online QR code generator (e.g., https://www.qr-code-generator.com/):
1. Paste the tag text (e.g., `S-001`)
2. Generate QR code
3. Print or display on another screen for testing

## What's Being Tested

✅ Camera access (iOS/Android)
✅ QR code scanning (on-device, no server calls)
✅ Multiple scanning modes (free scan, paste list, QR scan)
✅ Haptic feedback (vibration on scan)
✅ Audio beeping (error feedback)
✅ Item deduplication (can't scan same tag twice)
✅ Toast notifications
✅ Mobile-friendly UI

## Next Steps (After POC Validation)

Once this POC is confirmed to work on real devices:

1. **WP1:** Backend models + EF migration
2. **WP2:** Roles + policies + auth
3. **WP3:** JSON API controllers
4. **WP4:** Wire SPA to real backend
5. **WP5:** Asset management UI
6. **WP6:** Full borrow/return/check workflow
7. **WP7:** Tests

See `INVENTORY_PLAN.md` in the project root for the full plan.
