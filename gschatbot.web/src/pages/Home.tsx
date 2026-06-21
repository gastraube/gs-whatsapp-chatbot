import { useAuth } from '../contexts/AuthContext'
import { MessageSquareHeart } from 'lucide-react'

export default function Home() {
  const { usuario } = useAuth()

  return (
    <div className="flex items-center gap-5">
      <div className="w-12 h-12 bg-indigo-600 rounded-xl flex items-center justify-center">
        <MessageSquareHeart size={24} className="text-white" />
      </div>
      <div>
        <h1 className="text-2xl font-bold text-slate-800">
          Bem-vindo, {usuario?.nome ?? 'administrador'}!
        </h1>
        <p className="text-slate-500 text-sm">Gerencie sua clínica pelo painel abaixo.</p>
      </div>
    </div>
  )
}
