# Cyber Security & Zero-Leak Defenses

Pertahanan tertinggi di InTracker untuk memastikan tidak ada celah di kode, dependensi, dan database.

## 1. Zero-Leak Secret Protocol
Dilarang mem-hardcode data rahasia dalam file `.env` untuk kebutuhan Production, maupun menampilkannya di sisi Browser (Local / Frontend).
- **Hooks**: Setup Pre-commit *trufflehog / gitleaks* pada skrip bash proyek untuk memindai API key palsu dari Git sebelum file terunggah.
- **Vaulting**: Integrasikan rahasia sistem ke *Cloud Secret Manager* atau Infisical. Agen WAJIB memanggil refensi terenkripsi ke variabel sistem saat di server.

## 2. Shadow Secrets (Baiting / Honeytokens)
- **Taktik Tembakan Perangkap**: Tanam token sandi pancingan (e.g. `gsk_fake_key_12345...`) pada kode komentar atau `.env.example` yang terlihat sangat menarik dan realistis bagi *crawler* hacker.
- Hubungkan key pancingan ini pada pelacak monitoring server. Setiap pancingan yang disentuh akan mentriger server pertahanan agar otomatis me-*rate-limit* atau mem-banned IP penyusup seketika secara proaktif.

## 3. High-Security Authentication (BFF & Cookie)
- Dilarang menyimpan `AccessToken` JWT yang rentan dicuri kedalam Object `LocalStorage`. 
- **BFF (Backend-For-Frontend)**: Session API server harus menitipkan sandi autentikasi pengguna secara rahasia pada Header Cookie browser bertipe `HttpOnly`, `Secure`, dan `SameSite=Strict`.
- **Refresh Token Rotation**: Pastikan token rotasi diperbarui pada setiap akses, mencegah akses curian (Session Hijacking).

## 4. Database: Row Level Security (RLS)
- Segala mutasi basis data harus dilindungi dengan RLS di PostgreSQL.
- Verifikasi keamanan harus final di level Engine *Database* secara murni—Player A tak akan mampu melihat entitas ID dari Player B maupun me-mutasinya dengan injeksi CURL buatan.

## 5. Security & Malware Audits
- **Lockfile Pinning**: Agen dilarang memperbarui package dengan tanda aksen *Caret* (`^`). Terapkan ikatan presisi (*pinning*) versi dependensi penuh terkunci di `package.json`.
- Selalu uji *third party modules* dengan perintah `npm audit` / `Snyk Test`. Hentikan jika terdapat *Critical* Malware Vunerability secara independen ("Supply Chain Attack" Block).

## 6. AI-Security Guard (Code Cross-Audit)
- **Peran Kritis**: Setelah kode utama terbangun, operasikan model sekunder (Gemini Flash) mengambil fungsi yang dibentuk agen primer dan mengaudit secara independen untuk XSS, SSRF, maupun celah SQL Injection.
- **Merge Block**: Dilarang me-Merge ke struktur basis rilis versi stabil bila Flash Guardian mengembalikan instruksi "Vunerable Risk detected".

## 7. Data Pelindung PII Masking
- Segala privasi identitas krusial Player/Pelanggan harus dikabutkan (Masking) pada terminal log server (`yaser*****@gmail.com`). 
- Segala data NIK, Saldo nyata dan email sensitif wajib Dienkripsi di dalam kolum Database, sebelum diproyeksi keluar ke Publik.

## 8. Development Environment (Ephemeral Data)
- Server *staging* tidak pernah diberikan koneksi menyentuh profil Database sesungguhnya dari pengguna nyata.
- **Database Sekali Pakai**: Agen akan me-rakit replikasi DB dummy untuk sesi penelusuran regresi kode pengembangan. Seketika sesi uji program tertutup, basis data sementara tersebut segera "ditenggelamkan/dibuang" tanpa tersisa ke internet.
