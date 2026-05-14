# Finance & Logic Level Defense Protocol

Ekonomi internal dan integritas finansial sistem berada pada level presisi bank tingkat tinggi.

## 1. Financial Accuracy: Double-Entry Bookkeeping
Sistem dilarang menggunakan operasi aritmatika penambahan biasa (e.g., `user.money += 100`).
- **Wajib Double-Entry**: Setiap kali ada uang yang ditransfer atau dihasilkan, catat sebagai transaksi pemindahan (Debit dan Credit) pada sebuah tabel mutasi berpasangan (buku besar).
- Harus jelas asal aliran uang (Source: Jual Item, Hadiah Level) dan kemana tujuannya (Destination: Dompet Player A).
- Kemudahan menelusuri eksploitasi: Bug inflasi angka akan dengan cepat diaudit berdasarkan data mutasi debit/kredit yang tercatat.

## 2. Invariants & Logic-Level Defenses
AI sering kali salah menilik celah permainan alur program.
- **Instruksi**: Pasang fungsi pendeteksi *Invariants* di akhir semua operasi pembelanjaan/deposit.
- Lakukan pre-kondisi pengecekan logika. "Jika Uang di Dompet < 0 setelah operasi berjalan, Rollback transaksi."
- Angka uang di dompet tidak valid ditarik minus untuk menambah sisa uang (Eksploitasi bug penanda).
- Tandai (*mark*) UID player ke bucket pengawasan Admin apabila ada aktivitas percobaan nilai negatif terdeteksi.

## 3. High-Concurrency Backend Language Shift
- Untuk subsistem keuangan berfrekuensi sangat rapat dan bervolume jumbo (hampir jutaan request sinkronisasi per detik), agen patuh menginisiasi kode service mikro dalam bahasa mesin **Go (Golang)** yang memiliki efisiensi goroutine fantastis di sisi backend, lalu dikoneksikan ke Next.js via API tertutup.
