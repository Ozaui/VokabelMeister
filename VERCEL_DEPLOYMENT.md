# Vercel Deployment Kurulumu

## 1. Backend Deployment

### Vercel Dashboard'da:

1. **New Project** → GitHub repo'yu seç
2. **Root Directory**: `Backend` olarak ayarla
3. **Environment Variables** ekle:
   ```
   MONGO_URI=mongodb+srv://username:password@cluster.mongodb.net/vokabelmeister
   JWT_SECRET=your_jwt_secret_here
   NODE_ENV=production
   ```
4. **Deploy** et
5. Backend URL'ini not et (örn: `https://your-backend.vercel.app`)

## 2. Frontend Deployment

### Vercel Dashboard'da:

1. **New Project** → Aynı GitHub repo'yu seç
2. **Root Directory**: `VokabelMeister` olarak ayarla
3. **Build Command**: `npm run build` (otomatik olarak gelecek)
4. **Output Directory**: `dist` (otomatik olarak gelecek)
5. **Environment Variables** ekle:
   ```
   VITE_BASE_URL=https://your-backend.vercel.app/api
   NODE_ENV=production
   ```
6. **Deploy** et

### Önemli:

- `vercel-build` script'i `package.json`'da mevcut
- `vercel.json` dosyası basit konfigürasyon ile ayarlandı
- Build output `dist` klasörüne gidecek

## 3. Önemli Notlar

- Backend'i önce deploy et, sonra frontend'i
- MongoDB connection string'inizi doğru şekilde ayarlayın
- CORS ayarları backend'de zaten mevcut
- Frontend'de `VITE_BASE_URL` environment variable'ı kullanılıyor

## 4. Bağlantı Sorunu Çözümü

Eğer frontend-backend bağlantısında sorun yaşıyorsanız:

1. **Backend URL'ini kontrol edin**: `https://your-backend.vercel.app/api` formatında olmalı
2. **Environment Variables**: Vercel dashboard'da doğru şekilde ayarlandığından emin olun
3. **MongoDB**: Atlas'ta IP whitelist'e `0.0.0.0/0` ekleyin
4. **Network**: Browser console'da network errors'ları kontrol edin

## 5. CSS/JS MIME Type Hataları

Eğer şu hataları alıyorsanız:

- `'text/html' is not a valid JavaScript MIME type`
- `non CSS MIME types are not allowed in strict mode`

**Çözüm:**

1. Frontend'i yeniden deploy edin
2. Vercel cache'i temizleyin
3. Browser cache'i temizleyin (Ctrl+F5)
4. Vite build assets'ların doğru MIME type'larla serve edildiğinden emin olun

## 5. Test Etme

1. Backend URL'ini browser'da test edin: `https://your-backend.vercel.app/api/users`
2. Frontend'den API çağrılarını test edin
3. Console'da hata mesajlarını kontrol edin
