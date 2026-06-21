import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { AuthProvider } from './contexts/AuthContext'
import ProtectedRoute from './router/ProtectedRoute'
import Layout from './components/Layout'
import Login from './pages/Login'
import Home from './pages/Home'
import Especialistas from './pages/Especialistas'
import Consultas from './pages/Consultas'
import Clientes from './pages/Clientes'

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route
            path="/"
            element={
              <ProtectedRoute>
                <Layout />
              </ProtectedRoute>
            }
          >
            <Route index element={<Home />} />
            <Route path="especialistas" element={<Especialistas />} />
            <Route path="consultas" element={<Consultas />} />
            <Route path="clientes" element={<Clientes />} />
          </Route>
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  )
}
