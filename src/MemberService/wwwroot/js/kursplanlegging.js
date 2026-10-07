// Kursplanlegging: fordeling av kurs i salene vi leier, med drag & drop.
// Ren JavaScript uten rammeverk. Salene lagres som ett dokument per semester,
// kursene lagres hver for seg.
(function () {
    'use strict';

    const root = document.getElementById('planner');
    if (!root) return;

    const planId = root.dataset.planId;
    const api = root.dataset.api;
    const token = root.querySelector('input[name="__RequestVerificationToken"]').value;

    const DAYS = ['', 'Mandag', 'Tirsdag', 'Onsdag', 'Torsdag', 'Fredag', 'Lørdag', 'Søndag'];
    const COLORS = ['#dbe7f3', '#fde2c8', '#d5f0d8', '#f8d7e3', '#e6dcf5', '#fff3bf', '#cdeeee', '#e9ecef'];
    const PX_PER_MINUTE = 1.1;
    const DEFAULT_LENGTH = 90;
    const SNAP = 15;

    const board = document.getElementById('kp-board');
    const pool = document.getElementById('kp-pool');
    const roomsPanel = document.getElementById('kp-rooms');
    const weekSelect = document.getElementById('kp-week');
    const statusText = document.getElementById('kp-status');

    let plan = null;
    let mode = 'week';
    // '' = alle uker, 'semester' = mellom start- og sluttdato for semesteret, ellers mandagen i en uke
    let week = 'semester';
    let dragging = null;

    // ---------- Hjelpefunksjoner ----------

    function el(tag, attrs, ...children) {
        const node = document.createElement(tag);
        for (const [key, value] of Object.entries(attrs || {})) {
            if (value === null || value === undefined || value === false) continue;
            if (key === 'class') node.className = value;
            else if (key === 'style') Object.assign(node.style, value);
            else if (key === 'dataset') Object.assign(node.dataset, value);
            else if (key.startsWith('on')) node.addEventListener(key.slice(2), value);
            else if (value === true) node.setAttribute(key, '');
            else node.setAttribute(key, value);
        }
        for (const child of children.flat(Infinity)) {
            if (child === null || child === undefined || child === false) continue;
            node.append(child instanceof Node ? child : document.createTextNode(String(child)));
        }
        return node;
    }

    function setChildren(node, ...children) {
        node.replaceChildren(...children.flat(Infinity).filter(c => c !== null && c !== undefined && c !== false));
    }

    const newId = () => Math.random().toString(36).slice(2, 10);
    const toDate = s => new Date(s + 'T00:00:00Z');
    const fmt = d => d.toISOString().slice(0, 10);
    const addDays = (s, n) => { const d = toDate(s); d.setUTCDate(d.getUTCDate() + n); return fmt(d); };
    const dayOf = s => toDate(s).getUTCDay() || 7;
    const monday = s => addDays(s, 1 - dayOf(s));
    const short = s => { const d = toDate(s); return `${d.getUTCDate()}.${d.getUTCMonth() + 1}`; };
    const toMin = t => { if (!t) return 0; const [h, m] = t.split(':').map(Number); return h * 60 + m; };
    const fromMin = m => `${String(Math.floor(m / 60)).padStart(2, '0')}:${String(m % 60).padStart(2, '0')}`;
    const inWeekOf = (date, w) => date >= w && date < addDays(w, 7);

    function isoWeek(s) {
        const d = toDate(s);
        d.setUTCDate(d.getUTCDate() + 4 - (d.getUTCDay() || 7));
        const yearStart = new Date(Date.UTC(d.getUTCFullYear(), 0, 1));
        return Math.ceil(((d - yearStart) / 86400000 + 1) / 7);
    }

    function semesterWeeks() {
        const result = [];
        for (let w = monday(plan.startDate); w <= plan.endDate; w = addDays(w, 7)) result.push(w);
        return result;
    }

    function weeklyDates(day) {
        return semesterWeeks()
            .map(w => addDays(w, day - 1))
            .filter(d => d >= plan.startDate && d <= plan.endDate);
    }

    function datesSummary(dates) {
        if (!dates.length) return 'ingen datoer';
        const sorted = [...dates].sort();
        return `${sorted.length} ${sorted.length === 1 ? 'gang' : 'ganger'}, uke ${isoWeek(sorted[0])}–${isoWeek(sorted[sorted.length - 1])}`;
    }

    const inSemester = d => d >= plan.startDate && d <= plan.endDate;

    // Fridager (helligdager og ferier): vi holder ikke kurs da, kurset går uka etter i stedet
    const holidayOf = d => (plan.holidays || []).find(h => d >= h.from && d <= h.to);
    const isOpen = d => !holidayOf(d);
    const holidaysInWeek = w => (plan.holidays || []).filter(h => h.from <= addDays(w, 6) && h.to >= w);

    function visibleInWeek(dates) {
        if (!week) return true;
        if (week === 'semester') return dates.some(inSemester);
        return dates.some(d => inWeekOf(d, week));
    }

    // Ukene i semesteret, pluss uker før og etter der vi har leid saler
    function selectableWeeks() {
        const dates = plan.rooms.flatMap(r => r.slots.flatMap(s => s.dates));
        const first = monday([plan.startDate, ...dates].sort()[0]);
        const last = [plan.endDate, ...dates].sort().pop();
        const result = [];
        for (let w = first; w <= last; w = addDays(w, 7)) result.push(w);
        return result;
    }

    function allSlots() {
        return plan.rooms
            .flatMap((room, index) => room.slots.map(slot => ({ room, slot, index })))
            .sort((a, b) => a.slot.day - b.slot.day || a.index - b.index || toMin(a.slot.start) - toMin(b.slot.start));
    }

    function findPlacement(course) {
        const room = plan.rooms.find(r => r.id === course.roomId);
        const slot = room && room.slots.find(s => s.id === course.slotId);
        return slot ? { room, slot } : null;
    }

    const coursesIn = slot => plan.courses
        .filter(c => c.slotId === slot.id)
        .sort((a, b) => toMin(a.startTime) - toMin(b.startTime));

    const colorOf = (course, room) => course.color || (room && room.color) || COLORS[0];

    function overlaps(a, b) {
        return a.id !== b.id
            && toMin(a.startTime) < toMin(b.endTime)
            && toMin(b.startTime) < toMin(a.endTime)
            && a.dates.some(d => b.dates.includes(d));
    }

    // ---------- Lagring ----------

    function status(text, isError) {
        statusText.textContent = text;
        statusText.classList.toggle('text-danger', !!isError);
    }

    async function request(method, url, body) {
        const response = await fetch(api + url, {
            method,
            credentials: 'same-origin',
            headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token },
            body: body === undefined ? undefined : JSON.stringify(body),
        });
        if (!response.ok) {
            throw new Error((await response.text()) || response.statusText);
        }
        return response.status === 204 ? null : response.json();
    }

    async function saving(action) {
        status('Lagrer…');
        try {
            const result = await action();
            status('Alt er lagret');
            return result;
        } catch (e) {
            status('Klarte ikke å lagre: ' + e.message, true);
            throw e;
        }
    }

    let roomsTimer = null;
    function saveRooms() {
        clearTimeout(roomsTimer);
        status('Lagrer…');
        roomsTimer = setTimeout(() => saving(() => request('PUT', `/${planId}/rooms`, plan.rooms)), 400);
    }

    async function saveCourse(course) {
        const saved = await saving(() => course.id
            ? request('PUT', `/courses/${course.id}`, course)
            : request('POST', `/${planId}/courses`, course));
        const index = plan.courses.findIndex(c => c.id === saved.id);
        if (index >= 0) plan.courses[index] = saved; else plan.courses.push(saved);
        render();
        return saved;
    }

    async function deleteCourse(course) {
        await saving(() => request('DELETE', `/courses/${course.id}`));
        plan.courses = plan.courses.filter(c => c.id !== course.id);
        render();
    }

    // ---------- Plassering ----------

    function place(course, room, slot, startMinute) {
        const length = course.startTime && course.endTime
            ? toMin(course.endTime) - toMin(course.startTime)
            : DEFAULT_LENGTH;
        const slotStart = toMin(slot.start);
        const slotEnd = toMin(slot.end);

        let start = Math.max(slotStart, startMinute);
        if (start + length > slotEnd) start = Math.max(slotStart, slotEnd - length);
        const end = Math.min(start + length, slotEnd);

        let dates = course.dates;
        if (course.slotId !== slot.id) {
            const available = [...slot.dates].filter(isOpen).sort();
            const fromWeek = week === 'semester'
                ? available.find(inSemester)
                : week && available.find(d => inWeekOf(d, week));
            const from = fromWeek || available[0];
            dates = available.filter(d => d >= from).slice(0, course.weeks);
        }

        return saveCourse({
            ...course,
            roomId: room.id,
            slotId: slot.id,
            startTime: fromMin(start),
            endTime: fromMin(end),
            dates,
        });
    }

    // Kurset legges i ukene etter kurset det slippes på, og nederst i boksen til det kurset
    // (slutter samtidig). Er det ingen uker igjen etter, legges det i ukene før.
    function placeAfter(course, room, slot, other) {
        const length = course.startTime && course.endTime
            ? toMin(course.endTime) - toMin(course.startTime)
            : DEFAULT_LENGTH;
        const slotStart = toMin(slot.start);
        const slotEnd = toMin(slot.end);

        let end = Math.min(toMin(other.endTime), slotEnd);
        let start = Math.max(slotStart, end - length);
        end = Math.min(start + length, slotEnd);


        const available = [...slot.dates].filter(isOpen).sort();
        let dates = available.filter(d => d > lastDate(other)).slice(0, course.weeks);
        if (!dates.length) {
            dates = available.filter(d => d < firstDate(other)).slice(-course.weeks);
        }
        if (!dates.length) {
            status(`Det er ingen ledige uker før eller etter ${other.title} i denne salen.`, true);
            return;
        }

        // Bunnplassering: kurs som allerede ligger i de samme ukene og starter tidligere,
        // gjør at kurset kortes inn i toppen i stedet for å overlappe.
        for (const c of coursesIn(slot)) {
            if (c.id === course.id || !c.dates.some(d => dates.includes(d))) continue;
            const cStart = toMin(c.startTime);
            const cEnd = toMin(c.endTime);
            if (cStart < end && start < cEnd) {
                if (cStart <= start) start = cEnd; else end = cStart;
            }
        }
        if (end - start < SNAP) {
            status(`Det er ikke plass til ${course.title} etter ${other.title}.`, true);
            return;
        }

        return saveCourse({
            ...course,
            roomId: room.id,
            slotId: slot.id,
            startTime: fromMin(start),
            endTime: fromMin(end),
            dates,
        });
    }

    function unplace(course) {
        return saveCourse({ ...course, roomId: null, slotId: null, startTime: null, endTime: null, dates: [] });
    }

    // ---------- Visning ----------

    function courseCard(course, placement, extraClass) {
        const conflict = placement && coursesIn(placement.slot).some(other => overlaps(course, other));
        const wrongCount = placement && course.dates.length !== course.weeks;
        return el('div', {
            class: `kp-course ${extraClass || ''} ${conflict ? 'kp-conflict' : ''}`,
            draggable: 'true',
            dataset: { id: course.id },
            style: { background: colorOf(course, placement && placement.room) },
            title: course.note || '',
        },
            el('strong', {}, course.title),
            placement && el('span', {}, `${course.startTime}–${course.endTime}`),
            el('span', { class: wrongCount ? 'kp-warn' : 'kp-weeks' },
                placement ? `${course.dates.length}/${course.weeks} uker` : `${course.weeks} uker`),
            course.eventId && el('i', { class: 'bi bi-check2-circle', title: 'Opprettet for påmelding' }));
    }

    function renderWeekSelect() {
        setChildren(weekSelect,
            el('option', { value: 'semester' }, `Semesterperioden (${short(plan.startDate)}–${short(plan.endDate)})`),
            el('option', { value: '' }, 'Alle uker'),
            selectableWeeks().map(w => {
                const outside = addDays(w, 6) < plan.startDate || w > plan.endDate;
                const free = holidaysInWeek(w).map(h => h.name).join(', ');
                return el('option', { value: w }, `Uke ${isoWeek(w)} (${short(w)}–${short(addDays(w, 6))})${outside ? ' – utenfor semesteret' : ''}${free ? ' – fri: ' + free : ''}`);
            }));
        weekSelect.value = week;
        if (weekSelect.value !== week) week = weekSelect.value;
    }

    // Grupperer elementer som henger sammen (a overlapper b, b overlapper c => a, b og c i samme gruppe)
    function connected(items, linked) {
        const groups = [];
        for (const item of items) {
            const touching = groups.filter(g => g.some(other => linked(item, other)));
            const merged = [item, ...touching.flat()];
            touching.forEach(g => groups.splice(groups.indexOf(g), 1));
            groups.push(merged);
        }
        return groups;
    }

    const firstDate = c => [...c.dates].sort()[0] || '';
    const lastDate = c => [...c.dates].sort().pop() || '';
    const timesOverlap = (a, b) => toMin(a.startTime) < toMin(b.endTime) && toMin(b.startTime) < toMin(a.endTime);
    const periodsOverlap = (a, b) => firstDate(a) <= lastDate(b) && firstDate(b) <= lastDate(a);
    const periodText = dates => dates.length ? `uke ${isoWeek([...dates].sort()[0])}–${isoWeek([...dates].sort().pop())}` : '';

    // Kurs på samme tid i samme sal, men i hver sin periode (f.eks. to 6-ukers kurs etter hverandre),
    // vises i hver sin kolonne side om side, sortert etter når perioden starter.
    function assignLanes(courses) {
        const lanes = new Map();
        for (const cluster of connected(courses, timesOverlap)) {
            const startOf = period => period.map(firstDate).sort()[0];
            const periods = connected(cluster, periodsOverlap)
                .sort((a, b) => startOf(a).localeCompare(startOf(b)));
            periods.forEach((period, lane) => period.forEach(c => lanes.set(c.id, { lane, count: periods.length })));
        }
        return lanes;
    }

    function noRoomsMessage() {
        return el('span', {}, 'Legg til saler i fanen ',
            el('a', { href: '#saler', onclick: e => { e.preventDefault(); showTab('saler'); } }, 'Saler'),
            ' for å starte planleggingen.');
    }

    function showTab(name) {
        $(`#kp-tabs a[href="#${name}"]`).tab('show');
    }

    // Salen blir bredere når kurs deler den i flere kolonner, så teksten får plass
    function columnWidth(slot) {
        const lanes = assignLanes(coursesIn(slot).filter(c => visibleInWeek(c.dates)));
        const count = Math.max(1, ...[...lanes.values()].map(l => l.count));
        return count > 1 ? { minWidth: `${9 * count}rem`, flexBasis: `${9 * count}rem` } : null;
    }

    function renderWeek() {
        const slots = allSlots().filter(x => visibleInWeek(x.slot.dates));
        if (!slots.length) {
            return el('div', { class: 'alert alert-info' },
                plan.rooms.length ? (week === 'semester' ? 'Ingen saler er booket i semesterperioden.' : week ? 'Ingen saler er booket denne uken.' : 'Ingen saler har tider ennå.') : noRoomsMessage());
        }

        const first = Math.floor(Math.min(...slots.map(x => toMin(x.slot.start))) / 60) * 60;
        const last = Math.ceil(Math.max(...slots.map(x => toMin(x.slot.end))) / 60) * 60;
        const height = (last - first) * PX_PER_MINUTE + 'px';
        const top = m => (m - first) * PX_PER_MINUTE + 'px';
        const hours = [];
        for (let m = first; m < last; m += 60) hours.push(m);

        const days = [...new Set(slots.map(x => x.slot.day))];

        return el('div', { class: 'kp-week' },
            el('div', { class: 'kp-axis' },
                el('div', { class: 'kp-dayhead' }, ' '),
                el('div', { class: 'kp-colhead' }, ' '),
                el('div', { class: 'kp-body', style: { height } },
                    hours.map(m => el('div', { class: 'kp-hour', style: { top: top(m) } }, fromMin(m))))),
            days.map(day => el('div', { class: 'kp-day' },
                el('div', { class: 'kp-dayhead' }, DAYS[day]),
                el('div', { class: 'kp-cols' },
                    slots.filter(x => x.slot.day === day).map(({ room, slot }) => el('div', { class: 'kp-col', style: columnWidth(slot) },
                        el('div', { class: 'kp-colhead', style: { borderTopColor: room.color || COLORS[7] } },
                            el('strong', {}, room.name || 'Uten navn'),
                            el('small', {}, [room.venue, `${slot.start}–${slot.end}`].filter(Boolean).join(' · ')),
                            el('small', {}, datesSummary(slot.dates)),
                            (() => {
                                const date = week && week !== 'semester' && slot.dates.find(d => inWeekOf(d, week));
                                const holiday = date && holidayOf(date);
                                return holiday ? el('small', { class: 'text-danger' }, `Fri ${short(date)}: ${holiday.name}`) : null;
                            })()),
                        el('div', {
                            class: 'kp-body',
                            style: { height },
                            dataset: { drop: 'slot', room: room.id, slot: slot.id, first: first },
                        },
                            hours.map(m => el('div', { class: 'kp-line', style: { top: top(m) } })),
                            el('div', { class: 'kp-open', style: { top: top(toMin(slot.start)), height: (toMin(slot.end) - toMin(slot.start)) * PX_PER_MINUTE + 'px' } }),
                            (() => {
                                const visible = coursesIn(slot).filter(c => visibleInWeek(c.dates));
                                const lanes = assignLanes(visible);
                                return visible.map(c => {
                                    const { lane, count } = lanes.get(c.id);
                                    const card = courseCard(c, { room, slot }, 'kp-placed');
                                    card.style.top = top(toMin(c.startTime));
                                    card.style.height = Math.max(20, (toMin(c.endTime) - toMin(c.startTime)) * PX_PER_MINUTE - 2) + 'px';
                                    if (count > 1) {
                                        card.style.left = `calc(${(lane / count) * 100}% + 2px)`;
                                        card.style.right = `calc(${((count - lane - 1) / count) * 100}% + 2px)`;
                                        card.append(el('span', { class: 'kp-period' }, periodText(c.dates)));
                                    }
                                    return card;
                                });
                            })())))))));
    }

    function renderSemester() {
        const weeks = semesterWeeks();
        const slots = allSlots();
        if (!slots.length) {
            return el('div', { class: 'alert alert-info' }, noRoomsMessage());
        }

        const rows = [];
        let lastDay = 0;
        for (const { room, slot } of slots) {
            if (slot.day !== lastDay) {
                rows.push(el('tr', { class: 'kp-sem-day' }, el('th', { colspan: weeks.length + 1 }, DAYS[slot.day])));
                lastDay = slot.day;
            }
            rows.push(el('tr', { class: 'kp-sem-slot', dataset: { drop: 'slot', room: room.id, slot: slot.id } },
                el('th', {}, `${room.name}${room.venue ? ' (' + room.venue + ')' : ''} ${slot.start}–${slot.end}`),
                weeks.map(w => {
                    const date = slot.dates.find(d => inWeekOf(d, w));
                    const holiday = date && holidayOf(date);
                    return el('td', {
                        class: holiday ? 'booked kp-closed' : date ? 'booked' : '',
                        title: holiday ? `${short(date)}: ${holiday.name}, ingen kurs` : null,
                    });
                })));

            for (const course of coursesIn(slot)) {
                rows.push(el('tr', { class: 'kp-sem-course' },
                    el('td', {}, courseCard(course, { room, slot }, 'kp-inline')),
                    weeks.map(w => {
                        const slotDate = slot.dates.find(d => inWeekOf(d, w));
                        const holiday = slotDate && holidayOf(slotDate);
                        const on = course.dates.some(d => inWeekOf(d, w));
                        const canToggle = slotDate && (!holiday || on);
                        return el('td', {
                            class: `${on ? 'on' : ''} ${canToggle ? 'kp-toggle' : ''} ${holiday ? 'kp-closed' : ''}`,
                            style: on ? { background: colorOf(course, room) } : null,
                            title: holiday
                                ? `${short(slotDate)}: ${holiday.name}, ingen kurs`
                                : slotDate ? `${short(slotDate)}: klikk for å ${on ? 'fjerne' : 'legge til'} denne datoen` : 'Salen er ikke booket denne uken',
                            onclick: canToggle ? () => toggleDate(course, slotDate) : null,
                        });
                    })));
            }
        }

        return el('div', { class: 'kp-sem-wrapper' },
            el('table', { class: 'kp-sem' },
                el('thead', {}, el('tr', {},
                    el('th', {}, 'Sal og kurs'),
                    weeks.map(w => {
                        const free = holidaysInWeek(w).map(h => h.name).join(', ');
                        return el('th', { class: free ? 'kp-closed' : '', title: `${short(w)}–${short(addDays(w, 6))}${free ? ': ' + free : ''}` }, isoWeek(w));
                    }))),
                el('tbody', {}, rows)),
            el('p', { class: 'small text-muted mt-1' }, 'Grå felt viser når salen er booket, skraverte felt er fridager. Klikk i et felt for å legge til eller fjerne en dato for kurset. Dra et kurs til en sal for å plassere det.'));
    }

    function toggleDate(course, date) {
        const dates = course.dates.includes(date)
            ? course.dates.filter(d => d !== date)
            : [...course.dates, date].sort();
        saveCourse({ ...course, dates });
    }

    function renderPool() {
        const unplaced = plan.courses.filter(c => !findPlacement(c));
        pool.replaceChildren(...(unplaced.length
            ? unplaced.map(c => courseCard(c, null, 'kp-inline'))
            : [el('span', { class: 'text-muted small' }, 'Alle kursene er plassert.')]));
    }

    function renderRooms() {
        document.getElementById('kp-room-count').textContent = plan.rooms.length || '';
        if (!plan.rooms.length) {
            roomsPanel.replaceChildren(el('div', { class: 'alert alert-info' },
                'Ingen saler ennå. Legg til en sal, eller lim inn bekreftelser på bestillinger (f.eks. fra BLS) eller faste tider (f.eks. «Sal 3  Mandag 20:00 - 21:30»).'));
            return;
        }
        roomsPanel.replaceChildren(...plan.rooms.map(room => el('div', { class: 'card mb-3 kp-room', style: { borderLeftColor: room.color || COLORS[7] } },
            el('div', { class: 'card-body' },
                el('div', { class: 'form-row align-items-end' },
                    el('div', { class: 'form-group col-md-4' },
                        el('label', {}, 'Sal'),
                        el('input', { class: 'form-control', value: room.name || '', placeholder: 'F.eks. Sal 3', onchange: e => { room.name = e.target.value; roomsChanged(); } })),
                    el('div', { class: 'form-group col-md-4' },
                        el('label', {}, 'Sted'),
                        el('input', { class: 'form-control', value: room.venue || '', placeholder: 'F.eks. Bårdar Danseinstitutt', onchange: e => { room.venue = e.target.value; roomsChanged(); } })),
                    el('div', { class: 'form-group col-md-3' },
                        el('label', {}, 'Farge'),
                        colorPicker(room.color, color => { room.color = color; roomsChanged(); })),
                    el('div', { class: 'form-group col-md-1 text-right' },
                        el('button', { type: 'button', class: 'btn btn-outline-danger', title: 'Slett salen', onclick: () => removeRoom(room) }, el('i', { class: 'bi bi-trash' })))),
                el('table', { class: 'table table-sm mb-2' },
                    el('tbody', {}, room.slots.map(slot => slotRow(room, slot)))),
                el('button', { type: 'button', class: 'btn btn-sm btn-outline-secondary', onclick: () => addSlot(room) }, el('i', { class: 'bi bi-plus' }), ' Legg til dag og tid')))));
    }

    function slotRow(room, slot) {
        const weeksRow = el('tr', { class: 'd-none' }, el('td', { colspan: 5 },
            el('div', { class: 'kp-weekpicker' }, semesterWeeks().map(w => {
                const date = addDays(w, slot.day - 1);
                if (date < plan.startDate || date > plan.endDate) return null;
                return el('label', { class: 'kp-weekbox', title: short(date) },
                    el('input', {
                        type: 'checkbox',
                        checked: slot.dates.includes(date),
                        onchange: e => {
                            slot.dates = e.target.checked ? [...slot.dates, date].sort() : slot.dates.filter(d => d !== date);
                            roomsChanged();
                        },
                    }),
                    ` ${isoWeek(date)}`);
            }))));

        const row = el('tr', {},
            el('td', {}, el('select', {
                class: 'form-control form-control-sm',
                onchange: e => {
                    const delta = Number(e.target.value) - slot.day;
                    slot.day = Number(e.target.value);
                    slot.dates = slot.dates.map(d => addDays(d, delta));
                    moveCourses(slot, delta);
                    roomsChanged();
                },
            }, DAYS.slice(1).map((name, i) => el('option', { value: i + 1, selected: slot.day === i + 1 }, name)))),
            el('td', {}, el('input', { class: 'form-control form-control-sm', type: 'time', step: 300, value: slot.start, onchange: e => { slot.start = e.target.value; roomsChanged(); } })),
            el('td', {}, el('input', { class: 'form-control form-control-sm', type: 'time', step: 300, value: slot.end, onchange: e => { slot.end = e.target.value; roomsChanged(); } })),
            el('td', { class: 'align-middle small' },
                datesSummary(slot.dates), ' ',
                el('button', { type: 'button', class: 'btn btn-sm btn-link p-0', onclick: () => weeksRow.classList.toggle('d-none') }, 'velg uker'), ' · ',
                el('button', { type: 'button', class: 'btn btn-sm btn-link p-0', onclick: () => { slot.dates = weeklyDates(slot.day); roomsChanged(); } }, 'alle uker')),
            el('td', { class: 'text-right' },
                el('button', { type: 'button', class: 'btn btn-sm btn-link text-danger', title: 'Fjern', onclick: () => removeSlot(room, slot) }, el('i', { class: 'bi bi-x-lg' }))));

        return [row, weeksRow];
    }

    function colorPicker(selected, onPick) {
        return el('div', { class: 'kp-colors' }, COLORS.map(color => el('button', {
            type: 'button',
            class: `kp-swatch ${color === selected ? 'selected' : ''}`,
            style: { background: color },
            title: color,
            onclick: e => {
                e.currentTarget.parentNode.querySelectorAll('.kp-swatch').forEach(s => s.classList.remove('selected'));
                e.currentTarget.classList.add('selected');
                onPick(color);
            },
        })));
    }

    function render() {
        renderWeekSelect();
        weekSelect.classList.toggle('d-none', mode !== 'week');
        board.replaceChildren(mode === 'week' ? renderWeek() : renderSemester());
        renderPool();
    }

    function roomsChanged() {
        saveRooms();
        render();
        renderRooms();
    }

    // ---------- Saler ----------

    function addRoom(room) {
        plan.rooms.push(room || { id: newId(), name: '', venue: '', color: COLORS[plan.rooms.length % COLORS.length], slots: [] });
        roomsChanged();
    }

    function addSlot(room) {
        const previous = room.slots[room.slots.length - 1];
        const day = previous ? previous.day : 1;
        room.slots.push({ id: newId(), day, start: previous ? previous.start : '18:00', end: previous ? previous.end : '22:00', dates: weeklyDates(day) });
        roomsChanged();
    }

    async function moveCourses(slot, delta) {
        for (const course of plan.courses.filter(c => c.slotId === slot.id)) {
            await saveCourse({ ...course, dates: course.dates.map(d => addDays(d, delta)) });
        }
    }

    async function unplaceAll(predicate) {
        for (const course of plan.courses.filter(predicate)) {
            await unplace(course);
        }
    }

    async function removeSlot(room, slot) {
        const count = plan.courses.filter(c => c.slotId === slot.id).length;
        if (count && !confirm(`${count} kurs er plassert her. De flyttes tilbake til listen over kurs som ikke er plassert.`)) return;
        await unplaceAll(c => c.slotId === slot.id);
        room.slots = room.slots.filter(s => s !== slot);
        roomsChanged();
    }

    async function removeRoom(room) {
        const count = plan.courses.filter(c => c.roomId === room.id).length;
        if (!confirm(`Slette ${room.name || 'salen'}?${count ? ` ${count} kurs flyttes tilbake til listen over kurs som ikke er plassert.` : ''}`)) return;
        await unplaceAll(c => c.roomId === room.id);
        plan.rooms = plan.rooms.filter(r => r !== room);
        roomsChanged();
    }

    // ---------- Kurs-dialog ----------

    const courseModal = $('#kp-course-modal');
    const courseForm = document.getElementById('kp-course-form');
    let editing = null;
    let editingColor = null;

    function openCourse(course) {
        editing = course;
        editingColor = course.color;
        const placement = findPlacement(course);
        const f = courseForm.elements;
        f.title.value = course.title;
        f.weeks.value = course.weeks;
        f.note.value = course.note || '';
        f.description.value = course.description || '';
        f.signupHelp.value = course.signupHelp || '';
        document.getElementById('kp-c-colors').replaceChildren(colorPicker(course.color, c => { editingColor = c; }));
        courseForm.querySelector('.kp-placed-only').classList.toggle('d-none', !placement);
        if (placement) {
            f.startTime.value = course.startTime;
            f.endTime.value = course.endTime;
            const first = [...course.dates].sort()[0];
            f.firstDate.replaceChildren(...[...placement.slot.dates].filter(isOpen).sort().map(d => el('option', { value: d, selected: d === first }, `${short(d)} (uke ${isoWeek(d)})`)));
            document.getElementById('kp-c-place').textContent =
                `${DAYS[placement.slot.day]} i ${placement.room.name}${placement.room.venue ? ' på ' + placement.room.venue : ''}. Salen er leid ${placement.slot.start}–${placement.slot.end}. Kurset går ${datesSummary(course.dates)}.`;
        }
        const help = document.getElementById('kp-c-description-help');
        const updateHelp = () => {
            if (!placement) {
                help.textContent = 'Tid og sted legges automatisk til etter beskrivelsen når kurset opprettes for påmelding.';
                return;
            }
            const schedule = `${DAYS[placement.slot.day]} kl ${course.startTime.replace(':', '.')}-${course.endTime.replace(':', '.')} ${placement.room.name}${placement.room.venue ? ' på ' + placement.room.venue : ''}`;
            const text = f.description.value.trim().replace(/\s+/g, ' ');
            const full = text ? `${text}${/[.!?:]$/.test(text) ? '' : '.'} ${schedule}` : schedule;
            help.textContent = `Blir i påmeldingen: «${full}»`;
        };
        f.description.oninput = updateHelp;
        updateHelp();
        document.getElementById('kp-c-event').textContent = course.eventId ? 'Kurset er allerede opprettet for påmelding. Endringer her oppdaterer ikke påmeldingen.' : '';
        courseModal.modal('show');
    }

    courseForm.addEventListener('submit', async e => {
        e.preventDefault();
        const f = courseForm.elements;
        const placement = findPlacement(editing);
        const updated = {
            ...editing,
            title: f.title.value.trim(),
            weeks: Number(f.weeks.value),
            note: f.note.value,
            description: f.description.value,
            signupHelp: f.signupHelp.value,
            color: editingColor,
        };

        if (placement) {
            if (toMin(f.endTime.value) <= toMin(f.startTime.value)) {
                alert('Kurset må slutte etter at det starter.');
                return;
            }
            updated.startTime = f.startTime.value;
            updated.endTime = f.endTime.value;
            const first = [...editing.dates].sort()[0];
            if (f.firstDate.value !== first || updated.weeks !== editing.weeks) {
                updated.dates = [...placement.slot.dates].filter(isOpen).sort().filter(d => d >= f.firstDate.value).slice(0, updated.weeks);
            }
        }

        await saveCourse(updated);
        courseModal.modal('hide');
    });

    document.getElementById('kp-c-delete').addEventListener('click', async () => {
        if (!confirm(`Slette ${editing.title} fra planen?`)) return;
        await deleteCourse(editing);
        courseModal.modal('hide');
    });

    // ---------- Lim inn bestilling ----------

    const pasteModal = $('#kp-paste-modal');
    const pasteForm = document.getElementById('kp-paste-form');
    const pastePreview = document.getElementById('kp-p-preview');
    const pasteSave = document.getElementById('kp-p-save');
    let parsed = null;

    function openPaste() {
        pasteForm.reset();
        parsed = null;
        pasteSave.disabled = true;
        pastePreview.replaceChildren();
        pasteModal.modal('show');
    }

    const findRoomByName = name => plan.rooms.find(r => (r.name || '').trim().toLowerCase() === name.trim().toLowerCase());

    async function checkPaste() {
        try {
            parsed = await request('POST', '/parse-booking', { text: pasteForm.elements.text.value });
        } catch (e) {
            pastePreview.replaceChildren(el('div', { class: 'text-danger' }, e.message));
            return;
        }

        pasteSave.disabled = parsed.rooms.length === 0;

        setChildren(pastePreview,
            parsed.rooms.length
                ? parsed.rooms.map(room => el('div', { class: 'mb-2' },
                    el('strong', {}, room.name),
                    el('span', { class: 'text-muted' }, findRoomByName(room.name) ? ' (finnes, tidene legges til)' : ' (ny sal)'),
                    el('ul', { class: 'mb-0' }, room.slots.map(s => el('li', {},
                        `${DAYS[s.day]} ${s.start}–${s.end}: `,
                        s.dates.length ? `${datesSummary(s.dates)} (${s.dates.map(short).join(', ')})` : 'alle uker')))))
                : el('p', { class: 'text-danger' }, 'Fant ingen bestilte tider.'),
            parsed.skipped.length ? el('details', {}, el('summary', {}, `${parsed.skipped.length} linjer ble ikke lest inn`), el('pre', {}, parsed.skipped.join('\n'))) : null);
    }

    document.getElementById('kp-p-check').addEventListener('click', checkPaste);

    pasteForm.addEventListener('submit', e => {
        e.preventDefault();
        if (!parsed) return;

        const venue = pasteForm.elements.venue.value.trim();

        for (const incomingRoom of parsed.rooms) {
            let room = findRoomByName(incomingRoom.name);
            if (!room) {
                room = { id: newId(), name: incomingRoom.name, venue, color: COLORS[plan.rooms.length % COLORS.length], slots: [] };
                plan.rooms.push(room);
            }

            for (const incoming of incomingRoom.slots) {
                const dates = incoming.dates.length ? incoming.dates : weeklyDates(incoming.day);
                const same = room.slots.find(s => s.day === incoming.day && s.start === incoming.start && s.end === incoming.end);
                if (same) {
                    same.dates = [...new Set([...same.dates, ...dates])].sort();
                } else {
                    room.slots.push({ ...incoming, id: newId(), dates });
                }
            }
        }

        roomsChanged();
        pasteModal.modal('hide');
    });

    // ---------- Drag & drop ----------

    root.addEventListener('click', e => {
        const card = e.target.closest('.kp-course');
        if (!card) return;
        const course = plan.courses.find(c => c.id === card.dataset.id);
        if (course) openCourse(course);
    });

    root.addEventListener('dragstart', e => {
        const card = e.target.closest('.kp-course');
        if (!card) return;
        dragging = { id: card.dataset.id, offsetY: card.classList.contains('kp-placed') ? e.clientY - card.getBoundingClientRect().top : 0 };
        e.dataTransfer.effectAllowed = 'move';
        e.dataTransfer.setData('text/plain', card.dataset.id);
        root.classList.add('kp-dragging');
    });

    root.addEventListener('dragend', () => {
        dragging = null;
        root.classList.remove('kp-dragging');
        root.querySelectorAll('.kp-over').forEach(n => n.classList.remove('kp-over'));
    });

    root.addEventListener('dragover', e => {
        const target = dragging && e.target.closest('[data-drop]');
        if (!target) return;
        e.preventDefault();
        e.dataTransfer.dropEffect = 'move';
        root.querySelectorAll('.kp-over').forEach(n => n !== target && n.classList.remove('kp-over'));
        target.classList.add('kp-over');
    });

    root.addEventListener('drop', e => {
        const target = dragging && e.target.closest('[data-drop]');
        if (!target) return;
        e.preventDefault();
        const course = plan.courses.find(c => c.id === dragging.id);
        if (!course) return;

        if (target.dataset.drop === 'pool') {
            if (findPlacement(course)) unplace(course);
            return;
        }

        const room = plan.rooms.find(r => r.id === target.dataset.room);
        const slot = room && room.slots.find(s => s.id === target.dataset.slot);
        if (!slot) return;

        const onCourse = e.target.closest('.kp-placed');
        const other = onCourse && plan.courses.find(c => c.id === onCourse.dataset.id && c.id !== course.id && c.slotId === slot.id);
        if (other) {
            placeAfter(course, room, slot, other);
            return;
        }

        let start = toMin(slot.start);
        if (target.dataset.first) {
            const y = e.clientY - target.getBoundingClientRect().top - dragging.offsetY;
            start = Number(target.dataset.first) + Math.round(y / PX_PER_MINUTE / SNAP) * SNAP;
        } else {
            // Semestervisning: legg kurset rett etter det siste kurset i salen
            const taken = coursesIn(slot).filter(c => c.id !== course.id);
            if (taken.length) start = Math.max(...taken.map(c => toMin(c.endTime)));
        }

        place(course, room, slot, start);
    });

    // ---------- Verktøylinje ----------

    root.querySelectorAll('[data-mode]').forEach(button => button.addEventListener('click', () => {
        mode = button.dataset.mode;
        root.querySelectorAll('[data-mode]').forEach(b => b.classList.toggle('active', b === button));
        render();
    }));

    weekSelect.addEventListener('change', () => { week = weekSelect.value; render(); });
    document.getElementById('kp-add-room').addEventListener('click', () => addRoom());
    document.getElementById('kp-paste').addEventListener('click', openPaste);

    document.getElementById('kp-new-course').addEventListener('submit', async e => {
        e.preventDefault();
        const form = e.target;
        await saveCourse({
            title: form.elements.title.value.trim(),
            weeks: Number(form.elements.weeks.value),
            color: COLORS[plan.courses.length % COLORS.length],
            sortOrder: plan.courses.length,
            dates: [],
        });
        form.elements.title.value = '';
        form.elements.title.focus();
    });

    // Husk valgt fane i adressen, slik at en ny innlasting viser samme fane
    $('#kp-tabs a[data-toggle="tab"]').on('shown.bs.tab', e => {
        history.replaceState(null, '', e.target.getAttribute('href') === '#saler' ? '#saler' : location.pathname + location.search);
    });

    // ---------- Oppstart ----------

    request('GET', `/${planId}`)
        .then(data => {
            plan = data;
            render();
            renderRooms();
            if (location.hash === '#saler' || !plan.rooms.length) showTab('saler');
        })
        .catch(e => status('Klarte ikke å hente planen: ' + e.message, true));
})();
