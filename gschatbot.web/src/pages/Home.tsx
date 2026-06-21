import { useAuth } from '../contexts/AuthContext'
import { Calendar, Users, UserCheck, MessageSquareHeart } from 'lucide-react'

const cards = [
  { label: 'Especialistas', icon: UserCheck, cor: 'bg-indigo-50 text-indigo-600', href: '/especialistas' },
  { label: 'Consultas', icon: Calendar, cor: 'bg-emerald-50 text-emerald-600', href: '/consultas' },
  { label: 'Clientes', icon: Users, cor: 'bg-amber-50 text-amber-600', href: '/clientes' },
]

export default function Home() {
  const { usuario } = useAuth()

  return (
    <div className="flex flex-col gap-8">
      <div className="flex items-center gap-4">
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

      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        {cards.map(({ label, icon: Icon, cor, href }) => (
          <a
            key={label}
            href={href}
            className="flex items-center gap-4 bg-white rounded-xl p-5 shadow-sm border border-slate-100 hover:shadow-md transition-shadow"
          >
            <div className={`w-11 h-11 rounded-xl flex items-center justify-center ${cor}`}>
              <Icon size={22} />
            </div>
            <span className="font-medium text-slate-700">{label}</span>
          </a>
        ))}
      </div>
    </div>
  )
}
