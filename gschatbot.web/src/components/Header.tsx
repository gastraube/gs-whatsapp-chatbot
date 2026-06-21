import { LogIn, LogOut, Menu, User } from 'lucide-react'
import { useAuth } from '../contexts/AuthContext'
import { useNavigate } from 'react-router-dom'

interface Props {
  onMenuClick?: () => void
}

export default function Header({ onMenuClick }: Props) {
  const { usuario, logout } = useAuth()
  const navigate = useNavigate()

  function handleLogout() {
    logout()
    navigate('/login')
  }

  return (
    <header className="flex items-center gap-4 px-6 py-5 md:px-10 md:py-6 bg-white border-b border-slate-200 shadow-sm">
      <button
        className="md:hidden text-slate-600 hover:text-slate-900 transition-colors"
        onClick={onMenuClick}
      >
        <Menu size={22} />
      </button>

      <div className="flex-1" />

      {usuario ? (
        <div className="flex items-center gap-3">
          <div className="flex items-center gap-2 text-sm text-slate-600">
            <div className="w-8 h-8 rounded-full bg-indigo-100 flex items-center justify-center">
              <User size={16} className="text-indigo-600" />
            </div>
            <span className="hidden sm:inline">
              Bem-vindo, <span className="font-semibold text-slate-800">{usuario.nome}</span>
            </span>
          </div>
          <button
            onClick={handleLogout}
            className="flex items-center gap-1.5 text-sm text-slate-500 hover:text-red-600 transition-colors"
          >
            <LogOut size={16} />
            <span className="hidden sm:inline">Sair</span>
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
