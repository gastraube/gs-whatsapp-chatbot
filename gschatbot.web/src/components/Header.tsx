import { LogIn, LogOut, User } from 'lucide-react'
import { useAuth } from '../contexts/AuthContext'
import { useNavigate } from 'react-router-dom'

export default function Header() {
  const { usuario, logout } = useAuth()
  const navigate = useNavigate()

  function handleLogout() {
    logout()
    navigate('/login')
  }

  return (
    <header className="flex items-center justify-end gap-4 px-6 py-3 bg-white border-b border-slate-200 shadow-sm">
      {usuario ? (
        <div className="flex items-center gap-3">
          <div className="flex items-center gap-2 text-sm text-slate-600">
            <div className="w-8 h-8 rounded-full bg-indigo-100 flex items-center justify-center">
              <User size={16} className="text-indigo-600" />
            </div>
            <span>
              Bem-vindo, <span className="font-semibold text-slate-800">{usuario.nome}</span>
            </span>
          </div>
          <button
            onClick={handleLogout}
            className="flex items-center gap-1.5 text-sm text-slate-500 hover:text-red-600 transition-colors"
          >
            <LogOut size={16} />
            Sair
          </button>
        </div>
      ) : (
        <button
          onClick={() => navigate('/login')}
          className="flex items-center gap-2 px-4 py-2 bg-indigo-600 text-white text-sm font-medium rounded-lg hover:bg-indigo-700 transition-colors"
        >
          <LogIn size={16} />
          Login
        </button>
      )}
    </header>
  )
}
