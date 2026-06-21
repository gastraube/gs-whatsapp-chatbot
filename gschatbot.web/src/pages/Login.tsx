import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'
import { MessageSquareHeart, Loader2 } from 'lucide-react'

export default function Login() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [senha, setSenha] = useState('')
  const [erro, setErro] = useState('')
  const [carregando, setCarregando] = useState(false)

  async function handleSubmit(e: { preventDefault(): void }) {
    e.preventDefault()
    setErro('')
    setCarregando(true)
    try {
      await login(email, senha)
      navigate('/')
    } catch {
      setErro('E-mail ou senha inválidos.')
    } finally {
      setCarregando(false)
    }
  }

  return (
    <div className="w-full min-h-screen flex items-center justify-center bg-slate-100 px-4">
      <div className="bg-white rounded-2xl shadow-lg px-8 py-10 md:px-16 md:py-16 w-full max-w-xl">
        <div className="flex flex-col items-center gap-5 mb-12">
          <div className="w-16 h-16 bg-indigo-600 rounded-2xl flex items-center justify-center">
            <MessageSquareHeart size={34} className="text-white" />
          </div>
          <h1 className="text-3xl font-bold text-slate-800">gschatbot</h1>
          <p className="text-sm text-slate-500">Acesse o painel administrativo</p>
        </div>

        <form onSubmit={handleSubmit} className="flex flex-col gap-7">
          <div className="flex flex-col gap-2">
            <label className="text-sm font-medium text-slate-700">E-mail</label>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              placeholder="seu@email.com"
              className="px-4 py-3 rounded-lg border border-slate-200 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent transition"
            />
          </div>

          <div className="flex flex-col gap-2">
            <label className="text-sm font-medium text-slate-700">Senha</label>
            <input
              type="password"
              autoComplete="current-password"
              value={senha}
              onChange={(e) => setSenha(e.target.value)}
              required
              placeholder="••••••••"
              className="px-4 py-3 rounded-lg border border-slate-200 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent transition"
            />
          </div>

          {erro && (
            <p className="text-sm text-red-500 bg-red-50 px-4 py-3 rounded-lg">{erro}</p>
          )}

          <button
            type="submit"
            disabled={carregando}
            className="flex items-center justify-center gap-2 py-3 bg-indigo-600 text-white text-sm font-medium rounded-lg hover:bg-indigo-700 disabled:opacity-60 transition-colors mt-2"
          >
            {carregando ? <Loader2 size={16} className="animate-spin" /> : null}
            {carregando ? 'Entrando...' : 'Entrar'}
          </button>
        </form>
      </div>
    </div>
  )
}
