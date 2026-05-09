import { Route, Routes } from 'react-router-dom'
import { AppLayout } from './components/AppLayout'
import { AdminPhotoUploadPage } from './pages/AdminPhotoUploadPage'
import { AdminCatalogPage } from './pages/AdminCatalogPage'
import { BrandsPage } from './pages/BrandsPage'
import { CarDetailsPage } from './pages/CarDetailsPage'
import { ComparisonPage } from './pages/ComparisonPage'
import { HomePage } from './pages/HomePage'
import { LoginPage } from './pages/LoginPage'
import { NotFoundPage } from './pages/NotFoundPage'
import { RegisterPage } from './pages/RegisterPage'
import { TrimDetailsPage } from './pages/TrimDetailsPage'
import './App.css'

function App() {
  return (
    <Routes>
      <Route element={<AppLayout />}>
        <Route path="/" element={<HomePage />} />
        <Route path="/brands" element={<BrandsPage />} />
        <Route path="/cars/:generationId" element={<CarDetailsPage />} />
        <Route path="/cars/variants/:generationVariantId" element={<CarDetailsPage />} />
        <Route path="/cars/variants/:generationVariantId/trims/:trimId" element={<TrimDetailsPage />} />
        <Route path="/comparison" element={<ComparisonPage />} />
        <Route path="/admin/photos" element={<AdminPhotoUploadPage />} />
        <Route path="/admin/catalog" element={<AdminCatalogPage />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  )
}

export default App
