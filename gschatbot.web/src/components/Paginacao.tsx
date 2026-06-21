import { ChevronLeft, ChevronRight } from 'lucide-react'

interface Props {
  pagina: number
  totalPaginas: number
  total: number
  tamanhoPagina: number
  onPagina: (p: number) => void
}

export function Paginacao({ pagina, totalPaginas, total, tamanhoPagina, onPagina }: Props) {
  const inicio = total === 0 ? 0 : (pagina - 1) * tamanhoPagina + 1
  const fim = Math.min(pagina * tamanhoPagina, total)

  return (
    <div className="flex items-center justify-between text-sm text-slate-500">
      <span>
        {total === 0 ? 'Nenhum registro' : `${inicio}–${fim} de ${total} registros`}
      </span>
      <div className="flex items-center gap-1">
        <button
          onClick={() => onPagina(pagina - 1)}
          disabled={pagina <= 1}
          className="p-2 rounded-lg hover:bg-slate-100 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
        >
          <ChevronLeft size={16} />
        </button>
        <span className="px-3 font-medium text-slate-700">
          {pagina} / {totalPaginas || 1}
        </span>
        <button
          onClick={() => onPagina(pagina + 1)}
          disabled={pagina >= totalPaginas}
          className="p-2 rounded-lg hover:bg-slate-100 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
        >
          <ChevronRight size={16} />
        </button>
      </div>
    </div>
  )
}
