import { useState, useEffect, type FormEvent } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { ArrowLeft } from 'lucide-react'
import api from '../services/api'

interface FormState {
  nome: string
  cpf: string
  email: string
  dataNascimento: string
  ativo: boolean
}

const VAZIO: FormState = { nome: '', cpf: '', email: '', dataNascimento: '', ativo: true }

const campo = 'flex flex-col gap-1.5'
const lbl = 'text-sm font-medium text-slate-700'
const input = 'rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-800 outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent transition'

export default function ClientesForm() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()

  const [form, setForm] = useState<FormState>(VAZIO)
  const [carregando, setCarregando] = useState(true)
  const [salvando, setSalvando] = useState(false)
  const [erro, setErro] = useState('')

  useEffect(() => {
    api.get(`/clientes/${id}`)
      .then(r => {
        const c = r.data
        setForm({
          nome: c.nome ?? '',
          cpf: c.cpf ?? '',
          email: c.email ?? '',
          dataNascimento: c.dataNascimento ?? '',
          ativo: c.ativo,
        })
      })
      .catch(() => setErro('Erro ao carregar cliente.'))
      .finally(() => setCarregando(false))
  }, [id])

  function set<K extends keyof FormState>(chave: K, valor: FormState[K]) {
    setForm(f => ({ ...f, [chave]: valor }))
  }

  async function salvar(ev: FormEvent) {
    ev.preventDefault()
    setSalvando(true)
    setErro('')
    try {
      await api.put(`/clientes/${id}`, {
        ...form,
        dataNascimento: form.dataNascimento || null,
      })
      navigate('/clientes')
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
          <button onClick={() => navigate('/clientes')} className="p-2 rounded-lg hover:bg-slate-100 text-slate-500 transition-colors">
            <ArrowLeft size={18} />
          </button>
          <h1 className="text-xl font-bold text-slate-800">Editar Cliente</h1>
        </div>
        <div className="bg-white rounded-xl border border-slate-200 p-6 flex flex-col gap-4">
          {[1, 2, 3, 4].map(i => (
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
          onClick={() => navigate('/clientes')}
          className="p-2 rounded-lg hover:bg-slate-100 text-slate-500 transition-colors"
        >
          <ArrowLeft size={18} />
        </button>
        <h1 className="text-xl font-bold text-slate-800">Editar Cliente</h1>
      </div>

      {erro && <p className="text-sm text-red-500 bg-red-50 px-4 py-3 rounded-lg">{erro}</p>}

      <form onSubmit={salvar} className="bg-white rounded-xl border border-slate-200 p-6 flex flex-col gap-5">
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-5">
          <div className={campo}>
            <label className={lbl}>Nome</label>
            <input className={input} value={form.nome} onChange={e => set('nome', e.target.value)} />
          </div>

          <div className={campo}>
            <label className={lbl}>CPF</label>
            <input className={input} value={form.cpf} onChange={e => set('cpf', e.target.value)} />
          </div>

          <div className={campo}>
            <label className={lbl}>E-mail</label>
            <input type="email" className={input} value={form.email} onChange={e => set('email', e.target.value)} />
          </div>

          <div className={campo}>
            <label className={lbl}>Data de Nascimento</label>
            <input type="date" className={input} value={form.dataNascimento} onChange={e => set('dataNascimento', e.target.value)} />
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
            onClick={() => navigate('/clientes')}
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
