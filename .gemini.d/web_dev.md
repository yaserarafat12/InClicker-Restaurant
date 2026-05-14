# Web Development Protocol (The Edge Ecosystem)

Instruksi mendalam untuk arsitektur front-end dan backend InTracker.

## 1. Performance-First Proxy
Di era modern, web yang lambat tidak dapat ditoleransi.
- **Instruksi**: Setiap menyelesaikan komponen antarmuka yang masif, jalankan Lighthouse (atau evaluasi otonom via browser subagent). 
- Jika skor keseluruhan di bawah `90`, jangan serahkan ke user sebelum agen mengoptimalkannya secara mandiri melalui *lazy loading*, *dynamic imports*, kompresi aset SVG/WebP, atau modifikasi render-blocking.

## 2. Pemanfaatan Vercel/Cloudflare Paradigm
- **PPR (Partial Prerendering)**: Kerangka aplikasi di-render sebagai HTML statis super kilat secara global.
- Data yang sangat dinamis (informasi finansial/live tycoon stats) harus dirender di sisi client dengan *React Suspense Boundry* atau *Server-Sent Events (SSE) / Streaming* agar pengguna seketika melihat shell visual tanpa *blinking screen*.

## 3. Self-Healing UI (Automated Visual Fixes)
- Integrasikan kerangka kerja *Visual Regression Testing* bila memungkinkan.
- Agen WAJIB memverifikasi bahwa perubahan CSS Tailwnd di komponen *header* tidak akan mematahkan layout komponen *footer*.
- Jika ada kontradiksi *Z-index* atau tumpang tindih tata ruang *(overlap)* di *mobile view*, otomatis koreksi utilitas flexbox/grid.

## 4. Vector-Hybrid Search (Database)
- Aplikasi dengan entitas masif (log jurnal, log keuangan, atau inventory game) wajib didukung **pgvector** di PostgreSQL Supabase.
- Pengguna harus bisa mencari dengan natural language (e.g., *"Cari transaksi besar bulan lalu"*), menggunakan paduan metrik Euclidean/Cosine Distance dan pencarian Full-Text biasa.

## 5. Zod-Driven Truth
- **Jangan percayai payload apapun**. Seluruh data input user maupun respons fungsi Edge API harus masuk melalui pipa konfirmasi skema `z.object`.
- Mengamankan sistem dari error tak terduga (*undefined errors*) di production.

## 6. Agentic SEO & Analytics
- Agen wajib membangkitkan `<title>` dan `<meta>` unik berbasis komponen server halaman.
- Otomatiskan penambahan metrik kinerja pada internal logger.
- Menyusun laporan mingguan log untuk menyorot jalur halaman mana yang menunjukkan *High Load Time* terkonfirmasi.
