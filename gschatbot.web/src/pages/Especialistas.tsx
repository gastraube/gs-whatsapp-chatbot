import { useState, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import { Plus, Pencil, Trash2 } from 'lucide-react'
import { usePaginacao, type PaginadoResponse, type OrdemParams } from '../hooks/usePaginacao'
import { Tabela, type Coluna } from '../components/Tabela'
import { Paginacao } from '../components/Paginacao'
import api from '../services/api'

interface Especialista {
  id: number
  nome: string
  crm: string
  especialidade: string
  telefone: string
  email: string
  ativo: boolean
}

export default function Especialistas() {
  const navigate = useNavigate()
  const [excluindo, setExcluindo] = useState<Especialista | null>(null)
  const [excluindoErro, setExcluindoErro] = useState('')
  const [confirmando, setConfirmando] = useState(false)

  const buscarDados = useCallback(
    (pagina: number, tamanhoPagina: number, ordem: OrdemParams, busca: string): Promise<PaginadoResponse<Especialista>> =>
      api.get('/especialistas', {
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
    usePaginacao<Especialista>({ buscarDados, tamanhoPagina: 20, ordemInicial: { ordenarPor: 'nome', crescente: true } })

  async function executarExclusao() {
    if (!excluindo) return
    setConfirmando(true)
    setExcluindoErro('')
    try {
      await api.delete(`/especialistas/${excluindo.id}`)
      setExcluindo(null)
      recarregar()
    } catch {
      setExcluindoErro('Não foi possível excluir. O especialista pode ter registros vinculados.')
    } finally {
      setConfirmando(false)
    }
  }

  const colunas: Coluna<Especialista>[] = [
    { chave: 'nome',          titulo: 'Nome',          ordenavel: true,  accessor: 'nome' },
    { chave: 'crm',           titulo: 'CRM',           ordenavel: true,  accessor: 'crm' },
    { chave: 'especialidade', titulo: 'Especialidade', ordenavel: true,  accessor: 'especialidade' },
    { chave: 'email',         titulo: 'E-mail',        ordenavel: false, accessor: 'email' },
    {
      chave: 'telefone', titulo: 'Telefone', ordenavel: false,
      render: (e) => {
        const d = e.telefone.replace(/\D/g, '')
        if (d.length === 11) return `(${d.slice(0,2)}) ${d.slice(2,7)}-${d.slice(7)}`
        if (d.length === 10) return `(${d.slice(0,2)}) ${d.slice(2,6)}-${d.slice(6)}`
        return e.telefone
      },
    },
    {
      chave: 'ativo', titulo: 'Status', ordenavel: true,
      render: (e) => (
        <span className={`inline-flex px-2.5 py-0.5 rounded-full text-xs font-medium ${
          e.ativo ? 'bg-emerald-50 text-emerald-700' : 'bg-red-50 text-red-600'
        }`}>
          {e.ativo ? 'Ativo' : 'Inativo'}
        </span>
      ),
    },
    {
      chave: 'acoes', titulo: '',
      render: (e) => (
        <div className="flex items-center gap-1">
          <button
            onClick={() => navigate(`/especialistas/${e.id}/editar`)}
            className="p-1.5 rounded-lg text-slate-400 hover:text-indigo-600 hover:bg-indigo-50 transition-colors"
            title="Editar"
          >
            <Pencil size={14} />
          </button>
          <button
            onClick={() => { setExcluindo(e); setExcluindoErro('') }}
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
      <div className="flex items-center justify-between">
        <h1 className="text-xl font-bold text-slate-800">Especialistas</h1>
        <button
          onClick={() => navigate('/especialistas/novo')}
          className="flex items-center gap-2 px-4 py-2 bg-indigo-600 text-white text-sm font-medium rounded-lg hover:bg-indigo-700 transition-colors"
        >
          <Plus size={16} />
          Novo
        </button>
      </div>

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
            <p className="font-medium text-slate-800">Excluir especialista?</p>
            <p className="text-sm text-slate-500">
              <span className="font-medium text-slate-700">{excluindo.nome}</span> será removido permanentemente.
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
