import { useState, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import { Pencil, Trash2 } from 'lucide-react'
import { usePaginacao, type PaginadoResponse, type OrdemParams } from '../hooks/usePaginacao'
import { Tabela, type Coluna } from '../components/Tabela'
import { Paginacao } from '../components/Paginacao'
import api from '../services/api'

interface Cliente {
  id: number
  nome: string | null
  cpf: string | null
  email: string | null
  dataNascimento: string | null
  numeroPrincipal: string | null
  planoPrincipal: string | null
  ativo: boolean
}

function Vazio() {
  return <span className="text-slate-300 select-none">—</span>
}

function formatarData(iso: string | null) {
  if (!iso) return <Vazio />
  const [ano, mes, dia] = iso.split('-')
  return `${dia}/${mes}/${ano}`
}

function formatarTelefone(n: string | null) {
  if (!n) return <Vazio />
  const d = n.replace(/\D/g, '')
  if (d.length === 13) return `+${d.slice(0,2)} (${d.slice(2,4)}) ${d.slice(4,9)}-${d.slice(9)}`
  if (d.length === 12) return `+${d.slice(0,2)} (${d.slice(2,4)}) ${d.slice(4,8)}-${d.slice(8)}`
  if (d.length === 11) return `(${d.slice(0,2)}) ${d.slice(2,7)}-${d.slice(7)}`
  if (d.length === 10) return `(${d.slice(0,2)}) ${d.slice(2,6)}-${d.slice(6)}`
  return n
}

export default function Clientes() {
  const navigate = useNavigate()
  const [excluindo, setExcluindo] = useState<Cliente | null>(null)
  const [excluindoErro, setExcluindoErro] = useState('')
  const [confirmando, setConfirmando] = useState(false)

  const buscarDados = useCallback(
    (pagina: number, tamanhoPagina: number, ordem: OrdemParams, busca: string): Promise<PaginadoResponse<Cliente>> =>
      api.get('/clientes', {
        params: {
          pagina,
          tamanhoPagina,
          ordenarPor: ordem.ordenarPor,
          crescente: ordem.crescente,
          busca: busca || undefined,
        },
      }).then(r => r.data),
    []
  )

  const { dados, total, totalPaginas, pagina, ordem, carregando, erro, ordenarPor, irParaPagina, recarregar } =
    usePaginacao<Cliente>({ buscarDados, tamanhoPagina: 20, ordemInicial: { ordenarPor: 'nome', crescente: true } })

  async function executarExclusao() {
    if (!excluindo) return
    setConfirmando(true)
    setExcluindoErro('')
    try {
      await api.delete(`/clientes/${excluindo.id}`)
      setExcluindo(null)
      recarregar()
    } catch {
      setExcluindoErro('Não foi possível excluir. O cliente pode ter registros vinculados.')
    } finally {
      setConfirmando(false)
    }
  }

  const colunas: Coluna<Cliente>[] = [
    {
      chave: 'nome', titulo: 'Nome', ordenavel: true,
      render: (c) => c.nome ?? <Vazio />,
    },
    {
      chave: 'cpf', titulo: 'CPF', ordenavel: true,
      render: (c) => c.cpf ?? <Vazio />,
    },
    {
      chave: 'numeroPrincipal', titulo: 'Número', ordenavel: false,
      render: (c) => formatarTelefone(c.numeroPrincipal),
    },
    {
      chave: 'planoPrincipal', titulo: 'Plano', ordenavel: false,
      render: (c) => c.planoPrincipal ?? <Vazio />,
    },
    {
      chave: 'dataNascimento', titulo: 'Nascimento', ordenavel: true,
      render: (c) => formatarData(c.dataNascimento),
    },
    {
      chave: 'ativo', titulo: 'Status', ordenavel: true,
      render: (c) => (
        <span className={`inline-flex px-2.5 py-0.5 rounded-full text-xs font-medium ${
          c.ativo ? 'bg-emerald-50 text-emerald-700' : 'bg-red-50 text-red-600'
        }`}>
          {c.ativo ? 'Ativo' : 'Inativo'}
        </span>
      ),
    },
    {
      chave: 'acoes', titulo: '',
      render: (c) => (
        <div className="flex items-center gap-1">
          <button
            onClick={() => navigate(`/clientes/${c.id}/editar`)}
            className="p-1.5 rounded-lg text-slate-400 hover:text-indigo-600 hover:bg-indigo-50 transition-colors"
            title="Editar"
          >
            <Pencil size={14} />
          </button>
          <button
            onClick={() => { setExcluindo(c); setExcluindoErro('') }}
            className="p-1.5 rounded-lg text-slate-400 hover:text-red-600 hover:bg-red-50 transition-colors"
            title="Excluir"
          >
            <Trash2 size={14} />
          </button>
        </div>
      ),
    },
  ]

  return (
    <div className="flex flex-col gap-6">
      <h1 className="text-xl font-bold text-slate-800">Clientes</h1>

      {erro && (
        <p className="text-sm text-red-500 bg-red-50 px-4 py-3 rounded-lg">{erro}</p>
      )}

      <Tabela
        colunas={colunas}
        dados={dados}
        carregando={carregando}
        ordenarPor={ordem.ordenarPor}
        crescente={ordem.crescente}
        onOrdenar={ordenarPor}
      />

      <Paginacao
        pagina={pagina}
        totalPaginas={totalPaginas}
        total={total}
        tamanhoPagina={20}
        onPagina={irParaPagina}
      />

      {excluindo && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl p-6 w-80 flex flex-col gap-4 shadow-xl">
            <p className="font-medium text-slate-800">Excluir cliente?</p>
            <p className="text-sm text-slate-500">
              <span className="font-medium text-slate-700">{excluindo.nome ?? 'Este cliente'}</span> será removido permanentemente.
            </p>
            {excluindoErro && (
              <p className="text-xs text-red-500 bg-red-50 px-3 py-2 rounded-lg">{excluindoErro}</p>
            )}
            <div className="flex justify-end gap-3">
              <button
                onClick={() => setExcluindo(null)}
                disabled={confirmando}
                className="px-4 py-2 rounded-lg text-sm font-medium text-slate-600 hover:bg-slate-100 transition-colors"
              >
                Cancelar
              </button>
              <button
                onClick={executarExclusao}
                disabled={confirmando}
                className="px-4 py-2 rounded-lg text-sm font-medium bg-red-600 text-white hover:bg-red-700 disabled:opacity-60 transition-colors"
              >
                {confirmando ? 'Excluindo...' : 'Excluir'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
