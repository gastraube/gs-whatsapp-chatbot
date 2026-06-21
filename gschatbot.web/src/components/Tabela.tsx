import type { ReactNode } from 'react'
import { ChevronUp, ChevronDown, ChevronsUpDown } from 'lucide-react'

export interface Coluna<T> {
  chave: string
  titulo: string
  ordenavel?: boolean
  accessor?: string
  render?: (item: T) => ReactNode
}

interface Props<T> {
  colunas: Coluna<T>[]
  dados: T[]
  carregando: boolean
  ordenarPor: string
  crescente: boolean
  onOrdenar: (chave: string) => void
}

export function Tabela<T>({ colunas, dados, carregando, ordenarPor, crescente, onOrdenar }: Props<T>) {
  return (
    <div className="bg-white rounded-xl border border-slate-200 overflow-auto">
      <table className="w-full text-sm">
        <thead>
          <tr className="border-b border-slate-200 bg-slate-50">
            {colunas.map(col => (
              <th key={col.chave} className="px-5 py-4 text-left font-semibold text-slate-600 whitespace-nowrap">
                {col.ordenavel ? (
                  <button
                    onClick={() => onOrdenar(col.chave)}
                    className="flex items-center gap-1.5 hover:text-slate-900 transition-colors"
                  >
                    {col.titulo}
                    {ordenarPor === col.chave ? (
                      crescente ? <ChevronUp size={14} /> : <ChevronDown size={14} />
                    ) : (
                      <ChevronsUpDown size={14} className="opacity-40" />
                    )}
                  </button>
                ) : col.titulo}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {carregando
            ? Array.from({ length: 6 }).map((_, i) => (
                <tr key={i} className="border-b border-slate-100">
                  {colunas.map(col => (
                    <td key={col.chave} className="px-5 py-4">
                      <div className="h-4 bg-slate-100 rounded animate-pulse" style={{ width: `${50 + (i * 7) % 40}%` }} />
                    </td>
                  ))}
                </tr>
              ))
            : dados.length === 0
            ? (
                <tr>
                  <td colSpan={colunas.length} className="px-5 py-14 text-center text-slate-400">
                    Nenhum registro encontrado.
                  </td>
                </tr>
              )
            : dados.map((item, idx) => (
                <tr key={idx} className="border-b border-slate-100 last:border-0 hover:bg-slate-50 transition-colors">
                  {colunas.map(col => (
                    <td key={col.chave} className="px-5 py-4 text-slate-700">
                      {col.render
                        ? col.render(item)
                        : col.accessor
                        ? String((item as Record<string, unknown>)[col.accessor] ?? '')
                        : null}
                    </td>
                  ))}
                </tr>
              ))}
        </tbody>
      </table>
    </div>
  )
}
