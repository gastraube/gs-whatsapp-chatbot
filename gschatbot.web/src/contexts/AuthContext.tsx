import { createContext, useContext, useState, useEffect, ReactNode } from 'react'
import api from '../services/api'

interface Usuario {
  nome: string
  email: string
  role: string
}

interface AuthContextType {
  usuario: Usuario | null
  login: (email: string, senha: string) => Promise<void>
  logout: () => void
  carregando: boolean
}

const AuthContext = createContext<AuthContextType | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [usuario, setUsuario] = useState<Usuario | null>(null)
  const [carregando, setCarregando] = useState(true)

  useEffect(() => {
    const salvo = localStorage.getItem('usuario')
    if (salvo) setUsuario(JSON.parse(salvo))
    setCarregando(false)
  }, [])

  async function login(email: string, senha: string) {
    const { data } = await api.post('/auth/login', { email, senha })
    localStorage.setItem('token', data.token)
    localStorage.setItem('usuario', JSON.stringify({ nome: data.nome, email: data.email, role: data.role }))
    setUsuario({ nome: data.nome, email: data.email, role: data.role })
  }

  function logout() {
    localStorage.removeItem('token')
    localStorage.removeItem('usuario')
    setUsuario(null)
  }

  return (
    <AuthContext.Provider value={{ usuario, login, logout, carregando }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth fora do AuthProvider')
  return ctx
}
