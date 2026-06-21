import { useState } from 'react'
import { NavLink } from 'react-router-dom'
import { Users, Calendar, UserCheck, ChevronLeft, ChevronRight, MessageSquareHeart } from 'lucide-react'

const itens = [
  { to: '/especialistas', icon: UserCheck, label: 'Especialistas' },
  { to: '/consultas', icon: Calendar, label: 'Consultas' },
  { to: '/clientes', icon: Users, label: 'Clientes' },
]

interface Props {
  aberto?: boolean
  onFechar?: () => void
}

export default function Sidebar({ aberto = false, onFechar }: Props) {
  const [expandido, setExpandido] = useState(true)

  return (
    <aside
      className={[
        'flex flex-col bg-slate-900 text-white shrink-0',
        'fixed inset-y-0 left-0 z-30 md:static md:z-auto',
        'transition-transform duration-300',
        expandido ? 'w-60' : 'w-16',
        aberto ? 'translate-x-0' : '-translate-x-full md:translate-x-0',
      ].join(' ')}
    >
      <div className={`flex items-center gap-3 px-5 py-6 border-b border-slate-700 ${!expandido && 'justify-center'}`}>
        <MessageSquareHeart size={28} className="text-indigo-400 shrink-0" />
        {expandido && (
          <span className="font-bold text-lg tracking-tight text-white">gschatbot</span>
        )}
      </div>

      <nav className="flex-1 py-6">
        {itens.map(({ to, icon: Icon, label }) => (
          <NavLink
            key={to}
            to={to}
            onClick={onFechar}
            className={({ isActive }) =>
              `flex items-center gap-3 px-4 py-3.5 mx-3 mb-1 rounded-lg transition-colors text-sm font-medium ${
                isActive
                  ? 'bg-indigo-600 text-white'
                  : 'text-slate-400 hover:bg-slate-800 hover:text-white'
              } ${!expandido && 'justify-center'}`
            }
            title={!expandido ? label : undefined}
          >
            <Icon size={20} className="shrink-0" />
            {expandido && <span>{label}</span>}
          </NavLink>
        ))}
      </nav>

      <button
        onClick={() => setExpandido(!expandido)}
        className="hidden md:flex items-center justify-center py-4 border-t border-slate-700 text-slate-400 hover:text-white hover:bg-slate-800 transition-colors"
        title={expandido ? 'Recolher menu' : 'Expandir menu'}
      >
        {expandido ? <ChevronLeft size={18} /> : <ChevronRight size={18} />}
      </button>
    </aside>
  )
}
