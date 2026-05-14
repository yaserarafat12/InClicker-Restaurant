# Game Dev Protocol (Tycoon & Idle Dynamics)

Instruksi mendalam untuk agen saat membangun fitur atau sistem game idle/tycoon.

## 1. Math Integrity (The Quadrillion Protocol)
Dalam game idle, angka akan meroket secara eksponensial.
- **Wajib**: Gunakan library `decimal.js` atau tipe `BigInt` asli untuk SELURUH kalkulasi *currency* game. Tipe `number` biasa di JavaScript akan kehilangan presisi di atas `Number.MAX_SAFE_INTEGER`.
- Mencegah bug di mana uang graciously berubah negatif atau stuck karena limitasi float 64-bit IEEE.

## 2. Juiciness & Game Loop
Jangan buat UI statis untuk game. Game butuh "Juice" (efek memantul, feedback seketika).
- **Interaksi**: Setiap *tap/click* WAJIB memberikan visual feedback. Gunakan minimal intervensi Framer Motion (scale `1.1x` saat ditahan, `0.9x` saat diklik) dan memicu SFX ringan.
- **Engine rendering**: Untuk elemen yang memuat partikel atau pergerakan objek tanpa batas (non-DOM objects), gunakan `requestAnimationFrame` untuk memastikan 60fps tanpa membebani browser React diffing tree.
- **Asset Placeholder**: DILARANG menggunakan kotak div polos bermotif *placeholder*. Hasilkan aset visual dari kode berbasis SVG yang memiliki karakter "kartun", *rounded corners*, dan palet warna cerah untuk tahap prototipe iteratif.

## 3. Economy Balancing (The Simulation Layer)
- Jangan menebak-nebak harga upgrade.
- **Instruksi**: Buat skrip simulasi Python terpisah (misal `balancing_sim.py`) yang melakukan loop pembelian otomatis berdasarkan logika harga eksponensial game.
- Jalankan skrip ini untuk memprediksi sejauh mana progres pemain dalam 1 jam, 24 jam, dan 7 hari. Sesuaikan kurva biaya jika pemain akan terjebak (*paywall/timegate*) terlalu cepat.

## 4. Time-Travel Testing
Sistem *offline earnings* rentan gagal.
- **Instruksi**: Sertakan *Mock Time Provider* (backdoor environment testing) yang memungkinkan user atau agen melakukan manipulasi waktu sistem ("Fast Forward 7 Days").
- Verifikasi keamanan angka ekstrem yang di-generate pasca loncatan waktu, dan pastikan tidak menyebabkan UI *crash*.

## 5. Anti-Cheat & Validation
- Validasi semua request uang menggunakan parameter `Timestamp` dan `Signature`.
- Deteksi anomali: Jika RPS (Request Per Second) klik uang melampaui kemampuan fisik manusia (misal >100 klik/detik), batalkan request dan berikan flag pada profil tersebut sebagai anomali/cheater.
