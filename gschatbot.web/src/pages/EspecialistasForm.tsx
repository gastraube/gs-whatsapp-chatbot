import { useState, useEffect, type FormEvent } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { ArrowLeft } from 'lucide-react'
import api from '../services/api'

interface Especialidade {
  id: number
  nome: string
}

interface FormState {
  nome: string
  crm: string
  especialidadeId: number
  telefone: string
  email: string
  ativo: boolean
}

const VAZIO: FormState = { nome: '', crm: '', especialidadeId: 0, telefone: '', email: '', ativo: true }

const campo = 'flex flex-col gap-1.5'
const lbl = 'text-sm font-medium text-slate-700'
const input = 'rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent transition'

export default function EspecialistasForm() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const isEdit = !!id

  const [form, setForm] = useState<FormState>(VAZIO)
  const [especialidades, setEspecialidades] = useState<Especialidade[]>([])
  const [carregando, setCarregando] = useState(isEdit)
  const [salvando, setSalvando] = useState(false)
  const [erro, setErro] = useState('')

  useEffect(() => {
    api.get<Especialidade[]>('/especialistas/especialidades').then(r => setEspecialidades(r.data))
  }, [])

  useEffect(() => {
    if (!isEdit) return
    setCarregando(true)
    api.get(`/especialistas/${id}`)
      .then(r => {
        const e = r.data
        setForm({ nome: e.nome, crm: e.crm, especialidadeId: e.especialidadeId, telefone: e.telefone, email: e.email, ativo: e.ativo })
      })
      .catch(() => setErro('Erro ao carregar especialista.'))
      .finally(() => setCarregando(false))
  }, [id, isEdit])

  function set<K extends keyof FormState>(chave: K, valor: FormState[K]) {
    setForm(f => ({ ...f, [chave]: valor }))
  }

  async function salvar(ev: FormEvent) {
    ev.preventDefault()
    setSalvando(true)
    setErro('')
    try {
      if (isEdit) await api.put(`/especialistas/${id}`, form)
      else await api.post('/especialistas', form)
      navigate('/especialistas')
    } catch {
      setErro('Erro ao salvar. Verifique os dados e tente novamente.')
    } finally {
      setSalvando(false)
    }
  }

  if (carregando) {
    return (
      <div className="flex flex-col gap-6">
        <div className="flex items-center gap-3">
          <button onClick={() => navigate('/especialistas')} className="p-2 rounded-lg hover:bg-slate-100 text-slate-500 transition-colors">
            <ArrowLeft size={18} />
          </button>
          <h1 className="text-xl font-bold text-slate-800">Editar Especialista</h1>
        </div>
        <div className="bg-white rounded-xl border border-slate-200 p-6 flex flex-col gap-4">
          {[1, 2, 3, 4, 5].map(i => (
            <div key={i} className="h-9 bg-slate-100 rounded-lg animate-pulse" />
          ))}
        </div>
      </div>
    )
  }

  return (
    <div className="flex flex-col gap-6">
      <div className="flex items-center gap-3">
        <button
          type="button"
          onClick={() => navigate('/especialistas')}
          className="p-2 rounded-lg hover:bg-slate-100 text-slate-500 transition-colors"
        >
          <ArrowLeft size={18} />
        </button>
        <h1 className="text-xl font-bold text-slate-800">
          {isEdit ? 'Editar Especialista' : 'Novo Especialista'}
        </h1>
      </div>

      {erro && <p className="text-sm text-red-500 bg-red-50 px-4 py-3 rounded-lg">{erro}</p>}

      <form onSubmit={salvar} className="bg-white rounded-xl border border-slate-200 p-6 flex flex-col gap-5">
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-5">
          <div className={campo}>
            <label className={lbl}>Nome *</label>
            <input required className={input} value={form.nome} onChange={e => set('nome', e.target.value)} />
          </div>

          <div className={campo}>
            <label className={lbl}>CRM *</label>
            <input required className={input} value={form.crm} onChange={e => set('crm', e.target.value)} />
          </div>

          <div className={campo}>
            <label className={lbl}>Especialidade *</label>
            <select
              required
              className={input}
              value={form.especialidadeId}
              onChange={e => set('especialidadeId', Number(e.target.value))}
            >
              <option value={0} disabled>Selecione...</option>
              {especialidades.map(esp => (
                <option key={esp.id} value={esp.id}>{esp.nome}</option>
              ))}
            </select>
          </div>

          <div className={campo}>
            <label className={lbl}>Telefone</label>
            <input className={input} value={form.telefone} onChange={e => set('telefone', e.target.value)} />
          </div>

          <div className={campo}>
            <label className={lbl}>E-mail</label>
            <input type="email" className={input} value={form.email} onChange={e => set('email', e.target.value)} />
          </div>

          <div className={campo}>
            <label className={lbl}>Status</label>
            <label className="flex items-center gap-2 cursor-pointer mt-1">
              <input
                type="checkbox"
                checked={form.ativo}
                onChange={e => set('ativo', e.target.checked)}
                className="w-4 h-4 accent-indigo-600"
              />
              <span className="text-sm text-slate-700">Ativo</span>
            </label>
          </div>
        </div>

        <div className="flex justify-end gap-3 pt-2 border-t border-slate-100">
          <button
            type="button"
            onClick={() => navigate('/especialistas')}
            className="px-4 py-2 rounded-lg text-sm font-medium text-slate-600 hover:bg-slate-100 transition-colors"
          >
            Cancelar
          </button>
          <button
            type="submit"
            disabled={salvando}
            className="px-4 py-2 rounded-lg text-sm font-medium bg-indigo-600 text-white hover:bg-indigo-700 disabled:opacity-60 transition-colors"
          >
            {salvando ? 'Salvando...' : 'Salvar'}
          </button>
        </div>
      </form>
    </div>
  )
}
