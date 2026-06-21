import { Navigate } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'

export default function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const { usuario, carregando } = useAuth()

  if (carregando) return <div className="flex items-center justify-center h-screen text-slate-500">Carregando...</div>
  if (!usuario) return <Navigate to="/login" replace />

  return <>{children}</>
}
