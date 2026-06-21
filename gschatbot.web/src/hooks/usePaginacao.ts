import { useState, useEffect, useCallback } from 'react'

export interface PaginadoResponse<T> {
  dados: T[]
  total: number
  pagina: number
  totalPaginas: number
  tamanhoPagina: number
}

export interface OrdemParams {
  ordenarPor: string
  crescente: boolean
}

interface Options<T> {
  // Wrap this in useCallback to avoid infinite re-renders
  buscarDados: (pagina: number, tamanhoPagina: number, ordem: OrdemParams, busca: string) => Promise<PaginadoResponse<T>>
  tamanhoPagina?: number
  ordemInicial?: OrdemParams
}

export function usePaginacao<T>({
  buscarDados,
  tamanhoPagina = 20,
  ordemInicial = { ordenarPor: 'nome', crescente: true },
}: Options<T>) {
  const [pagina, setPagina] = useState(1)
  const [busca, setBusca] = useState('')
  const [ordem, setOrdem] = useState<OrdemParams>(ordemInicial)
  const [dados, setDados] = useState<T[]>([])
  const [total, setTotal] = useState(0)
  const [totalPaginas, setTotalPaginas] = useState(0)
  const [carregando, setCarregando] = useState(true)
  const [erro, setErro] = useState('')

  const carregar = useCallback(async () => {
    setCarregando(true)
    setErro('')
    try {
      const resp = await buscarDados(pagina, tamanhoPagina, ordem, busca)
      setDados(resp.dados)
      setTotal(resp.total)
      setTotalPaginas(resp.totalPaginas)
    } catch {
      setErro('Erro ao carregar dados.')
    } finally {
      setCarregando(false)
    }
  }, [pagina, busca, ordem, tamanhoPagina, buscarDados])

  useEffect(() => { carregar() }, [carregar])

  function ordenarPor(chave: string) {
    setOrdem(o =>
      o.ordenarPor === chave
        ? { ...o, crescente: !o.crescente }
        : { ordenarPor: chave, crescente: true }
    )
    setPagina(1)
  }

  function irParaPagina(p: number) {
    setPagina(p)
  }

  function pesquisar(texto: string) {
    setBusca(texto)
    setPagina(1)
  }

  return {
    dados, total, totalPaginas, pagina, busca, ordem,
    carregando, erro,
    ordenarPor, irParaPagina, pesquisar,
    recarregar: carregar,
  }
}
