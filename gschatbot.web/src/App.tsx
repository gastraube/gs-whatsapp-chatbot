import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { AuthProvider } from './contexts/AuthContext'
import ProtectedRoute from './router/ProtectedRoute'
import Layout from './components/Layout'
import Login from './pages/Login'
import Home from './pages/Home'
import Especialistas from './pages/Especialistas'
import EspecialistasForm from './pages/EspecialistasForm'
import Consultas from './pages/Consultas'
import Clientes from './pages/Clientes'
import ClientesForm from './pages/ClientesForm'

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
            <Route path="especialistas">
              <Route index element={<Especialistas />} />
              <Route path="novo" element={<EspecialistasForm />} />
              <Route path=":id/editar" element={<EspecialistasForm />} />
            </Route>
            <Route path="consultas" element={<Consultas />} />
            <Route path="clientes">
              <Route index element={<Clientes />} />
              <Route path=":id/editar" element={<ClientesForm />} />
            </Route>
          </Route>
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  )
}
